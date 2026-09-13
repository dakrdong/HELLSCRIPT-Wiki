using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        sealed class InventorySession
        {
            public InventoryQuery query=new InventoryQuery();
            public string selectedId="",anchorId="";
            public float listOffset,anchorFraction;
            public bool detailOpen;
        }
        readonly Dictionary<string,InventorySession> inventorySessions=new Dictionary<string,InventorySession>();
        InventorySession inventorySession;
        ScrollRect inventoryList,inventoryDetail,inventoryComparison,inventoryTool,inventoryReading;
        readonly List<RectTransform> inventoryDetailRows=new List<RectTransform>(),inventoryComparisonRows=new List<RectTransform>();
        bool inventoryWarehouse;
        string inventoryToolKind="";
        Vector2 inventorySize;
        Rect inventorySafe;
        InventoryBulkPlan inventoryBulk;
        bool inventoryDirty;
        IEnumerable<Item> InventorySource=>inventoryWarehouse?game.Store.Data.warehouse:game.Store.Data.Hero.inventory;
        Item InventorySelected=>InventorySource.FirstOrDefault(i=>i.id==inventorySession.selectedId);
        static string[] InventoryGrades=>new[]{"모든 등급","일반","마법","희귀","전설","세트"};
        static string[] InventoryOrders=>new[]{"장착·부위순","최근 획득순","오래된 획득순","아이템 레벨순","등급순"};
        void OpenInventory(bool warehouse,bool portal)
        {
            string key=game.Store.Data.Hero.id+":"+warehouse;
            if(!inventorySessions.TryGetValue(key,out var state)){state=new InventorySession();inventorySessions[key]=state;}
            Base(warehouse?"warehouse":"bag",warehouse?"공유 창고":portal?"포탈 가방 정리":"장비와 가방","",responsive:true);
            inventorySession=state;inventoryWarehouse=warehouse;portalBag=portal;inventoryToolKind="";inventoryBulk=null;
            var oldScroll=content.parent.gameObject;oldScroll.SetActive(false);Destroy(oldScroll);
            inventoryList=InventoryScroll("Inventory list");inventoryDetail=InventoryScroll("Inventory detail");inventoryComparison=InventoryScroll("Inventory comparison");inventoryTool=InventoryScroll("Inventory tools");
            header.SetAsLastSibling();footer.SetAsLastSibling();overlay.SetAsLastSibling();
            inventoryReading=inventoryList;
            RefreshInventory(false);
        }
        ScrollRect InventoryScroll(string name)
        {
            var frame=Box(name,root,new Color(.04f,.055f,.07f,.95f));
            frame.gameObject.AddComponent<RectMask2D>();var scroll=frame.gameObject.AddComponent<DialogScrollRect>();
            scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.readingStarted=()=>inventoryReading=scroll;
            var body=Rect(name+" content",frame);body.anchorMin=new Vector2(0,1);body.anchorMax=Vector2.one;body.pivot=new Vector2(.5f,1);body.sizeDelta=Vector2.zero;
            var layout=body.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=10;layout.childControlWidth=layout.childForceExpandWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.padding=new RectOffset(8,8,8,8);
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.viewport=frame;scroll.content=body;return scroll;
        }
        static void ClearInventoryRows(Transform parent)
        {foreach(Transform child in parent){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}}
        Text InventoryNote(Transform parent,string text,int size=20,Color? color=null)
        {return Note(parent,text,size,40,color??pale);}
        Button InventoryButton(Transform parent,string title,Action action,bool primary=false)
        {var button=BigButton(parent,title,action,primary);button.GetComponentInChildren<Text>().fontSize=20;return button;}
        void CaptureInventoryList()
        {
            if(inventoryList==null||inventorySession==null)return;
            inventorySession.listOffset=inventoryList.content.anchoredPosition.y;
            if(!inventoryList.gameObject.activeInHierarchy)return;
            float top=inventoryList.viewport.rect.yMax;
            foreach(RectTransform child in inventoryList.content)
            {
                if(!child.gameObject.activeSelf||!child.name.StartsWith("Inventory item ",StringComparison.Ordinal))continue;
                var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(inventoryList.viewport,child);
                if(bounds.min.y>=top)continue;
                inventorySession.anchorId=child.name.Substring("Inventory item ".Length);
                inventorySession.anchorFraction=Mathf.Clamp01((bounds.max.y-top)/Mathf.Max(1,bounds.size.y));break;
            }
        }
        void RestoreInventoryList()
        {
            var row=inventoryList.content.Find("Inventory item "+inventorySession.anchorId) as RectTransform;
            float target=inventorySession.listOffset;
            if(row!=null)
            {
                var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(inventoryList.content,row);
                target=inventoryList.content.rect.yMax-bounds.max.y+inventorySession.anchorFraction*bounds.size.y;
            }
            var position=inventoryList.content.anchoredPosition;position.y=Mathf.Clamp(target,0,Mathf.Max(0,inventoryList.content.rect.height-inventoryList.viewport.rect.height));
            inventoryList.StopMovement();inventoryList.content.anchoredPosition=position;
        }
        void RefreshInventory(bool capture=true)
        {
            if(capture)CaptureInventoryList();
            ClearInventoryRows(inventoryList.content);var all=InventorySource.ToArray();var visible=inventorySession.query.Apply(all).ToArray();
            foreach(var item in visible)
            {
                string id=item.id,protect=Protection(item);
                var button=InventoryButton(inventoryList.content,Loc.F("{0} +{1}\n{2} · {3} · I{4} · 요구 Lv.{5}\n{6}{7}", item.DisplayName, item.enhancement, Grade(item), GameCatalog.Slots[item.slot], item.level, item.RequiredLevel, (item.acquiredOrder>0&&!item.reviewed?"새 장비 · ":""), (protect==""?"상세·비교 열기":protect)),()=>SelectInventoryItem(id),id==inventorySession.selectedId);
                button.gameObject.name="Inventory item "+id;
                var label=button.GetComponentInChildren<Text>();label.color=ItemColor(item);Inset(label.rectTransform,84,8,8,8);
                EquipmentIcon(button.transform,item,10,12,62);button.GetComponent<LayoutElement>().minHeight=92;
            }
            if(visible.Length==0)InventoryNote(inventoryList.content,"이 필터에 맞는 장비가 없습니다. 하단 ‘필터’에서 조건을 바꿀 수 있습니다.");
            headerSubtitle.text=Loc.T(inventoryWarehouse?Loc.F("보관 {0}/400 · 필터 결과 {1}개", all.Length, visible.Length):Loc.F("가방 {0}/{1} · 필터 결과 {2}개", game.Store.Data.Hero.capacity-Economy.FreeSlots(game.Store.Data.Hero), game.Store.Data.Hero.capacity, visible.Length));
            if(InventorySelected==null){inventorySession.selectedId="";inventorySession.detailOpen=false;}
            BuildInventoryDetails();BuildInventoryFooter();inventoryDirty=true;ReflowInventory();RestoreInventoryList();
        }
        void SelectInventoryItem(string id)
        {
            var item=InventorySource.FirstOrDefault(i=>i.id==id);if(item==null){RefreshInventory();return;}
            CaptureInventoryList();inventorySession.selectedId=id;inventorySession.detailOpen=true;inventoryToolKind="";
            bool changed=item.acquiredOrder>0&&!item.reviewed;if(changed)item.reviewed=true;
            if(!inventoryWarehouse)changed|=FirstPlayGuide.ReadComparison(game.Store.Data.Hero,item);
            if(changed)game.Save();
            inventoryReading=inventoryDetail;RefreshInventory(false);inventoryDetail.verticalNormalizedPosition=1;inventoryComparison.verticalNormalizedPosition=1;
        }
        void BuildInventoryDetails()
        {
            foreach(var row in inventoryDetailRows.Concat(inventoryComparisonRows)){if(row!=null){row.gameObject.SetActive(false);Destroy(row.gameObject);}}
            inventoryDetailRows.Clear();inventoryComparisonRows.Clear();
            var a=game.Store.Data;var hero=a.Hero;var item=InventorySelected;
            var stats=new HeroStats(hero,false,game.Store.Data.runes);InventoryNote(inventoryDetail.content,Loc.F("현재 캐릭터 · HP {0:0} · 공격 기준 {1:0.0} · 방어도 {2:0}", stats.hp, stats.damage, stats.armor),18,muted);
            if(item==null)InventoryNote(inventoryDetail.content,"목록에서 장비를 선택하면 접두·접미와 장착 중인 장비를 비교할 수 있습니다.",22,gold);
            else
            {
                DescribeInventoryItem(inventoryDetail.content,item,"선택한 장비");
                if(!inventorySession.query.Matches(item))InventoryNote(inventoryDetail.content,"이 장비는 현재 목록 필터에서 제외되어 있지만 선택한 상세는 유지합니다.",18,gold);
                InventoryNote(inventoryDetail.content,Loc.F("보유 골드 {0:N0} · 재료 {1:N0}", a.gold, a.materials),18,muted);
                if(portalBag)InventoryNote(inventoryDetail.content,"균열을 보존한 채 가방을 정리합니다. 장착·강화·재설정은 균열을 완료한 뒤 이용할 수 있습니다.",20,gold);
                else if(!GearServiceAvailable)InventoryNote(inventoryDetail.content,"진행 중인 균열을 먼저 완료하면 장착·강화·재설정을 이용할 수 있습니다.",20,gold);
                string id=item.id;
                InventoryButton(inventoryDetail.content,item.locked?"잠금 해제":"장비 잠금",InventoryTransaction("lock:"+id+":"+!item.locked,staged=>
                {var owned=InventoryFind(staged,id);if(owned==null)return false;owned.locked=!item.locked;return true;},item.locked?"잠금을 해제했습니다.":"장비를 잠갔습니다."));
                if(inventoryWarehouse)
                {
                    InventoryButton(inventoryDetail.content,"현재 캐릭터의 가방으로 이동",InventoryTransaction("withdraw:"+id+":"+hero.id,staged=>
                    {var owned=staged.warehouse.Find(i=>i.id==id);if(owned==null||Economy.FreeSlots(staged.Hero)<=0)return false;staged.warehouse.Remove(owned);staged.Hero.inventory.Add(owned);return true;},"가방으로 옮겼습니다."));
                    InventoryNote(inventoryComparison.content,"창고의 장비는 현재 캐릭터의 가방으로 옮긴 뒤 장착할 수 있습니다. 아래 비교는 보유 장비를 바꾸지 않는 미리보기입니다.",20,gold);
                }
                else
                {
                    if(!portalBag&&GearServiceAvailable)
                    {
                        if(!ContentUnlocks.Has(game.Store.Data,ContentUnlocks.Enhance))InventoryNote(inventoryDetail.content,ContentUnlocks.Condition(ContentUnlocks.Enhance),18,muted);
                        if(item.enhancement<5&&ContentUnlocks.Has(game.Store.Data,ContentUnlocks.Enhance))InventoryButton(inventoryDetail.content,Loc.F("강화 +{0} · 재료 {1} / {2:N0} 골드", item.enhancement+1, Economy.EnhancementMaterials(item), Economy.EnhancementGold(item)),
                            InventoryTransaction("enhance:"+id+":"+item.enhancement,staged=>Economy.Enhance(staged,FindOwned(staged,id)),"강화를 완료했습니다."));
                        if(!ContentUnlocks.Has(game.Store.Data,ContentUnlocks.Reroll))InventoryNote(inventoryDetail.content,ContentUnlocks.Condition(ContentUnlocks.Reroll),18,muted);
                        if(item.rolls.Count>0&&ContentUnlocks.Has(game.Store.Data,ContentUnlocks.Reroll))InventoryButton(inventoryDetail.content,"한 줄 재설정",()=>ShowInventoryReroll(id));
                    }
                    if(!item.equipped)
                    {
                        bool canDispose=!Economy.Protected(hero,item)&&!a.heroes.Any(owner=>Economy.Referenced(owner,item));
                        var sell=InventoryButton(inventoryDetail.content,Loc.F("판매 · {0:N0} 골드", item.Price),()=>ShowInventoryConfirm(Loc.F("{0}\n판매하면 {1:N0} 골드를 받습니다. 이 장비는 사라집니다.", item.DisplayName, item.Price),InventoryTransaction("sell:"+id,staged=>Economy.Sell(staged,staged.Hero,FindOwned(staged,id)),"장비를 판매했습니다.",item)));sell.interactable=canDispose;
                        long refund=(item.contentVersion>0?item.investedMaterials:20L*((1<<item.enhancement)-1))*4/5;
                        string reward=item.rarity==3?Loc.F("{0} 코어 1개", GameCatalog.Slots[item.slot]):Loc.F("재료 {0}개", new[]{1,2,5}[item.rarity]);
                        var dismantle=InventoryButton(inventoryDetail.content,Loc.F("분해 · {0}{1}", reward, (refund>0?Loc.F(" + 강화 재료 {0}개", refund):"")),()=>ShowInventoryConfirm(Loc.F("{0}\n분해하면 {1}{2}를 받습니다. 이 장비는 사라집니다.", item.DisplayName, reward, (refund>0?Loc.F("와 강화 재료 {0}개", refund):"")),InventoryTransaction("dismantle:"+id,staged=>Economy.Dismantle(staged,staged.Hero,FindOwned(staged,id)),"장비를 분해했습니다.",item)));dismantle.interactable=canDispose;
                        InventoryButton(inventoryDetail.content,"공유 창고로 이동",InventoryTransaction("warehouse:"+id,staged=>
                        {var owned=FindOwned(staged,id);if(owned==null||owned.equipped||staged.warehouse.Count>=EdictCleanupPolicy.WarehouseCapacity)return false;staged.Hero.inventory.Remove(owned);staged.warehouse.Add(owned);return true;},"잠금·프리셋 참조·획득순을 유지하며 창고로 옮겼습니다."));
                    }
                }
                if(a.heroes.Any(owner=>Economy.Referenced(owner,item)))
                    InventoryButton(inventoryDetail.content,"현재 설정·프리셋의 장비 참조 해제",()=>ShowInventoryConfirm(Loc.F("{0}\n현재 설정과 모든 프리셋에서 이 장비의 참조만 해제합니다. 장착과 스킬 설정은 유지합니다.", item.DisplayName),InventoryTransaction("unreference:"+id,staged=>
                    {foreach(var owner in staged.heroes){owner.build.equipmentIds?.RemoveAll(x=>x==id);foreach(var preset in owner.presets)preset?.equipmentIds?.RemoveAll(x=>x==id);}return true;},"장비 참조를 해제했습니다.",item)));
                var current=hero.inventory.Find(i=>i.equipped&&i.slot==item.slot);
                if(current!=null&&current.id!=item.id)DescribeInventoryItem(inventoryComparison.content,current,"현재 장착한 장비");
                else InventoryNote(inventoryComparison.content,current==null?"현재 이 부위는 비어 있습니다.":"현재 장착 중인 장비를 보고 있습니다.",22,gold);
                var compareHero=hero;
                if(inventoryWarehouse){compareHero=JsonUtility.FromJson<HeroSave>(JsonUtility.ToJson(hero));compareHero.inventory.Add(JsonUtility.FromJson<Item>(JsonUtility.ToJson(item)));}
                content=inventoryComparison.content;ShowEquipmentComparison(compareHero,compareHero.inventory.Find(i=>i.id==item.id));
            }
            inventoryDetailRows.AddRange(inventoryDetail.content.Cast<Transform>().Where(t=>t.gameObject.activeSelf).Select(t=>(RectTransform)t));
            inventoryComparisonRows.AddRange(inventoryComparison.content.Cast<Transform>().Where(t=>t.gameObject.activeSelf).Select(t=>(RectTransform)t));
        }
        Item InventoryFind(AccountSave account,string id)=>inventoryWarehouse?account.warehouse.Find(i=>i.id==id):FindOwned(account,id);
        void DescribeInventoryItem(Transform parent,Item item,string heading)
        {
            var hero=game.Store.Data.Hero;var basis=ItemCatalog.Base(item);var unique=ItemCatalog.Unique(item.special);
            InventoryNote(parent,heading,22,gold);
            var title=Row(parent,90);EquipmentIcon(title,item,10,10,68);var label=Label(title,Loc.F("{0} +{1}",item.DisplayName,item.enhancement),24,ItemColor(item));Inset(label.rectTransform,88,8,8,8);
            string restriction=unique!=null&&unique.heroClass>=0?game.catalog.classNames[unique.heroClass]:basis.heroClass>=0?game.catalog.classNames[basis.heroClass]:"공용";
            InventoryNote(parent,Loc.F("{0} · {1} · {2}\n아이템 레벨 {3} · 요구 Lv.{4}\n{5}", Grade(item), restriction, GameCatalog.Slots[item.slot], item.level, item.RequiredLevel, Protection(item)),20,ItemColor(item));
            if(item.acquiredOrder<=0)InventoryNote(parent,"기존 장비 · 획득 순서 기록 없음",17,muted);
            float baseValue=basis.main*(1+.08f*(item.level-1)),main=ItemCatalog.MainValue(item);
            string mainName=item.slot==0?"무기 피해":item.slot<=5?"방어도":item.slot==6?"최대 HP":"비물리 저항";
            InventoryNote(parent,Loc.F("{0}\n{1} {2:0.0} = 기본 {3:0.0} + 강화 {4:0.0}{5}", basis.name, mainName, main, baseValue, main-baseValue, (basis.resistance>0?Loc.F("\n고정 부가 저항 {0:0.0}", basis.resistance*(1+.08f*(item.level-1))):basis.attackSpeed!=0?Loc.F("\n고정 공격속도 {0:+0%;-0%}", basis.attackSpeed):"")));
            foreach(var roll in item.rolls)
            {
                var def=ItemCatalog.Affix(roll.affixId);
                InventoryNote(parent,Loc.F("{0} · {1} [{2}]\n{3} +{4:0.##} · 가능 범위 {5:0.##}~{6:0.##}{7}{8}", (roll.side==AffixSide.Prefix?"접두":"접미"), def.phrase, roll.tierId, StatCatalog.Name(def.stat), roll.value, def.Value(item.level,0), def.Value(item.level,10000), (roll.legacyRoll?"\n기존 수치를 보존한 옵션입니다.":""), (item.rerollSlotId==roll.slotId?"\n재설정 대상으로 선택한 줄입니다.":"")));
            }
            InventoryNote(parent,GemCatalog.SocketSummary(item),20,muted);
            if(unique!=null)
            {
                if(unique.setId=="")InventoryNote(parent,unique.Description,21,ItemColor(item));
                if(!string.IsNullOrEmpty(unique.requiredSkill))
                {
                    var skill=game.catalog.skills.Find(s=>s.id==unique.requiredSkill);int index=game.catalog.skills.IndexOf(skill);
                    bool active=hero.build.activeSkills.Contains(index)&&hero.level>=skill.unlock;
                    InventoryNote(parent,Loc.F("{0}{1} · Lv.{2} 필요", (active?"필수 스킬 장착·해금 완료: ":"현재 효과 비활성: "), skill.name, skill.unlock),20,active?setGreen:gold);
                }
                if(unique.setId!="")
                {
                    var set=ItemCatalog.Sets.Single(s=>s.id==unique.setId);int worn=hero.inventory.Count(i=>i.equipped&&ItemCatalog.Unique(i.special)?.setId==set.id);
                    int bag=hero.inventory.Count(i=>ItemCatalog.Unique(i.special)?.setId==set.id),storage=game.Store.Data.warehouse.Count(i=>ItemCatalog.Unique(i.special)?.setId==set.id);
                    InventoryNote(parent,Loc.F("{0}\n현재 장착 {1}부위 · 현재 캐릭터 보유 {2}개 · 창고 {3}개\n2세트{4}: {5}\n4세트{6}: {7}", set.name, worn, bag, storage, (worn>=2?" 활성":" 비활성"), set.two, (worn>=4?" 활성":" 비활성"), set.four),20,setGreen);
                }
            }
        }
        Action InventoryTransaction(string operation,Func<AccountSave,bool> mutation,string success,Item quotedItem=null)
        {
            string request=Guid.NewGuid().ToString("N");bool committed=false;
            string heroId=game.Store.Data.Hero.id;bool warehouse=inventoryWarehouse;
            // Confirmations must validate the same item snapshot that supplied their displayed quote.
            var selected=operation.StartsWith("capacity:",StringComparison.Ordinal)?null:quotedItem??InventorySelected;
            string itemId=selected?.id,snapshot=selected==null?null:InventoryBulkPlan.Fingerprint(game.Store.Data,selected);
            return ()=>
            {
                if(committed)return;bool ok=game.Store.Transact(request,operation,staged=>
                {
                    if(staged.Hero.id!=heroId)return false;
                    if(itemId!=null)
                    {
                        var current=warehouse?staged.warehouse.Find(i=>i.id==itemId):FindOwned(staged,itemId);
                        if(current==null||InventoryBulkPlan.Fingerprint(staged,current)!=snapshot)return false;
                    }
                    return mutation(staged);
                });committed=ok;
                if(ok){inventoryToolKind="";RefreshInventory();ShowToast(success);}
                else ShowToast(game.Store.Error);
            };
        }
        void BuildInventoryFooter()
        {
            ClearInventoryRows(footer);
            if(inventoryToolKind!="")return;
            var selected=InventorySelected;
            FooterButton(0,6,"목록",()=>{inventorySession.detailOpen=false;inventoryReading=inventoryList;inventoryDirty=true;ReflowInventory();});
            FooterButton(1,6,"필터",ShowInventoryFilters);
            FooterButton(2,6,"정리",()=>ShowInventoryBulk(InventoryBulkOperation.Dismantle));
            FooterButton(3,6,inventoryWarehouse?"가방":"창고",()=>OpenInventory(!inventoryWarehouse,portalBag));
            var equip=Button(footer,"장착",selected==null?()=>{}:InventoryTransaction("equip:"+selected.id,staged=>staged.suspendedRun==null&&Economy.Equip(staged.Hero,FindOwned(staged,selected.id)),"장비를 교체했습니다."),new Color(.42f,.28f,.12f));AnchorButton(equip,4,6);
            equip.interactable=!inventoryWarehouse&&!portalBag&&GearServiceAvailable&&selected!=null&&Economy.EquipError(game.Store.Data.Hero,selected)=="";
            bool result=game.Combat!=null&&!game.Active;
            FooterButton(5,6,portalBag?"균열 복귀":result?"결과로":"성소로",()=>{if(portalBag)game.ContinuePortal();else if(result)ShowResult();else ShowTown();},portalBag);
        }
        void OpenInventoryTool(string kind)
        {CaptureInventoryList();inventoryToolKind=kind;ClearInventoryRows(inventoryTool.content);ClearInventoryRows(footer);inventoryTool.verticalNormalizedPosition=1;inventoryReading=inventoryTool;inventoryDirty=true;}
        void CloseInventoryTool(){inventoryToolKind="";BuildInventoryFooter();inventoryDirty=true;ReflowInventory();RestoreInventoryList();}
        void ShowInventoryFilters()
        {
            OpenInventoryTool("filters");var q=inventorySession.query;var body=inventoryTool.content;
            InventoryNote(body,"필터와 정렬",26,gold);
            InventoryButton(body,Loc.F("부위 · {0}", (q.slot<0?"모두":GameCatalog.Slots[q.slot])),()=>InventoryChoose("부위",new[]{"모두"}.Concat(GameCatalog.Slots).ToArray(),q.slot+1,n=>q.slot=n-1));
            InventoryButton(body,Loc.F("등급 · {0}", InventoryGrades[(int)q.grade]),()=>InventoryChoose("등급",InventoryGrades,(int)q.grade,n=>q.grade=(InventoryGrade)n));
            string[] classes={"모든 직업","공용 장비", "전사가 사용 가능", "궁수가 사용 가능", "마법사가 사용 가능"};
            InventoryButton(body,Loc.F("직업 · {0}", classes[q.heroClass+2]),()=>InventoryChoose("직업",classes,q.heroClass+2,n=>q.heroClass=n-2));
            string[] sets=new[]{"모든 세트·비세트"}.Concat(ItemCatalog.Sets.Select(s=>s.name)).ToArray();int setIndex=ItemCatalog.Sets.ToList().FindIndex(s=>s.id==q.setId)+1;
            InventoryButton(body,Loc.F("세트 · {0}", sets[setIndex]),()=>InventoryChoose("세트",sets,setIndex,n=>q.setId=n==0?"":ItemCatalog.Sets[n-1].id));
            string[] effects=new[]{"모든 접사 효과"}.Concat(ItemCatalog.Affixes.Select(d=>Loc.F("{0} · {1}",StatCatalog.Name(d.stat),d.phrase))).ToArray();int affix=ItemCatalog.Affixes.ToList().FindIndex(d=>d.id==q.affixId)+1;
            InventoryButton(body,Loc.F("접사 효과 · {0}", effects[affix]),()=>InventoryChoose("접사 효과",effects,affix,n=>q.affixId=n==0?"":ItemCatalog.Affixes[n-1].id));
            string[] uniques=new[]{"모든 특수 효과"}.Concat(ItemCatalog.Uniques.Select(d=>Loc.F("{0} · {1}",d.name,GameCatalog.Slots[d.slot]))).ToArray();int unique=ItemCatalog.Uniques.ToList().FindIndex(d=>d.id==q.uniqueId)+1;
            InventoryButton(body,Loc.F("전설·세트 효과 · {0}", uniques[unique]),()=>InventoryChoose("전설·세트 효과",uniques,unique,n=>q.uniqueId=n==0?"":ItemCatalog.Uniques[n-1].id));
            InventoryButton(body,Loc.F("정렬 · {0}", InventoryOrders[(int)q.order]),()=>InventoryChoose("정렬",InventoryOrders,(int)q.order,n=>q.order=(InventoryOrder)n));
            InventoryButton(body,q.unreadOnly?"새 장비만 · 켜짐":"새 장비만 · 꺼짐",()=>{q.unreadOnly=!q.unreadOnly;ShowInventoryFilters();});
            InventoryNote(body,"최근 획득순은 실제로 계정에 들어온 순서를 사용합니다. 창고나 다른 캐릭터로 옮겨도 순서는 유지합니다. 기존 저장에 순서가 없는 장비는 별도로 뒤에 표시합니다.",18,muted);
            if(!inventoryWarehouse&&!portalBag&&game.Store.Data.Hero.capacity<100)
            {
                int capacity=game.Store.Data.Hero.capacity,cost=5000*(1<<((capacity-50)/10));
                InventoryButton(body,Loc.F("가방 +10칸 · {0:N0} 골드", cost),()=>ShowInventoryConfirm(Loc.F("가방을 {0}칸에서 {1}칸으로 늘립니다.\n{2:N0} 골드가 필요합니다.", capacity, capacity+10, cost),InventoryTransaction("capacity:"+game.Store.Data.Hero.id+":"+capacity,staged=>
                {if(staged.Hero.capacity!=capacity||staged.Hero.capacity>=100||staged.gold<cost)return false;staged.gold-=cost;staged.Hero.capacity+=10;return true;},"가방을 10칸 확장했습니다.")));
            }
            InventoryButton(body,"전설·세트 도감",ShowItemCollection);
            FooterButton(0,2,"조건 초기화",()=>{inventorySession.query=new InventoryQuery();ShowInventoryFilters();});
            FooterButton(1,2,"목록 보기",()=>{inventorySession.listOffset=0;inventorySession.anchorId="";inventoryToolKind="";RefreshInventory(false);},true);ReflowInventory();
        }
        void InventoryChoose(string title,string[] values,int selected,Action<int> choose)
        {
            OpenInventoryTool("choice");InventoryNote(inventoryTool.content,title,26,gold);
            for(int i=0;i<values.Length;i++){int index=i;InventoryButton(inventoryTool.content,Loc.F("{0}{1}", (i==selected?"선택됨 · ":""), values[i]),()=>{choose(index);ShowInventoryFilters();},i==selected);}
            FooterButton(0,1,"필터로 돌아가기",ShowInventoryFilters);ReflowInventory();
        }
        void ShowInventoryBulk(InventoryBulkOperation operation)
        {
            OpenInventoryTool("bulk");
            if(inventoryWarehouse){InventoryNote(inventoryTool.content,"창고 장비는 가방으로 옮긴 뒤 판매·분해할 수 있습니다.");FooterButton(0,1,"닫기",CloseInventoryTool);ReflowInventory();return;}
            inventoryBulk=new InventoryBulkPlan(game.Store.Data,inventorySession.query.Apply(InventorySource).Select(i=>i.id),operation);
            var plan=inventoryBulk;var body=inventoryTool.content;
            InventoryNote(body,operation==InventoryBulkOperation.Sell?"일괄 판매 미리보기":"일괄 분해 미리보기",26,gold);
            InventoryNote(body,Loc.F("현재 필터의 장비 중 {0}개를 처리합니다.\n예상 골드 +{1:N0} · 재료 +{2:N0}\n보호·전설·세트 제외 {3}개", plan.Count, plan.Gold, plan.Materials, plan.entries.Count-plan.Count),22,gold);
            InventoryNote(body,"아래에 표시된 장비만 처리합니다. 확정 전 장비나 보호 상태가 바뀌면 전체 처리를 중단합니다. 강화 재료는 실제 투자량의 80%를 회수하고 골드는 환급하지 않습니다.",18,muted);
            InventoryButton(body,operation==InventoryBulkOperation.Sell?"분해로 변경":"판매로 변경",()=>ShowInventoryBulk(operation==InventoryBulkOperation.Sell?InventoryBulkOperation.Dismantle:InventoryBulkOperation.Sell));
            InventoryButton(body,"현재 장비로 미리보기 갱신",()=>ShowInventoryBulk(operation));
            InventoryNote(body,"처리 대상",22,gold);
            foreach(var entry in plan.entries.Where(e=>e.excludedReason==""))InventoryNote(body,entry.name+(operation==InventoryBulkOperation.Sell?Loc.F("\n골드 +{0:N0}", entry.gold):Loc.F("\n재료 +{0:N0}", entry.materials)));
            if(plan.Count==0)InventoryNote(body,"처리할 장비가 없습니다.");
            InventoryNote(body,"제외된 장비",22,gold);
            foreach(var entry in plan.entries.Where(e=>e.excludedReason!=""))InventoryNote(body,Loc.F("{0}\n{1}",entry.name,Loc.StoredText(entry.excludedReason)),19,muted);
            FooterButton(0,2,"취소",CloseInventoryTool);
            bool committed=false;var confirm=Button(footer,operation==InventoryBulkOperation.Sell?"판매 확정":"분해 확정",()=>
            {
                if(committed)return;committed=game.Store.Transact(plan.requestId,plan.operationKey,plan.Apply);
                if(committed){inventoryToolKind="";RefreshInventory();ShowToast(Loc.F("장비 {0}개를 처리했습니다. 골드 +{1:N0} · 재료 +{2:N0}", plan.Count, plan.Gold, plan.Materials));}
                else ShowToast(Loc.F("처리하지 않았습니다. 장비·보호 상태와 저장 공간을 확인한 뒤 다시 시도해 주세요. {0}", game.Store.Error));
            },new Color(.46f,.27f,.12f));AnchorButton(confirm,1,2);confirm.interactable=plan.Count>0;ReflowInventory();
        }
        void ShowInventoryConfirm(string message,Action confirm)
        {
            OpenInventoryTool("confirm");InventoryNote(inventoryTool.content,"장비 처리 확인",26,gold);InventoryNote(inventoryTool.content,message,22);
            FooterButton(0,2,"취소",CloseInventoryTool);FooterButton(1,2,"확인",confirm,true);ReflowInventory();
        }
        void ShowInventoryReroll(string id)
        {
            if(!RequireContent(ContentUnlocks.Reroll))return;
            var item=FindOwned(game.Store.Data,id);if(item==null){RefreshInventory();return;}
            OpenInventoryTool("reroll");var body=inventoryTool.content;
            InventoryNote(body,Loc.F("한 줄 재설정 · {0}", item.DisplayName),24,gold);
            InventoryNote(body,"처음 고른 한 줄만 계속 재설정할 수 있습니다. 접두·접미 종류를 유지하며 새 결과는 즉시 적용됩니다. 현재보다 낮은 수치가 나올 수 있습니다.");
            InventoryNote(body,Loc.F("다음 비용 {0:N0} 골드 · 보유 {1:N0}", Economy.RerollGold(item), game.Store.Data.gold),22,gold);
            foreach(var roll in item.rolls)
            {
                string slot=roll.slotId;var definition=ItemCatalog.Affix(roll.affixId);
                var button=InventoryButton(body,Loc.F("{0} · {1} +{2:0.##} [{3}]", (roll.side==AffixSide.Prefix?"접두":"접미"), StatCatalog.Name(definition.stat), roll.value, roll.tierId),()=>
                {
                    uint rng=(uint)DateTime.UtcNow.Ticks;
                    ShowInventoryConfirm(Loc.F("{0} 한 줄을 재설정합니다.\n비용 {1:N0} 골드 · 수치가 낮아질 수 있습니다.", StatCatalog.Name(definition.stat), Economy.RerollGold(item)),InventoryTransaction("reroll:"+id+":"+slot+":"+item.rerolls,staged=>Economy.Reroll(staged,FindOwned(staged,id),slot,ref rng),"한 줄을 재설정했습니다.",item));
                });button.interactable=!portalBag&&GearServiceAvailable&&(string.IsNullOrEmpty(item.rerollSlotId)||item.rerollSlotId==slot)&&ItemGenerator.RerollPool(item,slot).Count>0;
            }
            FooterButton(0,1,"장비 상세로",CloseInventoryTool);ReflowInventory();
        }
        void FitInventoryContent(ScrollRect scroll)
        {
            float padding=Mathf.Max(8,(scroll.viewport.rect.width-740)/2);scroll.content.GetComponent<VerticalLayoutGroup>().padding=new RectOffset((int)padding,(int)padding,8,8);
            LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            foreach(RectTransform row in scroll.content)
            {
                if(!row.gameObject.activeSelf)continue;var le=row.GetComponent<LayoutElement>();if(le==null)continue;
                var text=row.GetComponentInChildren<Text>();if(text==null)continue;
                float extra=-text.rectTransform.offsetMax.y+text.rectTransform.offsetMin.y;
                le.preferredHeight=Mathf.Max(row.GetComponent<Button>()!=null?Mathf.Max(52,le.minHeight):40,text.preferredHeight+extra+2);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        }
        void ReflowInventory()
        {
            if(inventoryList==null)return;var size=root.rect.size;var safe=UiSafeArea.Current;
            if(!inventoryDirty&&size==inventorySize&&safe==inventorySafe)return;
            var listAnchor=DialogReadingAnchor.Capture(inventoryList);var detailAnchor=DialogReadingAnchor.Capture(inventoryDetail);var compareAnchor=DialogReadingAnchor.Capture(inventoryComparison);var toolAnchor=DialogReadingAnchor.Capture(inventoryTool);
            Canvas.ForceUpdateCanvases();size=root.rect.size;inventorySize=size;inventorySafe=safe;inventoryDirty=false;
            // Size the fixed actions from their translated text before reserving the reading area.
            // At 140% a short landscape window may need a third line on its return button.
            Place(footer,0,size.y-72,size.x,72);
            foreach(var button in footer.GetComponentsInChildren<Button>())
            {
                var r=(RectTransform)button.transform;r.offsetMin=new Vector2(4,12);r.offsetMax=new Vector2(-4,-4);
                var label=button.GetComponentInChildren<Text>();label.fontSize=size.x<600?17:20;
                string caption=size.x<600&&button.name=="성소로"?"뒤로":size.x<600&&button.name=="창고"?"보관함":button.name;
                label.text=Loc.T(caption);
            }
            Canvas.ForceUpdateCanvases();
            float footerHeight=72;
            foreach(var label in footer.GetComponentsInChildren<Text>())
                footerHeight=Mathf.Max(footerHeight,label.preferredHeight-label.rectTransform.offsetMax.y+label.rectTransform.offsetMin.y+18);
            var plan=new InventoryLayout(size.x,size.y,inventorySession.detailOpen,footerHeight);
            Place(header,plan.header.x,plan.header.y,plan.header.width,plan.header.height);Place(footer,plan.footer.x,plan.footer.y,plan.footer.width,plan.footer.height);
            headerTitle.fontSize=size.x<600?23:28;Place(headerTitle.rectTransform,12,6,size.x-150,36);
            headerSubtitle.fontSize=17;Place(headerSubtitle.rectTransform,12,44,size.x-152,plan.header.height-46);
            var settings=header.GetComponentsInChildren<Button>().Single(b=>b.name=="설정·안내");Place((RectTransform)settings.transform,size.x-136,6,124,52);
            Place((RectTransform)inventoryList.transform,plan.list.x,plan.list.y,plan.list.width,plan.list.height);
            Place((RectTransform)inventoryDetail.transform,plan.detail.x,plan.detail.y,plan.detail.width,plan.detail.height);
            Place((RectTransform)inventoryComparison.transform,plan.comparison.x,plan.comparison.y,plan.comparison.width,plan.comparison.height);
            Place((RectTransform)inventoryTool.transform,plan.tool.x,plan.tool.y,plan.tool.width,plan.tool.height);
            bool tools=inventoryToolKind!="";
            inventoryList.gameObject.SetActive(!tools&&(plan.split||!inventorySession.detailOpen));
            inventoryDetail.gameObject.SetActive(!tools&&(plan.split||inventorySession.detailOpen));
            inventoryComparison.gameObject.SetActive(!tools&&plan.three);inventoryTool.gameObject.SetActive(tools);
            foreach(var row in inventoryDetailRows)if(row!=null)row.SetParent(inventoryDetail.content,false);
            foreach(var row in inventoryComparisonRows)if(row!=null)row.SetParent(plan.three?inventoryComparison.content:inventoryDetail.content,false);
            Canvas.ForceUpdateCanvases();foreach(var scroll in new[]{inventoryList,inventoryDetail,inventoryComparison,inventoryTool})if(scroll.gameObject.activeSelf)FitInventoryContent(scroll);
            Canvas.ForceUpdateCanvases();listAnchor?.Restore();detailAnchor?.Restore();compareAnchor?.Restore();toolAnchor?.Restore();
            if(inventoryReading==inventoryDetail)detailAnchor?.Restore();else if(inventoryReading==inventoryComparison)compareAnchor?.Restore();
            toastFrame.anchoredPosition=new Vector2(0,plan.footer.height+4);
            Page=inventoryWarehouse?"warehouse":portalBag||!inventorySession.detailOpen?"bag":"item";
        }
        void ClearInventoryLayout()
        {
            CaptureInventoryList();inventoryList=inventoryDetail=inventoryComparison=inventoryTool=inventoryReading=null;inventoryDetailRows.Clear();inventoryComparisonRows.Clear();inventorySize=Vector2.zero;inventoryDirty=false;inventoryBulk=null;inventoryToolKind="";
        }
    }
}

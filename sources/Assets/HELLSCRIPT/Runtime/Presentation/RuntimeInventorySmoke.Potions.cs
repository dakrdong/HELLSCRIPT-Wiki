using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class RuntimeInventorySmoke
    {
        void PotionGeometry()
        {
            Geometry();var slots=Enumerable.Range(0,3).Select(i=>Named("inventory-potion-"+i)).ToArray();
            var weapon=Named("equipment-0-1");var gear=Named("inventory-potion-settings");
            Require(View.GetComponentsInChildren<Button>().Count(b=>b.name=="inventory-potion-settings")==1&&gear.Find("gear")!=null,"Expected one shared settings gear.");
            Require(Bounds(gear).xMin>=Bounds(slots[2]).xMax&&Mathf.Abs(Center(gear).y-Center(weapon).y)<1,"Gear is not beside the potion group.");
            foreach(var slot in slots)
            {
                Require(slot.rect.width<View.SlotSize*.7f&&slot.rect.width==slot.rect.height,"Potion slot should be smaller than equipment.");
                Require(slot.Find("Potion settings icon")==null,"Individual potion gear remains.");
                Require(Bounds(slot).xMin>=Bounds(weapon).xMax&&Mathf.Abs(Center(slot).y-Center(weapon).y)<1,"Potion added a row instead of sitting beside weapons.");
                var art=slot.Find("Potion art").GetComponent<Image>();var mesh=art.canvasRenderer.GetMesh();
                Require(mesh.vertexCount>0,"Potion has no rendered artwork.");var center=art.transform.TransformPoint(mesh.bounds.center);
                Require(Vector2.Distance(center,Center(slot))<1,"Potion artwork is off centre: "+slot.name);
                foreach(var equipment in View.GetComponentsInChildren<InventoryCell>().Where(c=>!c.bag))Require(!Bounds(slot).Overlaps(Bounds((RectTransform)equipment.transform)),"Potion overlaps equipment.");
                Require(Within(Bounds(Named("Character equipment")),Bounds(slot)),"Potion escaped character panel.");
                Require(!Bounds(slot).Overlaps(Bounds(Named("Bag panel"))),"Potion overlaps inventory.");
            }
            if(View.DialogRect!=null)
                foreach(var text in View.DialogRect.GetComponentsInChildren<Text>())
                    if(!text.resizeTextForBestFit)Require(text.preferredHeight<=text.rectTransform.rect.height+1,"Potion text clipped: "+text.text+" / "+text.preferredHeight+" > "+text.rectTransform.rect.height);
        }
        void WalletGeometry(bool details=false)
        {
            var a=game.Store.Data;var rows=new[]{("gold",a.gold),("premium",a.premium),("materials",a.materials),("stones",a.enhancementStones)};
            var texts=View.GetComponentsInChildren<Text>();
            foreach(var row in rows)
            {
                var text=texts.Single(t=>t.name=="wallet-value-"+row.Item1);
                Require(text.text==row.Item2.ToString("N0"),"Wallet does not show the account balance: "+row.Item1);
                Require(text.cachedTextGenerator.lineCount==1,"Wallet amount wraps or truncates: "+text.text);
                if(details)Require(texts.Single(t=>t.name=="wallet-detail-value-"+row.Item1).text==row.Item2.ToString("N0"),"Detailed balance is stale: "+row.Item1);
            }
            Require(View.GetComponentsInChildren<Image>().Any(i=>i.name=="Abyssal Coin"&&i.sprite!=null&&i.sprite.texture.width==1254),"Original Abyssal Coin sprite was not imported.");
            bool Overlaps(Rect a,Rect b)=>Mathf.Min(a.xMax,b.xMax)-Mathf.Max(a.xMin,b.xMin)>.5f&&Mathf.Min(a.yMax,b.yMax)-Mathf.Max(a.yMin,b.yMin)>.5f;
            Require(!Overlaps(Bounds(Named("Wallet summary")),Bounds(Named("Bag panel"))),"Wallet overlaps the bag.");
            Require(!Overlaps(Bounds(Named("Wallet summary")),Bounds(Named("Character equipment"))),"Wallet overlaps equipment.");
            if(details)
            {
                for(int i=0;i<a.cores.Length;i++)Require(texts.Single(t=>t.name=="wallet-detail-value-core-"+i).text==a.cores[i].ToString("N0"),"Core balance is stale.");
                var scroll=View.DialogRect.GetComponentInChildren<ScrollRect>();Require(scroll.content.rect.height>scroll.viewport.rect.height,"Resource list should scroll inside a fixed sheet.");
            }
        }
        IEnumerator PotionSlotsAcceptance()
        {
            Prepare(HeroClass.Warrior);game.EnterPlaza(true);
            Require(game.Store.Transact(Guid.NewGuid().ToString("N"),"potion-slot-fixture",a=>
            {a.Hero.potions.Activate();foreach(var def in PotionCatalog.All)a.Hero.potions.Set(def.id,12);a.premium=4321;a.enhancementStones=12345;for(int i=0;i<a.cores.Length;i++)a.cores[i]=i*13;return true;}),game.Store.Error);
            int checks=0;
            foreach(var size in new[]{(440,956),(956,440),(1440,810),(1440,900),(1680,720)})
            foreach(string language in new[]{"ko","en"})
            foreach(int percent in new[]{100,150})
            {
                game.UI.ClosePlayInventory();yield return PolishResize(size.Item1,size.Item2);game.ApplyLanguage(language);
                game.InterfaceScale.Apply(percent);game.UI.ApplyInterfaceScale();game.UI.ShowPlayInventory();yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();
                PotionGeometry();WalletGeometry();string suffix=$"{size.Item1}x{size.Item2}-{language}-{percent}";
                var frame=Bounds(View.FrameRect);var bag=Bounds(View.BagScroll.viewport);var positions=Enumerable.Range(0,3).Select(i=>Bounds(Named("inventory-potion-"+i))).ToArray();
                if(percent==100)yield return Capture("slots-"+suffix);
                string accountBefore=JsonUtility.ToJson(game.Store.Data);Click(View.Find("inventory-wallet"));yield return new WaitForEndOfFrame();PotionGeometry();WalletGeometry(true);
                Require(accountBefore==JsonUtility.ToJson(game.Store.Data),"Inspecting resources mutated the account.");
                var walletDialog=View.DialogRect;var walletBounds=Bounds(walletDialog);var balanceScroll=walletDialog.GetComponentInChildren<ScrollRect>();balanceScroll.verticalNormalizedPosition=0;yield return new WaitForEndOfFrame();
                if(percent==150&&size.Item1==440)yield return Capture("wallet-scrolled-"+suffix);
                Require(game.Store.Transact(Guid.NewGuid().ToString("N"),"wallet-refresh-fixture",a=>{a.gold=checks%2==0?int.MaxValue:0;a.premium=checks%2==0?int.MaxValue:0;a.materials=checks%2==0?int.MaxValue:0;a.enhancementStones=checks%2==0?int.MaxValue:0;return true;}),game.Store.Error);
                yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();WalletGeometry(true);
                Require(walletDialog==View.DialogRect&&walletBounds==Bounds(View.DialogRect)&&balanceScroll.verticalNormalizedPosition<.01f,"Balance refresh moved the sheet or reset scrolling.");
                Click(View.Find("inventory-wallet-close"));yield return new WaitForEndOfFrame();
                for(int i=0;i<4;i++)
                {
                    Click(View.Find("inventory-potion-settings"));yield return new WaitForEndOfFrame();PotionGeometry();var bubble=View.DialogRect;var bounds=Bounds(bubble);
                    int choice=(checks+i)%4;Click(View.Find("potion-fallback-"+choice));yield return new WaitForEndOfFrame();
                    Require(Hero.potions.SharedFallback==(PotionFallback)choice,"Shared policy selection did not save.");
                    Require(View.DialogRect==bubble&&Bounds(bubble)==bounds,"Policy selection moved or rebuilt the bubble.");
                    for(int n=0;n<4;n++)
                    {
                        var check=View.Find("potion-fallback-"+n).transform.Find("Selection checkbox");
                        Require((check.childCount>0&&check.GetChild(0).gameObject.activeSelf)==(n==choice),"Policy selected-state marker is wrong.");
                    }
                    Require(Bounds(View.FrameRect)==frame&&Bounds(View.BagScroll.viewport)==bag&&positions.SequenceEqual(Enumerable.Range(0,3).Select(n=>Bounds(Named("inventory-potion-"+n)))),"Settings changed inventory geometry.");
                    if(i==0)yield return Capture("policy-"+suffix);
                    Click(View.Find("potion-settings-close"));yield return new WaitForEndOfFrame();
                }
                Click(View.Find("inventory-potion-2"));yield return new WaitForEndOfFrame();PotionGeometry();
                Require(!View.Find("potion-pick-PH01").interactable&&!View.Find("potion-pick-PM01").interactable,"Another slot's potion can be assigned twice.");
                if(percent==150&&size.Item1==440)yield return Capture("picker-"+suffix);
                Click(View.Find("potion-picker-back"));yield return new WaitForEndOfFrame();View.Dismiss();yield return new WaitForEndOfFrame();checks++;
            }
            game.UI.ClosePlayInventory();game.InterfaceScale.Apply(100);game.UI.ApplyInterfaceScale();game.ApplyLanguage("ko");yield return PolishResize(440,956);game.UI.ShowPlayInventory();yield return new WaitForEndOfFrame();
            Click(View.Find("inventory-potion-2"));yield return new WaitForEndOfFrame();Click(View.Find("potion-clear"));yield return new WaitForEndOfFrame();
            Require(Hero.potions.slots[2].id=="","Potion slot was not cleared.");
            Click(View.Find("potion-pick-PU01"));yield return new WaitForEndOfFrame();
            Require(Hero.potions.slots[2].id=="PU01","Owned potion was not assigned.");
            var saved=new GameStore(Environment.GetCommandLineArgs()[Array.IndexOf(Environment.GetCommandLineArgs(),"-hellscriptSavePath")+1]);
            Require(saved.Data.Hero.potions.slots.Select(s=>s.id).SequenceEqual(Hero.potions.slots.Select(s=>s.id))&&saved.Data.Hero.potions.SharedFallback==Hero.potions.SharedFallback,"Potion slots and shared policy did not round-trip.");
            View.Dismiss();yield return new WaitForEndOfFrame();
            var cell=View.Cell("drag-candidate",true);
            var pointer=new PointerEventData(EventSystem.current){position=Center((RectTransform)cell.transform),button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(cell.gameObject,pointer,ExecuteEvents.beginDragHandler);yield return new WaitForEndOfFrame();pointer.position=Center(Named("inventory-potion-0"));
            ExecuteEvents.Execute(cell.gameObject,pointer,ExecuteEvents.dragHandler);ExecuteEvents.Execute(cell.gameObject,pointer,ExecuteEvents.endDragHandler);yield return new WaitForEndOfFrame();
            Require(!Hero.inventory.Single(i=>i.id=="drag-candidate").equipped&&Hero.potions.slots[0].id=="PH01","Equipment was accepted by a potion slot.");
            yield return Capture("assigned-potions-portrait");game.UI.ClosePlayInventory();yield return new WaitForSecondsRealtime(.2f);
            Require(game.UI.GlobalHud.Snapshot.potions[2].id=="PU01","HUD does not use inventory assignments.");
            for(int i=0;i<3;i++)
            {
                var root=game.UI.GlobalHud.transform.Find("HUD safe area/Potion "+i);var image=root.Find("Icon").GetComponent<Image>();var mesh=image.canvasRenderer.GetMesh();
                Require(mesh.vertexCount>0&&Mathf.Abs(image.transform.TransformPoint(mesh.bounds.center).x-Center((RectTransform)root).x)<1,"HUD bottle is not centred.");
            }
            Require(Loc.MissingCount==0,"Missing potion translations: "+string.Join(";",Loc.Missing));
            File.WriteAllText(Path.Combine(output,"potion-slots-result.txt"),"PASS: 20 resolution/language/text-scale combinations; three compact potion-only slots beside the weapon row, one shared gear, anchored fixed-size speech bubble, four exclusive persisted choices applying to all slots, direct owned-potion selection and clearing, duplicate prevention, equipment-drop rejection, save reload, HUD assignment and rendered bottle centring. Four account balance counters plus eight core balances: exact zero/int.MaxValue values, original Abyssal Coin art, no UI-only grants or spending, committed changes refresh the open sheet without moving it or resetting scroll. Native macOS with synthetic uGUI input and isolated saves; physical mobile not tested.\n");
            Debug.Log("HELLSCRIPT_POTION_SLOTS_RUNTIME_OK");Application.Quit(0);
        }
    }
}

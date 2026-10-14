using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // The shared content shell owns pause, back, safe area and locale. This adapter keeps selection and tier.
    public sealed partial class MonsterCodexWindow:MonoBehaviour
    {
        GameController game;ContentWindowView view;int field=MonsterCodexPreview.All,tier;string selected;bool elite;
        float listOffset,skillOffset;ScrollRect skillsScroll;SettingsStepSlider slider;
        int tuningTier=-1;string tuningHash;LiveOpsRiftSettings tuning;
        Text life,attack,speed,lifeNote,attackNote,tierValue,tierStatus;Button minus,plus;
        readonly List<(Text text,MonsterCodexSkill skill)> damageRows=new List<(Text,MonsterCodexSkill)>();
        readonly List<RectTransform> cards=new List<RectTransform>();
        MonsterDefinition Definition=>selected==null?null:MonsterRegistry.ById(selected);
        AccountSave Account=>game.Store.Data;
        bool PortraitLayout=>view.Frame.rect.height>view.Frame.rect.width;
        float Touch=>Mathf.Max(44,44/view.GetComponent<Canvas>().scaleFactor);
        public static ContentWindowView Open(Transform parent,GameController game,Func<float> reading=null,Action close=null)
        {
            MonsterCodexWindow owner=null;
            return ContentWindowView.Open(parent,"몬스터 도감",EquipmentViewSource.Catalog,reading??(()=>1),v=>
            {
                if(owner==null)
                {
                    owner=v.gameObject.AddComponent<MonsterCodexWindow>();owner.game=game;owner.view=v;owner.tier=MonsterCodex.InitialTier(game.Store.Data);
                    StoreViewBinding.Attach(v,game.Store,owner.Repaint,()=>owner.slider==null||!owner.slider.Dragging,visibleState:owner.VisibleState);
                    MonsterRegistry.Changed+=owner.Repaint;
                }
                owner.Render(v);
            },close,back:()=>owner.Back());
        }
        string VisibleState()=>string.Join("|",Account.Hero.id,Account.Hero.highestClear,StoreViewBinding.Key(Account.monsterCodex),game.LiveOps?.ConfigHash);
        void OnDestroy(){MonsterRegistry.Changed-=Repaint;}
        void Back(){if(selected==null){view.Close();return;}selected=null;elite=false;Repaint();}
        void Repaint()
        {
            if(view==null)return;float savedList=listOffset;
            view.Repaint();if(selected==null){listOffset=Mathf.Clamp(savedList,0,Mathf.Max(0,view.Body.rect.height-view.Scroll.viewport.rect.height));view.Body.anchoredPosition=new Vector2(0,listOffset);CullCards();}
        }
        void Render(ContentWindowView v)
        {
            if(skillsScroll!=null)skillOffset=skillsScroll.content.anchoredPosition.y;
            if(selected!=null&&(Definition==null||MonsterCodex.Count(Account.monsterCodex,selected)<=0))selected=null;
            tier=Mathf.Clamp(tier,1,MonsterCodex.MaximumTier(Account));cards.Clear();damageRows.Clear();skillsScroll=null;slider=null;
            v.Body.GetComponent<VerticalLayoutGroup>().enabled=false;v.Body.GetComponent<ContentSizeFitter>().enabled=false;
            Header(v);
            if(selected==null)RenderList(v);else RenderDetail(v);
        }
        void Header(ContentWindowView v)
        {
            float w=v.Width;bool portrait=PortraitLayout;
            v.Frame.GetComponent<UiWindowFrame>().header=54;
            var back=Button(v.Frame,"codex-back","‹",8,5,44,44,Back);((UiButton)back).Configure(UiButtonRole.Quiet);
            back.GetComponentInChildren<Text>().fontSize=UiTheme.Title;
            var closer=(RectTransform)v.Frame.Find("content-close");UiLayout.Place(closer,w-52,5,Touch,Touch);
            float tx=portrait?56:206,progressW=portrait?100:184;
            UiLayout.Place(v.Title.rectTransform,tx,5,Mathf.Max(130,w-52-progressW-tx-18),44);v.Title.fontSize=UiTheme.Title;
            if(!portrait)Text(v.Frame,"Codex wordmark","HELLSCRIPT",60,5,138,44,UiTheme.Heading,UiTheme.Gold,TextAnchor.MiddleLeft,UiFonts.Display);
            float px=w-64-progressW;int discovered=MonsterCodexPreview.Discovered(Account,MonsterRegistry.All),total=MonsterRegistry.All.Count;
            Text(v.Frame,"Codex progress label",Loc.T("발견한 적"),px,9,progressW,14,UiTheme.Caption,UiTheme.Muted);
            Text(v.Frame,"codex-progress",discovered+" / "+total,px,23,progressW,18,UiTheme.Body,UiTheme.GoldBright,TextAnchor.MiddleRight);
            var track=Box(v.Frame,"Progress track",px,45,progressW,3,UiTheme.Edge);
            Box(track,"Progress fill",0,0,total>0?progressW*discovered/total:0,3,UiTheme.Gold);
        }
        void RenderList(ContentWindowView v)
        {
            float w=v.Width-24,h=v.Frame.rect.height;UiLayout.Place(v.Navigation,12,58,w,Touch);
            UiLayout.Place(v.Scroll.viewport,12,110,w,h-122);v.Actions.gameObject.SetActive(false);v.Scroll.vertical=true;
            v.Frame.GetComponent<UiWindowFrame>().footer=0;
            Tabs(v,w);
            var items=MonsterCodexPreview.Entries(field).ToArray();int columns=PortraitLayout?4:8;float gap=8,cell=(w-gap*(columns-1))/columns,pic=cell-12,cardH=pic+64;
            UiLayout.Place(v.Body,0,0,w,Mathf.Max(1,Mathf.CeilToInt(items.Length/(float)columns)*(cardH+gap)-gap));
            for(int i=0;i<items.Length;i++)
            {
                var d=items[i];int count=MonsterCodex.Count(Account.monsterCodex,d.id);bool known=count>0;
                var card=Button(v.Body,"codex-card-"+d.id,"",i%columns*(cell+gap),i/columns*(cardH+gap),cell,cardH,()=>OpenDetail(d.id));
                card.interactable=known;((UiButton)card).Configure(UiButtonRole.Item,false,!known);
                var r=(RectTransform)card.transform;cards.Add(r);
                var frame=Panel(r,"Portrait frame",6,6,pic,pic,d.boss?UiTheme.GoldHex:UiTheme.BorderHex);
                if(known)Portrait(frame,d,2,2,pic-4);else Text(frame,"Undiscovered portrait","?",0,0,pic,pic,36,UiTheme.Faint,TextAnchor.MiddleCenter);
                if(d.boss){string label=Loc.T("보스");Chip(frame,"Boss ribbon",label,2,pic-20,LabelWidth(label,UiTheme.Caption)+6,18,UiTheme.Gold);}
                float idWidth=LabelWidth(d.id,UiTheme.Caption)+6;Chip(frame,"Codex id",d.id,pic-idWidth-2,pic-20,idWidth,18,UiTheme.Muted);
                Text(r,"Monster name",known?Loc.T(d.name):"???",4,pic+12,cell-8,32,UiTheme.Body,known?UiTheme.Text:UiTheme.Faint,TextAnchor.MiddleCenter);
                Text(r,"Kill count",known?Loc.F("처치 {0}",count.ToString("N0")):Loc.T("미발견"),4,pic+46,cell-8,14,UiTheme.Caption,UiTheme.Muted,TextAnchor.MiddleCenter);
            }
            v.Scroll.onValueChanged.AddListener(_=>{listOffset=v.Body.anchoredPosition.y;CullCards();});CullCards();
        }
        void CullCards()
        {
            if(selected!=null||view==null)return;float offset=view.Body.anchoredPosition.y,h=view.Scroll.viewport.rect.height;
            foreach(var r in cards){float y=-r.anchoredPosition.y;r.gameObject.SetActive(y+r.rect.height>=offset-r.rect.height&&y<=offset+h+r.rect.height);}
        }
        void Tabs(ContentWindowView v,float w)
        {
            v.Navigation.gameObject.AddComponent<Image>().color=Color.clear;v.Navigation.gameObject.AddComponent<RectMask2D>();
            var body=UiLayout.Rect("Codex tabs",v.Navigation);var scroll=v.Navigation.gameObject.AddComponent<ScrollRect>();scroll.viewport=v.Navigation;scroll.content=body;scroll.vertical=false;scroll.horizontal=true;scroll.movementType=ScrollRect.MovementType.Clamped;
            float x=0;foreach(int f in new[]{MonsterCodexPreview.All}.Concat(MonsterCodexPreview.Fields()).Concat(new[]{MonsterCodexPreview.Bosses}))
            {
                var entries=MonsterCodexPreview.Entries(f).ToArray();string label=(f==MonsterCodexPreview.All?Loc.T("전체"):f==MonsterCodexPreview.Bosses?Loc.T("보스"):FieldName(f))+"  "+MonsterCodexPreview.Discovered(Account,entries)+"/"+entries.Length;
                float width=Mathf.Max(76,LabelWidth(label,UiTheme.Body)+24);int filter=f;
                var b=v.Tab(body,label,x,0,width,Touch,()=>{field=filter;listOffset=0;v.Body.anchoredPosition=Vector2.zero;Repaint();},field==f);b.name="codex-tab-"+f;x+=width+6;
            }
            UiLayout.Place(body,0,0,Mathf.Max(w,x-6),Touch);
        }
        void OpenDetail(string id)
        {
            if(MonsterCodex.Count(Account.monsterCodex,id)<=0||MonsterRegistry.ById(id)==null)return;
            listOffset=view.Body.anchoredPosition.y;selected=id;elite=false;skillOffset=0;skillsScroll=null;Repaint();
        }
        LiveOpsRiftSettings Tuning()
        {
            string hash=game.LiveOps?.ConfigHash??"";
            if(tuningTier!=tier||tuningHash!=hash){tuningTier=tier;tuningHash=hash;tuning=game.LiveOps?.Capture(Mathf.Min(1000,tier))?.rift??new LiveOpsRiftSettings();}
            return tuning;
        }
        Button Button(Transform parent,string name,string label,float x,float y,float w,float h,Action action)
        {var b=view.Button(parent,label,x,y,Mathf.Max(Touch,w),Mathf.Max(Touch,h),action);b.name=name;return b;}
        static RectTransform Box(Transform parent,string name,float x,float y,float w,float h,Color? color=null)
        {var r=UiLayout.Rect(name,parent);UiLayout.Place(r,x,y,w,h);if(color.HasValue){var image=r.gameObject.AddComponent<Image>();image.color=color.Value;image.raycastTarget=false;}return r;}
        static RectTransform Panel(Transform parent,string name,float x,float y,float w,float h,string edge=null)
        {var r=Box(parent,name,x,y,w,h);var s=r.gameObject.AddComponent<StorageSurface>();s.Paint(UiTheme.PanelHex,UiTheme.BackgroundHex,edge??UiTheme.BorderHex);s.raycastTarget=false;return r;}
        static Text Text(Transform parent,string name,string value,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.UpperLeft,Font font=null)
        {var t=UiLayout.Text(parent,name,value,x,y,w,h,size,color,font);t.alignment=align;return t;}
        static float LabelWidth(string value,int size)
        {var settings=new TextGenerationSettings{font=UiFonts.Body,fontSize=size,fontStyle=FontStyle.Normal,scaleFactor=1,richText=true};return Mathf.Ceil(new TextGenerator().GetPreferredWidth(value,settings))+2;}
        static string FieldName(int f)=>Loc.T(GameCatalog.FieldNames[f]);
        static void Chip(Transform parent,string name,string value,float x,float y,float w,float h,Color color)
        {var r=Box(parent,name,x,y,w,h,UiTheme.Background);Text(r,name+" text",value,3,0,w-6,h,UiTheme.Caption,color,TextAnchor.MiddleCenter);}
        static void Portrait(Transform parent,MonsterDefinition d,float x,float y,float size)
        {
            var r=Box(parent,"Monster portrait",x,y,size,size);var picture=r.gameObject.AddComponent<RawImage>();picture.raycastTarget=false;
            string path=string.IsNullOrEmpty(d.portrait)?"Art/TrainingGround/Portraits/"+d.id:d.portrait;
            picture.texture=Resources.Load<Texture2D>(path);
            if(picture.texture==null){picture.texture=Resources.Load<Texture2D>("Art/GlobalHUD/portrait-placeholder");picture.color=UiTheme.Muted;}
            if(picture.texture!=null)
            {
                // Keep the entire face visible, including on tall portraits; do not crop or stretch the source.
                float scale=size/Mathf.Max(picture.texture.width,picture.texture.height);
                float width=picture.texture.width*scale,height=picture.texture.height*scale;
                UiLayout.Place(r,x+(size-width)/2,y,width,height);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // The monster codex (canvas "HELLSCRIPT 몬스터 도감 · 캐주얼"), drawn in the mockup's own CSS pixels (see MonsterCodexWindow.Look.cs): the grid of known
    // monsters and a detail page per monster with stats, defences, skills and the rift tier preview. The shared shell owns pause, back and locale; this adapter
    // keeps the selection, the tier and the scroll offsets across redraws.
    public sealed partial class MonsterCodexWindow:MonoBehaviour
    {
        GameController game;ContentWindowView view;Func<float> reading;int field=MonsterCodexPreview.All,tier;string selected;bool elite;
        float listOffset,skillOffset,tabOffset;ScrollRect listScroll,skillsScroll,tabScroll;SettingsStepSlider slider;
        int tuningTier=-1;string tuningHash;LiveOpsRiftSettings tuning;
        Text life,attack,speed,lifeNote,attackNote,tierValue,tierStatus;Button minus,plus;Action<float> syncFill;
        readonly List<(Text text,MonsterCodexSkill skill)> damageRows=new List<(Text,MonsterCodexSkill)>();
        MonsterDefinition Definition=>selected==null?null:MonsterRegistry.ById(selected);
        AccountSave Account=>game.Store.Data;
        // Editor tests pin the board size (the real one follows the screen); null in the game.
        static Func<Rect,(float scale,Rect plate)> fitOverride;
        public static (float scale,Rect plate) Fit(Rect safe)
        {
            if(fitOverride!=null)return fitOverride(safe);
            float s=UiMock.WindowScale(safe,956);return (s,new Rect(safe.x/s,safe.y/s,safe.width/s,safe.height/s));
        }
        public static ContentWindowView Open(Transform parent,GameController game,Func<float> reading=null,Action close=null)
        {
            MonsterCodexWindow owner=null;
            return ContentWindowView.Open(parent,"몬스터 도감",EquipmentViewSource.Catalog,reading??(()=>1),v=>
            {
                if(owner==null)
                {
                    owner=v.gameObject.AddComponent<MonsterCodexWindow>();owner.game=game;owner.view=v;owner.reading=reading??(()=>1);owner.tier=MonsterCodex.InitialTier(game.Store.Data);
                    StoreViewBinding.Attach(v,game.Store,owner.Repaint,()=>owner.slider==null||!owner.slider.Dragging,visibleState:owner.VisibleState);
                    MonsterRegistry.Changed+=owner.Repaint;
                }
                owner.Render(v);
            },close,back:()=>owner.Back(),skin:UiSkin.Casual,fit:Fit);
        }
        string VisibleState()=>string.Join("|",Account.Hero.id,Account.Hero.highestClear,StoreViewBinding.Key(Account.monsterCodex),game.LiveOps?.ConfigHash);
        void OnDestroy(){MonsterRegistry.Changed-=Repaint;}
        void Back(){if(selected==null){view.Close();return;}selected=null;elite=false;Repaint();}
        void Repaint(){if(view!=null)view.Repaint();}
        void Render(ContentWindowView v)
        {
            view=v;var frame=v.Frame;W=frame.rect.width;H=frame.rect.height;
            board=H>W?UiBoard.Portrait:UiMock.Of(UiSafeArea.Current);kit=new MockKit(frame,skin,reading);
            if(selected!=null&&(Definition==null||MonsterCodex.Count(Account.monsterCodex,selected)<=0))selected=null;
            tier=Mathf.Clamp(tier,1,MonsterCodex.MaximumTier(Account));damageRows.Clear();listScroll=skillsScroll=tabScroll=detailScroll=null;slider=null;syncFill=null;
            var backdrop=v.transform.Find("Backdrop");if(backdrop!=null)backdrop.GetComponent<Image>().color=UiTheme.MockSkyMid;
            DrawSky(v);
            DrawMasthead(frame,selected==null?null:Loc.T(Definition.name));
            if(selected==null)DrawList(frame);else DrawDetail(frame);
        }
        // The page's own ground covers the whole canvas, so notch and home-bar margins carry on the sky.
        void DrawSky(ContentWindowView v)
        {
            var f=v.Frame;float scale=Mathf.Max(.01f,v.GetComponent<Canvas>().scaleFactor),cw=Screen.width/scale,ch=Screen.height/scale;
            kit.Sky(f,-f.anchoredPosition.x,-(ch-f.anchoredPosition.y-H),cw,ch);
        }
        void OpenDetail(string id)
        {
            if(MonsterCodex.Count(Account.monsterCodex,id)<=0||MonsterRegistry.ById(id)==null)return;
            selected=id;elite=false;skillOffset=0;detailOffset=0;Repaint();
        }
        LiveOpsRiftSettings Tuning()
        {
            string hash=game.LiveOps?.ConfigHash??"";
            if(tuningTier!=tier||tuningHash!=hash){tuningTier=tier;tuningHash=hash;tuning=game.LiveOps?.Capture(Mathf.Min(1000,tier))?.rift??new LiveOpsRiftSettings();}
            return tuning;
        }
        static string FieldName(int f)=>Loc.T(GameCatalog.FieldNames[f]);
        static void Portrait(Transform parent,MonsterDefinition d,float x,float y,float size)
        {
            var r=UiLayout.Rect("Monster portrait",parent);UiLayout.Place(r,x,y,size,size);var picture=r.gameObject.AddComponent<RawImage>();picture.raycastTarget=false;
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

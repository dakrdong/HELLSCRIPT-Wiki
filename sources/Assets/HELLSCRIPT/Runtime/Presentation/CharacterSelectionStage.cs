using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Stage timing and movement live here, independent of saved gameplay and the title background.
    public sealed class CharacterSelectionStage:MonoBehaviour
    {
        RectTransform area;GameController game;CharacterSelectionState state;TitleSession session;
        CharacterFigure[] figures;RectTransform[] shadows;float[] emphasis;Vector2 lastSize;int revision=-1;
        float time,selectionAge;SelectionRune rune;
        public float Clock=>time;
        public bool Ready=>state.Selected>=0&&revision==state.Revision&&selectionAge>=1.05f;
        public CharacterFigure[] Figures=>figures;
        public System.Action<int> Choose;
        public void Initialize(GameController owner,CharacterSelectionState choice,TitleSession account)
        {
            game=owner;state=choice;session=account;area=(RectTransform)transform;
            var ring=new GameObject("Selected hero sigil",typeof(RectTransform));ring.transform.SetParent(transform,false);rune=ring.AddComponent<SelectionRune>();rune.raycastTarget=false;
            figures=new CharacterFigure[game.Store.Data.heroes.Count];emphasis=new float[figures.Length];shadows=new RectTransform[figures.Length];
            for(int i=0;i<figures.Length;i++)
            {
                var shadow=new GameObject("Hero contact shadow "+i,typeof(RectTransform));shadow.transform.SetParent(transform,false);
                var graphic=shadow.AddComponent<SelectionGroundShadow>();graphic.raycastTarget=false;graphic.color=new Color(0,0,0,.55f);
                shadows[i]=(RectTransform)shadow.transform;shadows[i].anchorMin=shadows[i].anchorMax=Vector2.zero;
            }
            for(int i=0;i<figures.Length;i++)
            {
                int index=i;var obj=new GameObject("character-actor-"+i,typeof(RectTransform));obj.transform.SetParent(transform,false);
                var f=obj.AddComponent<CharacterFigure>();f.Load(game.Store.Data.heroes[i].heroClass);
                f.Clicked=()=>Choose?.Invoke(index);f.Available=CharacterSelectionState.CanChoose(game.Store.Data,i);figures[i]=f;
                f.rectTransform.anchorMin=f.rectTransform.anchorMax=Vector2.zero;
            }
            Tick(0,true);
        }
        void Update()=>Tick(Mathf.Min(Time.unscaledDeltaTime,.1f),false);
        void Tick(float dt,bool snap)
        {
            if(figures==null)return;
            if(session.MotionEnabled)time+=dt;
            if(revision!=state.Revision){revision=state.Revision;selectionAge=0;}
            selectionAge+=dt;
            var size=area.rect.size;bool portrait=size.y>size.x*.86f;
            bool resize=size!=lastSize;lastSize=size;
            for(int i=0;i<figures.Length;i++)
            {
                bool selected=state.Selected==i;float target=selected?1:0;
                emphasis[i]=snap||resize?target:Mathf.MoveTowards(emphasis[i],target,dt*1.7f);
                var f=figures[i];float focus=emphasis[i];
                float homeX=figures.Length==1?.5f:.19f+i*.62f/(figures.Length-1);
                float slot=homeX;
                if(state.Selected>=0&&!selected)
                {
                    int rank=i<state.Selected?i:i-1;slot=rank==0?.13f:.87f;
                }
                float x=Mathf.Lerp(slot,.5f,focus);
                float baseHeight=portrait?Mathf.Min(size.y*.57f,size.x*.63f):Mathf.Min(size.y*.85f,size.x*.29f);
                if(state.Selected>=0)baseHeight*=.77f;
                float fullHeight=portrait?Mathf.Min(size.y*.9f,size.x*1.12f):size.y*.95f;
                float bodyHeight=Mathf.Lerp(baseHeight,fullHeight,focus);
                Vector2 at=new Vector2(size.x*x,size.y*Mathf.Lerp(.2f,.07f,focus));
                if(snap||resize)f.rectTransform.anchoredPosition=at;
                else f.rectTransform.anchoredPosition=Vector2.Lerp(f.rectTransform.anchoredPosition,at,1-Mathf.Exp(-dt*9));
                float breathe=session.MotionEnabled?Mathf.Sin(time*(1.3f+i*.17f)+i)*.003f:0;
                f.rectTransform.localScale=new Vector3(1,1+breathe,1);
                int frame;
                if(selected)
                    frame=selectionAge<.35f?3:selectionAge<.7f?4:session.MotionEnabled?4+((int)((time+i)*1.3f)%2):5;
                else
                {
                    float phase=session.MotionEnabled?(time*(.52f+i*.035f)+i*.9f)%4.8f:0;
                    frame=phase<1.8f?0:phase<2.6f?1:phase<3.4f?2:phase<4.2f?1:0;
                }
                f.SetFrame(frame);
                var cell=f.Atlas.frames[frame];float pixelsToStage=bodyHeight/f.Atlas.referenceHeight;
                f.rectTransform.sizeDelta=new Vector2(cell.pixelsWide*pixelsToStage,cell.pixelsHigh*pixelsToStage);
                shadows[i].anchoredPosition=f.rectTransform.anchoredPosition;
                shadows[i].sizeDelta=new Vector2(bodyHeight*.5f,bodyHeight*.085f);
                float light=state.Selected<0?1:Mathf.Lerp(.43f,1,focus);
                if(f.Hovered&&f.Available)light=Mathf.Max(light,.9f);
                if(!f.Available)light*=.42f;
                f.color=new Color(light,light,light,1);
            }
            if(state.Selected>=0)figures[state.Selected].transform.SetAsLastSibling();
            rune.gameObject.SetActive(state.Selected>=0);
            if(state.Selected>=0)
            {
                var f=figures[state.Selected];var r=rune.rectTransform;r.anchorMin=r.anchorMax=Vector2.zero;
                r.anchoredPosition=f.rectTransform.anchoredPosition;r.sizeDelta=new Vector2(Mathf.Min(size.x*.52f,300),45);
                rune.color=CharacterSelectionView.ClassColor(game.Store.Data.heroes[state.Selected].heroClass);rune.Phase=time;
            }
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SelectionGroundShadow:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;vh.AddVert(Vector3.zero,color,Vector2.zero);var edge=color;edge.a=0;
            for(int n=0;n<=48;n++)
            {float a=n*Mathf.PI*2/48;vh.AddVert(new Vector3(Mathf.Cos(a)*r.width*.5f,Mathf.Sin(a)*r.height*.5f),edge,Vector2.zero);}
            for(int n=0;n<48;n++)vh.AddTriangle(0,n+1,n+2);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SelectionRune:MaskableGraphic
    {
        float phase;public float Phase {set{if(phase==value)return;phase=value;SetVerticesDirty();}}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            for(int n=0;n<80;n++)
            {
                float a=n*Mathf.PI*2/80,b=(n+1)*Mathf.PI*2/80;int start=vh.currentVertCount;
                Color tint=color;tint.a=.48f+.14f*Mathf.Sin(phase*1.5f+n*.3f);
                float outer=n%5==0?1:.93f,inner=.88f;
                vh.AddVert(new Vector3(Mathf.Cos(a)*r.width*.5f*inner,Mathf.Sin(a)*r.height*.5f*inner),tint,Vector2.zero);
                vh.AddVert(new Vector3(Mathf.Cos(a)*r.width*.5f*outer,Mathf.Sin(a)*r.height*.5f*outer),tint,Vector2.zero);
                vh.AddVert(new Vector3(Mathf.Cos(b)*r.width*.5f*outer,Mathf.Sin(b)*r.height*.5f*outer),tint,Vector2.zero);
                vh.AddVert(new Vector3(Mathf.Cos(b)*r.width*.5f*inner,Mathf.Sin(b)*r.height*.5f*inner),tint,Vector2.zero);
                vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
            }
        }
    }
}

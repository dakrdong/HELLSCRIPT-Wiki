using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    [Serializable] public sealed class CharacterAtlasFrame
    {
        public float x,y,width,height,pixelsWide,pixelsHigh;
        public float footX=.5f,footY=.86f;
        public string hitMask;
        [NonSerialized] public byte[] mask;
    }
    [Serializable] public sealed class CharacterAtlasData
    {
        public int columns=2,rows=3;
        public float referenceHeight;
        public CharacterAtlasFrame[] frames;
    }
    // Alpha-derived hit masks are baked once; the shipped texture stays GPU-only.
    public sealed class CharacterFigure:RawImage,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
    {
        public CharacterAtlasData Atlas {get;private set;}
        public int Frame {get;private set;}=-1;
        public bool Hovered {get;private set;}
        public bool Available=true;
        public Action Clicked;
        public void Load(HeroClass heroClass)
        {
            string path="Art/CharacterSelection/"+heroClass;
            texture=Resources.Load<Texture2D>(path);
            var json=Resources.Load<TextAsset>(path+".atlas");
            if(texture==null||json==null)throw new InvalidOperationException("Missing character selection art: "+path);
            Atlas=JsonUtility.FromJson<CharacterAtlasData>(json.text);
            foreach(var frame in Atlas.frames)frame.mask=Convert.FromBase64String(frame.hitMask);
            SetFrame(0);
        }
        public void SetFrame(int frame)
        {
            if(Frame==frame)return;Frame=frame;
            var cell=Atlas.frames[frame];
            uvRect=new Rect(cell.x,cell.y,cell.width,cell.height);
            rectTransform.pivot=new Vector2(Atlas.frames[frame].footX,1-Atlas.frames[frame].footY);
        }
        public override bool Raycast(Vector2 screen,Camera camera)
        {
            if(!Available||!base.Raycast(screen,camera)||Atlas==null)return false;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screen,camera,out var local))return false;
            var rect=rectTransform.rect;float x=(local.x-rect.xMin)/rect.width,y=1-(local.y-rect.yMin)/rect.height;
            if(x<0||x>=1||y<0||y>=1)return false;
            int bit=Mathf.Min(63,(int)(x*64))+Mathf.Min(63,(int)(y*64))*64;
            return (Atlas.frames[Frame].mask[bit/8]&(1<<(bit%8)))!=0;
        }
        public void OnPointerClick(PointerEventData data){if(Available&&data.button==PointerEventData.InputButton.Left)Clicked?.Invoke();}
        public void OnPointerEnter(PointerEventData data)=>Hovered=true;
        public void OnPointerExit(PointerEventData data)=>Hovered=false;
    }
}

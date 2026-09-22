using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed class CharacterSelectionView:MonoBehaviour
    {
        GameController game;Font font;TitleSession session;TitleAtmosphere atmosphere;RectTransform root,tools,identity,stageArea,choices,details;
        CharacterSelectionState state;CanvasGroup group;Button back,motion,language,start;Button[] classButtons;
        Text title,subtitle,account,heroName,description,hint;Vector2 lastSize;float scale;bool ready;
        static readonly Color Bone=new Color(.91f,.865f,.75f),Gold=new Color(.65f,.49f,.3f),Muted=new Color(.67f,.69f,.69f);
        public CharacterSelectionStage Stage {get;private set;}
        public CharacterSelectionState Selection=>state;
        public Button StartButton=>start;
        public bool BackdropReady=>atmosphere.Ready;
        public static Color ClassColor(HeroClass type)=>type==HeroClass.Warrior?new Color(.9f,.53f,.22f):type==HeroClass.Mage?new Color(.39f,.72f,1):new Color(.44f,.8f,.57f);
        public void Initialize(GameController owner,Font bodyFont,CharacterSelectionState selection,TitleSession login,TitleAtmosphere backdrop)
        {
            game=owner;font=bodyFont;state=selection;session=login;atmosphere=backdrop;root=(RectTransform)transform;group=gameObject.AddComponent<CanvasGroup>();
            stageArea=Rect("Hero stage",root);Stage=stageArea.gameObject.AddComponent<CharacterSelectionStage>();Stage.Choose=Choose;Stage.Initialize(game,state,session);
            identity=Rect("Selection identity",root);
            title=Caption(identity,"selection-heading","운명을 선택하라",29,Bone);
            subtitle=Caption(identity,"selection-subtitle","성소의 부름에 응할 자는 누구인가.",14,Muted);
            tools=Rect("Selection tools",root);
            back=Button(tools,"characters-back","‹ 타이틀",Back,14);
            motion=Button(tools,"characters-motion",session.MotionEnabled?"연출 켜짐":"연출 멈춤",()=>{session.MotionEnabled=!session.MotionEnabled;Refresh();},12);
            language=Button(tools,"characters-language",LanguageOptions.Find(Loc.Language).NativeName,()=>game.ApplyLanguage(Loc.Language=="ko"?"en":"ko"),12);
            account=Caption(root,"selection-account",Loc.F("{0}   ·   {1}",session.DisplayName,TitleSession.ServerNames[session.Server]),12,Muted);
            choices=Rect("Character choices",root);classButtons=new Button[game.Store.Data.heroes.Count];
            for(int i=0;i<classButtons.Length;i++)
            {
                int index=i;var hero=game.Store.Data.heroes[i];
                classButtons[i]=Button(choices,"character-choice-"+i,Loc.F("{0}  ·  Lv.{1}",game.catalog.classNames[(int)hero.heroClass],hero.level),()=>Choose(index),16);
                classButtons[i].interactable=CharacterSelectionState.CanChoose(game.Store.Data,i);
            }
            details=Rect("Selected hero details",root);
            heroName=Caption(details,"selected-hero-name","",26,Bone);
            description=Caption(details,"selected-hero-description","",14,Muted);
            start=Button(details,"character-start","시작하기",StartGame,22,true);
            hint=Caption(details,"selection-hint","캐릭터를 누르면 전투 태세를 취합니다.",13,Muted);
            Refresh();Reflow();
        }
        public Button Find(string id)
        {foreach(var b in GetComponentsInChildren<Button>())if(b.name==id)return b;return null;}
        public void Choose(int index)
        {
            if(session.Entering||game.UI.CommonPanelOpen||!state.Choose(game.Store.Data,index))return;
            game.Audio?.Play(SoundCue.Select);ready=false;Refresh();
        }
        void Back(){if(session.Entering)return;game.UI.ShowTitle();}
        void Refresh()
        {
            motion.GetComponentInChildren<Text>().text=Loc.T(session.MotionEnabled?"연출 켜짐":"연출 멈춤");
            bool selected=CharacterSelectionState.CanChoose(game.Store.Data,state.Selected);
            heroName.gameObject.SetActive(selected);description.gameObject.SetActive(selected);
            start.gameObject.SetActive(selected&&Stage.Ready);start.interactable=selected&&Stage.Ready&&!session.Entering;
            for(int i=0;i<classButtons.Length;i++)
                classButtons[i].GetComponent<Image>().color=state.Selected==i?new Color(.24f,.11f,.065f,.97f):new Color(.035f,.039f,.045f,.9f);
            if(selected)
            {
                var h=game.Store.Data.heroes[state.Selected];heroName.text=Loc.F("{0}  ·  Lv.{1}",game.catalog.classNames[(int)h.heroClass],h.level);
                heroName.color=ClassColor(h.heroClass);
                description.text=Loc.T(h.heroClass==HeroClass.Warrior?"강철의 의지 · 검으로 길을 여는 전사":h.heroClass==HeroClass.Mage?"금지된 지식 · 원소를 지배하는 마법사":"침묵의 추적자 · 어둠을 꿰뚫는 궁수");
            }
            hint.text=Loc.T(session.Entering?"성소로 향하는 중…":CharacterSelectionState.LockedHero(game.Store.Data)!=-1?"진행 중인 균열의 캐릭터로 이어갑니다.":!selected?"캐릭터를 누르면 전투 태세를 취합니다.":Stage.Ready?"선택한 캐릭터로 성소에 입장합니다.":"전투 태세를 갖추는 중…");
        }
        void StartGame()
        {
            if(!Stage.Ready||session.Entering||!session.SignedIn||!game.CommitEntryCharacter(state.Selected))return;
            if(!session.BeginEntry())return;Refresh();StartCoroutine(Enter());
        }
        IEnumerator Enter()
        {
            group.interactable=false;foreach(var f in Stage.Figures)f.Available=false;
            float elapsed=0;while(elapsed<.65f){elapsed+=Time.unscaledDeltaTime;float t=Mathf.SmoothStep(0,1,elapsed/.65f);group.alpha=1-t;atmosphere.SetEntryFade(t);yield return null;}
            game.EnterPlaza(true);
        }
        void Update()
        {
            Reflow();if(ready!=Stage.Ready){ready=Stage.Ready;Refresh();}
            if(session.Entering||game.UI.CommonPanelOpen)return;
            var keyboard=Keyboard.current;if(keyboard==null)return;
            if(keyboard.escapeKey.wasPressedThisFrame){Back();return;}
            int direction=keyboard.rightArrowKey.wasPressedThisFrame?1:keyboard.leftArrowKey.wasPressedThisFrame?-1:0;
            if(direction!=0)
                for(int step=1;step<=classButtons.Length;step++)
                {int next=(Mathf.Max(0,state.Selected)+direction*step+classButtons.Length*2)%classButtons.Length;if(CharacterSelectionState.CanChoose(game.Store.Data,next)){Choose(next);break;}}
            if(keyboard.enterKey.wasPressedThisFrame&&EventSystem.current?.currentSelectedGameObject==null)StartGame();
        }
        void Reflow()
        {
            var size=root.rect.size;if(size==lastSize)return;lastSize=size;
            bool portrait=size.y>size.x;scale=Mathf.Clamp(Mathf.Min(size.x/(portrait?440:1280),size.y/(portrait?860:760)),1,1.65f);
            float w=size.x/scale,h=size.y/scale;bool compact=!portrait&&h<570;
            Fit(tools,16,10,w-32,44);Put((RectTransform)back.transform,0,0,100,44);
            Put((RectTransform)motion.transform,w-32-204,0,100,44);Put((RectTransform)language.transform,w-32-96,0,96,44);
            Fit(identity,20,compact?60:70,w-40,70);Put(title.rectTransform,0,0,w-40,37);Put(subtitle.rectTransform,0,39,w-40,26);
            subtitle.gameObject.SetActive(!compact);
            float detailsH=compact?106:174,choiceH=48,choiceY=h-detailsH-choiceH-14;
            float stageTop=compact?99:148,stageBottom=choiceY-5;
            Put(stageArea,0,stageTop*scale,size.x,Mathf.Max(100,(stageBottom-stageTop)*scale));
            float choiceW=Mathf.Min(w-32,780);Fit(choices,(w-choiceW)/2,choiceY,choiceW,choiceH);
            float cell=(choiceW-12*(classButtons.Length-1))/classButtons.Length;
            for(int i=0;i<classButtons.Length;i++)Put((RectTransform)classButtons[i].transform,i*(cell+12),0,cell,choiceH);
            Fit(details,16,h-detailsH,w-32,detailsH);
            float dw=w-32;
            Put(heroName.rectTransform,0,4,dw,36);Put(description.rectTransform,0,39,dw,28);
            Put((RectTransform)start.transform,(dw-Mathf.Min(420,dw))/2,76,Mathf.Min(420,dw),58);
            Put(hint.rectTransform,0,detailsH-30,dw,26);
            if(compact)
            {
                float bw=Mathf.Min(280,dw*.4f);
                Put(heroName.rectTransform,0,4,dw-bw-16,34);Put(description.rectTransform,0,39,dw-bw-16,26);
                Put((RectTransform)start.transform,dw-bw,5,bw,58);
            }
            account.gameObject.SetActive(!portrait&&!compact);Fit(account.rectTransform,180,14,Mathf.Max(160,w-430),34);
        }
        void Fit(RectTransform r,float x,float y,float w,float h){Put(r,x*scale,y*scale,w,h);r.localScale=Vector3.one*scale;}
        static RectTransform Rect(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return (RectTransform)go.transform;}
        static void Put(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        Text Caption(Transform parent,string name,string value,int size,Color color)
        {
            var r=Rect(name,parent);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=Loc.T(value);t.fontSize=size;t.color=color;t.supportRichText=false;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;
        }
        Button Button(Transform parent,string name,string label,Action action,int size,bool primary=false)
        {
            var r=Rect(name,parent);var image=r.gameObject.AddComponent<Image>();image.color=primary?new Color(.36f,.064f,.045f,.98f):new Color(.035f,.039f,.045f,.9f);
            var border=Rect("Etched frame",r);border.anchorMin=Vector2.zero;border.anchorMax=Vector2.one;border.offsetMin=border.offsetMax=Vector2.zero;
            var frame=border.gameObject.AddComponent<TitleFrameGraphic>();frame.color=Gold;frame.raycastTarget=false;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;var colors=b.colors;colors.highlightedColor=new Color(1.4f,1.3f,1.15f);colors.selectedColor=colors.highlightedColor;colors.disabledColor=new Color(.4f,.4f,.4f,.7f);b.colors=colors;
            var t=Caption(r,"Caption",label,size,Bone);t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.offsetMin=new Vector2(8,3);t.rectTransform.offsetMax=new Vector2(-8,-3);
            b.onClick.AddListener(()=>{game.Audio?.Play(SoundCue.Select);action();});return b;
        }
    }
}

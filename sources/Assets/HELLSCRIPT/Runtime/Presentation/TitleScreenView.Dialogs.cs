using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class TitleScreenView
    {
        RectTransform dialog,dialogPanel,dialogScroll,dialogBody;Text dialogHeading,loginError;
        InputField accountInput,passwordInput;Button dialogClose;float dialogHeight,dialogWidth,lastKeyboardHeight;
        void OpenDialog(string kind,string heading,float contentHeight)
        {
            CloseDialog();DialogKind=kind;
            dialog=Plate("title-modal",root,new Color(.008f,.009f,.012f,.83f),false);Fill(dialog);
            dialogPanel=Plate("Title dialog",dialog,new Color(.038f,.037f,.038f,.99f));
            dialogHeading=Caption(dialogPanel,"dialog-heading",heading,24,Bone,TextAnchor.MiddleLeft);
            dialogClose=ActionButton(dialogPanel,"title-dialog-close","닫기",CloseDialog,false,13);
            dialogScroll=Rect("Dialog viewport",dialogPanel);dialogScroll.gameObject.AddComponent<Image>().color=Color.clear;dialogScroll.gameObject.AddComponent<RectMask2D>();
            var scroll=dialogScroll.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            dialogBody=Rect("Dialog content",dialogScroll);dialogBody.anchorMin=new Vector2(0,1);dialogBody.anchorMax=new Vector2(1,1);dialogBody.pivot=new Vector2(.5f,1);dialogBody.sizeDelta=new Vector2(0,contentHeight);
            scroll.content=dialogBody;scroll.viewport=dialogScroll;scroll.scrollSensitivity=25;
            dialogHeight=contentHeight+84;dialogWidth=Mathf.Min(480,root.rect.width/scale-28);
            homeGroup.interactable=false;ReflowDialog();
        }
        void ReflowDialog()
        {
            if(dialog==null)return;
            float keyboard=TouchScreenKeyboard.visible?TouchScreenKeyboard.area.height/root.GetComponentInParent<Canvas>().scaleFactor:0;
            lastKeyboardHeight=keyboard;
            float available=Mathf.Max(180,root.rect.height-keyboard),panelHeight=Mathf.Min(dialogHeight,available/scale-24);
            // Content stretches when rotating; the current input controls and their values are preserved.
            dialogWidth=Mathf.Min(480,root.rect.width/scale-28);
            Put(dialogPanel,(root.rect.width-dialogWidth*scale)/2,(available-panelHeight*scale)/2,dialogWidth,panelHeight);dialogPanel.localScale=Vector3.one*scale;
            Put(dialogHeading.rectTransform,20,12,dialogWidth-102,46);Put((RectTransform)dialogClose.transform,dialogWidth-72,12,56,44);
            Put(dialogScroll,20,70,dialogWidth-40,panelHeight-84);
        }
        public void CloseDialog()
        {
            if(accountInput!=null){accountInput.DeactivateInputField();accountInput.text="";}
            if(passwordInput!=null){passwordInput.DeactivateInputField();passwordInput.text="";}
            accountInput=passwordInput=null;
            if(dialog!=null){dialog.gameObject.SetActive(false);Destroy(dialog.gameObject);}
            dialog=null;DialogKind="";
            if(homeGroup!=null)homeGroup.interactable=!Session.Entering;
            if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(null);
        }
        void BodyRect(RectTransform r,float y,float height,float left=0,float right=0)
        {
            r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(.5f,1);
            r.anchoredPosition=new Vector2((left-right)*.5f,-y);r.sizeDelta=new Vector2(-left-right,height);
        }
        public void ShowLogin()
        {
            OpenDialog("login","계정 로그인",366);
            var note=Caption(dialogBody,"login-note","로그인 체험입니다. 실제 계정 정보는 입력하지 마세요.",13,Muted,TextAnchor.MiddleLeft);BodyRect(note.rectTransform,0,46);
            var nameLabel=Caption(dialogBody,"account-label","계정 이름",13,Gold,TextAnchor.MiddleLeft);BodyRect(nameLabel.rectTransform,54,22);
            accountInput=Field("title-account","방랑자의 이름을 입력하세요",false);BodyRect((RectTransform)accountInput.transform,80,48);
            var passwordLabel=Caption(dialogBody,"password-label","비밀번호",13,Gold,TextAnchor.MiddleLeft);BodyRect(passwordLabel.rectTransform,137,22);
            passwordInput=Field("title-password","체험용 문자를 입력하세요",true);BodyRect((RectTransform)passwordInput.transform,164,48);
            loginError=Caption(dialogBody,"login-error","",12,new Color(.96f,.53f,.4f),TextAnchor.MiddleLeft);BodyRect(loginError.rectTransform,219,30);
            var submit=ActionButton(dialogBody,"title-login-submit","로그인",()=>
            {
                if(!Session.SignIn(accountInput.text,passwordInput.text)){loginError.text=Loc.T("계정 이름과 체험용 비밀번호를 입력하세요.");return;}
                CloseDialog();game.UI.OpenCharacterSelection();
            },true,18);BodyRect((RectTransform)submit.transform,257,54);
            var asGuest=ActionButton(dialogBody,"title-login-guest","게스트로 계속",()=>{Session.SignInAsGuest();CloseDialog();game.UI.OpenCharacterSelection();},false,15);BodyRect((RectTransform)asGuest.transform,321,44);
            var n=accountInput.navigation;n.mode=Navigation.Mode.Explicit;n.selectOnDown=passwordInput;accountInput.navigation=n;
            n=passwordInput.navigation;n.mode=Navigation.Mode.Explicit;n.selectOnUp=accountInput;n.selectOnDown=submit;passwordInput.navigation=n;
        }
        InputField Field(string id,string placeholder,bool password)
        {
            var r=Plate(id,dialogBody,new Color(.02f,.022f,.027f,1));
            var value=Caption(r,"Input value","",17,Bone,TextAnchor.MiddleLeft);Fill(value.rectTransform);value.rectTransform.offsetMin=new Vector2(12,4);value.rectTransform.offsetMax=new Vector2(-12,-4);
            var hint=Caption(r,"Input placeholder",placeholder,14,Muted,TextAnchor.MiddleLeft);Fill(hint.rectTransform);hint.rectTransform.offsetMin=new Vector2(12,4);hint.rectTransform.offsetMax=new Vector2(-12,-4);
            var input=r.gameObject.AddComponent<InputField>();input.targetGraphic=r.GetComponent<Image>();input.textComponent=value;input.placeholder=hint;
            input.lineType=InputField.LineType.SingleLine;input.characterLimit=password?64:24;input.contentType=password?InputField.ContentType.Password:InputField.ContentType.Standard;
            input.caretColor=Bone;input.customCaretColor=true;input.selectionColor=new Color(.6f,.25f,.1f,.4f);input.onValueChanged.AddListener(_=>{if(loginError!=null)loginError.text="";});return input;
        }
        public void ShowServers()
        {
            OpenDialog("servers","서버 선택",310);
            var note=Caption(dialogBody,"server-note","체험 서버 · 어느 서버를 골라도 현재 캐릭터로 시작합니다.",13,Muted,TextAnchor.MiddleLeft);BodyRect(note.rectTransform,0,50);
            for(int i=0;i<TitleSession.ServerNames.Length;i++)
            {
                int index=i;string name=Loc.F("{0}   ·   {1}\n{2}",TitleSession.ServerNames[i],TitleSession.ServerStates[i],Session.Server==i?"선택됨":"한국 · 체험 서버");
                var row=ActionButton(dialogBody,"title-server-"+i,name,()=>{if(Session.SelectServer(index)){CloseDialog();Refresh();}},Session.Server==i,16);
                BodyRect((RectTransform)row.transform,60+i*84,74);row.interactable=i!=2;
                if(i==2)row.GetComponentInChildren<Text>().color=new Color(.42f,.42f,.42f);
            }
        }
    }
}

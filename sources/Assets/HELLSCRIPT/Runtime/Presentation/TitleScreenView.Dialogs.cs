using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class TitleScreenView
    {
        RectTransform dialog,dialogPanel,dialogScroll,dialogBody;Text dialogHeading,loginError;
        Button dialogClose,googleContinue;float dialogHeight,dialogWidth,lastKeyboardHeight;bool googleAccountOpenAttempted;
        void OpenDialog(string kind,string heading,float contentHeight)
        {
            CloseDialog(false);DialogKind=kind;
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
        public void CloseDialog()=>CloseDialog(true);
        void CloseDialog(bool cancelLogin)
        {
            if(cancelLogin&&(DialogKind=="login"||DialogKind=="google-profile"))game.GoogleLogin.SignOut();
            googleContinue=null;loginError=null;
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
            float read=game.InterfaceScale.Factor;
            if(Session.SignedIn&&!Session.Guest)
            {
                OpenDialog("account","Google 계정",150*read);
                var identity=Caption(dialogBody,"google-account",Session.AccountName,Mathf.RoundToInt(15*read),Bone,TextAnchor.MiddleLeft);
                BodyRect(identity.rectTransform,0,85*read);
                var local=Caption(dialogBody,"account-storage-note","진행 상황은 현재 기기에 저장됩니다.",Mathf.RoundToInt(13*read),Muted,TextAnchor.MiddleLeft);
                BodyRect(local.rectTransform,90*read,60*read);return;
            }
            OpenDialog("login","계정 로그인",260*read);
            googleAccountOpenAttempted=false;
            loginError=Caption(dialogBody,"google-login-status",game.GoogleLogin.Message,Mathf.RoundToInt(15*read),Bone,TextAnchor.MiddleLeft);
            BodyRect(loginError.rectTransform,0,100*read);
            googleContinue=GoogleButton(dialogBody,"title-google-login","Google로 계속",()=>
            {if(game.GoogleLogin.Ready)googleAccountOpenAttempted=false;else game.GoogleLogin.Begin();},Mathf.RoundToInt(14*read),read);
            BodyRect((RectTransform)googleContinue.transform,110*read,56*read);
            var asGuest=ActionButton(dialogBody,"title-login-guest","게스트로 계속",()=>
            {if(!game.EnterAsGuest())loginError.text=game.Notice;},false,Mathf.RoundToInt(15*read));
            BodyRect((RectTransform)asGuest.transform,178*read,50*read);
            RefreshGoogleLogin();
        }
        void RefreshGoogleLogin()
        {
            if(DialogKind!="login")return;
            var auth=game.GoogleLogin;
            if(auth.Ready)
            {
                if(googleAccountOpenAttempted)return;googleAccountOpenAttempted=true;
                if(googleContinue!=null)googleContinue.interactable=true;
                try
                {
                    if(game.GoogleProfileExists){if(!game.CompleteGoogleLogin(false))loginError.text=game.Notice;return;}
                    ShowGoogleProfileChoice();return;
                }
                catch(System.Exception){loginError.text=Loc.T("계정 저장을 열지 못했습니다. 기존 저장은 보존했습니다. 다시 시도해 주세요.");return;}
            }
            if(loginError!=null)loginError.text=auth.Message;
            if(googleContinue!=null)googleContinue.interactable=!auth.Busy;
        }
        void ShowGoogleProfileChoice()
        {
            float read=game.InterfaceScale.Factor;
            OpenDialog("google-profile","진행 상황 선택",390*read);
            var name=Caption(dialogBody,"google-account",game.GoogleLogin.Session.displayName,Mathf.RoundToInt(15*read),Bone,TextAnchor.MiddleLeft);
            BodyRect(name.rectTransform,0,75*read);
            var note=Caption(dialogBody,"google-profile-note","이 기기의 게스트 진행 상황을 Google 계정에 연결하거나 새 게임을 시작할 수 있습니다. 진행 상황은 현재 기기에 저장됩니다.",Mathf.RoundToInt(14*read),Muted,TextAnchor.MiddleLeft);
            BodyRect(note.rectTransform,78*read,116*read);
            loginError=Caption(dialogBody,"google-profile-error","",Mathf.RoundToInt(12*read),Bone,TextAnchor.MiddleLeft);BodyRect(loginError.rectTransform,198*read,52*read);
            var link=ActionButton(dialogBody,"title-google-link-guest","게스트 진행 상황 연결",()=>
            {if(!game.CompleteGoogleLogin(true))loginError.text=game.Notice;},true,Mathf.RoundToInt(16*read));
            BodyRect((RectTransform)link.transform,258*read,54*read);link.interactable=game.CanLinkGuest;
            var fresh=ActionButton(dialogBody,"title-google-new-game","새 게임으로 시작",()=>
            {if(!game.CompleteGoogleLogin(false))loginError.text=game.Notice;},false,Mathf.RoundToInt(15*read));
            BodyRect((RectTransform)fresh.transform,324*read,50*read);
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

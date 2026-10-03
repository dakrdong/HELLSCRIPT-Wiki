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
            if(cancelLogin){game.GoogleLogin.Cancel();if(!Session.SignedIn&&(DialogKind=="login"||DialogKind=="signup"||DialogKind=="google-profile"))game.GoogleLogin.SignOut();}
            accountUsername=accountPassword=null;
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
            if(Session.SignedIn&&!Session.Guest)
            {
                OpenDialog("account","계정",310);
                var identity=Caption(dialogBody,"account-name",Session.AccountName,18,Bone,TextAnchor.MiddleLeft);BodyRect(identity.rectTransform,0,36);
                var uid=Caption(dialogBody,"account-uid",game.SupportUid,16,Bone,TextAnchor.MiddleLeft);BodyRect(uid.rectTransform,44,56);
                var copy=ActionButton(dialogBody,"title-copy-uid","문의 UID 복사",()=>WebPlayerAuthentication.CopyUid(game.SupportUid));BodyRect((RectTransform)copy.transform,106,48);
                var local=Caption(dialogBody,"account-storage-note","UID는 문의용 번호입니다. 로그인이나 비밀번호 복구 수단이 아닙니다. 캐릭터 진행은 현재 기기에 저장됩니다.",13,Muted,TextAnchor.MiddleLeft);BodyRect(local.rectTransform,165,118);return;
            }
            ShowCredentials(false);
        }
        public void ShowCredentials(bool register)
        {
            OpenDialog(register?"signup":"login",register?"계정 만들기":"계정 로그인",668);
            googleAccountOpenAttempted=false;
            var note=Caption(dialogBody,"account-entry-note",register?"게스트도 계정을 만듭니다. 가입하면 이 기기의 기존 게스트 캐릭터·진행·대기 로그를 그대로 이어갑니다.":"아이디·비밀번호로 로그인하세요. 세션은 12시간이며 게임을 다시 열면 로그인합니다.",14,Muted,TextAnchor.MiddleLeft);BodyRect(note.rectTransform,0,84);
            var user=Caption(dialogBody,"username-label","아이디",14,Bone,TextAnchor.MiddleLeft);BodyRect(user.rectTransform,92,24);
            accountUsername=AccountInput("account-username",false);BodyRect((RectTransform)accountUsername.transform,120,52);
            var pass=Caption(dialogBody,"password-label","비밀번호",14,Bone,TextAnchor.MiddleLeft);BodyRect(pass.rectTransform,180,24);
            accountPassword=AccountInput("account-password",true);BodyRect((RectTransform)accountPassword.transform,208,52);
            var limits=Caption(dialogBody,"credential-format","아이디는 영문·숫자·밑줄 3~24자, 비밀번호는 15~128자로 입력하세요.",12,Muted,TextAnchor.MiddleLeft);BodyRect(limits.rectTransform,267,48);
            var privacy=Caption(dialogBody,"account-privacy","계정 UID와 플레이 시간·진행·재화 소비·전투 기록을 게임 서버에 보냅니다. 외부 차트에는 익명 집계만 사용합니다. 이메일 복구는 없으니 아이디·비밀번호를 보관하거나 가입 후 Google을 연동하세요.",12,Muted,TextAnchor.MiddleLeft);BodyRect(privacy.rectTransform,322,108);
            loginError=Caption(dialogBody,"google-login-status",game.GoogleLogin.Message,13,Bone,TextAnchor.MiddleLeft);BodyRect(loginError.rectTransform,436,64);
            accountSubmit=ActionButton(dialogBody,"title-account-submit",register?"가입하고 이어 하기":"로그인",()=>{
                if(game.GoogleLogin.Ready){googleAccountOpenAttempted=false;return;}
                game.GoogleLogin.BeginCredentials(accountUsername.text,accountPassword.text,register);
            },true,16);BodyRect((RectTransform)accountSubmit.transform,505,50);
            var toggle=ActionButton(dialogBody,"title-account-toggle",register?"이미 계정이 있어요 · 로그인":"처음이에요 · 계정 만들기",()=>{if(!game.GoogleLogin.Busy)ShowCredentials(!register);},false,13);BodyRect((RectTransform)toggle.transform,562,48);
            googleContinue=GoogleButton(dialogBody,"title-google-login","Google로 계속",()=>
            {if(game.GoogleLogin.Ready)googleAccountOpenAttempted=false;else game.GoogleLogin.Begin();},14,1);BodyRect((RectTransform)googleContinue.transform,618,48);
            RefreshGoogleLogin();
        }
        void RefreshGoogleLogin()
        {
            if(DialogKind!="login"&&DialogKind!="signup")return;
            if(game.AccountOwnershipBusy){if(loginError!=null)loginError.text=game.Notice;if(accountSubmit!=null)accountSubmit.interactable=false;return;}
            var auth=game.GoogleLogin;
            if(auth.Ready)
            {
                if(accountSubmit!=null)accountSubmit.interactable=true;
                if(googleAccountOpenAttempted){if(loginError!=null&&!Session.SignedIn)loginError.text=game.Notice;return;}googleAccountOpenAttempted=true;
                if(googleContinue!=null)googleContinue.interactable=true;
                try
                {
                    if(game.GoogleProfileExists){if(!game.CompleteAccountLogin(false))loginError.text=game.Notice;return;}
                    if(DialogKind=="signup"){if(!game.CompleteAccountLogin(true))loginError.text=game.Notice;return;}
                    ShowGoogleProfileChoice();return;
                }
                catch(System.Exception){loginError.text=Loc.T("계정 저장을 열지 못했습니다. 기존 저장은 보존했습니다. 다시 시도해 주세요.");return;}
            }
            if(loginError!=null)loginError.text=auth.Message;
            if(googleContinue!=null)googleContinue.interactable=!auth.Busy;
            if(accountSubmit!=null)accountSubmit.interactable=!auth.Busy;
        }
        void ShowGoogleProfileChoice()
        {
            const float read=1f;
            OpenDialog("google-profile","진행 상황 선택",390*read);
            var name=Caption(dialogBody,"google-account",game.GoogleLogin.Session.displayName,Mathf.RoundToInt(15*read),Bone,TextAnchor.MiddleLeft);
            BodyRect(name.rectTransform,0,75*read);
            var note=Caption(dialogBody,"google-profile-note","이 기기의 기존 게스트 진행을 계정에 연결하거나 이 계정의 새 기기 저장으로 시작할 수 있습니다. 다른 계정의 저장은 합치거나 덮어쓰지 않습니다. 진행은 현재 기기에 저장됩니다.",Mathf.RoundToInt(14*read),Muted,TextAnchor.MiddleLeft);
            BodyRect(note.rectTransform,78*read,116*read);
            loginError=Caption(dialogBody,"google-profile-error","",Mathf.RoundToInt(12*read),Bone,TextAnchor.MiddleLeft);BodyRect(loginError.rectTransform,198*read,52*read);
            var link=ActionButton(dialogBody,"title-google-link-guest","기존 게스트 진행 이어 하기",()=>
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

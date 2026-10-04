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
            if(cancelLogin){game.GoogleLogin.Cancel();if(!Session.SignedIn&&(DialogKind=="login"||DialogKind=="guest"||DialogKind=="google-profile"))game.GoogleLogin.SignOut();}
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
            ShowGoogleAuthentication(false);
        }
        void ShowGuestProgress()
        {
            OpenDialog("guest","게스트로 시작",330);
            googleAccountOpenAttempted=false;
            var note=Caption(dialogBody,"guest-recovery-note","이 브라우저에서 같은 게스트 UID로 이어갑니다. 브라우저 데이터를 지우면 게스트 접근과 기기 저장을 잃을 수 있습니다. Google 연동으로 계정 UID를 보호하세요.",14,Muted,TextAnchor.MiddleLeft);BodyRect(note.rectTransform,0,122);
            var privacy=Caption(dialogBody,"account-privacy",GuestPrivacy,12,Muted,TextAnchor.MiddleLeft);BodyRect(privacy.rectTransform,130,88);
            loginError=Caption(dialogBody,"google-login-status",game.GoogleLogin.Message,13,Bone,TextAnchor.MiddleLeft);BodyRect(loginError.rectTransform,222,62);
            googleContinue=ActionButton(dialogBody,"title-guest-retry","다시 시도",()=>{googleAccountOpenAttempted=false;game.GoogleLogin.BeginGuest();});BodyRect((RectTransform)googleContinue.transform,290,44);
        }
        const string GuestPrivacy="계정 UID와 플레이 시간·진행·재화 소비·전투 기록을 게임 서버에 보냅니다. 외부 차트에는 익명 집계만 사용합니다. 캐릭터 진행은 현재 기기에 저장됩니다.";
        void ShowGoogleAuthentication(bool link)
        {
            OpenDialog(link?"link":"login","Google 연동",300);googleAccountOpenAttempted=false;
            var note=Caption(dialogBody,"account-entry-note",link?"Google 연동은 같은 계정 UID를 유지합니다. 다른 계정에 연결된 Google 계정은 합치거나 덮어쓰지 않습니다.":"Google 계정으로 로그인하거나 게스트로 시작하세요.",14,Muted,TextAnchor.MiddleLeft);BodyRect(note.rectTransform,0,84);
            var privacy=Caption(dialogBody,"account-privacy",GuestPrivacy,12,Muted,TextAnchor.MiddleLeft);BodyRect(privacy.rectTransform,92,86);
            loginError=Caption(dialogBody,"google-login-status",game.GoogleLogin.Message,13,Bone,TextAnchor.MiddleLeft);BodyRect(loginError.rectTransform,180,62);
            googleContinue=GoogleButton(dialogBody,"title-google-login","Google로 계속",()=>
            {if(link)game.PrepareGoogleLink();else game.GoogleLogin.Begin();},14,1);BodyRect((RectTransform)googleContinue.transform,250,48);
        }
        void RefreshGoogleLogin()
        {
            if(DialogKind!="login"&&DialogKind!="guest"&&DialogKind!="link")return;
            if(game.AccountOwnershipBusy){if(loginError!=null)loginError.text=game.Notice;return;}
            var auth=game.GoogleLogin;
            if(DialogKind=="link")
            {
                if(loginError!=null)loginError.text=auth.GoogleLinked?Loc.T("Google 연동 완료 · 같은 계정과 캐릭터를 사용합니다."):auth.Message;
                if(googleContinue!=null)googleContinue.interactable=!auth.Busy&&!auth.GoogleLinked&&auth.Ready;
                if(auth.GoogleLinked)Refresh();return;
            }
            if(auth.Ready)
            {
                if(googleAccountOpenAttempted){if(loginError!=null&&!Session.SignedIn)loginError.text=game.Notice;return;}googleAccountOpenAttempted=true;
                if(googleContinue!=null)googleContinue.interactable=true;
                try
                {
                    if(game.GoogleProfileExists){if(!game.CompleteAccountLogin(false))loginError.text=game.Notice;return;}
                    if(auth.GuestSession){if(!game.CompleteAccountLogin(game.CanLinkGuest))loginError.text=game.Notice;return;}
                    ShowGoogleProfileChoice();return;
                }
                catch(System.Exception){loginError.text=Loc.T("계정 저장을 열지 못했습니다. 기존 저장은 보존했습니다. 다시 시도해 주세요.");return;}
            }
            if(loginError!=null)loginError.text=auth.Message;
            if(googleContinue!=null)googleContinue.interactable=!auth.Busy;
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

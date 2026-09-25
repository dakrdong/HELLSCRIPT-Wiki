using System;

namespace Hellscript
{
    // Presentation state after the controller has verified identity and selected its local profile.
    public sealed class TitleSession
    {
        public static readonly string[] ServerNames={"잿빛 성소","검은 종탑","망각의 문"};
        public static readonly string[] ServerStates={"원활","혼잡","점검 중"};
        bool signedIn;long expiresAt;
        public bool SignedIn=>signedIn&&(Guest||expiresAt>DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        public bool Guest {get;private set;}
        public string AccountId {get;private set;}="";
        public string AccountName {get;private set;}="";
        public int Server {get;private set;}
        public bool Entering {get;private set;}
        public bool MotionEnabled=true;
        public string DisplayName=>Guest?Loc.T("방랑자"):AccountName;
        public bool SelectServer(int index)
        {
            if(Entering||index<0||index>=ServerNames.Length||index==2)return false;
            Server=index;return true;
        }
        public bool SignInWithGoogle(GooglePlayerSession session)
        {
            if(Entering||session==null||!session.Valid(DateTimeOffset.UtcNow.ToUnixTimeSeconds()))return false;
            AccountId=session.accountId;AccountName=session.displayName;expiresAt=session.expiresAt;
            Guest=false;signedIn=true;return true;
        }
        public void SignInAsGuest(){if(Entering)return;AccountId=AccountName="";expiresAt=0;Guest=true;signedIn=true;}
        public void SignOut(){if(Entering)return;AccountId=AccountName="";expiresAt=0;Guest=false;signedIn=false;}
        public bool BeginEntry(){if(!SignedIn||Entering)return false;Entering=true;return true;}
        public void CancelEntry()=>Entering=false;
    }
}

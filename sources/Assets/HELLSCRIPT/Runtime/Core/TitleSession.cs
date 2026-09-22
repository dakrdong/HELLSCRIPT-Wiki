using System;

namespace Hellscript
{
    // Presentation-only state. No network client, account save, password storage or server migration.
    public sealed class TitleSession
    {
        public static readonly string[] ServerNames={"잿빛 성소","검은 종탑","망각의 문"};
        public static readonly string[] ServerStates={"원활","혼잡","점검 중"};
        public bool SignedIn {get;private set;}
        public bool Guest {get;private set;}
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
        public bool SignIn(string name,string password)
        {
            if(Entering||string.IsNullOrWhiteSpace(name)||string.IsNullOrWhiteSpace(password))return false;
            name=name.Trim();
            if(name.Length>24||Array.Exists(name.ToCharArray(),char.IsControl))return false;
            AccountName=name;Guest=false;SignedIn=true;return true;
        }
        public void SignInAsGuest(){if(Entering)return;AccountName="";Guest=true;SignedIn=true;}
        public void SignOut(){if(Entering)return;AccountName="";Guest=false;SignedIn=false;}
        public bool BeginEntry(){if(!SignedIn||Entering)return false;Entering=true;return true;}
        public void CancelEntry()=>Entering=false;
    }
}

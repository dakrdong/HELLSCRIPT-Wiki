using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hellscript
{
    // Local directory ownership only. Google identity is verified by the server before binding.
    // Linking transfers a directory reference atomically; no save is copied, merged or deleted.
    public sealed class AccountProfiles
    {
        [Serializable] sealed class Entry { public string key,directory; }
        [Serializable] sealed class Index
        { public int version=1;public string guest="";public Entry[] accounts=Array.Empty<Entry>(); }
        readonly string root,path;
        public AccountProfiles(string root)
        {this.root=Path.GetFullPath(root);Directory.CreateDirectory(this.root);path=Path.Combine(this.root,"account-profiles-v1.json");Read();}
        static bool ValidDirectory(string value)=>value==""||value!=null&&Regex.IsMatch(value,@"\Aaccounts/(?:google-[a-f0-9]{64}|guest-[a-f0-9]{32})\z");
        string Full(string relative)=>relative==""?root:Path.Combine(root,relative.Replace('/',Path.DirectorySeparatorChar));
        static string Key(Uri server,string account)
        {
            if(server==null||!server.IsAbsoluteUri||server.AbsolutePath!="/"||!string.IsNullOrEmpty(server.Query)||
                !string.IsNullOrEmpty(server.Fragment)||!string.IsNullOrEmpty(server.UserInfo)||
                !Regex.IsMatch(account??"",@"\A[a-f0-9]{32}\z"))throw new ArgumentException("Invalid authenticated profile.");
            return CombatJournalArchive.Hash(server.AbsoluteUri+"\n"+account);
        }
        // Offline ownership alias only; this address is never contacted.
        static string PreviousKey(Uri server,string account)=>server.AbsoluteUri=="https://hellscript-player-logs.hoosung.chatgpt.site/"
            ?Key(new Uri("https://hellscript-production.up.railway.app/"),account):null;
        static Entry Find(Index index,string key,string previous)
        {
            var current=Array.Find(index.accounts,e=>e.key==key);
            var legacy=previous==null?null:Array.Find(index.accounts,e=>e.key==previous);
            if(current!=null&&legacy!=null)throw new InvalidDataException("Both server profiles exist. Original saves are preserved.");
            return current??legacy;
        }
        Index Read()
        {
            if(!File.Exists(path))return new Index();
            try
            {
                if(new FileInfo(path).Length>65536)throw new InvalidDataException();
                string json=File.ReadAllText(path);LiveOpsJson.Shape<Index>(json);
                var index=JsonUtility.FromJson<Index>(json);
                if(index==null||index.version!=1||!ValidDirectory(index.guest)||index.accounts==null||index.accounts.Length>128||
                    index.accounts.Any(e=>e==null||!Regex.IsMatch(e.key??"",@"\A[a-f0-9]{64}\z")||!ValidDirectory(e.directory))||
                    index.accounts.Select(e=>e.key).Distinct().Count()!=index.accounts.Length||
                    index.accounts.Select(e=>e.directory).Append(index.guest).Distinct().Count()!=index.accounts.Length+1)
                    throw new InvalidDataException();
                return index;
            }
            catch(Exception error) when(error is ArgumentException||error is IOException)
            {throw new InvalidDataException("Account profile ownership could not be read. Original files are preserved.",error);}
        }
        public string GuestDirectory=>Full(Read().guest);
        public string GoogleDirectory(Uri server,string account)
        {var key=Key(server,account);var entry=Find(Read(),key,PreviousKey(server,account));return entry==null?null:Full(entry.directory);}
        public string NewGoogleDirectory(Uri server,string account)=>Full("accounts/google-"+Key(server,account));
        public string BindGoogle(Uri server,string account,bool linkGuest,string expectedGuestDirectory)
        {
            using var gate=new FileStream(path+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            var index=Read();string key=Key(server,account);
            var existing=Find(index,key,PreviousKey(server,account));
            if(existing!=null)
            {
                if(linkGuest)throw new InvalidOperationException("This Google account already has a local profile.");
                if(existing.key!=key){existing.key=key;Write(index);}
                return Full(existing.directory);
            }
            if(index.accounts.Length>=128)throw new InvalidOperationException("Local profile limit reached.");
            if(linkGuest&&Full(index.guest)!=Path.GetFullPath(expectedGuestDirectory))
                throw new InvalidOperationException("The guest profile changed. Please reopen sign-in.");
            string destination=linkGuest?index.guest:"accounts/google-"+key;
            if(!File.Exists(Path.Combine(Full(destination),"hellscript-local-v1.json")))
                throw new InvalidOperationException("Save the target profile before binding it.");
            index.accounts=index.accounts.Append(new Entry{key=key,directory=destination}).ToArray();
            if(linkGuest)index.guest="accounts/guest-"+Guid.NewGuid().ToString("N");
            Write(index);
            return Full(destination);
        }
        void Write(Index index)
        {
            string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
            try
            {
                byte[] bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(index));
                using(var file=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None))
                {file.Write(bytes,0,bytes.Length);file.Flush(true);}
                if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
            }
            finally{if(File.Exists(temp))File.Delete(temp);}
        }
    }
}

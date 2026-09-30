using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hellscript
{
    // Public endpoint configuration contains no credentials. QA credentials remain outside game saves/assets.
    public static class GameServerConnection
    {
        [Serializable] public sealed class Settings { public int version;public string baseUrl,telemetryBaseUrl; }
        [Serializable] public sealed class QaSession
        { public int version;public string kind,baseUrl,accessToken; }
        sealed class TelemetrySession:ICombatTelemetrySession
        {
            public Uri Endpoint {get;}
            public string AccessToken {get;}
            public TelemetrySession(Uri endpoint,string token){Endpoint=endpoint;AccessToken=token;}
        }
        public sealed class PlayerTelemetrySession:ICombatTelemetrySession
        {
            readonly Func<GooglePlayerSession> read;
            public Uri Endpoint {get;}
            public string AccessToken
            {
                get{var value=read();return value!=null&&value.Valid(DateTimeOffset.UtcNow.ToUnixTimeSeconds())?value.accessToken:null;}
            }
            public PlayerTelemetrySession(Uri endpoint,Func<GooglePlayerSession> read)
            {Endpoint=endpoint;this.read=read??throw new ArgumentNullException(nameof(read));}
        }
        public static string Status {get;private set;}="not_configured";

        public static bool TryBaseUri(string value,bool development,out Uri uri)
        {
            uri=null;
            if(string.IsNullOrWhiteSpace(value)||!Uri.TryCreate(value,UriKind.Absolute,out var parsed)||
                !string.IsNullOrEmpty(parsed.UserInfo)||!string.IsNullOrEmpty(parsed.Query)||
                !string.IsNullOrEmpty(parsed.Fragment)||parsed.AbsolutePath!="/"||
                (parsed.Scheme!="https"&&!(development&&parsed.IsLoopback&&parsed.Scheme=="http")))return false;
            uri=parsed;return true;
        }
        public static bool TryServers(Settings settings,bool development,out Uri server,out Uri telemetry)
        {
            server=telemetry=null;
            if(settings==null||(settings.version!=1&&settings.version!=2)||!TryBaseUri(settings.baseUrl,development,out server))return false;
            if(settings.version==1){telemetry=server;return true;}
            return TryBaseUri(settings.telemetryBaseUrl,development,out telemetry);
        }
        static Settings ReadSettings()
        {
            var asset=Resources.Load<TextAsset>("Data/ServerConnection");
            if(asset==null)return null;
            try{return JsonUtility.FromJson<Settings>(asset.text);}
            catch(ArgumentException){return null;}
        }
        public static void ConfigurePlayerTelemetry(GameController game)
        {
            var settings=ReadSettings();
            if(game.GoogleLogin?.Ready!=true||settings?.version!=2||
                !TryServers(settings,Debug.isDebugBuild,out _,out var telemetry))return;
            game.Telemetry.Configure(new PlayerTelemetrySession(new Uri(telemetry,"v1/combat-runs"),()=>game.GoogleLogin.Session));
            Status="player_telemetry_connected";
        }
        public static bool ValidateQaSession(QaSession session,Uri expectedBase,bool development)
        {
            if(session==null||session.version!=1||session.kind!="hellscript-qa-telemetry"||expectedBase==null||
                !TryBaseUri(session.baseUrl,development,out var actual)||actual!=expectedBase)return false;
            return session.accessToken!=null&&Regex.IsMatch(session.accessToken,"\\A[A-Za-z0-9_-]{32,256}\\z");
        }

        public static void Configure(GameController game,string saveDirectory)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // This static-hosted guest edition uses the bundled game rules. Native OAuth
            // callbacks and the operator server's same-origin API are not browser endpoints.
            Status="web_local_guest";
            return;
#else
            Status="not_configured";
            var settings=ReadSettings();
            if(!TryServers(settings,Debug.isDebugBuild,out var server,out var telemetry))
            {Status="invalid_server_configuration";return;}
            game.LiveOps.Configure(new Uri(server,"v1/liveops/current"));
            game.GoogleLogin?.Configure(server);
            Status="liveops_connected";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // A tester receives a dedicated file from the operator; it is never shared in a game build.
            string sessionPath=Path.Combine(saveDirectory,"qa-session.json");
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-hellscriptQaSessionFile")sessionPath=args[i+1];
            if(!File.Exists(sessionPath))return;
            try
            {
                if(new FileInfo(sessionPath).Length>4096){Status="invalid_qa_session";return;}
                var session=JsonUtility.FromJson<QaSession>(File.ReadAllText(sessionPath));
                if(!ValidateQaSession(session,telemetry,Debug.isDebugBuild)){Status="invalid_qa_session";return;}
                game.Telemetry.Configure(new TelemetrySession(new Uri(telemetry,"v1/combat-runs"),session.accessToken));
                Status="qa_connected";
            }
            catch(Exception error) when(error is IOException||error is UnauthorizedAccessException||error is ArgumentException)
            {Status="qa_session_unavailable";}
#endif
#endif
        }
    }
}

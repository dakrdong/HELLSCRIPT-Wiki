#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Opt-in native upload proof; never uses the player's ordinary save.
    public sealed class RuntimeTelemetrySitesSmoke:MonoBehaviour
    {
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var args=Environment.GetCommandLineArgs();
            if(!Debug.isDebugBuild||!args.Contains("-hellscriptTelemetrySitesSmoke"))return;
            Application.runInBackground=true;
            Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};
            new GameObject("Sites telemetry verification").AddComponent<RuntimeTelemetrySitesSmoke>();
        }
        [Serializable] sealed class Evidence
        {
            public bool passed,synthetic=true,realNativeUploader=true,duplicateAcknowledged,outboxRemoved;
            public string runId,payloadHash,serverConnection,uploaderStatus;
            public int pending,localHistoryRecords;
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();
            Require(args.Contains("-hellscriptSavePath")&&args.Contains("-hellscriptQaSessionFile"),"Isolated save and dedicated QA session required.");
            int index=Array.IndexOf(args,"-hellscriptEvidencePath");
            Require(index>=0&&index+1<args.Length,"Evidence directory required.");
            string output=args[index+1];Directory.CreateDirectory(output);
            yield return null;
            var game=FindAnyObjectByType<GameController>();
            Require(game?.Store!=null&&GameServerConnection.Status=="qa_connected","Sites QA session was not configured.");
            Require(game.Telemetry.PendingCount==0&&game.Store.Data.records.Count==0,"A clean test save is required.");
            game.enabled=false;game.Store.Data.guide.legacyExempt=true;
            game.Begin(seed:9138);
            float deadline=Time.realtimeSinceStartup+30;
            while(!game.Active&&Time.realtimeSinceStartup<deadline)yield return null;
            Require(game.Active,"Native combat did not start.");
            game.Combat.State.paused=false;
            for(int tick=0;tick<6400&&game.Active;tick++)
            {
                game.Combat.Tick(CombatSimulation.Step);
                if(tick%200==0)yield return null;
            }
            if(game.Active)game.Combat.Abandon();
            game.Save();
            Require(game.Store.Data.records.Count==1,"Native combat did not produce exactly one history record.");
            var record=game.Store.Data.records.Single();
            string pending=Path.Combine(game.Store.CombatArchive.OutboxPath,record.id+".json");
            Require(File.Exists(pending),"No pending native combat journal.");
            string payload=File.ReadAllText(pending),hash=CombatJournalArchive.Hash(payload);
            File.WriteAllText(Path.Combine(output,"actual-run.json"),payload);
            deadline=Time.realtimeSinceStartup+60;
            while(game.Telemetry.PendingCount>0&&Time.realtimeSinceStartup<deadline)yield return null;
            Require(game.Telemetry.PendingCount==0&&game.Telemetry.Status=="delivered","First Sites ACK failed: "+game.Telemetry.Status);
            // Restart the actual uploader, then replay the exact derivative file.
            // The game save and earned rewards are never replayed.
            game.Telemetry.SwitchArchive(game.Store.CombatArchive);
            GameServerConnection.Configure(game,args[Array.IndexOf(args,"-hellscriptSavePath")+1]);
            File.WriteAllText(pending,payload);
            deadline=Time.realtimeSinceStartup+60;
            while(game.Telemetry.PendingCount>0&&Time.realtimeSinceStartup<deadline)yield return null;
            Require(game.Telemetry.PendingCount==0&&game.Telemetry.Status=="delivered","Duplicate Sites ACK failed: "+game.Telemetry.Status);
            Require(game.Store.ReadCombatRecord(record).journal.events.Count>0,"Uploading removed the player's local history.");
            var evidence=new Evidence{passed=true,duplicateAcknowledged=true,outboxRemoved=!File.Exists(pending),
                runId=record.id,payloadHash=hash,serverConnection=GameServerConnection.Status,uploaderStatus=game.Telemetry.Status,
                pending=game.Telemetry.PendingCount,localHistoryRecords=game.Store.Data.records.Count};
            File.WriteAllText(Path.Combine(output,"native-uploader.json"),JsonUtility.ToJson(evidence,true));
            Application.Quit(0);
        }
    }
}
#endif

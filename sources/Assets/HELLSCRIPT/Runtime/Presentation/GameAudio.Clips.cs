using UnityEngine;

namespace Hellscript
{
    // Imported music belongs to Resources. The observer owns only playback sources, never the assets.
    public sealed partial class GameAudio
    {
        public const string MusicPath = "Audio/Music/";
        public const float MusicFadeSeconds = 1.5f;
        static readonly string[] MusicIds = { "title", "sanctuary", "rift", "boss" };
        sealed class MusicVoice
        {
            public string id;
            public AudioSource source;
            public float gain;
            public bool started;
        }
        readonly MusicVoice[] musicVoices = new MusicVoice[4];
        GameController musicGame;
        public string MusicContext { get; private set; } = "sanctuary";
        public int MusicClipCount { get; private set; }
        public int MissingMusicClips { get; private set; }

        public static string MusicContextFor(RunPhase? phase, string page)
        {
            if (phase == RunPhase.Boss) return "boss";
            if (phase.HasValue && phase != RunPhase.Cleared && phase != RunPhase.Failed) return "rift";
            return page is "title" or "characters" ? "title" : "sanctuary";
        }

        void InitializeMusic()
        {
            musicGame = GetComponent<GameController>();
            MusicContext = musicGame != null ? "title" : "sanctuary";
            MusicClipCount = MissingMusicClips = 0;
            for (int i = 0; i < MusicIds.Length; i++)
            {
                string id = MusicIds[i];
                var source = Source(id + " music", true);
                source.clip = Resources.Load<AudioClip>(MusicPath + "music_" + id);
                source.volume = 0;
                if (source.clip == null) MissingMusicClips++; else MusicClipCount++;
                musicVoices[i] = new MusicVoice { id = id, source = source };
            }
            if (MissingMusicClips > 0) Debug.LogWarning("HELLSCRIPT music bank is missing " + MissingMusicClips + " clips");
            UpdateMusic(0);
        }

        void UpdateMusic(float delta)
        {
            if (background || PowerSaving) return;
            // Page and combat state are read from their owners; music never changes either.
            string page = musicGame != null && musicGame.UI != null ? musicGame.UI.Page : musicGame != null ? "title" : null;
            MusicContext = MusicContextFor(simulation != null ? simulation.State.phase : (RunPhase?)null, page);
            float step = Mathf.Max(0, delta) / MusicFadeSeconds;
            foreach (var voice in musicVoices)
            {
                if (voice?.source == null || voice.source.clip == null) continue;
                bool selected = voice.id == MusicContext;
                voice.gain = Mathf.MoveTowards(voice.gain, selected ? 1 : 0, step);
                if (selected)
                {
                    // Apply gain before starting so a transition cannot play one frame at full volume.
                    voice.source.volume = MusicLevel * voice.gain;
                    if (!voice.started) { voice.source.Play(); voice.started = true; }
                    else if (!voice.source.isPlaying) voice.source.UnPause();
                }
                else if (voice.gain == 0 && voice.source.isPlaying) voice.source.Pause();
            }
        }

        float MusicLevel => Preferences == null || Preferences.muted ? 0 : Preferences.master * Preferences.music;
        void ApplyMusicVolumes()
        {
            foreach (var voice in musicVoices)
                if (voice?.source != null) voice.source.volume = MusicLevel * voice.gain;
        }
        void PauseMusic()
        {
            foreach (var voice in musicVoices) if (voice?.source != null) voice.source.Pause();
        }
        void ResumeMusic()
        {
            if (background || PowerSaving) return;
            foreach (var voice in musicVoices)
                if (voice?.source != null && voice.started && (voice.gain > 0 || voice.id == MusicContext)) voice.source.UnPause();
        }
        void ReleaseMusic()
        {
            foreach (var voice in musicVoices)
                if (voice?.source != null) { voice.source.Stop(); voice.source.clip = null; }
        }
    }
}

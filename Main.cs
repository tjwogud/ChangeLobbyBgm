using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ChangeLobbyBgm
{
    public class Main
    {
        public static Harmony Harmony { get; private set; }
        public static Settings Settings { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        public static Dictionary<SystemLanguage, Dictionary<string, string>> Localizations { get; }
            = new Dictionary<SystemLanguage, Dictionary<string, string>>
            {
                {
                    SystemLanguage.Korean, new Dictionary<string, string>
                    {
                        { "settings.apply", "적용" },
                        { "settings.loading", "로딩중..." },
                        { "settings.defaultBpm", "기본 BPM" },
                        { "settings.fastBpm", "빨라질 시 BPM" },
                        { "settings.multiplyMusic", "속도 변경 시 음악 배속" },
                        { "settings.customMusic", "음악 변경" },
                        { "settings.defaultMusicPath", "기본 음악" },
                        { "settings.fastMusicPath", "빨라질 시 음악" },
                        { "settings.reloadToApply", "<b>몇몇 설정은 로비에서 나갔다가 들어와야 적용됩니다.</b>" },
                        { "settings.default", "일반" },
                        { "settings.fast", "배속" }
                    }
                },
                {
                    SystemLanguage.English, new Dictionary<string, string>
                    {
                        { "settings.apply", "Faster BPM" },
                        { "settings.loading", "Loading..." },
                        { "settings.defaultBpm", "Default BPM" },
                        { "settings.fastBpm", "Faster BPM" },
                        { "settings.multiplyMusic", "Speed up bgm when changing speed" },
                        { "settings.customMusic", "Custom bgm" },
                        { "settings.defaultMusicPath", "Default bgm" },
                        { "settings.fastMusicPath", "Faster bgm" },
                        { "settings.reloadToApply", "<b>Some settings will be applied after rejoining lobby.</b>" },
                        { "settings.default", "default" },
                        { "settings.fast", "fast" }
                    }
                }
            };

        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            Logger = modEntry.Logger;
            defaultBpmCache = Settings.defaultBpm;
            fastBpmCache = Settings.fastBpm;
            defaultMusicPathCache = Settings.defaultMusicPath;
            fastMusicPathCache = Settings.fastMusicPath;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool active)
        {
            if (active)
            {
                Harmony = new Harmony(modEntry.Info.Id);
                Harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
                Harmony.UnpatchAll(modEntry.Info.Id);
            return true;
        }

        private static float defaultBpmCache;
        private static float fastBpmCache;
        private static string defaultMusicPathCache;
        private static string fastMusicPathCache;
        private static bool loadingDefault;
        private static bool loadingFast;
        private static bool refresh;

        public static AudioClip defaultBgm;
        public static AudioClip fastBgm;

        public static void OnGUI(UnityModManager.ModEntry modEntry) {
            Dictionary<string, string> localizations = Localizations.TryGetValue(RDString.language, out Dictionary<string, string> dict) ? dict : Localizations[SystemLanguage.English];
            GUILayout.Label(localizations["settings.defaultBpm"]);
            GUILayout.BeginHorizontal();
            try
            {
                defaultBpmCache = float.Parse(GUILayout.TextField(string.Format("{0:0.0000}", defaultBpmCache), GUILayout.Width(100)));
            }
            catch (Exception)
            {
            }
            if (defaultBpmCache != Settings.defaultBpm)
            {
                GUILayout.Space(5);
                if (GUILayout.Button(localizations["settings.apply"], GUILayout.Width(70)))
                    defaultBpmCache = Settings.defaultBpm = Mathf.Max(defaultBpmCache, 0.0001f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(localizations["settings.fastBpm"]);
            GUILayout.BeginHorizontal();
            try
            {
                fastBpmCache = float.Parse(GUILayout.TextField(string.Format("{0:0.0000}", fastBpmCache), GUILayout.Width(100)));
            }
            catch (Exception)
            {
            }
            if (fastBpmCache != Settings.fastBpm)
            {
                GUILayout.Space(5);
                if (GUILayout.Button(localizations["settings.apply"], GUILayout.Width(70)))
                    fastBpmCache = Settings.fastBpm = Mathf.Max(fastBpmCache, 0.0001f);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            Settings.multiplyMusic = GUILayout.Toggle(Settings.multiplyMusic, localizations["settings.multiplyMusic"]);
            GUILayout.Space(20);

            Settings.customMusic = GUILayout.Toggle(Settings.customMusic, localizations["settings.customMusic"]);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(localizations["settings.default"], GUILayout.Width(30));
            GUILayout.Space(5);
            defaultMusicPathCache = GUILayout.TextField(defaultMusicPathCache, GUILayout.Width(300));
            GUILayout.Space(5);
            if (!loadingDefault)
            {
                if (defaultMusicPathCache != Settings.defaultMusicPath)
                    if (File.Exists(defaultMusicPathCache) && GUILayout.Button(localizations["settings.apply"], GUILayout.Width(70)))
                    {
                        Settings.defaultMusicPath = defaultMusicPathCache;
                        LoadMusic(defaultMusicPathCache, true);
                    }
            }
            else
                GUILayout.Label(localizations["settings.loading"]);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label(localizations["settings.fast"], GUILayout.Width(30));
            GUILayout.Space(5);
            fastMusicPathCache = GUILayout.TextField(fastMusicPathCache, GUILayout.Width(300));
            GUILayout.Space(5);
            if (!loadingFast)
            {
                if (fastMusicPathCache != Settings.fastMusicPath)
                    if (File.Exists(fastMusicPathCache) && GUILayout.Button(localizations["settings.apply"], GUILayout.Width(70)))
                    {
                        Settings.fastMusicPath = fastMusicPathCache;
                        LoadMusic(fastMusicPathCache, false);
                    }
            }
            else
                GUILayout.Label(localizations["settings.loading"]);
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.Label(localizations["settings.reloadToApply"]);
        }

        public static void LoadMusic(string path, bool defaultOrFast)
        {
            StaticCoroutine.Start(LoadMusicCo(path, defaultOrFast));
        }

        private static IEnumerator LoadMusicCo(string path, bool defaultOrFast)
        {
            if (defaultOrFast)
            {
                loadingDefault = true;
                defaultBgm = null;
            }
            else
            {
                loadingFast = true;
                fastBgm = null;
            }
            AudioClip bgm = null;
            if (path != null && File.Exists(path))
            {
                Logger.Log($"start loading '{path}', '{defaultOrFast}'");
                bgm = AudioManager.Instance.FindOrLoadAudioClip(Path.GetFileName(path) + "*external");
                if (bgm == null)
                {
                    var load = AudioManager.Instance.FindOrLoadAudioClipExternal(path, false);
                    yield return load;
                    var result = (RDAudioLoadResult)load.Current;
                    Logger.Log(result.type.ToString());
                    if (result.type == RDAudioLoadType.SuccessExternalClipLoaded)
                        bgm = result.clip;
                }
                Logger.Log($"end loading '{path}', '{defaultOrFast}'");
            }
            else
                Logger.Log($"skip loading '{path}', '{defaultOrFast}'");

            if (defaultOrFast)
            {
                loadingDefault = false;
                defaultBgm = bgm;
                if (Settings.customMusic && scnLevelSelect.instance)
                {
                    if (!(scrConductor.instance.song.clip = defaultBgm))
                        scrConductor.instance.song.Stop();
                }
            }
            else
            {
                loadingFast = false;
                fastBgm = bgm;
                if (Settings.customMusic && scnLevelSelect.instance)
                {
                    if (!(scrConductor.instance.song2.clip = fastBgm))
                        scrConductor.instance.song2.Stop();
                }
            }
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }
    }
}

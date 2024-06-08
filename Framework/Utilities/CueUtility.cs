using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using System.Collections.Generic;
using System.Linq;

namespace Trickster.Framework.Utilities
{
    public class ActiveCue(string name, string location, ICue? localCue)
    {
        public string Name { get; set; } = name;
        public string Location { get; set; } = location;
        public ICue? TrackedCue { get; set; } = localCue;
    }

    public class CueDetails(string name, string category, bool favorite)
    {
        public string Name { get; set; } = name;
        public string Category { get; set; } = category;
        public bool Favorite { get; set; } = favorite;
    }

    internal static class CueUtility
    {
        private static readonly Dictionary<uint, string> categoryDict = new()
        {
            { 0, "Global" },
            { 1, "Default" },
            { 2, "Music" },
            { 3, "Sound" },
            { 4, "Ambient" },
            { 5, "Footsteps" }
        };

        private static readonly string[] _loopingCues =
        [
            "roadnoise",
            "flybuzzing",
            "trainLoop",
            "fastReel",
            "slowReel",
            "minecartLoop",
            "nightTime",
            "SinWave",
            "fuse"
        ];

        private static List<CueDetails> _defaultCueList = [];
        private static readonly List<ActiveCue> activeCues = [];
        public static List<CueDetails> CueList { get; private set; } = [];
        public static List<string> LoopingCueList { get; private set; } = [];
        public static List<string> MusicCueList { get; private set; } = [];

        private static string? GetCategory(uint index) => categoryDict.GetValueOrDefault(index);
        public static CueDetails? GetCueDetailsByName(string cueName) => CueList.Find(cue => cue.Name == cueName);
        public static bool IsFavoriteCue(string cueName) => CueList.Any(c => c.Name == cueName && c.Favorite);
        public static void PopulateCueDict()
        {
            var soundBank = ModEntry.ModHelper.Reflection.GetField<SoundBank>(Game1.soundBank, "soundBank").GetValue();
            var soundBankCues = ModEntry.ModHelper.Reflection.GetField<Dictionary<string, CueDefinition>>(soundBank, "_cues").GetValue();
            var favList  = ModEntry.ModHelper.Data.ReadJsonFile<List<string>>("favorites.json");

            foreach (var cue in soundBankCues.Values)
            {
                var categoryName = GetCategory(cue.sounds[0].categoryID);
                if (categoryName == null) continue;

                bool favorite = favList?.Any(favCueName => favCueName == cue.name) == true;

                CueList.Add(new CueDetails(cue.name, categoryName, favorite));
            }

            // Not actually sure if music loops, don't care enough to check
            MusicCueList = GetCueByCategory(CueList, "Music");
            LoopingCueList = CueList
                .Where(cueDetails => _loopingCues.Contains(cueDetails.Name))
                .Select(cueDetails => cueDetails.Name)
                .ToList();

            // Store the default cue list
            _defaultCueList = [.. CueList];
        }

        public static List<string> GetAllCues(List<CueDetails> list) => list.ConvertAll(cueDetails => cueDetails.Name);

        private static List<string> GetCueByCategory(List<CueDetails> list, string category) => list
            .Where(cueDetails => cueDetails.Category == category)
            .Select(cueDetails => cueDetails.Name)
            .ToList();

        private static void PlayAll(string cueName, GameLocation location)
        {
            if (!MusicCueList.Contains(cueName))
            {
                if (activeCues.Any(activeCue => activeCue.Name == cueName && activeCue.Location == location.NameOrUniqueName)) return;
                PlayLocalDummy(cueName, out var cue);
                location.netAudio.StartPlaying(cueName);
                activeCues.Add(new ActiveCue(cueName, location.NameOrUniqueName, cue));
                ModEntry.ModMonitor.Log($"Playing {cueName} cue at {location.NameOrUniqueName}", LogLevel.Debug);
            }
            else
            {
                if (Game1.getMusicTrackName() == cueName) return;
                Game1.changeMusicTrack(cueName, true);
                ModEntry.ModMonitor.LogOnce("Music may not work right idk, I didn't test", LogLevel.Warn);
                ModEntry.ModMonitor.Log($"Requesting music track {cueName}", LogLevel.Debug);
                activeCues.Add(new ActiveCue(cueName, location.NameOrUniqueName, null));
            }
        }

        private static bool PlayLocalDummy(string cueName, out ICue cue)
        {
            cue = Game1.soundBank.GetCue(cueName);
            cue.Play();
            cue.Volume = 0f;
            return true;
        }

        public static void PrepareToPlayCue(string cueName, GameLocation location)
        {
            List<ActiveCue> cuesToRemove = [];
            foreach (var cue in activeCues.Where(cue => cue.Name == cueName && cue.Location == location.NameOrUniqueName))
            {
                Game1.getLocationFromName(cue.Location).netAudio.StopPlaying(cue.Name);
                cue.TrackedCue?.Stop(AudioStopOptions.Immediate);
                cuesToRemove.Add(cue);
            }

            foreach (var cueToRemove in cuesToRemove)
            {
                activeCues.Remove(cueToRemove);
            }

            DelayedAction.functionAfterDelay(() => PlayAll(cueName, location), 100);
        }

        public static void StopAllPlayingCues()
        {
            int numOfRemovedCues = 0;
            foreach (var cue in activeCues)
            {
                numOfRemovedCues++;
                if (!MusicCueList.Contains(cue.Name))
                {
                    Game1.getLocationFromName(cue.Location).netAudio.StopPlaying(cue.Name);
                    cue.TrackedCue?.Stop(AudioStopOptions.Immediate);
                }
                else
                {
                    Game1.stopMusicTrack(MusicContext.Default);
                }
            }

            if (numOfRemovedCues == 0) return;
            ModEntry.ModMonitor.Log($"Killed {numOfRemovedCues} cues", LogLevel.Debug);
            activeCues.Clear();
        }

        public static void UpdateCueState()
        {
            foreach (var cue in activeCues.Where(cue => cue.TrackedCue is { IsStopped: true }).ToList())
            {
                Game1.getLocationFromName(cue.Location).netAudio.StopPlaying(cue.Name);
                activeCues.Remove(cue);
            }
        }

        // Sorting methods
        private static int currentSortModeIndex;
        public static int NextSortMode()
        {
            currentSortModeIndex = (currentSortModeIndex + 1) % 3;
            CueList = currentSortModeIndex switch
            {
                1 => [.. CueList.OrderBy(c => c.Name)],
                2 => [.. CueList.OrderByDescending(c => c.Name)],
                _ => [.. _defaultCueList]
            };

            return currentSortModeIndex;
        }
    }
}
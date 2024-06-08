using Common.Managers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Linq;
using Microsoft.Xna.Framework;
using SpaceCore.Events;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using Trickster.Framework.Menus;
using Trickster.Framework.Utilities;
using static StardewValley.Farmer;

namespace Trickster
{
    public class ModEntry : Mod
    {
        public static IModHelper ModHelper { get; private set; } = null!;
        public static IMonitor ModMonitor { get; private set; } = null!;
        public static ModConfig Config { get; private set; } = null!;
        public static Multiplayer? Multiplayer { get; private set; }
        public override void Entry(IModHelper helper)
        {
            // Setup the monitor, helper, config and multiplayer
            ModMonitor = Monitor;
            ModHelper = helper;
            Config = Helper.ReadConfig<ModConfig>();
            Multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Init Common
            ConfigManager.Init(ModManifest, Config, ModHelper, ModMonitor);
            //PatchHelper.Init(new Harmony(ModManifest.UniqueID));

            // Hook into Game events
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.Input.ButtonsChanged += OnButtonChanged;
        }
        private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Config.EnableMod) return;

            CueUtility.UpdateCueState();
        }
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            if (!Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu")) return;

            ConfigManager.AddOption(nameof(ModConfig.EnableMod));
            ConfigManager.AddOption(nameof(ModConfig.EnableMusicCues));
            ConfigManager.AddOption(nameof(ModConfig.OpenMenuKey));
        }

        private static void OnSaving(object? sender, SavingEventArgs e)
        {
            var favoriteCueNames = CueUtility.CueList.Where(c => c.Favorite).Select(c => c.Name).ToList();
            ModHelper.Data.WriteJsonFile("favorites.json", favoriteCueNames);
        }

        private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            CueUtility.PopulateCueDict();
        }
        private static void OnButtonChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsWorldReady || !Config.EnableMod)
                return;

            //if (Config.TestKey!.JustPressed())
            //{
            //    //Game1.displayFarmer = false;
            //    //Game1.player.isSitting.Value = !Game1.player.isSitting.Value; // Null ref object
            //    //Game1.player.PerformKiss(Game1.player.facingDirection.Value);
            //    //Multiplayer!.globalChatInfoMessageEvenInSinglePlayer("UserNotificationMessageFormat", "You Will Die In 7 Days.");
            //    //var farmer = Game1.getAllFarmers().LastOrDefault(f => f != Game1.player);
            //    //Item item = ItemRegistry.Create("(B)505");
            //    //performEatAnimation(ItemRegistry.Create<SObject>("(O)789"));
            //    foreach (var emote in Farmer.EMOTES)
            //    {
            //        ModMonitor.Log($"Name: {emote.displayName}, Hidden: {emote.hidden}", LogLevel.Debug);
            //    }
            //}

            if (Config.OpenMenuKey!.JustPressed())
                HandleMenuButtonPress();
        }

        private static void HandleMenuButtonPress()
        {
            if (Context.IsPlayerFree && Game1.activeClickableMenu == null)
            {
                Game1.activeClickableMenu = new AudioMenu();
            }
            else if (Game1.activeClickableMenu is AudioMenu)
            {
                Game1.activeClickableMenu.exitThisMenu();
            }
        }
    }
}
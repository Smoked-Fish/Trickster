global using SObject = StardewValley.Object;
using Common.Managers;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Trickster
{
    public class ModEntry : Mod
    {
        internal static IModHelper ModHelper { get; set; }
        internal static IMonitor ModMonitor { get; set; }
        internal static ModConfig Config { get; set; }
        internal static Multiplayer Multiplayer { get; set; }
        internal static ApiManager ApiManager { get; set; }

        private static Harmony harmony;

        public override void Entry(IModHelper helper)
        {
            // Setup the monitor, helper, config and multiplayer
            ModMonitor = Monitor;
            ModHelper = helper;
            Config = Helper.ReadConfig<ModConfig>();
            Multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Setup the manager
            ApiManager = new ApiManager(helper, ModMonitor);

            // Load the Harmony patches
            harmony = new Harmony(this.ModManifest.UniqueID);
            //new ExamplePatch(harmony).Apply();

            // Hook into Game events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ConfigManager.Initialize(ModManifest, Config, ModHelper, ModMonitor, harmony);
            if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
            {
                ConfigManager.AddOption(nameof(ModConfig.EnableMod));
            }
        }
    }
}
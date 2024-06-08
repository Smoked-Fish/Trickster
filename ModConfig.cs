using Common.Interfaces;
using Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Trickster
{
    public sealed class ModConfig : IConfigurable
    {
        [DefaultValue(true)]
        public bool EnableMod { get; set; }

        [DefaultValue(false)]
        public bool EnableMusicCues { get; set; }

        [DefaultValue(SButton.MouseX1)]
        public KeybindList? OpenMenuKey {  get; set; }

        [DefaultValue(SButton.MouseX2)]
        public KeybindList? TestKey {  get; set; }

        public ModConfig()
        {
            ConfigUtility.InitializeDefaultConfig(this);
        }
    }
}

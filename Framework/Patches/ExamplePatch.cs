using Common.Util;
using HarmonyLib;
using StardewValley;
using StardewValley.Objects;

namespace Trickster.Framework.Patches
{
    internal class ExamplePatch : PatchTemplate
    {
        internal ExamplePatch(Harmony harmony) : base(harmony, typeof(Furniture)) { }
        internal void Apply()
        {
            Patch(PatchType.Postfix, nameof(Furniture.canBeRemoved), nameof(ExamplePostfix), [typeof(Farmer)]);
        }
        private static void ExamplePostfix(Furniture __instance, Farmer who, ref bool __result)
        {
            if (!ModEntry.Config.EnableMod)
                return;
        }
    }
}
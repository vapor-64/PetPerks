using HarmonyLib;
using StardewValley;
using StardewValley.Buildings;

namespace PetPerks.Patches;

/// <summary>A turtle on the farm multiplies the roe produced by fish ponds.</summary>
[HarmonyPatch(typeof(FishPond), nameof(FishPond.GetFishProduce))]
internal static class FishPondProducePatch
{
    public static void Postfix(ref Item? __result)
    {
        var config = ModEntry.Instance.Config;
        if (__result is null || !config.TurtlesBoostRoe || config.TurtleRoeMultiplier <= 1f)
            return;
        if (!__result.Name.Contains("Roe") || !PetHelper.HasPet(PetHelper.Turtle))
            return;

        // Round the scaled stack up or down at random so the average matches the multiplier (1 roe x 1.5 = 1 or 2).
        float scaled = __result.Stack * config.TurtleRoeMultiplier;
        int stack = (int)scaled;
        if (Game1.random.NextDouble() < scaled - stack)
            stack++;
        __result.Stack = Math.Max(__result.Stack, stack);
    }
}

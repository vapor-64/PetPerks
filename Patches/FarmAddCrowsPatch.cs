using HarmonyLib;
using StardewValley;

namespace PetPerks.Patches;

/// <summary>Cats scare the crows off before they can eat any crops, so no scarecrow is needed.</summary>
[HarmonyPatch(typeof(Farm), nameof(Farm.addCrows))]
internal static class FarmAddCrowsPatch
{
    public static bool Prefix()
    {
        return !(ModEntry.Instance.Config.CatsScareCrows && PetHelper.HasPet(PetHelper.Cat));
    }
}

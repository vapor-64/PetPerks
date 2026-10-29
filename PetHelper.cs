using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;

namespace PetPerks;

internal static class PetHelper
{
    public const string Cat = "Cat";
    public const string Dog = "Dog";
    public const string Turtle = "Turtle";

    /// <summary>Whether any pet of the given type lives on the farm.</summary>
    public static bool HasPet(string petType)
    {
        if (!Context.IsWorldReady)
            return false;

        foreach (Pet pet in Utility.getAllPets())
        {
            if (pet.petType.Value == petType)
                return true;
        }
        return false;
    }
}

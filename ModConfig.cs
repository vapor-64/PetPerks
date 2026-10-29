namespace PetPerks;

/// <summary>Mod configuration loaded from config.json.</summary>
public class ModConfig
{
    /// <summary>Cats scare crows away, so scarecrows are not needed.</summary>
    public bool CatsScareCrows { get; set; } = true;

    /// <summary>Dogs open the barn/coop doors, herd animals out in the morning and back in at night.</summary>
    public bool DogsHerdAnimals { get; set; } = true;

    /// <summary>Turtles make fish ponds produce more roe.</summary>
    public bool TurtlesBoostRoe { get; set; } = true;

    /// <summary>Roe stack multiplier applied when a turtle lives on the farm.</summary>
    public float TurtleRoeMultiplier { get; set; } = 1.5f;

    /// <summary>Time (24h, e.g. 610 = 6:10 AM) the dog opens the animal doors and lets the animals out.</summary>
    public int HerdOutTime { get; set; } = 610;

    /// <summary>Time (24h, e.g. 1800 = 6:00 PM) the dog starts herding the animals back in.</summary>
    public int HerdInTime { get; set; } = 1800;

    /// <summary>Time (24h, e.g. 1900 = 7:00 PM) the dog gets stragglers inside and closes the animal doors.</summary>
    public int CloseDoorsTime { get; set; } = 1900;
}

using StardewModdingAPI;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Pathfinding;

namespace PetPerks;

/// <summary>
/// The dog's perk: opens the animal doors in the morning and lets the animals out, then herds them back in
/// at night and closes the doors. Runs on the host only, since the host owns the animals.
/// </summary>
internal class AnimalHerder
{
    private readonly Queue<FarmAnimal> _toRelease = new();
    private readonly Queue<FarmAnimal> _toRecall = new();

    private static ModConfig Config => ModEntry.Instance.Config;

    private static bool Active => Context.IsWorldReady && Context.IsMainPlayer
        && Config.DogsHerdAnimals && PetHelper.HasPet(PetHelper.Dog);

    public void Reset()
    {
        _toRelease.Clear();
        _toRecall.Clear();
    }

    public void OnTimeChanged(int time)
    {
        if (!Active)
            return;

        if (time == Config.HerdOutTime)
            StartHerdingOut();
        else if (time == Config.HerdInTime)
            StartHerdingIn();
        else if (time > Config.HerdInTime && time < Config.CloseDoorsTime)
            RequeueStragglers();
        else if (time == Config.CloseDoorsTime)
            FinishHerdingIn();
    }

    /// <summary>Releases one queued animal per call, so they trickle out instead of appearing all at once.</summary>
    public void OnSecondPassed()
    {
        if (!Context.IsWorldReady || !Context.IsMainPlayer)
            return;

        if (_toRelease.Count > 0)
            ReleaseAnimal(_toRelease.Dequeue());
        else if (_toRecall.Count > 0)
            RecallAnimal(_toRecall.Dequeue());
    }

    // ── Morning ───────────────────────────────────────────────────────────────

    public void StartHerdingOut()
    {
        Farm farm = Game1.getFarm();
        if (farm.IsRainingHere() || farm.IsWinterHere() || Utility.isFestivalDay())
            return;

        _toRecall.Clear();
        foreach (Building building in AnimalBuildings(farm))
        {
            SetDoor(building, open: true);
            foreach (FarmAnimal animal in building.GetIndoors().animals.Values)
                _toRelease.Enqueue(animal);
        }
    }

    private void ReleaseAnimal(FarmAnimal animal)
    {
        Building? home = animal.home;
        GameLocation? indoors = animal.homeInterior;
        if (home is null || indoors is null || !indoors.animals.ContainsKey(animal.myID.Value))
            return;
        if (!home.animalDoorOpen.Value || indoors.farmers.Any())
            return;

        GameLocation farm = home.GetParentLocation();
        Rectangle door = home.getRectForAnimalDoor();
        door.Inflate(-2, -2);

        // Same exit position vanilla uses when an animal walks out on its own.
        var sprite = animal.GetOrLoadTexture();
        Vector2 position = new(door.X, door.Y - (sprite.getHeight() * 4 - animal.GetBoundingBox().Height) + 32);

        indoors.animals.Remove(animal.myID.Value);
        farm.animals.Remove(animal.myID.Value);
        farm.animals.TryAdd(animal.myID.Value, animal);
        animal.Position = position;
        animal.faceDirection(2);
        animal.SetMovingDown(true);
        animal.noWarpTimer = 3000;
        home.currentOccupants.Value = Math.Max(0, home.currentOccupants.Value - 1);

        if (FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick)
        {
            FarmAnimal.NumPathfindingThisTick++;
            animal.controller = new PathFindController(animal, farm, FarmAnimal.grassEndPointFunction,
                Game1.random.Next(4), FarmAnimal.behaviorAfterFindingGrassPatch, 200, Point.Zero);
            if (animal.controller.pathToEndPoint is null || animal.controller.pathToEndPoint.Count < 3)
                animal.controller = null;
        }
    }

    // ── Evening ───────────────────────────────────────────────────────────────

    public void StartHerdingIn()
    {
        Farm farm = Game1.getFarm();
        _toRelease.Clear();
        foreach (Building building in AnimalBuildings(farm))
            SetDoor(building, open: true);
        QueueOutdoorAnimals(farm);
    }

    private void RequeueStragglers()
    {
        if (_toRecall.Count == 0)
            QueueOutdoorAnimals(Game1.getFarm());
    }

    private void QueueOutdoorAnimals(Farm farm)
    {
        foreach (FarmAnimal animal in farm.animals.Values)
        {
            if (animal.home is not null && !animal.IsHome)
                _toRecall.Enqueue(animal);
        }
    }

    private void RecallAnimal(FarmAnimal animal)
    {
        Building? home = animal.home;
        if (home is null || animal.IsHome || animal.currentLocation is null)
            return;

        // The vanilla animal update warps the animal inside once it reaches the door.
        Point doorTile = new(home.tileX.Value + home.animalDoor.X, home.tileY.Value + home.animalDoor.Y);
        animal.noWarpTimer = 0;
        animal.controller = new PathFindController(animal, animal.currentLocation, PathFindController.isAtEndPoint,
            0, null, 200, doorTile);
        if (animal.controller.pathToEndPoint is null)
            animal.controller = null;
    }

    public void FinishHerdingIn()
    {
        Farm farm = Game1.getFarm();
        _toRelease.Clear();
        _toRecall.Clear();

        foreach (FarmAnimal animal in farm.animals.Values.ToList())
        {
            if (animal.home is not null && !animal.IsHome)
                animal.warpHome();
        }
        foreach (Building building in AnimalBuildings(farm))
            SetDoor(building, open: false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IEnumerable<Building> AnimalBuildings(Farm farm)
    {
        foreach (Building building in farm.buildings)
        {
            if (building.GetIndoors() is AnimalHouse && !building.isUnderConstruction())
                yield return building;
        }
    }

    private static void SetDoor(Building building, bool open)
    {
        if (building.animalDoorOpen.Value == open)
            return;

        var data = building.GetData();
        // Mirrors Building.ToggleAnimalDoor, which picks the sound from the state before toggling.
        string? sound = open ? data?.AnimalDoorCloseSound : data?.AnimalDoorOpenSound;
        if (sound is not null && Game1.currentLocation == building.GetParentLocation())
            Game1.currentLocation.playSound(sound);
        building.animalDoorOpen.Value = open;
    }
}

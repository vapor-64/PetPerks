using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;

namespace PetPerks;

public class ModEntry : Mod
{
    public static ModEntry Instance { get; private set; } = null!;
    public ModConfig Config { get; private set; } = null!;

    private readonly AnimalHerder _herder = new();

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();
        new Harmony(ModManifest.UniqueID).PatchAll();

        helper.Events.GameLoop.GameLaunched    += OnGameLaunched;
        helper.Events.GameLoop.DayStarted      += (_, _) => _herder.Reset();
        helper.Events.GameLoop.ReturnedToTitle += (_, _) => _herder.Reset();
        helper.Events.GameLoop.TimeChanged     += (_, e) => _herder.OnTimeChanged(e.NewTime);
        helper.Events.GameLoop.OneSecondUpdateTicked += (_, _) => _herder.OnSecondPassed();

        helper.ConsoleCommands.Add(
            "petperks_herd",
            "Runs the dog's herding now to test it. Usage: petperks_herd out|in|close",
            CommandHerd);

        Monitor.Log(helper.Translation.Get("mod.loaded"), LogLevel.Info);
    }

    private void CommandHerd(string command, string[] args)
    {
        if (!Context.IsWorldReady || !Context.IsMainPlayer)
        {
            Monitor.Log("Load a save as the host first.", LogLevel.Warn);
            return;
        }

        switch (args.FirstOrDefault())
        {
            case "out": _herder.StartHerdingOut(); break;
            case "in": _herder.StartHerdingIn(); break;
            case "close": _herder.FinishHerdingIn(); break;
            default: Monitor.Log("Usage: petperks_herd out|in|close", LogLevel.Warn); break;
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        RegisterWithGmcm();
    }

    private static string FormatTime(int time) => $"{time / 100}:{time % 100:00}";

    private void RegisterWithGmcm()
    {
        var gmcm = Helper.ModRegistry.GetApi<IGmcmApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;

        gmcm.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config));

        gmcm.AddSectionTitle(ModManifest, () => Helper.Translation.Get("gmcm.section.cats"));
        gmcm.AddBoolOption(
            mod: ModManifest,
            getValue: () => Config.CatsScareCrows,
            setValue: v => Config.CatsScareCrows = v,
            name: () => Helper.Translation.Get("gmcm.cats-scare-crows.name"),
            tooltip: () => Helper.Translation.Get("gmcm.cats-scare-crows.tooltip"));

        gmcm.AddSectionTitle(ModManifest, () => Helper.Translation.Get("gmcm.section.dogs"));
        gmcm.AddBoolOption(
            mod: ModManifest,
            getValue: () => Config.DogsHerdAnimals,
            setValue: v => Config.DogsHerdAnimals = v,
            name: () => Helper.Translation.Get("gmcm.dogs-herd.name"),
            tooltip: () => Helper.Translation.Get("gmcm.dogs-herd.tooltip"));
        gmcm.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.HerdOutTime,
            setValue: v => Config.HerdOutTime = v,
            name: () => Helper.Translation.Get("gmcm.herd-out-time.name"),
            tooltip: () => Helper.Translation.Get("gmcm.herd-out-time.tooltip"),
            min: 600, max: 1200, interval: 10,
            formatValue: FormatTime);
        gmcm.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.HerdInTime,
            setValue: v => Config.HerdInTime = v,
            name: () => Helper.Translation.Get("gmcm.herd-in-time.name"),
            tooltip: () => Helper.Translation.Get("gmcm.herd-in-time.tooltip"),
            min: 1500, max: 2200, interval: 10,
            formatValue: FormatTime);
        gmcm.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.CloseDoorsTime,
            setValue: v => Config.CloseDoorsTime = v,
            name: () => Helper.Translation.Get("gmcm.close-doors-time.name"),
            tooltip: () => Helper.Translation.Get("gmcm.close-doors-time.tooltip"),
            min: 1600, max: 2400, interval: 10,
            formatValue: FormatTime);

        gmcm.AddSectionTitle(ModManifest, () => Helper.Translation.Get("gmcm.section.turtles"));
        gmcm.AddBoolOption(
            mod: ModManifest,
            getValue: () => Config.TurtlesBoostRoe,
            setValue: v => Config.TurtlesBoostRoe = v,
            name: () => Helper.Translation.Get("gmcm.turtles-roe.name"),
            tooltip: () => Helper.Translation.Get("gmcm.turtles-roe.tooltip"));
        gmcm.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.TurtleRoeMultiplier,
            setValue: v => Config.TurtleRoeMultiplier = (float)Math.Round(v, 2),
            name: () => Helper.Translation.Get("gmcm.roe-multiplier.name"),
            tooltip: () => Helper.Translation.Get("gmcm.roe-multiplier.tooltip"),
            min: 1f, max: 5f, interval: 0.25f,
            formatValue: v => $"x{v:0.##}");
    }
}

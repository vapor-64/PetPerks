using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;

namespace PetPerks;

public class ModEntry : Mod
{
    public static ModEntry Instance { get; private set; } = null!;
    public ModConfig Config { get; private set; } = null!;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();
        new Harmony(ModManifest.UniqueID).PatchAll();

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded   += OnSaveLoaded;
        helper.Events.GameLoop.DayStarted   += OnDayStarted;

        Monitor.Log(helper.Translation.Get("mod.loaded"), LogLevel.Info);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Integrate with other mods' APIs here.
        var gmcm = Helper.ModRegistry.GetApi<IGmcmApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;

        gmcm.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config));

        // Example option - replace with real settings from ModConfig.cs
        gmcm.AddBoolOption(
            mod: ModManifest,
            getValue: () => Config.Enabled,
            setValue: v => Config.Enabled = v,
            name: () => Helper.Translation.Get("gmcm.enabled.name"),
            tooltip: () => Helper.Translation.Get("gmcm.enabled.tooltip"));
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        // Safe to access Game1.player from here on.
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
    }
}
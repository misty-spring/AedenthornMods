using CustomMonsterFloors.Patches;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Locations;

namespace CustomMonsterFloors;

/// <summary>The mod entry point.</summary>
public class ModEntry : Mod
{
    internal static ModConfig Config;

    internal static IModHelper Help { get; set; }
    internal static IMonitor SMonitor { get; set; }

    /*********
     ** Public methods
     *********/
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        Config = helper.ReadConfig<ModConfig>();
        SMonitor = Monitor;
        Help = Helper;
            
        if (!Config.EnableMod)
            return;

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.DayStarted += OnDayStarted;

        var harmony = new Harmony(ModManifest.UniqueID);

        if (Config.EnableFloorTypeChanges)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "loadLevel"),
                postfix: new HarmonyMethod(typeof(MineShaftPatches), nameof(MineShaftPatches.loadLevel_Postfix))
            );
        }
        if (Config.EnableTileChanges)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "adjustLevelChances"),
                postfix: new HarmonyMethod(typeof(MineShaftPatches), nameof(MineShaftPatches.adjustLevelChances_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "createLitterObject"),
                prefix: new HarmonyMethod(typeof(MineShaftPatches), nameof(MineShaftPatches.chooseStoneType_Prefix)),
                postfix: new HarmonyMethod(typeof(MineShaftPatches), nameof(MineShaftPatches.chooseStoneType_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "checkStoneForItems"),
                postfix: new HarmonyMethod(typeof(MineShaftPatches), nameof(MineShaftPatches.checkStoneForItems_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "tryToAddAreaUniques"),
                transpiler: new HarmonyMethod(typeof(MineShaftPatches), nameof(MineShaftPatches.MineShaft_tryToAddAreaUniques_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "populateLevel"),
                transpiler: new HarmonyMethod(typeof(MineShaftPatches), nameof(MineShaftPatches.MineShaft_populateLevel_Transpiler))
            );
        }
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        // get Generic Mod Config Menu's API (if it's installed)
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        // register mod
        configMenu.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
            );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("EnableMod.title"),
            //tooltip: () => Helper.Translation.Get("EnableMod.description"),
            getValue: () => Config.EnableMod,
            setValue: value => Config.EnableMod = value
            );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("EnableFloorTypeChanges.title"),
            //tooltip: () => Helper.Translation.Get("EnableFloorTypeChanges.description"),
            getValue: () => Config.EnableFloorTypeChanges,
            setValue: value => Config.EnableFloorTypeChanges = value
            );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("EnableTileChanges.title"),
            //tooltip: () => Helper.Translation.Get("EnableTileChanges.description"),
            getValue: () => Config.EnableTileChanges,
            setValue: value => Config.EnableTileChanges = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("PercentChanceMonsterFloor.title"),
            //tooltip: () => Helper.Translation.Get("PercentChanceMonsterFloor.description"),
            getValue: () => Config.PercentChanceMonsterFloor,
            setValue: value => Config.PercentChanceMonsterFloor = value
            );
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("SlimeDinoMonsterSplitPercents.title"),
            //tooltip: () => Helper.Translation.Get("SlimeDinoMonsterSplitPercents.description"),
            getValue: () => Config.SlimeDinoMonsterSplitPercents,
            setValue: value => Config.SlimeDinoMonsterSplitPercents = value
            );
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("SlimeMonsterSplitPercents.title"),
            //tooltip: () => Helper.Translation.Get("SlimeMonsterSplitPercents.description"),
            getValue: () => Config.SlimeMonsterSplitPercents,
            setValue: value => Config.SlimeMonsterSplitPercents = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("MinFloorsBetweenMonsterFloors.title"),
            //tooltip: () => Helper.Translation.Get("MinFloorsBetweenMonsterFloors.description"),
            getValue: () => Config.MinFloorsBetweenMonsterFloors,
            setValue: value => Config.MinFloorsBetweenMonsterFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("TreasureChestFloorMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("TreasureChestFloorMultiplier.description"),
            getValue: () => Config.TreasureChestFloorMultiplier,
            setValue: value => Config.TreasureChestFloorMultiplier = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("MinFloorsBetweenTreasureFloors.title"),
            //tooltip: () => Helper.Translation.Get("MinFloorsBetweenTreasureFloors.description"),
            getValue: () => Config.MinFloorsBetweenTreasureFloors,
            setValue: value => Config.MinFloorsBetweenTreasureFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("MonsterMultiplierOnDinoFloors.title"),
            //tooltip: () => Helper.Translation.Get("MonsterMultiplierOnDinoFloors.description"),
            getValue: () => Config.MonsterMultiplierOnDinoFloors,
            setValue: value => Config.MonsterMultiplierOnDinoFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("MonsterMultiplierOnSlimeFloors.title"),
            //tooltip: () => Helper.Translation.Get("MonsterMultiplierOnSlimeFloors.description"),
            getValue: () => Config.MonsterMultiplierOnSlimeFloors,
            setValue: value => Config.MonsterMultiplierOnSlimeFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("MonsterMultiplierOnMonsterFloors.title"),
            //tooltip: () => Helper.Translation.Get("MonsterMultiplierOnMonsterFloors.description"),
            getValue: () => Config.MonsterMultiplierOnMonsterFloors,
            setValue: value => Config.MonsterMultiplierOnMonsterFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("MonsterMultiplierOnRegularFloors.title"),
            //tooltip: () => Helper.Translation.Get("MonsterMultiplierOnRegularFloors.description"),
            getValue: () => Config.MonsterMultiplierOnRegularFloors,
            setValue: value => Config.MonsterMultiplierOnRegularFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ItemMultiplierOnDinoFloors.title"),
            //tooltip: () => Helper.Translation.Get("ItemMultiplierOnDinoFloors.description"),
            getValue: () => Config.ItemMultiplierOnDinoFloors,
            setValue: value => Config.ItemMultiplierOnDinoFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ItemMultiplierOnSlimeFloors.title"),
            //tooltip: () => Helper.Translation.Get("ItemMultiplierOnSlimeFloors.description"),
            getValue: () => Config.ItemMultiplierOnSlimeFloors,
            setValue: value => Config.ItemMultiplierOnSlimeFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("MinFloorsBetweenMonsterFloors.title"),
            //tooltip: () => Helper.Translation.Get("MinFloorsBetweenMonsterFloors.description"),
            getValue: () => Config.ItemMultiplierOnMonsterFloors,
            setValue: value => Config.ItemMultiplierOnMonsterFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ItemMultiplierOnRegularFloors.title"),
            //tooltip: () => Helper.Translation.Get("ItemMultiplierOnRegularFloors.description"),
            getValue: () => Config.ItemMultiplierOnRegularFloors,
            setValue: value => Config.ItemMultiplierOnRegularFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("StoneMultiplierOnRegularFloors.title"),
            //tooltip: () => Helper.Translation.Get("StoneMultiplierOnRegularFloors.description"),
            getValue: () => Config.StoneMultiplierOnRegularFloors,
            setValue: value => Config.StoneMultiplierOnRegularFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("GemstoneMultiplierOnRegularFloors.title"),
            //tooltip: () => Helper.Translation.Get("GemstoneMultiplierOnRegularFloors.description"),
            getValue: () => Config.GemstoneMultiplierOnRegularFloors,
            setValue: value => Config.GemstoneMultiplierOnRegularFloors = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ChanceForOresMultiplierInMines.title"),
            //tooltip: () => Helper.Translation.Get("ChanceForOresMultiplierInMines.description"),
            getValue: () => Config.ChanceForOresMultiplierInMines,
            setValue: value => Config.ChanceForOresMultiplierInMines = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ChanceForOreMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("ChanceForOreMultiplier.description"),
            getValue: () => Config.ChanceForOreMultiplier,
            setValue: value => Config.ChanceForOreMultiplier = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ChanceForIridiumMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("ChanceForIridiumMultiplier.description"),
            getValue: () => Config.ChanceForIridiumMultiplier,
            setValue: value => Config.ChanceForIridiumMultiplier = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ChanceForGoldMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("ChanceForGoldMultiplier.description"),
            getValue: () => Config.ChanceForGoldMultiplier,
            setValue: value => Config.ChanceForGoldMultiplier = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ChanceForIronMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("ChanceForIronMultiplier.description"),
            getValue: () => Config.ChanceForIronMultiplier,
            setValue: value => Config.ChanceForIronMultiplier = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("PurpleStoneMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("PurpleStoneMultiplier.description"),
            getValue: () => Config.PurpleStoneMultiplier,
            setValue: value => Config.PurpleStoneMultiplier = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("MysticStoneMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("MysticStoneMultiplier.description"),
            getValue: () => Config.MysticStoneMultiplier,
            setValue: value => Config.MysticStoneMultiplier = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ResourceClumpChance.title"),
            //tooltip: () => Helper.Translation.Get("ResourceClumpChance.description"),
            getValue: () => (float)Config.ResourceClumpChance,
            setValue: value => Config.ResourceClumpChance = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("WeedsChance.title"),
            //tooltip: () => Helper.Translation.Get("WeedsChance.description"),
            getValue: () => (float)Config.WeedsChance,
            setValue: value => Config.WeedsChance = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("WeedsMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("WeedsMultiplier.description"),
            getValue: () => (float)Config.WeedsMultiplier,
            setValue: value => Config.WeedsMultiplier = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ChanceForLadderInStoneMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("ChanceForLadderInStoneMultiplier.description"),
            getValue: () => Config.ChanceForLadderInStoneMultiplier,
            setValue: value => Config.ChanceForLadderInStoneMultiplier = value
            );
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ChanceLadderIsShaftMultiplier.title"),
            //tooltip: () => Helper.Translation.Get("ChanceLadderIsShaftMultiplier.description"),
            getValue: () => Config.ChanceLadderIsShaftMultiplier,
            setValue: value => Config.ChanceLadderIsShaftMultiplier = value
            );
    }

    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        MineShaftPatches.monsterFloors.Clear();
        MineShaftPatches.treasureFloors.Clear();
        MineShaftPatches.GotShaft = false;
    }
}
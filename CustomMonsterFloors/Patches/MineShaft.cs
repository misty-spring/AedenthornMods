using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using Object = StardewValley.Object;

namespace CustomMonsterFloors.Patches;

public static class MineShaftPatches
{
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.SMonitor.Log(msg, lv);
    private static ModConfig Config => ModEntry.Config;
    private static IModHelper Helper => ModEntry.Help;

    public static List<int> monsterFloors = new List<int>();
    public static List<int> treasureFloors = new List<int>();

    public static void loadLevel_Postfix(ref MineShaft __instance, int level)
    {
        GotShaft = false;
        var isTreasureRoom = Helper.Reflection.GetField<NetBool>(__instance, "netIsTreasureRoom");
        var isSlimeArea = Helper.Reflection.GetField<NetBool>(__instance, "netIsSlimeArea");
        var isDinoArea = Helper.Reflection.GetField<NetBool>(__instance, "netIsDinoArea");
        var isMonsterArea = Helper.Reflection.GetField<NetBool>(__instance, "netIsMonsterArea");
        var isQuarryArea = Helper.Reflection.GetField<NetBool>(__instance, "netIsQuarryArea");
        var mapImageSource = __instance.mapImageSource;
        var loadedDarkArea = __instance.loadedDarkArea;

        if (__instance.getMineArea(level) == 77377 || level == 220)
        {
            return;
        }
        if (isTreasureRoom.GetValue().Value)
        {
            treasureFloors.Add(level);
            return;
        }
        Log($"Loaded level postfix {__instance.getMineArea(level)} {level}");

        if (__instance.getMineArea(level) == 121)
        {
            var treasureChance = 0.01;
            treasureChance += Game1.player.team.AverageDailyLuck(Game1.currentLocation) / 10.0 + Game1.player.team.AverageLuckLevel(Game1.currentLocation) / 100.0;
                
            Log($"checking for treasure level chance: {treasureChance * ModEntry.Config.TreasureChestFloorMultiplier - treasureChance}");
            if (Game1.random.NextDouble() < treasureChance * ModEntry.Config.TreasureChestFloorMultiplier - treasureChance)
            {
                Log("Creating treasure floor");
                treasureFloors.Add(level);
                isTreasureRoom.SetValue(new NetBool { true });
                __instance.loadedMapNumber = 10;
                if (isSlimeArea.GetValue().Value || isDinoArea.GetValue().Value)
                {
                    RevertMapImageSource(ref mapImageSource, ref loadedDarkArea, level, __instance.getMineArea(), __instance.getMineArea(level), __instance.loadedMapNumber);
                }
                return;
            }
        }

        var isMonsterFloor = Game1.random.Next(0, 100) < ModEntry.Config.PercentChanceMonsterFloor;

        if (isMonsterFloor)
        {

            if (IsBelowMinFloorsApart(level))
            {
                if (isSlimeArea.GetValue().Value || isDinoArea.GetValue().Value)
                {
                    RevertMapImageSource(ref mapImageSource, ref loadedDarkArea, level, __instance.getMineArea(), __instance.getMineArea(level), __instance.loadedMapNumber);
                }
                isDinoArea.SetValue(new NetBool { false });
                isSlimeArea.SetValue(new NetBool { false });
                isMonsterArea.SetValue(new NetBool { false });
                return;
            }

            monsterFloors.Add(level);

            if (isQuarryArea.GetValue().Value)
            {
                isMonsterArea.SetValue(new NetBool { true });
            }
            else
            {
                var roll = Game1.random.Next(0, 100);
                isMonsterArea.SetValue(new NetBool { true });
                if (__instance.getMineArea() == 121)
                {
                    var chances = ModEntry.Config.SlimeDinoMonsterSplitPercents.Split(':');
                    var slimeChance = int.Parse(chances[0]);
                    var dinoChance = int.Parse(chances[1]);
                    if (roll < slimeChance)
                    {
                        isDinoArea.SetValue(new NetBool { false });
                        isSlimeArea.SetValue(new NetBool { false });
                        isMonsterArea.SetValue(new NetBool { false });

                    }
                    else if (roll < dinoChance + slimeChance)
                    {
                        isDinoArea.SetValue(new NetBool { false });
                        isSlimeArea.SetValue(new NetBool { false });
                        isMonsterArea.SetValue(new NetBool { false });
                        mapImageSource.Value = "Maps\\Mines\\mine_dino";
                    }
                    else if (isSlimeArea.GetValue().Value || isDinoArea.GetValue().Value)
                    {
                        RevertMapImageSource(ref mapImageSource, ref loadedDarkArea, level, __instance.getMineArea(), __instance.getMineArea(level), __instance.loadedMapNumber);
                    }
                }
                else
                {
                    var chances = ModEntry.Config.SlimeMonsterSplitPercents.Split(':');
                    if (roll < int.Parse(chances[0]))
                    {
                        isDinoArea.SetValue(new NetBool { false });
                        isSlimeArea.SetValue(new NetBool { false });
                        isMonsterArea.SetValue(new NetBool { false });
                        mapImageSource.Value = "Maps\\Mines\\mine_slime";
                    }
                }
            }
        }
        else
        {
            isDinoArea.SetValue(new NetBool { false });
            isSlimeArea.SetValue(new NetBool { false });
            isMonsterArea.SetValue(new NetBool { false });
        }
    }

    private static bool IsBelowMinFloorsApart(int level)
    {
        if (ModEntry.Config.MinFloorsBetweenMonsterFloors <= 0)
            return false;

        foreach(var i in monsterFloors)
        {
            if(Math.Abs(level-i) <= ModEntry.Config.MinFloorsBetweenMonsterFloors){
                return true;
            }
        }
        return false;
    }

    private static void RevertMapImageSource(ref NetString mapImageSource, ref bool loadedDarkArea, int level, int mineAreaNeg, int mineAreaLevel, int mapNumberToLoad)
    {

        if (mineAreaNeg == 0 || mineAreaNeg == 10 || (mineAreaLevel != 0 && mineAreaLevel != 10))
        {
            if (mineAreaLevel == 40)
            {
                mapImageSource.Value = "Maps\\Mines\\mine_frost";
                if (level >= 70)
                {
                    var netString = mapImageSource;
                    netString.Value += "_dark";
                    loadedDarkArea = true;
                }
            }
            else if (mineAreaLevel == 80)
            {
                mapImageSource.Value = "Maps\\Mines\\mine_lava";
                if (level >= 110 && level != 120)
                {
                    var netString2 = mapImageSource;
                    netString2.Value += "_dark";
                    loadedDarkArea = true;
                }
            }
            else if (mineAreaLevel == 121)
            {
                mapImageSource.Value = "Maps\\Mines\\mine_desert";
                if (mapNumberToLoad % 40 >= 30)
                {
                    var netString3 = mapImageSource;
                    netString3.Value += "_dark";
                    loadedDarkArea = true;
                }
            }
            else
            {
                mapImageSource.Value = "Maps\\Mines\\mine";
            }
        }
        else
        {
            mapImageSource.Value = "Maps\\Mines\\mine";
        }
    }

    internal static void adjustLevelChances_Postfix(ref MineShaft __instance, ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
    {
        var isSlimeArea = Helper.Reflection.GetField<NetBool>(__instance, "netIsSlimeArea");
        var isDinoArea = Helper.Reflection.GetField<NetBool>(__instance, "netIsDinoArea");
        var isMonsterArea = Helper.Reflection.GetField<NetBool>(__instance, "netIsMonsterArea");
        if (isDinoArea.GetValue().Value)
        {
            monsterChance *= Config.MonsterMultiplierOnDinoFloors;
            itemChance *= Config.ItemMultiplierOnDinoFloors;
        }
        else if (isSlimeArea.GetValue().Value)
        {
            monsterChance *= Config.MonsterMultiplierOnSlimeFloors;
            itemChance *= Config.ItemMultiplierOnSlimeFloors;
        }
        else if (isMonsterArea.GetValue().Value)
        {
            monsterChance *= Config.MonsterMultiplierOnMonsterFloors;
            itemChance *= Config.ItemMultiplierOnMonsterFloors;
        }
        else
        {
            monsterChance *= Config.MonsterMultiplierOnRegularFloors;
            itemChance *= Config.ItemMultiplierOnRegularFloors;
            stoneChance *= Config.StoneMultiplierOnRegularFloors;
            gemStoneChance *= Config.GemstoneMultiplierOnRegularFloors;
        }
    }

    public static void chooseStoneType_Prefix(ref double chanceForPurpleStone, ref double chanceForMysticStone)
    {
        chanceForPurpleStone *= Config.PurpleStoneMultiplier;
        chanceForMysticStone *= Config.MysticStoneMultiplier;
    }

    internal static void chooseStoneType_Postfix(MineShaft __instance, ref Object __result, Vector2 tile)
    {
        if(__result == null)
        {
            return;
        }
        var ores = new List<int> { 765, 764, 290, 751 };
        var x = __result.ParentSheetIndex;
        if (!(x is >= 31 and <= 42 || x is >= 47 and <= 54 || ores.Contains(x)))
        {
            return;
        }

        if (__instance.getMineArea() == 0 || __instance.getMineArea() == 10)
        {
            var chanceForOre = 0.029 * Config.ChanceForOresMultiplierInMines - 0.029;

            if (__instance.mineLevel != 1 && __instance.mineLevel % 5 != 0 && Game1.random.NextDouble() < chanceForOre)
            {
                __result = new Object("751",1)
                {
                    TileLocation = tile,
                    MinutesUntilReady = 3
                };
            }
        }
        else if (__instance.getMineArea() == 40)
        {
            var chanceForOre = 0.029 * Config.ChanceForOresMultiplierInMines - 0.029;
            if (__instance.mineLevel % 5 != 0 && Game1.random.NextDouble() < chanceForOre)
            {
                __result = new Object("290", 1)
                {
                    TileLocation = tile,
                    MinutesUntilReady = 4
                };
            }
        }
        else if (__instance.getMineArea() == 80)
        {
            var chanceForOre = 0.029 * Config.ChanceForOresMultiplierInMines - 0.029;
            if (__instance.mineLevel % 5 != 0 && Game1.random.NextDouble() < chanceForOre)
            {
                __result = new Object("764", 1)
                {
                    TileLocation = tile,
                    MinutesUntilReady = 8
                };
            }
        }
        else if (__instance.getMineArea() == 77377)
        {
        }
        else
        {
            var skullCavernMineLevel = __instance.mineLevel - 120;
            var chanceForOre = 0.02 + skullCavernMineLevel * 0.0005;
            if (__instance.mineLevel >= 130)
            {
                chanceForOre += 0.01 * ((Math.Min(100, skullCavernMineLevel) - 10) / 10f);
            }
            var iridiumBoost = 0.0;
            if (__instance.mineLevel >= 130)
            {
                iridiumBoost += 0.001 * ((skullCavernMineLevel - 10) / 10f);
            }
            iridiumBoost = Math.Min(iridiumBoost, 0.004);
            if (skullCavernMineLevel > 100)
            {
                iridiumBoost += skullCavernMineLevel / 1000000.0;
            }

            chanceForOre = chanceForOre * Config.ChanceForOreMultiplier - chanceForOre;

            if (ores.Contains(x) || Game1.random.NextDouble() < chanceForOre) // if already an ore, don't check again
            {
                var chanceForIridium = Math.Min(100, skullCavernMineLevel) * (0.0003 + iridiumBoost);
                var chanceForGold = 0.01 + (__instance.mineLevel - Math.Min(150, skullCavernMineLevel)) * 0.0005;
                var chanceForIron = Math.Min(0.5, 0.1 + (__instance.mineLevel - Math.Min(200, skullCavernMineLevel)) * 0.005);

                chanceForIridium *= Config.ChanceForIridiumMultiplier;
                chanceForGold *= Config.ChanceForGoldMultiplier;
                chanceForIron *= Config.ChanceForIronMultiplier;

                if (Game1.random.NextDouble() < chanceForIridium)
                {
                    __result = new Object("765", 1)
                    {
                        TileLocation = tile,
                        MinutesUntilReady = 16
                    };
                }
                else if (Game1.random.NextDouble() < chanceForGold)
                {
                    __result = new Object("764", 1)
                    {
                        TileLocation = tile,
                        MinutesUntilReady = 8
                    };
                }
                else if (Game1.random.NextDouble() < chanceForIron)
                {
                    __result = new Object("290", 1)
                    {
                        TileLocation = tile,
                        MinutesUntilReady = 4
                    };
                }
                else
                {
                    __result = new Object("751", 1)
                    {
                        TileLocation = tile,
                        MinutesUntilReady = 2
                    };
                }
            }
        }

        /*
        if (!ores.Contains(__result.ParentSheetIndex))
        {
            foreach(CustomOreNode node in CustomOreNodeData)
            {

            }
        }
        */
    }


    internal static bool GotShaft;

    internal static void checkStoneForItems_Postfix(MineShaft __instance, string stoneId, int x, int y, Farmer who)
    {
        var createLadderDownEvent = Helper.Reflection.GetField<NetPointDictionary<bool, NetBool>>(__instance, "createLadderDownEvent").GetValue();
        var stonesLeftOnThisLevel = __instance.stonesLeftOnThisLevel;
        
        if(!createLadderDownEvent.ContainsKey(new Point(x, y)))
        {
            var chanceForLadderDown = 0.02 + 1.0 / Math.Max(1, stonesLeftOnThisLevel) + who.LuckLevel / 100.0 + Game1.player.DailyLuck / 5.0;
            if (__instance.EnemyCount == 0)
            {
                chanceForLadderDown += 0.04;
            }

            chanceForLadderDown = chanceForLadderDown * Config.ChanceForLadderInStoneMultiplier - chanceForLadderDown;

            if (!__instance.mustKillAllMonstersToAdvance() && (stonesLeftOnThisLevel == 0 || Game1.random.NextDouble() < chanceForLadderDown) && __instance.shouldCreateLadderOnThisLevel())
            {
                var isShaft = !GotShaft && __instance.getMineArea() == 121 && !__instance.mustKillAllMonstersToAdvance() && Game1.random.NextDouble() < 0.2 * Config.ChanceLadderIsShaftMultiplier;
                if(isShaft || !__instance.ladderHasSpawned)
                {
                    if (isShaft)
                        GotShaft = true;
                    createLadderDownEvent[new Point(x, y)] = isShaft;

                }
            }
        }
        else if(createLadderDownEvent[new Point(x, y)])
        {
            GotShaft = true;
        }
    }

    internal static IEnumerable<CodeInstruction> MineShaft_populateLevel_Transpiler(IEnumerable<CodeInstruction> instructions)
    {

        var codes = new List<CodeInstruction>(instructions);
        var start = false;
        for (var i = 0; i < codes.Count; i++)
        {
            if (start && codes[i].opcode == OpCodes.Ldc_R8 && codes[i].operand.Equals(0.005))
            {
                Log("got 0.005 opcode");
                codes[i].operand = Config.ResourceClumpChance; 
                break;
            }

            if (!start && codes[i].opcode == OpCodes.Call && (codes[i].operand as MethodInfo)?.Name == nameof(MineShaft.getRandomItemForThisLevel))
            {
                Log($"got call: {codes[i].operand}");
                start = true;
            }
        }

        return codes.AsEnumerable();
    }

    internal static IEnumerable<CodeInstruction> MineShaft_tryToAddAreaUniques_Transpiler(IEnumerable<CodeInstruction> instructions)
    {

        var codes = new List<CodeInstruction>(instructions);
        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldc_R8 && codes[i].operand.Equals(0.1))
            {
                Log("got Ldc_R8 opcode try");
                codes[i].operand = Config.WeedsChance;
            }
            else if (codes[i].opcode == OpCodes.Ldc_I4_7)
            {
                Log("got Ldc_I4_7 opcode try");
                codes[i] = new CodeInstruction(OpCodes.Ldc_I4, (int)Math.Round(7 * Config.WeedsMultiplier));
                codes[i + 1] = new CodeInstruction(OpCodes.Ldc_I4, (int)Math.Round(24 * Config.WeedsMultiplier));
                break;
            }
        }

        return codes.AsEnumerable();
    }
}
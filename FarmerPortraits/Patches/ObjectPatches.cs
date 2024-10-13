using FarmerPortraits.Framework;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace FarmerPortraits.Patches;

public class ObjectPatches
{
    private static ModConfig Config => ModEntry.Config;
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    private static int CurrentHeight { get; set; } = -1;
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV method \"Object.CheckForActionOnFarmComputer\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Object), "CheckForActionOnFarmComputer"),
            postfix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(Post_CheckForActionOnFarmComputer))
        );
    }

    internal static void Post_CheckForActionOnFarmComputer(Farmer who, bool __result,
        bool justCheckingForActivity = false)
    {
        if (justCheckingForActivity)
            return;
        
        Data.ShouldResize = __result;
    }
}
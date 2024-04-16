using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace FarmerPortraits.Patches;

public class EventPatches
{
    internal static string FestivalId = null;
    #if DEBUG
    private static LogLevel Level =>  LogLevel.Debug;
    #else
    private static LogLevel Level =>  LogLevel.Trace;
    #endif

    private static IModHelper Help => ModEntry.SHelper;
    private static void Log(string msg, LogLevel lv = LogLevel.Trace) => ModEntry.SMonitor.Log(msg, lv);

    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(EventPatches)}\": postfixing SDV method \"Event.endBehaviors\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Event), nameof(Event.endBehaviors), new[]{typeof(GameLocation)}),
            postfix: new HarmonyMethod(typeof(EventPatches), nameof(Post_endBehaviors))
        );
        
        Log($"Applying Harmony patch \"{nameof(EventPatches)}\": postfixing SDV method \"Event.tryToLoadFestival\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(Event), nameof(Event.tryToLoadFestival)),
            postfix: new HarmonyMethod(typeof(EventPatches), nameof(Post_tryToLoadFestival))
        );
    }
    
    private static void Post_endBehaviors(GameLocation location = null)
    {
        FestivalId = null;
    }
    
    public static void Post_tryToLoadFestival(string festival, Event ev, bool __result)
    {
        if (__result == false || ev == null)
            return;
#if DEBUG
        Log($"Festival id: {festival}, name {ev.FestivalName}, id {ev.id}", LogLevel.Info);
#endif
        if(string.IsNullOrWhiteSpace(festival))
            return;

        FestivalId = festival;

        Help.GameContent.InvalidateCache("aedenthorn.FarmerPortraits/portrait");
        for (var i = 0; i < 12; i++)
        {
            if (i == 0)
            {
                Help.GameContent.InvalidateCache("aedenthorn.FarmerPortraits/portrait");
                continue;
            }
            
            try
            {
                Help.GameContent.InvalidateCache($"aedenthorn.FarmerPortraits/portrait{i}");
            }
            catch (Exception)
            {
                Log($"Portrait emotion #{i} not found. Skipping");
            }
        }
    }
}
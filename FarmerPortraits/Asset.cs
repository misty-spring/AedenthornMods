using System.Collections.Generic;
using System.Linq;
using FarmerPortraits.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FarmerPortraits;

public static class Asset
{
    private static ModConfig Config => ModEntry.Config;
    internal static void OnRequest(object sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.BaseName.Equals("aedenthorn.FarmerPortraits/ignore_lines"))
             e.LoadFrom(() => new List<string>(), AssetLoadPriority.Low);
        
        if (e.NameWithoutLocale.BaseName.Equals("aedenthorn.FarmerPortraits/reactions") == false)
            return;
        
        e.LoadFrom(() => new Dictionary<string, Dictionary<int, int>>
        {
            {
                "Default",
                new()
                {
                    { 0, Config.Reaction0 }, { 1, Config.Reaction1 }, { 2, Config.Reaction2 },
                    { 3, Config.Reaction3 }, { 4, Config.Reaction4 }, { 5, Config.Reaction5 }
                }
            }
        }, AssetLoadPriority.Low);
    }

    public static void OnInvalidate(object sender, AssetsInvalidatedEventArgs e)
    {
        if (Context.IsWorldReady == false)
            return;

        if (e.NamesWithoutLocale.Any(name => name.BaseName == "aedenthorn.FarmerPortraits/ignore_lines"))
            Data.IgnoreLines = ModEntry.Help.GameContent.Load<List<string>>("aedenthorn.FarmerPortraits/ignore_lines");
        
        if (e.NamesWithoutLocale.Any(name => name.BaseName == "aedenthorn.FarmerPortraits/reactions"))
        {
            Data.Reactions = ModEntry.Help.GameContent.Load<Dictionary<string, Dictionary<int, int>>>("aedenthorn.FarmerPortraits/reactions");
        }
        
        if (e.NamesWithoutLocale.Any( name => name.StartsWith("aedenthorn.FarmerPortraits/portrait") || name.StartsWith("aedenthorn.FarmerPortraits/data")))
        {
            ModEntry.ReloadData();
        }
    }
}
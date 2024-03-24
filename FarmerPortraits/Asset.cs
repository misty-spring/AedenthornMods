using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FarmerPortraits;

public static class Asset
{
    private static ModConfig Config => ModEntry.Config;
    internal static void OnRequest(object sender, AssetRequestedEventArgs e)
    {
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
        
        if (e.NamesWithoutLocale.Any( name => name.BaseName == "aedenthorn.FarmerPortraits/reactions") == false)
            return;

        ModEntry.Reactions = ModEntry.SHelper.GameContent.Load<Dictionary<string, Dictionary<int, int>>>("aedenthorn.FarmerPortraits/reactions");
    }
}
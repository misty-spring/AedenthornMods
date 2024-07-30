using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace FarmerPortraits.Framework;

public static class Data
{
    internal const int Distance = 460;
    internal static int CurrentFarmerEmotion { get; set; }
    
    internal static Texture2D PortraitTexture;
    internal static Texture2D BackgroundTexture;

    internal static Dictionary<string, Dictionary<int, int>> Reactions { get; set; } = new();
}
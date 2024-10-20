using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace FarmerPortraits.Framework;

public static class Data
{
    internal static int TimesCalled { get; set; }
    internal static bool ShouldResize { get; set; } = false;
    internal static bool IgnoreCurrent { get; set; } = false;
    internal static bool HasChangingSkies { get; set; }
    internal static bool HasCPDDFAdvanced { get; set; }
    internal static int DividerWidth { get; set; }
    internal const int Distance = 460;
    internal static int CurrentFarmerEmotion { get; set; }
    
    public static Texture2D PortraitTexture { get; internal set; }
    public static Texture2D BackgroundTexture { get; internal set; }
    public static float PortraitScale { get; internal set; } = 4f;
    public static float BackgroundScale { get; internal set; } = 4f;

    internal static List<string> IgnoreLines { get; set; } = new();
    internal static Dictionary<string, Dictionary<int, int>> Reactions { get; set; } = new();
}
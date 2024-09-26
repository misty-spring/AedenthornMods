using System;
using FarmerPortraits.APIs;
using FarmerPortraits.Framework;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using static FarmerPortraits.Framework.Data;
using static FarmerPortraits.Framework.Methods;

// ReSharper disable PossibleLossOfFraction

namespace FarmerPortraits.Patches;

public class DialogueBoxPatches
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
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV method \"DialogueBox.setUpIcons\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.drawBox)),
            postfix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(Post_drawBox))
        );
        
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV method \"DialogueBox.setUpIcons\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(DialogueBox), "setUpIcons"),
            prefix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(Pre_setUpIcons))
        );
        
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV constructor \"DialogueBox(Dialogue)\".");
        harmony.Patch(
            original: AccessTools.Constructor(typeof(DialogueBox), new[] { typeof(Dialogue)}),
            postfix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(Post_new))
        );
        
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV method \"DialogueBox.closeDialogue\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(DialogueBox), "closeDialogue"),
            prefix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(Pre_closeDialogue))
        );
        
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV method \"DialogueBox.closeDialogue\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(DialogueBox), "receiveLeftClick"),
            prefix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(Pre_receiveLeftClick))
        ); //only re-check on left click, which should also remove the key
    }
    
    public static void Pre_setUpIcons(DialogueBox __instance)
    {
        try
        {
            if(!ShouldShow(ref __instance))
                return;
            
            AdjustWindow(ref __instance);
            
            if (!Config.PortraitReactions)
                return;

            var currentEmotion = __instance.characterDialogue?.getPortraitIndex();
            if (currentEmotion.HasValue == false)
                return;
            
            //fix unique reaction if datable NPC
            if (__instance.characterDialogue.speaker.datable.Value)
            {
                //for datables, $3 is unique. so tell the mod they're doing the (supposedly) unique reaction
                if(currentEmotion == 3)
                    currentEmotion = 5;
                
                //& vice versa
                if(currentEmotion == 5)
                    currentEmotion = 3;
            }

            var name = __instance.characterDialogue?.speaker.Name ?? "Default";

#if DEBUG
            Log($"Speaker: {name}");
#endif

            if (!Reactions.ContainsKey(name))
            {
                Log($"{name} not in custom reactions. Using default...");
                name = "Default";
            }

            if (Reactions.TryGetValue(name, out var reactions) &&
                reactions.TryGetValue((int)currentEmotion, out var farmerEmotion))
            {
                CurrentFarmerEmotion = farmerEmotion;
                
                //-1 means disabled
                if (CurrentFarmerEmotion == -1)
                {
                    PortraitTexture = null;
                    ResizeWindow(ref __instance);
                    return;
                }
#if DEBUG
                Log($"Using emotion {CurrentFarmerEmotion}");
#endif
                try
                {
                    var emotion = CurrentFarmerEmotion == 0 ? "" : $"{CurrentFarmerEmotion}";
                    
                    if(Game1.content.DoesAssetExist<Texture2D>($"aedenthorn.FarmerPortraits/portrait{emotion}"))
                        PortraitTexture = ModEntry.Help.GameContent.Load<Texture2D>($"aedenthorn.FarmerPortraits/portrait{emotion}");
                }
                catch (Exception)
                {
                    PortraitTexture = null;
                }
            }
        }
        catch (Exception e)
        {
            Log($"Error: {e}");
        }
    }
    
    public static void Post_drawBox(DialogueBox __instance, SpriteBatch b)
    {
        try
        {
            if(!ShouldShow(ref __instance))
                return;

            if (CurrentFarmerEmotion == -1)
                return;
            
            const int boxHeight = 384;
            DrawBox(b, __instance.x - 448 - 32, __instance.y + __instance.height - boxHeight, 448, boxHeight);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }
    
    public static void Post_new(DialogueBox __instance)
    {
        try
        {
            if(!ShouldShow(ref __instance))
                return;
            
            AdjustWindow(ref __instance);
            DividerWidth = (int)(__instance.width * 0.2);
        }
        catch (Exception e)
        {
            Log($"Error: {e}", LogLevel.Error);
        }
    }

    internal static void Pre_closeDialogue()
    {
        #if DEBUG
        Log("closing!!!");
#endif
        if(IgnoreAll)
            IgnoreAll = false;
        
        if(IgnoreCurrent)
            IgnoreCurrent = false;
    }

    private static void Pre_receiveLeftClick(ref DialogueBox __instance, int x, int y, bool playSound = true)
    {
        if(IgnoreAll)
            IgnoreAll = false;
        
        if(IgnoreCurrent)
            IgnoreCurrent = false;

        try
        {
            ShouldIgnore(ref __instance, 1);
        }
        catch (Exception e)
        {
            ModEntry.Mon.VerboseLog("No next line found.");
        }
    }
}
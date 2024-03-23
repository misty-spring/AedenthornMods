using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Tools;

namespace FarmerPortraits.Patches;

public class DialogueBoxPatches
{
    private static Texture2D Background => ModEntry.backgroundTexture;
    private static Texture2D Portrait => ModEntry.portraitTexture;
    private static ModConfig Config => ModEntry.Config;
    internal static bool Done { get; set; }
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.SMonitor.Log(msg, lv);
    internal static void Apply(Harmony harmony)
    {
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV method \"DialogueBox.setUpIcons\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.drawBox)),
            postfix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(DialogueBoxPatches.Post_drawBox))
        );
        
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV method \"DialogueBox.setUpIcons\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(DialogueBox), "setUpIcons"),
            prefix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(Pre_setUpIcons))
        );
        
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV constructor \"DialogueBox(Dialogue)\".");
        harmony.Patch(
            original: AccessTools.Constructor(typeof(DialogueBox), new Type[] { typeof(Dialogue)}),
            postfix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(Post_new))
        );
        
        Log($"Applying Harmony patch \"{nameof(DialogueBoxPatches)}\": postfixing SDV method \"DialogueBox.closeDialogue\".");
        harmony.Patch(
            original: AccessTools.Method(typeof(DialogueBox), nameof(DialogueBox.closeDialogue)),
            postfix: new HarmonyMethod(typeof(DialogueBoxPatches), nameof(Post_closeDialogue))
        );
    }
    
    public static void Pre_setUpIcons(DialogueBox __instance)
    {
        if (!Config.EnableMod || !__instance.transitionInitialized || __instance.transitioning || (!Config.ShowWithQuestions && __instance.isQuestion) || (!Config.ShowWithNpcPortrait && __instance.isPortraitBox()) || (!Config.ShowWithEvents && Game1.eventUp) || (!Config.ShowMisc && !__instance.isQuestion && !__instance.isPortraitBox()))
            return;
        AdjustWindow(ref __instance);
        
        if (__instance.characterDialogue.speaker is null)
            return;

        if (!Config.PortraitReactions) 
            return;
        
        var currentEmotion = __instance.characterDialogue?.getPortraitIndex();
        if (currentEmotion.HasValue == false)
            return;
            
        var name = __instance.characterDialogue?.speaker.Name ?? "Default";
            
        #if DEBUG
        Log($"Speaker: {name}");
        #endif

        if (!ModEntry.Reactions.ContainsKey(name))
        {
            Log($"{name} not in custom reactions. Using default...");
            name = "Default";
        }
        
        if(ModEntry.Reactions.TryGetValue(name, out var reactions) && reactions.TryGetValue((int)currentEmotion, out var farmerEmotion))
        {
#if DEBUG
            Log($"Using emotion {farmerEmotion}");
#endif
            try
            {
                var p = ModEntry.SHelper.GameContent.Load<Texture2D>($"aedenthorn.FarmerPortraits/portrait{farmerEmotion}");
                
                //if no portrait for that emotion, make it jump to default
                if (p is null)
                    throw new NullReferenceException();
                
                ModEntry.portraitTexture = p;
            }
            catch (Exception)
            {
                ModEntry.portraitTexture = ModEntry.SHelper.GameContent.Load<Texture2D>($"aedenthorn.FarmerPortraits/portrait");
            }
        }
    }
    
    public static void Post_drawBox(DialogueBox __instance, SpriteBatch b)
    {
        if (!Config.EnableMod || !__instance.transitionInitialized ||  __instance.transitioning || (!Config.ShowWithQuestions && __instance.isQuestion) ||  (!Config.ShowWithNpcPortrait && __instance.isPortraitBox()) || (!Config.ShowWithEvents && Game1.eventUp) || (!Config.ShowMisc && !__instance.isQuestion && !__instance.isPortraitBox()))
            return;
        const int boxHeight = 384;
        DrawBox(b, __instance.x - 448 - 32, __instance.y + __instance.height - boxHeight, 448, boxHeight);
    }

    private static void DrawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
    {
        b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle?(new Rectangle(275, 313, 1, 6)), Color.White);
        b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle?(new Rectangle(275, 328, 1, 8)), Color.White);
        b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle?(new Rectangle(264, 325, 8, 1)), Color.White);
        b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle?(new Rectangle(261, 311, 14, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle?(new Rectangle(261, 327, 14, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle?(new Rectangle(293, 324, 7, 1)), Color.White);
        b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle?(new Rectangle(291, 311, 12, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle?(new Rectangle(291, 326, 12, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);

        if (Background != null && Config.UseCustomBackground)
            b.Draw(Background, new Rectangle(xPos - 4, yPos, boxWidth + 12, boxHeight + 4), null, Color.White);
        else
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 4, yPos), new Rectangle?(new Rectangle(583, 411, 115, 97)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f); // background

        var portraitBoxX = xPos + 76;
        var portraitBoxY = yPos + boxHeight / 2 - 148 - 36;
        var frame = Config.FacingFront ? 0 : 6;
        if (Portrait != null && Config.UseCustomPortrait)
        {
            b.Draw(Portrait, new Rectangle(portraitBoxX + 20, portraitBoxY + 24, 256, 256), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.88f);
        }
        else
        {
            FarmerRenderer.isDrawingForUI = true;
            DrawFarmer(b, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(xPos + boxWidth / 2 - 128, yPos + boxHeight / 2 - 208), Color.White);
            if (Game1.timeOfDay >= 1900)
            {
                DrawFarmer(b, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(xPos + boxWidth / 2 - 128, yPos + boxHeight / 2 - 192), Color.DarkBlue * 0.3f);
            }
            FarmerRenderer.isDrawingForUI = false;
        }
        SpriteText.drawStringHorizontallyCenteredAt(b, Game1.player.Name, xPos + boxWidth / 2, portraitBoxY + 296 + 16);
    }

    private static void DrawFarmer(SpriteBatch b, int currentFrame, Rectangle sourceRect, Vector2 position, Color overrideColor)
    {
        var animationFrame = new FarmerSprite.AnimationFrame(Game1.player.bathingClothes.Value ? 108 : currentFrame, 0, false, false, null, false);
        var who = Game1.player;
        var layerDepth = 0.8f;
        var scale = 4f;

        AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions").Invoke(Game1.player.FarmerRenderer, new object[] { who });

        position = new Vector2((float)Math.Floor(position.X), (float)Math.Floor(position.Y));

        var positionOffset = new Vector2(animationFrame.positionOffset * 4, animationFrame.positionOffset * 4);

        var baseTexture = AccessTools.FieldRefAccess<FarmerRenderer, Texture2D>(Game1.player.FarmerRenderer, "baseTexture");

        // body
        b.Draw(baseTexture, position + positionOffset, sourceRect, overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f);
				
        // eyes
        sourceRect.Offset(288, 0);
        if (who.currentEyes != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)))
        {
            if (!who.UsingTool || who.CurrentTool is not FishingRod f || f.isFishing)
            {
                var xAdjustment = 5 - FarmerRenderer.featureXOffsetPerFrame[currentFrame];
                if (!Config.FacingFront)
                {
                    xAdjustment += 3;
                }
                xAdjustment *= 4;
                b.Draw(baseTexture, position + positionOffset + new Vector2(xAdjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && !Config.FacingFront) ? 36 : 40)) * scale, new Rectangle?(new Rectangle(5, 16, Config.FacingFront ? 6 : 2, 2)), overrideColor, 0f, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 5E-08f);
                b.Draw(baseTexture, position + positionOffset + new Vector2(xAdjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (!Config.FacingFront ? 40 : 44)) * scale, new Rectangle?(new Rectangle(264 + 0, 2 + (who.currentEyes - 1) * 2, Config.FacingFront ? 6 : 2, 2)), overrideColor, 0f, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 1.2E-07f);
            }
        }

        // hair and accessories

        var hairStyle = who.getHair(false);
        var hairMetadata = Farmer.GetHairStyleMetadata(who.hair.Value);
        if (who != null && who.hat.Value != null && who.hat.Value.hairDrawType.Value == 1 && hairMetadata != null && hairMetadata.coveredIndex != -1)
        {
            hairStyle = hairMetadata.coveredIndex;
            hairMetadata = Farmer.GetHairStyleMetadata(hairStyle);
        }
        AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions").Invoke(Game1.player.FarmerRenderer, new object[] { who });

        const int hatCutoff = 4;
        const int shirtCutoff = 4;
        //old: var shirtSourceRect = new Rectangle(Game1.player.FarmerRenderer.ClampShirt(who.GetShirtIndex()) * 8 % 128, Game1.player.FarmerRenderer.ClampShirt(who.GetShirtIndex()) * 8 / 128 * 32, 8, 8 - shirtCutoff);
        //                 
        Game1.player.GetDisplayShirt(out _, out var spriteIndex1);
        var shirtSourceRect = new Rectangle( spriteIndex1 * 8 % 128, spriteIndex1 * 8 / 128 * 32, 8, 8 - shirtCutoff);
        var hairTexture = FarmerRenderer.hairStylesTexture;
        //old: var hairstyleSourceRect = new Rectangle(hair_style * 16 % FarmerRenderer.hairStylesTexture.Width, hair_style * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32);
        var hairstyleSourceRect = new Rectangle(hairStyle * 16 % FarmerRenderer.hairStylesTexture.Width, hairStyle * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32);
        var hatSourceRect = who.hat.Value != null ? new Rectangle(20 * who.hat.Value.ParentSheetIndex % FarmerRenderer.hatsTexture.Width, 20 * who.hat.Value.ParentSheetIndex / FarmerRenderer.hatsTexture.Width * 20 * 4 + hatCutoff, 20, 20 - hatCutoff) : new Rectangle();
        var accessorySourceRect = who.accessory.Value >= 0 ? new Rectangle(who.accessory.Value * 16 % FarmerRenderer.accessoriesTexture.Width, who.accessory.Value * 16 / FarmerRenderer.accessoriesTexture.Width * 32, 16, 16) : new Rectangle();

        if (hairMetadata != null)
        {
            hairTexture = hairMetadata.texture;
            hairstyleSourceRect = new Rectangle(hairMetadata.tileX * 16, hairMetadata.tileY * 16, 16, 32);
        }
        // ReSharper disable once RedundantAssignment
        var dyedShirtSourceRect = shirtSourceRect;
        const float dyeLayerOffset = 1E-07f;
        const float hairDrawLayer = 2.2E-05f;
        const int heightOffset = 0;

        if (Config.FacingFront)
        {
            dyedShirtSourceRect = shirtSourceRect;
            dyedShirtSourceRect.Offset(128, 0);

            // shirt

            if (!who.bathingClothes.Value)
            {
                b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (float)heightOffset * 4 - (who.IsMale ? 0 : 0)) * scale, new Rectangle?(shirtSourceRect), overrideColor.Equals(Color.White) ? Color.White : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 1.5E-07f);
                b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (float)heightOffset * 4 - (who.IsMale ? 0 : 0)), new Rectangle?(dyedShirtSourceRect), overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 1.5E-07f + dyeLayerOffset);
            }

            // accessory

            if (who.accessory.Value >= 0)
            {
                b.Draw(FarmerRenderer.accessoriesTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset - 4), new Rectangle?(accessorySourceRect), (overrideColor.Equals(Color.White) && who.accessory.Value < 6) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, 0.8f + ((who.accessory.Value < 8) ? 1.9E-05f : 2.9E-05f));
            }

            // hair

            b.Draw(hairTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0))) * scale, new Rectangle?(hairstyleSourceRect), overrideColor.Equals(Color.White) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, 0.8f + hairDrawLayer);
        }
        else
        {

            shirtSourceRect.Offset(0, 8);
            hairstyleSourceRect.Offset(0, 32);
            dyedShirtSourceRect = shirtSourceRect;
            dyedShirtSourceRect.Offset(128, 0);
            if (who.accessory.Value >= 0)
            {
                accessorySourceRect.Offset(0, 16);
            }
            if (who.hat.Value != null)
            {
                hatSourceRect.Offset(0, 20);
            }

            // shirt

            if (!who.bathingClothes.Value)
            {
                b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16f + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56f + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset) * scale, new Rectangle?(shirtSourceRect), overrideColor.Equals(Color.White) ? Color.White : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f);
                b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16f + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56f + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset) * scale, new Rectangle?(dyedShirtSourceRect), overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f + dyeLayerOffset);
            }

            // accessory

            if (who.accessory.Value >= 0)
            {
                b.Draw(FarmerRenderer.accessoriesTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset) * scale, new Rectangle?(accessorySourceRect), (overrideColor.Equals(Color.White) && who.accessory.Value < 6) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + ((who.accessory.Value < 8) ? 1.9E-05f : 2.9E-05f));
            }

            // hair

            b.Draw(hairTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0))) * scale, new Rectangle?(hairstyleSourceRect), overrideColor.Equals(Color.White) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, layerDepth + hairDrawLayer);
        }

        // hat

        if (who.hat.Value != null && !who.bathingClothes.Value)
        {
            const float layerOffset = 3.9E-05f;
            b.Draw(FarmerRenderer.hatsTexture, position + positionOffset * scale + new Vector2(-9 * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4 - 8, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (who.hat.Value.ignoreHairstyleOffset.Value ? 0 : FarmerRenderer.hairstyleHatOffset[who.hair.Value % 16]) + 8 + heightOffset + 4 * hatCutoff) * scale, new Rectangle?(hatSourceRect), who.hat.Value.isPrismatic.Value ? Utility.GetPrismaticColor(0, 1f) : Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + layerOffset);
        }
        const float armLayerOffset = 4.9E-05f;
        sourceRect.Offset(-288 + (animationFrame.armOffset == 12 ? 192 : 96), 0);
        b.Draw(baseTexture, position + positionOffset + who.armOffset, new Rectangle?(sourceRect), overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + armLayerOffset);
    }
    
    public static void Post_new(DialogueBox __instance)
    {
        if (!Config.EnableMod || !__instance.transitionInitialized || __instance.transitioning || (!Config.ShowWithQuestions && __instance.isQuestion) || (!Config.ShowWithNpcPortrait && __instance.isPortraitBox()) || (!Config.ShowWithEvents && Game1.eventUp) || (!Config.ShowMisc && !__instance.isQuestion && !__instance.isPortraitBox()))
            return;
        AdjustWindow(ref __instance);
    }

    private static void AdjustText(ref DialogueBox box)
    {
        if (Done)
            return;
        
        if (box.characterDialogue is null)
            return;
        
        //after a few hours of making the most convoluted code ever, i found out re-setting the dialogue auto adjusts the text.
        var originalDialogues = box.characterDialogue.dialogues;
        box.characterDialogue.dialogues = originalDialogues;
        Done = true;
        
#if DEBUG
        Log(box.characterDialogue.speaker.displayName + ':');
        foreach (var line in box.characterDialogue.dialogues)
        {
            Log(line.Text);
        }
#endif
    }

    private static void AdjustWindow(ref DialogueBox __instance)
    {
        __instance.x = Math.Max(520, (int)Utility.getTopLeftPositionForCenteringOnScreen(__instance.width, __instance.height, 0, 0).X + 260);
        __instance.width = (int)Math.Min(Game1.uiViewport.Width - __instance.x - 48, 1200);
        __instance.friendshipJewel = new Rectangle(__instance.x + __instance.width - 64, __instance.y + 256, 44, 44);

        //adjust
        AdjustText(ref __instance);
    }

    private static void Post_closeDialogue(DialogueBox __instance)
    {
        #if DEBUG
        Log("Closing");
        #endif
        Done = __instance.characterDialogue.dialogues.Count > 1;
    }
}
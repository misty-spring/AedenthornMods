using System;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Tools;
using static FarmerPortraits.Framework.Data;

namespace FarmerPortraits.Framework;

public static class Methods
{
    private static ModConfig Config => ModEntry.Config;
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif
    private static void Log(string msg, LogLevel lv = Level) => ModEntry.Mon.Log(msg, lv);
    
    internal static bool ShouldShow(ref DialogueBox box, bool checkAgain = false)
    {
        if (!Config.EnableMod || !box.transitionInitialized || box.transitioning ||
                (!Config.ShowWithQuestions && box.isQuestion) ||
                (!Config.ShowWithNpcPortrait && box.isPortraitBox()) ||
                (!Config.ShowWithEvents && Game1.eventUp) ||
                (!Config.ShowMisc && !box.isQuestion && !box.isPortraitBox()))
                return false;

        if (box.characterDialogue?.speaker is null && box.isPortraitBox())
            return false;

        #if DEBUG
        Log($"LINE: {box.getCurrentString()}");
        Log($"or '{box.characterDialogue?.dialogues[0]}'");
        #endif

        if (IgnoreCurrent && !checkAgain)
            return false;

        if (ShouldIgnore(ref box))
            return false;
        
        return true;
    }

    internal static bool ShouldIgnore(ref DialogueBox box, int num = 0)
    {
        IgnoreCurrent = false;
        
        #if DEBUG
        var debugText = $"DEBUG: {(box.characterDialogue?.dialogues?[num] != null ? box.characterDialogue.dialogues[num] : box.dialogues?[num])}";
        Log(box.dialogues?.Count + $" - {box.dialogues?[0]}, {box.dialogues?[1]}");
        Log($"{box.characterDialogue?.dialogues?.Count ?? 0}, {box.characterDialogue?.dialogues?[0]}");
        #endif
        
        if (box.getCurrentString().StartsWith("$show_player ", StringComparison.OrdinalIgnoreCase))
        {
            //fix string
            if(box.dialogues?.Count > num)
            {
                var text = box.dialogues[num]?.Remove(0, 13);
                box.dialogues[num] = text;
            }

            if (box.characterDialogue?.dialogues?.Count > num)
            {
                var characterText = box.characterDialogue.dialogues[num]?.Text.Remove(0, 13);
                box.characterDialogue.dialogues[num].Text = characterText;
            }

            IgnoreCurrent = false;
#if DEBUG
            Log(debugText);
#endif
            return false;
        }
        
        if (box.getCurrentString().StartsWith("$no_player ", StringComparison.OrdinalIgnoreCase))
        {
            //fix string
            if(box.dialogues?.Count > num)
            {
                var text = box.dialogues[num]?.Remove(0, 11);
                box.dialogues[num] = text;
            }

            if (box.characterDialogue?.dialogues?.Count > num)
            {
                var characterText = box.characterDialogue.dialogues[num]?.Text.Remove(0, 11);
                box.characterDialogue.dialogues[num].Text = characterText;
            }

            IgnoreCurrent = true;
            //ResizeWindow(ref box);
#if DEBUG
            Log(debugText);
#endif
            return true;
        }

        var testText = "";
        if (box.dialogues?.Count > num)
            testText = box.dialogues?[num];
        else if (box.characterDialogue?.dialogues?.Count > num)
            testText = box.characterDialogue?.dialogues[num]?.Text;
        testText ??= "";
        
        #if DEBUG
        Log(testText, LogLevel.Info);
        #endif

        if (IgnoreLines.Contains(testText))
        {
            IgnoreCurrent = true;
            return true;
        }

        return false;
    }

    internal static void DrawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
    {
        b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle(275, 313, 1, 6), Color.White);
        b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle(275, 328, 1, 8), Color.White);
        b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White);
        b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White);
        b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);

        //return;
        
        if (BackgroundTexture != null && Config.UseCustomBackground)
            b.Draw(BackgroundTexture, new Rectangle(xPos - 4, yPos, boxWidth + 12, boxHeight + 4), null, Color.White);
        else
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 4, yPos), new Rectangle(583, 411, 115, 97), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f); // background

        var portraitBoxX = xPos + 76;
        var portraitBoxY = yPos + boxHeight / 2 - 148 - 36;
        var frame = Config.FacingFront ? 0 : 6;
        if (PortraitTexture != null && Config.UseCustomPortrait)
        {
            b.Draw(PortraitTexture, new Rectangle(portraitBoxX + 20, portraitBoxY + 28, 256, 256), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.88f);
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

    internal static void DrawFarmer(SpriteBatch b, int currentFrame, Rectangle sourceRect, Vector2 position, Color overrideColor)
    {
        var animationFrame = new FarmerSprite.AnimationFrame(Game1.player.bathingClothes.Value ? 108 : currentFrame, 0, false, false);
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
                b.Draw(baseTexture, position + positionOffset + new Vector2(xAdjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && !Config.FacingFront) ? 36 : 40)) * scale, new Rectangle(5, 16, Config.FacingFront ? 6 : 2, 2), overrideColor, 0f, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 5E-08f);
                b.Draw(baseTexture, position + positionOffset + new Vector2(xAdjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (!Config.FacingFront ? 40 : 44)) * scale, new Rectangle(264 + 0, 2 + (who.currentEyes - 1) * 2, Config.FacingFront ? 6 : 2, 2), overrideColor, 0f, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 1.2E-07f);
            }
        }

        // hair and accessories

        var hairStyle = who.getHair();
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
                b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (float)heightOffset * 4 - 0) * scale, shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 1.5E-07f);
                b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (float)heightOffset * 4 - 0), dyedShirtSourceRect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 1.5E-07f + dyeLayerOffset);
            }

            // accessory

            if (who.accessory.Value >= 0)
            {
                b.Draw(FarmerRenderer.accessoriesTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset - 4), accessorySourceRect, (overrideColor.Equals(Color.White) && who.accessory.Value < 6) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, 0.8f + ((who.accessory.Value < 8) ? 1.9E-05f : 2.9E-05f));
            }

            // hair

            b.Draw(hairTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0))) * scale, hairstyleSourceRect, overrideColor.Equals(Color.White) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, 0.8f + hairDrawLayer);
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
                b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16f + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56f + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset) * scale, shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f);
                b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16f + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56f + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset) * scale, dyedShirtSourceRect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f + dyeLayerOffset);
            }

            // accessory

            if (who.accessory.Value >= 0)
            {
                b.Draw(FarmerRenderer.accessoriesTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset) * scale, accessorySourceRect, (overrideColor.Equals(Color.White) && who.accessory.Value < 6) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + ((who.accessory.Value < 8) ? 1.9E-05f : 2.9E-05f));
            }

            // hair

            b.Draw(hairTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0))) * scale, hairstyleSourceRect, overrideColor.Equals(Color.White) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, layerDepth + hairDrawLayer);
        }

        // hat

        if (who.hat.Value != null && !who.bathingClothes.Value)
        {
            const float layerOffset = 3.9E-05f;
            b.Draw(FarmerRenderer.hatsTexture, position + positionOffset * scale + new Vector2(-9 * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4 - 8, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (who.hat.Value.ignoreHairstyleOffset.Value ? 0 : FarmerRenderer.hairstyleHatOffset[who.hair.Value % 16]) + 8 + heightOffset + 4 * hatCutoff) * scale, hatSourceRect, who.hat.Value.isPrismatic.Value ? Utility.GetPrismaticColor() : Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + layerOffset);
        }
        const float armLayerOffset = 4.9E-05f;
        sourceRect.Offset(-288 + (animationFrame.armOffset == 12 ? 192 : 96), 0);
        b.Draw(baseTexture, position + positionOffset + who.armOffset, sourceRect, overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + armLayerOffset);
    }
    
    internal static void AdjustText(ref DialogueBox box)
    {
        if (!Config.FixText)
            return;
        
        if (box.characterDialogue is null)
            return;

        var d = box.characterDialogue.dialogues[box.characterDialogue.currentDialogueIndex].Text; //box.characterDialogue.getCurrentDialogue();
               
        var realWidth = box.width - 384;
        var size = SpriteText.getHeightOfString(d, realWidth);
#if DEBUG
        Log($"Y: {size} (x2 {size * 2}), height: {box.height} & width: {realWidth}");
        Log(d);
#endif
        if (size * 2 <= box.height)
        {
            return;
        }
        
        StringBuilder original = new("");
        StringBuilder addition = new("");
        var g = ArgUtility.SplitBySpace(d);
        var count = 0;
        var limit = box.height - 70;
        foreach (var str in g)
        {
            if (count < limit)
            {
                original.Append(str);
                original.Append(' ');
            }
            else
            {
                addition.Append(str);
                addition.Append(' ');
            }
       
            count = SpriteText.getHeightOfString(original.ToString(), realWidth);
        }

        box.characterDialogue.dialogues[box.characterDialogue.currentDialogueIndex].Text = original.ToString();
        if(addition.Length > 0)
            box.characterDialogue.dialogues.Insert(box.characterDialogue.currentDialogueIndex + 1, new DialogueLine(addition.ToString()));
#if DEBUG
        foreach (var line in box.characterDialogue.dialogues)
        {
            Log(line.Text);
        }
#endif
    }

    internal static void AdjustWindow(ref DialogueBox box)
    {
        if (HasCPDDFAdvanced)
            return;
        
        box.x = Math.Max(520, (int)Utility.getTopLeftPositionForCenteringOnScreen(box.width, box.height).X + 260);
        box.width = Math.Min(Game1.uiViewport.Width - box.x - 48, 1200);
        box.friendshipJewel = new Rectangle(box.x + box.width - 64, box.y + 256, 44, 44);
        
        //adjust
        AdjustText(ref box);

        if (Config.ShowMisc && !box.isQuestion && !box.isPortraitBox())
        {
            box.height = 384;
            box.y = Game1.uiViewport.Height - box.height - 64;
        }
    }
    
    internal static void ResizeWindow(ref DialogueBox box)
    {
        if (HasCPDDFAdvanced)
            return;
        
        box.x = 76; //(int)Utility.getTopLeftPositionForCenteringOnScreen(box.width, box.height).X;
        box.xPositionOnScreen = box.x;
        box.width = 1200;
        box.friendshipJewel = new Rectangle(box.x + box.width - 64, box.y + 256, 44, 44);
        
        //adjust
        AdjustText(ref box);

        if (Config.ShowMisc && !box.isQuestion && !box.isPortraitBox())
        {
            box.height = 384;
            box.y = Game1.uiViewport.Height - box.height - 64;
        }
    }
}
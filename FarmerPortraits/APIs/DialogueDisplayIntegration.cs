using System.Collections.Generic;
using FarmerPortraits;
using FarmerPortraits.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace DialogueDisplayFrameworkApi
{
    internal class DialogueDisplayIntegrations
    {
        public static bool IsApplied { get; private set; } = false;

        private static IModHelper Helper => ModEntry.Help;
        private static ModConfig Config => ModEntry.Config;

        private static IDialogueDisplayApi DialogueDisplayApi;

        public static void Apply(IDialogueDisplayApi api)
        {
            DialogueDisplayApi = api;

            DialogueDisplayApi.RenderingPortrait += CheckForResizing;
            DialogueDisplayApi.RenderingImage += CheckWidth;
            
            DialogueDisplayApi.RenderedPortrait += OnRenderedPortrait;
            DialogueDisplayApi.RenderedText += OnRenderedText;
            DialogueDisplayApi.RenderedImage += OnRenderedImage;
            DialogueDisplayApi.RenderedDivider += OnRenderedDivider;

            IsApplied = true;
        }

        private static void CheckWidth(object sender, IRenderEventArgs<IImageData> e)
        {
            if (e.Data.ID != "DialogueDisplayFramework.Images.PortraitBackground")
                return;

            Data.DividerWidth = (int)(e.Data.W * e.Data.Scale);
            
            #if DEBUG
            ModEntry.Mon.LogOnce($"DIVIDER WIDTH: {Data.DividerWidth}", LogLevel.Info);
            #endif
        }

        private static void CheckForResizing(object sender, IRenderEventArgs<IPortraitData> e)
        {
            if (Data.CurrentFarmerEmotion == -1 || e.Data.Disabled)
                return;

            if (Game1.activeClickableMenu is not DialogueBox dialogueBox)
                return;
            
            /* default values:
               "xOffset": -352,
               "yOffset": 32,
               "right": true
             */
            if (e.Data.YOffset != 32 || e.Data.XOffset != -352)
                Methods.ResizeWindow(ref dialogueBox);
        }

        /**/
        private static void OnRenderedDivider(object sender, IRenderEventArgs<IDividerData> e)
        {
            if (e.Data.ID != "DialogueDisplayFramework.Dividers.PortraitDivider")
                return;
            
            if (Data.CurrentFarmerEmotion == -1 || e.Data.Disabled || CustomDivider(e.Data))
                return;
            
            var xPos = e.DialogueBox.x - Data.Distance;
            var yPos = e.DialogueBox.y;
            var boxWidth = e.DialogueBox.width;
            var boxHeight = e.DialogueBox.height;
            
            // top bar
            e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth / 2 + Data.DividerWidth, 24), new Rectangle(275, 313, 1, 6), Color.White); 
            // bottom bar
            e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth / 2 + Data.DividerWidth, 32), new Rectangle(275, 328, 1, 8), Color.White);
            // left bar
            e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White); 
            // top-left corner
            e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            // bottom-left corner
            e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            /* some bar we should NOT draw with DDF
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White); 
            // corners that shouldn't be drawn with DDF
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);*/
            //instead, for DDF we repeat the left bar - done in drawn portrait since otherwise it'll draw underneath
        }

        /**/
        private static void OnRenderedText(object sender, IRenderEventArgs<ITextData> e)
        {
            if (Data.CurrentFarmerEmotion == -1)
                return;

            if (e.Data.XOffset != -222 || e.Data.Disabled)
                return;

            var portraitBoxY = e.DialogueBox.y + e.DialogueBox.height / 2 - 148 - 36;
            var YOffset = e.Data.YOffset == 320 ? 296 + 16 : e.Data.YOffset;
            
            SpriteText.drawStringHorizontallyCenteredAt(e.SpriteBatch, Game1.player.Name, e.DialogueBox.x - Data.Distance / 2, portraitBoxY + YOffset);
            //SpriteText.drawStringHorizontallyCenteredAt(b, Game1.player.Name, xPos + e.Data.Width / 2, portraitBoxY + 296 + 16);
            
#if DEBUG
            ModEntry.Mon.LogOnce($"(text) WIDTH: {e.DialogueBox.width} X OFFSET: {e.Data.XOffset}", LogLevel.Info);
#endif
        }
        
        private static void OnRenderedPortrait(object sender, IRenderEventArgs<IPortraitData> e)
        {
            if (Data.CurrentFarmerEmotion == -1 || e.Data.Disabled)
                return;
            
            var portraitBoxX = e.Data.XOffset == -352 ? e.DialogueBox.x + 76 - Data.Distance : e.DialogueBox.x + e.Data.XOffset - Data.DividerWidth;
            var portraitBoxY = e.Data.YOffset == 32 ? e.DialogueBox.y + e.DialogueBox.height / 2 - 148 - 36 + 24 : e.DialogueBox.y + e.Data.YOffset;
            
            if (Data.HasCPDDFAdvanced)
                portraitBoxY -= 8;
            
            var frame = Config.FacingFront ? 0 : 6;
            var boxWidth = e.DialogueBox.width;
            var boxHeight = e.DialogueBox.height;
            var distance = boxWidth / 4 - 128;
            
            var YOffset = e.Data.YOffset == 0 ? 28 : e.Data.YOffset;
            
            if (Data.PortraitTexture != null && Config.UseCustomPortrait)
            {
                e.SpriteBatch.Draw(Data.PortraitTexture, new Rectangle(portraitBoxX + 20 - e.Data.XOffset, portraitBoxY, 256, 256), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.88f);
            }
            else
            {
                FarmerRenderer.isDrawingForUI = true;
                Methods.DrawFarmer(e.SpriteBatch, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(portraitBoxX, portraitBoxY + e.DialogueBox.height / 2 - 208), Color.White);
                if (Game1.timeOfDay >= 1900)
                {
                    Methods.DrawFarmer(e.SpriteBatch, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(portraitBoxX, portraitBoxY + e.DialogueBox.height / 2 - 192), Color.DarkBlue * 0.3f);
                }
                FarmerRenderer.isDrawingForUI = false;
            }
            
            //the bar
            // right bar
            e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(e.DialogueBox.x - 24, e.DialogueBox.y + 24, 32, boxHeight - 28), new Rectangle(266, 325, 8, 1), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f); 
            // top-right corner
            e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(e.DialogueBox.x - 24, e.DialogueBox.y - 28), new Rectangle(266, 311, 12, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            // bottom-right corner
            e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(e.DialogueBox.x - 24, e.DialogueBox.y + boxHeight - 4), new Rectangle(266, 327, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
        }

        /// <summary>
        /// Renders the portrait background.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRenderedImage(object sender, IRenderEventArgs<IImageData> e)
        {
            if (e.Data.ID != "DialogueDisplayFramework.Images.PortraitBackground")
                return;

            if (Data.CurrentFarmerEmotion == -1 || e.Data.Disabled)
                return;

            //we calculate using defaults
            //from https://github.com/MangusuPixel/DialogueDisplayFrameworkContinued/blob/main/docs/defaults.json

            var backgroundX = e.Data.XOffset == -452 ? e.DialogueBox.x - 4 - Data.Distance : e.DialogueBox.x + e.Data.XOffset - Data.Distance;
            var backgroundY = e.DialogueBox.y + e.Data.YOffset;

            if (Data.BackgroundTexture != null && Config.UseCustomBackground)
                e.SpriteBatch.Draw(Data.BackgroundTexture,
                    new Rectangle(backgroundX, backgroundY, e.Data.W, e.Data.H), null, Color.White);
            else
            {
                var texture = ModEntry.Help.GameContent.Load<Texture2D>(e.Data.TexturePath);
                e.SpriteBatch.Draw(texture, new Vector2(backgroundX, backgroundY),
                    new Rectangle(e.Data.X, e.Data.Y, e.Data.W, e.Data.H), Color.White, 0f, Vector2.Zero, e.Data.Scale, SpriteEffects.None, e.Data.LayerDepth);
            }
        }

        private static bool CustomDivider(IDividerData data)
        {
            /* default values:
               "xOffset": -484,
               "right": true
             */

            return data.Disabled || data.XOffset != -484; //data.Small || data.YOffset != 0 || data.XOffset != 0 || data.BottomConnector == false || data.TopConnector == false;
        }
    }
}
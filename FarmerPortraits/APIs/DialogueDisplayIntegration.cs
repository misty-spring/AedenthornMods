using FarmerPortraits.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace FarmerPortraits.APIs
{
    internal class DialogueDisplayIntegrations
    {
        public static bool IsApplied { get; private set; }

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

        /// <summary>
        /// Checks background image width.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CheckWidth(object sender, IRenderEventArgs<IImageData> e)
        {
            if (e.Data.ID != "DialogueDisplayFramework.Images.PortraitBackground")
                return;

            Data.DividerWidth = (int)(e.Data.W * e.Data.Scale);
            
            #if DEBUG
            ModEntry.Mon.LogOnce($"DIVIDER WIDTH: {Data.DividerWidth}", LogLevel.Info);
            #endif
        }

        /// <summary>
        /// Checks whether the dialogue box width should be reset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CheckForResizing(object sender, IRenderEventArgs<IPortraitData> e)
        {
            if (Data.CurrentFarmerEmotion == -1 || e.Data.Disabled || Data.HasChangingSkies)
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
            var color = Utility.StringToColor(e.Data.Color) ?? Color.White;
            
            // top bar
            e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth / 2 + Data.DividerWidth, 24), new Rectangle(275, 313, 1, 6), color); 
            // bottom bar
            e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth / 2 + Data.DividerWidth, 32), new Rectangle(275, 328, 1, 8), color);
            // left bar
            e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), color); 
            // top-left corner
            e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            // bottom-left corner
            e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            /* some bar we should NOT draw with DDF
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White); 
            // corners that shouldn't be drawn with DDF
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);*/
            //instead, for DDF we repeat the left bar - done in drawn portrait since otherwise it'll draw underneath
        }

        /// <summary>
        /// Renders player name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRenderedText(object sender, IRenderEventArgs<ITextData> e)
        {
            var data = e.Data;

            if (Game1.activeClickableMenu is not DialogueBox dialogueBox)
                return;

            if (data?.Disabled != false || Data.CurrentFarmerEmotion == -1 || Data.HasCPDDFAdvanced) 
                return;
            
            var pos = GetDataPoint(dialogueBox, data);

            if (data.Alignment == SpriteText.ScrollTextAlignment.Center)
                pos.X -= SpriteText.getWidthOfString(Game1.player.displayName) / 2;
            else if (data.Alignment == SpriteText.ScrollTextAlignment.Right)
                pos.X -= SpriteText.getWidthOfString(Game1.player.displayName);

            SpriteText.drawString(e.SpriteBatch, Game1.player.displayName, (int)pos.X, (int)pos.Y, 999999, data.Width, 999999, data.Alpha, data.LayerDepth, data.Junimo, data.ScrollType, data.PlaceholderText ?? "", Utility.StringToColor(data.Color), data.Alignment);
        }

        /**/
        private static void OnRenderedPortrait(object sender, IRenderEventArgs<IPortraitData> e)
        {
            var db = e.DialogueBox;
            if(!Methods.ShouldShow(ref db))
                return;
            
            if (Data.CurrentFarmerEmotion == -1 || e.Data.Disabled)
                return;
            
            var frame = Config.FacingFront ? 0 : 6;
            var boxHeight = e.DialogueBox.height;

            if (Game1.activeClickableMenu is not DialogueBox dialogueBox)
            {
                return;
            }

            //from ddf
            var shouldShake = dialogueBox.newPortaitShakeTimer > 0;
            var offset = new Vector2(shouldShake ? Game1.random.Next(-11, 2) : 12, 0);
            
            if (Data.PortraitTexture != null && Config.UseCustomPortrait)
            {
                e.SpriteBatch.Draw(Data.PortraitTexture, new Rectangle(GetDataPoint(dialogueBox, e.Data) - offset.ToPoint(), new Point(Data.PortraitTexture.Width*4, Data.PortraitTexture.Height*4)), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.88f);
            }
            else
            {
                var fakeData = new PortraitData();
                FarmerRenderer.isDrawingForUI = true;
                Methods.DrawFarmer(e.SpriteBatch, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), GetDataPoint(dialogueBox, fakeData).ToVector2() - offset, Color.White);
                if (Game1.timeOfDay >= 1900)
                {
                    Methods.DrawFarmer(e.SpriteBatch, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), GetDataPoint(dialogueBox, fakeData).ToVector2() - offset, Color.DarkBlue * 0.3f);
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

        private static Point GetDataPoint(DialogueBox box, IBaseData data)
        {
            var og = new Point(box.x + (!data.Right ? box.width : 0) + data.XOffset, box.y + (data.Bottom ? box.height : 0) + data.YOffset);

            if (Data.HasCPDDFAdvanced)
                og.X += 256;
            return og;
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
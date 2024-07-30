using FarmerPortraits;
using FarmerPortraits.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

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

            DialogueDisplayApi.RenderedPortrait += OnRenderedPortrait;
            DialogueDisplayApi.RenderedText += OnRenderedText;
            //DialogueDisplayApi.RenderedDialogueBox += OnRenderedDialogueBox;
            DialogueDisplayApi.RenderedDivider += OnRenderedDivider;

            IsApplied = true;
        }

        private static void OnRenderedDivider(object sender, IRenderEventArgs<IDividerData> e)
        {
            if (Data.CurrentFarmerEmotion == -1)
                return;
            
            var xPos = e.DialogueBox.x - Data.Distance;
            var yPos = e.DialogueBox.y;
            var boxWidth = e.DialogueBox.width;
            var boxHeight = e.DialogueBox.height;
            var b = e.SpriteBatch;
            var corner = (int)(boxWidth * 0.11);
            
            // top bar
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth / 2 + corner, 24), new Rectangle(275, 313, 1, 6), Color.White); 
            // bottom bar
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth / 2 + corner, 32), new Rectangle(275, 328, 1, 8), Color.White);
            // left bar
            b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White); 
            // top-left corner
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            // bottom-left corner
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            /* some bar we should NOT draw with DDF
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White); 
            // corners that shouldn't be drawn with DDF
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);*/
            //instead, for DDF we repeat the left bar - done in drawn portrait since otherwise it'll draw underneath
        }

        private static void OnRenderedText(object sender, IRenderEventArgs<ITextData> e)
        {
            if (Data.CurrentFarmerEmotion == -1)
                return;
            
            var portraitBoxY = e.DialogueBox.y + e.DialogueBox.height / 2 - 148 - 36;
            SpriteText.drawStringHorizontallyCenteredAt(e.SpriteBatch, Game1.player.Name, e.DialogueBox.x + 128 - Data.Distance, portraitBoxY + 296 + 16);
        }
        
        private static void OnRenderedPortrait(object sender, IRenderEventArgs<IPortraitData> e)
        {
            if (Data.CurrentFarmerEmotion == -1)
                return;
            
            var portraitBoxX = e.DialogueBox.x + 76 - Data.Distance;
            var portraitBoxY = e.DialogueBox.y + e.DialogueBox.height / 2 - 148 - 36;
            var frame = Config.FacingFront ? 0 : 6;
            var boxWidth = e.DialogueBox.width;
            var boxHeight = e.DialogueBox.height;
            var distance = boxWidth / 4 - 128;
            
            if (Data.BackgroundTexture != null && Config.UseCustomBackground)
                e.SpriteBatch.Draw(Data.BackgroundTexture, new Rectangle(e.DialogueBox.x - 4 - Data.Distance, e.DialogueBox.y, 460, boxHeight + 4), null, Color.White);
            else
                e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(e.DialogueBox.x - 4 - Data.Distance, e.DialogueBox.y), new Rectangle(583, 411, 115, 97), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f); // background
            
            if (Data.PortraitTexture != null && Config.UseCustomPortrait)
            {
                e.SpriteBatch.Draw(Data.PortraitTexture, new Rectangle(portraitBoxX + 20, portraitBoxY + 24, 256, 256), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.88f);
            }
            else
            {
                FarmerRenderer.isDrawingForUI = true;
                Methods.DrawFarmer(e.SpriteBatch, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(portraitBoxX + distance, portraitBoxY + e.DialogueBox.height / 2 - 208), Color.White);
                if (Game1.timeOfDay >= 1900)
                {
                    Methods.DrawFarmer(e.SpriteBatch, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(portraitBoxX + distance, portraitBoxY + e.DialogueBox.height / 2 - 192), Color.DarkBlue * 0.3f);
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

        private static void OnRenderedDialogueBox(object sender, IRenderEventArgs<IDialogueDisplayData> e)
        {
        }
    }
}
using FarmerPortraits;
using FarmerPortraits.Patches;
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

        private static IModHelper Helper => ModEntry.SHelper;
        private static ModConfig Config => ModEntry.Config;

        private static IDialogueDisplayApi DialogueDisplayApi;
        public static IDialogueDisplayData Data { get; private set; }

        public static void Apply(IDialogueDisplayApi api)
        {
            DialogueDisplayApi = api;
            Data = api.CurrentDisplayData;

            DialogueDisplayApi.OnRenderingPortrait = OnRenderingPortrait;

            IsApplied = true;
        }

        public static void TestEvent(SpriteBatch b, DialogueBox box, IDialogueDisplayData data)
        {
            ModEntry.SMonitor.Log($"test ! {data.XOffset}", LogLevel.Warn);
            //throw new System.Exception("...");
        }

        public static void OnRenderingDialogueBox(SpriteBatch b, DialogueBox dialogueBox, dynamic data)
        {
        }

        public static void OnRenderingPortrait(SpriteBatch b, DialogueBox dialogueBox, dynamic rawData)
        {
            var data = rawData as IPortraitData; 

            ModEntry.SMonitor.Log($"It's working!! {rawData.XOffset}", LogLevel.Warn);

            var xPos = data?.X ?? 20;
            var yPos = data?.Y ?? 384;
            var boxWidth = data?.Width ?? 1200;
            var boxHeight = data?.Height ?? 384;
            
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle(275, 313, 1, 6), Color.White); 
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle(275, 328, 1, 8), Color.White); 
            b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White); 
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White); 
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f); 
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        }

        public static void OnRenderingText(SpriteBatch b, DialogueBox dialogueBox, dynamic data)
        {

        }

        public static void OnRenderingDivider(SpriteBatch b, DialogueBox dialogueBox, dynamic data)
        {
/*
 * if (DialogueBoxPatches.Background != null && Config.UseCustomBackground)
       b.Draw(DialogueBoxPatches.Background, new Rectangle(xPos - 4, yPos, boxWidth + 12, boxHeight + 4), null, Color.White);
   else
       b.Draw(Game1.mouseCursors, new Vector2(xPos - 4, yPos), new Rectangle(583, 411, 115, 97), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f); // background

   var portraitBoxX = xPos + 76;
   var portraitBoxY = yPos + boxHeight / 2 - 148 - 36;
   var frame = Config.FacingFront ? 0 : 6;
   if (DialogueBoxPatches.Portrait != null && Config.UseCustomPortrait)
   {
       b.Draw(DialogueBoxPatches.Portrait, new Rectangle(portraitBoxX + 20, portraitBoxY + 24, 256, 256), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.88f);
   }
   else
   {
       FarmerRenderer.isDrawingForUI = true;
       DialogueBoxPatches.DrawFarmer(b, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(xPos + boxWidth / 2 - 128, yPos + boxHeight / 2 - 208), Color.White);
       if (Game1.timeOfDay >= 1900)
       {
           DialogueBoxPatches.DrawFarmer(b, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(xPos + boxWidth / 2 - 128, yPos + boxHeight / 2 - 192), Color.DarkBlue * 0.3f);
       }
       FarmerRenderer.isDrawingForUI = false;
   }
   SpriteText.drawStringHorizontallyCenteredAt(b, Game1.player.Name, xPos + boxWidth / 2, portraitBoxY + 296 + 16);
 */
        }
    }
}
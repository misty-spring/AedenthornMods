using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;

namespace DialogueDisplayFrameworkApi
{
    public delegate void DialogueBoxRenderDelegate(SpriteBatch b, DialogueBox box, IDialogueDisplayData data);

    public interface IDialogueDisplayApi
    {
        public IDialogueDisplayData CurrentDisplayData { get; }

        //public Action<SpriteBatch, DialogueBox, IDialogueDisplayData> OnRenderingDialogueBox { set; }
        //public DialogueBoxRenderDelegate OnRenderingDialogueBox { get; set; }
        public Action<SpriteBatch, DialogueBox, IPortraitData> OnRenderingPortrait { get; set; }
        //public Action<SpriteBatch, DialogueBox, ITextData> OnRenderingText { get; set; }
        //public Action<SpriteBatch, DialogueBox, IImageData> OnRenderingImage { get; set; }
        //public Action<SpriteBatch, DialogueBox, IDividerData> OnRenderingDivider { get; set; }
    }

    public interface IDialogueDisplayData
    {
        public string CopyFrom { get; set; }
        public string PackName { get; set; }
        public int? XOffset { get; set; }
        public int? YOffset { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool Disabled { get; set; }
    }

    public interface IBaseData
    {
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public bool Right { get; set; }
        public bool Bottom { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Alpha { get; set; }
        public float Scale { get; set; }
        public float LayerDepth { get; set; }
        public bool Disabled { get; set; }
    }

    public interface IPortraitData : IBaseData
    {
        public string TexturePath { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public bool TileSheet { get; set; }
    }
    
    public interface ITextData : IBaseData
    {
        public string ID { get; set; }
        public string Color { get; set; }
        public string Text { get; set; }
        public bool Junimo { get; set; }
        public bool Scroll { get; set; }
        public string PlaceholderText { get; set; }
        public int ScrollType { get; set; }
        public SpriteText.ScrollTextAlignment Alignment { get; set; }
    }

    public interface IImageData : IBaseData
    {
        public string ID { get; set; }
        public string TexturePath { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }

    public interface IDividerData : IBaseData
    {
        public string ID { get; set; }
        public bool Horizontal { get; set; }
        public bool Small { get; set; }
        public string Color { get; set; }
    }
}
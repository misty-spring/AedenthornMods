namespace FarmerPortraits
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool ShowWithQuestions { get; set; } = true;
        public bool ShowWithEvents { get; set; }
        public bool ShowWithNpcPortrait { get; set; } = true;
        public bool ShowMisc { get; set; }
        public bool FacingFront { get; set; }
        public bool FixText { get; set; }
        //public bool SmallerWindow { get; set; } = true;
        public bool UseCustomPortrait { get; set; } = true;
        public bool UseCustomBackground { get; set; } = true;
        public bool PortraitReactions { get; set; }
        public int Reaction0 { get; set; }
        public int Reaction1 { get; set; } = 1;
        public int Reaction2 { get; set; }
        public int Reaction3 { get; set; } = 2;
        public int Reaction4 { get; set; }
        public int Reaction5 { get; set; }
    }
}

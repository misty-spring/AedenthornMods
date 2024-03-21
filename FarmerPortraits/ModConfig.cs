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
        public bool UseCustomPortrait { get; set; } = true;
        public bool UseCustomBackground { get; set; } = true;
    }
}

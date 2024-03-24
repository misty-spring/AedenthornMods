using System.Collections.Generic;
using FarmerPortraits.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FarmerPortraits;

/// <summary>The mod entry point.</summary>
public sealed class ModEntry : Mod
{
    internal static IMonitor SMonitor;
    internal static IModHelper SHelper;
    internal static ModConfig Config;
    //internal static ModEntry context;

    internal static Texture2D PortraitTexture;
    internal static Texture2D BackgroundTexture;

    internal static Dictionary<string, Dictionary<int, int>> Reactions { get; set; } = new();

    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        Config = Helper.ReadConfig<ModConfig>();

        //context = this;

        SMonitor = Monitor;
        SHelper = helper;

        helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
#if DEBUG
        helper.Events.Input.ButtonPressed += Input_ButtonPressed;
#endif
        helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

        helper.Events.Display.MenuChanged += Display_MenuChanged;
        
        helper.Events.Content.AssetRequested += Asset.OnRequest;
        helper.Events.Content.AssetsInvalidated += Asset.OnInvalidate;
            
        var harmony = new Harmony(ModManifest.UniqueID);
        DialogueBoxPatches.Apply(harmony);
    }

    private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
    {

        // get Generic Mod Config Menu's API (if it's installed)
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        // register mod
        configMenu.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );
        
        configMenu.AddSectionTitle(
            mod: ModManifest,
            text: () => Helper.Translation.Get("Section.General")
        );

        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("Enabled"),
            getValue: () => Config.EnableMod,
            setValue: value => Config.EnableMod = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ShowWithNPCs"),
            getValue: () => Config.ShowWithNpcPortrait,
            setValue: value => Config.ShowWithNpcPortrait = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ShowWithQuestions"),
            getValue: () => Config.ShowWithQuestions,
            setValue: value => Config.ShowWithQuestions = value
        );;
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ShowOtherwise.title"),
            tooltip: () => Helper.Translation.Get("ShowOtherwise.description"),
            getValue: () => Config.ShowMisc,
            setValue: value => Config.ShowMisc = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("ShowDuringEvents"),
            getValue: () => Config.ShowWithEvents,
            setValue: value => Config.ShowWithEvents = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("FacingFront.title"),
            tooltip: () => Helper.Translation.Get("FacingFront.description"),
            getValue: () => Config.FacingFront,
            setValue: value => Config.FacingFront = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("FixText.title"),
            tooltip: () => Helper.Translation.Get("FixText.description"),
            getValue: () => Config.FixText,
            setValue: value => Config.FixText = value
        );
        
        configMenu.AddSectionTitle(
            mod: ModManifest,
            text: () => Helper.Translation.Get("Section.Customizable")
            );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("CustomPortrait.title"),
            tooltip: () => Helper.Translation.Get("CustomPortrait.description"),
            getValue: () => Config.UseCustomPortrait,
            setValue: value => Config.UseCustomPortrait = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("CustomBackground.title"),
            tooltip: () => Helper.Translation.Get("CustomBackground.description"),
            getValue: () => Config.UseCustomBackground,
            setValue: value => Config.UseCustomBackground = value
        );
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("PortraitReactions.title"),
            tooltip: () => Helper.Translation.Get("PortraitReactions.description"),
            getValue: () => Config.PortraitReactions,
            setValue: value => Config.PortraitReactions = value
        );
        configMenu.AddPageLink(
            mod: ModManifest,
            pageId: "Emotions",
            text: () => Helper.Translation.Get("SpecificReactions.title") + GetDots()
            );
        
        configMenu.AddPage(
            mod: ModManifest,
            pageId: "Emotions",
            pageTitle: GetDots
            );

        configMenu.AddSectionTitle(
            mod: ModManifest,
            text: () => Helper.Translation.Get("SpecificReactions.title")
            );

        configMenu.AddParagraph(
            mod: ModManifest,
            text: () => Helper.Translation.Get("SpecificReactions.description")
        );
        
        var allowedValues = new[] { "0", "1", "2", "3", "4", "5" };
        
        //this gets the value as string, then turns it back into
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("SpecificReactions.NPC") + Helper.Translation.Get("SpecificReactions.values.0"),
            getValue: () => $"{Config.Reaction0}",
            setValue: value => Config.Reaction0 = int.Parse(value),
            allowedValues: allowedValues,
            //what this huge thing does: checks if TL is default/"not found", if so uses "Panel {0}". Otherwise, it uses the appropiate key
            formatAllowedValue: value => Helper.Translation.Get($"SpecificReactions.values.{value}").ToString().Contains("No translation") ? string.Format(Helper.Translation.Get("SpecificReactions.notFound"), value) : Helper.Translation.Get($"SpecificReactions.values.{value}")
            );
        
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("SpecificReactions.NPC") + Helper.Translation.Get("SpecificReactions.values.1"),
            getValue: () => $"{Config.Reaction1}",
            setValue: value => Config.Reaction1 = int.Parse(value),
            allowedValues: allowedValues,
            formatAllowedValue: value => Helper.Translation.Get($"SpecificReactions.values.{value}").ToString().Contains("No translation") ? string.Format(Helper.Translation.Get("SpecificReactions.notFound"), value) : Helper.Translation.Get($"SpecificReactions.values.{value}")
            );
        
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("SpecificReactions.NPC") + Helper.Translation.Get("SpecificReactions.values.2"),
            getValue: () => $"{Config.Reaction2}",
            setValue: value => Config.Reaction2 = int.Parse(value),
            allowedValues: allowedValues,
            formatAllowedValue: value => Helper.Translation.Get($"SpecificReactions.values.{value}").ToString().Contains("No translation") ? string.Format(Helper.Translation.Get("SpecificReactions.notFound"), value) : Helper.Translation.Get($"SpecificReactions.values.{value}")
        );
        
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("SpecificReactions.NPC") + Helper.Translation.Get("SpecificReactions.values.3"),
            getValue: () => $"{Config.Reaction3}",
            setValue: value => Config.Reaction3 = int.Parse(value),
            allowedValues: allowedValues,
            formatAllowedValue: value => Helper.Translation.Get($"SpecificReactions.values.{value}").ToString().Contains("No translation") ? string.Format(Helper.Translation.Get("SpecificReactions.notFound"), value) : Helper.Translation.Get($"SpecificReactions.values.{value}")
        );
        
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("SpecificReactions.NPC") + Helper.Translation.Get("SpecificReactions.values.4"),
            getValue: () => $"{Config.Reaction4}",
            setValue: value => Config.Reaction4 = int.Parse(value),
            allowedValues: allowedValues,
            formatAllowedValue: value => Helper.Translation.Get($"SpecificReactions.values.{value}").ToString().Contains("No translation") ? string.Format(Helper.Translation.Get("SpecificReactions.notFound"), value) : Helper.Translation.Get($"SpecificReactions.values.{value}")
        );
        
        configMenu.AddTextOption(
            mod: ModManifest,
            name: () => Helper.Translation.Get("SpecificReactions.NPC") + Helper.Translation.Get("SpecificReactions.values.5"),
            getValue: () => $"{Config.Reaction5}",
            setValue: value => Config.Reaction5 = int.Parse(value),
            allowedValues: allowedValues,
            formatAllowedValue: value => Helper.Translation.Get($"SpecificReactions.values.{value}").ToString().StartsWith("(no translation:") ? string.Format(Helper.Translation.Get("SpecificReactions.notFound"), value) : Helper.Translation.Get($"SpecificReactions.values.{value}")
        );
    }

    private static string GetDots()
    {
        return LocalizedContentManager.CurrentLanguageCode switch
        {
            LocalizedContentManager.LanguageCode.ja => "。。。",
            LocalizedContentManager.LanguageCode.zh => "。。。",
            _ => "..."
        };
    }

    private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        Reactions = Helper.GameContent.Load<Dictionary<string, Dictionary<int, int>>>("aedenthorn.FarmerPortraits/reactions");
    }

    private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if(e.Button == SButton.Enter && Context.IsWorldReady)
        {
            var d = new Dialogue(Game1.getCharacterFromName("Lewis"), null, "This is a reaction change!$1#$b#Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.$u");
            Game1.DrawDialogue(d);
        }
    }

    private static void Display_MenuChanged(object sender, MenuChangedEventArgs e)
    {
        ReloadTextures();
    }

    private static void ReloadTextures()
    {
        try
        {
            PortraitTexture = SHelper.GameContent.Load<Texture2D>("aedenthorn.FarmerPortraits/portrait");
        }
        catch
        {
            PortraitTexture = null;
        }
        try
        {
            BackgroundTexture = SHelper.GameContent.Load<Texture2D>("aedenthorn.FarmerPortraits/background");
        }
        catch
        {
            BackgroundTexture = null;
        }
    }
}
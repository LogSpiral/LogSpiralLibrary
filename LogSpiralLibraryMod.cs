global using System;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria.ModLoader;
global using Terraria;
global using Terraria.ID;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using MeleeSequence = LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Sequence<LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.MeleeAction>;
using Terraria.ModLoader.Config;
using System.ComponentModel;
using System.IO;
using NetSimplified;
using Terraria.ModLoader.UI;
using LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
using Terraria.ModLoader.Core;
using Terraria.Localization;
using static Terraria.Localization.NetworkText;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Hjson;
using Newtonsoft.Json.Linq;
using Terraria.Audio;
namespace LogSpiralLibrary;



public partial class LogSpiralLibraryMod : Mod
{
    #region Misc
    public static double ModTime => GlobalTimeSystem.GlobalTime;
    public static double ModTime2 => GlobalTimeSystem.GlobalTimePaused;
    static void FuckSDKCheck() => UIModSources.dotnetSDKFound = true;
    #endregion

    public static LogSpiralLibraryMod Instance;
    public LogSpiralLibraryMod()
    {
        InitializeBlendStates();
    }

    public override void Load()
    {
        Instance = this;
        AddContent<NetModuleLoader>();

        if (Main.dedServ) return;

        LoadAllTextures();
        AddOnResolutionChangedHook();
        FuckSDKCheck();
        LocalizationFix();
    }
    public override void Unload() => Instance = null;
    public override void HandlePacket(BinaryReader reader, int whoAmI) => NetModule.ReceiveModule(reader, whoAmI);
}

public class LogSpiralLibraryMiscConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static LogSpiralLibraryMiscConfig Instance => ModContent.GetInstance<LogSpiralLibraryMiscConfig>();
    [CustomModConfigItem(typeof(AvailableConfigElement))]
    public ShakingSetting screenShakingSetting = new();
    public class ShakingSetting : IAvailabilityChangableConfig
    {
        public bool Available { get; set; } = true;
        [Range(0, 1)]
        [DefaultValue(0.25f)]
        public float strength = 0.25f;

    }

    [DefaultValue(false)]
    public bool WTHConfig = false;
}
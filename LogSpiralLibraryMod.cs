global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using System;
global using Terraria;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Terraria.ID;
global using Terraria.ModLoader;
using LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
using Microsoft.Xna.Framework.Audio;
using NetSimplified;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;

namespace LogSpiralLibrary;

public partial class LogSpiralLibraryMod : Mod
{
    #region Misc

    public static double ModTime => GlobalTimeSystem.GlobalTime;
    public static double ModTime2 => GlobalTimeSystem.GlobalTimePaused;

    private static void FuckSDKCheck() => UIModSources.dotnetSDKFound = true;

    #endregion Misc

    public static LogSpiralLibraryMod Instance { get; private set; }

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


        //MonoModHooks.Add(
        //    typeof(SoundEffectInstance)
        //    .GetMethod(nameof(SoundEffectInstance.UpdatePitch), BindingFlags.NonPublic | BindingFlags.Instance),
        //    (Action<SoundEffectInstance> orig, SoundEffectInstance self) =>
        //    {
        //        float doppler;
        //        float dopplerScale = SoundEffect.Device().DopplerScale;
        //        if (!self.is3D || dopplerScale == 0.0f)
        //        {
        //            doppler = 1.0f;
        //        }
        //        else
        //        {
        //            doppler = self.dspSettings.DopplerFactor * dopplerScale;
        //        }
        //        Console.WriteLine((self.Pitch, self.INTERNAL_pitch));
        //        var result = FAudio.FAudioSourceVoice_SetFrequencyRatio(
        //            self.handle,
        //            (float)Math.Pow(2.0, self.INTERNAL_pitch) * doppler,
        //            0
        //        );
        //    });
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
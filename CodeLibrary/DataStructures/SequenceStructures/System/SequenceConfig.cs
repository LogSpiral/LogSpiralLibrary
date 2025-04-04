using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;

public class SequenceConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    [DefaultValue(false)]
    public bool ShowWrapBox = false;
    [DefaultValue(false)]
    public bool ShowGroupBox = false;
    [DefaultValue(false)]
    public bool ShowSequenceBox = false;
    [DefaultValue(typeof(Vector2), "32, 16")]
    [Range(0f, 64f)]
    public Vector2 Step = new(32, 16);
    public static SequenceConfig Instance { get; private set; }
    public override void OnLoaded()
    {
        Instance = this;
        base.OnLoaded();
    }
}

using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using LogSpiralLibrary.ForFun.GeogebraShin;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace LogSpiralLibrary.ForFun.ColorfulMushroom;

public class ColorfulMushroom : ModItem
{
    public override bool IsLoadingEnabled(Mod mod) => false;
    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.width = Item.height = 32;
        Item.useTime = Item.useAnimation = 12;
        base.SetDefaults();
    }
    public override void UseAnimation(Player player)
    {
        if (player.itemAnimation == 0)
        {
            active = !active;
            if (active)
                SoundEngine.PlaySound(SoundID.Zombie104, player.Center);
        }
        base.UseAnimation(player);
    }
    public static bool active;
}
public class ColorfulMushroomSystem : ModSystem
{
    public override bool IsLoadingEnabled(Mod mod) => false;
    public static ColorfulMushroomData ScreenTransformData;
    public override void PostSetupContent()
    {
        if (Main.netMode == NetmodeID.Server) return;
        ScreenTransformData = new ColorfulMushroomData(ModContent.Request<Effect>("LogSpiralLibrary/Effects/MatrixFunctionEffect", AssetRequestMode.ImmediateLoad), "McosMt");//
        Filters.Scene["LogSpiralLibrary:ColorfulMushroom"] = new Filter(ScreenTransformData, EffectPriority.VeryHigh);
        base.PostSetupContent();
    }
    public override void PreUpdateEntities()
    {
        if (Main.netMode == NetmodeID.Server) return;
        ControlScreenShader("LogSpiralLibrary:ColorfulMushroom", ColorfulMushroom.active);
    }
    private static void ControlScreenShader(string name, bool state)
    {
        if (!Filters.Scene[name].IsActive() && state)
            Filters.Scene.Activate(name);
        if (Filters.Scene[name].IsActive() && !state)
            Filters.Scene.Deactivate(name);
    }
}
public class ColorfulMushroomData : ScreenShaderData
{
    public ColorfulMushroomData(string passName) : base(passName)
    {
    }
    public ColorfulMushroomData(Asset<Effect> shader, string passName) : base(shader, passName)
    {

    }
    public override void Apply()
    {
        //base.Apply();
        float uTime = Main.GlobalTimeWrappedHourly.CosFactor(15) * 1000;
        Shader.Parameters["uTime"]?.SetValue(100);
        Shader.CurrentTechnique.Passes[0].Apply();
    }
}

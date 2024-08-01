using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;
using ReLogic.Content;
using System;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace LogSpiralLibrary.ForFun.GeogebraShin
{
    public class GeogebraShin : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("圆神");
            // Tooltip.SetDefault("你知道我要说什么的对吧");
        }
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.width = Item.height = 32;
            Item.useTime = Item.useAnimation = 12;
            //Item.UseSound = null;
            base.SetDefaults();
        }
        public override void UseAnimation(Player player)
        {
            if (player.itemAnimation == 0)
            {
                active = !active;
                //if (active)
                //{
                //    //Main.NewText("圆神，启动！");
                //    foreach (var sequence in SequenceSystem.sequenceBases.Values)
                //    {
                //        //Main.NewText(sequence == null);
                //        sequence.Save();
                //    }
                //    SoundEngine.PlaySound(SoundID.Zombie104, player.Center);
                //    //SetDefaults();
                //}
            }
            base.UseAnimation(player);
        }
        public static bool active;
    }
    public class GeogebraShinSystem : ModSystem
    {
        public static ScreenTransformData ScreenTransformData;
        public override void Load()
        {
            ModContent.Request<Effect>("LogSpiralLibrary/Effects/ScreenTransform", AssetRequestMode.ImmediateLoad);
            //ModContent.Request<Effect>("LogSpiralLibrary/Effects/ColorScreen", AssetRequestMode.ImmediateLoad);
            ScreenTransformData = (ScreenTransformData)new ScreenTransformData(new Ref<Effect>(ModContent.Request<Effect>("LogSpiralLibrary/Effects/ScreenTransform", AssetRequestMode.ImmediateLoad).Value),
                "ConicSection");//.UseImage(ModContent.Request<Texture2D>("LogSpiralLibrary/Images/HeatMap/HeatMap_0").Value, 1).UseImage(LogSpiralLibrary.AniTex[8].Value, 2)
            Filters.Scene["LogSpiralLibrary:WTFScreen"] = new Filter(ScreenTransformData, EffectPriority.Medium);
        }
        public override void PreUpdateEntities()
        {
            ControlScreenShader("LogSpiralLibrary:WTFScreen", GeogebraShin.active);//LogSpiralLibraryConfig.instance.UseScreenShader
        }
        private void ControlScreenShader(string name, bool state)
        {
            if (!Filters.Scene[name].IsActive() && state)
            {
                Filters.Scene.Activate(name);
            }
            if (Filters.Scene[name].IsActive() && !state)
            {
                Filters.Scene.Deactivate(name);
            }
        }
    }
    public class ScreenTransformData : ScreenShaderData
    {
        public ScreenTransformData(string passName) : base(passName)
        {
        }
        public ScreenTransformData(Ref<Effect> shader, string passName) : base(shader, passName)
        {

        }
        public override void Apply()
        {
            #region Old
            ////float[] m = LogSpiralLibraryConfig.instance.Matrix;
            ////Matrix matrix = new Matrix
            ////    (
            ////    m[0], m[1], m[2], 0,
            ////    m[3], m[4], m[5], 0,
            ////    m[6], m[7], m[8], 0,
            ////    0, 0, 0, 0
            ////    );
            ////var (c, s) = MathF.SinCos(Main.GlobalTimeWrappedHourly * 0.5f);
            ////Matrix matrix = new Matrix
            ////    (
            ////    c, s, 0, 0,
            ////    -s, c, 0, 0,
            ////    0, 0, 1, 0,
            ////    0, 0, 0, 0
            ////    );
            //var vec = (Main.MouseScreen - Main.ScreenSize.ToVector2() / 2);
            //var scaler = 1 - 1 / (1 + vec.Length() / 64f);
            //scaler *= MathHelper.Pi / 4;
            //var (s, c) = MathF.SinCos(vec.ToRotation()); //Main.GlobalTimeWrappedHourly
            //var (c2, s2) = MathF.SinCos(scaler);
            ////var (c, s) = MathF.SinCos(Main.GlobalTimeWrappedHourly); //Main.GlobalTimeWrappedHourly
            ////var (c2, s2) = MathF.SinCos(MathHelper.Pi / 3);
            //Matrix matrix = new Matrix
            //    (
            //    1, 0, c * c2, 0,
            //    0, 1, s * c2, 0,
            //    0, 0, s2, 0,
            //    0, 0, 0, 0
            //    );
            //Shader.Parameters["TransformMatrix"].SetValue(Matrix.Lerp(Matrix.Identity, matrix, MathHelper.SmoothStep(0, 1, CombinedOpacity)));
            ////float value = (MathF.Sin(Main.GlobalTimeWrappedHourly) + 1) * .5f;
            //Shader.Parameters["width"].SetValue(new Vector2(0.2f,0.5f));

            //Main.instance.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            //Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            //Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;

            //Main.instance.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/AniTex/Style_8").Value;
            //Main.instance.GraphicsDevice.Textures[2] = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/AniTex/Style_8").Value;

            //Shader.Parameters["useHeatMap"].SetValue(false);
            //base.Apply();
            ////if (ModContent.TryFind<ModPlayer>("CoolerItemVisualEffect", "CoolerItemVisualEffectPlayer", out var value))
            ////{
            ////    dynamic coolerPlr = value;
            ////    Main.instance.GraphicsDevice.Textures[1] = coolerPlr.colorInfo.Item1;
            ////}
            #endregion

            #region 采样图
            //var texture = Main.LocalPlayer.GetModPlayer<CoolerItemVisualEffectPlayer>().colorInfo.tex;
            //if (texture != null)
            //{
            //    Main.instance.GraphicsDevice.Textures[1] = texture;
            //}
            //else
            //{
            //    Main.instance.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/HeatMap_0").Value;

            //}
            //Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            #endregion

            #region 启动！
            //var factor = ForFun.ggb.Factor;
            var factor = CombinedOpacity;
            var fac1 = MathHelper.SmoothStep(0, 1, factor * 2);
            var fac2 = MathHelper.SmoothStep(0, 1, factor * 2 - 1);
            //var vec = (Main.MouseScreen - Main.ScreenSize.ToVector2() / 2);
            var scaler = MathHelper.Lerp(0, MathHelper.Pi * .45f, fac2);
            var (s, c) = MathF.SinCos(336.7f);
            var (s2, c2) = MathF.SinCos(scaler);
            Matrix matrix = new Matrix
                (
                1, 0, c * s2, 0,
                0, 1, s * s2, 0,
                0, 0, c2 * .5f, 0,
                0, 0, 0, 0
                );
            Shader.Parameters["TransformMatrix"].SetValue(Matrix.Lerp(Matrix.Identity, matrix, fac2));
            Shader.Parameters["width"].SetValue(Vector2.Lerp(new Vector2(0.35f), new Vector2(0.2f, 0.5f), fac1));
            Shader.Parameters["offset"].SetValue(Vector2.Lerp(default, new Vector2(0.5f) * new Vector2(Main.screenWidth / (float)Main.screenHeight, 1), fac2));

            Main.instance.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.AnisotropicWrap;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;

            Main.instance.GraphicsDevice.Textures[1] = LogSpiralLibraryMod.AniTex[8].Value;
            Main.instance.GraphicsDevice.Textures[2] = LogSpiralLibraryMod.AniTex[8].Value;

            Shader.Parameters["useHeatMap"].SetValue(false);
            base.Apply();
            #endregion
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
    //不必再存在的config
    //public class GeogebraShinConfig : ModConfig
    //{
    //    public override ConfigScope Mode => ConfigScope.ClientSide;
    //    public static GeogebraShinConfig instance => ModContent.GetInstance<GeogebraShinConfig>();

    //    [Label("使用奇妙滤镜")]
    //    [Tooltip("天旋地转！！话说为什么阿汪要在这里留个滤镜")]

    //    public bool UseScreenShader;
    //    public enum ShaderPassMode
    //    {
    //        [Label("单一")]
    //        Single,
    //        [Label("取模")]
    //        Wrap,
    //        [Label("原来你也玩圆神")]
    //        ConicSection,
    //        [Label("测试")]
    //        Test
    //    }
    //    [Label("滤镜模式")]
    //    public ShaderPassMode PassMode;
    //    public override void OnChanged()
    //    {
    //        if (GeogebraShinSystem.ScreenTransformData != null)
    //            GeogebraShinSystem.ScreenTransformData.SwapProgram(PassMode.ToString());
    //        base.OnChanged();
    //    }
    //}
}

global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria.ModLoader;
global using Terraria;
global using Terraria.ID;
global using Terraria.DataStructures;
global using Terraria.GameContent;
using ReLogic.Content;
using System.Collections.Generic;

namespace LogSpiralLibrary
{
    public class LogSpiralLibraryMod : Mod
    {
        #region Effects
        private static Effect itemEffect;
        private static Effect shaderSwooshEffect;//第一代刀光effect
        private static Effect shaderSwooshEX;//第二代
        //↑但是很不幸的是，都丢失.fx了，等阿汪做出第三代吧
        private static Effect shaderSwooshUL;//第三代，目前有问题

        private static Effect distortEffect;
        private static Effect finalFractalTailEffect;
        private static Effect colorfulEffect;
        private static Effect eightTrigramsFurnaceEffect;//第一代抛物激光effect，下次做第二代
        private static Effect vertexDraw;
        private static Effect vertexDrawEX;
        private static Effect transformEffect;

        public static Effect ItemEffect => itemEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/ItemGlowEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect ShaderSwooshEffect => shaderSwooshEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/ShaderSwooshEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect ShaderSwooshEX => shaderSwooshEX ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/ShaderSwooshEffectEX", AssetRequestMode.ImmediateLoad).Value;
        public static Effect ShaderSwooshUL => shaderSwooshUL ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/ShaderSwooshEffectUL", AssetRequestMode.ImmediateLoad).Value;
        public static Effect DistortEffect => distortEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/DistortEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect FinalFractalTailEffect => finalFractalTailEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/FinalFractalTailEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect ColorfulEffect => colorfulEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/ColorfulEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect EightTrigramsFurnaceEffect => eightTrigramsFurnaceEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/EightTrigramsFurnaceEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect VertexDraw => vertexDraw ??= ModContent.Request<Effect>("StoneOfThePhilosophers/Effects/VertexDraw", AssetRequestMode.ImmediateLoad).Value;
        public static Effect VertexDrawEX => vertexDrawEX ??= ModContent.Request<Effect>("StoneOfThePhilosophers/Effects/VertexDrawEX", AssetRequestMode.ImmediateLoad).Value;
        public static Effect TransformEffect => transformEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/TransformEffect", AssetRequestMode.ImmediateLoad).Value;

        #endregion

        #region Textures
        public static Asset<Texture2D>[] BaseTex;
        public static Asset<Texture2D>[] AniTex;
        public static Asset<Texture2D>[] HeatMap;
        public static Asset<Texture2D>[] MagicZone;
        /// <summary>
        /// 杂图，以下是内容表(0-17)
        /// <para>0-2:也许是给物品附魔光泽用的贴图</para>
        /// <para>3:刀光的灰度图，为什么会在这里有一张？？</para>
        /// <para>4-5:箭头和磁场点叉</para>
        /// <para>6:符文条带</para>
        /// <para>7-10:闪电激光</para>
        /// <para>11:车万激光</para>
        /// <para>12:压扁的白色箭头？？</para>
        /// <para>13-17:有些来着原版的Extra，有些是我自己瞎画，给最终分形那些用</para>
        /// </summary>
        public static Asset<Texture2D>[] Misc;
        //public static string BaseTex = nameof(BaseTex);
        //public static string AniTex = nameof(AniTex);
        //public static string HeatMap = nameof(HeatMap);
        //public static string MagicZone = nameof(MagicZone);
        //public static string Misc = nameof(Misc);
        //public static Dictionary<string, Asset<Texture2D>[]> Textures;
        #endregion

        #region Other
        public static double ModTime => LogSpiralLibrarySystem.ModTime;
        public static double ModTime2 => LogSpiralLibrarySystem.ModTime2;

        public static LogSpiralLibraryMod Instance;

        public static BlendState AllOne;
        public LogSpiralLibraryMod()
        {
            AllOne = new BlendState();
            AllOne.ColorDestinationBlend = AllOne.AlphaDestinationBlend = AllOne.ColorSourceBlend = AllOne.AlphaSourceBlend = Blend.One;
        }
        #endregion

        #region Render
        public static bool CanUseRender => Lighting.Mode != Terraria.Graphics.Light.LightMode.Retro && Lighting.Mode != Terraria.Graphics.Light.LightMode.Trippy && Main.WaveQuality != 0;
        public RenderTarget2D Render
        {
            get => render ??= new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth == 0 ? 1920 : Main.screenWidth, Main.screenHeight == 0 ? 1120 : Main.screenHeight);
            set
            {
                render = value;
            }
        }

        private RenderTarget2D render;
        public RenderTarget2D Render_AirDistort
        {
            get => render_AirDistort ??= new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth == 0 ? 1920 : Main.screenWidth, Main.screenHeight == 0 ? 1120 : Main.screenHeight);
            set
            {
                render_AirDistort = value;
            }
        }

        private RenderTarget2D render_AirDistort;
        private void OnResolutionChanged_RenderCreate(Vector2 useless) => Main.RunOnMainThread(CreateRender);
        public void CreateRender()
        {
            if (Render != null) Render.Dispose();
            Render = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth == 0 ? 1920 : Main.screenWidth, Main.screenHeight == 0 ? 1120 : Main.screenHeight);
            if (Render_AirDistort != null) Render_AirDistort.Dispose();
            Render_AirDistort = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth == 0 ? 1920 : Main.screenWidth, Main.screenHeight == 0 ? 1120 : Main.screenHeight);
        }
        #endregion
        //public static bool CIVELoaded => ModLoader.HasMod("CoolerItemVisualEffect");
        public override void Load()
        {
            Instance = this;
            LoadTextures(ref BaseTex, nameof(BaseTex));
            LoadTextures(ref AniTex, nameof(AniTex));
            LoadTextures(ref HeatMap, nameof(HeatMap));
            LoadTextures(ref MagicZone, nameof(MagicZone));
            LoadTextures(ref Misc, nameof(Misc));
            Main.OnResolutionChanged += OnResolutionChanged_RenderCreate;
            On.Terraria.Graphics.Effects.FilterManager.EndCapture += FilterManager_EndCapture_LSLib; ;
            On.Terraria.Main.DrawProjectiles += Main_DrawProjectiles_LSLib; ;
            base.Load();
        }

        private void FilterManager_EndCapture_LSLib(On.Terraria.Graphics.Effects.FilterManager.orig_EndCapture orig, Terraria.Graphics.Effects.FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, clearColor);
            if (!CanUseRender) return;
            foreach (var renderDrawing in LogSpiralLibrarySystem.renderBasedDrawings) 
            {
                renderDrawing.RenderDrawingMethods(Main.spriteBatch, Main.instance.GraphicsDevice, Render, Render_AirDistort);
            }
        }

        private void Main_DrawProjectiles_LSLib(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
        {
            orig.Invoke(self);
            if (CanUseRender) return;
            foreach (var renderDrawing in LogSpiralLibrarySystem.renderBasedDrawings)
            {
                renderDrawing.CommonDrawingMethods(Main.spriteBatch);
            }
        }

        public override void Unload()
        {
            Instance = null;
            Main.OnResolutionChanged -= OnResolutionChanged_RenderCreate;
            On.Terraria.Graphics.Effects.FilterManager.EndCapture -= FilterManager_EndCapture_LSLib; ;
            On.Terraria.Main.DrawProjectiles -= Main_DrawProjectiles_LSLib; ;
            base.Unload();
        }
        private static void LoadTextures(ref Asset<Texture2D>[] assets, string textureName)
        {
            int i = 0;
            string path = $"LogSpiralLibrary/Images/{textureName}/{textureName}_";
            while (true)
            {
                if (ModContent.HasAsset($"{path}{i}"))
                {
                    i++;
                }
                else
                    break;
            }
            assets = new Asset<Texture2D>[i];
            for (int n = 0; n < i; n++)
            {
                assets[n] = ModContent.Request<Texture2D>($"{path}{n}", AssetRequestMode.ImmediateLoad);
            }
        }
    }
    public abstract class RenderBasedDrawing : ModType
    {
        protected override void Register()
        {
            ModTypeLookup<RenderBasedDrawing>.Register(this);
        }
        /// <summary>
        /// 对需要Render的弹幕进行合批绘制！！
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="render">超级画布布！！</param>
        /// <param name="renderAirDistort">专门给空气扭曲用的Render,和上面那个没区别其实</param>
        public abstract void RenderDrawingMethods(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort);
        /// <summary>
        /// 哦但是，如果那些要用到Render的弹幕在Render不可用的时候应该怎么办呢？？
        /// </summary>
        /// <param name="spriteBatch"></param>
        public abstract void CommonDrawingMethods(SpriteBatch spriteBatch);
    }
    public class LogSpiralLibrarySystem : ModSystem
    {
        public static List<RenderBasedDrawing> renderBasedDrawings;
        public override void OnModLoad()
        {
            renderBasedDrawings = new List<RenderBasedDrawing>();
            base.OnModLoad();
        }
        public static double ModTime;
        public static double ModTime2;

        public override void UpdateUI(GameTime gameTime)
        {
            ModTime++;
            ModTime2 += Main.gamePaused ? 0 : 1;
            //Main.NewText(Filters.Scene["CoolerItemVisualEffect:InvertGlass"].GetShader().CombinedOpacity);
        }
    }


}
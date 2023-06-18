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

        public static Effect ItemEffect => itemEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/ItemGlowEffect").Value;
        public static Effect ShaderSwooshEffect => shaderSwooshEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/ShaderSwooshEffect").Value;
        public static Effect ShaderSwooshEX => shaderSwooshEX ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/ShaderSwooshEffectEX").Value;
        public static Effect ShaderSwooshUL => shaderSwooshUL ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/ShaderSwooshEffectUL").Value;
        public static Effect DistortEffect => distortEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/DistortEffect").Value;
        public static Effect FinalFractalTailEffect => finalFractalTailEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/FinalFractalTailEffect").Value;
        public static Effect ColorfulEffect => colorfulEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/ColorfulEffect").Value;
        public static Effect EightTrigramsFurnaceEffect => eightTrigramsFurnaceEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/EightTrigramsFurnaceEffect").Value;
        public static Effect VertexDraw => vertexDraw ??= ModContent.Request<Effect>("StoneOfThePhilosophers/Effects/VertexDraw").Value;
        public static Effect VertexDrawEX => vertexDrawEX ??= ModContent.Request<Effect>("StoneOfThePhilosophers/Effects/VertexDrawEX").Value;
        #endregion

        #region Textures
        public static Asset<Texture2D>[] BaseTex;
        public static Asset<Texture2D>[] AniTex;
        public static Asset<Texture2D>[] HeatMap;
        public static Asset<Texture2D>[] MagicZone;
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

            base.Load();
        }
        public override void Unload()
        {
            Instance = null;
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
                assets[n] = ModContent.Request<Texture2D>($"{path}{n}");
            }
        }
    }
    public class LogSpiralLibrarySystem : ModSystem 
    {
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
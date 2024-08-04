global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria.ModLoader;
global using Terraria;
global using Terraria.ID;
global using Terraria.DataStructures;
global using Terraria.GameContent;
using ReLogic.Content;
using System.Collections.Generic;
using LogSpiralLibrary.CodeLibrary;
using System;
using Terraria.Audio;
using Microsoft.Xna.Framework.Audio;
using LogSpiralLibrary.CodeLibrary.DataStructures;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader.Config;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;
using System.IO;
using ReLogic.Graphics;
using Terraria.ModLoader.Core;

namespace LogSpiralLibrary
{
    public class LogSpiralLibraryMod : Mod
    {
        #region Effects
        private static Effect itemEffect;
        private static Effect itemEffectEX;
        private static Effect shaderSwooshEffect;//第一代刀光effect
        private static Effect shaderSwooshEX;//第二代
        //↑但是很不幸的是，都丢失.fx了，等阿汪做出第三代吧
        private static Effect shaderSwooshUL;//第三代，目前有问题

        private static Effect renderEffect;
        private static Effect finalFractalTailEffect;
        private static Effect colorfulEffect;
        private static Effect eightTrigramsFurnaceEffect;//第一代抛物激光effect，下次做第二代
        private static Effect vertexDraw;
        private static Effect vertexDrawEX;
        private static Effect transformEffect;
        private static Effect transformEffectEX;
        private static Effect fadeEffect;
        private static Effect airDistortEffect;
        public static Effect ItemEffect => itemEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/ItemGlowEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect ItemEffectEX => itemEffectEX ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/ItemGlowEffectEX", AssetRequestMode.ImmediateLoad).Value;
        public static Effect ShaderSwooshEffect => shaderSwooshEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/ShaderSwooshEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect ShaderSwooshEX => shaderSwooshEX ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/ShaderSwooshEffectEX", AssetRequestMode.ImmediateLoad).Value;
        public static Effect ShaderSwooshUL => shaderSwooshUL ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/ShaderSwooshEffectUL", AssetRequestMode.ImmediateLoad).Value;
        public static Effect RenderEffect => renderEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/RenderEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect FinalFractalTailEffect => finalFractalTailEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/FinalFractalTailEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect ColorfulEffect => colorfulEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/ColorfulEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect EightTrigramsFurnaceEffect => eightTrigramsFurnaceEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/EightTrigramsFurnaceEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect VertexDraw => vertexDraw ??= ModContent.Request<Effect>("StoneOfThePhilosophers/Effects/VertexDraw", AssetRequestMode.ImmediateLoad).Value;
        public static Effect VertexDrawEX => vertexDrawEX ??= ModContent.Request<Effect>("StoneOfThePhilosophers/Effects/VertexDrawEX", AssetRequestMode.ImmediateLoad).Value;
        public static Effect TransformEffect => transformEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/TransformEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect TransformEffectEX => transformEffectEX ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/TransformEffectEX", AssetRequestMode.ImmediateLoad).Value;
        public static Effect AirDistortEffect => airDistortEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/AirDistortEffect", AssetRequestMode.ImmediateLoad).Value;
        public static Effect FadeEffect => fadeEffect ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/FadeEffect", AssetRequestMode.ImmediateLoad).Value;
        #endregion

        #region Textures
        public static List<Asset<Texture2D>> BaseTex;
        public static List<Asset<Texture2D>> AniTex;
        public static List<Asset<Texture2D>> HeatMap;
        public static List<Asset<Texture2D>> MagicZone;
        /// <summary>
        /// 杂图，以下是内容表(0-17)
        /// <br>0-2:也许是给物品附魔光泽用的贴图</br>
        /// <br>3:刀光的灰度图，为什么会在这里有一张？？</br>
        /// <br>4-5:箭头和磁场点叉</br>
        /// <br>6:符文条带</br>
        /// <br>7-10:闪电激光</br>
        /// <br>11:车万激光</br>
        /// <br>12:压扁的白色箭头？？</br>
        /// <br>13-17:有些来着原版的Extra，有些是我自己瞎画，给最终分形那些用</br>
        /// <br>18:高斯模糊用加权贴图</br>
        /// <br>19:光玉</br>
        /// <br>20:星空</br>
        /// <br>21:星空2</br>
        /// </summary>
        public static List<Asset<Texture2D>> Misc;
        public static List<Asset<Texture2D>> Fractal;

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
        public RenderTarget2D Render_Swap
        {
            get => render_Swap ??= new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth == 0 ? 1920 : Main.screenWidth, Main.screenHeight == 0 ? 1120 : Main.screenHeight);
            set
            {
                render_Swap = value;
            }
        }
        private RenderTarget2D render_Swap;

        /// <summary>
        /// 处理Bloom效果专用的小RenderTarget2D
        /// </summary>
        private RenderTarget2D Render_Bloom
        {
            get => render_Bloom ??= new RenderTarget2D(Main.graphics.GraphicsDevice, (Main.screenWidth == 0 ? 1920 : Main.screenWidth) / 4, (Main.screenHeight == 0 ? 1120 : Main.screenHeight) / 4);
            set
            {
                render_Bloom = value;
            }
        }
        private RenderTarget2D render_Bloom;
        private void OnResolutionChanged_RenderCreate(Vector2 useless) => Main.RunOnMainThread(CreateRender);
        public void CreateRender()
        {
            if (Render != null) Render.Dispose();
            Render = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth == 0 ? 1920 : Main.screenWidth, Main.screenHeight == 0 ? 1120 : Main.screenHeight);
            if (Render_Swap != null) Render_Swap.Dispose();
            Render_Swap = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth == 0 ? 1920 : Main.screenWidth, Main.screenHeight == 0 ? 1120 : Main.screenHeight);
            if (render_Bloom != null) Render_Bloom.Dispose();
            Render_Bloom = new RenderTarget2D(Main.graphics.GraphicsDevice, (Main.screenWidth == 0 ? 1920 : Main.screenWidth) / 4, (Main.screenHeight == 0 ? 1120 : Main.screenHeight) / 4);
        }
        #endregion
        //public static bool CIVELoaded => ModLoader.HasMod("CoolerItemVisualEffect");
        public override void Load()
        {
            Instance = this;
            LoadTextures(nameof(BaseTex), out BaseTex);
            LoadTextures(nameof(AniTex), out AniTex);
            LoadTextures(nameof(HeatMap), out HeatMap);
            LoadTextures(nameof(MagicZone), out MagicZone);
            LoadTextures(nameof(Misc), out Misc);
            LoadTextures(nameof(Fractal), out Fractal);

            Main.OnResolutionChanged += OnResolutionChanged_RenderCreate;
            Terraria.Graphics.Effects.On_FilterManager.EndCapture += FilterManager_EndCapture_LSLib;
            On_Main.DrawProjectiles += Main_DrawProjectiles_LSLib;
            On_MP3AudioTrack.ReadAheadPutAChunkIntoTheBuffer += MP3AudioTrack_ReadAheadPutAChunkIntoTheBuffer;
            base.Load();
        }

        private void FilterManager_EndCapture_LSLib(Terraria.Graphics.Effects.On_FilterManager.orig_EndCapture orig, Terraria.Graphics.Effects.FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            if (!CanUseRender) goto label;
            foreach (var renderDrawing in LogSpiralLibrarySystem.renderBasedDrawings)
            {
                try
                {
                    renderDrawing.RenderDrawingMethods(Main.spriteBatch, Main.instance.GraphicsDevice, Render, Render_Swap);
                }
                catch
                {
                    goto label;
                }
            }
        label:
            orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, clearColor);

        }

        private void Main_DrawProjectiles_LSLib(Terraria.On_Main.orig_DrawProjectiles orig, Main self)
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
            Terraria.Graphics.Effects.On_FilterManager.EndCapture -= FilterManager_EndCapture_LSLib; ;
            On_Main.DrawProjectiles -= Main_DrawProjectiles_LSLib; ;
            base.Unload();
        }
        private void LoadTextures(string textureName, out List<Asset<Texture2D>> assets)
        {
            string basePath = $"Images/{textureName}/{textureName}_";
            assets = new();
            for (int i = 0; ; i++)
            {
                string path = $"{basePath}{i}";
                if (!RootContentSource.HasAsset(path))
                {
                    break;
                }
                assets.Add(Assets.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad));
            }
        }
        #region TestCode
        public static byte[] musicBuffer;
        public static ulong globalCounter;
        private void MP3AudioTrack_ReadAheadPutAChunkIntoTheBuffer(Terraria.Audio.On_MP3AudioTrack.orig_ReadAheadPutAChunkIntoTheBuffer orig, MP3AudioTrack self)
        {
            //if (NPC.AnyNPCs(ModContent.NPCType<AsraNox>()) && Main.gamePaused && Main.audioSystem is LegacyAudioSystem audioSystem)
            //{
            //    audioSystem.PauseAll();
            //    //var track = audioSystem.AudioTracks[Main.curMusic];

            //    //track.Stop(AudioStopOptions.Immediate);
            //}
            //else
            //{

            //}

            //orig.Invoke(self);

            //var fieldInfo = typeof(MP3AudioTrack).GetField("_mp3Stream", BindingFlags.NonPublic | BindingFlags.Instance);
            //var fieldInfo2 = typeof(MP3AudioTrack).GetField("_bufferToSubmit", BindingFlags.NonPublic | BindingFlags.Instance);
            //var fieldInfo3 = typeof(MP3AudioTrack).GetField("_soundEffectInstance", BindingFlags.NonPublic | BindingFlags.Instance);

            //var mp3str = (XPT.Core.Audio.MP3Sharp.MP3Stream)fieldInfo.GetValue(self);
            //long position = mp3str.Position;
            //Main.NewText($"Mp3Length更新前:{position}");

            //{
            //    byte[] bufferToSubmit = (byte[])fieldInfo2.GetValue(self);
            //    int count = mp3str.Read(bufferToSubmit, 0, bufferToSubmit.Length);
            //    Main.NewText("读取了" + count);
            //    if (count < 1)
            //    {
            //        self.Stop(AudioStopOptions.Immediate);
            //    }
            //    else
            //    {
            //        byte[] newbuffer = new byte[bufferToSubmit.Length];
            //        int offsetor = (int)(ModTime / 10) % 32;
            //        for (int n = 0; n < bufferToSubmit.Length; n++)
            //        {
            //            newbuffer[n] = bufferToSubmit[n];
            //        }
            //        musicBuffer = newbuffer;
            //        ((DynamicSoundEffectInstance)fieldInfo3.GetValue(self)).SubmitBuffer(newbuffer);
            //    }
            //}
            //Main.NewText($"Mp3Length更新后:{mp3str.Position}");
            //Main.NewText($"Mp3Length差值:{mp3str.Position - position}");
            //float MyFunc(float t) => MathF.Cos(t * 55);//t取值范围0,1 函数值取值范围-1，1
            float MyFunc(float t)
            {
                float result = 0f;
                float a = .5f * (Main.GlobalTimeWrappedHourly / 10f).CosFactor();
                float b = 5f;//(Main.GlobalTimeWrappedHourly / 10f).CosFactor() *
                for (int n = 0; n < 20; n++)
                {
                    result += MathF.Pow(a, n) * MathF.Cos(MathF.Pow(b, n) * MathHelper.Pi * t);
                }
                return result;
            }

            byte[] bufferToSubmit = self._bufferToSubmit;
            //Main.NewText(DateTime.Now + "." + DateTime.Now.Millisecond);
            if (self._mp3Stream.Read(bufferToSubmit, 0, bufferToSubmit.Length) < 1)
                self.Stop(AudioStopOptions.Immediate);
            else
            {
                byte[] newbuffer = new byte[bufferToSubmit.Length];
                float scale = 64;
                double second = 1;
                for (int n = 0; n < bufferToSubmit.Length; n++)
                {
                    //newbuffer[n] = (byte)(n/1024%2 == 1 ? 127 : 0);
                    //newbuffer[n] = bufferToSubmit[n];
                    //newbuffer[n] = (byte)((n / 4096f * 220).CosFactor() * 255);
                    //newbuffer[n] = (byte)(MyFunc(n / 4096f * 4) * scale);

                    second = globalCounter / 4096.0 / 43;

                    newbuffer[n] = (byte)(Math.Cos(second * 294 * MathHelper.TwoPi) * 128);
                    globalCounter++;//4096字节对应1/120秒?? 


                    newbuffer[n] = bufferToSubmit[n];

                }
                if (DateTime.Now.Millisecond < 100)
                    //Main.NewText((DateTime.Now.Second, second.ToString("0.00")));
                    musicBuffer = newbuffer;
                self._soundEffectInstance.SubmitBuffer(newbuffer);
            }
        }
        #endregion
    }
    public abstract class RenderBasedDrawing : ModType
    {
        public sealed override void Register()
        {
            ModTypeLookup<RenderBasedDrawing>.Register(this);
            LogSpiralLibrarySystem.renderBasedDrawings.Add(this);
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
        public override void PostSetupContent()
        {

            base.PostSetupContent();
        }
        public static List<RenderBasedDrawing> renderBasedDrawings = new List<RenderBasedDrawing>();
        public static Dictionary<Type, VertexDrawInfo> vertexDrawInfoInstance = new Dictionary<Type, VertexDrawInfo>();
        public override void OnModLoad()
        {
            if (Main.chatMonitor is RemadeChatMonitor remade)
            {
                remade._showCount = 40;
            }
            //renderBasedDrawings = new List<RenderBasedDrawing>();
            //vertexDrawInfoInstance = new Dictionary<Type, VertexDrawInfo>();
            base.OnModLoad();
        }
        public static double ModTime;
        public static double ModTime2;
        public override void UpdateUI(GameTime gameTime)
        {
            ModTime++;
            ModTime2 += Main.gamePaused ? 0 : 1;
            //Main.NewText(vertexDrawInfoInstance.Count);
            //Main.NewText(Filters.Scene["CoolerItemVisualEffect:InvertGlass"].GetShader().CombinedOpacity);
        }
        public static VertexDrawInfo[] vertexEffects = new VertexDrawInfo[100];
        public override void PostUpdateEverything()
        {
            UpdateVertexInfo();
        }
        public static void UpdateVertexInfo() => vertexEffects.UpdateVertexInfo();
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            #region MusicBufferTest
            /*var buffer = LogSpiralLibraryMod.musicBuffer;
            int length = buffer.Length;
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, 1920, 1120), Color.White * .5f);
            //float rows = 16f;
            //float screenHeight = 1024f;
            //for (int n = 0; n < length - 1; n++)
            //{
            //    float factor1 = n / (length - 1f);
            //    float factor2 = (n + 1) / (length - 1f);
            //    float offsetY = (int)(factor1 * rows) / rows;
            //    if (offsetY != (int)(factor2 * rows) / rows) continue;
            //    else offsetY *= screenHeight;
            //    factor1 = factor1 * rows % 1;
            //    factor2 = factor2 * rows % 1;
            //    Main.spriteBatch.DrawLine(new Vector2(factor1 * 1920, buffer[n] * (screenHeight / rows / 255f * .75f) + offsetY), new Vector2(factor2 * 1920, buffer[n + 1] * (screenHeight / rows / 255f * .75f) + offsetY), Color.Red, 4);
            //}
            //for (int n = 0; n < length; n++)
            //{
            //    float factor = n / (length - 1f);
            //    int offsetY = buffer[n];
            //    if (offsetY > 127) offsetY -= 255;
            //    Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(factor * 1920, 560 + offsetY - 127), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 4, 0, 0);
            //}
            for (int n = 0; n < length - 1; n++)
            {
                float factor = n / (length - 1f);
                int offsetY = buffer[n];
                if (offsetY > 127) offsetY -= 255;
                var pos1 = new Vector2(factor * 1920, 560 + offsetY - 127);
                offsetY = buffer[n + 1];
                if (offsetY > 127) offsetY -= 255;
                var pos2 = new Vector2(factor * 1920, 560 + offsetY - 127);
                Main.spriteBatch.DrawLine(pos1, pos2, Color.Red, 4);

            }*/
            #endregion
            //if (ModContent.TryFind<ILocalizedModType>("LogSpiralLibraryMod", "SwooshInfo", out var value))
            //{
            //    var str = value?.GetLocalization("DisplayName", () => "草")?.ToString();
            //    spriteBatch.DrawString(FontAssets.MouseText.Value, str ?? "坏", new Vector2(200, 200), (Main.DiscoColor.ToVector3() * new Vector3(0.25f, 0.5f, 0.75f)).ToColor());
            //}
            //else 
            //{
            //    Main.NewText("找到个锤子");
            //}
            base.PostDrawInterface(spriteBatch);
        }
    }
    public class LogSpitalLibraryRenderDrawing : RenderBasedDrawing
    {
        static void DrawVertexEffects(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            Dictionary<Type, List<VertexDrawInfo>> dict = new();
            foreach (var instance in LogSpiralLibrarySystem.vertexDrawInfoInstance.Values)
            {
                dict.Add(instance.GetType(), new List<VertexDrawInfo>());
            }
            foreach (var vertexDraw in LogSpiralLibrarySystem.vertexEffects)
            {
                if (vertexDraw != null && vertexDraw.Active && dict.TryGetValue(vertexDraw.GetType(), out var list))
                {
                    list.Add(vertexDraw);
                }
            }
            foreach (var pair in dict)
            {
                if (pair.Value.Count > 0)
                    pair.Value.DrawVertexInfo(pair.Key, spriteBatch, graphicsDevice, render, renderAirDistort);
            }
        }
        public override void CommonDrawingMethods(SpriteBatch spriteBatch)
        {
            DrawVertexEffects(spriteBatch, null, null, null);
        }

        public override void RenderDrawingMethods(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            DrawVertexEffects(spriteBatch, graphicsDevice, render, renderAirDistort);
        }

    }
    public class LogSpiralLibraryPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            ModDefinitionElement.locals = ModOrganizer.FindAllMods();
            base.OnEnterWorld();
        }
        public float strengthOfShake;
        public override void ModifyScreenPosition()
        {
            var set = LogSpiralLibraryMiscConfig.Instance.screenShakingSetting;
            if (set.Available)
            {
                strengthOfShake *= 0.6f;
                if (strengthOfShake < 0.025f) strengthOfShake = 0;
                Main.screenPosition += Main.rand.NextVector2Unit() * strengthOfShake * 48 * set.strength;
            }
        }
    }
    public class LogSpiralLibraryMiscConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public static LogSpiralLibraryMiscConfig Instance => ModContent.GetInstance<LogSpiralLibraryMiscConfig>();
        [CustomModConfigItem(typeof(AvailableConfigElement))]
        public ShakingSetting screenShakingSetting = new ShakingSetting();
        public class ShakingSetting : IAvailabilityChangableConfig
        {
            public bool Available { get; set; } = true;
            [Range(0, 1)]
            [DefaultValue(0.25f)]
            public float strength = 0.25f;

        }
    }
}
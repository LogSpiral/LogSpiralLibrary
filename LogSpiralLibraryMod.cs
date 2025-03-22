global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria.ModLoader;
global using Terraria;
global using Terraria.ID;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using MeleeSequence = LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Sequence<LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Melee.MeleeAction>;
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
using NetSimplified;
using NetSimplified.Syncing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.Graphics.Renderers;
using Terraria.Graphics;
using static Terraria.Localization.NetworkText;

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
        private static Effect magicRing;
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
        public static Effect MagicRing => magicRing ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/MagicRing", AssetRequestMode.ImmediateLoad).Value;

        static Effect vanillaPixelShader;
        public static Effect VanillaPixelShader => vanillaPixelShader ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Xnbs/PixelShader", AssetRequestMode.ImmediateLoad).Value;
        #endregion

        #region Textures
        public static List<Asset<Texture2D>> BaseTex;
        public static List<Asset<Texture2D>> AniTex;
        public static List<Asset<Texture2D>> BaseTex_Swoosh;
        public static List<Asset<Texture2D>> AniTex_Swoosh;
        public static List<Asset<Texture2D>> BaseTex_Stab;
        public static List<Asset<Texture2D>> AniTex_Stab;
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
        public static List<Asset<Texture2D>> Mask;
        #endregion

        #region Other
        public static double ModTime => LogSpiralLibrarySystem.ModTime;
        public static double ModTime2 => LogSpiralLibrarySystem.ModTime2;

        public static LogSpiralLibraryMod Instance;

        public static BlendState AllOne;
        public static BlendState InverseColor;
        public static BlendState SoftAdditive;//from yiyang233
        public LogSpiralLibraryMod()
        {
            AllOne = new BlendState();
            AllOne.Name = "BlendState.AllOne";
            AllOne.ColorDestinationBlend = AllOne.AlphaDestinationBlend = AllOne.ColorSourceBlend = AllOne.AlphaSourceBlend = Blend.One;

            InverseColor = new BlendState()
            {
                Name = "BlendState.InverseColor",
                ColorDestinationBlend = Blend.InverseSourceColor,
                ColorSourceBlend = Blend.InverseDestinationColor,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero
            };
            SoftAdditive = new BlendState()
            {
                Name = "BlendState.SoftAdditve",
                ColorDestinationBlend = Blend.One,
                ColorSourceBlend = Blend.InverseDestinationColor,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.SourceAlpha
            };
        }
        #endregion

        #region Render
        public static bool CanUseRender => Lighting.Mode != Terraria.Graphics.Light.LightMode.Retro && Lighting.Mode != Terraria.Graphics.Light.LightMode.Trippy && Main.WaveQuality != 0;

        //public const int tinyScalerInvert = 4;
        RenderTarget2D DirectCreateNewRender(int level = 0)
        {

            var r1 = Main.screenTarget;
            var r2 = Main.screenTargetSwap;
            int invert = 1 << level;
            if (level != 0)
                return new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenTarget.Width / invert, Main.screenTarget.Height / invert);
            else
                return new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenTarget.Width, Main.screenTarget.Height);
        }
        public RenderTarget2D Render
        {
            get => render ??= DirectCreateNewRender();
            set
            {
                render = value;
            }
        }
        private RenderTarget2D render;
        public RenderTarget2D Render_Swap
        {
            get => render_Swap ??= DirectCreateNewRender();
            set
            {
                render_Swap = value;
            }
        }
        private RenderTarget2D render_Swap;
        public RenderTarget2D Render_Swap2
        {
            get => render_Swap2 ??= DirectCreateNewRender();
            set
            {
                render_Swap2 = value;
            }
        }
        private RenderTarget2D render_Swap2;
        /// <summary>
        /// 处理Bloom效果专用的小RenderTarget2D
        /// </summary>
        public RenderTarget2D Render_Tiny
        {
            get => render_Tiny ??= DirectCreateNewRender(1);
            set
            {
                render_Tiny = value;
            }
        }
        private RenderTarget2D render_Tiny;

        /// <summary>
        /// 处理Bloom效果专用的小RenderTarget2D 2号
        /// </summary>
        public RenderTarget2D Render_Tiny_Swap
        {
            get => render_Tiny_Swap ??= DirectCreateNewRender(1);
            set
            {
                render_Tiny_Swap = value;
            }
        }
        private RenderTarget2D render_Tiny_Swap;

        /// <summary>
        /// 处理Bloom效果专用的小RenderTarget2D
        /// </summary>
        public RenderTarget2D Render_Tiniest
        {
            get => render_Tiniest ??= DirectCreateNewRender(2);
            set
            {
                render_Tiniest = value;
            }
        }
        private RenderTarget2D render_Tiniest;

        /// <summary>
        /// 处理Bloom效果专用的小RenderTarget2D 2号
        /// </summary>
        public RenderTarget2D Render_Tiniest_Swap
        {
            get => render_Tiniest_Swap ??= DirectCreateNewRender(2);
            set
            {
                render_Tiniest_Swap = value;
            }
        }
        private RenderTarget2D render_Tiniest_Swap;
        private void OnResolutionChanged_RenderCreate(Vector2 useless) => Main.RunOnMainThread(CreateRender);
        public void CreateRender()
        {
            if (Render != null) Render.Dispose();
            Render = DirectCreateNewRender();
            if (Render_Swap != null) Render_Swap.Dispose();
            Render_Swap = DirectCreateNewRender();
            if (Render_Swap2 != null) Render_Swap2.Dispose();
            Render_Swap2 = DirectCreateNewRender();
            if (Render_Tiny != null) Render_Tiny.Dispose();
            Render_Tiny = DirectCreateNewRender(1);
            if (Render_Tiny_Swap != null) Render_Tiny_Swap.Dispose();
            Render_Tiny_Swap = DirectCreateNewRender(1);
            if (Render_Tiniest != null) Render_Tiniest.Dispose();
            Render_Tiniest = DirectCreateNewRender(2);
            if (Render_Tiniest_Swap != null) Render_Tiniest_Swap.Dispose();
            Render_Tiniest_Swap = DirectCreateNewRender(2);
        }
        #endregion

        //public static bool CIVELoaded => ModLoader.HasMod("CoolerItemVisualEffect");
        public override void Load()
        {
            Instance = this;
            AddContent<NetModuleLoader>();

            if (Main.netMode == NetmodeID.Server) return;

            LoadTextures(nameof(BaseTex), out BaseTex);
            LoadTextures(nameof(AniTex), out AniTex);
            LoadTextures("Swoosh/" + nameof(BaseTex), nameof(BaseTex), out BaseTex_Swoosh);
            LoadTextures("Swoosh/" + nameof(AniTex), nameof(AniTex), out AniTex_Swoosh);
            LoadTextures("Stab/" + nameof(BaseTex), nameof(BaseTex), out BaseTex_Stab);
            LoadTextures("Stab/" + nameof(AniTex), nameof(AniTex), out AniTex_Stab);
            LoadTextures(nameof(HeatMap), out HeatMap);
            LoadTextures(nameof(MagicZone), out MagicZone);
            LoadTextures(nameof(Misc), out Misc);
            LoadTextures(nameof(Fractal), out Fractal);
            LoadTextures(nameof(Mask), out Mask);

            Main.OnResolutionChanged += OnResolutionChanged_RenderCreate;
            //On_Main.SetDisplayMode += On_Main_SetDisplayMode;
            Terraria.Graphics.Effects.On_FilterManager.EndCapture += FilterManager_EndCapture_LSLib;
            On_Main.DrawProjectiles += Main_DrawProjectiles_LSLib;
            //On_MP3AudioTrack.ReadAheadPutAChunkIntoTheBuffer += MP3AudioTrack_ReadAheadPutAChunkIntoTheBuffer;
            //var method = typeof(Mod).GetMethod("get_SourceFolder", BindingFlags.Public | BindingFlags.Instance);
            //MonoModHooks.Add(method, ModSourceFolderOldStyle);
            UIModSources.dotnetSDKFound = true;
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
            if (Main.netMode == NetmodeID.Server) return;

            Main.OnResolutionChanged -= OnResolutionChanged_RenderCreate;
            //On_Main.SetDisplayMode -= On_Main_SetDisplayMode;
            Terraria.Graphics.Effects.On_FilterManager.EndCapture -= FilterManager_EndCapture_LSLib; ;
            On_Main.DrawProjectiles -= Main_DrawProjectiles_LSLib;
            base.Unload();
        }
        private void LoadTextures(string textureName, out List<Asset<Texture2D>> assets)
        {
            string basePath = $"Images/{textureName}/{textureName}_";
            assets = [];
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
        private void LoadTextures(string folderName, string textureName, out List<Asset<Texture2D>> assets)
        {
            string basePath = $"Images/{folderName}/{textureName}_";
            assets = [];
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

        //public enum MessageType
        //{
        //    SequenceSyncAll = 0,
        //    SyncMousePosition = 1,
        //}
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            NetModule.ReceiveModule(reader, whoAmI);
            /*
            MessageType msgType = (MessageType)reader.ReadByte();
            switch (msgType)
            {
                case MessageType.SequenceSyncAll:
                    {
                        int plrIndex = reader.ReadByte();
                        Player plr = Main.player[plrIndex];
                        SequencePlayer sequencePlayer = plr.GetModPlayer<SequencePlayer>();
                        sequencePlayer.ReceiveAllSeqFile(reader);
                        if (Main.netMode == NetmodeID.Server)
                        {
                            sequencePlayer.SyncPlayer(-1, whoAmI, false);
                        }
                        break;
                    }
                case MessageType.SyncMousePosition:
                    {
                        int plrIndex = reader.ReadByte();
                        Player plr = Main.player[plrIndex];
                        LogSpiralLibraryPlayer logSpiralLibraryPlayer = plr.GetModPlayer<LogSpiralLibraryPlayer>();
                        Vector2 tarVec = reader.ReadVector2();
                        logSpiralLibraryPlayer.targetedMousePosition = tarVec;
                        if (Main.netMode == NetmodeID.Server)
                        {
                            ModPacket modPacket = Instance.GetPacket();
                            modPacket.Write((byte)MessageType.SyncMousePosition);
                            modPacket.Write((byte)plrIndex);
                            modPacket.WriteVector2(tarVec);
                            modPacket.Send(-1, whoAmI);
                        }
                        break;
                    }
                //case MessageType.TestMessage: 
                //    {
                //        int plrIndex = reader.ReadByte();
                //        Player plr = Main.player[plrIndex];
                //        SyncTestPlayer syncPlayer = plr.GetModPlayer<SyncTestPlayer>();
                //        syncPlayer.hashCode = reader.ReadInt32();
                //        if (Main.netMode == NetmodeID.Server)
                //        {
                //            syncPlayer.SyncPlayer(-1, whoAmI, true);
                //        }
                //        break;
                //    }
                default:
                    {
                        Main.NewText("意外的数据类型");
                        break;
                    }
            }*/
            base.HandlePacket(reader, whoAmI);
        }
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
        public static List<RenderBasedDrawing> renderBasedDrawings = [];
        public static Dictionary<Type, VertexDrawInfo> vertexDrawInfoInstance = [];
        public override void OnModLoad()
        {
            //if (Main.chatMonitor is RemadeChatMonitor remade)
            //{
            //    remade._showCount = 40;
            //}
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
        public static VertexDrawInfo[] vertexEffects = new VertexDrawInfo[1000];
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
            Dictionary<Type, List<VertexDrawInfo>> dict = [];
            foreach (var instance in LogSpiralLibrarySystem.vertexDrawInfoInstance.Values)
            {
                dict.Add(instance.GetType(), []);
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
                {
                    VertexDrawInfo.DrawVertexInfo(pair.Value, pair.Key, spriteBatch, graphicsDevice, render, renderAirDistort);

                }
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
    [AutoSync]
    public class SyncMousePosition : NetModule
    {
        int whoAmI;
        Vector2 pos;
        public static SyncMousePosition Get(int whoAmI, Vector2 position)
        {
            var result = NetModuleLoader.Get<SyncMousePosition>();
            result.pos = position;
            result.whoAmI = whoAmI;
            return result;
        }
        public override void Receive()
        {
            Main.player[whoAmI].GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition = pos;
            if (Main.dedServ)
            {
                Get(whoAmI, pos).Send(-1, whoAmI);
            }
        }
    }
    [AutoSync]
    public class SyncPlayerPosition : NetModule
    {
        int whoAmI;
        Vector2 pos;
        public static SyncPlayerPosition Get(int whoAmI, Vector2 position)
        {
            var result = NetModuleLoader.Get<SyncPlayerPosition>();
            result.pos = position;
            result.whoAmI = whoAmI;
            return result;
        }
        public override void Receive()
        {
            Main.player[whoAmI].position = pos;
            if (Main.dedServ)
            {
                Get(whoAmI, pos).Send(-1, whoAmI);
            }
        }
    }
    public class LogSpiralLibraryPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            base.OnEnterWorld();
        }
        public float strengthOfShake;
        public bool ultraFallEnable;
        public override void ResetEffects()
        {
            ultraFallEnable = false;

            base.ResetEffects();
        }
        public override void PreUpdate()
        {
            if (ultraFallEnable)
                Player.maxFallSpeed = 214514;

            if (Main.myPlayer == Player.whoAmI)
            {
                targetedMousePosition = Main.MouseWorld;
                //if (Main.netMode == NetmodeID.MultiplayerClient)
                //{
                //    ModPacket modPacket = LogSpiralLibraryMod.Instance.GetPacket();
                //    modPacket.Write((byte)LogSpiralLibraryMod.MessageType.SyncMousePosition);
                //    modPacket.Write((byte)Player.whoAmI);
                //    modPacket.WriteVector2(targetedMousePosition);
                //    modPacket.Send(-1, Player.whoAmI);
                //}
                if ((int)Main.time % 10 == 0)
                    SyncMousePosition.Get(Player.whoAmI, targetedMousePosition).Send();
            }


            base.PreUpdate();
        }
        public override void PreUpdateMovement()
        {
            base.PreUpdateMovement();
        }
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
        public Vector2 targetedMousePosition;
        //public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        //{
        //    SpriteBatch spb = Main.spriteBatch;

        //    spb.End();
        //    spb.Begin(SpriteSortMode.Immediate, BlendState.Additive);
        //    LogSpiralLibraryMod.MagicRing.Parameters["factor"].SetValue(1.0f);
        //    LogSpiralLibraryMod.MagicRing.Parameters["uTimer"].SetValue((float)LogSpiralLibraryMod.ModTime / 60f);
        //    LogSpiralLibraryMod.MagicRing.CurrentTechnique.Passes[0].Apply();
        //    spb.Draw(LogSpiralLibraryMod.BaseTex[8].Value, drawInfo.drawPlayer.Center - Main.screenPosition, null, Color.White, 0, Vector2.One *385 * .5f, 1f, 0, 0);
        //    spb.End();
        //    spb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,RasterizerState.CullNone,null,Main.GameViewMatrix.TransformationMatrix);
        //    base.ModifyDrawInfo(ref drawInfo);
        //}
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

        //当年测试Qot配置中心用的
        /*
        public int[] TestArray = new int[30];

        public Color[] TestColors = new Color[9];

        public HashSet<Color> TestColorSet = [];

        public Dictionary<int, Color> testDict = [];
        public Dictionary<Color,int> testDict2 = [];
        */
    }
}
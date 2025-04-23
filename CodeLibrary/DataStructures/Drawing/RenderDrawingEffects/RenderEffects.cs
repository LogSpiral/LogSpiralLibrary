using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using System;
using System.IO;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using static LogSpiralLibrary.LogSpiralLibraryMod;
using static Terraria.GameContent.Bestiary.IL_BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects
{
    /// <summary>
    /// 一般需要搭配<see cref="VertexDrawInfo"/>使用
    /// </summary>
    public interface IRenderDrawInfo
    {
        void Reset();
        /// <summary>
        /// 绘制前做些手脚(切换rendertarget还有参数准备之类
        /// </summary>
        void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort);
        /// <summary>
        /// rendertarget上现在有图了，整活开始
        /// </summary>
        void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort);

        /// <summary>
        /// 最后将处理结果画到屏幕上的实现程序
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="render"></param>
        /// <param name="renderSwap"></param>
        void DrawToScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap);
        bool Active { get; }
        /// <summary>
        /// 是否独立绘制
        /// <br>请将独立绘制的特效都往前塞——</br>
        /// <br><see cref="VertexDrawInfo.PreDraw"/></br>
        /// <br><see cref="PreDraw"/></br>
        /// <br><see cref="Draw"/></br>
        /// </summary>
        //bool StandAlone { get; }

        /// <summary>
        /// 是否绘制实体(?)
        /// </summary>
        bool DoRealDraw { get; }
    }
    public struct AirDistortEffectInfo(float Range, float Director = 0f, float ColorOffset = 0f, int tier = 1) : IRenderDrawInfo
    {
        public void Reset() => this = new();
        //int tier;
        //public int Tier
        //{
        //    get => tier;
        //    set => tier = Math.Clamp(value, 0, 2);
        //}

        ///// <summary>
        ///// 相对于原图像的缩放大小
        ///// </summary>
        /// <summary>
        /// 步长系数
        /// </summary>
        public float distortScaler = Range;
        ///// <summary>
        ///// 不是导演是方向
        ///// </summary>
        //public Vector2 director;
        /// <summary>
        /// 偏移方向偏移量
        /// </summary>
        public float director = Director;
        /// <summary>
        /// 色差距离
        /// </summary>
        public float colorOffset = ColorOffset;
        public bool DoRealDraw => false;

        public void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            ShaderSwooshUL.Parameters["distortScaler"].SetValue(1.5f);
            graphicsDevice.SetRenderTarget(render);
            graphicsDevice.Clear(new Color(.5f, .5f, .5f));
        }
        public void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            //因为是空气扭曲，不需要对原画布上内容进行另外调整，原有内容现搬空至DrawToScreen
        }
        public void DrawToScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap)
        {

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);//将画布设置为这个
            graphicsDevice.Clear(Color.Transparent);
            Main.instance.GraphicsDevice.Textures[2] = Misc[18].Value;
            AirDistortEffect.Parameters["uScreenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            AirDistortEffect.Parameters["strength"].SetValue(distortScaler);
            AirDistortEffect.Parameters["rotation"].SetValue(Matrix.CreateRotationZ(director));
            AirDistortEffect.Parameters["tex0"].SetValue(render);
            AirDistortEffect.Parameters["colorOffset"].SetValue(colorOffset);
            AirDistortEffect.CurrentTechnique.Passes[3].Apply();//ApplyPass
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);//绘制原先屏幕内容
            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);

            //graphicsDevice.SetRenderTarget(Main.screenTarget);

            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
        public bool Active => distortScaler > 0;
    }
    public struct MaskEffectInfo(Texture2D FillTex, Color GlowColor, float Tier1, float Tier2, Vector2 Offset, bool LightAsAlpha, bool Inverse) : IRenderDrawInfo
    {
        public void Reset() => this = new();

        /// <summary>
        /// 到达阈值之后替换的贴图
        /// </summary>
        public Texture2D fillTex = FillTex;
        public Vector2 TexSize => fillTex.Size();
        /// <summary>
        /// 低于阈值的颜色
        /// </summary>
        public Color glowColor = GlowColor;
        public float tier1 = Tier1;
        public float tier2 = Tier2;
        public Vector2 offset = Offset;
        /// <summary>
        /// 是否让亮度参与透明度的决定
        /// </summary>
        public bool lightAsAlpha = LightAsAlpha;
        /// <summary>
        /// 翻转亮度决定值
        /// </summary>
        public bool inverse = Inverse;
        public bool DoRealDraw => true;
        public void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            ShaderSwooshUL.Parameters["distortScaler"].SetValue(1f);

            graphicsDevice.SetRenderTarget(render);
            graphicsDevice.Clear(Color.Transparent);
        }

        public void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            graphicsDevice.SetRenderTarget(renderSwap);
            graphicsDevice.Clear(Color.Transparent);
            Main.graphics.GraphicsDevice.Textures[1] = fillTex;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            RenderEffect.Parameters["screenScale"].SetValue(Main.ScreenSize.ToVector2());
            RenderEffect.Parameters["tex0"].SetValue(render);
            RenderEffect.Parameters["lightAsAlpha"].SetValue(lightAsAlpha);
            RenderEffect.Parameters["tier1"].SetValue(tier1);
            RenderEffect.Parameters["tier2"].SetValue(tier2);
            RenderEffect.Parameters["offset"].SetValue((float)ModTime * new Vector2(0.707f) + (Main.gameMenu ? default : Main.LocalPlayer.Center));//offset
            RenderEffect.Parameters["maskGlowColor"].SetValue(glowColor.ToVector4());
            RenderEffect.Parameters["ImageSize"].SetValue(TexSize);
            RenderEffect.Parameters["inverse"].SetValue(inverse);
            //spriteBatch.Draw(render, Vector2.Zero, Color.White);
            RenderEffect.CurrentTechnique.Passes[1].Apply();
            spriteBatch.Draw(render, Vector2.Zero, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            graphicsDevice.SetRenderTarget(render);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(renderSwap, Vector2.Zero, Color.White);
            spriteBatch.End();




        }
        public void DrawToScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            
            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            spriteBatch.Draw(renderSwap, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        public bool Active => fillTex != null;
    }
    public struct BloomEffectInfo(float Threshold, float Intensity, float Range, int Times, bool Additive) : IRenderDrawInfo
    {
        public void Reset() => this = new();

        /// <summary>
        /// 阈值
        /// </summary>
        public float threshold = Threshold;
        /// <summary>
        /// 亮度
        /// </summary>
        public float intensity = Intensity;
        /// <summary>
        /// 范围
        /// </summary>
        public float range = Range;
        /// <summary>
        /// 迭代次数
        /// </summary>
        public int times = Times;
        /// <summary>
        /// 是否启动加法模式
        /// </summary>
        public bool additive = Additive;
        public bool DoRealDraw => true;
        public void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            ShaderSwooshUL.Parameters["distortScaler"].SetValue(1f);

            graphicsDevice.SetRenderTarget(render);
            graphicsDevice.Clear(Color.Transparent);
        }
        //static bool useDownSample => false;

        /// <summary>
        /// 是否启用降采样
        /// </summary>
        public bool useDownSample => downSampleLevel != 0;

        public byte downSampleLevel;

        public int downSampleCount => 1 << downSampleLevel;
        /// <summary>
        /// 是否使用MasakiKawase算法
        /// </summary>
        public bool useModeMK;
        public void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap)
        {
            RenderTarget2D renderTiny = downSampleLevel == 1 ? Instance.Render_Tiny : Instance.Render_Tiniest;
            RenderTarget2D renderTinySwap = downSampleLevel == 1 ? Instance.Render_Tiny_Swap : Instance.Render_Tiniest_Swap;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, useDownSample ? Matrix.CreateScale((float)render.Width / renderTiny.Width, (float)render.Height / renderTiny.Height, 1) : Matrix.identity);
            //bool useDownSample = true;
            RenderEffect.Parameters["threshold"].SetValue(threshold);
            RenderEffect.Parameters["range"].SetValue(range);
            RenderEffect.Parameters["intensity"].SetValue(intensity * 2.5f);
            RenderEffect.Parameters["uBloomAdditive"].SetValue(true);
            if (useDownSample)
            {
                RenderEffect.Parameters["screenScale"].SetValue(Main.ScreenSize.ToVector2() / downSampleCount);

                for (int n = 0; n < times - 1; n++)//times是模糊次数(
                {
                    graphicsDevice.SetRenderTarget(renderTinySwap);
                    graphicsDevice.Clear(Color.Transparent);
                    RenderEffect.CurrentTechnique.Passes[useModeMK ? 4 : 3].Apply();
                    if (n != 0)
                        spriteBatch.Draw(renderTiny, Vector2.Zero, Color.White);
                    else
                        spriteBatch.Draw(render, default, null, Color.White, 0, default, 1f / downSampleCount, 0, 0);
                    graphicsDevice.SetRenderTarget(renderTiny);

                    graphicsDevice.Clear(Color.Transparent);
                    RenderEffect.CurrentTechnique.Passes[useModeMK ? 4 : 2].Apply();
                    spriteBatch.Draw(renderTinySwap, Vector2.Zero, Color.White);
                }
                graphicsDevice.SetRenderTarget(renderTinySwap);
                graphicsDevice.Clear(Color.Transparent);
                RenderEffect.CurrentTechnique.Passes[useModeMK ? 4 : 3].Apply();
                if (times > 1)
                    spriteBatch.Draw(renderTiny, Vector2.Zero, Color.White);
                else
                    spriteBatch.Draw(render, default, null, Color.White, 0, default, 1f / downSampleCount, 0, 0);

                graphicsDevice.SetRenderTarget(renderTiny);
                graphicsDevice.Clear(Color.Transparent);
                RenderEffect.CurrentTechnique.Passes[useModeMK ? 4 : 2].Apply();
                spriteBatch.Draw(renderTinySwap, Vector2.Zero, Color.White);
            }
            else
            {
                RenderTarget2D renderSwap2 = Instance.Render_Swap2;
                RenderEffect.Parameters["screenScale"].SetValue(Main.ScreenSize.ToVector2());
                for (int n = 0; n < times - 1; n++)//times是模糊次数(
                {
                    graphicsDevice.SetRenderTarget(renderSwap);
                    graphicsDevice.Clear(Color.Transparent);
                    RenderEffect.CurrentTechnique.Passes[useModeMK ? 4 : 3].Apply();
                    if (n != 0)
                        spriteBatch.Draw(renderSwap2, Vector2.Zero, Color.White);
                    else
                        spriteBatch.Draw(render, Vector2.Zero, Color.White);
                    graphicsDevice.SetRenderTarget(renderSwap2);
                    graphicsDevice.Clear(Color.Transparent);
                    RenderEffect.CurrentTechnique.Passes[useModeMK ? 4 : 2].Apply();
                    spriteBatch.Draw(renderSwap, Vector2.Zero, Color.White);
                }
                graphicsDevice.SetRenderTarget(renderSwap);
                graphicsDevice.Clear(Color.Transparent);
                RenderEffect.CurrentTechnique.Passes[useModeMK ? 4 : 3].Apply();
                if (times > 1)
                    spriteBatch.Draw(renderSwap2, Vector2.Zero, Color.White);
                else
                    spriteBatch.Draw(render, Vector2.Zero, Color.White);
                graphicsDevice.SetRenderTarget(renderSwap2);
                graphicsDevice.Clear(Color.Transparent);
                RenderEffect.CurrentTechnique.Passes[useModeMK ? 4 : 2].Apply();
                spriteBatch.Draw(renderSwap, Vector2.Zero, Color.White);
            }
            spriteBatch.End();
        }
        public void DrawToScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap)
        {
            //bool useDownSample = true;
            var v = Main.graphics.GraphicsDevice.Viewport;
            if (useDownSample)
            {
                var renderTiny = downSampleLevel == 1 ? Instance.Render_Tiny : Instance.Render_Tiniest;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale((float)renderTiny.Width / render.Width, (float)renderTiny.Height / render.Height, 1));
                graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                Main.instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                spriteBatch.Draw(render, Vector2.Zero, Color.White);
                Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
                spriteBatch.Draw(renderTiny, default, null, Color.White, 0, default, downSampleCount, 0, 0);
                graphicsDevice.SetRenderTarget(Main.screenTarget);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            }
            else
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                Main.instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                spriteBatch.Draw(render, Vector2.Zero, Color.White);
                Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
                spriteBatch.Draw(renderSwap, Vector2.Zero, Color.White);
                graphicsDevice.SetRenderTarget(Main.screenTarget);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            }



            spriteBatch.End();
        }

        public bool Active => range > 0 && intensity > 0 && times > 0;
    }
    public struct ArmorDyeInfo(int itemType) : IRenderDrawInfo
    {
        public int type = itemType;

        public void Reset() => this = new();

        public bool Active => GameShaders.Armor._shaderLookupDictionary.ContainsKey(type);

        public bool DoRealDraw => true;

        public void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            graphicsDevice.SetRenderTarget(render);
            graphicsDevice.Clear(Color.Transparent);
        }
        public void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            graphicsDevice.SetRenderTarget(renderSwap);
            graphicsDevice.Clear(Color.Transparent);

            var shaderData = GameShaders.Armor.GetShaderFromItemId(type);
            if (shaderData != null)
            {
                shaderData.Apply(null);
                Vector4 value3 = new (0f, 0f, render.Width, render.Height);
                shaderData.Shader.Parameters["uSourceRect"]?.SetValue(value3);
                shaderData.Shader.Parameters["uLegacyArmorSourceRect"]?.SetValue(value3);
                shaderData.Shader.Parameters["uLegacyArmorSheetSize"]?.SetValue(new Vector2(render.Width, render.Height));
                shaderData.Apply();
            }


            spriteBatch.Draw(render, Vector2.Zero, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            graphicsDevice.SetRenderTarget(render);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(renderSwap, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
        public void DrawToScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            spriteBatch.Draw(renderSwap, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
    }
}

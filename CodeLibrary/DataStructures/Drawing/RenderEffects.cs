using System.IO;
using static LogSpiralLibrary.LogSpiralLibraryMod;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing
{
    public struct AirDistortEffectInfo : VertexDrawInfo.IRenderDrawInfo
    {
        public void Reset() => this = new AirDistortEffectInfo();
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
        public float distortScaler;
        ///// <summary>
        ///// 不是导演是方向
        ///// </summary>
        //public Vector2 director;
        /// <summary>
        /// 偏移方向偏移量
        /// </summary>
        public float director;
        /// <summary>
        /// 色差距离
        /// </summary>
        public float colorOffset;
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
        public AirDistortEffectInfo(float Range, float Director = 0f, float ColorOffset = 0f, int tier = 1)
        {
            distortScaler = Range;
            director = Director;
            colorOffset = ColorOffset;
        }
    }
    public struct MaskEffectInfo : VertexDrawInfo.IRenderDrawInfo
    {
        public void Reset() => this = new MaskEffectInfo();

        /// <summary>
        /// 到达阈值之后替换的贴图
        /// </summary>
        public Texture2D fillTex;
        public Vector2 TexSize => fillTex.Size();
        /// <summary>
        /// 低于阈值的颜色
        /// </summary>
        public Color glowColor;
        public float tier1;
        public float tier2;
        public Vector2 offset;
        /// <summary>
        /// 是否让亮度参与透明度的决定
        /// </summary>
        public bool lightAsAlpha;
        /// <summary>
        /// 翻转亮度决定值
        /// </summary>
        public bool inverse;
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
            RenderEffect.Parameters["offset"].SetValue((float)LogSpiralLibraryMod.ModTime * new Vector2(0.707f) + (Main.gameMenu ? default : Main.LocalPlayer.Center));//offset
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
        public MaskEffectInfo(Texture2D FillTex, Color GlowColor, float Tier1, float Tier2, Vector2 Offset, bool LightAsAlpha, bool Inverse)
        {
            fillTex = FillTex;
            glowColor = GlowColor;
            tier1 = Tier1;
            tier2 = Tier2;
            offset = Offset;
            lightAsAlpha = LightAsAlpha;
            inverse = Inverse;
        }
    }
    public struct BloomEffectInfo : VertexDrawInfo.IRenderDrawInfo
    {
        public void Reset() => this = new BloomEffectInfo();

        /// <summary>
        /// 阈值
        /// </summary>
        public float threshold;
        /// <summary>
        /// 亮度
        /// </summary>
        public float intensity;
        /// <summary>
        /// 范围
        /// </summary>
        public float range;
        /// <summary>
        /// 迭代次数
        /// </summary>
        public int times;
        /// <summary>
        /// 是否启动加法模式
        /// </summary>
        public bool additive;
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
                RenderTarget2D renderSwap2 = LogSpiralLibraryMod.Instance.Render_Swap2;
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
        public BloomEffectInfo(float Threshold, float Intensity, float Range, int Times, bool Additive)
        {
            threshold = Threshold;
            intensity = Intensity;
            range = Range;
            times = Times;
            additive = Additive;
        }
    }
}

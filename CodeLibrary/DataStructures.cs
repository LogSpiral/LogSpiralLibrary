using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config.UI;
using Terraria;
using System.IO;
using static LogSpiralLibrary.LogSpiralLibraryMod;
using Terraria.GameContent.NetModules;

namespace LogSpiralLibrary.CodeLibrary
{
    /// <summary>
    /// 满足一般顶点绘制需求
    /// </summary>
    public struct CustomVertexInfo : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
        {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        });
        public Vector2 Position;
        public Color Color;
        public Vector3 TexCoord;

        public CustomVertexInfo(Vector2 position, Color color, Vector3 texCoord)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
        }
        public CustomVertexInfo(Vector2 position, float alpha, Vector3 texCoord)
        {
            Position = position;
            Color = Color.White with { A = (byte)(MathHelper.Clamp(255 * alpha, 0, 255)) };
            TexCoord = texCoord;
        }
        public CustomVertexInfo(Vector2 position, Vector3 texCoord)
        {
            Position = position;
            Color = Color.White;
            TexCoord = texCoord;
        }

        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return _vertexDeclaration;
            }
        }
    }
    /// <summary>
    /// 支持空间顶点，非常酷齐次坐标
    /// </summary>
    public struct CustomVertexInfoEX : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
        {
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        });
        /// <summary>
        /// 使用齐次坐标！！
        /// </summary>
        public Vector4 Position;
        public Color Color;
        public Vector3 TexCoord;

        public CustomVertexInfoEX(Vector4 position, Color color, Vector3 texCoord)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
        }
        public CustomVertexInfoEX(Vector4 position, float alpha, Vector3 texCoord) : this(position, Color.White with { A = (byte)MathHelper.Clamp(255 * alpha, 0, 255) }, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector4 position, Vector3 texCoord) : this(position, Color.White, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector3 position, Color color, Vector3 texCoord) : this(new Vector4(position, 1), color, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector3 position, float alpha, Vector3 texCoord) : this(new Vector4(position, 1), alpha, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector3 position, Vector3 texCoord) : this(new Vector4(position, 1), texCoord)
        {
        }
        public CustomVertexInfoEX(Vector2 position, Color color, Vector3 texCoord) : this(new Vector4(position, 0, 1), color, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector2 position, float alpha, Vector3 texCoord) : this(new Vector4(position, 0, 1), alpha, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector2 position, Vector3 texCoord) : this(new Vector4(position, 0, 1), texCoord)
        {
        }
        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return _vertexDeclaration;
            }
        }
    }
    /// <summary>
    /// 非常抽象的顶点绘制用结构体
    /// </summary>  
    public abstract class VertexDrawInfo : ModType
    {
        /// <summary>
        /// 需要搭配<see cref="VertexDrawInfo"/>使用
        /// </summary>
        public interface IRenderDrawInfo
        {
            /// <summary>
            /// 绘制前做些手脚(切换rendertarget还有参数准备之类
            /// </summary>
            void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort);
            /// <summary>
            /// rendertarget上现在有图了，整活开始
            /// </summary>
            void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort);
            bool Active { get; }
            /// <summary>
            /// 是否重绘
            /// <br>如果为false且不是第一个就不会执行</br>
            /// <br><see cref="VertexDrawInfo.PreDraw"/></br>
            /// <br><see cref="IRenderDrawInfo.PreDraw"/></br>
            /// <br><see cref="VertexDrawInfo.Draw"/></br>
            /// </summary>
            bool ReDraw { get; set; }
        }
        public sealed override void Register()
        {
            ModTypeLookup<VertexDrawInfo>.Register(this);
            LogSpiralLibrarySystem.vertexDrawInfoInstance.Add(this.GetType(), this);
        }
        /// <summary>
        /// 仅给<see  cref="LogSpiralLibrarySystem.vertexDrawInfoInstance"/>中的实例使用
        /// <br>代码的耦合度持续放飞自我</br>
        /// </summary>
        public abstract IRenderDrawInfo[] RenderDrawInfos { get; }
        /// <summary>
        /// 存在时长
        /// </summary>
        public byte timeLeft;
        /// <summary>
        /// 中心偏移
        /// </summary>
        public Vector2 center;
        /// <summary>
        /// 热度/采样图
        /// </summary>
        public Texture2D heatMap;
        /// <summary>
        /// 颜色插值
        /// </summary>
        public Func<float, Color> color;
        /// <summary>
        /// 是否处于激活状态
        /// </summary>
        public bool Active => timeLeft > 0;
        public float factor => timeLeft / (float)timeLeftMax;
        public bool OnSpawn => timeLeft == timeLeftMax;
        /// <summary>
        /// 缩放大小
        /// </summary>
        public float scaler;
        /// <summary>
        /// 最大存在时长，用于插值
        /// </summary>
        public byte timeLeftMax;
        /// <summary>
        /// 加上11为<see cref="LogSpiralLibraryMod.AniTex"/>的下标，取值范围为[0,5]
        /// </summary>
        public int aniTexIndex;
        /// <summary>
        /// <see cref="LogSpiralLibraryMod.BaseTex"/>的下标，取值范围为[0,11]
        /// </summary>
        public int baseTexIndex;
        public BlendState blendState;
        /// <summary>
        /// 顶点数据，存起来不每帧更新降低运算负担
        /// </summary>
        public abstract CustomVertexInfo[] VertexInfos { get; }
        /// <summary>
        /// 合批绘制前做的事情，这东西是静态性质的实例函数(?
        /// <br>实际上仅由<see cref="LogSpiralLibrarySystem.vertexDrawInfoInstance">中的对应的实例执行</br>
        /// <br>需要<see cref="SpriteBatch.Begin"/>，可以在这里开启画布然后把东西画上之类</br>
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState ?? BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, null, VertexDrawInfo.TransformationMatrix);
        }
        public void DrawPrimitives(float distortScaler) => Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, DrawingMethods.CreateTriList(VertexInfos, center, distortScaler, true), 0, VertexInfos.Length - 2);
        /// <summary>
        /// 单个绘制
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch, IRenderDrawInfo renderDrawInfo, params object[] contextArgument)
        {
            //for (int n = 0; n < VertexInfos.Length - 1; n++) 
            //{
            //    spriteBatch.DrawLine(VertexInfos[n].Position, VertexInfos[n + 1].Position, Color.White, 1, false, -Main.screenPosition);
            //}
            var scaler = 1f;
            if (renderDrawInfo is AirDistortEffectInfo airDistort)
            {
                scaler = airDistort.distortScaler;
            }
            Main.graphics.GraphicsDevice.Textures[0] = LogSpiralLibraryMod.BaseTex[baseTexIndex].Value;
            Main.graphics.GraphicsDevice.Textures[1] = LogSpiralLibraryMod.AniTex[aniTexIndex + 11].Value;
            Main.graphics.GraphicsDevice.Textures[2] = TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value;
            Main.graphics.GraphicsDevice.Textures[3] = heatMap;
            LogSpiralLibraryMod.ShaderSwooshUL.Parameters["lightShift"].SetValue(factor - 1f);
            LogSpiralLibraryMod.ShaderSwooshUL.Parameters["distortScaler"].SetValue(scaler);
            LogSpiralLibraryMod.ShaderSwooshUL.CurrentTechnique.Passes[7].Apply();


            DrawPrimitives(scaler);
        }
        /// <summary>
        /// 合批绘制完毕记得<see cref="SpriteBatch.End"/>
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            spriteBatch.End();
        }
        /// <summary>
        /// 顶点数据更新
        /// </summary>
        public abstract void Uptate();

        /// <summary>
        /// spbdraw那边的矩阵
        /// </summary>
        public static Matrix TransformationMatrix => Main.GameViewMatrix?.TransformationMatrix ?? Matrix.Identity;
        static Matrix projection => Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        static Matrix model => Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        /// <summary>
        /// 丢给顶点坐标变换的矩阵
        /// <br>先右乘<see cref="VertexDrawInfo.model"/>将世界坐标转屏幕坐标</br>
        /// <br>再右乘<see cref="VertexDrawInfo.TransformationMatrix"/>进行画面缩放等</br>
        /// <br>最后右乘<see cref="VertexDrawInfo.projection"/>将坐标压缩至[0,1]</br>
        /// </summary>
        public static Matrix uTransform => model * TransformationMatrix * projection;
    }
    public abstract class MeleeVertexInfo : VertexDrawInfo
    {
        public float rotation;
        public float xScaler;
        public bool negativeDir;
        public bool gather = true;
        /// <summary>
        /// 用来左乘颜色矩阵的系数向量
        /// <br>x:方向渐变</br>
        /// <br>y:武器对角线</br>
        /// <br>z:热度图</br>
        /// </summary>
        public Vector3 ColorVector;
        public bool normalize;
        public override IRenderDrawInfo[] RenderDrawInfos => _rendeDrawInfos;
        IRenderDrawInfo[] _rendeDrawInfos = new IRenderDrawInfo[] { new AirDistortEffectInfo(), new MaskEffectInfo(), new BloomEffectInfo() };
        public override void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            base.PreDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
            Effect effect = LogSpiralLibraryMod.ShaderSwooshUL;
            effect.Parameters["uTransform"].SetValue(VertexDrawInfo.uTransform);
            effect.Parameters["uTime"].SetValue(-(float)LogSpiralLibrarySystem.ModTime * 0.03f);
            effect.Parameters["checkAir"].SetValue(false);
            effect.Parameters["airFactor"].SetValue(1);
            //var _v = modPlayer.ConfigurationSwoosh.directOfHeatMap.ToRotationVector2();
            effect.Parameters["heatRotation"].SetValue(Matrix.Identity);
            effect.Parameters["lightShift"].SetValue(0f);
            effect.Parameters["distortScaler"].SetValue(scaler);
            effect.Parameters["alphaFactor"].SetValue(2);
            effect.Parameters["heatMapAlpha"].SetValue(true);
            effect.Parameters["stab"].SetValue(false);

            //if (flag)
            //    effect.Parameters["AlphaVector"].SetValue(ConfigurationUltraTest.ConfigSwooshUltraInstance.AlphaVector);
            var sampler = SamplerState.AnisotropicWrap;
            Main.graphics.GraphicsDevice.SamplerStates[0] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[3] = SamplerState.AnisotropicClamp;
        }
        public override void Draw(SpriteBatch spriteBatch, IRenderDrawInfo renderDrawInfo, params object[] contextArgument)
        {
            LogSpiralLibraryMod.ShaderSwooshUL.Parameters["gather"].SetValue(gather);
            if (heatMap == null)
            {
                ColorVector = new Vector3(0, 1, 0);
            }
            else if (ColorVector == default)
                ColorVector = new Vector3(0.33f);
            else if (normalize)
                ColorVector /= Vector3.Dot(ColorVector, Vector3.One);
            LogSpiralLibraryMod.ShaderSwooshUL.Parameters["AlphaVector"].SetValue(ColorVector);

            base.Draw(spriteBatch, renderDrawInfo, contextArgument);
        }
        public override void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {

            base.PostDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
        }
    }
    public class UltraStab : MeleeVertexInfo
    {
        #region 参数和属性
        //TODO 改成用ps实现
        CustomVertexInfo[] _vertexInfos = new CustomVertexInfo[90];
        public override CustomVertexInfo[] VertexInfos => _vertexInfos;
        #endregion
        #region 生成函数
        /// <summary>
        /// 生成新的穿刺于指定数组中
        /// </summary>
        public static UltraStab NewUltraStab(
            Color color, VertexDrawInfo[] vertexEffects, byte timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true) => NewUltraStab(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的穿刺于指定数组中
        /// </summary>
        public static UltraStab NewUltraStab(
            Func<float, Color> colorFunc, VertexDrawInfo[] vertexEffects, byte timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true)
        {
            UltraStab result = null;
            for (int n = 0; n < vertexEffects.Length; n++)
            {
                var effect = vertexEffects[n];
                if (effect == null || !effect.Active)
                {
                    effect = vertexEffects[n] = new UltraStab();
                    if (!effect.Active && effect is UltraStab swoosh)
                    {
                        swoosh.color = colorFunc;
                        swoosh.timeLeftMax = swoosh.timeLeft = timeLeft;
                        swoosh.scaler = _scaler;
                        swoosh.center = center ?? Main.LocalPlayer.Center;
                        swoosh.heatMap = heat;
                        swoosh.negativeDir = _negativeDir;
                        swoosh.rotation = _rotation;
                        swoosh.xScaler = xscaler;
                        result = swoosh;
                        swoosh.aniTexIndex = _aniIndex;
                        swoosh.baseTexIndex = _baseIndex;
                        swoosh.ColorVector = colorVec == default ? new Vector3(0.33f) : colorVec;
                        swoosh.normalize = normalize;
                    }
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 生成新的穿刺于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static UltraStab NewUltraStab(
            Color color, byte timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true) =>
            NewUltraStab(color, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的穿刺于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static UltraStab NewUltraStab(
            Func<float, Color> colorFunc, byte timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true) =>
            NewUltraStab(colorFunc, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        #endregion
        #region 绘制和更新，主体
        public override void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            base.PreDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
            //LogSpiralLibraryMod.ShaderSwooshUL.Parameters["stab"].SetValue(true);
            //graphicsDevice.SetRenderTarget(render);
            //graphicsDevice.Clear(Color.White);
        }

        public override void Draw(SpriteBatch spriteBatch, IRenderDrawInfo renderDrawInfo, params object[] contextArgument)
            => base.Draw(spriteBatch, renderDrawInfo, contextArgument);
        public override void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
            => base.PostDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
        public override void Uptate()
        {
            //if (OnSpawn)
            //{
            //    var realColor = Color.White;
            //    Vector2 offsetVec = 20f * new Vector2(8, 3 / xScaler) * scaler;
            //    if (negativeDir) offsetVec.Y *= -1;
            //    VertexInfos[0] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 1, 0.5f));
            //    VertexInfos[2] = new CustomVertexInfo(center + (offsetVec with { X = 0 }).RotatedBy(rotation), realColor, new Vector3(1, 1, 0.5f));
            //    offsetVec.Y *= -1;
            //    VertexInfos[1] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 0, 0.5f));
            //    VertexInfos[3] = new CustomVertexInfo(center + (offsetVec with { X = 0 }).RotatedBy(rotation), realColor, new Vector3(1, 0, 0.5f));
            //}
            for (int i = 0; i < 45; i++)
            {
                var f = i / 44f;
                var num = 1 - factor;
                var realColor = color.Invoke(f);
                //realColor.A = (byte)((1 - f).HillFactor2(1) * 255);
                float width;
                var t = 8 * f;
                if (t < 1) width = t;
                else if (t < 2) width = .5f + 1 / (3 - t);
                else if (t < 3.5f) width = 0.66f + 5f / (t - 1) / 6f;
                else if (t < 5.5f) width = 1;
                else width = 2.6f + 52 / (5 * t - 60);
                Vector2 offsetVec = scaler / 8 * new Vector2(t, width / xScaler * 2);
                if (negativeDir) offsetVec.Y *= -1;
                VertexInfos[2 * i] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(1 - f, 1, 0.5f));
                offsetVec.Y *= -1;
                VertexInfos[2 * i + 1] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 0, 0.5f));
            }
            timeLeft--;
        }
        #endregion
    }
    public class UltraSwoosh : MeleeVertexInfo
    {
        #region 参数和属性
        CustomVertexInfo[] _vertexInfos = new CustomVertexInfo[90];
        public override CustomVertexInfo[] VertexInfos => _vertexInfos;

        public (float from, float to) angleRange;
        #endregion
        #region 生成函数
        /// <summary>
        /// 生成新的剑气于指定数组中
        /// </summary>
        public static T NewUltraSwoosh<T>(
            Color color, VertexDrawInfo[] vertexEffects, byte timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true) where T : UltraSwoosh, new() => NewUltraSwoosh<T>(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);
        /// <summary>
        /// 生成新的剑气于指定数组中，给子类用
        /// </summary>
        public static T NewUltraSwoosh<T>(
            Func<float, Color> colorFunc, VertexDrawInfo[] vertexEffects, byte timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true) where T : UltraSwoosh, new()
        {
            T result = null;
            for (int n = 0; n < vertexEffects.Length; n++)
            {
                var effect = vertexEffects[n];
                if (effect == null || !effect.Active)
                {
                    if (effect == null) effect = vertexEffects[n] = new T();

                    if (!effect.Active && effect is T swoosh)
                    {
                        swoosh.color = colorFunc;
                        swoosh.timeLeftMax = swoosh.timeLeft = timeLeft;
                        swoosh.scaler = _scaler;
                        swoosh.center = center ?? Main.LocalPlayer.Center;
                        swoosh.heatMap = heat;
                        swoosh.negativeDir = _negativeDir;
                        swoosh.rotation = _rotation;
                        swoosh.xScaler = xscaler;
                        swoosh.angleRange = angleRange ?? (-1.125f, 0.7125f);

                        swoosh.aniTexIndex = _aniIndex;
                        swoosh.baseTexIndex = _baseIndex;
                        swoosh.ColorVector = colorVec == default ? new Vector3(0.33f) : colorVec;
                        swoosh.normalize = normalize;
                        result = swoosh;
                    }
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 生成新的剑气于指定数组中
        /// </summary>
        public static UltraSwoosh NewUltraSwoosh(
            Func<float, Color> colorFunc, VertexDrawInfo[] vertexEffects, byte timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true)
        {
            UltraSwoosh result = null;
            for (int n = 0; n < vertexEffects.Length; n++)
            {
                var effect = vertexEffects[n];
                if (effect == null || !effect.Active)
                {
                    if (effect == null) effect = vertexEffects[n] = new UltraSwoosh();

                    if (!effect.Active && effect is UltraSwoosh swoosh)
                    {
                        swoosh.color = colorFunc;
                        swoosh.timeLeftMax = swoosh.timeLeft = timeLeft;
                        swoosh.scaler = _scaler;
                        swoosh.center = center ?? Main.LocalPlayer.Center;
                        swoosh.heatMap = heat;
                        swoosh.negativeDir = _negativeDir;
                        swoosh.rotation = _rotation;
                        swoosh.xScaler = xscaler;
                        swoosh.angleRange = angleRange ?? (-1.125f, 0.7125f);
                        result = swoosh;

                        swoosh.aniTexIndex = _aniIndex;
                        swoosh.baseTexIndex = _baseIndex;
                        swoosh.ColorVector = colorVec == default ? new Vector3(0.33f) : colorVec;
                        swoosh.normalize = normalize;
                    }
                    break;
                }
            }
            return result;
        }

        public static UltraSwoosh NewUltraSwoosh(
    Color color, VertexDrawInfo[] vertexEffects, byte timeLeft = 30, float _scaler = 1f,
    Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
    (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true) => NewUltraSwoosh(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的剑气于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static UltraSwoosh NewUltraSwoosh(
            Color color, byte timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true) => NewUltraSwoosh(color, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的剑气于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static UltraSwoosh NewUltraSwoosh(
            Func<float, Color> colorFunc, byte timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = true) => NewUltraSwoosh(colorFunc, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);
        #endregion
        #region 绘制和更新，主体
        public override void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
            => base.PreDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
        public override void Draw(SpriteBatch spriteBatch, IRenderDrawInfo renderDrawInfo, params object[] contextArgument)
        {
            //for (int n = 0; n < 59; n++) 
            //{
            //    spriteBatch.DrawLine(VertexInfos[n].Position, VertexInfos[n + 1].Position, Color.Red, 4, false, -Main.screenPosition);
            //}
            base.Draw(spriteBatch, renderDrawInfo, contextArgument);
        }

        public override void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
            => base.PostDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
        public override void Uptate()
        {
            timeLeft--;
            for (int i = 0; i < 45; i++)
            {
                var f = i / 44f;
                var num = 1 - factor;
                //float theta2 = (1.8375f * MathHelper.Lerp(num, 1f, f) - 1.125f) * MathHelper.Pi;
                var lerp = f.Lerp(num, 1);
                float theta2 = MathHelper.Lerp(angleRange.from, angleRange.to, lerp) * MathHelper.Pi + MathHelper.Pi;
                if (negativeDir) theta2 = MathHelper.TwoPi - theta2;
                Vector2 offsetVec = (theta2.ToRotationVector2() * new Vector2(1, 1 / xScaler)).RotatedBy(rotation) * scaler;
                Vector2 adder = (offsetVec * 0.05f + rotation.ToRotationVector2() * 2f) * num;
                var realColor = color.Invoke(f);
                realColor.A = (byte)((1 - f).HillFactor2(1) * 255);
                VertexInfos[2 * i] = new CustomVertexInfo(center + offsetVec + adder, realColor, new Vector3(1 - f, 1, 0.5f));
                VertexInfos[2 * i + 1] = new CustomVertexInfo(center + adder, realColor, new Vector3(0, 0, 0.5f));
            }
        }
        #endregion
    }
    public struct AirDistortEffectInfo : VertexDrawInfo.IRenderDrawInfo
    {
        /// <summary>
        /// 相对于原图像的缩放大小
        /// </summary>
        public float distortScaler;
        /// <summary>
        /// 不是导演是方向
        /// </summary>
        public Vector2 director;
        public bool ReDraw 
        {
            get => true;
            set => Main.NewText("这货不需要设置，固定是true的");
        }

        public void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            graphicsDevice.SetRenderTarget(renderAirDistort);
            graphicsDevice.Clear(Color.Transparent);
        }
        public void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);//将画布设置为这个
            graphicsDevice.Clear(Color.Transparent);
            Main.instance.GraphicsDevice.Textures[2] = LogSpiralLibraryMod.Misc[18].Value;
            LogSpiralLibraryMod.AirDistortEffect.Parameters["uScreenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            LogSpiralLibraryMod.AirDistortEffect.Parameters["strength"].SetValue(.0005f);
            LogSpiralLibraryMod.AirDistortEffect.Parameters["rotation"].SetValue(Matrix.Identity);
            LogSpiralLibraryMod.AirDistortEffect.Parameters["tex0"].SetValue(renderAirDistort);
            AirDistortEffect.Parameters["colorOffset"].SetValue(0);
            LogSpiralLibraryMod.AirDistortEffect.CurrentTechnique.Passes[0].Apply();//ApplyPass
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);//绘制原先屏幕内容
            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
        public bool Active => distortScaler > 0 && director != default;
        public AirDistortEffectInfo(float Range, Vector2 Director)
        {
            distortScaler = Range;
            director = Director;
        }
    }
    public struct MaskEffectInfo : VertexDrawInfo.IRenderDrawInfo
    {
        /// <summary>
        /// 到达阈值之后替换的贴图
        /// </summary>
        public Texture2D fillTex;
        /// <summary>
        /// 贴图大小，方便缩放 欸，为什么不搞成属性
        /// </summary>
        public Vector2 texSize;
        /// <summary>
        /// 低于阈值的颜色
        /// </summary>
        public Color glowColor;
        /// <summary>
        /// 中层边界颜色
        /// </summary>
        public Color boundColor;
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
        public bool ReDraw { get; set; } = true;
        public void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            graphicsDevice.SetRenderTarget(render);
            graphicsDevice.Clear(Color.Transparent);
        }

        public void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            graphicsDevice.SetRenderTarget(renderAirDistort);
            graphicsDevice.Clear(Color.Transparent);
            Main.graphics.GraphicsDevice.Textures[1] = fillTex;
            RenderEffect.Parameters["tex0"].SetValue(render);
            RenderEffect.Parameters["invAlpha"].SetValue(tier1);
            RenderEffect.Parameters["lightAsAlpha"].SetValue(lightAsAlpha);
            RenderEffect.Parameters["tier2"].SetValue(tier2);
            RenderEffect.Parameters["position"].SetValue(offset);
            RenderEffect.Parameters["maskGlowColor"].SetValue(glowColor.ToVector4());
            RenderEffect.Parameters["maskBoundColor"].SetValue(boundColor.ToVector4());
            RenderEffect.Parameters["ImageSize"].SetValue(texSize);
            RenderEffect.Parameters["inverse"].SetValue(inverse);
            //spriteBatch.Draw(render, Vector2.Zero, Color.White);
            RenderEffect.CurrentTechnique.Passes[1].Apply();
            spriteBatch.Draw(render, Vector2.Zero, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            graphicsDevice.SetRenderTarget(render);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(renderAirDistort, Vector2.Zero, Color.White);

            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            spriteBatch.Draw(renderAirDistort, Vector2.Zero, Color.White);

            spriteBatch.End();

        }
        public bool Active => fillTex != null;
        public MaskEffectInfo(Texture2D FillTex, Vector2 TexSize, Color GlowColor, Color BoundColor, float Tier1, float Tier2, Vector2 Offset, bool LightAsAlpha, bool Inverse)
        {
            fillTex = FillTex;
            texSize = TexSize;
            glowColor = GlowColor;
            boundColor = BoundColor;
            tier1 = Tier1;
            tier2 = Tier2;
            offset = Offset;
            lightAsAlpha = LightAsAlpha;
            inverse = Inverse;
        }
    }
    public struct BloomEffectInfo : VertexDrawInfo.IRenderDrawInfo
    {
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
        public bool ReDraw { get; set; } = true;
        public void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            graphicsDevice.SetRenderTarget(render);
            graphicsDevice.Clear(Color.Transparent);
        }

        public void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            RenderEffect.Parameters["offset"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));

            RenderEffect.Parameters["position"].SetValue(new Vector2(threshold, range));
            RenderEffect.Parameters["tier2"].SetValue(intensity);
            RenderEffect.Parameters["invAlpha"].SetValue(0.9f);
            for (int n = 0; n < times - 1; n++)
            {
                graphicsDevice.SetRenderTarget(renderAirDistort);
                RenderEffect.Parameters["tex0"].SetValue(render);
                graphicsDevice.Clear(Color.Transparent);
                RenderEffect.CurrentTechnique.Passes[9].Apply();
                spriteBatch.Draw(render, Vector2.Zero, Color.White);
                graphicsDevice.SetRenderTarget(render);
                RenderEffect.Parameters["tex0"].SetValue(renderAirDistort);
                graphicsDevice.Clear(Color.Transparent);
                RenderEffect.CurrentTechnique.Passes[8].Apply();
                spriteBatch.Draw(renderAirDistort, Vector2.Zero, Color.White);
            }
            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            RenderEffect.Parameters["tex0"].SetValue(render);
            RenderEffect.CurrentTechnique.Passes[9].Apply();
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);



            graphicsDevice.SetRenderTarget(Main.screenTarget);
            graphicsDevice.Clear(Color.Transparent);
            RenderEffect.CurrentTechnique.Passes[8].Apply();
            spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            Main.instance.GraphicsDevice.BlendState = AllOne;
            spriteBatch.Draw(render, Vector2.Zero, Color.White);
            Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
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
    /// <summary>
    /// 脑袋空空的效果
    /// <br>我的意思是，什么效果也没有，单纯的占位符(x</br>
    /// <br>你永远可以怀疑螺线的写码水平</br>
    /// </summary>
    public struct EmptyEffectInfo : VertexDrawInfo.IRenderDrawInfo
    {
        public bool Active => true;
        public bool ReDraw { get; set; }

        public void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
        }

        public void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
        }
    }
    public class ComplexPanelInfo
    {
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation)
        {
            int count = (int)(length / 192f) + 1;
            Vector2 start = rotation.ToRotationVector2() * length * .5f;
            Vector2 end = center + start;
            start = end - 2 * start;
            float lengthScaler = length / 192f / count;
            for (int n = 0; n < count; n++)
            {
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(336, 0, 192, 40), Color.White, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
            }

        }
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation, Color glowLight)
        {
            int count = (int)(length / 192f) + 1;
            Vector2 start = rotation.ToRotationVector2() * length * .5f;
            Vector2 end = center + start;
            start = end - 2 * start;
            float lengthScaler = length / 192f / count;
            for (int n = 0; n < count; n++)
            {
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(336, 0, 192, 40), Color.White, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(530, 0, 192, 40), glowLight, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
            }
        }
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation, Color glowLight, float glowShakingStrength, float glowHueOffsetRange = .2f)
        {
            int count = (int)(length / 192f) + 1;
            Vector2 start = rotation.ToRotationVector2() * length * .5f;
            Vector2 end = center + start;
            start = end - 2 * start;
            float lengthScaler = length / 192f / count;
            for (int n = 0; n < count; n++)
            {
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(336, 0, 192, 40), Color.White, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                if (glowShakingStrength == 0)
                    spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(530, 0, 192, 40), glowLight, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                else
                    for (int k = 0; k < 4; k++)
                        spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, Main.rand.NextFloat(4f * glowShakingStrength)), new Rectangle(530, 0, 192, 40), ModifyHueByRandom(glowLight, glowHueOffsetRange), rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);

            }
        }
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation, Color glowLight, float glowShakingStrength, int count, float glowHueOffsetRange = .2f)
        {
            Vector2 start = rotation.ToRotationVector2() * length * .5f;
            Vector2 end = center + start;
            start = end - 2 * start;
            float lengthScaler = length / 192f / count;
            for (int n = 0; n < count; n++)
            {
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(336, 0, 192, 40), Color.White, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                if (glowShakingStrength == 0)
                    spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(530, 0, 192, 40), glowLight, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                else
                    for (int k = 0; k < 4; k++)
                        spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, Main.rand.NextFloat(4f * glowShakingStrength)), new Rectangle(530, 0, 192, 40), ModifyHueByRandom(glowLight, glowHueOffsetRange), rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);

            }
        }
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation, Color glowLight, float glowShakingStrength, int? count, float glowHueOffsetRange = .2f)
        {
            if (count == null) DrawComplexPanel_Bound(spriteBatch, texture, center, length, widthScaler, rotation, glowLight, glowShakingStrength, glowHueOffsetRange);
            else DrawComplexPanel_Bound(spriteBatch, texture, center, length, widthScaler, rotation, glowLight, glowShakingStrength, count.Value, glowHueOffsetRange);
        }
        /// <summary>
        /// 指定背景图
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="frame"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawComplexPanel_BackGround(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Rectangle frame, Vector2 size, Color color)
        {
            (float sizeX, float sizeY) = (size.X, size.Y);
            int countX = (int)(destination.Width / sizeX) + 1;
            int countY = (int)(destination.Height / sizeY) + 1;
            float width = frame.Width;
            for (int i = 0; i < countX; i++)
            {
                if (i == countX - 1) width = (destination.Width - i * sizeX) / sizeX * width;
                float height = frame.Height;
                for (int j = 0; j < countY; j++)
                {
                    if (j == countY - 1) height = (destination.Height - j * sizeY) / sizeY * height;
                    spriteBatch.Draw(texture, destination.TopLeft() + new Vector2(i * sizeX, j * sizeY), new Rectangle(frame.X, frame.Y, (int)width, (int)height), color, 0, default, new Vector2(sizeX, sizeY) / frame.Size() * 1.025f, 0, 0);
                }
            }
        }
        /// <summary>
        /// 使用config材质
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="texture"></param>
        /// <param name="rectangle"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        public static void DrawComplexPanel_BackGround(SpriteBatch spriteBatch, Texture2D texture, Rectangle rectangle, Vector2 size)
        {
            int countX = (int)(rectangle.Width / size.X) + 1;
            int countY = (int)(rectangle.Height / size.Y) + 1;
            float width = 40;
            for (int i = 0; i < countX; i++)
            {
                if (i == countX - 1) width = (rectangle.Width - i * size.X) / size.X * 40;
                float height = 40;
                for (int j = 0; j < countY; j++)
                {
                    if (j == countY - 1) height = (rectangle.Height - j * size.Y) / size.Y * 40;
                    spriteBatch.Draw(texture, rectangle.TopLeft() + new Vector2(i * size.X, j * size.Y), new Rectangle(210, 0, (int)width, (int)height), Color.White, 0, default, size / 40f * 1.025f, 0, 0);
                }
            }
        }
        public static Color ModifyHueByRandom(Color color, float range)
        {
            var alpha = color.A;
            var vec = Main.rgbToHsl(color);
            vec.X += Main.rand.NextFloat(-range, range);
            while (vec.X < 0) vec.X++;
            vec.X %= 1;
            return Main.hslToRgb(vec) with { A = alpha };
        }
        #region 背景
        /// <summary>
        /// 指定背景贴图，为null的时候使用默认背景
        /// </summary>
        public Texture2D? backgroundTexture;
        public virtual Texture2D StyleTexture { get; set; }
        /// <summary>
        /// 指定贴图背景的部分，和绘制那边一个用法
        /// </summary>
        public Rectangle? backgroundFrame;
        /// <summary>
        /// 单位大小，最后是进行平铺的
        /// </summary>
        public Vector2 backgroundUnitSize;
        /// <summary>
        /// 颜色，可以试试半透明的，很酷
        /// </summary>
        public Color backgroundColor;
        #endregion

        #region 边框
        /// <summary>
        /// 指定横向边界数
        /// </summary>
        public int? xBorderCount;
        /// <summary>
        /// 指定纵向边界数
        /// </summary>
        public int? yBorderCount;
        /// <summary>
        /// 外发光颜色
        /// </summary>
        public Color glowEffectColor;
        /// <summary>
        /// 外发光震动剧烈程度
        /// </summary>
        public float glowShakingStrength;
        /// <summary>
        /// 外发光色调偏移范围
        /// </summary>
        public float glowHueOffsetRange;
        #endregion

        #region 全局
        public Color mainColor;
        public Vector2 origin;
        public float scaler = 1f;
        public Vector2 offset;
        public Rectangle destination;
        #endregion

        public Rectangle ModifiedRectangle
        {
            get
            {
                Vector2 size = destination.Size() * scaler;
                //Vector2 topLeft = (origin - destination.TopLeft()) * scaler + offset;
                Vector2 topLeft = origin * (1 - scaler) + destination.TopLeft() + offset;
                return VectorsToRectangle(topLeft, size);
            }
        }
        public static Rectangle VectorsToRectangle(Vector2 topLeft, Vector2 size)
        {
            return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.X, (int)size.Y);
        }
        public ComplexPanelInfo()
        {
            mainColor = Color.White;
        }
        public virtual Rectangle DrawComplexPanel(SpriteBatch spriteBatch)
        {
            var rectangle = ModifiedRectangle;
            #region 参数准备
            //ConfigElement.DrawPanel2(spriteBatch, rectangle.TopLeft(), TextureAssets.SettingsPanel.Value, rectangle.Width, rectangle.Height, color);
            Vector2 center = rectangle.Center();
            Vector2 scalerVec = rectangle.Size() / new Vector2(64);
            var clampVec = Vector2.Clamp(scalerVec, default, Vector2.One);
            bool flagX = scalerVec.X == clampVec.X;
            bool flagY = scalerVec.Y == clampVec.Y;
            Texture2D texture = StyleTexture;
            float left = flagX ? center.X : rectangle.X + 32;
            float top = flagY ? center.Y : rectangle.Y + 32;
            float right = flagX ? center.X : rectangle.X + rectangle.Width - 32;
            float bottom = flagY ? center.Y : rectangle.Y + rectangle.Height - 32;
            #endregion
            #region 背景
            //spriteBatch.Draw(texture, rectangle, new Rectangle(210, 0, 40, 40), Color.White);
            if (backgroundTexture != null) 
            {
                DrawComplexPanel_BackGround(spriteBatch, backgroundTexture, rectangle, backgroundFrame ?? new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height), backgroundUnitSize * scaler, backgroundColor);

            }
            else 
            {
                DrawComplexPanel_BackGround(spriteBatch, texture, rectangle, new Vector2(40 * scaler));
            }
            #endregion
            #region 四个边框
            DrawComplexPanel_Bound(spriteBatch, texture, new Vector2(left - 28 * clampVec.X, center.Y), rectangle.Height - 24, clampVec.X, -MathHelper.PiOver2, glowEffectColor, glowShakingStrength, yBorderCount, glowHueOffsetRange);
            DrawComplexPanel_Bound(spriteBatch, texture, new Vector2(right + 28 * clampVec.X, center.Y), rectangle.Height - 24, clampVec.X, MathHelper.PiOver2, glowEffectColor, glowShakingStrength, yBorderCount, glowHueOffsetRange);
            DrawComplexPanel_Bound(spriteBatch, texture, new Vector2(center.X, top - 28 * clampVec.Y), rectangle.Width - 24, clampVec.Y, 0, glowEffectColor, glowShakingStrength, xBorderCount, glowHueOffsetRange);
            DrawComplexPanel_Bound(spriteBatch, texture, new Vector2(center.X, bottom + 28 * clampVec.Y), rectangle.Width - 24, clampVec.Y, MathHelper.Pi, glowEffectColor, glowShakingStrength, xBorderCount, glowHueOffsetRange);
            #endregion
            #region 四个角落
            spriteBatch.Draw(texture, new Vector2(left, top), new Rectangle(0, 0, 40, 40), Color.White, 0, new Vector2(40), clampVec, 0, 0);
            spriteBatch.Draw(texture, new Vector2(left, bottom), new Rectangle(42, 0, 40, 40), Color.White, 0, new Vector2(40, 0), clampVec, SpriteEffects.FlipVertically, 0);
            spriteBatch.Draw(texture, new Vector2(right, bottom), new Rectangle(42, 0, 40, 40), Color.White, MathHelper.Pi, new Vector2(40), clampVec, 0, 0);
            spriteBatch.Draw(texture, new Vector2(right, top), new Rectangle(42, 0, 40, 40), Color.White, 0, new Vector2(0, 40), clampVec, SpriteEffects.FlipHorizontally, 0);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(left, top), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 4f, 0, 0);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(960, 560), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 4f, 0, 0);

            #endregion
            return rectangle;
        }
    }
    public interface IVertexTriangle
    {
        //这个不好用（明明是阿汪你不会用
        CustomVertexInfo A { get; }
        CustomVertexInfo B { get; }
        CustomVertexInfo C { get; }
        CustomVertexInfo this[int index] { get; }
        //CustomVertexInfo[] ToVertexInfo(IVertexTriangle[] tris);
    }
    public struct VertexTriangle3List
    {
        //ListOfTriangleIn3DSpace
        public int Length => tris.Length;
        public float height;
        public Vector2 offset;
        public VertexTriangle3[] tris;
        public VertexTriangle3 this[int i] => tris[i];

        public VertexTriangle3List(float _height, Vector2 _offset, params VertexTriangle3[] _tris)
        {
            height = _height;
            offset = _offset;
            tris = _tris;
        }
        public Vector2 Projectile(Vector3 vector) => height / (height - vector.Z) * (new Vector2(vector.X, vector.Y) + offset - Main.screenPosition - new Vector2(960, 560)) + Main.screenPosition + new Vector2(960, 560);
        public CustomVertexInfo[] ToVertexInfo()
        {
            var vis = new CustomVertexInfo[tris.Length * 3];
            for (int i = 0; i < tris.Length; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    var t = tris[i];
                    vis[i * 3 + n] = new CustomVertexInfo(Projectile(t.positions[n]), t.colors[n], t.vertexs[n]);
                }
            }
            return vis;
        }
    }
    public struct VertexTriangle3 : IVertexTriangle
    {
        //TriangleIn3DSpace
        public VertexTriangle3(Vector3 vA, Vector3 vB, Vector3 vC, Color cA, Color cB, Color cC, Vector3 pA = default, Vector3 pB = default, Vector3 pC = default)
        {
            positions = new Vector3[3];
            vertexs = new Vector3[3];
            colors = new Color[3];
            vertexs[0] = vA;
            vertexs[1] = vB;
            vertexs[2] = vC;
            colors[0] = cA;
            colors[1] = cB;
            colors[2] = cC;
            positions[0] = pA;
            positions[1] = pB;
            positions[2] = pC;
        }
        public static float height = 100;
        public readonly Vector3[] positions;
        public readonly Vector3[] vertexs;
        public readonly Color[] colors;
        public static Vector2 offset = default;
        public static Vector2 Projectile(Vector3 vector) => height / (height - vector.Z) * (new Vector2(vector.X, vector.Y) + offset - Main.screenPosition - new Vector2(960, 560)) + Main.screenPosition + new Vector2(960, 560);
        public CustomVertexInfo this[int index] => new CustomVertexInfo(Projectile(positions[index]), colors[index], vertexs[index]);
        public CustomVertexInfo A => new CustomVertexInfo(Projectile(positions[0]), colors[0], vertexs[0]);
        public CustomVertexInfo B => new CustomVertexInfo(Projectile(positions[1]), colors[1], vertexs[1]);
        public CustomVertexInfo C => new CustomVertexInfo(Projectile(positions[2]), colors[2], vertexs[2]);
        public static CustomVertexInfo[] ToVertexInfo(VertexTriangle3[] tris)
        {
            var vis = new CustomVertexInfo[tris.Length * 3];
            for (int i = 0; i < tris.Length; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    var t = tris[i];
                    vis[i * 3 + n] = new CustomVertexInfo(Projectile(t.positions[n]), t.colors[n], t.vertexs[n]);
                }
            }
            return vis;
        }
    }
    public struct VertexTriangleList
    {
        public int Length => tris.Length;
        public Vector2 offset;
        public VertexTriangle this[int i] => tris[i];

        public VertexTriangle[] tris;
        public Vector2 GetRealPosition(Vector2 vector) => vector + offset;
        public VertexTriangleList(Vector2 _offset, params VertexTriangle[] _tris)
        {
            offset = _offset;
            tris = _tris;
        }
        public CustomVertexInfo[] ToVertexInfo()
        {
            var vis = new CustomVertexInfo[tris.Length * 3];
            for (int i = 0; i < tris.Length; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    var t = tris[i];
                    vis[i * 3 + n] = new CustomVertexInfo(GetRealPosition(t.positions[n]), t.colors[n], t.vertexs[n]);
                }
            }
            return vis;
        }
    }
    public struct VertexTriangle : IVertexTriangle
    {
        public VertexTriangle(Vector3 vA, Vector3 vB, Vector3 vC, Color cA, Color cB, Color cC, Vector2 pA = default, Vector2 pB = default, Vector2 pC = default)
        {
            positions = new Vector2[3];
            vertexs = new Vector3[3];
            colors = new Color[3];
            vertexs[0] = vA;
            vertexs[1] = vB;
            vertexs[2] = vC;
            colors[0] = cA;
            colors[1] = cB;
            colors[2] = cC;
            positions[0] = pA;
            positions[1] = pB;
            positions[2] = pC;
        }
        public readonly Vector2[] positions;
        public readonly Vector3[] vertexs;
        public readonly Color[] colors;
        public static Vector2 offset = default;
        public static Vector2 GetRealPosition(Vector2 vector) => vector + offset;
        public CustomVertexInfo this[int index] => new CustomVertexInfo(GetRealPosition(positions[index]), colors[index], vertexs[index]);
        public CustomVertexInfo A => new CustomVertexInfo(GetRealPosition(positions[0]), colors[0], vertexs[0]);
        public CustomVertexInfo B => new CustomVertexInfo(GetRealPosition(positions[1]), colors[1], vertexs[1]);
        public CustomVertexInfo C => new CustomVertexInfo(GetRealPosition(positions[2]), colors[2], vertexs[2]);
        public static CustomVertexInfo[] ToVertexInfo(VertexTriangle[] tris)
        {
            var vis = new CustomVertexInfo[tris.Length * 3];
            for (int i = 0; i < tris.Length; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    var t = tris[i];
                    vis[i * 3 + n] = t[n];
                }
            }
            return vis;
        }

    }
}

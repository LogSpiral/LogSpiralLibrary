using System;
using System.Collections.Generic;
using System.Linq;
using static LogSpiralLibrary.LogSpiralLibraryMod;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing
{
    /// <summary>
    /// 非常抽象的顶点绘制用结构体
    /// </summary>  
    public abstract class VertexDrawInfo : ModType
    {
        public sealed override void Register()
        {
            ModTypeLookup<VertexDrawInfo>.Register(this);
            LogSpiralLibrarySystem.vertexDrawInfoInstance.Add(GetType(), this);
        }
        /// <summary>
        /// 仅给<see  cref="LogSpiralLibrarySystem.vertexDrawInfoInstance"/>中的实例使用
        /// <br>代码的耦合度持续放飞自我</br>
        /// </summary>
        public IRenderDrawInfo[][] RenderDrawInfos = [[new AirDistortEffectInfo()], [new MaskEffectInfo(), new BloomEffectInfo()]];

        /// <summary>
        /// 代表元
        /// </summary>
        public VertexDrawInfo Representative => LogSpiralLibrarySystem.vertexDrawInfoInstance[GetType()];
        //public void ModityRenderInfo(IRenderDrawInfo newInfo, int index)
        //{
        //    var array = LogSpiralLibrarySystem.vertexDrawInfoInstance[GetType()].RenderDrawInfos;
        //    array[index] = newInfo;
        //    OnModifyRenderInfo(array);
        //}
        public void ModityAllRenderInfo(params IRenderDrawInfo[][] newInfos)
        {
            if (newInfos == null)
            {
                ResetAllRenderInfo();
                return;
            }
            var array = Representative.RenderDrawInfos;
            for (int n = 0; n < newInfos.Length; n++)
                array[n] = newInfos[n];
            OnModifyRenderInfo(newInfos);
            //Representative.RenderDrawInfos = newInfos;
            //OnModifyRenderInfo(newInfos);
        }
        public void ResetAllRenderInfo()
        {
            var array = LogSpiralLibrarySystem.vertexDrawInfoInstance[GetType()].RenderDrawInfos;
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array[i].Length; j++)
                    array[i][j].Reset();
            }
            OnModifyRenderInfo(array);
        }
        public virtual void OnModifyRenderInfo(IRenderDrawInfo[][] infos)
        {

        }
        public static void DrawVertexInfo(IEnumerable<VertexDrawInfo> infos, Type type, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap)
        {
            if (!LogSpiralLibrarySystem.vertexDrawInfoInstance.TryGetValue(type, out var instance)) return;
            var newInfos = from info in infos where info != null && info.Active select info;
            if (!newInfos.Any()) return;
            List<List<IRenderDrawInfo>> renderPipeLines = [];
            foreach (var pipe in instance.RenderDrawInfos)
            {
                var list = new List<IRenderDrawInfo>();
                foreach (var rInfo in pipe)
                {
                    if (rInfo.Active)
                        list.Add(rInfo);
                }
                if (list.Count != 0)
                    renderPipeLines.Add(list);
            }
            if (renderPipeLines.Count == 0 || !CanUseRender || graphicsDevice == null)
            {
                instance.PreDraw(spriteBatch, graphicsDevice, render, renderSwap);
                foreach (var info in newInfos) info.Draw(spriteBatch);
                instance.PostDraw(spriteBatch, graphicsDevice, render, renderSwap);
            }
            else
            {
                bool realDraw = false;

                foreach (var pipeLine in renderPipeLines)
                {
                    var realLine = pipeLine;
                    if (!realLine.Any()) continue;
                    int counter = 0;
                    foreach (var renderEffect in realLine)
                    {
                        if (counter == 0)
                        {
                            instance.PreDraw(spriteBatch, graphicsDevice, render, renderSwap);
                            renderEffect.PreDraw(spriteBatch, graphicsDevice, render, renderSwap);
                            foreach (var info in newInfos) info.Draw(spriteBatch);
                            instance.PostDraw(spriteBatch, graphicsDevice, render, renderSwap);
                        }
                        renderEffect.PostDraw(spriteBatch, graphicsDevice, render, renderSwap);
                        counter++;
                        if (counter == realLine.Count())
                        {
                            renderEffect.DrawToScreen(spriteBatch, graphicsDevice, render, renderSwap);
                        }
                        realDraw |= renderEffect.DoRealDraw;
                    }

                }
                if (!realDraw)
                {
                    instance.PreDraw(spriteBatch, graphicsDevice, render, renderSwap);
                    foreach (var info in newInfos) info.Draw(spriteBatch);
                    instance.PostDraw(spriteBatch, graphicsDevice, render, renderSwap);
                }
            }
        }
        /// <summary>
        /// 存在时长
        /// </summary>
        public int timeLeft;
        /// <summary>
        /// 中心偏移
        /// </summary>
        public Vector2 center;
        /// <summary>
        /// 热度/采样图
        /// </summary>
        public Texture2D heatMap;

        public Texture2D weaponTex;
        public Rectangle? frame;
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
        public int timeLeftMax;

        public int aniTexIndex;

        public int baseTexIndex;

        /// <summary>
        /// 用于在特效与物体未解除绑定时，与物体的动画同步
        /// </summary>
        public bool autoUpdate = true;
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
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, null, TransformationMatrix);
        }
        public void DrawPrimitives(float distortScaler) => Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, DrawingMethods.CreateTriList(VertexInfos, center, distortScaler, true, !distortScaler.Equals(1.0f)), 0, VertexInfos.Length - 2);
        /// <summary>
        /// 单个绘制
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            //for (int n = 0; n < VertexInfos.Length - 1; n++)
            //{
            //    spriteBatch.DrawLine(VertexInfos[n].Position, VertexInfos[n + 1].Position, Color.White, 1, false, UIDrawing ? default : -Main.screenPosition);
            //}
            //var m = Main.spriteBatch.transformMatrix;
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(800), new Rectangle(0, 0, 1, 1), Color.White, 0, new Vector2(.5f), 4f, 0, 0);
            //return;

            //Main.graphics.GraphicsDevice.Textures[0] = BaseTex[baseTexIndex].Value;
            //Main.graphics.GraphicsDevice.Textures[1] = AniTex[aniTexIndex + 11].Value;
            Main.graphics.GraphicsDevice.Textures[2] = weaponTex ?? TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value;
            Main.graphics.GraphicsDevice.Textures[3] = heatMap;
            var swooshUL = ShaderSwooshUL;
            swooshUL.Parameters["lightShift"].SetValue(factor - 1f);
            if (frame != null)
            {
                Rectangle uframe = frame.Value;
                Vector2 size = (weaponTex ?? TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value).Size();
                swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(uframe.TopLeft() / size, uframe.Width / size.X, uframe.Height / size.Y));
            }
            else
                swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(0, 0, 1, 1));
            float dS = swooshUL.Parameters["distortScaler"].GetValueSingle();
            //ShaderSwooshUL.Parameters["distortScaler"].SetValue(1f);
            swooshUL.CurrentTechnique.Passes[dS.Equals(1.0f) ? 7 : 0].Apply();//
            DrawPrimitives(dS);


            //CustomVertexInfo[] customVertexInfos = new CustomVertexInfo[6];
            //customVertexInfos[0] = new CustomVertexInfo(new Vector2(1800, 600), new Vector3(0, 0, 1));
            //customVertexInfos[1] = new CustomVertexInfo(new Vector2(2200, 600), new Vector3(1, 0, 1));
            //customVertexInfos[2] = new CustomVertexInfo(new Vector2(1800, 1000), new Vector3(0, 1, 1));
            //customVertexInfos[5] = new CustomVertexInfo(new Vector2(2200, 1000), new Vector3(1, 1, 1));
            //customVertexInfos[3] = customVertexInfos[2];
            //customVertexInfos[4] = customVertexInfos[1];
            //Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, customVertexInfos, 0, 2);
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
        public static Matrix TransformationMatrix
        {
            get
            {
                if (Main.gameMenu)
                {
                    return Matrix.CreateScale(Main.instance.Window.ClientBounds.Width / (float)Main.screenWidth, Main.instance.Window.ClientBounds.Height / (float)Main.screenHeight, 1);
                }
                else if (UIDrawing)
                {
                    return Matrix.Identity;
                }
                return Main.GameViewMatrix?.TransformationMatrix ?? Matrix.Identity;
            }
        }
        static Matrix projection => Main.gameMenu ? Matrix.CreateOrthographicOffCenter(0, Main.instance.Window.ClientBounds.Width, Main.instance.Window.ClientBounds.Height, 0, 0, 1) : Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);//Main.screenWidth  Main.screenHeight
        static Matrix model => Matrix.CreateTranslation(UIDrawing ? default : new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        /// <summary>
        /// 丢给顶点坐标变换的矩阵
        /// <br>先右乘<see cref="model"/>将世界坐标转屏幕坐标</br>
        /// <br>再右乘<see cref="TransformationMatrix"/>进行画面缩放等</br>
        /// <br>最后右乘<see cref="projection"/>将坐标压缩至[0,1]</br>
        /// </summary>
        public static Matrix uTransform => model * TransformationMatrix * projection;

        public static bool UIDrawing;
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
        public float alphaFactor = 2f;
        public float heatRotation;
        //public override IRenderDrawInfo[] RenderDrawInfos => _rendeDrawInfos;
        //IRenderDrawInfo[] _rendeDrawInfos = [new AirDistortEffectInfo(), new MaskEffectInfo(), new BloomEffectInfo()];

        public override void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            base.PreDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
            Effect effect = ShaderSwooshUL;
            effect.Parameters["uTransform"].SetValue(uTransform);
            effect.Parameters["uTime"].SetValue(-(float)LogSpiralLibrarySystem.ModTime * 0.03f);
            effect.Parameters["checkAir"].SetValue(false);
            effect.Parameters["airFactor"].SetValue(2);
            effect.Parameters["heatRotation"].SetValue(Matrix.CreateRotationZ(heatRotation));
            effect.Parameters["lightShift"].SetValue(0f);
            effect.Parameters["distortScaler"].SetValue(1f);
            effect.Parameters["alphaFactor"].SetValue(alphaFactor);
            effect.Parameters["heatMapAlpha"].SetValue(true);
            effect.Parameters["stab"].SetValue(false);
            effect.Parameters["alphaOffset"].SetValue(0f);
            //if (flag)
            //    effect.Parameters["AlphaVector"].SetValue(ConfigurationUltraTest.ConfigSwooshUltraInstance.AlphaVector);
            var sampler = SamplerState.AnisotropicWrap;
            Main.graphics.GraphicsDevice.SamplerStates[0] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[3] = SamplerState.AnisotropicWrap;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Effect effect = ShaderSwooshUL;
            effect.Parameters["gather"].SetValue(gather);
            if (heatMap == null)
                ColorVector = new Vector3(0, 1, 0);
            effect.Parameters["AlphaVector"].SetValue(ColorVector);
            effect.Parameters["normalize"].SetValue(normalize);
            effect.Parameters["heatRotation"].SetValue(Matrix.CreateRotationZ(heatRotation));
            effect.Parameters["alphaFactor"].SetValue(alphaFactor);
            base.Draw(spriteBatch);
        }
        public override void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {

            base.PostDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
        }
        //public override void OnModifyRenderInfo(IRenderDrawInfo[] infos)
        //{
        //    //dynamic mask = infos[1];
        //    //dynamic bloom = infos[2];
        //    //if (mask.Active) bloom.ReDraw = false;
        //    base.OnModifyRenderInfo(infos);
        //}
    }
    public class FractalStabInfo : MeleeVertexInfo
    {
        CustomVertexInfo[] _vertexInfos = new CustomVertexInfo[6];
        public override CustomVertexInfo[] VertexInfos => _vertexInfos;
        public override void Uptate()
        {
            _vertexInfos = DrawingMethods.GetItemVertexes(new Vector2(.5f), 0, rotation, LogSpiralLibraryMod.BaseTex[baseTexIndex].Value, 0.5f, scaler / 1000f, center, negativeDir);
            //var vertexs = new CustomVertexInfo[4];
            //for (int n = 0; n < 4; n++) 
            //{
            //    vertexs[n] = new CustomVertexInfo(Main.screenPosition + new Vector2(200,200) * new Vector2(n%2,n/2) + new Vector2(900,600),new Vector3(n%2,n/2,1));
            //}
            //_vertexInfos[0] = vertexs[0];
            //_vertexInfos[1] = vertexs[1];
            //_vertexInfos[2] = vertexs[2];
            //_vertexInfos[3] = vertexs[1];
            //_vertexInfos[4] = vertexs[2];
            //_vertexInfos[5] = vertexs[3];

            timeLeft--;
        }
        #region 生成函数
        public static T NewFractalStab<T>(
    Color color, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
    Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) where T : FractalStabInfo, new()
            => NewFractalStab<T>(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);
        public static T NewFractalStab<T>(
    Func<float, Color> colorFunc, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
    Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) where T : FractalStabInfo, new()
        {
            T result = null;
            for (int n = 0; n < vertexEffects.Length; n++)
            {
                var effect = vertexEffects[n];
                if (effect == null || !effect.Active)
                {
                    effect = vertexEffects[n] = new T();
                    if (effect is T stab)
                    {
                        stab.color = colorFunc;
                        stab.timeLeftMax = stab.timeLeft = timeLeft;
                        stab.scaler = _scaler;
                        stab.center = center ?? Main.LocalPlayer.Center;
                        stab.heatMap = heat;
                        stab.negativeDir = _negativeDir;
                        stab.rotation = _rotation;
                        stab.xScaler = xscaler;
                        result = stab;
                        stab.aniTexIndex = _aniIndex;
                        stab.baseTexIndex = _baseIndex;
                        stab.ColorVector = colorVec == default ? new Vector3(0.33f) : colorVec;
                        stab.normalize = normalize;
                    }
                    break;
                }
            }
            return result;
        }


        /// <summary>
        /// 生成新的穿刺于指定数组中
        /// </summary>
        public static FractalStabInfo NewFractalStab(
            Color color, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) => NewFractalStab(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的穿刺于指定数组中
        /// </summary>
        public static FractalStabInfo NewFractalStab(
            Func<float, Color> colorFunc, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false)
            => NewFractalStab<FractalStabInfo>(colorFunc, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的穿刺于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static FractalStabInfo NewFractalStab(
            Color color, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) =>
            NewFractalStab(color, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的穿刺于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static FractalStabInfo NewFractalStab(
            Func<float, Color> colorFunc, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) =>
            NewFractalStab(colorFunc, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        #endregion
    }
    public class UltraStab : MeleeVertexInfo
    {
        const bool usePSShaderTransform = true;
        #region 参数和属性
        //TODO 改成用ps实现
        CustomVertexInfo[] _vertexInfos = new CustomVertexInfo[usePSShaderTransform ? 4 : 90];//
        public override CustomVertexInfo[] VertexInfos => _vertexInfos;
        #endregion
        #region 生成函数
        public static T NewUltraStab<T>(
    Color color, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
    Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false) where T : UltraStab, new()
            => NewUltraStab<T>(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);
        public static T NewUltraStab<T>(
    Func<float, Color> colorFunc, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
    Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false) where T : UltraStab, new()
        {
            T result = null;
            for (int n = 0; n < vertexEffects.Length; n++)
            {
                var effect = vertexEffects[n];
                if (effect == null || !effect.Active)
                {
                    effect = vertexEffects[n] = new T();
                    if (effect is T stab)
                    {
                        stab.color = colorFunc;
                        stab.timeLeftMax = stab.timeLeft = timeLeft;
                        stab.scaler = _scaler;
                        stab.center = center ?? Main.LocalPlayer.Center;
                        stab.heatMap = heat;
                        stab.negativeDir = _negativeDir;
                        stab.rotation = _rotation;
                        stab.xScaler = xscaler;
                        result = stab;
                        stab.aniTexIndex = _aniIndex;
                        stab.baseTexIndex = _baseIndex;
                        stab.ColorVector = colorVec == default ? new Vector3(0.33f) : colorVec;
                        stab.normalize = normalize;
                    }
                    break;
                }
            }
            return result;
        }


        /// <summary>
        /// 生成新的穿刺于指定数组中
        /// </summary>
        public static UltraStab NewUltraStab(
            Color color, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false) => NewUltraStab(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的穿刺于指定数组中
        /// </summary>
        public static UltraStab NewUltraStab(
            Func<float, Color> colorFunc, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false)
            => NewUltraStab<UltraStab>(colorFunc, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的穿刺于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static UltraStab NewUltraStab(
            Color color, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false) =>
            NewUltraStab(color, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的穿刺于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static UltraStab NewUltraStab(
            Func<float, Color> colorFunc, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false) =>
            NewUltraStab(colorFunc, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

        #endregion
        #region 绘制和更新，主体
        public override void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        {
            base.PreDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
            LogSpiralLibraryMod.ShaderSwooshUL.Parameters["stab"].SetValue(usePSShaderTransform);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Main.graphics.GraphicsDevice.Textures[0] = BaseTex_Stab[baseTexIndex].Value;
            Main.graphics.GraphicsDevice.Textures[1] = AniTex_Stab[aniTexIndex].Value;
            base.Draw(spriteBatch);
        }
        public override void Uptate()
        {
            if (usePSShaderTransform)
            {
                if (OnSpawn)
                {
                    var realColor = Color.White;
                    //Vector2 offsetVec = 20f * new Vector2(8, 3 / xScaler) * scaler;
                    Vector2 offsetVec = new Vector2(1, 3 / xScaler / 8) * scaler;

                    if (negativeDir) offsetVec.Y *= -1;
                    VertexInfos[0] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 1, 1f));
                    VertexInfos[2] = new CustomVertexInfo(center + (offsetVec with { X = 0 }).RotatedBy(rotation), realColor, new Vector3(1, 1, 1f));
                    offsetVec.Y *= -1;
                    VertexInfos[1] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 0, 1f));
                    VertexInfos[3] = new CustomVertexInfo(center + (offsetVec with { X = 0 }).RotatedBy(rotation), realColor, new Vector3(1, 0, 1f));
                }
                for (int n = 0; n < 4; n++)
                    VertexInfos[n].Position += rotation.ToRotationVector2();
            }
            else
            {
                center += rotation.ToRotationVector2();
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
                    VertexInfos[2 * i] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(1 - f, 1, 1));
                    offsetVec.Y *= -1;
                    VertexInfos[2 * i + 1] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 0, 1));
                }
            }

            timeLeft--;
        }
        #endregion
    }
    public class UltraSwoosh : MeleeVertexInfo
    {
        #region 参数和属性
        int counts = 45;
        public int Counts 
        {
            get { return counts = Math.Clamp(counts, 2, 45); }
            set { counts = Math.Clamp(value, 2, 45); Array.Resize(ref _vertexInfos, 2 * counts); if (autoUpdate) { Uptate(); timeLeft++; } }
        }
        CustomVertexInfo[] _vertexInfos = new CustomVertexInfo[90];
        public override CustomVertexInfo[] VertexInfos => _vertexInfos;
        public (float from, float to) angleRange;
        #endregion
        #region 生成函数
        /// <summary>
        /// 生成新的剑气于指定数组中
        /// </summary>
        public static T NewUltraSwoosh<T>(
            Color color, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) where T : UltraSwoosh, new() => NewUltraSwoosh<T>(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);
        /// <summary>
        /// 生成新的剑气于指定数组中，给子类用
        /// </summary>
        public static T NewUltraSwoosh<T>(
            Func<float, Color> colorFunc, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) where T : UltraSwoosh, new()
        {
            T result = null;
            for (int n = 0; n < vertexEffects.Length; n++)
            {
                var effect = vertexEffects[n];
                if (effect == null || !effect.Active)
                {
                    effect = vertexEffects[n] = new T();

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
                        swoosh.ColorVector = (colorVec == default && normalize) ? new Vector3(0.33f) : colorVec;
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
            Func<float, Color> colorFunc, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false)
            => NewUltraSwoosh<UltraSwoosh>(colorFunc, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);

        public static UltraSwoosh NewUltraSwoosh(
    Color color, VertexDrawInfo[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
    Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
    (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) => NewUltraSwoosh(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的剑气于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static UltraSwoosh NewUltraSwoosh(
            Color color, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) => NewUltraSwoosh(color, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);

        /// <summary>
        /// 生成新的剑气于<see cref="LogSpiralLibrarySystem.vertexEffects"/>
        /// </summary>
        public static UltraSwoosh NewUltraSwoosh(
            Func<float, Color> colorFunc, int timeLeft = 30, float _scaler = 1f,
            Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
            (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) => NewUltraSwoosh(colorFunc, LogSpiralLibrarySystem.vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);
        #endregion
        #region 绘制和更新，主体
        public override void Draw(SpriteBatch spriteBatch)
        {
            var uls = this;
            Main.graphics.GraphicsDevice.Textures[0] = BaseTex_Swoosh[baseTexIndex].Value;
            Main.graphics.GraphicsDevice.Textures[1] = AniTex_Swoosh[aniTexIndex].Value;
            base.Draw(spriteBatch);
        }
        public override void Uptate()
        {
            timeLeft--;
            for (int i = 0; i < Counts; i++)
            {
                var f = i / (Counts - 1f);
                var num = 1 - factor;
                //float theta2 = (1.8375f * MathHelper.Lerp(num, 1f, f) - 1.125f) * MathHelper.Pi;
                var lerp = f.Lerp(num * .5f, 1);//num
                float theta2 = MathHelper.Lerp(angleRange.from, angleRange.to, lerp) * MathHelper.Pi;
                if (negativeDir) theta2 = MathHelper.TwoPi - theta2;
                Vector2 offsetVec = (theta2.ToRotationVector2() * new Vector2(1, 1 / xScaler)).RotatedBy(rotation) * scaler;//  * MathHelper.Lerp(2 - 1 / (MathF.Abs(angleRange.from - angleRange.to) + 1), 1, f)
                Vector2 adder = (offsetVec * 0.05f + rotation.ToRotationVector2() * 2f) * num;
                //adder = default;
                var realColor = color.Invoke(f);
                realColor.A = (byte)((1 - f).HillFactor2(1) * 255);//
                VertexInfos[2 * i] = new CustomVertexInfo(center + offsetVec + adder, realColor, new Vector3(1 - f, 1, 1));
                VertexInfos[2 * i + 1] = new CustomVertexInfo(center + adder, realColor, new Vector3(0, 0, 1));
            }
        }
        #endregion
    }
}

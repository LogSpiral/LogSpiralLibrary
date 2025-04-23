using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using static LogSpiralLibrary.LogSpiralLibraryMod;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;

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
        Color color, RenderDrawingContent[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
        Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
        (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) where T : UltraSwoosh, new() => NewUltraSwoosh<T>(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);
    /// <summary>
    /// 生成新的剑气于指定数组中，给子类用
    /// </summary>
    public static T NewUltraSwoosh<T>(
        Func<float, Color> colorFunc, RenderDrawingContent[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
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
                    swoosh.ColorVector = colorVec == default && normalize ? new Vector3(0.33f) : colorVec;
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
        Func<float, Color> colorFunc, RenderDrawingContent[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
        Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
        (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false)
        => NewUltraSwoosh<UltraSwoosh>(colorFunc, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);

    public static UltraSwoosh NewUltraSwoosh(
Color color, RenderDrawingContent[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
(float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) => NewUltraSwoosh(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);

    /// <summary>
    /// 生成新的剑气于<see cref="RenderDrawingContentsSystem.RenderDrawingContents,"/>
    /// </summary>
    public static UltraSwoosh NewUltraSwoosh(
        Color color, int timeLeft = 30, float _scaler = 1f,
        Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
        (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) => NewUltraSwoosh(color, RenderDrawingContentsSystem.RenderDrawingContents, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);

    /// <summary>
    /// 生成新的剑气于<see cref="RenderDrawingContentsSystem.RenderDrawingContents,"/>
    /// </summary>
    public static UltraSwoosh NewUltraSwoosh(
        Func<float, Color> colorFunc, int timeLeft = 30, float _scaler = 1f,
        Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1,
        (float, float)? angleRange = null, int _aniIndex = 3, int _baseIndex = 7, Vector3 colorVec = default, bool normalize = false) => NewUltraSwoosh(colorFunc, RenderDrawingContentsSystem.RenderDrawingContents, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, angleRange, _aniIndex, _baseIndex, colorVec, normalize);
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
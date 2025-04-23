using static LogSpiralLibrary.LogSpiralLibraryMod;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
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
Color color, RenderDrawingContent[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false) where T : UltraStab, new()
        => NewUltraStab<T>(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);
    public static T NewUltraStab<T>(
Func<float, Color> colorFunc, RenderDrawingContent[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
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
        Color color, RenderDrawingContent[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
        Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false) => NewUltraStab(t => color, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

    /// <summary>
    /// 生成新的穿刺于指定数组中
    /// </summary>
    public static UltraStab NewUltraStab(
        Func<float, Color> colorFunc, RenderDrawingContent[] vertexEffects, int timeLeft = 30, float _scaler = 1f,
        Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false)
        => NewUltraStab<UltraStab>(colorFunc, vertexEffects, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

    /// <summary>
    /// 生成新的穿刺于<see cref="RenderDrawingContentsSystem.RenderDrawingContents,"/>
    /// </summary>
    public static UltraStab NewUltraStab(
        Color color, int timeLeft = 30, float _scaler = 1f,
        Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false) =>
        NewUltraStab(color, RenderDrawingContentsSystem.RenderDrawingContents, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

    /// <summary>
    /// 生成新的穿刺于<see cref="RenderDrawingContentsSystem.RenderDrawingContents,"/>
    /// </summary>
    public static UltraStab NewUltraStab(
        Func<float, Color> colorFunc, int timeLeft = 30, float _scaler = 1f,
        Vector2? center = default, Texture2D heat = null, bool _negativeDir = false, float _rotation = 0, float xscaler = 1, int _aniIndex = 9, int _baseIndex = 0, Vector3 colorVec = default, bool normalize = false) =>
        NewUltraStab(colorFunc, RenderDrawingContentsSystem.RenderDrawingContents, timeLeft, _scaler, center, heat, _negativeDir, _rotation, xscaler, _aniIndex, _baseIndex, colorVec, normalize);

    #endregion
    #region 绘制和更新，主体
    public override void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
    {
        base.PreDraw(spriteBatch, graphicsDevice, render, renderAirDistort);
        ShaderSwooshUL.Parameters["stab"].SetValue(usePSShaderTransform);
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

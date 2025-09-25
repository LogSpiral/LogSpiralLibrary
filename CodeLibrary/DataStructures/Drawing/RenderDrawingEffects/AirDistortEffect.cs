using LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
using Newtonsoft.Json;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;

public class AirDistortEffect(float intensity, float scaler, float rotation = 0f, float colorOffset = 0f) : IRenderEffect
{
    #region 参数属性

    /// <summary>
    /// 步长系数
    /// </summary>
    public float Intensity { get; set; } = intensity;

    /// <summary>
    /// 偏移方向偏移量
    /// </summary>
    public float Rotation { get; set; } = rotation;

    /// <summary>
    /// 缩放系数
    /// </summary>
    public float Scaler { get; set; } = scaler;

    /// <summary>
    /// 色差距离
    /// </summary>
    public float ColorOffset { get; set; } = colorOffset;

    #endregion 参数属性

    #region 公有静态属性

    public static float DistortScaler { get; private set; }

    #endregion 公有静态属性

    #region 接口实现

    public bool Active => Intensity > 0;

    public bool DoRealDraw => false;

    public bool RedrawContents(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        DistortScaler = Scaler;
        LogSpiralLibraryMod.ShaderSwooshUL.Parameters["distortScaler"].SetValue(Scaler);

        return true;
    }

    public void ProcessRender(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ref RenderTarget2D contentRender, ref RenderTarget2D assistRender)
    {
        // 这里对内容画布没有调整，而是修改Main.screenTarget以达成扭曲效果

        #region 准备状态

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        #endregion 准备状态

        #region 切换绘制目标至备用画布

        graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
        graphicsDevice.Clear(Color.Transparent);

        #endregion 切换绘制目标至备用画布

        #region 设置参数

        var effect = LogSpiralLibraryMod.AirDistortEffect;
        effect.Parameters["uScreenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        effect.Parameters["strength"].SetValue(Intensity);
        effect.Parameters["rotation"].SetValue(Matrix.CreateRotationZ(Rotation));
        effect.Parameters["tex0"].SetValue(contentRender);
        effect.Parameters["colorOffset"].SetValue(ColorOffset);
        Main.instance.GraphicsDevice.Textures[2] = LogSpiralLibraryMod.Misc[18].Value;
        effect.CurrentTechnique.Passes[0].Apply();//ApplyPass

        #endregion 设置参数

        #region 绘制内容

        spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);

        #endregion 绘制内容

        #region 恢复状态

        spriteBatch.End();
        LogSpiralLibraryMod.ShaderSwooshUL.Parameters["distortScaler"].SetValue(1f);
        DistortScaler = 1f;

        #endregion 恢复状态
    }

    #endregion 接口实现

    public AirDistortEffect() : this(0, 0)
    {
    }
}

public class AirDistortConfigs : IAvailabilityChangableConfig
{
    public bool Available { get; set; } = true;

    [Range(0, 10f)]
    [DefaultValue(6f)]
    public float Intensity
    {
        get;
        set;
    } = 6f;

    [Range(1f, 3f)]
    [DefaultValue(1.5f)]
    public float Scaler
    {
        get;
        set;
    } = 1.5f;

    [Range(0, MathHelper.TwoPi)]
    [DefaultValue(0f)]
    public float Rotation
    {
        get;
        set;
    } = 0f;

    [Range(0, 1)]
    [DefaultValue(0.5f)]
    public float ColorOffset
    {
        get;
        set;
    } = .5f;

    [JsonIgnore]
    public AirDistortEffect EffectInstance => !Available ? new AirDistortEffect() : new AirDistortEffect(Intensity, Scaler, Rotation, ColorOffset);

    // field ??=
    public void CopyToInstance(AirDistortEffect effect)
    {
        effect.Intensity = Available ? Intensity : 0;
        effect.Scaler = Scaler;
        effect.Rotation = Rotation;
        effect.ColorOffset = ColorOffset;
    }
}
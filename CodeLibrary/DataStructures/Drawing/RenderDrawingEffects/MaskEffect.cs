using LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
using Newtonsoft.Json;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;

public class MaskEffect(Texture2D fillTex, Color glowColor, float tier1, float tier2, Vector2 offset, bool lightAsAlpha, bool inverse) : IRenderEffect
{
    #region 参数属性

    /// <summary>
    /// 到达阈值之后替换的贴图
    /// </summary>
    public Texture2D FillTex { get; set; } = fillTex;

    /// <summary>
    /// 低于阈值的颜色
    /// </summary>
    public Color GlowColor { get; set; } = glowColor;

    public float Tier1 { get; set; } = tier1;

    public float Tier2 { get; set; } = tier2;

    public Vector2 Offset { get; set; } = offset;

    /// <summary>
    /// 是否让亮度参与透明度的决定
    /// </summary>
    public bool LightAsAlpha { get; set; } = lightAsAlpha;

    /// <summary>
    /// 翻转亮度决定值
    /// </summary>
    public bool Inverse { get; set; } = inverse;

    #endregion 参数属性

    #region 接口实现

    public bool Active => FillTex != null;

    public bool DoRealDraw => true;

    public void ProcessRender(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ref RenderTarget2D contentRender, ref RenderTarget2D assistRender)
    {
        #region 准备状态

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        #endregion 准备状态

        #region 切换绘制目标至备用画布

        graphicsDevice.SetRenderTarget(assistRender);
        graphicsDevice.Clear(Color.Transparent);

        #endregion 切换绘制目标至备用画布

        #region 设置参数

        Effect effect = LogSpiralLibraryMod.RenderEffect;
        effect.Parameters["screenScale"].SetValue(Main.ScreenSize.ToVector2());
        effect.Parameters["lightAsAlpha"].SetValue(LightAsAlpha);
        effect.Parameters["tier1"].SetValue(Tier1);
        effect.Parameters["tier2"].SetValue(Tier2);
        effect.Parameters["offset"].SetValue((float)GlobalTimeSystem.GlobalTime * new Vector2(0.707f) + (Main.gameMenu ? default : Main.LocalPlayer.Center));//offset
        effect.Parameters["maskGlowColor"].SetValue(GlowColor.ToVector4());
        effect.Parameters["ImageSize"].SetValue(FillTex.Size());
        effect.Parameters["inverse"].SetValue(Inverse);
        Main.graphics.GraphicsDevice.Textures[1] = FillTex;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
        effect.CurrentTechnique.Passes[1].Apply();

        #endregion 设置参数

        #region 绘制内容

        spriteBatch.Draw(contentRender, Vector2.Zero, Color.White);

        // 因为最新的处理结果在assistRender上，这里就直接交换引用了
        // 毕竟那两张画布没什么本质的不同.png
        Utils.Swap(ref contentRender, ref assistRender);

        #endregion 绘制内容

        #region 恢复状态

        spriteBatch.End();

        #endregion 恢复状态
    }

    #endregion 接口实现

    public MaskEffect() : this(null, default, 0, 0, default, true, false)
    {
    }
}

public class MaskConfigs : IAvailabilityChangableConfig
{
    public bool Available { get; set; } = false;

    [Range(0, 6)]
    [Slider]
    [DrawTicks]
    [DefaultValue(1)]
    public int SkyStyle
    {
        get;
        set;
    } = 1;

    [DefaultValue(typeof(Color), "152, 74, 255, 255")]
    public Color GlowColor
    {
        get;
        set;
    } = new Color(152, 74, 255, 255);

    [Range(0, 1f)]
    [DefaultValue(0.2f)]
    public float Tier1
    {
        get;
        set;
    } = .2f;

    [Range(0, 1f)]
    [DefaultValue(0.25f)]
    public float Tier2
    {
        get;
        set;
    } = .25f;

    [JsonIgnore]
    public MaskEffect EffectInstance => !Available ? new() : new MaskEffect(LogSpiralLibraryMod.Mask[SkyStyle].Value, GlowColor, Tier1, Tier2, default, true, false);

    // field ??=

    public void CopyToInstance(MaskEffect effect)
    {
        effect.FillTex = Available ? LogSpiralLibraryMod.Mask[SkyStyle].Value : null;
        effect.GlowColor = GlowColor;
        effect.Tier1 = Tier1;
        effect.Tier2 = Tier2;
    }
}
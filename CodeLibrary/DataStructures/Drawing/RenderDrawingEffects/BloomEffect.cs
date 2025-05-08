using LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;
public class BloomEffect(float threshold, float intensity, float range, int count, bool additive, byte downSampleLevel, bool useModeMK) : IRenderEffect
{

    #region 参数属性

    /// <summary>
    /// 阈值
    /// </summary>
    public float Threshold { get; set; } = threshold;

    /// <summary>
    /// 亮度
    /// </summary>
    public float Intensity { get; set; } = intensity;

    /// <summary>
    /// 范围
    /// </summary>
    public float Range { get; set; } = range;

    /// <summary>
    /// 迭代次数
    /// </summary>
    public int Count { get; set; } = count;

    /// <summary>
    /// 是否启动加法模式
    /// </summary>
    public bool Additive { get; set; } = additive;

    public byte DownSampleLevel { get; set; } = downSampleLevel;

    /// <summary>
    /// 是否使用MasakiKawase算法
    /// </summary>
    public bool UseModeMK { get; set; } = useModeMK;

    #endregion

    #region 辅助属性
    /// <summary>
    /// 是否启用降采样
    /// </summary>
    bool UseDownSample => DownSampleLevel != 0;

    /// <summary>
    /// 降采样比例，4代表长宽都只有原来的1/4
    /// </summary>
    int DownSampleCount => 1 << DownSampleLevel;
    #endregion

    #region 接口实现

    public bool Active => Range > 0 && Intensity > 0 && Count > 0;

    public bool DoRealDraw => true;

    public void ProcessRender(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ref RenderTarget2D contentRender, ref RenderTarget2D assistRender)
    {

        #region 准备状态

        RenderTarget2D renderTiny = DownSampleLevel == 1 ? LogSpiralLibraryMod.Instance.Render_Tiny : LogSpiralLibraryMod.Instance.Render_Tiniest;
        RenderTarget2D renderTinySwap = DownSampleLevel == 1 ? LogSpiralLibraryMod.Instance.Render_Tiny_Swap : LogSpiralLibraryMod.Instance.Render_Tiniest_Swap;
        if (UseDownSample)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            graphicsDevice.SetRenderTarget(assistRender);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(contentRender, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, UseDownSample ? Matrix.CreateScale((float)contentRender.Width / renderTiny.Width, (float)contentRender.Height / renderTiny.Height, 1) : Matrix.identity);

        #endregion

        #region 设置参数

        var effect = LogSpiralLibraryMod.RenderEffect;
        effect.Parameters["threshold"].SetValue(Threshold);
        effect.Parameters["range"].SetValue(Range);
        effect.Parameters["intensity"].SetValue(Intensity * 1.75f);
        effect.Parameters["uBloomAdditive"].SetValue(true);

        #endregion

        #region 绘制内容_降采样

        if (UseDownSample)
        {


            #region 变量准备

            effect.Parameters["screenScale"].SetValue(Main.ScreenSize.ToVector2() / DownSampleCount);
            int pass1 = UseModeMK ? 4 : 3;
            int pass2 = UseModeMK ? 4 : 2;

            #endregion

            #region 反复模糊处理

            for (int n = 0; n < Count - 1; n++)
            {
                graphicsDevice.SetRenderTarget(renderTinySwap);
                graphicsDevice.Clear(Color.Transparent);
                effect.CurrentTechnique.Passes[pass1].Apply();
                if (n != 0)
                    spriteBatch.Draw(renderTiny, Vector2.Zero, Color.White);
                else
                    spriteBatch.Draw(contentRender, default, null, Color.White, 0, default, 1f / DownSampleCount, 0, 0);
                graphicsDevice.SetRenderTarget(renderTiny);

                graphicsDevice.Clear(Color.Transparent);
                effect.CurrentTechnique.Passes[pass2].Apply();
                spriteBatch.Draw(renderTinySwap, Vector2.Zero, Color.White);
            }
            graphicsDevice.SetRenderTarget(renderTinySwap);
            graphicsDevice.Clear(Color.Transparent);
            effect.CurrentTechnique.Passes[pass1].Apply();
            if (Count > 1)
                spriteBatch.Draw(renderTiny, Vector2.Zero, Color.White);
            else
                spriteBatch.Draw(contentRender, default, null, Color.White, 0, default, 1f / DownSampleCount, 0, 0);

            graphicsDevice.SetRenderTarget(renderTiny);
            graphicsDevice.Clear(Color.Transparent);
            effect.CurrentTechnique.Passes[pass2].Apply();
            spriteBatch.Draw(renderTinySwap, Vector2.Zero, Color.White);

            #endregion

            #region 将发光内容叠加至原图层上

            graphicsDevice.BlendState = BlendState.Additive;

            graphicsDevice.SetRenderTarget(contentRender);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.spriteEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(renderTiny, Vector2.Zero, Color.White);
            spriteBatch.Draw(assistRender, default, null, Color.White, 0, default, 1f / DownSampleCount, 0, 0);
            #endregion

        }

        #endregion

        #region 绘制内容_非降采样

        else
        {

            #region 变量准备

            effect.Parameters["screenScale"].SetValue(Main.ScreenSize.ToVector2());

            RenderTarget2D assistRenderSwap = LogSpiralLibraryMod.Instance.Render_Swap2;
            int pass1 = UseModeMK ? 4 : 3;
            int pass2 = UseModeMK ? 4 : 2;

            #endregion

            #region 反复模糊处理

            for (int n = 0; n < Count - 1; n++)//times是模糊次数(
            {
                graphicsDevice.SetRenderTarget(assistRender);
                graphicsDevice.Clear(Color.Transparent);
                effect.CurrentTechnique.Passes[pass1].Apply();
                if (n != 0)
                    spriteBatch.Draw(assistRenderSwap, Vector2.Zero, Color.White);
                else
                    spriteBatch.Draw(contentRender, Vector2.Zero, Color.White);
                graphicsDevice.SetRenderTarget(assistRenderSwap);
                graphicsDevice.Clear(Color.Transparent);
                effect.CurrentTechnique.Passes[pass2].Apply();
                spriteBatch.Draw(assistRender, Vector2.Zero, Color.White);
            }
            graphicsDevice.SetRenderTarget(assistRender);
            graphicsDevice.Clear(Color.Transparent);
            effect.CurrentTechnique.Passes[pass1].Apply();
            if (Count > 1)
                spriteBatch.Draw(assistRenderSwap, Vector2.Zero, Color.White);
            else
                spriteBatch.Draw(contentRender, Vector2.Zero, Color.White);
            graphicsDevice.SetRenderTarget(assistRenderSwap);
            graphicsDevice.Clear(Color.Transparent);
            effect.CurrentTechnique.Passes[pass2].Apply();
            spriteBatch.Draw(assistRender, Vector2.Zero, Color.White);

            #endregion

            #region 将发光内容叠加至原图层上
            graphicsDevice.SetRenderTarget(assistRender);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.spriteEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.BlendState = LogSpiralLibraryMod.AllOne;
            spriteBatch.Draw(contentRender, Vector2.Zero, Color.White);



            graphicsDevice.SetRenderTarget(contentRender);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(assistRenderSwap, Vector2.Zero, Color.White);
            spriteBatch.Draw(assistRender, Vector2.Zero, Color.White);

            #endregion

        }

        #endregion

        #region 恢复状态

        spriteBatch.End();

        #endregion

    }

    #endregion

    public BloomEffect() : this(0, 0, 0, 0, true, 0, true) { }
}
public class BloomConfigs : IAvailabilityChangableConfig
{

    public bool Available { get; set; } = true;

    [Range(0, 1f)]
    [DefaultValue(0f)]
    public float Threshold
    {
        get;
        set;
    } = 0;

    [Range(0, 1f)]
    [DefaultValue(1f)]
    public float Intensity
    {
        get;
        set;
    } = 1;


    [Range(1f, 12f)]
    [DefaultValue(1f)]
    public float Range
    {
        get;
        set;
    } = 1;

    [Range(1, 5)]
    [Slider]
    [DrawTicks]
    [DefaultValue(3)]
    public int Count
    {
        get;
        set;
    } = 3;

    [JsonIgnore]
    [DefaultValue(true)]
    public bool Additive
    {
        get;
        set;
    } = true;

    [DefaultValue(2)]
    [Range(0, 2)]
    [Slider]
    [DrawTicks]
    public int DownSampleLevel
    {
        get;
        set;
    } = 2;

    [DefaultValue(true)]
    public bool UseModeMK
    {
        get;
        set;
    } = true;

    [JsonIgnore]
    public BloomEffect EffectInstance => field ??= !Available ? new() : new BloomEffect(Threshold, Intensity * 1.125f, Range, Count, Additive, (byte)DownSampleLevel, UseModeMK);

}
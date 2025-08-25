namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.ComplexPanel;

public partial class ComplexPanelInfo
{
    #region 背景

    /// <summary>
    /// 指定背景贴图，为null的时候使用默认背景
    /// </summary>
    public Texture2D backgroundTexture;

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

    #endregion 背景

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

    #endregion 边框

    #region 全局

    public Color mainColor;
    public Vector2 origin;
    public float scaler = 1f;
    public Vector2 offset;
    public Rectangle destination;

    #endregion 全局

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

    public ComplexPanelInfo()
    {
        mainColor = Color.White;
    }
}
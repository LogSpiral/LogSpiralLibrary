using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;
using Terraria.Audio;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

/// <summary>
/// 不同物品有自己独有的标准值
/// </summary>
public struct StandardInfo
{

    /// <summary>
    /// 物品贴图朝向
    /// </summary>
    public float standardRotation = MathHelper.PiOver4;
    /// <summary>
    /// 物品手持中心
    /// </summary>
    public Vector2 standardOrigin = new(.1f, .9f);
    /// <summary>
    /// 标准持续时长
    /// </summary>
    public int standardTimer;

    /// <summary>
    /// 标准射击冷却
    /// </summary>
    public int standardShotCooldown;
    /// <summary>
    /// 标准颜色
    /// </summary>
    public Color standardColor;
    /// <summary>
    /// 高亮贴图
    /// </summary>
    public Texture2D standardGlowTexture;
    public VertexDrawInfoStandardInfo vertexStandard = default;
    public int itemType;
    public SoundStyle? soundStyle;
    public float dustAmount;
    public Rectangle? frame;
    public float extraLight = 1f;
    public StandardInfo()
    {
    }
    public StandardInfo(float rotation, Vector2 origin, int timer, Color color, Texture2D glow, int type)
    {
        standardRotation = rotation;
        standardOrigin = origin;
        standardTimer = timer;
        standardColor = color;
        standardGlowTexture = glow;
        itemType = type;
    }
    //TODO 改成弹幕序列独有
}

/// <summary>
/// 物品相应顶点绘制特效的标准值
/// </summary>
public struct VertexDrawInfoStandardInfo
{
    public bool active;
    public Texture2D heatMap;
    public int timeLeft;
    public float scaler;
    public float heatRotation;
    public float alphaFactor;
    public IRenderDrawInfo[][] renderInfos;
    /// <summary>
    /// x:方位渐变 y:武器贴图 z:热度图 ,均为颜色系数
    /// </summary>
    public Vector3 colorVec;

    public (int, int)? swooshTexIndex;
    public (int, int)? stabTexIndex;
}

using Terraria.Audio;
using Terraria.Graphics.Shaders;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.Core;

/// <summary>
/// 不同物品有自己独有的标准值
/// </summary>
public class StandardInfo
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

    /// <summary>
    /// 标准物品大小
    /// </summary>
    public float standardScaler;

    public VertexDrawStandardInfo VertexStandard
    {
        get
        {
            if (field == null)
            {
                field = new VertexDrawStandardInfo();
                InitalizeVertexStandard(field);
            }
            return field;
        }
    }

    public int itemType;
    public SoundStyle? soundStyle;
    public float dustAmount;
    public Rectangle? frame;
    public float extraLight = 1f;

    public StandardInfo()
    {
    }

    public StandardInfo(float rotation, Vector2 origin, int scaler, int timer, Color color, Texture2D glow, int type)
    {
        standardRotation = rotation;
        standardOrigin = origin;
        standardScaler = scaler;
        standardTimer = timer;
        standardColor = color;
        standardGlowTexture = glow;
        itemType = type;
    }

    public void InitalizeVertexStandard(VertexDrawStandardInfo vertexStandard)
    {
        vertexStandard.scaler = standardScaler;
        vertexStandard.timeLeft = 30;
        vertexStandard.colorVec = Vector3.one * .333f;
        vertexStandard.active = true;
    }

    //TODO 改成弹幕序列独有
}

/// <summary>
/// 物品相应顶点绘制特效的标准值
/// </summary>
public class VertexDrawStandardInfo
{
    public string canvasName = RenderCanvasSystem.DEFAULTCANVASNAME;
    public bool active;
    public Texture2D heatMap;
    public int timeLeft;
    public float scaler;
    public float heatRotation;
    public float alphaFactor = 1f;

    /// <summary>
    /// x:方位渐变 y:武器贴图 z:热度图 ,均为颜色系数
    /// </summary>
    public Vector3 colorVec;

    public (int, int)? swooshTexIndex;
    public (int, int)? stabTexIndex;

    public int? dyeShaderID;

    public void SetDyeShaderID(int itemID) => dyeShaderID = GameShaders.Armor.GetShaderIdFromItemId(itemID);
}
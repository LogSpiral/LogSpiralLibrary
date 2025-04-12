using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using Terraria.Localization;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
// 以如下标准整理
// 类中类
// 常数/常属性(调试用)
// 辅助字段
// 参数字段
// 辅助属性
// 重写属性
// 辅助函数
// 重写函数

// 其中重写属性按如下顺序排序
// Factor -> Rotation -> Center -> Origin -> Damage -> Attactive

// 其中重写函数按如下顺序排序
// Update -> Active -> Single -> Charge -> Attack -> Collide -> Draw -> Net(目前无效)
// Start -> End
public abstract partial class MeleeAction : ModType, ISequenceElement
{
    #region 参数属性
    //持续时间 角度 位移 修改数据
    /// <summary>
    /// 近战数据修改
    /// </summary>
    [ElementCustomData]
    //[CustomSeqConfigItem(typeof(SeqActionModifyDataElement))]
    public ActionModifyData ModifyData { get; set; } = new ActionModifyData(1);
    /// <summary>
    /// 执行次数
    /// </summary>
    [ElementCustomData]
    //[CustomSeqConfigItem(typeof(SeqIntInputElement))]
    public virtual int Cycle { get; set; } = 1;
    #endregion

    #region 逻辑属性
    /// <summary>
    /// 旋转角，非插值
    /// </summary>
    public float Rotation { get; set; }
    /// <summary>
    /// 扁平程度？
    /// </summary>
    public float KValue { get; set; } = 1f;
    public int counter { get; set; }
    public float fTimer;
    public int timer
    {
        get => (int)fTimer; set => fTimer = value;
    }
    public int timerMax { get; set; }
    public bool flip { get; set; }
    #endregion

    #region 重写属性
    /// <summary>
    /// 当前周期的进度
    /// </summary>
    public virtual float Factor => fTimer / timerMax;
    //public virtual float Factor => timer / (float)timerMax;
    /// <summary>
    /// 中心偏移量，默认零向量
    /// </summary>
    public virtual Vector2 offsetCenter => default;
    /// <summary>
    /// 原点偏移量，默认为零向量,取值范围[0,1]
    /// </summary>
    public virtual Vector2 offsetOrigin => default;
    /// <summary>
    /// 旋转量
    /// </summary>
    public virtual float offsetRotation { get; }

    public virtual float CompositeArmRotation => targetedVector.ToRotation() - MathHelper.PiOver2;
    /// <summary>
    /// 大小
    /// </summary>
    public virtual float offsetSize => 1f;
    /// <summary>
    /// 是否具有攻击性
    /// </summary>
    public virtual bool Attacktive { get; }

    /// <summary>
    /// 伤害
    /// </summary>
    public virtual float offsetDamage => 1f;

    public virtual bool OwnerHitCheek => true;
    #endregion

    #region 辅助属性
    public Entity Owner { get; set; }
    public Projectile Projectile { get; set; }
    public StandardInfo standardInfo { get; set; }
    public int CurrentDamage => Owner is Player plr ? (int)(plr.GetWeaponDamage(plr.HeldItem) * ModifyData.actionOffsetDamage * offsetDamage) : Projectile.damage;
    public string LocalizationCategory => nameof(MeleeAction);
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", () => GetType().Name);
    public abstract string Category { get; }
    #endregion
}

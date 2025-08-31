using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using PropertyPanelLibrary.PropertyPanelComponents.Attributes;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces;
using System.Collections.Generic;
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
public partial class MeleeAction : ModType, ISequenceElement, ILocalizedModType, ILoadable, IMemberLocalized
{
    #region 参数属性

    //持续时间 角度 位移 修改数据
    /// <summary>
    /// 近战数据修改
    /// </summary>
    [ElementCustomData]
    [LabelPresentValue(false)]
    public ActionModifyData ModifyData { get; set; } = new ActionModifyData(1);

    /// <summary>
    /// 执行次数
    /// </summary>
    [ElementCustomData]
    public virtual int CounterMax { get; set; } = 1;

    #endregion 参数属性

    #region 逻辑属性

    /// <summary>
    /// 旋转角，非插值
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// 扁平程度？
    /// </summary>
    public float KValue { get; set; } = 1f;

    public int Counter { get; set; }
    protected float fTimer;

    public int Timer
    {
        get => (int)fTimer; set => fTimer = value;
    }

    public int TimerMax { get; set; }

    /// <summary>
    /// 当前武器视觉上是否翻转
    /// </summary>
    public bool Flip { get; set; }

    bool ISequenceElement.IsCompleted => Timer == 0 && Counter == CounterMax;

    #endregion 逻辑属性

    #region 重写属性

    /// <summary>
    /// 当前周期的进度
    /// </summary>
    public virtual float Factor => fTimer / TimerMax;

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

    #endregion 重写属性

    #region 辅助属性

    /// <summary>
    /// 当前元素所属的实体
    /// <br>目前只支持<see cref="Player"/></br>
    /// </summary>
    public Entity Owner { get; set; }

    /// <summary>
    /// 当前元素代理的弹幕对象
    /// </summary>
    public Projectile Projectile { get; set; }

    /// <summary>
    /// 标准信息
    /// </summary>
    public StandardInfo StandardInfo { get; set; }

    public int CurrentDamage => Owner is Player plr ? (int)(plr.GetWeaponDamage(plr.HeldItem) * ModifyData.Damage * offsetDamage) : Projectile.damage;
    public string LocalizationCategory => $"Sequence.{nameof(MeleeAction)}";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", () => GetType().Name);
    public virtual string Category { get; } = "";

    #endregion 辅助属性

    public override string ToString() => DisplayName.ToString();


    string IMemberLocalized.LocalizationRootPath => Mod.GetLocalizationKey($"{LocalizationCategory}.{Name}");
    private static string[] Suffixes { get; } = ["Label"];
    IReadOnlyList<string> IMemberLocalized.LocalizationSuffixes => Suffixes;
}
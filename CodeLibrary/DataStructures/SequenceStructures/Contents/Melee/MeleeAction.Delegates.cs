using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

partial class MeleeAction
{
    #region 辅助字段
    public Action<MeleeAction> _OnActive;
    public Action<MeleeAction> _OnAttack;
    public Action<MeleeAction> _OnCharge;
    public Action<MeleeAction> _OnDeactive;
    public Action<MeleeAction> _OnEndAttack;
    public Action<MeleeAction> _OnEndSingle;
    public Action<MeleeAction> _OnStartAttack;
    public Action<MeleeAction> _OnStartSingle;
    //上面这些也许大概已经过时了？
    //毕竟现在都是直接操作xml文件了
    //当然要是愿意直接用代码搭建序列剑，也是可以用这些的
    #endregion

    #region 参数字段
    [ElementCustomData]
    public SequenceDelegateDefinition OnEndAttackDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    public SequenceDelegateDefinition OnStartAttackDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    public SequenceDelegateDefinition OnAttackDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    public SequenceDelegateDefinition OnChargeDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    public SequenceDelegateDefinition OnActiveDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    public SequenceDelegateDefinition OnDeactiveDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    public SequenceDelegateDefinition OnEndSingleDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    public SequenceDelegateDefinition OnStartSingleDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    public SequenceDelegateDefinition OnHitTargetDelegate { get; set; } = new SequenceDelegateDefinition();
    #endregion
}

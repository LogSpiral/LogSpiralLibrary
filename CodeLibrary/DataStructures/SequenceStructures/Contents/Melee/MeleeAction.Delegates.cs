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
    public SeqDelegateDefinition OnEndAttackDelegate { get; set; } = new SeqDelegateDefinition();

    [ElementCustomData]
    public SeqDelegateDefinition OnStartAttackDelegate { get; set; } = new SeqDelegateDefinition();

    [ElementCustomData]
    public SeqDelegateDefinition OnAttackDelegate { get; set; } = new SeqDelegateDefinition();

    [ElementCustomData]
    public SeqDelegateDefinition OnChargeDelegate { get; set; } = new SeqDelegateDefinition();

    [ElementCustomData]
    public SeqDelegateDefinition OnActiveDelegate { get; set; } = new SeqDelegateDefinition();

    [ElementCustomData]
    public SeqDelegateDefinition OnDeactiveDelegate { get; set; } = new SeqDelegateDefinition();

    [ElementCustomData]
    public SeqDelegateDefinition OnEndSingleDelegate { get; set; } = new SeqDelegateDefinition();

    [ElementCustomData]
    public SeqDelegateDefinition OnStartSingleDelegate { get; set; } = new SeqDelegateDefinition();

    [ElementCustomData]
    public SeqDelegateDefinition OnHitTargetDelegate { get; set; } = new SeqDelegateDefinition();
    #endregion
}

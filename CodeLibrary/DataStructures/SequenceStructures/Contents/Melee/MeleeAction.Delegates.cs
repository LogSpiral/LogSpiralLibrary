using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using PropertyPanelLibrary.EntityDefinition;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

public partial class MeleeAction
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

    #endregion 辅助字段

    #region 参数字段

    [ElementCustomData]
    [CustomEntityDefinitionHandler<SequenceDelegateDefinitionHandler>]
    public SequenceDelegateDefinition OnEndAttackDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    [CustomEntityDefinitionHandler<SequenceDelegateDefinitionHandler>]
    public SequenceDelegateDefinition OnStartAttackDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    [CustomEntityDefinitionHandler<SequenceDelegateDefinitionHandler>]
    public SequenceDelegateDefinition OnAttackDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    [CustomEntityDefinitionHandler<SequenceDelegateDefinitionHandler>]
    public SequenceDelegateDefinition OnChargeDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    [CustomEntityDefinitionHandler<SequenceDelegateDefinitionHandler>]
    public SequenceDelegateDefinition OnActiveDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    [CustomEntityDefinitionHandler<SequenceDelegateDefinitionHandler>]
    public SequenceDelegateDefinition OnDeactiveDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    [CustomEntityDefinitionHandler<SequenceDelegateDefinitionHandler>]
    public SequenceDelegateDefinition OnEndSingleDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    [CustomEntityDefinitionHandler<SequenceDelegateDefinitionHandler>]
    public SequenceDelegateDefinition OnStartSingleDelegate { get; set; } = new SequenceDelegateDefinition();

    [ElementCustomData]
    [CustomEntityDefinitionHandler<SequenceDelegateDefinitionHandler>]
    public SequenceDelegateDefinition OnHitTargetDelegate { get; set; } = new SequenceDelegateDefinition();

    #endregion 参数字段
}
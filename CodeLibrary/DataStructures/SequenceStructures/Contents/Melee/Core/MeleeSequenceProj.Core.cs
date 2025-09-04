using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

/// <summary>
/// 来把基剑
/// </summary>
public abstract partial class MeleeSequenceProj : ModProjectile
{
    #region 辅助属性

    //辅助更新函数使用的属性
    public Player Player => Main.player[Projectile.owner];

    public MeleeAction CurrentElement
    {
        get
        {
            if (IsLocalProj)
                field = SequenceModel?.CurrentElement as MeleeAction ?? null;
            return field;
        }
        set;
    }

    #endregion 辅助属性

    #region 参数属性

    // 这里算是这个弹幕的核心部分
    // 分别是标准参数
    // 目前执行组件
    // 更新函数
    public StandardInfo StandardInfo
    {
        get
        {
            if (field == null)
            {
                field = new(-MathHelper.PiOver4, new Vector2(0.1f, 0.9f), 80, Player.itemAnimationMax, Color.White, null, ItemID.IronBroadsword);
                InitializeStandardInfo(field, field.VertexStandard);
            }
            return field;
        }
    }

    // 改成发出弹幕时生成而非总是从头new?

    #endregion 参数属性

    #region 重写函数

    public virtual void InitializeStandardInfo(StandardInfo standardInfo, VertexDrawStandardInfo vertexStandard)
    {
    }

    public virtual void UpdateStandardInfo(StandardInfo standardInfo, VertexDrawStandardInfo vertexStandard)
    {
    }

    // 你只需要知道上面这一段负责 *记录* 我们弹幕具体是怎么运行
    // 弹幕发射和弹幕加载时的初始化，这部分和别的弹幕大差不差
    public override void SetDefaults()
    {
        Projectile.timeLeft = 10;
        Projectile.width = Projectile.height = 1;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
        Projectile.ownerHitCheck = true;
        base.SetDefaults();
    }

    public override void AI()
    {
        //这里是手持弹幕的一些常规检测和赋值
        Player.heldProj = Projectile.whoAmI;
        Projectile.damage = Player.GetWeaponDamage(Player.HeldItem);
        Projectile.direction = Player.direction;
        Projectile.velocity = (Player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition - Player.Center).SafeNormalize(default);
        UpdateStandardInfo(StandardInfo, StandardInfo.VertexStandard);
        if (Player.DeadOrGhost)
        {
            CurrentElement?.OnDeactive();
            Projectile.Kill();
        }

        bool triggered = Player.controlUseItem || Player.controlUseTile || CurrentElement == null;//首要-触发条件

        if (triggered)
            SequenceModel?.IsCompleted = false;

        if (Player.GetModPlayer<SequencePlayer>().PendingForcedNext)
        {
            triggered = true;
            Player.GetModPlayer<SequencePlayer>().PendingForcedNext = false;
        }

        if (triggered || !CurrentElement.IsCompleted)
            SequenceModel?.Update();

        if (!IsLocalProj && CurrentElement is { IsCompleted : false})
            CurrentElement.Update();

        if (CurrentElement == null) return;

        //if (!IsLocalProj)
        //    Main.NewText((CurrentElement.Counter, CurrentElement.CounterMax, CurrentElement.TimerMax, CurrentElement.Timer));

        if (!CurrentElement.IsCompleted || triggered)
            Projectile.timeLeft = 32;
        //依旧是常规赋值，但是要中间那段执行正常才应当执行
        Projectile.Center = Player.Center + CurrentElement.OffsetCenter + Player.gfxOffY * Vector2.UnitY;
        if (Player.itemAnimation != 2)
            Player.itemAnimation = 2;
        if (Player.itemTime != 2)
            Player.itemTime = 2;

        if (CurrentElement.IsCompleted)
            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 0);


        base.AI();
    }

    //还有这个是阻止弹幕自行更新位置的，因为我们会在核心逻辑那里写入弹幕的位置
    public override bool ShouldUpdatePosition() => false;

    #endregion 重写函数
}
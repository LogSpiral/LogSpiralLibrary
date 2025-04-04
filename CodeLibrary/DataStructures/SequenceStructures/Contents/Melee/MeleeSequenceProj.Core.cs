using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
/// <summary>
/// 来把基剑
/// </summary>
public abstract partial class MeleeSequenceProj : ModProjectile
{
    #region 辅助属性
    //辅助更新函数使用的属性
    public Player player => Main.player[Projectile.owner];
    public MeleeAction currentData => meleeSequence?.currentData;
    #endregion

    #region 参数属性
    //这里算是这个弹幕的核心部分
    //分别是标准参数
    //目前执行组件
    //更新函数
    public virtual StandardInfo StandardInfo => new(-MathHelper.PiOver4, new Vector2(0.1f, 0.9f), player.itemAnimationMax, Color.White, null, ItemID.IronBroadsword);
    #endregion

    #region 重写函数
    //你只需要知道上面这一段负责 *记录* 我们弹幕具体是怎么运行
    //弹幕发射和弹幕加载时的初始化，这部分和别的弹幕大差不差
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
        //if (!SequenceSystem.loaded) ModContent.GetInstance<SequenceSystem>().Load();
        //if (!SequenceManager<MeleeAction>.loaded) SequenceManager<MeleeAction>.Load();
        //InitializeSequence(Mod.Name, Name);

        base.SetDefaults();
    }

    public override void AI()
    {
        //if (player.heldProj != -1 && player.heldProj != Projectile.whoAmI)
        //    try
        //    {
        //        Main.projectile[player.heldProj].Kill();
        //    }
        //    catch (Exception e)
        //    {
        //        Mod.Logger.Error("Error occured when try to clean other heldProj:" + e.Message);
        //    }
        //这里是手持弹幕的一些常规检测和赋值
        player.heldProj = Projectile.whoAmI;
        Projectile.damage = player.GetWeaponDamage(player.HeldItem);
        Projectile.direction = player.direction;
        Projectile.velocity = (player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition - player.Center).SafeNormalize(default);
        if (meleeSequence == null)//这里确保弹幕的执行序列加载到了
        {
            InitializeSequence(Mod.Name, Name);
            meleeSequence.SetOwner(player);
        }
        if (player.DeadOrGhost)
        {
            currentData?.OnDeactive();
            Projectile.Kill();
        }
        if (Projectile.timeLeft == 10) return;

        //这里就是由弹幕检测玩家是否符合执行条件，符合就更新状态
        if (meleeSequence.Groups.Count < 1) return;
        bool flag1 = player.controlUseItem || player.controlUseTile || currentData == null;//首要-触发条件
        if (player.GetModPlayer<SequencePlayer>().PendingForcedNext)
        {
            flag1 = true;
            player.GetModPlayer<SequencePlayer>().PendingForcedNext = false;
        }
        int prev = -1;
        bool flag2 = false;//次要-持续条件
        if (currentData != null)
        {
            flag2 = currentData.counter < currentData.Cycle;//我还没完事呢
            flag2 |= currentData.counter == currentData.Cycle && currentData.timer >= 0;//最后一次
                                                                                        //flag2 &= !meleeSequence.currentWrapper.finished;//如果当前打包器完工了就给我停下
                                                                                        //Main.NewText(currentData.Cycle);
            prev = currentData.timer;
        }
        if (
           flag1 || flag2// 
            )
        {
            if (flag1 || (currentData.counter < currentData.Cycle || currentData.counter == currentData.Cycle && currentData.timer > 0) && !meleeSequence.currentWrapper.finished)
                Projectile.timeLeft = 2;

            meleeSequence.Update(player, Projectile, StandardInfo, flag1);
        }
        if (currentData == null) return;
        //依旧是常规赋值，但是要中间那段执行正常才应当执行
        Projectile.Center = player.Center + currentData.offsetCenter + player.gfxOffY * Vector2.UnitY;
        //if (prev != currentData.timer - 1) 
        //{
        //    player.itemAnimation = currentData.timer;
        //    player.itemTime = currentData.timer;

        //    //player.itemAnimationMax = currentData.timerMax;
        //    //player.itemTimeMax = currentData.timerMax;
        //}
        if (player.itemAnimation < 2)
            player.itemAnimation = 2;
        if (player.itemTime < 2)
            player.itemTime = 2;
        base.AI();
    }

    public override void OnKill(int timeLeft)
    {
        //if (Main.netMode != NetmodeID.Server)
        meleeSequence?.ResetCounter();
        base.OnKill(timeLeft);
    }

    //还有这个是阻止弹幕自行更新位置的，因为我们会在核心逻辑那里写入弹幕的位置
    public override bool ShouldUpdatePosition()
    {
        return false;
    }
    #endregion
}

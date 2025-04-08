using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using System.ComponentModel;
using Terraria.Audio;
using Terraria.ModLoader.Config;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.ExtendedMelee;

public class StormInfo : ExtendedMelee
{
    #region 辅助字段
    protected UltraSwoosh swoosh;
    #endregion

    #region 参数字段
    [DefaultValue(4f)]
    [Range(0, 4f)]
    [Increment(0.1f)]
    [ElementCustomData]
    public float angleRange = 4f;


    [DefaultValue(2f)]
    [Range(1, 4f)]
    [Increment(0.1f)]
    [ElementCustomData]
    public float xScaler = 2f;
    #endregion

    #region 重写属性

    public override float offsetRotation => MathHelper.SmoothStep(0, MathHelper.TwoPi * angleRange, Factor) * (flip ? 1 : -1);

    public override bool Attacktive => Math.Abs(Factor - 0.5f) < 0.45f;
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        if (!float.IsNaN(targetedVector.X))
            Owner.direction = Math.Sign(targetedVector.X);
        base.Update(triggered);
    }

    public override void OnStartAttack()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            var u = swoosh = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, standardInfo.standardTimer, standardInfo.vertexStandard.scaler, Owner.Center, null, flip, Rotation, KValue, (0, 1));
            u.ApplyStdValueToVtxEffect(standardInfo);
        }
        SoundEngine.PlaySound(SoundID.DD2_BookStaffCast);

        base.OnStartAttack();
    }
    public override void OnAttack()
    {
        if (swoosh != null)
        {
            swoosh.timeLeft = (int)(swoosh.timeLeftMax * (1 - Math.Abs(0.5f - Factor) * 2));
            swoosh.angleRange = (offsetRotation / MathHelper.Pi - 1f + .25f * (flip ? -1 : 1), offsetRotation / MathHelper.Pi + .25f * (flip ? -1 : 1));
            if (flip)
            {
                swoosh.angleRange = (1 - swoosh.angleRange.to, 1 - swoosh.angleRange.from);

            }
            swoosh.center = Owner.Center;
        }
        base.OnAttack();
    }
    public override void OnStartSingle()
    {
        flip = Owner.direction == -1;
        KValue = xScaler;
        base.OnStartSingle();
    }
    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        Projectile.localNPCHitCooldown /= 4;
    }
    #endregion
}

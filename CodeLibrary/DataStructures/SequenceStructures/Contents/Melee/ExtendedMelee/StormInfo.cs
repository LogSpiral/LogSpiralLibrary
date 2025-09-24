using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.ComponentModel;
using System.IO;
using Terraria.Audio;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.ExtendedMelee;

public class StormInfo : ExtendedMelee
{
    #region 辅助字段

    protected UltraSwoosh swoosh;

    #endregion 辅助字段

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

    #endregion 参数字段

    #region 重写属性

    public override float OffsetRotation => MathHelper.SmoothStep(0, MathHelper.TwoPi * angleRange, Factor) * (Flip ? 1 : -1);

    public override bool Attacktive => Math.Abs(Factor - 0.5f) < 0.45f;

    #endregion 重写属性

    #region 重写函数

    public override void UpdateStatus(bool triggered)
    {
        if (!float.IsNaN(targetedVector.X))
            Owner.direction = Math.Sign(targetedVector.X);

        if (swoosh != null)
        {
            swoosh.timeLeft = Math.Max((int)(swoosh.timeLeftMax * (1 - Math.Abs(0.5f - Factor) * 2)), 2);
            swoosh.angleRange = (OffsetRotation / MathHelper.Pi - 1f + .25f * (Flip ? -1 : 1), OffsetRotation / MathHelper.Pi + .25f * (Flip ? -1 : 1));
            if (Flip)
                swoosh.angleRange = (1 - swoosh.angleRange.to, 1 - swoosh.angleRange.from);

            swoosh.center = Owner.Center;
        }
        base.UpdateStatus(triggered);
    }

    public override void OnStartAttack()
    {
        if (!Main.dedServ)
        {
            var verS = StandardInfo.VertexStandard;

            var u = swoosh = UltraSwoosh.NewUltraSwoosh(verS.canvasName, verS.timeLeft, verS.scaler, Owner.Center, (0, 0));
            u.heatMap = verS.heatMap;
            u.negativeDir = Flip;
            u.rotation = Rotation;
            u.ColorVector = verS.colorVec;
            u.xScaler = KValue;
            (u.aniTexIndex, u.baseTexIndex) = verS.swooshTexIndex ?? (3, 7);
            u.ApplyStdValueToVtxEffect(StandardInfo);
        }
        SoundEngine.PlaySound(SoundID.DD2_BookStaffCast, Owner.Center);

        base.OnStartAttack();
    }

    public override void OnStartSingle()
    {
        Flip = Owner.direction == -1;
        KValue = xScaler;
        base.OnStartSingle();
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        Projectile.localNPCHitCooldown /= 4;
    }

    public override void NetSendInitializeElement(BinaryWriter writer)
    {
        base.NetSendInitializeElement(writer);
        writer.Write(xScaler);
        writer.Write(angleRange);
    }

    public override void NetReceiveInitializeElement(BinaryReader reader)
    {
        base.NetReceiveInitializeElement(reader);
        xScaler = reader.ReadSingle();
        angleRange = reader.ReadSingle();
    }
    #endregion 重写函数
}

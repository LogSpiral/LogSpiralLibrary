using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.ComponentModel;
using System.IO;
using Terraria.Audio;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;

public class StabInfo : LSLMelee
{
    #region 辅助字段

    private int hitCounter;

    #endregion 辅助字段

    #region 参数字段

    [DefaultValue(1f)]
    [Range(0.2f, 5f)]
    [Increment(.2f)]
    [ElementCustomData]
    public float minKValue = 1f;

    [DefaultValue(1.4f)]
    [Range(0f, 5f)]
    [Increment(.2f)]
    [ElementCustomData]
    public float KValueRange = 1f;

    [DefaultValue(false)]
    [ElementCustomData]
    public bool visualCentered = false;

    [DefaultValue(MathHelper.Pi / 6)]
    [ElementCustomData]
    [Range(0, MathHelper.PiOver2)]
    [Increment(MathHelper.Pi / 24)]
    public float randAngleRange = MathHelper.Pi / 6;

    #endregion 参数字段

    #region 重写属性

    public override float Factor
    {
        get
        {
            float k = MathF.Sqrt(TimerMax);
            float max = TimerMax;
            float t = Timer;
            if (t >= k)
            {
                return MathHelper.SmoothStep(1, 1.125f, Utils.GetLerpValue(max, k, t, true));
            }
            else
            {
                //return MathHelper.SmoothStep(0, 1.125f, t / k);
                return MathHelper.Hermite(0, -5, 1.125f, 0, t / k);
            }
            float fac = base.Factor;
            fac = 1 - fac;
            fac *= fac;
            return fac.CosFactor();
        }
    }

    public override Vector2 OffsetOrigin => new Vector2(Factor * .4f - .4f, 0).RotatedBy(StandardInfo.standardRotation);
    public override float OffsetDamage => MathF.Pow(.75f, hitCounter);
    public override bool Attacktive => Timer <= MathF.Sqrt(TimerMax);

    #endregion 重写属性

    #region 辅助函数

    public UltraStab NewStab()
    {
        var verS = StandardInfo.VertexStandard;
        if (verS.active)
        {
            var pair = StandardInfo.VertexStandard.stabTexIndex;
            UltraStab u;
            if (StandardInfo.itemType == ItemID.TrueExcalibur)
            {
                float size = verS.scaler * ModifyData.Size * OffsetSize * 1.25f;
                u = UltraStab.NewUltraStab(verS.canvasName, (int)(verS.timeLeft * 1.2f), size, Owner.Center);
                u.heatMap = LogSpiralLibraryMod.HeatMap[5].Value;
                u.negativeDir = Flip;
                u.rotation = Rotation;
                u.xScaler = KValue * 1.5f;
                u.aniTexIndex = pair?.Item1 ?? 9;
                u.baseTexIndex = pair?.Item2 ?? 0;
                u.ColorVector = verS.colorVec;

                var su = UltraStab.NewUltraStab(verS.canvasName, verS.timeLeft, size * .67f, Owner.Center + Rotation.ToRotationVector2() * size * .2f);
                su.heatMap = verS.heatMap;
                su.negativeDir = !Flip;
                su.rotation = Rotation;
                u.xScaler = KValue * 1.5f;
                u.aniTexIndex = pair?.Item1 ?? 9;
                u.baseTexIndex = pair?.Item2 ?? 0;
                u.ColorVector = verS.colorVec;
                su.ApplyStdValueToVtxEffect(StandardInfo);

                u.gather = !visualCentered;

                su.gather = !visualCentered;
            }
            else
            {
                float size = verS.scaler * ModifyData.Size * OffsetSize * 1.25f;
                u = UltraStab.NewUltraStab(verS.canvasName, (int)(verS.timeLeft * 1.2f), size, Owner.Center);
                u.heatMap = verS.heatMap;
                u.negativeDir = Flip;
                u.rotation = Rotation;
                u.xScaler = KValue * 1.5f;
                u.aniTexIndex = pair?.Item1 ?? 9;
                u.baseTexIndex = pair?.Item2 ?? 0;
                u.ColorVector = verS.colorVec;
                u.gather = !visualCentered;
            }
            u.ApplyStdValueToVtxEffect(StandardInfo);
            return u;
        }
        return null;
    }

    #endregion 辅助函数

    #region 重写函数

    public override void OnStartSingle()
    {
        base.OnStartSingle();
        hitCounter = 0;
        KValue = minKValue + Main.rand.NextFloat(0, KValueRange);
        if (randAngleRange > 0)
            Rotation += Main.rand.NextFloat(0, Main.rand.NextFloat(0, randAngleRange)) * Main.rand.Next([-1, 1]);
        //if (Projectile.owner == Main.myPlayer)
        //{
        //    KValue = Main.rand.NextFloat(1f, 2.4f);
        //    Rotation += Main.rand.NextFloat(0, Main.rand.NextFloat(0, MathHelper.Pi / 6)) * Main.rand.Next(new int[] { -1, 1 });
        //    Projectile.netImportant = true;
        //}

        Flip ^= true;
    }

    public override void OnEndSingle()
    {
        hitCounter = 0;
        base.OnEndSingle();
    }

    public override void OnStartAttack()
    {
        SoundEngine.PlaySound(StandardInfo.soundStyle ?? MySoundID.SwooshNormal_1, Owner?.Center);
        if (Owner is Player plr)
        {
            SequencePlayer seqPlr = plr.GetModPlayer<SequencePlayer>();
            int dmg = CurrentDamage;
            if (StandardInfo.standardShotCooldown > 0)
            {
                float delta = StandardInfo.standardTimer * ModifyData.TimeScaler / CounterMax;
                seqPlr.cachedTime += delta + 1;
                int count = (int)(seqPlr.cachedTime / StandardInfo.standardShotCooldown);
                seqPlr.cachedTime -= count * StandardInfo.standardShotCooldown;
                if (count > 0)
                {
                    count--;
                    ShootProjCall(plr, dmg);
                }
                Vector2 orig = plr.Center;
                Vector2 unit = (Main.MouseWorld - orig).SafeNormalize(default) * 64;
                for (int i = 0; i < count; i++)
                {
                    plr.Center += unit.RotatedBy(MathHelper.Pi / count * (i - (count - 1) * .5f));
                    ShootProjCall(plr, dmg);

                    plr.Center = orig;
                }
                if (count > 0)
                    if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                    }
            }
            else
                ShootProjCall(plr, dmg);
        }
        for (int n = 0; n < 30; n++)
        {
            if (Main.rand.NextFloat(0, 1) < StandardInfo.dustAmount)
                for (int k = 0; k < 2; k++)
                {
                    var flag = k == 0;
                    var unit = ((MathHelper.TwoPi / 30 * n).ToRotationVector2() * new Vector2(1, .75f)).RotatedBy(Rotation) * (flag ? 2 : 1) * .5f;
                    var Center = Owner.Center + OffsetCenter + targetedVector * .75f;
                    var velocity = unit - targetedVector * .125f;//-Owner.velocity * 2 +
                    velocity *= 2;
                    MiscMethods.FastDust(Center, velocity, StandardInfo.standardColor);
                }
        }
        base.OnStartAttack();
    }

    public override void OnEndAttack()
    {
        NewStab();
        //Projectile.NewProjectile(Owner.GetSource_FromThis(), Owner.Center, Rotation.ToRotationVector2() * 16, ProjectileID.TerraBeam, 100, 1, Owner.whoAmI);
        base.OnEndAttack();
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        hitCounter++;
        base.OnHitEntity(victim, damageDone, context);
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Rotation);
        writer.Write(KValue);
        base.NetSend(writer);
    }

    public override void NetReceive(BinaryReader reader)
    {
        Rotation = reader.ReadSingle();
        KValue = reader.ReadSingle();
        base.NetReceive(reader);
    }

    #endregion 重写函数
}

public class RapidlyStabInfo : StabInfo
{
    #region 参数字段

    [ElementCustomData]
    [DefaultValue(-2)]
    [Range(-5, 5)]
    [Slider]
    public int rangeOffsetMin = -2;

    [ElementCustomData]
    [DefaultValue(1)]
    [Range(-5, 5)]
    [Slider]
    public int rangeOffsetMax = 1;

    [ElementCustomData]
    [Range(1, 10)]
    [DefaultValue(4)]
    [Slider]
    public int givenCycle = 4;

    #endregion 参数字段

    #region 辅助属性

    public (int min, int max) CycleOffsetRange
    {
        get => (rangeOffsetMin, rangeOffsetMax);
        set
        {
            var v = value;
            v.min = Math.Clamp(v.min, 1 - givenCycle, v.max);
            v.max = Math.Clamp(v.max, v.min, int.MaxValue);
            (rangeOffsetMin, rangeOffsetMax) = v;
            ResetCycle();
        }
    }

    #endregion 辅助属性

    #region 重写属性

    [ElementCustomDataAbabdoned]
    public override int CounterMax { get => realCycle; set => givenCycle = value; }

    public int realCycle;

    #endregion 重写属性

    #region 辅助函数

    private void ResetCycle()
    {
        realCycle = Math.Clamp(rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);

        //if (Projectile.owner == Main.myPlayer)
        //{
        //    realCycle = rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : Math.Clamp(givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);
        //    Projectile.netImportant = true;
        //}
    }

    #endregion 辅助函数

    #region 重写函数

    public override void OnActive()
    {
        ResetCycle();
        base.OnActive();
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((byte)realCycle);
        base.NetSend(writer);
    }

    public override void NetReceive(BinaryReader reader)
    {
        realCycle = reader.ReadByte();
        base.NetReceive(reader);
    }

    #endregion 重写函数
}
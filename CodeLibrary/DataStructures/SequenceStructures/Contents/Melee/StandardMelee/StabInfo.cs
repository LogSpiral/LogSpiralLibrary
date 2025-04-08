using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.ComponentModel;
using System.IO;
using Terraria.Audio;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;

public class StabInfo : LSLMelee
{
    #region 辅助字段
    int hitCounter;
    #endregion

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
    #endregion

    #region 重写属性
    public override float Factor
    {
        get
        {
            float k = MathF.Sqrt(timerMax);
            float max = timerMax;
            float t = timer;
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
    public override Vector2 offsetOrigin => new Vector2(Factor * .4f - .4f, 0).RotatedBy(standardInfo.standardRotation);
    public override float offsetDamage => MathF.Pow(.75f, hitCounter);
    public override bool Attacktive => timer <= MathF.Sqrt(timerMax);
    #endregion

    #region 辅助函数
    public UltraStab NewStab()
    {
        var verS = standardInfo.vertexStandard;
        if (verS.active)
        {
            var pair = standardInfo.vertexStandard.stabTexIndex;
            UltraStab u;
            if (standardInfo.itemType == ItemID.TrueExcalibur)
            {
                float size = verS.scaler * ModifyData.actionOffsetSize * offsetSize * 1.25f;
                u = UltraStab.NewUltraStab(standardInfo.standardColor, (int)(verS.timeLeft * 1.2f), size,
                Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, flip, Rotation, KValue * 1.5f, pair?.Item1 ?? 9, pair?.Item2 ?? 0, colorVec: verS.colorVec);
                var su = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, size * .67f,
                Owner.Center + Rotation.ToRotationVector2() * size * .2f, verS.heatMap, !flip, Rotation, KValue * 1.5f, pair?.Item1 ?? 9, pair?.Item2 ?? 0, colorVec: verS.colorVec);
                su.ApplyStdValueToVtxEffect(standardInfo);

                u.gather = !visualCentered;

                su.gather = !visualCentered;
            }
            else
            {
                u = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize * 1.25f,
                Owner.Center, verS.heatMap, flip, Rotation, KValue * 1.5f, pair?.Item1 ?? 9, pair?.Item2 ?? 0, colorVec: verS.colorVec);
                u.gather = !visualCentered;
            }
            //Main.NewText(Owner.Center);
            //var u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize, Owner.Center, verS.heatMap, this.flip, Rotation, KValue, (.625f, -.75f), colorVec: verS.colorVec);
            if (verS.renderInfos == null)
                u.ResetAllRenderInfo();
            else
            {
                u.ModityAllRenderInfo(verS.renderInfos);
            }
            u.ApplyStdValueToVtxEffect(standardInfo);
            return u;
        }
        return null;
    }
    #endregion

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

        flip ^= true;
    }
    public override void OnEndSingle()
    {
        hitCounter = 0;
        base.OnEndSingle();
    }
    public override void OnStartAttack()
    {
        SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.SwooshNormal_1, Owner?.Center);
        if (Owner is Player plr)
        {
            SequencePlayer seqPlr = plr.GetModPlayer<SequencePlayer>();
            int dmg = CurrentDamage;
            if (standardInfo.standardShotCooldown > 0)
            {
                float delta = standardInfo.standardTimer * ModifyData.actionOffsetTimeScaler / Cycle;
                seqPlr.cachedTime += delta + 1;
                int count = (int)(seqPlr.cachedTime / standardInfo.standardShotCooldown);
                seqPlr.cachedTime -= count * standardInfo.standardShotCooldown;
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
            if (Main.rand.NextFloat(0, 1) < standardInfo.dustAmount)
                for (int k = 0; k < 2; k++)
                {
                    var flag = k == 0;
                    var unit = ((MathHelper.TwoPi / 30 * n).ToRotationVector2() * new Vector2(1, .75f)).RotatedBy(Rotation) * (flag ? 2 : 1) * .5f;
                    var Center = Owner.Center + offsetCenter + targetedVector * .75f;
                    var velocity = unit - targetedVector * .125f;//-Owner.velocity * 2 + 
                    velocity *= 2;
                    OtherMethods.FastDust(Center, velocity, standardInfo.standardColor);

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
    #endregion
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
    #endregion

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

    #endregion

    #region 重写属性
    [ElementCustomDataAbabdoned]
    public override int Cycle { get => realCycle; set => givenCycle = value; }
    public int realCycle;


    #endregion

    #region 辅助函数
    void ResetCycle()
    {
        realCycle = Math.Clamp(rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);

        //if (Projectile.owner == Main.myPlayer)
        //{
        //    realCycle = rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : Math.Clamp(givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);
        //    Projectile.netImportant = true;
        //}

    }
    #endregion

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
    #endregion
}
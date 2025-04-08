using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.ComponentModel;
using System.IO;
using Terraria.Audio;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;
public class SwooshInfo : LSLMelee
{
    #region 类中类
    public enum SwooshMode
    {
        Chop,
        Slash,
        //Storm // TODO:完成旋风斩
    }
    #endregion

    #region 常数
    //int cutTime => 8;
    //float k => 0.25f;
    const int cutTime = 8;
    const float k = 0.25f;

    #endregion

    #region 辅助字段
    int hitCounter;
    UltraSwoosh swoosh;
    UltraSwoosh subSwoosh;
    #endregion

    #region 参数字段
    [ElementCustomData]
    [DrawTicks]
    public SwooshMode mode;

    [DefaultValue(1f)]
    [Range(0.2f, 5f)]
    [Increment(.2f)]
    [ElementCustomData]
    public float minKValue = 1f;

    [DefaultValue(1f)]
    [Range(0f, 5f)]
    [Increment(.2f)]
    [ElementCustomData]
    public float KValueRange = 1f;

    [DefaultValue(MathHelper.Pi / 6)]
    [ElementCustomData]
    [Range(0, MathHelper.PiOver2)]
    [Increment(MathHelper.Pi / 24)]
    public float randAngleRange = MathHelper.Pi / 6;
    #endregion

    #region 重写属性
    public override float offsetRotation => TimeToAngle(fTimer);
    public override float offsetDamage => MathF.Pow(.75f, hitCounter);
    public override bool Attacktive
    {
        get
        {
            float t = (timerMax - cutTime) * k;
            return fTimer > t && fTimer < t + cutTime;
        }
    }
    #endregion

    #region 辅助函数
    float TimeToAngle(float t)
    {
        float max = timerMax;
        var fac = t / max;
        if (max > cutTime * 1.5f)
        {
            float tier2 = (max - cutTime) * k;
            float tier1 = tier2 + cutTime;
            if (t > tier1)
                fac = MathHelper.SmoothStep(mode == SwooshMode.Chop ? 160 / 99f : 1, 1.125f, Utils.GetLerpValue(max, tier1, t, true));
            else if (t < tier2)
                fac = 0;
            else
                fac = MathHelper.SmoothStep(0, 1.125f, Utils.GetLerpValue(tier2, tier1, t, true));

        }
        else
            fac = MathHelper.SmoothStep(-.125f, 1.25f, fac);

        fac = flip ? 1 - fac : fac;
        float start = -.75f;
        float end = .625f;
        return MathHelper.Lerp(end, start, fac) * MathHelper.Pi;
    }
    public void NewSwoosh()
    {
        var verS = standardInfo.vertexStandard;
        if (verS.active)
        {
            swoosh = subSwoosh = null;
            var range = mode switch
            {
                SwooshMode.Chop => (.875f, -1f),
                SwooshMode.Slash => (.625f, -.75f),
                //SwooshMode.Storm or _ => (.625f, -.75f)
            };
            bool f = mode switch
            {
                SwooshMode.Chop => !flip,
                _ => flip
            };
            float size = verS.scaler * ModifyData.actionOffsetSize * offsetSize;
            var pair = standardInfo.vertexStandard.swooshTexIndex;
            UltraSwoosh u;
            if (standardInfo.itemType == ItemID.TrueExcalibur)
            {
                var eVec = verS.colorVec with { Y = 0 };
                if (eVec.X == 0 && eVec.Z == 0)
                    eVec = new(.5f, 0, .5f);
                u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size * .67f, Owner.Center, verS.heatMap, f, Rotation, KValue, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                subSwoosh = UltraSwoosh.NewUltraSwoosh(Color.Pink, (int)(verS.timeLeft * 1.2f), size, Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, f, Rotation, KValue, (range.Item1 + 0.125f, range.Item2 - 0.125f), pair?.Item1 ?? 3, pair?.Item2 ?? 7, eVec);
                subSwoosh.ApplyStdValueToVtxEffect(standardInfo);
                subSwoosh.heatRotation = 0;
            }
            else
            {
                u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size, Owner.Center, verS.heatMap, f, Rotation, KValue, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
            }
            swoosh = u;
            u.ApplyStdValueToVtxEffect(standardInfo);
        }
        //return null;
    }
    void UpdateSwoosh(UltraSwoosh swoosh, (float, float) range)
    {
        if (swoosh == null)
            return;
        swoosh.center = Owner.Center;
        swoosh.rotation = Rotation;
        swoosh.negativeDir = flip;
        swoosh.angleRange = range;
        if (flip)
            swoosh.angleRange = (swoosh.angleRange.from, -swoosh.angleRange.to);
        swoosh.timeLeft = (int)(MathHelper.Clamp(MathF.Abs(swoosh.angleRange.Item1 - swoosh.angleRange.Item2), 0, 1) * swoosh.timeLeftMax) + 1;
    }
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        if (timer > (timerMax - cutTime) * k)
        {
            timer--;
            UpdateSwoosh(swoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : -1.25f + .5f * Factor, offsetRotation / MathF.PI));
            UpdateSwoosh(subSwoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : -1.25f + .5f * Factor, offsetRotation / MathF.PI));
            timer++;
        }
        else
        {
            swoosh = null;
            subSwoosh = null;

        }
        base.Update(triggered);
    }

    public override void OnActive()
    {
        flip = Owner.direction == -1;
        base.OnActive();
    }
    public override void OnDeactive()
    {
        if (mode == SwooshMode.Slash)
            flip ^= true;
        base.OnDeactive();
    }

    public override void OnStartSingle()
    {
        base.OnStartSingle();

        hitCounter = 0;
        if (randAngleRange > 0)
            Rotation += Main.rand.NextFloat(0, randAngleRange) * Main.rand.Next([-1, 1]);
        KValue = minKValue + Main.rand.NextFloat(0, KValueRange);
        if (mode == SwooshMode.Slash)
            flip ^= true;
        NewSwoosh();
    }
    public override void OnEndSingle()
    {
        hitCounter = 0;
        base.OnEndSingle();
    }

    public override void OnStartAttack()
    {
        SoundEngine.PlaySound((standardInfo.soundStyle ?? MySoundID.Scythe) with { MaxInstances = -1 }, Owner?.Center);
        if (Owner is Player plr)
        {
            SequencePlayer seqPlayer = plr.GetModPlayer<SequencePlayer>();

            int dmg = CurrentDamage;
            if (standardInfo.standardShotCooldown > 0)
            {
                float delta = standardInfo.standardTimer * ModifyData.actionOffsetTimeScaler / Cycle;
                bool canShoot = plr.HeldItem.shoot > ProjectileID.None;

                float m = Math.Max(standardInfo.standardShotCooldown, delta);
                if (canShoot || seqPlayer.cachedTime < m)
                    seqPlayer.cachedTime += delta + 1;
                if (seqPlayer.cachedTime > m)
                    seqPlayer.cachedTime = m;
                int count = (int)(seqPlayer.cachedTime / standardInfo.standardShotCooldown);
                if (canShoot)
                {
                    seqPlayer.cachedTime -= standardInfo.standardShotCooldown * count;
                    if (count > 0)
                    {
                        count--;
                        ShootProjCall(plr, dmg);
                    }
                }

                Vector2 orig = Main.MouseWorld;
                Vector2 unit = orig - plr.Center;//.SafeNormalize(default) * 32f;
                float angleMax = MathHelper.Pi / 6;
                if (count % 2 == 1)
                {
                    count--;
                    Vector2 target = plr.Center + unit.RotatedBy(angleMax * Main.rand.NextFloat(-.5f, .5f));
                    Main.mouseX = (int)(target.X - Main.screenPosition.X);
                    Main.mouseY = (int)(target.Y - Main.screenPosition.Y);
                    ShootProjCall(plr, dmg);

                }
                count /= 2;
                for (int i = 0; i < count; i++)
                {
                    float angle = angleMax * MathF.Pow((i + 1f) / count, 2);

                    Vector2 target = plr.Center + unit.RotatedBy(angle);
                    Main.mouseX = (int)(target.X - Main.screenPosition.X);
                    Main.mouseY = (int)(target.Y - Main.screenPosition.Y);
                    ShootProjCall(plr, dmg);


                    target = plr.Center + unit.RotatedBy(-angle);
                    Main.mouseX = (int)(target.X - Main.screenPosition.X);
                    Main.mouseY = (int)(target.Y - Main.screenPosition.Y);
                    ShootProjCall(plr, dmg);

                }
                Main.mouseX = (int)(orig.X - Main.screenPosition.X);
                Main.mouseY = (int)(orig.Y - Main.screenPosition.Y);
            }
            else
                ShootProjCall(plr, dmg);
        }
        base.OnStartAttack();
    }
    public override void OnAttack()
    {
        for (int n = 0; n < 30 * (1 - Factor) * standardInfo.dustAmount; n++)
        {
            var Center = Owner.Center + offsetCenter + targetedVector * Main.rand.NextFloat(0.5f, 1f);//
            var velocity = -Owner.velocity * 2 + targetedVector.RotatedBy(MathHelper.PiOver2 * (flip ? -1 : 1) + Main.rand.NextFloat(-MathHelper.Pi / 12, MathHelper.Pi / 12)) * Main.rand.NextFloat(.125f, .25f);
            OtherMethods.FastDust(Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), velocity * .25f, standardInfo.standardColor);
        }
        base.OnAttack();
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

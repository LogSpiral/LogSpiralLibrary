using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;
public class CutInfo : LSLMelee
{
    #region 辅助字段
    int hitCounter;
    UltraSwoosh swoosh;
    UltraSwoosh subSwoosh;
    #endregion

    #region 辅助属性
    bool Adjusted => base.Factor < .75f;
    #endregion

    #region 重写属性
    public override float Factor => Adjusted ? MathHelper.SmoothStep(0, 1, MathHelper.SmoothStep(0, 1, 1 - MathF.Pow(1 - base.Factor / .75f, 2))) : (base.Factor - .75f) / .25f;
    public override float offsetRotation => (Adjusted ? MathHelper.SmoothStep(.03125f * MathHelper.Pi, -0.0625f * MathHelper.Pi, Factor) : MathHelper.SmoothStep(-0.0625f * MathHelper.Pi, .625f * MathHelper.Pi, Factor)) * Owner.direction;
    public override Vector2 offsetCenter => base.offsetCenter - (Adjusted ? Rotation.ToRotationVector2() * (1 - Factor) * 16 : default);
    public override Vector2 offsetOrigin => base.offsetOrigin + (Adjusted ? new Vector2(0.1f, -0.1f) * (1 - Factor) : default);
    public override float offsetDamage => base.offsetDamage * MathF.Pow(.75f, hitCounter);
    public override bool Attacktive => base.Factor < .5f;
    #endregion

    #region 辅助函数
    public void NewSwoosh()
    {
        var verS = standardInfo.vertexStandard;
        if (verS.active)
        {
            swoosh = subSwoosh = null;
            var range = (.625f, -.75f);
            bool f = false;
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
            if (verS.renderInfos == null)
                u.ResetAllRenderInfo();
            else
            {
                u.ModityAllRenderInfo(verS.renderInfos);
            }
            swoosh = u;
            u.ApplyStdValueToVtxEffect(standardInfo);
        }
    }
    void UpdateSwoosh(UltraSwoosh swoosh, (float, float) range)
    {
        if (swoosh == null)
            return;
        swoosh.center = Owner.Center;
        swoosh.rotation = Rotation;
        swoosh.negativeDir = flip;
        swoosh.angleRange = range;
        swoosh.xScaler = 2f;
        swoosh.timeLeft = (int)MathHelper.Clamp(swoosh.timeLeftMax * MathHelper.SmoothStep(0, 1, Utils.GetLerpValue(1, 0.5f, Factor, true)), 1, swoosh.timeLeftMax - 1) + 1;
    }
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        flip = Owner.direction == -1;
        if (Adjusted)
        {
            if (swoosh == null)
                NewSwoosh();

            timer--;
            UpdateSwoosh(swoosh, (-0.5f, MathHelper.Lerp(.75f, -.25f, Factor)));
            UpdateSwoosh(subSwoosh, (-0.5f, MathHelper.Lerp(.75f, -.25f, Factor)));
            timer++;
        }
        else
        {
            swoosh = subSwoosh = null;
        }
        base.Update(triggered);
    }

    public override void OnStartSingle()
    {
        hitCounter = 0;
        base.OnStartSingle();
    }
    public override void OnEndSingle()
    {
        hitCounter = 0;
        base.OnEndSingle();
    }

    public override void OnCharge()
    {
        flip = Owner.direction == -1;
        if (!Adjusted)
            KValue = 2f - MathF.Cos(MathHelper.TwoPi * Factor);
        base.OnCharge();
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
        if (Main.rand.NextFloat(0, 1) < standardInfo.dustAmount)
            for (int k = 0; k < 2; k++)
            {
                var flag = k == 0;
                var unit = Rotation.ToRotationVector2();//((MathHelper.TwoPi / 30 * n).ToRotationVector2() * new Vector2(1, .75f)).RotatedBy(Rotation) * (flag ? 2 : 1) * .5f;
                var Center = Owner.Center + offsetCenter + targetedVector * .75f;
                var velocity = unit - targetedVector * .125f;//-Owner.velocity * 2 + 
                velocity *= 2;
                MiscMethods.FastDust(Center, velocity, standardInfo.standardColor);
            }
        base.OnAttack();
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        hitCounter++;
        base.OnHitEntity(victim, damageDone, context);
    }
    #endregion
}

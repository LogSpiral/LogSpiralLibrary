using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;

public class CutInfo : LSLMelee
{
    #region 辅助字段

    private int hitCounter;
    private UltraSwoosh swoosh;
    private UltraSwoosh subSwoosh;

    #endregion 辅助字段

    #region 辅助属性

    private bool Adjusted => base.Factor < .75f;

    #endregion 辅助属性

    #region 重写属性

    public override float Factor => Adjusted ? MathHelper.SmoothStep(0, 1, MathHelper.SmoothStep(0, 1, 1 - MathF.Pow(1 - base.Factor / .75f, 2))) : (base.Factor - .75f) / .25f;
    public override float OffsetRotation => (Adjusted ? MathHelper.SmoothStep(.03125f * MathHelper.Pi, -0.0625f * MathHelper.Pi, Factor) : MathHelper.SmoothStep(-0.0625f * MathHelper.Pi, .625f * MathHelper.Pi, Factor)) * Owner.direction;
    public override Vector2 OffsetCenter => base.OffsetCenter - (Adjusted ? Rotation.ToRotationVector2() * (1 - Factor) * 16 : default);
    public override Vector2 OffsetOrigin => base.OffsetOrigin + (Adjusted ? new Vector2(0.1f, -0.1f) * (1 - Factor) : default);
    public override float OffsetDamage => base.OffsetDamage * MathF.Pow(.75f, hitCounter);
    public override bool Attacktive => base.Factor < .5f;

    #endregion 重写属性

    #region 辅助函数

    public void NewSwoosh()
    {
        var verS = StandardInfo.VertexStandard;
        if (verS.active && !Main.dedServ)
        {
            swoosh = subSwoosh = null;
            var range = (.625f, -.75f);
            bool f = false;
            float size = verS.scaler * ModifyData.Size * OffsetSize;
            var pair = StandardInfo.VertexStandard.swooshTexIndex;
            UltraSwoosh u;
            if (StandardInfo.itemType == ItemID.TrueExcalibur)
            {
                var eVec = verS.colorVec with { Y = 0 };
                if (eVec.X == 0 && eVec.Z == 0)
                    eVec = new(.5f, 0, .5f);

                u = UltraSwoosh.NewUltraSwoosh(verS.canvasName, verS.timeLeft, size * .67f, Owner.Center, range);
                u.heatMap = verS.heatMap;
                u.negativeDir = f;
                u.xScaler = KValue;
                u.rotation = Rotation;
                u.aniTexIndex = pair?.Item1 ?? 3;
                u.baseTexIndex = pair?.Item2 ?? 7;
                u.ColorVector = verS.colorVec;

                var su = subSwoosh = UltraSwoosh.NewUltraSwoosh(verS.canvasName, (int)(verS.timeLeft * 1.2f), size, Owner.Center, (range.Item1 + 0.125f, range.Item2 - 0.125f));
                su.heatMap = LogSpiralLibraryMod.HeatMap[5].Value;
                su.xScaler = KValue;
                su.rotation = Rotation;
                su.aniTexIndex = pair?.Item1 ?? 3;
                su.baseTexIndex = pair?.Item2 ?? 7;
                su.ColorVector = eVec;
                su.ApplyStdValueToVtxEffect(StandardInfo);
                su.heatRotation = 0;
            }
            else
            {
                u = UltraSwoosh.NewUltraSwoosh(verS.canvasName, verS.timeLeft, size, Owner.Center, range);
                u.heatMap = verS.heatMap;
                u.negativeDir = f;
                u.xScaler = KValue;
                u.rotation = Rotation;
                u.aniTexIndex = pair?.Item1 ?? 3;
                u.baseTexIndex = pair?.Item2 ?? 7;
                u.ColorVector = verS.colorVec;
            }
            swoosh = u;
            u.ApplyStdValueToVtxEffect(StandardInfo);
        }
    }

    private void UpdateSwoosh(UltraSwoosh swoosh, (float, float) range)
    {
        if (swoosh == null)
            return;
        swoosh.center = Owner.Center;
        swoosh.rotation = Rotation;
        swoosh.negativeDir = Flip;
        swoosh.angleRange = range;
        swoosh.xScaler = 2f;
        swoosh.timeLeft = (int)MathHelper.Clamp(swoosh.timeLeftMax * MathHelper.SmoothStep(0, 1, Utils.GetLerpValue(1, 0.5f, Factor, true)), 1, swoosh.timeLeftMax - 1) + 1;
    }

    #endregion 辅助函数

    #region 重写函数

    public override void UpdateStatus(bool triggered)
    {
        Flip = Owner.direction == -1;
        if (Adjusted)
        {
            if (swoosh == null)
                NewSwoosh();

            Timer--;
            UpdateSwoosh(swoosh, (-0.5f, MathHelper.Lerp(.75f, -.25f, Factor)));
            UpdateSwoosh(subSwoosh, (-0.5f, MathHelper.Lerp(.75f, -.25f, Factor)));
            Timer++;
        }
        else
        {
            swoosh = subSwoosh = null;
        }
        base.UpdateStatus(triggered);
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
        Flip = Owner.direction == -1;
        if (!Adjusted)
            KValue = 2f - MathF.Cos(MathHelper.TwoPi * Factor);
        base.OnCharge();
    }

    public override void OnStartAttack()
    {
        SoundEngine.PlaySound((StandardInfo.soundStyle ?? MySoundID.Scythe) with { MaxInstances = -1 }, Owner?.Center);
        base.OnStartAttack();
    }

    public override void OnAttack()
    {
        if (Main.rand.NextFloat(0, 1) < StandardInfo.dustAmount)
            for (int k = 0; k < 2; k++)
            {
                var flag = k == 0;
                var unit = Rotation.ToRotationVector2();//((MathHelper.TwoPi / 30 * n).ToRotationVector2() * new Vector2(1, .75f)).RotatedBy(Rotation) * (flag ? 2 : 1) * .5f;
                var Center = Owner.Center + OffsetCenter + targetedVector * .75f;
                var velocity = unit - targetedVector * .125f;//-Owner.velocity * 2 +
                velocity *= 2;
                MiscMethods.FastDust(Center, velocity, StandardInfo.standardColor);
            }
        base.OnAttack();
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        hitCounter++;
        base.OnHitEntity(victim, damageDone, context);
    }

    #endregion 重写函数
}
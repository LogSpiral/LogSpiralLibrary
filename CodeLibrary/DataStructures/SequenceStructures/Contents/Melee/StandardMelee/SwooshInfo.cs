using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.ComponentModel;
using System.Diagnostics;
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

    #endregion 类中类

    #region 常数

    //int cutTime => 8;
    //float k => 0.25f;
    // private const int CutTime = 8;

    // private const float AttackStartupRecoverRatio = 0.25f;

    #endregion 常数

    #region 辅助字段

    private int hitCounter;
    private UltraSwoosh swoosh;
    private UltraSwoosh subSwoosh;

    #endregion 辅助字段

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

    [DefaultValue(false)]
    [ElementCustomData]
    public bool enableNailBouncingMode = false;

    [Range(4,12)]
    [DefaultValue(4)]
    [ElementCustomData]
    public int CutTime = 4;

    [Range(0.05f, 0.95f)]
    [DefaultValue(0.8f)]
    [Increment(0.05f)]
    [ElementCustomData]
    public float AttackStartupRecoverRatio = 0.8f;//0.25f;
    #endregion 参数字段

    #region 重写属性

    public override float OffsetRotation => TimeToAngle(fTimer);
    public override float OffsetDamage => MathF.Pow(.75f, hitCounter);

    public override bool Attacktive
    {
        get
        {
            float t = (TimerMax - CutTime) * AttackStartupRecoverRatio;
            return fTimer > t && fTimer < t + CutTime;
        }
    }

    #endregion 重写属性

    #region 辅助函数

    private float GetCutFactor(float t)
    {
        float max = TimerMax;
        var fac = t / max;
        if (max > CutTime * 1.5f)
        {
            float tier2 = (max - CutTime) * AttackStartupRecoverRatio;
            float tier1 = tier2 + CutTime;
            if (t > tier1)
                fac = MathHelper.SmoothStep(mode == SwooshMode.Chop ? 160 / 99f : 1, 1.125f, Utils.GetLerpValue(max, tier1, t, true));
            else if (t < tier2)
                fac = 0;
            else
                fac = MathHelper.SmoothStep(0, 1.125f, Utils.GetLerpValue(tier2, tier1, t, true));
        }
        else
            fac = MathHelper.SmoothStep(-.125f, 1.25f, fac);
        return fac;
    }

    private float TimeToAngle(float t)
    {
        var fac = GetCutFactor(t);

        fac = Flip ? 1 - fac : fac;
        float start = -.75f;
        float end = .625f;
        return MathHelper.Lerp(end, start, fac) * MathHelper.Pi;
    }

    public void NewSwoosh()
    {
        var verS = StandardInfo.VertexStandard;
        if (verS.active && !Main.dedServ)
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
                SwooshMode.Chop => !Flip,
                _ => Flip
            };
            float size = verS.scaler * ModifyData.Size * OffsetSize;
            var pair = StandardInfo.VertexStandard.swooshTexIndex;
            UltraSwoosh u;
            if (StandardInfo.itemType == ItemID.TrueExcalibur)
            {
                var eVec = verS.colorVec with { Y = 0 };
                if (eVec.X == 0 && eVec.Z == 0)
                    eVec = new Vector3(.5f, 0, .5f);

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
        //return null;
    }

    private void UpdateSwoosh(UltraSwoosh swoosh, (float, float) range)
    {
        if (swoosh == null)
            return;
        swoosh.center = Owner.Center;
        swoosh.rotation = Rotation;
        swoosh.negativeDir = Flip;
        swoosh.angleRange = range;
        if (Flip)
            swoosh.angleRange = (swoosh.angleRange.from, -swoosh.angleRange.to);
        swoosh.timeLeft = (int)(MathHelper.Clamp(MathF.Abs(swoosh.angleRange.from - swoosh.angleRange.to), 0, 1) * swoosh.timeLeftMax) + 1;
        if (swoosh.timeLeft < 2)
            swoosh.timeLeft = 2;
    }

    #endregion 辅助函数

    #region 重写函数

    public override void UpdateStatus(bool triggered)
    {
        if (Timer > (TimerMax - CutTime) * AttackStartupRecoverRatio)
        {
            Timer--;
            UpdateSwoosh(swoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : -1.25f + .5f * Factor, OffsetRotation / MathF.PI));
            UpdateSwoosh(subSwoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : -1.25f + .5f * Factor, OffsetRotation / MathF.PI));
            Timer++;
        }
        else
        {
            swoosh = null;
            subSwoosh = null;
        }
        base.UpdateStatus(triggered);
    }

    public override void OnActive()
    {
        Flip = Owner.direction == -1;
        base.OnActive();
    }

    public override void OnDeactive()
    {
        if (mode == SwooshMode.Slash)
            Flip ^= true;
        base.OnDeactive();
    }

    public override void OnStartSingle()
    {
        base.OnStartSingle();
        if (IsLocalProjectile)
        {
            hitCounter = 0;
            if (randAngleRange > 0)
                Rotation += Main.rand.NextFloat(-randAngleRange, randAngleRange);
            KValue = minKValue + Main.rand.NextFloat(0, KValueRange);
            if (mode == SwooshMode.Slash)
                Flip ^= true;
        }
        if (!Main.dedServ)
            NewSwoosh();
    }

    public override void OnEndSingle()
    {
        hitCounter = 0;
        base.OnEndSingle();
    }

    private CollisionHelper.EllipticSector _ellipticSector;

    public override void OnStartAttack()
    {
        float size = StandardInfo.VertexStandard.scaler * ModifyData.Size * OffsetSize;
        _ellipticSector = new CollisionHelper.EllipticSector(Owner.Center, size, size / KValue, Rotation, -MathHelper.PiOver4 * 3, MathHelper.PiOver2 * 3);
        SoundEngine.PlaySound((StandardInfo.soundStyle ?? MySoundID.Scythe) with { MaxInstances = -1 }, Owner?.Center);
        base.OnStartAttack();
    }

    public override void OnAttack()
    {
        _ellipticSector.Center = Owner.Center;
        if (Main.dedServ)
            goto label;
        int amount = (int)(30 * (1 - Factor) * StandardInfo.dustAmount);
        if (amount > 0)
        {
            float timerOrig = fTimer;

            Vector2 lastTarget = default;
            for (int n = 0; n < amount; n++)
            {
                float f = n / (float)amount;
                fTimer = timerOrig - f;


                Vector2 finalOrigin = OffsetOrigin + StandardInfo.standardOrigin;
                float finalRotation = OffsetRotation + StandardInfo.standardRotation;
                Vector2 drawCen = OffsetCenter + Owner.Center;

                float k = 1f;
                if (StandardInfo.VertexStandard.scaler > 0 && TextureAssets.Item[Main.LocalPlayer.HeldItem.type] is { } textureAsset)
                    k = StandardInfo.VertexStandard.scaler / textureAsset.Value.Size().Length();
                float sc = 1;
                if (Owner is Player plr)
                    sc = plr.GetAdjustedItemScale(plr.HeldItem);
                CustomVertexInfo[] c = DrawingMethods.GetItemVertexes(finalOrigin, StandardInfo.standardRotation, OffsetRotation, Rotation, TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value, KValue, OffsetSize * ModifyData.Size * sc * k, drawCen, !Flip);
                var targetedVector = c[4].Position - (OffsetCenter + Owner.Center);

                if (lastTarget == default)
                {
                    lastTarget = targetedVector;
                    continue;
                }

                var Center = c[4].Position;
                var velocity = (lastTarget - targetedVector) * Main.rand.NextFloat(.15f, 1f) * -12 * StandardInfo.dustAmount;
                var dust = MiscMethods.FastDust(Center, Owner.velocity - velocity * .25f, StandardInfo.standardColor);
                dust.scale *= MathF.Pow(1 - Factor, 2);
                lastTarget = targetedVector;
            }
            fTimer = timerOrig;
        }
    label:
        base.OnAttack();
    }

    public override bool Collide(Rectangle rectangle)
    {
        var sector = _ellipticSector;

        return
            CollisionHelper.CheckEllipticSectorAndRectangle(
            _ellipticSector,
            CollisionHelper.OrthogonalRectangle.FromRectangle(rectangle));
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        hitCounter++;
        base.OnHitEntity(victim, damageDone, context);

        if (!enableNailBouncingMode) return;

        if (Owner is Player plr)
        {
            if (MathF.Abs(Rotation - MathHelper.PiOver2 * plr.gravDir) < MathHelper.PiOver4)
                Owner.velocity = -Vector2.UnitY * 8 * plr.gravDir;

            if (Main.netMode == NetmodeID.MultiplayerClient && plr.whoAmI == Main.myPlayer)
                SyncPlayerVelocity.Get(plr.whoAmI, plr.velocity).Send(-1, plr.whoAmI);

        }


    }

    public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {

        float t = fTimer;
        float max = TimerMax;
        var fac = t / max;
        if (max > CutTime * 1.5f)
        {
            float tier2 = (max - CutTime) * AttackStartupRecoverRatio;
            float tier1 = tier2 + CutTime;
            if (t > tier1)
                fac = 1;
            else if (t < tier2)
                fac = 0;
            else
                fac = MathHelper.SmoothStep(0, 1f, Utils.GetLerpValue(tier2, tier1, t, true));
        }
        else
            fac = MathHelper.SmoothStep(0f, 1f, fac);

        return base.GetWeaponVertex(texture, 1 - 4 * fac * (1 - fac));
    }

    public override void NetSendInitializeElement(BinaryWriter writer)
    {
        base.NetSendInitializeElement(writer);
        writer.Write((byte)mode);
        writer.Write(minKValue);
        writer.Write(KValueRange);
        writer.Write(randAngleRange);
    }
    public override void NetReceiveInitializeElement(BinaryReader reader)
    {
        base.NetReceiveInitializeElement(reader);
        mode = (SwooshMode)reader.ReadByte();
        minKValue = reader.ReadSingle();
        KValueRange = reader.ReadSingle();
        randAngleRange = reader.ReadSingle();
    }

    #endregion 重写函数
}
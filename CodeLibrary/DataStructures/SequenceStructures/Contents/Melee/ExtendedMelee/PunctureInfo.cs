using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.ExtendedMelee;

public class PunctureInfo : ExtendedMelee
{
    #region 辅助字段

    private float deltaH = 0f;

    #endregion 辅助字段

    #region 重写属性

    public override float offsetRotation => MathHelper.SmoothStep(0, MathHelper.PiOver2 - (Rotation < -MathHelper.PiOver2 ? Rotation + MathHelper.TwoPi : Rotation), MathF.Pow(MathHelper.Clamp((1 - Factor) * 2, 0, 1), 2));
    public override float CompositeArmRotation => base.CompositeArmRotation + MathHelper.SmoothStep(0, -MathHelper.PiOver2 * .75f * Owner.direction, MathF.Pow(MathHelper.Clamp((1 - Factor) * 3, 0, 1), 2));
    public override Vector2 offsetOrigin => base.offsetOrigin + Vector2.SmoothStep(default, new Vector2(-.25f, .05f).RotatedBy(StandardInfo.standardRotation + MathHelper.PiOver4), MathHelper.Clamp((1 - Factor) * 2, 0, 1));
    public override bool Attacktive => Factor < .85f;

    #endregion 重写属性

    #region 重写函数

    public virtual void OnBurst(float fallFac)
    {
        for (int n = 0; n < 15 / fallFac; n++)
            MiscMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 32), StandardInfo.standardColor, 2);

        for (int n = 0; n < 30 / fallFac; n++)
            MiscMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4) - Vector2.UnitY * 40 * Main.rand.NextFloat(0, 1) / fallFac * .5f, StandardInfo.standardColor, 2);

        var verS = StandardInfo.VertexStandard;
        if (verS.active)
        {
            var pair = StandardInfo.VertexStandard.stabTexIndex;
            var scaler = verS.scaler * ModifyData.Size * offsetSize * 1.25f * 4f / fallFac;
            var center = Owner.Center - Vector2.UnitY * StandardInfo.VertexStandard.scaler / fallFac * 2f;
            UltraStab u = UltraStab.NewUltraStab(verS.canvasName, verS.timeLeft, scaler, center);
            u.heatMap = verS.heatMap;
            u.negativeDir = Flip;
            u.rotation = MathHelper.PiOver2;
            u.xScaler = 2 / fallFac;
            u.aniTexIndex = pair?.Item1 ?? 9;
            u.baseTexIndex = pair?.Item2 ?? 0;
            u.ColorVector = verS.colorVec;
            u.ApplyStdValueToVtxEffect(StandardInfo);
        }

        SoundEngine.PlaySound(SoundID.Item92);
    }

    public override void Update(bool triggered)
    {
        Flip = Owner.direction == 1;

        float fallFac = MathHelper.Lerp(.5f, .33f, MathHelper.Clamp(deltaH / 320f, 0, 1));
        if (Factor > fallFac)
        {
            Owner.velocity += Vector2.UnitY * 32f * (0.85f - Factor) * (deltaH / 800f + 1);
            if (Owner is Player plr)
                plr.GetModPlayer<LogSpiralLibraryPlayer>().ultraFallEnable = true;
        }

        if (Timer == (int)(TimerMax * fallFac))
            OnBurst(fallFac);

        base.Update(triggered);
    }

    public override void OnStartSingle()
    {
        int t = 0;
        Point point = Owner.Bottom.ToTileCoordinates();
        while (point.Y + t < Main.maxTilesY && t < 100 && !Framing.GetTileSafely(point.X, point.Y + t).HasTile)
        {
            t++;
        }
        deltaH = 16 * t;
        base.OnStartSingle();
    }

    #endregion 重写函数
}
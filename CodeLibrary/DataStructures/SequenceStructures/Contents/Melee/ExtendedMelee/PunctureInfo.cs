using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using Terraria.Audio;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.ExtendedMelee;

public class PunctureInfo : ExtendedMelee
{
    #region 辅助字段
    float deltaH = 0f;
    #endregion

    #region 重写属性
    public override float offsetRotation => MathHelper.SmoothStep(0, MathHelper.PiOver2 - (Rotation < -MathHelper.PiOver2 ? Rotation + MathHelper.TwoPi : Rotation), MathF.Pow(MathHelper.Clamp((1 - Factor) * 2, 0, 1), 2));
    public override float CompositeArmRotation => base.CompositeArmRotation + MathHelper.SmoothStep(0, -MathHelper.PiOver2 * .75f * Owner.direction, MathF.Pow(MathHelper.Clamp((1 - Factor) * 3, 0, 1), 2));
    public override Vector2 offsetOrigin => base.offsetOrigin + Vector2.SmoothStep(default, new Vector2(-.25f, .05f).RotatedBy(standardInfo.standardRotation + MathHelper.PiOver4), MathHelper.Clamp((1 - Factor) * 2, 0, 1));
    public override bool Attacktive => Factor < .85f;
    #endregion

    #region 重写函数
    public virtual void OnBurst(float fallFac)
    {
        for (int n = 0; n < 15 / fallFac; n++)
            OtherMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 32), standardInfo.standardColor, 2);

        for (int n = 0; n < 30 / fallFac; n++)
            OtherMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4) - Vector2.UnitY * 40 * Main.rand.NextFloat(0, 1) / fallFac * .5f, standardInfo.standardColor, 2);

            
        var verS = standardInfo.vertexStandard;
        if (verS.active)
        {
            UltraStab u = null;
            var pair = standardInfo.vertexStandard.stabTexIndex;
            u = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize * 1.25f * 4f / fallFac,
            Owner.Center - Vector2.UnitY * standardInfo.vertexStandard.scaler / fallFac * 2f, verS.heatMap, flip, MathHelper.PiOver2, 2 / fallFac, pair?.Item1 ?? 9, pair?.Item2 ?? 0, colorVec: verS.colorVec);
            u.ApplyStdValueToVtxEffect(standardInfo);
        }

        SoundEngine.PlaySound(SoundID.Item92);
    }
    public override void Update(bool triggered)
    {
        flip = Owner.direction == 1;

        float fallFac = MathHelper.Lerp(.5f, .33f, MathHelper.Clamp(deltaH / 320f, 0, 1));
        if (Factor > fallFac)
        {
            Owner.velocity += Vector2.UnitY * 32f * (0.85f - Factor) * (deltaH / 800f + 1);
            if (Owner is Player plr)
                plr.GetModPlayer<LogSpiralLibraryPlayer>().ultraFallEnable = true;
        }


        if (timer == (int)(timerMax * fallFac))
            OnBurst(fallFac);


        base.Update(triggered);
    }
    public override void OnStartSingle()
    {
        int t = 0;
        Point point = Owner.Bottom.ToTileCoordinates();
        while (point.Y + t < Main.maxTilesY && t < 100 && !Main.tile[point.X, point.Y + t].HasTile)
        {
            t++;
        }
        deltaH = 16 * t;
        base.OnStartSingle();
    }
    #endregion
}

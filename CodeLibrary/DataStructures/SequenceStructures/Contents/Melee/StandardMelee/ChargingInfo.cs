using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using Terraria.Audio;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;
public class ChargingInfo : LSLMelee
{
    #region 参数字段
    [ElementCustomData]
    [Range(-MathF.PI, MathF.PI)]
    [Increment(0.1f)]
    public float ChargingRotation = 0;

    [ElementCustomData]
    [Range(-MathF.PI, MathF.PI)]
    [Increment(0.1f)]
    public float StartRotation = 0;

    [ElementCustomData]
    public bool AutoNext = false;
    #endregion

    #region 重写属性
    public override float offsetRotation => Main.rand.NextFloat(-1, 1) * Main.rand.NextFloat(0, 1) * Factor * .5f + MathHelper.Lerp(StartRotation, ChargingRotation, MathHelper.SmoothStep(1, 0, MathF.Pow(Factor, 3))) * Owner.direction;
    public override float CompositeArmRotation => Main.GlobalTimeWrappedHourly;//base.CompositeArmRotation;
    public override float offsetSize => base.offsetSize;
    public override bool Attacktive => timer == 1;
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        standardInfo = standardInfo with { extraLight = 3 * MathF.Pow(1 - Factor, 4f) };
        flip = Owner.direction == 1;
        switch (Owner)
        {
            case Player player:
                {
                    //SoundEngine.PlaySound(SoundID.Item71);
                    var tarpos = player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition;
                    player.direction = Math.Sign(tarpos.X - player.Center.X);
                    Rotation = (tarpos - Owner.Center).ToRotation();
                    break;
                }

        }
        base.Update(triggered);
        if (timer > 0)
            for (int n = 0; n < 4; n++)
            {
                Vector2 unit = (MathHelper.PiOver2 * n + 4 * Factor).ToRotationVector2();
                OtherMethods.FastDust(Owner.Center + unit * (MathF.Exp(Factor) - 1) * 128, default, standardInfo.standardColor, 2f);

                OtherMethods.FastDust(Owner.Center + new Vector2(unit.X + unit.Y, -unit.X + unit.Y) * (MathF.Exp(Factor) - 1) * 128, default, standardInfo.standardColor, 1.5f);

            }
        if (timer == 1 && counter == Cycle)
        {
            timer = 0;
            SoundEngine.PlaySound(SoundID.Item84);
            for (int n = 0; n < 40; n++)
            {
                OtherMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 32), standardInfo.standardColor, Main.rand.NextFloat(1, 4));
                OtherMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4) + (Rotation + offsetRotation).ToRotationVector2() * Main.rand.NextFloat(0, 64), standardInfo.standardColor, Main.rand.NextFloat(1, 2));

            }
        }
        if (!AutoNext && timer < 2 && counter == Cycle)
        {

            if (triggered)
                timer++;
            else
                switch (Owner)
                {
                    case Player plr:
                        SequencePlayer mplr = plr.GetModPlayer<SequencePlayer>();
                        mplr.PendingForcedNext = true;
                        timer = 0;
                        break;
                }
        }
        if (!triggered && timer != 0)
        {
            timer = 0;
            SoundEngine.PlaySound(MySoundID.MagicStaff);
        }
    }
    public override void OnEndSingle()
    {
        if (!AutoNext)
            switch (Owner)
            {
                case Player plr:
                    SequencePlayer mplr = plr.GetModPlayer<SequencePlayer>();
                    mplr.PendingForcedNext = true;
                    break;
            }
        base.OnEndSingle();
    }
    public override bool Collide(Rectangle rectangle) => false;
    #endregion
}

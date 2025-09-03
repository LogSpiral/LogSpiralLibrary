using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using Terraria.Audio;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.ExtendedMelee;

public class ChargingInfo : ExtendedMelee
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

    #endregion 参数字段

    #region 重写属性

    public override float offsetRotation => Main.rand.NextFloat(-1, 1) * Main.rand.NextFloat(0, 1) * Factor * .5f + MathHelper.Lerp(StartRotation, ChargingRotation, MathHelper.SmoothStep(1, 0, MathF.Pow(Factor, 3))) * Owner.direction;
    public override float offsetSize => base.offsetSize;
    public override bool Attacktive => Timer == 1;

    #endregion 重写属性

    #region 重写函数

    public override void UpdateStatus(bool triggered)
    {
        StandardInfo.extraLight = 3 * MathF.Pow(1 - Factor, 4f);
        Flip = Owner.direction == 1;
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
        base.UpdateStatus(triggered);
        if (Timer > 0)
            for (int n = 0; n < 4; n++)
            {
                Vector2 unit = (MathHelper.PiOver2 * n + 4 * Factor).ToRotationVector2();
                MiscMethods.FastDust(Owner.Center + unit * (MathF.Exp(Factor) - 1) * 128, default, StandardInfo.standardColor, 2f);

                MiscMethods.FastDust(Owner.Center + new Vector2(unit.X + unit.Y, -unit.X + unit.Y) * (MathF.Exp(Factor) - 1) * 128, default, StandardInfo.standardColor, 1.5f);
            }
        if (Timer == 1 && Counter == CounterMax)
        {
            Timer = 0;
            SoundEngine.PlaySound(SoundID.Item84);
            for (int n = 0; n < 40; n++)
            {
                MiscMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 32), StandardInfo.standardColor, Main.rand.NextFloat(1, 4));
                MiscMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4) + (Rotation + offsetRotation).ToRotationVector2() * Main.rand.NextFloat(0, 64), StandardInfo.standardColor, Main.rand.NextFloat(1, 2));
            }
        }
        if (!AutoNext && Timer < 2 && Counter == CounterMax)
        {
            if (triggered)
                Timer++;
            else
                switch (Owner)
                {
                    case Player plr:
                        SequencePlayer mplr = plr.GetModPlayer<SequencePlayer>();
                        mplr.PendingForcedNext = true;
                        Timer = 0;
                        break;
                }
        }
        if (!triggered && Timer != 0)
        {
            Timer = 0;
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

    #endregion 重写函数
}
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.IO;
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

    public override float OffsetRotation => Main.rand.NextFloat(-1, 1) * Main.rand.NextFloat(0, 1) * Factor * .5f + MathHelper.Lerp(StartRotation, ChargingRotation, MathHelper.SmoothStep(1, 0, MathF.Pow(Factor, 3))) * Owner.direction;
    public override float OffsetSize => Timer == 0 ? 0 : 1;
    public override bool Attacktive => Timer == 1;

    #endregion 重写属性

    #region 重写函数

    public override void UpdateStatus(bool triggered)
    {
        var shaderID = StandardInfo.VertexStandard.dyeShaderID;
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

        #region 蓄力粒子效果

        if (Timer > 0)
            for (int n = 0; n < 4; n++)
            {
                Vector2 unit = (MathHelper.PiOver2 * n + 4 * Factor).ToRotationVector2();
                MiscMethods.FastDust(Owner.Center + unit * (MathF.Exp(Factor) - 1) * 128, default, StandardInfo.standardColor, 2f, shaderID);

                MiscMethods.FastDust(Owner.Center + new Vector2(unit.X + unit.Y, -unit.X + unit.Y) * (MathF.Exp(Factor) - 1) * 128, default, StandardInfo.standardColor, 1.5f, shaderID);
            }

        #endregion 蓄力粒子效果

        #region 蓄力完全完成时的粒子爆发

        if (Timer == 1)
        {
            bool max = Counter == CounterMax;
            // 此处将计时器从1设为了0
            // 避免了Timer == 1的反复触发
            if (max)
                Timer = 0;
            SoundEngine.PlaySound(SoundID.Item84 with { Volume = max ? 1.0f : .5f }, Owner.Center);
            int dustAmount = max ? 40 : 10;
            for (int n = 0; n < dustAmount; n++)
            {
                MiscMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 32), StandardInfo.standardColor, Main.rand.NextFloat(1, 4), shaderID);
                MiscMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4) + (Rotation + OffsetRotation).ToRotationVector2() * Main.rand.NextFloat(0, 64), StandardInfo.standardColor, Main.rand.NextFloat(1, 2), shaderID);
            }
        }

        #endregion 蓄力完全完成时的粒子爆发

        #region 蓄力锁更新进程

        // 话说现在没有强绑定计时器了是不是可以直接魔改IsCompleted
        if (!AutoNext && IsCompleted)
        {
            if (triggered)
                Timer++;
            else
                switch (Owner)
                {
                    case Player plr:
                        SequencePlayer mplr = plr.GetModPlayer<SequencePlayer>();
                        mplr.PendingForcedNext = true;
                        break;
                }
        }

        #endregion 蓄力锁更新进程

        if (!triggered && Timer != 0)
        {
            if (Counter == 1)
                SoundEngine.PlaySound(MySoundID.MagicStaff, Owner.Center);
            else if (!AutoNext)
                switch (Owner)
                {
                    case Player plr:
                        SequencePlayer mplr = plr.GetModPlayer<SequencePlayer>();
                        mplr.PendingForcedNext = true;
                        break;
                }


            Timer = 0;
            Counter = CounterMax;


        }
    }

    public override void OnDeactive()
    {
        if (!AutoNext)
            switch (Owner)
            {
                case Player plr:
                    if (!plr.GetModPlayer<SequencePlayer>().PendingForcedNext)
                        Projectile.Kill();
                    break;
            }
        base.OnDeactive();
    }

    public override bool Collide(Rectangle rectangle) => false;

    public override void NetSend(BinaryWriter writer)
    {
        base.NetSend(writer);
        writer.Write(ChargingRotation);
        writer.Write(StartRotation);
    }

    public override void NetReceive(BinaryReader reader)
    {
        base.NetReceive(reader);
        ChargingRotation = reader.ReadSingle();
        StartRotation = reader.ReadSingle();
    }

    #endregion 重写函数
}
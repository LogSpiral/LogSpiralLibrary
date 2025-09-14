using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.ExtendedMelee;

public class ConvoluteInfo : ExtendedMelee
{
    #region 辅助字段

    private Vector2 _unit;

    #endregion 辅助字段

    #region 重写属性

    public override float OffsetRotation => Factor * MathHelper.TwoPi * 2 + (float)LogSpiralLibraryMod.ModTime2 * .025f;
    public override float CompositeArmRotation => Owner.direction;
    public override Vector2 OffsetCenter => _unit * Factor.CosFactor() * 512;
    public override bool Attacktive => Factor >= .25f;
    public override bool OwnerHitCheek => false;

    #endregion 重写属性

    #region 重写函数

    public override void OnStartSingle()
    {
        base.OnStartSingle();
        KValue = 1.5f;
        _unit = Rotation.ToRotationVector2();
    }

    public override void OnStartAttack()
    {
        if (Owner is Player plr && IsLocalProjectile)
        {
            plr.Center += OffsetCenter;
            ShootExtraProjectile();

            plr.Center -= OffsetCenter;
        }
        base.OnStartAttack();
    }

    public override void OnAttack()
    {
        if ((int)LogSpiralLibraryMod.ModTime2 % 6 == 0)
            SoundEngine.PlaySound(MySoundID.BoomerangRotating, Owner?.Center);

        base.OnAttack();
    }

    public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        var origf = fTimer;
        IEnumerable<CustomVertexInfo> result = base.GetWeaponVertex(texture, 1f);
        for (int i = 1; i < 10; i++)
        {
            fTimer += .2f;
            result = result.Concat(base.GetWeaponVertex(texture, 1f - i / 10f));
        }
        fTimer = origf;
        return [.. result];
    }

    public override void NetSendUpdateElement(BinaryWriter writer)
    {
        base.NetSendUpdateElement(writer);
        writer.WriteVector2(_unit);
    }

    public override void NetReceiveUpdateElement(BinaryReader reader)
    {
        base.NetReceiveUpdateElement(reader);
        _unit = reader.ReadVector2();
    }
    #endregion 重写函数
}
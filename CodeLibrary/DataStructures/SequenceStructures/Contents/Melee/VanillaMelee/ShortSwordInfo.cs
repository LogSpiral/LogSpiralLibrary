using LogSpiralLibrary.CodeLibrary.Utilties;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 经典短剑
/// </summary>
public class ShortSwordInfo : VanillaMelee
{
    #region 重写属性

    public override Vector2 OffsetOrigin => Vector2.SmoothStep(new Vector2(-0.15f, 0.15f), -new Vector2(-0.15f, 0.15f), 1 - (1 - Factor) * (1 - Factor));
    public override bool Attacktive => Factor <= 0.5f;

    #endregion 重写属性

    #region 重写函数

    public override void OnStartSingle()
    {
        Flip = Owner.direction != 1;
        base.OnStartSingle();
    }

    public override void OnStartAttack()
    {
        SoundEngine.PlaySound(StandardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
        ShootExtraProjectile();

        base.OnStartAttack();
    }

    #endregion 重写函数
}
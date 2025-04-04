using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 经典短剑
/// </summary>
public class ShortSwordInfo : VanillaMelee
{
    #region 重写属性
    public override Vector2 offsetOrigin => Vector2.SmoothStep(new Vector2(-0.15f, 0.15f), -new Vector2(-0.15f, 0.15f), 1 - (1 - Factor) * (1 - Factor));
    public override bool Attacktive => Factor <= 0.5f;
    #endregion

    #region 重写函数
    public override void OnStartSingle()
    {
        flip = Owner.direction != 1;
        base.OnStartSingle();
    }

    public override void OnStartAttack()
    {
        SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
        if (Owner is Player plr)
        {
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
        }
        base.OnStartAttack();
    }
    #endregion
}
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// Lancer!!♠
/// </summary>
public class LanceInfo : VanillaMelee
{
    #region 重写属性

    public override Vector2 offsetOrigin => Vector2.Lerp(new Vector2(-0.3f, 0.3f), default, 1 - MathHelper.Clamp(MathHelper.SmoothStep(1, 0, Factor) * 4, 0, 1));
    public override bool Attacktive => Factor < 0.75f;

    #endregion 重写属性

    #region 重写函数

    public override void OnStartSingle()
    {
        Flip = Owner.direction != 1;
        base.OnStartSingle();
    }

    public override void OnStartAttack()
    {
        if (Owner is Player plr)
        {
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
        }
        SoundEngine.PlaySound(StandardInfo.soundStyle);
        base.OnStartAttack();
    }

    #endregion 重写函数
}
using LogSpiralLibrary.CodeLibrary.Utilties;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;
/// <summary>
/// 长枪
/// </summary>
public class SpearInfo : VanillaMelee
{
    #region 重写属性
    public override float Factor => MathF.Pow(base.Factor, 3);

    public override float offsetRotation => MathF.Sin(Factor * MathHelper.TwoPi) * .25f;

    public override Vector2 offsetCenter
    {
        get
        {
            float v = MathF.Pow(1 - MathF.Abs(2 * Factor - 1), 2);
            //KValue = 1 + v * 4f;
            return Rotation.ToRotationVector2() * v * 96;
        }
    }

    public override bool Attacktive => Factor < 0.85f;
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

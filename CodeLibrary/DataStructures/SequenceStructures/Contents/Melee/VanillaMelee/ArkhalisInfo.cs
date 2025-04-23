using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;
/// <summary>
/// 我没拿到真空刀
/// </summary>
public class ArkhalisInfo : VanillaMelee
{
    #region 重写属性
    public override float Factor => base.Factor * 2 % 1;
    public override float offsetRotation => MathHelper.Lerp(1f, -1f, Factor) * (flip ? -1 : 1) * MathHelper.PiOver2;
    public override bool Attacktive => Factor < 0.85f;
    #endregion

    #region 重写函数
    public override void OnStartAttack()
    {
        SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
        if (Owner is Player plr)
        {
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
        }
        flip ^= true;
        var verS = standardInfo.vertexStandard;
        if (verS.active)
        {
            var range = (1.625f * Main.rand.NextFloat(.5f, 1.25f), -.75f);
            bool f = flip;
            float size = verS.scaler * ModifyData.actionOffsetSize * offsetSize;
            var pair = standardInfo.vertexStandard.swooshTexIndex;
            float randK = KValue * Main.rand.NextFloat(1f, 1.75f);
            float randR = Rotation + Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6) * Main.rand.NextFloat(0, 1);
            UltraSwoosh u;
            if (standardInfo.itemType == ItemID.TrueExcalibur)
            {
                u = UltraSwoosh.NewUltraSwoosh(Color.Pink, (int)(verS.timeLeft * 1.2f), size, Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, f, randR, randK, (range.Item1 + 0.125f, range.Item2 - 0.125f), pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                UltraSwoosh subSwoosh = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size * .67f, Owner.Center, verS.heatMap, f, randR, randK, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                subSwoosh.ApplyStdValueToVtxEffect(standardInfo);
            }
            else
            {
                u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size, Owner.Center, verS.heatMap, f, randR, randK, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
            }
            u.ApplyStdValueToVtxEffect(standardInfo);
            //return u;
        }
        base.OnStartAttack();
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
    }
    #endregion
}

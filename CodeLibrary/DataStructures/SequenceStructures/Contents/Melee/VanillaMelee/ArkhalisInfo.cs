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
    public override float OffsetRotation => MathHelper.Lerp(1f, -1f, Factor) * (Flip ? -1 : 1) * MathHelper.PiOver2;
    public override bool Attacktive => Factor < 0.85f;

    #endregion 重写属性

    #region 重写函数

    public override void OnStartAttack()
    {
        SoundEngine.PlaySound(StandardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
        if (Owner is Player plr)
        {
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
        }
        Flip ^= true;
        var verS = StandardInfo.VertexStandard;
        if (verS.active)
        {
            var range = (1.625f * Main.rand.NextFloat(.5f, 1.25f), -.75f);
            bool f = Flip;
            float size = verS.scaler * ModifyData.Size * OffsetSize;
            var pair = StandardInfo.VertexStandard.swooshTexIndex;
            float randK = KValue * Main.rand.NextFloat(1f, 1.75f);
            float randR = Rotation + Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6) * Main.rand.NextFloat(0, 1);
            UltraSwoosh u;
            if (StandardInfo.itemType == ItemID.TrueExcalibur)
            {
                var subSwoosh = UltraSwoosh.NewUltraSwoosh(verS.canvasName, verS.timeLeft, size * .67f, Owner.Center, range);
                subSwoosh.heatMap = verS.heatMap;
                subSwoosh.negativeDir = f;
                subSwoosh.rotation = randR;
                subSwoosh.xScaler = randK;
                subSwoosh.aniTexIndex = pair?.Item1 ?? 3;
                subSwoosh.baseTexIndex = pair?.Item2 ?? 7;
                subSwoosh.ColorVector = verS.colorVec;

                u = UltraSwoosh.NewUltraSwoosh(verS.canvasName, (int)(verS.timeLeft * 1.2f), size, Owner.Center, (range.Item1 + 0.125f, range.Item2 - 0.125f));
                u.heatMap = LogSpiralLibraryMod.HeatMap[5].Value;
                u.rotation = randR;
                u.xScaler = randK;
                u.aniTexIndex = pair?.Item1 ?? 3;
                u.baseTexIndex = pair?.Item2 ?? 7;
                u.ColorVector = verS.colorVec;
                u.ApplyStdValueToVtxEffect(StandardInfo);
                u.heatRotation = 0;

                subSwoosh.ApplyStdValueToVtxEffect(StandardInfo);
            }
            else
            {
                u = UltraSwoosh.NewUltraSwoosh(verS.canvasName, verS.timeLeft, size, Owner.Center, range);
                u.heatMap = verS.heatMap;
                u.negativeDir = f;
                u.rotation = randR;
                u.xScaler = randK;
                u.aniTexIndex = pair?.Item1 ?? 3;
                u.baseTexIndex = pair?.Item2 ?? 7;
                u.ColorVector = verS.colorVec;
            }
            u.ApplyStdValueToVtxEffect(StandardInfo);
            //return u;
        }
        base.OnStartAttack();
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
    }

    #endregion 重写函数
}
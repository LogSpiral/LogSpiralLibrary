using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 请不要再冕了...什么不是烈冕号啊
/// </summary>
public class EruptionInfo : VanillaMelee
{
    #region 重写属性

    //public override float offsetSize => (-MathF.Pow(0.5f - Factor, 2) * 28 + 8) * .75f;
    //public override float offsetRotation => MathHelper.Lerp(1f, -1f, Factor) * (flip ? -1 : 1) * MathHelper.Pi / 3;

    public override float offsetRotation => ((MathHelper.Lerp(1f, -1f, Factor) * (Flip ? -1 : 1) * MathHelper.Pi).ToRotationVector2() * new Vector2(1, 1f / KValue) + Vector2.UnitX * 1.05f).ToRotation();
    public override float offsetSize => ((MathHelper.Lerp(1f, -1f, Factor) * (Flip ? -1 : 1) * MathHelper.Pi).ToRotationVector2() * new Vector2(1, 1f / KValue) + Vector2.UnitX * 1.05f).Length() * 2;
    public override bool Attacktive => true;
    public override bool OwnerHitCheek => false;

    #endregion 重写属性

    #region 辅助函数

    private CustomVertexInfo[] EruptionVertex(Texture2D texture, float alpha)
    {
        Vector2 finalOrigin = offsetOrigin + StandardInfo.standardOrigin;
        Vector2 drawCen = offsetCenter + Owner.Center;
        float sc = 1;
        if (Owner is Player plr)
            sc = plr.GetAdjustedItemScale(plr.HeldItem);
        var vtxs = DrawingMethods.GetItemVertexes(finalOrigin, StandardInfo.standardRotation, offsetRotation, Rotation, texture, KValue, offsetSize * ModifyData.Size * sc, drawCen, !Flip, alpha, StandardInfo.frame);
        List<CustomVertexInfo> result = [];
        Vector2 offVec = vtxs[4].Position - vtxs[0].Position;
        float angle = offVec.ToRotation();

        float chainMax = offVec.Length() / 8f;
        if (alpha == 1)
            for (int u = 0; u < chainMax; u++)
            {
                Texture2D chain = TextureAssets.Chain41.Value;
                Vector2 pos = Vector2.Lerp(vtxs[0].Position, vtxs[4].Position, u / chainMax) + offVec * .1f;
                Main.spriteBatch.Draw(chain, pos - Main.screenPosition, null, Lighting.GetColor(pos.ToTileCoordinates()), angle, new Vector2(4, 5), 1f, 0, 0);
            }

        offVec *= .1f;

        Vector2 off2 = offVec * .5f;

        //Rectangle fullFrame = standardInfo.frame ?? new Rectangle(0, 0, texture.Width, texture.Height);
        //Rectangle subFrame = Utils.CenteredRectangle(fullFrame.Center.ToVector2(), fullFrame.Size() * .4f);
        for (int n = 0; n < 10; n++)
        {
            //Rectangle curFrame = subFrame;
            //if (n == 0)
            //    curFrame = new Rectangle(fullFrame.X, fullFrame.Y + (int)(fullFrame.Height * .3f), fullFrame.Width * 3 / 10, fullFrame.Height * 3 / 10);
            //if(n == 9)
            //    curFrame = new Rectangle(fullFrame.X + (int)(fullFrame.Height * .3f), fullFrame.Y, fullFrame.Width * 3 / 10, fullFrame.Height * 3 / 10);
            bool flag = n != 0 && n != 9;
            CustomVertexInfo[] curGroup;
            if (flag)
                curGroup = DrawingMethods.GetItemVertexes(.5f * Vector2.One, StandardInfo.standardRotation, 0, angle + MathHelper.PiOver2, texture, .5f, ModifyData.Size * sc * .5f, drawCen + off2, !Flip, alpha, StandardInfo.frame);
            else
                curGroup = DrawingMethods.GetItemVertexes(finalOrigin, StandardInfo.standardRotation, 0, angle, texture, 1f, ModifyData.Size * sc * 1f, drawCen - (n == 9 ? off2 : default), !Flip, alpha, StandardInfo.frame);

            if (flag)
            {
                curGroup[0].TexCoord = curGroup[4].TexCoord;
                //curGroup[3].TexCoord = curGroup[2].TexCoord;
                //curGroup[5].TexCoord = curGroup[1].TexCoord;
            }
            if (flag)
                result.AddRange(curGroup);
            else if (n == 0)
            {
                result.Add(curGroup[0]);
                result.Add(curGroup[1]);
                result.Add(curGroup[2]);
            }
            else
            {
                result.Add(curGroup[3]);
                result.Add(curGroup[4]);
                result.Add(curGroup[5]);
            }
            drawCen += offVec;
        }
        return [.. result];
    }

    #endregion 辅助函数

    #region 重写函数

    public override void OnStartSingle()
    {
        base.OnStartSingle();
        KValue = Main.rand.NextFloat(1.5f, 2f);
        Rotation += Main.rand.NextFloat(-1, 1) * MathHelper.PiOver2 / 12;
        Flip = Main.rand.NextBool();
        SoundEngine.PlaySound(SoundID.Item116, Owner.Center);
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        if (Owner is Player plr && Main.rand.NextBool(5))
        {
            Vector2 orig = plr.Center;
            plr.Center = victim.Hitbox.Center();
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
            plr.Center = orig;
            if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
            {
                SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
            }
        }
    }

    public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        float origf = fTimer;
        IEnumerable<CustomVertexInfo> result = [];
        fTimer += 2f;
        for (int i = 9; i >= 0; i--)
        {
            fTimer -= .2f;
            result = result.Concat(EruptionVertex(texture, (1f - i / 10f) * (i == 0 ? 1f : .5f)));
        }
        fTimer = origf;
        return [.. result];
    }

    #endregion 重写函数
}
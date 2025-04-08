using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 转啊转
/// </summary>
public class BoomerangInfo : VanillaMelee
{
    #region 辅助字段
    public bool back;
    public Vector2 realCenter;
    #endregion

    #region 重写属性
    public override float offsetRotation => (float)LogSpiralLibraryMod.ModTime2 * 0.25f;
    public override Vector2 offsetCenter => realCenter - Owner.Center;
    public override bool Attacktive => true;
    public override bool OwnerHitCheek => false;
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        if (Factor <= .5f)
        {
            back = true;
        }
        var tile = Main.tile[realCenter.ToTileCoordinates16()];
        if (tile.HasTile && Main.tileSolid[tile.TileType])
        {
            back = true;
            Collision.HitTiles(realCenter, default, 32, 32);
            SoundEngine.PlaySound(MySoundID.ProjectileHit);
        }
        if (back && offsetCenter.Length() >= 32f)
        {
            timer = 2;
        }
        if ((int)LogSpiralLibraryMod.ModTime2 % 7 == 0)
            SoundEngine.PlaySound(MySoundID.BoomerangRotating);
        //if (back)
        //{
        //    //realCenter = Vector2.Lerp(realCenter, Owner.Center, 0.05f);
        //    realCenter += (Owner.Center - realCenter).SafeNormalize(default) * MathHelper.Max(16,(Owner.Center - realCenter).Length() * .25f);
        //}
        //else
        //{
        //    realCenter = Vector2.Lerp(realCenter, Owner.Center, -0.35f * Factor);

        //}

        realCenter += (realCenter - Owner.Center).SafeNormalize(default) * (Factor - 0.5f) * 144f;
        timer--;
    }

    public override void OnStartSingle()
    {
        Vector2 tarVec = Owner switch
        {
            Player plr => plr.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition,
            _ => default
        };
        Vector2 unit = tarVec - Owner.Center;
        unit.Normalize();
        realCenter = Owner.Center + unit * 16;
        Rotation = unit.ToRotation();
        back = false;
        base.OnStartSingle();
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        back = true;
        if (Owner is Player plr && Main.rand.NextBool(3))
        {
            Vector2 orig = plr.Center;
            plr.Center = realCenter;
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
        var origf = LogSpiralLibrarySystem.ModTime2;
        IEnumerable<CustomVertexInfo> result = [];
        LogSpiralLibrarySystem.ModTime2 -= 2f;
        for (int i = 9; i >= 0; i--)
        {
            LogSpiralLibrarySystem.ModTime2 += .2f;
            result = result.Concat(base.GetWeaponVertex(texture, (1f - i / 10f) * (i == 0 ? 1f : .5f)));
        }
        LogSpiralLibrarySystem.ModTime2 = origf;
        return [.. result];
    }
    #endregion
}
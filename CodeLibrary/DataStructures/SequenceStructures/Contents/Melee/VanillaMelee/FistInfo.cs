﻿using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;
/// <summary>
/// 石巨人之拳！！
/// </summary>
public class FistInfo : VanillaMelee
{
    #region 重写属性
    public override Vector2 offsetCenter => Rotation.ToRotationVector2() * MathF.Pow(1 - MathF.Abs(2 * Factor - 1), 2) * 512;
    public override bool Attacktive => Factor < .65f;
    public override bool OwnerHitCheek => false;
    #endregion

    #region 重写函数
    public override void OnStartAttack()
    {
        SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
        if (Owner is Player plr)
        {
            plr.Center += offsetCenter;
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
            plr.Center -= offsetCenter;
            if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
            {
                SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
            }
        }
        base.OnStartAttack();
    }


    public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        int type = Projectile.type;
        Projectile.type = 947;
        Main.DrawProj_FlailChains(Projectile, Owner.Center);
        Projectile.type = type;
        base.Draw(spriteBatch, texture);
    }

    public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        float origf = fTimer;
        IEnumerable<CustomVertexInfo> result = [];
        fTimer += 2f;
        for (int i = 9; i >= 0; i--)
        {
            fTimer -= .2f;
            result = result.Concat(base.GetWeaponVertex(texture, (1f - i / 10f) * (i == 0 ? 1f : .5f)));
        }
        fTimer = origf;
        return [.. result];
    }
    #endregion
}

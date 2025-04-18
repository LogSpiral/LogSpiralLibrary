﻿using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 白云一片去悠悠
/// <para>原版AI参考<see cref="VanillaCodeRef."/></para>
/// </summary>
public class YoyoInfo : VanillaMelee
{
    #region 辅助字段
    public Vector2 realCenter;
    #endregion

    #region 重写属性
    public override float offsetRotation => (float)LogSpiralLibraryMod.ModTime2 * 0.45f;
    public override Vector2 offsetCenter => realCenter - Owner.Center;
    public override bool Attacktive => Factor > 0.05f;
    public override bool OwnerHitCheek => false;
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        Vector2 tarVec = Owner switch
        {
            Player plr => plr.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition,
            _ => default
        };
        if (!triggered) timer = 1;
        if (timer > 10)
            realCenter = Vector2.Lerp(realCenter, tarVec, 0.05f);
        else
        {
            realCenter = Vector2.Lerp(realCenter, Owner.Center, 0.15f);
            if ((realCenter - Owner.Center).LengthSquared() < 256f)
                timer = 1;
        }
        Rotation += 0.05f;
        if ((int)LogSpiralLibraryMod.ModTime2 % 4 == 0)
            timer--;
    }

    public override void OnStartSingle()
    {
        realCenter = Owner.Center;
        KValue = Main.rand.NextFloat(1, 2);
        Rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
        SoundEngine.PlaySound(standardInfo.soundStyle);
        base.OnStartSingle();
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        if (Owner is Player plr && Main.rand.NextBool(5))
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

    public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        base.Draw(spriteBatch, texture);
        Projectile.aiStyle = 99;
        Main.instance.DrawProj_DrawYoyoString(Projectile, Owner.Center);
        Projectile.aiStyle = -1;
    }

    public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        float origf = (float)LogSpiralLibrarySystem.ModTime2;
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

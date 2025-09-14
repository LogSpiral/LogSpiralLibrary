using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System.Collections.Generic;
using System.IO;
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

    #endregion 辅助字段

    #region 重写属性

    public override float OffsetRotation => (float)LogSpiralLibraryMod.ModTime2 * 0.45f;
    public override Vector2 OffsetCenter => realCenter - Owner.Center;
    public override bool Attacktive => Factor > 0.05f;
    public override bool OwnerHitCheek => false;

    #endregion 重写属性

    #region 重写函数

    public override void UpdateStatus(bool triggered)
    {
        if (IsLocalProjectile)
        {
            Vector2 tarVec = Owner switch
            {
                Player plr => Main.MouseWorld,
                _ => default
            };
            if (!triggered) Timer = 1;
            if (Timer > 10)
                realCenter = Vector2.Lerp(realCenter, tarVec, 0.05f);
            else
            {
                realCenter = Vector2.Lerp(realCenter, Owner.Center, 0.15f);
                if ((realCenter - Owner.Center).LengthSquared() < 256f)
                    Timer = 1;
            }
            if ((int)LogSpiralLibraryMod.ModTime2 % 4 == 0)
                NetUpdateNeeded = true;
        }
        if (realCenter == default)
            realCenter = Owner.Center;
        Rotation += 0.05f;
        if ((int)LogSpiralLibraryMod.ModTime2 % 4 == 0)
            Timer--;
    }
    public override void OnStartSingle()
    {
        if (IsLocalProjectile)
        {
            realCenter = Owner.Center;
            KValue = Main.rand.NextFloat(1, 2);
            Rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
        }

        SoundEngine.PlaySound(StandardInfo.soundStyle);
        base.OnStartSingle();
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        if (Owner is Player plr && Main.rand.NextBool(5))
        {
            Vector2 orig = plr.Center;
            plr.Center = realCenter;
            ShootExtraProjectile();
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
        float origf = (float)GlobalTimeSystem.GlobalTimePaused;
        IEnumerable<CustomVertexInfo> result = [];
        GlobalTimeSystem.GlobalTimePaused -= 2f;
        for (int i = 9; i >= 0; i--)
        {
            GlobalTimeSystem.GlobalTimePaused += .2f;
            result = result.Concat(base.GetWeaponVertex(texture, (1f - i / 10f) * (i == 0 ? 1f : .5f)));
        }
        GlobalTimeSystem.GlobalTimePaused = origf;
        return [.. result];
    }
    public override void NetSendUpdateElement(BinaryWriter writer)
    {
        base.NetSendUpdateElement(writer);
        writer.WriteVector2(realCenter);
    }
    public override void NetReceiveUpdateElement(BinaryReader reader)
    {
        base.NetReceiveUpdateElement(reader);
        realCenter = reader.ReadVector2();
    }
    #endregion 重写函数
}
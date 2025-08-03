using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 链球，说实话这个不是很适合用这个实现，这个结构更适合制作插值动画式的动作
/// <para>原版的AI解析参考这个<see cref="VanillaCodeRef.AI_015_Flails(Projectile)"/></para>
/// </summary>
public class FlailInfo : VanillaMelee
{
    #region 辅助字段
    //0旋转
    //1掷出
    //2回收
    //3滞留
    //4回收2
    public int state;
    public int assistTimer;
    public Vector2 realPos;
    #endregion

    #region 重写属性
    public override float offsetRotation => state switch
    {
        3 => assistTimer * .5f,
        _ => (float)LogSpiralLibraryMod.ModTime2
    };

    public override Vector2 offsetCenter => state switch
    {
        0 => (new Vector2(64, 16) * ((float)LogSpiralLibraryMod.ModTime2 / 4f).ToRotationVector2()).RotatedBy(Rotation),
        2 or 4 => Vector2.SmoothStep(default, realPos - Owner.Center, Factor),
        _ => realPos - Owner.Center
    };
    public override bool Attacktive => true;
    public override bool OwnerHitCheek => false;
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        if (state != 3)
        {
            Vector2 tarVec = Owner switch
            {
                Player plr => plr.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition,
                _ => default
            };
            Rotation = (tarVec - Owner.Center).ToRotation();
            if ((int)LogSpiralLibraryMod.ModTime2 % 10 == 0)
                SoundEngine.PlaySound(SoundID.Item7);
        }

        switch (state)
        {
            case 0:
                {
                    if (!triggered)
                    {
                        realPos = offsetCenter + Owner.Center;
                        state = 1;
                    }
                    break;
                }
            case 1:
                {
                    if (triggered)
                    {
                        state = 3;
                        timer = timerMax = timerMax * 10;
                        assistTimer = 0;
                        break;
                    }
                    realPos += 32f * Rotation.ToRotationVector2() + new Vector2(0, assistTimer * assistTimer * .25f);
                    assistTimer++;
                    var tile = Framing.GetTileSafely(realPos.ToTileCoordinates16());

                    if (assistTimer > 30 || offsetCenter.Length() > 512 || tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        state = 2;
                        assistTimer = timerMax;
                        timer = timerMax = 30;
                    }
                    break;
                }
            case 2:
                {
                    if (triggered)
                    {
                        Vector2 pos = offsetCenter + Owner.position;
                        state = 3;
                        timer = timerMax = assistTimer * 10;
                        realPos = pos;
                        assistTimer = 0;
                        break;
                    }
                    timer--;

                    break;
                }
            case 3:
                {
                    timer--;
                    if (timer <= 10 || !triggered || offsetCenter.Length() > 512)
                    {
                        timer = timerMax = 10;
                        state = 4;
                    }
                    var tile = Framing.GetTileSafely(realPos.ToTileCoordinates16()) ;
                    if (!(tile.HasTile && Main.tileSolid[tile.TileType]))
                    {
                        assistTimer++;
                        realPos += assistTimer * assistTimer * new Vector2(0, 0.0625f);
                        tile = Framing.GetTileSafely((realPos + assistTimer * 4f * Vector2.UnitY).ToTileCoordinates16());
                        if (tile.HasTile && Main.tileSolid[tile.TileType])
                        {
                            realPos += assistTimer * 4f * Vector2.UnitY;
                            assistTimer = 0;
                        }
                    }
                    else
                    {
                        assistTimer = 0;
                    }
                    break;
                }
            case 4:
                {
                    timer--;
                    break;
                }
        }
    }

    public override void OnStartSingle()
    {
        state = 0;
        assistTimer = 0;
        realPos = default;
        base.OnStartSingle();
    }


    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        if (Owner is Player plr && Main.rand.NextBool(5))
        {
            Vector2 orig = plr.Center;
            plr.Center = Projectile.Center;
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
        int type = Projectile.type;
        Projectile.type = 947;
        Main.DrawProj_FlailChains(Projectile, Owner.Center);
        Projectile.type = type;
        base.Draw(spriteBatch, texture);
    }

    public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        var origf = GlobalTimeSystem.GlobalTimePaused;
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
    #endregion
}

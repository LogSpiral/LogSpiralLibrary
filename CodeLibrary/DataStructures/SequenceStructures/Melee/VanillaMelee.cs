using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Melee
{
    //原版手持弹幕近战复刻总集篇！！
    /// <summary>
    /// 经典宽剑
    /// </summary>
    public class BoardSwordInfo : MeleeAction
    {
        public override float offsetRotation => MathHelper.SmoothStep(0.15f, -0.75f, Factor * Factor) * MathHelper.Pi * Owner.direction;
        public override bool Attacktive => Factor < .75f;
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
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));

            }
            base.OnStartAttack();
        }
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            float origf = fTimer;
            IEnumerable<CustomVertexInfo> result = [];
            fTimer += 2.0f;
            for (int i = 9; i >= 0; i--)
            {
                fTimer -= .2f;
                result = result.Concat(base.GetWeaponVertex(texture, (1f - i / 10f) * (i == 0 ? 1f : .5f)));
            }
            fTimer = origf;
            return result.ToArray();
        }
    }
    /// <summary>
    /// 经典短剑
    /// </summary>
    public class ShortSwordInfo : MeleeAction
    {
        public override float Factor => base.Factor;//  * 2 % 1
        public override bool Attacktive => Factor <= 0.5f;
        public override Vector2 offsetOrigin => Vector2.SmoothStep(new Vector2(-0.15f, 0.15f), -new Vector2(-0.15f, 0.15f), 1 - (1 - Factor) * (1 - Factor));

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
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));

            }
            base.OnStartAttack();
        }
    }
    /// <summary>
    /// 转啊转
    /// </summary>
    public class BoomerangInfo : MeleeAction
    {
        public override bool Attacktive => true;
        public override Vector2 offsetCenter => realCenter - Owner.Center;
        public Vector2 realCenter;
        public override float offsetRotation => (float)LogSpiralLibraryMod.ModTime2 * 0.25f;
        public bool back;
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
        public override void OnStartAttack()
        {

            base.OnStartAttack();
        }
        public override bool Collide(Rectangle rectangle)
        {
            bool flag = base.Collide(rectangle);
            if (flag)
            {
                back = true;
                if (Owner is Player plr && Main.rand.NextBool(3))
                {
                    Vector2 orig = plr.Center;
                    plr.Center = realCenter;
                    plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
                    plr.Center = orig;
                    if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                    }
                }
            }
            return flag;
        }
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
            return result.ToArray();
        }
    }
    /// <summary>
    /// 链球，说实话这个不是很适合用这个实现，这个结构更适合制作插值动画式的动作
    /// <para>原版的AI解析参考这个<see cref="VanillaCodeRef.AI_015_Flails(Projectile)"/></para>
    /// </summary>
    public class FlailInfo : MeleeAction
    {
        public override bool Attacktive => true;
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
        //0旋转
        //1掷出
        //2回收
        //3滞留
        //4回收2
        public int state;
        public int assistTimer;
        public Vector2 realPos;
        public override void OnStartSingle()
        {
            state = 0;
            assistTimer = 0;
            realPos = default;
            base.OnStartSingle();
        }
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
                        var tile = Main.tile[realPos.ToTileCoordinates16()];

                        if (assistTimer > 30 || offsetCenter.Length() > 512 || (tile.HasTile && Main.tileSolid[tile.TileType]))
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
                        var tile = Main.tile[realPos.ToTileCoordinates16()];
                        if (!(tile.HasTile && Main.tileSolid[tile.TileType]))
                        {
                            assistTimer++;
                            realPos += assistTimer * assistTimer * new Vector2(0, 0.0625f);
                            tile = Main.tile[(realPos + assistTimer * 4f * Vector2.UnitY).ToTileCoordinates16()];
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
        public override bool Collide(Rectangle rectangle)
        {
            bool flag = base.Collide(rectangle);
            if (flag)
            {
                if (Owner is Player plr && Main.rand.NextBool(5))
                {
                    Vector2 orig = plr.Center;
                    plr.Center = Projectile.Center;
                    plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
                    plr.Center = orig;
                    if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                    }
                }
            }
            return flag;
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
            var origf = LogSpiralLibrarySystem.ModTime2;
            IEnumerable<CustomVertexInfo> result = [];
            LogSpiralLibrarySystem.ModTime2 -= 2f;

            for (int i = 9; i >= 0; i--)
            {
                LogSpiralLibrarySystem.ModTime2 += .2f;
                result = result.Concat(base.GetWeaponVertex(texture, (1f - i / 10f) * (i == 0 ? 1f : .5f)));
            }
            LogSpiralLibrarySystem.ModTime2 = origf;
            return result.ToArray();
        }
    }
    /// <summary>
    /// 长枪
    /// </summary>
    public class SpearInfo : MeleeAction
    {
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
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));

            }
            base.OnStartAttack();
        }
        public override float Factor => MathF.Pow(base.Factor, 3);
    }
    /// <summary>
    /// 石巨人之拳！！
    /// </summary>
    public class FistInfo : MeleeAction
    {
        public override bool Attacktive => Factor < .65f;
        public override Vector2 offsetCenter => Rotation.ToRotationVector2() * MathF.Pow(1 - MathF.Abs(2 * Factor - 1), 2) * 512;
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
            if (Owner is Player plr)
            {
                plr.Center += offsetCenter;
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
                plr.Center -= offsetCenter;
                if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                {
                    SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                }
            }
            base.OnStartAttack();
        }
        public override float Factor => base.Factor;
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
            return result.ToArray();
        }
    }
    /// <summary>
    /// 圣骑士会个🔨
    /// </summary>
    public class HammerInfo : MeleeAction
    {
        public override float offsetRotation => Factor * MathHelper.TwoPi + (float)LogSpiralLibraryMod.ModTime2 * .025f;
        public override Vector2 offsetCenter => Rotation.ToRotationVector2() * MathF.Pow(1 - MathF.Abs(2 * (Factor * 2 % 1) - 1), 2) * 256;
        public override bool Attacktive => true;
        public override void Update(bool triggered)
        {
            timer--;
            if ((int)LogSpiralLibraryMod.ModTime2 % 6 == 0)
                SoundEngine.PlaySound(MySoundID.BoomerangRotating, Owner?.Center);

        }
        public override bool Collide(Rectangle rectangle)
        {
            bool flag = base.Collide(rectangle);
            if (Owner is Player plr && flag && Main.rand.NextBool(3))
            {
                plr.Center += offsetCenter;
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
                plr.Center -= offsetCenter;
                if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                {
                    SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                }
            }
            return flag;
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
            return result.ToArray();
        }
    }
    /// <summary>
    /// ─━╋ ─━╋ ─━╋
    /// </summary>
    public class KnivesInfo : MeleeAction
    {
        public override Vector2 offsetCenter => Rotation.ToRotationVector2() * (1 - Factor) * 1024 + new Vector2(0, MathF.Pow(1 - Factor, 2) * 256);
        public override bool Attacktive => true;
        public override float offsetRotation => base.offsetRotation + MathF.Pow((1 - Factor), 4) * MathHelper.Pi * 4;
        public override void Update(bool triggered)
        {
            timer--;
        }
        public override bool Collide(Rectangle rectangle)
        {
            bool flag = base.Collide(rectangle);
            if (flag)
            {
                if (Owner is Player plr && Main.rand.NextBool(10))
                {
                    Vector2 orig = plr.Center;
                    plr.Center = offsetCenter + Owner.Center;
                    plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
                    plr.Center = orig;
                    if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                    }
                }
            }
            return flag;
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
            return result.ToArray();
        }
    }
    /// <summary>
    /// 白云一片去悠悠
    /// <para>原版AI参考<see cref="VanillaCodeRef."/></para>
    /// </summary>
    public class YoyoInfo : MeleeAction
    {
        public override Vector2 offsetCenter => realCenter - Owner.Center;
        public override float offsetRotation => (float)LogSpiralLibraryMod.ModTime2 * 0.45f;
        public override bool Attacktive => Factor > 0.05f;
        public Vector2 realCenter;
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
        public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            base.Draw(spriteBatch, texture);
            Projectile.aiStyle = 99;
            Main.instance.DrawProj_DrawYoyoString(Projectile, Owner.Center);
            Projectile.aiStyle = -1;
        }
        public override bool Collide(Rectangle rectangle)
        {
            bool flag = base.Collide(rectangle);
            if (flag)
            {
                if (Owner is Player plr && Main.rand.NextBool(5))
                {
                    Vector2 orig = plr.Center;
                    plr.Center = realCenter;
                    plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
                    plr.Center = orig;
                    if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                    }
                }
            }
            return flag;
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
            return result.ToArray();
        }
    }
    /// <summary>
    /// 我没拿到真空刀
    /// </summary>
    public class ArkhalisInfo : MeleeAction
    {
        public override float Factor => base.Factor * 2 % 1;
        public override float offsetRotation => MathHelper.Lerp(1f, -1f, Factor) * (flip ? -1 : 1) * MathHelper.PiOver2;
        public override bool Attacktive => Factor < 0.85f;
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
            if (Owner is Player plr)
            {
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
            }
            flip ^= true;
            var verS = standardInfo.vertexStandard;
            if (verS.active)
            {
                UltraSwoosh u = null;
                UltraSwoosh subSwoosh = null;
                var range = (1.625f * Main.rand.NextFloat(.5f, 1.25f), -.75f);
                bool f = flip;
                float size = verS.scaler * ModifyData.actionOffsetSize * offsetSize;
                var pair = standardInfo.vertexStandard.swooshTexIndex;
                float randK = KValue * Main.rand.NextFloat(1f, 1.75f);
                float randR = Rotation + Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6) * Main.rand.NextFloat(0, 1);
                if (standardInfo.itemType == ItemID.TrueExcalibur)
                {
                    u = UltraSwoosh.NewUltraSwoosh(Color.Pink, (int)(verS.timeLeft * 1.2f), size, Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, f, randR, randK, (range.Item1 + 0.125f, range.Item2 - 0.125f), pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                    subSwoosh = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size * .67f, Owner.Center, verS.heatMap, f, randR, randK, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                    subSwoosh.ApplyStdValueToVtxEffect(standardInfo);
                }
                else
                {
                    u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size, Owner.Center, verS.heatMap, f, randR, randK, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                }
                if (verS.renderInfos == null)
                    u.ResetAllRenderInfo();
                else
                {
                    u.ModityAllRenderInfo(verS.renderInfos);
                }
                u.ApplyStdValueToVtxEffect(standardInfo);
                //return u;
            }
            base.OnStartAttack();
        }
        public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            //base.Draw(spriteBatch, texture);
        }
    }
    /// <summary>
    /// 请不要再冕了...什么不是烈冕号啊
    /// </summary>
    public class EruptionInfo : MeleeAction
    {
        //public override float offsetSize => (-MathF.Pow(0.5f - Factor, 2) * 28 + 8) * .75f;
        //public override float offsetRotation => MathHelper.Lerp(1f, -1f, Factor) * (flip ? -1 : 1) * MathHelper.Pi / 3;
        public override float offsetSize => ((MathHelper.Lerp(1f, -1f, Factor) * (flip ? -1 : 1) * MathHelper.Pi).ToRotationVector2() * new Vector2(1, 1f / KValue) + Vector2.UnitX * 1.05f).Length() * 2;
        public override float offsetRotation => ((MathHelper.Lerp(1f, -1f, Factor) * (flip ? -1 : 1) * MathHelper.Pi).ToRotationVector2() * new Vector2(1, 1f / KValue) + Vector2.UnitX * 1.05f).ToRotation();
        public override bool Attacktive => true;
        public override void OnAttack()
        {
            //KValue = -MathF.Pow(0.5f - Factor, 2) * 28 + 8;
            //KValue = 1;

            base.OnAttack();
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            KValue = Main.rand.NextFloat(1.5f, 2f);
            Rotation += Main.rand.NextFloat(-1, 1) * MathHelper.PiOver2 / 12;
            flip = Main.rand.NextBool();
            SoundEngine.PlaySound(SoundID.Item116, Owner.Center);
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
            return result.ToArray();
        }
        CustomVertexInfo[] EruptionVertex(Texture2D texture, float alpha)
        {
            Vector2 finalOrigin = offsetOrigin + standardInfo.standardOrigin;
            float finalRotation = offsetRotation + standardInfo.standardRotation;
            Vector2 drawCen = offsetCenter + Owner.Center;
            float sc = 1;
            if (Owner is Player plr)
                sc = plr.GetAdjustedItemScale(plr.HeldItem);
            var vtxs = DrawingMethods.GetItemVertexes(finalOrigin, finalRotation, Rotation, texture, KValue, offsetSize * ModifyData.actionOffsetSize * sc, drawCen, !flip, alpha, standardInfo.frame);
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
                    curGroup = DrawingMethods.GetItemVertexes(.5f * Vector2.One, standardInfo.standardRotation, angle + MathHelper.PiOver2, texture, .5f, ModifyData.actionOffsetSize * sc * .5f, drawCen + off2, !flip, alpha, standardInfo.frame);
                else
                    curGroup = DrawingMethods.GetItemVertexes(finalOrigin, standardInfo.standardRotation, angle, texture, 1f, ModifyData.actionOffsetSize * sc * 1f, drawCen - (n == 9 ? off2 : default), !flip, alpha, standardInfo.frame);

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
            return result.ToArray();
        }
        public override bool Collide(Rectangle rectangle)
        {
            bool flag = base.Collide(rectangle);
            if (flag)
            {
                if (Owner is Player plr && Main.rand.NextBool(5))
                {
                    Vector2 orig = plr.Center;
                    plr.Center = rectangle.Center();
                    plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
                    plr.Center = orig;
                    if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                    }
                }
            }
            return flag;
        }
    }
    /// <summary>
    /// 其实是天龙之怒
    /// </summary>
    public class RotatingInfo : MeleeAction
    {
        public override float offsetRotation => (float)LogSpiralLibraryMod.ModTime2 * 0.45f * (flip ? -1 : 1);
        public override Vector2 offsetOrigin => base.offsetOrigin;
        public override bool Attacktive => true;
        public override void OnStartSingle()
        {
            flip = Owner.direction != 1;
            if (Owner is Player plr)
            {
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
            }
            SoundEngine.PlaySound(standardInfo.soundStyle);
            base.OnStartSingle();
        }
        public override void OnAttack()
        {

            base.OnAttack();
        }
        public override void Update(bool triggered)
        {
            base.Update(triggered);
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
            return result.ToArray();
        }
    }
    /// <summary>
    /// Lancer!!♠
    /// </summary>
    public class LanceInfo : MeleeAction
    {
        public override Vector2 offsetOrigin => Vector2.Lerp(new Vector2(-0.3f, 0.3f), default, 1 - MathHelper.Clamp(MathHelper.SmoothStep(1, 0, Factor) * 4, 0, 1));
        public override void OnStartAttack()
        {
            if (Owner is Player plr)
            {
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
            }
            SoundEngine.PlaySound(standardInfo.soundStyle);
            base.OnStartAttack();
        }
        public override void OnStartSingle()
        {
            flip = Owner.direction != 1;
            base.OnStartSingle();
        }
        public override void Update(bool triggered)
        {
            //if (triggered && timer < 3) timer = 2;
            base.Update(triggered);
        }
        public override bool Attacktive => Factor < 0.75f;
    }
    /// <summary>
    /// 银色战车！！
    /// </summary>
    public class StarlightInfo : MeleeAction
    {
        public override bool Attacktive => true;
        public override Vector2 offsetCenter => (Main.rand.NextVector2Unit() * new Vector2(16, 4) + 16 * Vector2.UnitX).RotatedBy(Rotation);
        public override float offsetRotation => Main.rand.NextFloat(-1f, 1f) * MathHelper.Pi / 12f;
        public override void OnAttack()
        {
            Vector2 tarVec = Owner switch
            {
                Player plr => plr.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition,
                _ => default
            };
            Rotation = (tarVec - Owner.Center).ToRotation();
            if (timer % 3 == 0)
            {
                SoundEngine.PlaySound((standardInfo.soundStyle ?? MySoundID.SwooshNormal_1) with { MaxInstances = -1 });

            }
            base.OnAttack();
        }
        public override void OnStartSingle()
        {
            flip = Owner.direction != 1;
            if (Owner is Player plr)
            {
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
            }
            base.OnStartSingle();
        }
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            Vector2 finalOrigin = offsetOrigin + standardInfo.standardOrigin;
            float finalRotation = offsetRotation + standardInfo.standardRotation;
            Vector2 drawCen = offsetCenter + Owner.Center;
            float sc = 1;
            if (Owner is Player plr)
                sc = plr.GetAdjustedItemScale(plr.HeldItem);
            return DrawingMethods.GetItemVertexes(finalOrigin, finalRotation, Rotation, texture, KValue, ModifyData.actionOffsetSize * sc, drawCen, !flip, alpha, standardInfo.frame);
        }
        public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            base.Draw(spriteBatch, texture);
            float sc = 1;
            if (Owner is Player plr)
                sc = plr.GetAdjustedItemScale(plr.HeldItem);
            spriteBatch.DrawStarLight(Rotation, Owner.Center, standardInfo.standardColor, ModifyData.actionOffsetSize * sc * offsetSize * texture.Size().Length() * 3, 1, 1f);

        }
        public override bool Collide(Rectangle rectangle)
        {
            if (Attacktive)
            {
                Projectile.localNPCHitCooldown = Math.Clamp(timerMax / 2, 1, 514);
                float point1 = 0f;
                return Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), Projectile.Center,
                        targetedVector * 1.5f + Projectile.Center, 48f, ref point1);
            }
            return false;
        }
    }
    /// <summary>
    /// 天顶
    /// </summary>
    public class ZenithInfo : MeleeAction
    {
        public override Vector2 offsetCenter => Rotation.ToRotationVector2() * dist * .5f + (offsetRotation.ToRotationVector2() * new Vector2(dist * .5f, 100 / KValue)).RotatedBy(Rotation);
        public override float offsetRotation => MathHelper.SmoothStep(1f, -1f, MathHelper.Clamp((1 - Factor) * 2, 0, 1)) * (flip ? 1 : -1) * MathHelper.Pi;
        public override bool Attacktive => true;
        public float dist;
        public UltraSwoosh[] ultras = new UltraSwoosh[3];
        public override void OnStartSingle()
        {
            Vector2 tarVec = Owner switch
            {
                Player plr => plr.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition,
                _ => default
            };
            dist = (tarVec - Owner.Center).Length();
            if (dist < 100) dist = 100;
            KValue = Main.rand.NextFloat(1, 2);
            flip = Main.rand.NextBool();
            var verS = standardInfo.vertexStandard;
            for (int n = 0; n < 3; n++)
            {
                var pair = verS.swooshTexIndex ?? (3, 7);
                ultras[n] = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, timerMax, heat: verS.heatMap, _aniIndex: pair.Item1, _baseIndex: pair.Item2, colorVec: verS.colorVec);
                ultras[n].autoUpdate = false;
                ultras[n].timeLeft = 1;
                ultras[n].ApplyStdValueToVtxEffect(standardInfo);
            }
            if (verS.renderInfos == null)
                ultras[0].ResetAllRenderInfo();
            else
            {
                ultras[0].ModityAllRenderInfo(verS.renderInfos);
            }
            SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);

            base.OnStartSingle();
        }
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            var origf = fTimer;
            IEnumerable<CustomVertexInfo> result = [];
            for (int n = 0; n < 3; n++)
            {
                var origf_s = fTimer;

                fTimer += timerMax / 4f * n;
                float alphaG = 1 - n / 3f;
                for (int i = 0; i < 10; i++)
                {
                    float alphaS = 1f - i / 10f;
                    float alphaH = i == 0 ? 1f : .5f;
                    float alphaT = MathHelper.Clamp((1 - Factor) * 2, 0, 1);
                    alphaT = MathHelper.Clamp(alphaT * (1 - alphaT) * 8, 0, 1);
                    result = result.Concat(base.GetWeaponVertex(texture, alphaG * alphaS * alphaH * alphaT));
                    fTimer += .2f;
                }
                fTimer = origf_s;
            }
            result = result.Reverse();
            fTimer = origf;
            return result.ToArray();
        }

        public override void Update(bool triggered)
        {
            if (timer == timerMax)
            {
                for (int n = 0; n < 3; n++)
                    ultras[n].timeLeft = ultras[n].timeLeftMax = timerMax;
            }
            timer--;
            var origf = fTimer;
            for (int n = 0; n < 3; n++)
            {
                var origf_s = fTimer;
                fTimer += timerMax / 4f * n;
                float alphaG = 1 - MathF.Pow(n / 3f,4);
                UltraSwoosh u = ultras[n];
                u.timeLeft--;
                u.center = Owner.Center + Rotation.ToRotationVector2() * dist * .5f;
                var vertex = u.VertexInfos;
                for (int i = 0; i < 45; i++)
                {
                    float alphaT = MathHelper.Clamp((1 - Factor) * 2, 0, 1);
                    alphaT = MathHelper.Clamp(alphaT * (1 - alphaT) * 8, 0, 1);
                    var curTex = base.GetWeaponVertex(TextureAssets.Item[standardInfo.itemType].Value, 1f);
                    var f = i / 44f;
                    var realColor = standardInfo.standardColor;

                    realColor.A = (byte)MathHelper.Clamp(f.HillFactor2(1) * 640 * alphaT * alphaG, 0, 255);
                    vertex[2 * i] = new CustomVertexInfo(curTex[4].Position, realColor, new Vector3(f, 1, 1));
                    vertex[2 * i + 1] = new CustomVertexInfo(curTex[0].Position, realColor, new Vector3(0, 0, 1));

                    fTimer += .2f;
                }
                fTimer = origf_s;
            }
            fTimer = origf;
        }
        public override bool Collide(Rectangle rectangle)
        {
            var origf_s = fTimer;
            for (int n = 0; n < 3; n++)
            {
                if (base.Collide(rectangle))
                {
                    fTimer = origf_s;
                    Projectile.localNPCHitCooldown = Math.Clamp(timerMax / 6, 1, 514);
                    if (Owner is Player plr && Main.rand.NextBool(5))
                    {
                        Vector2 orig = plr.Center;
                        plr.Center += offsetCenter;
                        plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
                        plr.Center = orig;
                        if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                        }
                    }
                    return true;
                }
                fTimer += timerMax / 4f;
            }
            fTimer = origf_s;
            return false;
        }
        public override void OnDeactive()
        {
            foreach (var u in ultras)
            {
                if (u != null)
                    u.timeLeft = 0;
            }
            base.OnDeactive();
        }
    }
    /// <summary>
    /// 泰拉棱镜???!!!
    /// </summary>
    public class TerraprismaInfo : MeleeAction
    {
        public override Vector2 offsetCenter => realCenter - Owner.Center;
        public override float offsetRotation => realRotation;
        public override bool Attacktive => target != null;
        public override bool Collide(Rectangle rectangle)
        {
            Projectile.localNPCHitCooldown = 15;

            for (int n = 0; n < 9; n++)
            {
                float point1 = 0f;
                float sc = 1;
                if (Owner is Player plr)
                    sc = plr.GetAdjustedItemScale(plr.HeldItem);
                var flag = Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), oldCenters[n],
                        oldCenters[n] + oldRotations[n].ToRotationVector2() * standardInfo.vertexStandard.scaler * offsetSize * sc, 48f, ref point1);
                if (flag)
                {
                    if (Owner is Player player && Main.rand.NextBool(5))
                    {
                        Vector2 orig = player.Center;
                        player.Center = realCenter;
                        player.ItemCheck_Shoot(player.whoAmI, player.HeldItem, (int)(ModifyData.actionOffsetDamage * player.GetWeaponDamage(player.HeldItem)));
                        player.Center = orig;
                        if (Main.myPlayer == player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            SyncPlayerPosition.Get(player.whoAmI, player.position).Send(-1, player.whoAmI);
                        }
                    }
                    return true;

                }
            }

            return false;
        }
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            IEnumerable<CustomVertexInfo> result = [];
            Vector2 finalOrigin = offsetOrigin + standardInfo.standardOrigin;
            float k = MathF.Pow(Factor, 0.5f);
            for (int n = 20; n >= 0; n -= 5)
            {
                float sc = 1;
                if (Owner is Player plr)
                    sc = plr.GetAdjustedItemScale(plr.HeldItem);
                var currentVertex = DrawingMethods.GetItemVertexes(finalOrigin, oldRotations[n] + standardInfo.standardRotation, Rotation, texture, KValue, offsetSize * ModifyData.actionOffsetSize * sc, oldCenters[n], !flip, (n == 0 ? 1 : (45f - n) / 90f) * k, standardInfo.frame);
                result = result.Concat(currentVertex);
            }
            return result.ToArray();
        }
        public NPC target;
        public Vector2 realCenter;
        public Vector2 assistVelocity;
        public float[] assistParas = new float[2];
        public float realRotation;
        public Vector2[] oldCenters = new Vector2[45];
        public float[] oldRotations = new float[45];
        public UltraSwoosh ultra;
        void FindTarget()
        {
            foreach (var npc in Main.npc)
            {
                if (npc.CanBeChasedBy() && !npc.friendly && Vector2.Distance(npc.Center, Owner.Center) < 1024f)
                {
                    target = npc;
                    break;
                }
            }
        }
        public override void OnStartSingle()
        {
            var verS = standardInfo.vertexStandard;
            var pair = verS.swooshTexIndex ?? (3, 7);

            ultra = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, timerMax, heat: verS.heatMap, _aniIndex: pair.Item1, _baseIndex: pair.Item2, colorVec: verS.colorVec);
            ultra.autoUpdate = false;
            ultra.timeLeft = 1;
            ultra.ApplyStdValueToVtxEffect(standardInfo);
            if (verS.renderInfos == null)
                ultra.ResetAllRenderInfo();
            else
            {
                ultra.ModityAllRenderInfo(verS.renderInfos);
            }
            base.OnStartSingle();
            Rotation = 0;
            realRotation = MathHelper.PiOver2;
            Projectile.ownerHitCheck = false;
            assistParas = new float[5];
            realCenter = Owner.Center;
        }
        public override void Update(bool triggered)
        {
            var verS = standardInfo.vertexStandard;
            if (Owner is Player plr)
                plr.direction = Math.Sign(plr.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition.X - plr.Center.X);
            if (target == null || !target.CanBeChasedBy())
            {
                FindTarget();
                if (!triggered)
                    timer--;
                flip = Owner.direction == -1;
                realCenter = realCenter.MoveTowards(Owner.Center + new Vector2(-32 * Owner.direction, -32), 32);
                realRotation = realRotation.AngleLerp(MathHelper.Pi * (flip ? 0.4f : 0.6f), 0.2f);
            }
            else
            {
                fTimer -= .125f;
                assistParas[0]++;
                if (assistParas[0] < 10)
                {

                    assistVelocity = (realCenter - target.Center).SafeNormalize(default) * 2;
                    realRotation = realRotation.AngleLerp((-assistVelocity).ToRotation(), 0.2f);
                }
                else
                {
                    if (assistParas[0] < 30)
                    {
                        if (assistParas[0] == 10)
                        {
                            assistParas[1] = realCenter.X;
                            assistParas[2] = realCenter.Y;
                            assistVelocity = default;
                            realRotation = (target.Center - realCenter).ToRotation();
                        }
                        realCenter = Vector2.Lerp(new Vector2(assistParas[1], assistParas[2]), target.Center, MathHelper.SmoothStep(0, 1.2f, Utils.GetLerpValue(10, 30, assistParas[0])));
                    }
                    else
                    {
                        if (assistParas[0] == 30)
                        {
                            assistParas[1] = realCenter.X;
                            assistParas[2] = realCenter.Y;
                            assistVelocity = default;
                        }
                        assistVelocity = (realCenter - target.Center).SafeNormalize(default) * 2;
                        //realCenter = Vector2.Lerp(new Vector2(assistParas[1], assistParas[2]), target.Center, MathHelper.SmoothStep(0, 7f, Utils.GetLerpValue(30, 60, assistParas[0])));
                        realRotation += MathHelper.Clamp((assistParas[0] - 30) / 15, 0, 1) * (.5f - MathF.Cos(realRotation - assistVelocity.ToRotation()) * .2f) * (flip ? -1 : 1);
                        float sc = 1;
                        if (Owner is Player plr2)
                            sc = plr2.GetAdjustedItemScale(plr2.HeldItem);
                        if (Vector2.Distance(target.Center, realCenter) > verS.scaler * offsetSize * sc)
                            assistParas[0] = 9;
                    }
                }
                realCenter += assistVelocity;
            }
            if (oldCenters[0] == default)
            {
                Array.Fill(oldCenters, realCenter);
                Array.Fill(oldRotations, realRotation);
            }
            else
            {
                for (int n = 44; n > 0; n--)
                {
                    oldCenters[n] = oldCenters[n - 1];
                    oldRotations[n] = oldRotations[n - 1];
                }
                oldCenters[0] = realCenter;
                oldRotations[0] = realRotation;
            }
            ultra.timeLeftMax = timerMax;
            ultra.timeLeft = (int)(timerMax * Math.Pow(Factor,0.25));
            ultra.center = Owner.Center;;

            var vertex = ultra.VertexInfos;
            for (int i = 0; i < 45; i++)
            {
                var f = i / 44f;
                var realColor = standardInfo.standardColor;
                realColor.A = (byte)(f.HillFactor2(1) * 255);//96
                vertex[2 * i] = new CustomVertexInfo(oldCenters[i] + verS.scaler * oldRotations[i].ToRotationVector2() * offsetSize * ModifyData.actionOffsetSize + Main.rand.NextVector2Unit() * (i / 4f), realColor, new Vector3(f, 1, 1));
                vertex[2 * i + 1] = new CustomVertexInfo(oldCenters[i] + Main.rand.NextVector2Unit() * (i / 4f), realColor, new Vector3(0, 0, 1));
            }

            //Main.NewText(ultra.Active);
            //Main.NewText(LogSpiralLibrarySystem.vertexEffects.Contains(ultra));
        }
        public override void OnEndSingle()
        {
            target = null;
            //realCenter = default;
            assistVelocity = default;
            realRotation = default;
            Array.Clear(oldCenters);
            Array.Clear(oldRotations);
            Array.Clear(assistParas);
            ultra.timeLeft = -1;
            ultra = null;
            //Array.Clear(LogSpiralLibrarySystem.vertexEffects);
            Projectile.ownerHitCheck = true;
            base.OnEndSingle();
        }
        public override void OnDeactive()
        {
            if (ultra != null)
            {
                ultra.timeLeft = 0;
                ultra = null;
            }
            base.OnDeactive();
        }
    }
}

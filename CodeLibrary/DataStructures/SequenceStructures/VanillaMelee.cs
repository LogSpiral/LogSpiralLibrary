﻿using static Humanizer.In;
using System;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{
    //原版手持弹幕近战复刻总集篇！！
    /// <summary>
    /// 经典宽剑
    /// </summary>
    public class BoardSwordInfo : NormalAttackAction
    {
        public override float offsetRotation => base.offsetRotation;
        public override bool Attacktive => base.Attacktive;
        public override void Update()
        {
            switch (Owner)
            {
                case Player player:
                    {
                        player.itemTime = 2;
                        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, CompositeArmRotation);
                        break;
                    }
            }
            base.Update();
        }
    }
    /// <summary>
    /// 经典短剑
    /// </summary>
    public class ShortSwordInfo : NormalAttackAction
    {
        public override Vector2 offsetOrigin => base.offsetOrigin;
    }
    /// <summary>
    /// 转啊转
    /// </summary>
    public class BoomerangInfo : NormalAttackAction
    {
        public override Vector2 offsetCenter => base.offsetCenter;
        public override void OnStartSingle()
        {
            base.OnStartSingle();
        }
        public override void Update()
        {
            if (Factor <= .5f) 
            {

            }
            base.Update();
        }
    }
    /// <summary>
    /// 链球
    /// </summary>
    public class FlailInfo : NormalAttackAction
    {
    }
    /// <summary>
    /// 长枪
    /// </summary>
    public class SpearInfo : NormalAttackAction
    {
    }
    /// <summary>
    /// 石巨人之拳！！
    /// </summary>
    public class FistInfo : NormalAttackAction
    {
    }
    /// <summary>
    /// 圣骑士会个🔨
    /// </summary>
    public class HammerInfo : NormalAttackAction
    {
    }
    /// <summary>
    /// ─━╋ ─━╋ ─━╋
    /// </summary>
    public class KnivesInfo : NormalAttackAction
    {
    }
    /// <summary>
    /// 白云一片去悠悠
    /// </summary>
    public class YoyoInfo : NormalAttackAction
    {
        public override void Update()
        {
            Rotation += .45f;
            base.Update();
        }
        public override bool Attacktive => Factor > 0.05f;
        public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            base.Draw(spriteBatch, texture);
            Projectile.aiStyle = 99;
            Main.instance.DrawProj_DrawYoyoString(Projectile,Owner.Center);
            Projectile.aiStyle = -1;
        }
        //以下是原版Yoyo实现的ai的注释
        //private void AI_099_2()
        //{
        //    #region 检查是否为第二个
        //    bool flag = false;//或许是用来检查是不是第二个悠悠
        //    for (int i = 0; i < whoAmI; i++)
        //    {
        //        if (Main.projectile[i].active && Main.projectile[i].owner == owner && Main.projectile[i].type == type)
        //            flag = true;
        //    }
        //    #endregion
        //    #region 计时器
        //    if (owner == Main.myPlayer)
        //    {
        //        localAI[0] += 1f;
        //        if (flag)
        //            localAI[0] += (float)Main.rand.Next(10, 31) * 0.1f;//如果是第二个，这个增长会快很多

        //        float num = localAI[0] / 60f;

        //        /*
        //        num /= (1f + Main.player[owner].meleeSpeed) / 2f;
        //        */
        //        num /= (1f + Main.player[owner].inverseMeleeSpeed) / 2f;
        //        float num2 = ProjectileID.Sets.YoyosLifeTimeMultiplier[type];
        //        if (num2 != -1f && num > num2)
        //            ai[0] = -1f;//也许是收回的意思？
        //    }
        //    #endregion
        //    #region 泰拉球弹幕生成
        //    if (type == 603 && owner == Main.myPlayer)
        //    {
        //        localAI[1] += 1f;
        //        if (localAI[1] >= 6f)
        //        {
        //            float num3 = 400f;
        //            Vector2 vector = velocity;
        //            Vector2 vector2 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
        //            vector2.Normalize();
        //            vector2 *= (float)Main.rand.Next(10, 41) * 0.1f;
        //            if (Main.rand.Next(3) == 0)
        //                vector2 *= 2f;

        //            vector *= 0.25f;
        //            vector += vector2;
        //            for (int j = 0; j < 200; j++)
        //            {
        //                if (Main.npc[j].CanBeChasedBy(this))
        //                {
        //                    float num4 = Main.npc[j].position.X + (float)(Main.npc[j].width / 2);
        //                    float num5 = Main.npc[j].position.Y + (float)(Main.npc[j].height / 2);
        //                    float num6 = Math.Abs(position.X + (float)(width / 2) - num4) + Math.Abs(position.Y + (float)(height / 2) - num5);
        //                    if (num6 < num3 && Collision.CanHit(position, width, height, Main.npc[j].position, Main.npc[j].width, Main.npc[j].height))
        //                    {
        //                        num3 = num6;
        //                        vector.X = num4;
        //                        vector.Y = num5;
        //                        vector -= base.Center;
        //                        vector.Normalize();
        //                        vector *= 8f;
        //                    }
        //                }
        //            }

        //            vector *= 0.8f;
        //            NewProjectile(GetProjectileSource_FromThis(), base.Center.X - vector.X, base.Center.Y - vector.Y, vector.X, vector.Y, 604, damage, knockBack, owner);
        //            localAI[1] = 0f;
        //        }
        //    }
        //    #endregion
        //    #region 玩家检测更新等
        //    bool flag2 = false;
        //    if (type >= 556 && type <= 561)
        //        flag2 = true;

        //    if (Main.player[owner].dead)
        //    {
        //        Kill();
        //        return;
        //    }

        //    if (!flag2 && !flag)
        //    {
        //        Main.player[owner].heldProj = whoAmI;
        //        Main.player[owner].SetDummyItemTime(2);
        //        if (position.X + (float)(width / 2) > Main.player[owner].position.X + (float)(Main.player[owner].width / 2))
        //        {
        //            Main.player[owner].ChangeDir(1);
        //            direction = 1;
        //        }
        //        else
        //        {
        //            Main.player[owner].ChangeDir(-1);
        //            direction = -1;
        //        }
        //    }

        //    if (velocity.HasNaNs())
        //        Kill();

        //    timeLeft = 6;
        //    #endregion

        //    #region 基本参量
        //    float num7 = 10f;
        //    float yoYoSpeed = 10f;
        //    float num9 = 3f;
        //    float yoYoLength = 200f;
        //    yoYoLength = ProjectileID.Sets.YoyosMaximumRange[type];
        //    yoYoSpeed = ProjectileID.Sets.YoyosTopSpeed[type];
        //    #endregion

        //    #region 火焰悠悠粒子生成
        //    if (type == 545)
        //    {
        //        if (Main.rand.Next(6) == 0)
        //        {
        //            int num11 = Dust.NewDust(position, width, height, 6);
        //            Main.dust[num11].noGravity = true;
        //        }
        //    }
        //    else if (type == 553 && Main.rand.Next(2) == 0)
        //    {
        //        int num12 = Dust.NewDust(position, width, height, 6);
        //        Main.dust[num12].noGravity = true;
        //        Main.dust[num12].scale = 1.6f;
        //    }
        //    #endregion


        //    if (Main.player[owner].yoyoString)
        //        yoYoLength = yoYoLength * 1.25f + 30f;

        //    /*
        //    num10 /= (1f + Main.player[owner].meleeSpeed * 3f) / 4f;
        //    num8 /= (1f + Main.player[owner].meleeSpeed * 3f) / 4f;
        //    */
        //    yoYoLength /= (1f + Main.player[owner].inverseMeleeSpeed * 3f) / 4f;
        //    yoYoSpeed /= (1f + Main.player[owner].inverseMeleeSpeed * 3f) / 4f;
        //    num7 = 14f - yoYoSpeed / 2f;
        //    if (num7 < 1f)
        //        num7 = 1f;

        //    // Yoyos with effective top speed (boosted by melee speed) num8 > 26 will set num11 to be less than 1.
        //    // This breaks the AI's acceleration vector math and stops the velocity from being correctly capped every frame.
        //    // Providing a minimum value of 1.01 to num11 fixes this, allowing for very fast modded yoyos.
        //    // See issue #751 for more details.
        //    if (num7 < 1.01f)
        //        num7 = 1.01f;

        //    num9 = 5f + yoYoSpeed / 2f;
        //    if (flag)
        //        num9 += 20f;

        //    if (ai[0] >= 0f)
        //    {
        //        //超过最大速度就限速
        //        if (velocity.Length() > yoYoSpeed)
        //            velocity *= 0.98f;

        //        bool overLength = false;
        //        bool overLenthEX = false;
        //        Vector2 vector3 = Main.player[owner].Center - base.Center;
        //        if (vector3.Length() > yoYoLength)
        //        {
        //            overLength = true;
        //            if ((double)vector3.Length() > (double)yoYoLength * 1.3)
        //                overLenthEX = true;
        //        }

        //        if (owner == Main.myPlayer)
        //        {
        //            if (!Main.player[owner].channel || Main.player[owner].stoned || Main.player[owner].frozen)
        //            {
        //                ai[0] = -1f;
        //                ai[1] = 0f;
        //                netUpdate = true;
        //            }
        //            else
        //            {
        //                Vector2 vector4 = Main.ReverseGravitySupport(Main.MouseScreen) + Main.screenPosition;
        //                float x = vector4.X;
        //                float y = vector4.Y;
        //                Vector2 vector5 = new Vector2(x, y) - Main.player[owner].Center;
        //                if (vector5.Length() > yoYoLength)
        //                {
        //                    vector5.Normalize();
        //                    vector5 *= yoYoLength;
        //                    vector5 = Main.player[owner].Center + vector5;
        //                    x = vector5.X;
        //                    y = vector5.Y;
        //                }

        //                if (ai[0] != x || ai[1] != y)
        //                {
        //                    Vector2 vector6 = new Vector2(x, y) - Main.player[owner].Center;
        //                    if (vector6.Length() > yoYoLength - 1f)
        //                    {
        //                        vector6.Normalize();
        //                        vector6 *= yoYoLength - 1f;
        //                        Vector2 vector7 = Main.player[owner].Center + vector6;
        //                        x = vector7.X;
        //                        y = vector7.Y;
        //                    }

        //                    ai[0] = x;
        //                    ai[1] = y;
        //                    netUpdate = true;
        //                }
        //            }
        //        }

        //        if (overLenthEX && owner == Main.myPlayer)
        //        {
        //            ai[0] = -1f;
        //            netUpdate = true;
        //        }

        //        if (ai[0] >= 0f)
        //        {
        //            //边界检测以限速
        //            if (overLength)
        //            {
        //                num7 /= 2f;
        //                yoYoSpeed *= 2f;
        //                if (base.Center.X > Main.player[owner].Center.X && velocity.X > 0f)
        //                    velocity.X *= 0.5f;

        //                if (base.Center.Y > Main.player[owner].Center.Y && velocity.Y > 0f)
        //                    velocity.Y *= 0.5f;

        //                if (base.Center.X < Main.player[owner].Center.X && velocity.X < 0f)
        //                    velocity.X *= 0.5f;

        //                if (base.Center.Y < Main.player[owner].Center.Y && velocity.Y < 0f)
        //                    velocity.Y *= 0.5f;
        //            }

        //            Vector2 vector8 = new Vector2(ai[0], ai[1]) - base.Center;
        //            if (overLength)
        //                num7 = 1f;

        //            velocity.Length();
        //            float num13 = vector8.Length();
        //            if (num13 > num9)
        //            {
        //                vector8.Normalize();
        //                float num14 = Math.Min(num13 / 2f, yoYoSpeed);
        //                if (overLength)
        //                    num14 = Math.Min(num14, yoYoSpeed / 2f);

        //                vector8 *= num14;
        //                velocity = (velocity * (num7 - 1f) + vector8) / num7;
        //            }
        //            else if (flag)
        //            {
        //                if ((double)velocity.Length() < (double)yoYoSpeed * 0.6)
        //                {
        //                    vector8 = velocity;
        //                    vector8.Normalize();
        //                    vector8 *= yoYoSpeed * 0.6f;
        //                    velocity = (velocity * (num7 - 1f) + vector8) / num7;
        //                }
        //            }
        //            else
        //            {
        //                velocity *= 0.8f;
        //            }

        //            if (flag && !overLength && (double)velocity.Length() < (double)yoYoSpeed * 0.6)
        //            {
        //                velocity.Normalize();
        //                velocity *= yoYoSpeed * 0.6f;
        //            }
        //        }
        //    }
        //    #region 收回
        //    else
        //    {
        //        num7 = (int)((double)num7 * 0.8);
        //        yoYoSpeed *= 1.5f;
        //        tileCollide = false;
        //        Vector2 vector9 = Main.player[owner].Center - base.Center;
        //        float num15 = vector9.Length();
        //        if (num15 < yoYoSpeed + 10f || num15 == 0f || num15 > 2000f)
        //        {
        //            Kill();
        //        }
        //        else
        //        {
        //            vector9.Normalize();
        //            vector9 *= yoYoSpeed;
        //            velocity = (velocity * (num7 - 1f) + vector9) / num7;
        //        }
        //    }
        //    #endregion


        //    rotation += 0.45f;
        //}
    }
    /// <summary>
    /// 我没拿到真空刀
    /// </summary>
    public class ArkhalisInfo : NormalAttackAction
    {

    }
    /// <summary>
    /// 请不要再冕了...什么不是烈冕号啊
    /// </summary>
    public class EruptionInfo : NormalAttackAction
    {
        public override float offsetSize => base.offsetSize;
        public override float offsetRotation => base.offsetRotation;

    }
    /// <summary>
    /// 其实是天龙之怒
    /// </summary>
    public class RotatingInfo : NormalAttackAction
    {
        public override float offsetRotation => Factor * MathHelper.TwoPi * 2;
        public override bool Attacktive => true;
    }
    /// <summary>
    /// Lancer!!♠
    /// </summary>
    public class LanceInfo : NormalAttackAction
    {

    }
    /// <summary>
    /// 银色战车！！
    /// </summary>
    public class StarlightInfo : NormalAttackAction
    {
    }
    /// <summary>
    /// 天顶
    /// </summary>
    public class ZenithInfo : NormalAttackAction
    {

    }
    /// <summary>
    /// 泰拉棱镜???!!!
    /// </summary>
    public class TerraprismaInfo : NormalAttackAction
    {

    }
}

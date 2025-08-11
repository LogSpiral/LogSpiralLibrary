using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using Terraria.ID;

namespace LogSpiralLibrary.CodeLibrary;

/// <summary>
/// 这个类单纯存了些原版有的函数的注释解析之类，不会被调用到，不注释掉单纯是为了高亮看着舒服
/// </summary>
static class VanillaCodeRef
{
    //附录-原版链球ID详解
    public static void AI_015_Flails(this Projectile projectile)
    {
        Player player = Main.player[projectile.owner];
        if (!player.active || player.dead || player.noItems || player.CCed || Vector2.Distance(projectile.Center, player.Center) > 900f)//发射弹幕的玩家不满足条件时消失
        {
            projectile.Kill();
            return;
        }
        
        if (Main.myPlayer == projectile.owner && Main.mapFullscreen)//打开大地图时消失
        {
            projectile.Kill();
            return;
        }

        if (projectile.type == ProjectileID.FlamingMace && projectile.wet && !projectile.lavaWet)//如果是火链球，在水里变成普通链球！
        {
            projectile.type = ProjectileID.Mace;
            projectile.netUpdate = true;
        }

        Vector2 mountedCenter = player.MountedCenter;
        bool useRotating = true;
        bool hitCheck = false;
        int timeMax = 10;
        float throwingSpeed = 24f;
        float rangeMax = 800f;
        float moveSpeedMax = 3f;
        float pickupRange = 16f;
        float moveSpeedMax2 = 6f;
        float pickupRange2 = 48f;
        float num8 = 1f;
        float num9 = 14f;
        int num10 = 60;
        int num11 = 10;
        int hitCoolDownRotating = 15;
        int hitCoolDownThrown = 10;
        int throwingTimeMaxForOldVersion = timeMax + 5;
        switch (projectile.type)
        {
            case ProjectileID.Mace://链锤 
            case ProjectileID.FlamingMace://烧！
                timeMax = 13;
                throwingSpeed = 12f;
                pickupRange = 8f;
                pickupRange2 = 13f;
                break;
            case ProjectileID.BallOHurt://链球 对的就是叫这个，腐化的
                timeMax = 15;
                throwingSpeed = 14f;
                pickupRange = 10f;
                pickupRange2 = 15f;
                break;
            case ProjectileID.TheMeatball://肉丸(不是 猩红那边的链球
                timeMax = 15;
                throwingSpeed = 15f;
                pickupRange = 11f;
                pickupRange2 = 16f;
                break;
            case ProjectileID.BlueMoon://贪玩蓝月
                timeMax = 15;
                throwingSpeed = 16f;
                pickupRange = 12f;
                pickupRange2 = 16f;
                break;
            case ProjectileID.Sunfury://阳炎之怒
                timeMax = 15;
                throwingSpeed = 17f;
                pickupRange = 14f;
                pickupRange2 = 18f;
                break;
            case ProjectileID.TheDaoofPow://你不准道观
                timeMax = 13;
                throwingSpeed = 21f;
                pickupRange = 20f;
                pickupRange2 = 24f;
                hitCoolDownRotating = 12;
                break;
            case ProjectileID.DripplerFlail://血月滴滴怪链球
                timeMax = 13;
                throwingSpeed = 22f;
                pickupRange = 22f;
                pickupRange2 = 26f;
                hitCoolDownRotating = 12;
                break;
            case ProjectileID.FlowerPow://花的力量！
                timeMax = 13;
                throwingSpeed = 23f;
                hitCoolDownRotating = 12;
                break;
        }

        /*
        float meleeSpeed = player.meleeSpeed;
        float num15 = 1f / meleeSpeed;
        */
        float num15 = 1f / player.inverseMeleeSpeed;//我不知道为什么不直接用player.GetTotalAttackSpeed(DamageClass.Melee)
        throwingSpeed *= num15;
        num8 *= num15;
        num9 *= num15;
        moveSpeedMax *= num15;
        pickupRange *= num15;
        moveSpeedMax2 *= num15;
        pickupRange2 *= num15;
        float num16 = throwingSpeed * (float)timeMax;
        float stayRange = num16 + 160f;
        projectile.localNPCHitCooldown = num11;
        switch ((int)projectile.ai[0])
        {
            //状态零 旋转
            case 0:
                {
                    hitCheck = true;
                    if (projectile.owner == Main.myPlayer)
                    {
                        Vector2 origin = mountedCenter;
                        Vector2 mouseWorld = Main.MouseWorld;
                        Vector2 vector3 = origin.DirectionTo(mouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
                        player.ChangeDir((vector3.X > 0f) ? 1 : (-1));
                        //如果没蓄力就掷出
                        if (!player.channel)
                        {
                            projectile.ai[0] = 1f;
                            projectile.ai[1] = 0f;
                            projectile.velocity = vector3 * throwingSpeed + player.velocity;
                            projectile.Center = mountedCenter;
                            projectile.netUpdate = true;
                            projectile.ResetLocalNPCHitImmunity();
                            projectile.localNPCHitCooldown = hitCoolDownThrown;//将攻击冷却改为掷出模式
                            break;
                        }
                    }

                    projectile.localAI[1] += 1f;
                    Vector2 vector4 = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (projectile.localAI[1] / 60f) * (float)player.direction);
                    vector4.Y *= 0.8f;
                    if (vector4.Y * player.gravDir > 0f)
                        vector4.Y *= 0.5f;

                    projectile.Center = mountedCenter + vector4 * 30f;//实现旋转动画
                    projectile.velocity = Vector2.Zero;//居然直接把弹幕速度设置为0了？
                    projectile.localNPCHitCooldown = hitCoolDownRotating;
                    break;
                }
            //状态一 掷出
            case 1:
                {
                    bool flag3 = projectile.ai[1]++ >= (float)timeMax;
                    flag3 |= projectile.Distance(mountedCenter) >= rangeMax;//时间或者范围超出限制
                    if (player.controlUseItem)//左键单击，切换至滞留状态
                    {
                        projectile.ai[0] = 6f;
                        projectile.ai[1] = 0f;
                        projectile.netUpdate = true;
                        projectile.velocity *= 0.2f;
                        if (Main.myPlayer == projectile.owner && projectile.type == ProjectileID.DripplerFlail)//血月链球的额外弹幕
                            Projectile.NewProjectile(projectile.GetProjectileSource_FromThis(), projectile.Center, projectile.velocity, ProjectileID.DripplerFlailExtraBall, projectile.damage, projectile.knockBack, Main.myPlayer);

                        break;
                    }

                    if (flag3)
                    {
                        projectile.ai[0] = 2f;
                        projectile.ai[1] = 0f;
                        projectile.netUpdate = true;
                        projectile.velocity *= 0.3f;
                        if (Main.myPlayer == projectile.owner && projectile.type == 757)
                            Projectile.NewProjectile(projectile.GetProjectileSource_FromThis(), projectile.Center, projectile.velocity, 928, projectile.damage, projectile.knockBack, Main.myPlayer);
                    }

                    player.ChangeDir((player.Center.X < projectile.Center.X) ? 1 : (-1));
                    projectile.localNPCHitCooldown = hitCoolDownThrown;
                    break;
                }
            //状态二 回收
            case 2:
                {
                    Vector2 vector = projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                    if (projectile.Distance(mountedCenter) <= pickupRange)
                    {
                        projectile.Kill();
                        return;
                    }

                    if (player.controlUseItem)//回收过程中如果按下左键切换至滞留模式
                    {
                        projectile.ai[0] = 6f;
                        projectile.ai[1] = 0f;
                        projectile.netUpdate = true;
                        projectile.velocity *= 0.2f;
                    }
                    else
                    {
                        projectile.velocity *= 0.98f;
                        projectile.velocity = projectile.velocity.MoveTowards(vector * pickupRange, moveSpeedMax);
                        player.ChangeDir((player.Center.X < projectile.Center.X) ? 1 : (-1));
                    }

                    break;
                }
            //状态三 我不到阿
            case 3:
                {
                    if (!player.controlUseItem)
                    {
                        projectile.ai[0] = 4f;
                        projectile.ai[1] = 0f;
                        projectile.netUpdate = true;
                        break;
                    }

                    float num18 = projectile.Distance(mountedCenter);
                    projectile.tileCollide = projectile.ai[1] == 1f;
                    bool flag4 = num18 <= num16;
                    if (flag4 != projectile.tileCollide)
                    {
                        projectile.tileCollide = flag4;
                        projectile.ai[1] = (projectile.tileCollide ? 1 : 0);
                        projectile.netUpdate = true;
                    }

                    if (num18 > (float)num10)
                    {
                        if (num18 >= num16)
                        {
                            projectile.velocity *= 0.5f;
                            projectile.velocity = projectile.velocity.MoveTowards(projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * num9, num9);
                        }

                        projectile.velocity *= 0.98f;
                        projectile.velocity = projectile.velocity.MoveTowards(projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * num9, num8);
                    }
                    else
                    {
                        if (projectile.velocity.Length() < 6f)
                        {
                            projectile.velocity.X *= 0.96f;
                            projectile.velocity.Y += 0.2f;
                        }

                        if (player.velocity.X == 0f)
                            projectile.velocity.X *= 0.96f;
                    }

                    player.ChangeDir((player.Center.X < projectile.Center.X) ? 1 : (-1));
                    break;
                }
            //状态四 回收二 和第一个回收不同的区别是不能切换至滞留
            case 4:
                {
                    projectile.tileCollide = false;
                    Vector2 vector2 = projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                    if (projectile.Distance(mountedCenter) <= pickupRange2)
                    {
                        projectile.Kill();
                        return;
                    }

                    projectile.velocity *= 0.98f;
                    projectile.velocity = projectile.velocity.MoveTowards(vector2 * pickupRange2, moveSpeedMax2);
                    Vector2 target = projectile.Center + projectile.velocity;
                    Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
                    if (Vector2.Dot(vector2, value) < 0f)
                    {
                        projectile.Kill();
                        return;
                    }

                    player.ChangeDir((player.Center.X < projectile.Center.X) ? 1 : (-1));
                    break;
                }
            //状态五 旧版链球
            case 5:
                if (projectile.ai[1]++ >= (float)throwingTimeMaxForOldVersion)
                {
                    projectile.ai[0] = 6f;
                    projectile.ai[1] = 0f;
                    projectile.netUpdate = true;
                }
                else
                {
                    projectile.localNPCHitCooldown = hitCoolDownThrown;
                    projectile.velocity.Y += 0.6f;
                    projectile.velocity.X *= 0.95f;
                    player.ChangeDir((player.Center.X < projectile.Center.X) ? 1 : (-1));
                }
                break;
            //状态六 滞留
            case 6:
                if (!player.controlUseItem || projectile.Distance(mountedCenter) > stayRange)
                {
                    projectile.ai[0] = 4f;
                    projectile.ai[1] = 0f;
                    projectile.netUpdate = true;
                    break;
                }
                if (!projectile.shimmerWet)
                    projectile.velocity.Y += 0.8f;
                projectile.velocity.X *= 0.95f;
                player.ChangeDir((player.Center.X < projectile.Center.X) ? 1 : (-1));
                break;
        }
        //如果是花之力就发射弹幕
        if (projectile.type == ProjectileID.FlowerPow)
        {
            useRotating = false;
            float num20 = (Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y)) * 0.01f;
            projectile.rotation += ((projectile.velocity.X > 0f) ? num20 : (0f - num20));
            if (projectile.ai[0] == 0f)
                projectile.rotation += (float)Math.PI * 2f / 15f * (float)player.direction;

            float num21 = 600f;
            NPC nPC = null;
            if (projectile.owner == Main.myPlayer)
            {
                projectile.localAI[0] += 1f;
                if (projectile.localAI[0] >= 20f)
                {
                    projectile.localAI[0] = 17f;
                    for (int i = 0; i < 200; i++)
                    {
                        NPC nPC2 = Main.npc[i];
                        if (nPC2.CanBeChasedBy(projectile))
                        {
                            float num22 = projectile.Distance(nPC2.Center);
                            if (!(num22 >= num21) && Collision.CanHit(projectile.position, projectile.width, projectile.height, nPC2.position, nPC2.width, nPC2.height))
                            {
                                nPC = nPC2;
                                num21 = num22;
                            }
                        }
                    }
                }

                if (nPC != null)
                {
                    projectile.localAI[0] = 0f;
                    float num23 = 14f;
                    Vector2 center = projectile.Center;
                    Vector2 vector5 = center.DirectionTo(nPC.Center) * num23;
                    Projectile.NewProjectile(projectile.GetProjectileSource_FromThis(), center, vector5, 248, (int)((double)projectile.damage / 1.5), projectile.knockBack / 2f, Main.myPlayer);
                }
            }
        }

        projectile.direction = ((projectile.velocity.X > 0f) ? 1 : (-1));
        projectile.spriteDirection = projectile.direction;
        projectile.ownerHitCheck = hitCheck;
        //链球本体的旋转动画效果，只有花之力不会转
        if (useRotating)
        {
            if (projectile.velocity.Length() > 1f)
                projectile.rotation = projectile.velocity.ToRotation() + projectile.velocity.X * 0.1f;
            else
                projectile.rotation += projectile.velocity.X * 0.1f;
        }

        projectile.timeLeft = 2;
        player.heldProj = projectile.whoAmI;
        player.SetDummyItemTime(2);
        player.itemRotation = projectile.DirectionFrom(mountedCenter).ToRotation();
        if (projectile.Center.X < mountedCenter.X)
            player.itemRotation += (float)Math.PI;

        player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
        //AI_015_Flails_Dust(doFastThrowDust);
    }

    //以下是原版Yoyo实现的ai的注释
    public static void AI_099_2(this Projectile projectile)
    {
        #region 检查是否为第二个
        bool flag = false;//或许是用来检查是不是第二个悠悠
        for (int i = 0; i < projectile.whoAmI; i++)
        {
            if (Main.projectile[i].active && Main.projectile[i].owner == projectile.owner && Main.projectile[i].type == projectile.type)
                flag = true;
        }
        #endregion
        #region 计时器
        if (projectile.owner == Main.myPlayer)
        {
            projectile.localAI[0] += 1f;
            if (flag)
                projectile.localAI[0] += (float)Main.rand.Next(10, 31) * 0.1f;//如果是第二个，这个增长会快很多

            float num = projectile.localAI[0] / 60f;

            /*
            num /= (1f + Main.player[projectile.owner].meleeSpeed) / 2f;
            */
            num /= (1f + Main.player[projectile.owner].inverseMeleeSpeed) / 2f;
            float num2 = ProjectileID.Sets.YoyosLifeTimeMultiplier[projectile.type];
            if (num2 != -1f && num > num2)
                projectile.ai[0] = -1f;//也许是收回的意思？
        }
        #endregion
        #region 泰拉球弹幕生成
        if (projectile.type == 603 && projectile.owner == Main.myPlayer)
        {
            projectile.localAI[1] += 1f;
            if (projectile.localAI[1] >= 6f)
            {
                float num3 = 400f;
                Vector2 vector = projectile.velocity;
                Vector2 vector2 = new(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                vector2.Normalize();
                vector2 *= (float)Main.rand.Next(10, 41) * 0.1f;
                if (Main.rand.NextBool(3))
                    vector2 *= 2f;

                vector *= 0.25f;
                vector += vector2;
                for (int j = 0; j < 200; j++)
                {
                    if (Main.npc[j].CanBeChasedBy(projectile))
                    {
                        float num4 = Main.npc[j].position.X + (float)(Main.npc[j].width / 2);
                        float num5 = Main.npc[j].position.Y + (float)(Main.npc[j].height / 2);
                        float num6 = Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num4) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num5);
                        if (num6 < num3 && Collision.CanHit(projectile.position, projectile.width, projectile.height, Main.npc[j].position, Main.npc[j].width, Main.npc[j].height))
                        {
                            num3 = num6;
                            vector.X = num4;
                            vector.Y = num5;
                            vector -= projectile.Center;
                            vector.Normalize();
                            vector *= 8f;
                        }
                    }
                }

                vector *= 0.8f;
                Projectile.NewProjectile(projectile.GetProjectileSource_FromThis(), projectile.Center.X - vector.X, projectile.Center.Y - vector.Y, vector.X, vector.Y, 604, projectile.damage, projectile.knockBack, projectile.owner);
                projectile.localAI[1] = 0f;
            }
        }
        #endregion
        #region 玩家检测更新等
        bool flag2 = false;
        if (projectile.type >= 556 && projectile.type <= 561)
            flag2 = true;

        if (Main.player[projectile.owner].dead)
        {
            projectile.Kill();
            return;
        }

        if (!flag2 && !flag)
        {
            Main.player[projectile.owner].heldProj = projectile.whoAmI;
            Main.player[projectile.owner].SetDummyItemTime(2);
            if (projectile.position.X + (float)(projectile.width / 2) > Main.player[projectile.owner].position.X + (float)(Main.player[projectile.owner].width / 2))
            {
                Main.player[projectile.owner].ChangeDir(1);
                projectile.direction = 1;
            }
            else
            {
                Main.player[projectile.owner].ChangeDir(-1);
                projectile.direction = -1;
            }
        }

        if (projectile.velocity.HasNaNs())
            projectile.Kill();

        projectile.timeLeft = 6;
        #endregion
        #region 基本参量
        float yoYoLength = ProjectileID.Sets.YoyosMaximumRange[projectile.type];
        float yoYoSpeed = ProjectileID.Sets.YoyosTopSpeed[projectile.type];
        #endregion

        #region 火焰悠悠粒子生成
        if (projectile.type == 545)
        {
            if (Main.rand.NextBool(6))
            {
                int num11 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Torch);
                Main.dust[num11].noGravity = true;
            }
        }
        else if (projectile.type == 553 && Main.rand.NextBool(2))
        {
            int num12 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Torch);
            Main.dust[num12].noGravity = true;
            Main.dust[num12].scale = 1.6f;
        }
        #endregion


        if (Main.player[projectile.owner].yoyoString)
            yoYoLength = yoYoLength * 1.25f + 30f;

        /*
        num10 /= (1f + Main.player[projectile.owner].meleeSpeed * 3f) / 4f;
        num8 /= (1f + Main.player[projectile.owner].meleeSpeed * 3f) / 4f;
        */
        yoYoLength /= (1f + Main.player[projectile.owner].inverseMeleeSpeed * 3f) / 4f;
        yoYoSpeed /= (1f + Main.player[projectile.owner].inverseMeleeSpeed * 3f) / 4f;
        float num7 = 14f - yoYoSpeed / 2f;
        if (num7 < 1f)
            num7 = 1f;

        // Yoyos with effective top speed (boosted by melee speed) num8 > 26 will set num11 to be less than 1.
        // This breaks the projectile.ai's acceleration vector math and stops the projectile.velocity from being correctly capped every frame.
        // Providing a minimum value of 1.01 to num11 fixes this, allowing for very fast modded yoyos.
        // See issue #751 for more details.
        if (num7 < 1.01f)
            num7 = 1.01f;

        float num9 = 5f + yoYoSpeed / 2f;
        if (flag)
            num9 += 20f;

        if (projectile.ai[0] >= 0f)
        {
            //超过最大速度就限速
            if (projectile.velocity.Length() > yoYoSpeed)
                projectile.velocity *= 0.98f;

            bool overLength = false;
            bool overLenthEX = false;
            Vector2 vector3 = Main.player[projectile.owner].Center - projectile.Center;
            if (vector3.Length() > yoYoLength)
            {
                overLength = true;
                if ((double)vector3.Length() > (double)yoYoLength * 1.3)
                    overLenthEX = true;
            }

            if (projectile.owner == Main.myPlayer)
            {
                if (!Main.player[projectile.owner].channel || Main.player[projectile.owner].stoned || Main.player[projectile.owner].frozen)
                {
                    projectile.ai[0] = -1f;
                    projectile.ai[1] = 0f;
                    projectile.netUpdate = true;
                }
                else
                {
                    Vector2 vector4 = Main.ReverseGravitySupport(Main.MouseScreen) + Main.screenPosition;
                    float x = vector4.X;
                    float y = vector4.Y;
                    Vector2 vector5 = new Vector2(x, y) - Main.player[projectile.owner].Center;
                    if (vector5.Length() > yoYoLength)
                    {
                        vector5.Normalize();
                        vector5 *= yoYoLength;
                        vector5 = Main.player[projectile.owner].Center + vector5;
                        x = vector5.X;
                        y = vector5.Y;
                    }

                    if (projectile.ai[0] != x || projectile.ai[1] != y)
                    {
                        Vector2 vector6 = new Vector2(x, y) - Main.player[projectile.owner].Center;
                        if (vector6.Length() > yoYoLength - 1f)
                        {
                            vector6.Normalize();
                            vector6 *= yoYoLength - 1f;
                            Vector2 vector7 = Main.player[projectile.owner].Center + vector6;
                            x = vector7.X;
                            y = vector7.Y;
                        }

                        projectile.ai[0] = x;
                        projectile.ai[1] = y;
                        projectile.netUpdate = true;
                    }
                }
            }

            if (overLenthEX && projectile.owner == Main.myPlayer)
            {
                projectile.ai[0] = -1f;
                projectile.netUpdate = true;
            }

            if (projectile.ai[0] >= 0f)
            {
                //边界检测以限速
                if (overLength)
                {
                    num7 /= 2f;
                    yoYoSpeed *= 2f;
                    if (projectile.Center.X > Main.player[projectile.owner].Center.X && projectile.velocity.X > 0f)
                        projectile.velocity.X *= 0.5f;

                    if (projectile.Center.Y > Main.player[projectile.owner].Center.Y && projectile.velocity.Y > 0f)
                        projectile.velocity.Y *= 0.5f;

                    if (projectile.Center.X < Main.player[projectile.owner].Center.X && projectile.velocity.X < 0f)
                        projectile.velocity.X *= 0.5f;

                    if (projectile.Center.Y < Main.player[projectile.owner].Center.Y && projectile.velocity.Y < 0f)
                        projectile.velocity.Y *= 0.5f;
                }

                Vector2 vector8 = new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center;
                if (overLength)
                    num7 = 1f;

                projectile.velocity.Length();
                float num13 = vector8.Length();
                if (num13 > num9)
                {
                    vector8.Normalize();
                    float num14 = Math.Min(num13 / 2f, yoYoSpeed);
                    if (overLength)
                        num14 = Math.Min(num14, yoYoSpeed / 2f);

                    vector8 *= num14;
                    projectile.velocity = (projectile.velocity * (num7 - 1f) + vector8) / num7;
                }
                else if (flag)
                {
                    if ((double)projectile.velocity.Length() < (double)yoYoSpeed * 0.6)
                    {
                        vector8 = projectile.velocity;
                        vector8.Normalize();
                        vector8 *= yoYoSpeed * 0.6f;
                        projectile.velocity = (projectile.velocity * (num7 - 1f) + vector8) / num7;
                    }
                }
                else
                {
                    projectile.velocity *= 0.8f;
                }

                if (flag && !overLength && (double)projectile.velocity.Length() < (double)yoYoSpeed * 0.6)
                {
                    projectile.velocity.Normalize();
                    projectile.velocity *= yoYoSpeed * 0.6f;
                }
            }
        }
        #region 收回
        else
        {
            num7 = (int)((double)num7 * 0.8);
            yoYoSpeed *= 1.5f;
            projectile.tileCollide = false;
            Vector2 vector9 = Main.player[projectile.owner].Center - projectile.Center;
            float num15 = vector9.Length();
            if (num15 < yoYoSpeed + 10f || num15 == 0f || num15 > 2000f)
            {
                projectile.Kill();
            }
            else
            {
                vector9.Normalize();
                vector9 *= yoYoSpeed;
                projectile.velocity = (projectile.velocity * (num7 - 1f) + vector9) / num7;
            }
        }
        #endregion


        projectile.rotation += 0.45f;
    }


    public static void AI_182_FinalFractal(this Projectile projectile)
    {
        Player player = Main.player[projectile.owner];
        Vector2 mountedCenter = player.MountedCenter;//坐骑上玩家的中心
        float lerpValue = Utils.GetLerpValue(900f, 0f, projectile.velocity.Length() * 2f, true);//获取线性插值的t值
        float rate = MathHelper.Lerp(0.7f, 2f, lerpValue);//速度的模长的两倍越接近900，这个值越接近0.7f //事实上，速度的两倍就是玩家到目标点的向量
        projectile.localAI[0] += rate;
        if (projectile.localAI[0] >= 120f)
        {
            projectile.Kill();
            return;
        }
        //目标离你越近，剑气的飞行时间越短
        float factor = Utils.GetLerpValue(0f, 1f, projectile.localAI[0] / 60f, true);//??这不就单纯clamp了一下
        float shortAxis = projectile.ai[0];
        float rotation = projectile.velocity.ToRotation();
        float PI = 3.14159274f;
        float direction = projectile.velocity.X > 0f ? 1 : -1;
        float theta = PI + direction * factor * 6.28318548f;
        float longAxis = projectile.velocity.Length() + Utils.GetLerpValue(0.5f, 1f, factor, true) * 40f;
        float minLongAxis = 60f;
        if (longAxis < minLongAxis)
        {
            longAxis = minLongAxis;//保证半长轴最短是60
        }
        Vector2 center = mountedCenter + projectile.velocity;//椭圆中心
        Vector2 offset = new Vector2(1f, 0f).RotatedBy(theta, default) * new Vector2(longAxis, shortAxis * MathHelper.Lerp(2f, 1f, lerpValue));//插值生成椭圆轨迹
        Vector2 stdOffset = center + offset.RotatedBy(rotation, default);//加上弹幕自身旋转量



        Vector2 value3 = (1f - Utils.GetLerpValue(0f, 0.5f, factor, true)) *
            new Vector2((projectile.velocity.X > 0f ? 1 : -1) * -longAxis * 0.1f, -projectile.ai[0] * 0.3f);//坐标修改偏移量



        float finalRotation = theta + rotation;
        projectile.rotation = finalRotation + 1.57079637f;//弹幕绘制旋转量
        projectile.Center = stdOffset + value3;
        projectile.spriteDirection = projectile.direction = projectile.velocity.X > 0f ? 1 : -1;
        if (shortAxis < 0f)//小于零就反向
        {
            projectile.rotation = PI + direction * factor * -6.28318548f + rotation;
            projectile.rotation += 1.57079637f;
            projectile.spriteDirection = projectile.direction = projectile.velocity.X > 0f ? -1 : 1;
        }
        projectile.Opacity = Utils.GetLerpValue(0f, 5f, projectile.localAI[0], true) * Utils.GetLerpValue(120f, 115f, projectile.localAI[0], true);//修改透明度
    }


    //原版泰拉棱镜或者血蝙蝠
    //这个函数除了处理一些基本更新逻辑，唯一的用处就是Think的入口
    public static void AI_156_BatOfLight(this Projectile projectile)
    {
        List<int> ai156_blacklistedTargets = Projectile._ai156_blacklistedTargets;//空列表，用于在后续AI中存储或者调用黑名单目标
        Player player = Main.player[projectile.owner];
        bool isBat = projectile.type == 755;
        bool isSword = projectile.type == 946;
        if (isBat)
        {
            if (player.dead)
                player.batsOfLight = false;

            if (player.batsOfLight)
                projectile.timeLeft = 2;

            DelegateMethods.v3_1 = projectile.AI_156_GetColor().ToVector3();
            Point point = projectile.Center.ToTileCoordinates();
            DelegateMethods.CastLightOpen(point.X, point.Y);//其实就是单纯根据剑配色打光
            if (++projectile.frameCounter >= 6)
            {
                projectile.frameCounter = 0;
                if (++projectile.frame >= Main.projFrames[projectile.type] - 1)
                    projectile.frame = 0;
            }//播放动画

            int num2 = player.direction;
            if (projectile.velocity.X != 0f)
                num2 = Math.Sign(projectile.velocity.X);

            projectile.spriteDirection = num2;//根据玩家朝向改弹幕朝向
        }

        if (isSword)
        {
            if (player.dead)
                player.empressBlade = false;

            if (player.empressBlade)
                projectile.timeLeft = 2;

            DelegateMethods.v3_1 = projectile.AI_156_GetColor().ToVector3();
            Point point2 = projectile.Center.ToTileCoordinates();
            DelegateMethods.CastLightOpen(point2.X, point2.Y);//打光
        }

        ai156_blacklistedTargets.Clear();
        projectile.AI_156_Think(ai156_blacklistedTargets);
    }

    //有个软用啊喂
    public static Color AI_156_GetColor(this Projectile projectile)
    {
        if (projectile.aiStyle != 156)
            return Color.Transparent;

        bool num = projectile.type == 755;
        _ = projectile.type;
        if (num)
            return Color.Crimson;

        return Color.Transparent;
    }

    //所以其实这个才是最主要的那个函数，其它无非就是获取颜色 位置这些奇怪辅助信息的函数
    public static void AI_156_Think(this Projectile projectile, List<int> blacklist)
    {
        bool isBat = projectile.type == 755;
        bool isSword = projectile.type == 946;
        int batTotalTime = 60;
        int batTimerMax = batTotalTime - 1;
        int swordTotalTime = batTotalTime + 60;
        int swordTimeMax = swordTotalTime - 1;
        int timeMax = batTotalTime + 1;
        if (isBat)
            batTotalTime = 66;

        if (isSword)
        {
            batTotalTime = 40;
            batTimerMax = batTotalTime - 1;
            swordTotalTime = batTotalTime + 40;
            swordTimeMax = swordTotalTime - 1;
            timeMax = batTotalTime + 1;
        }
        //上面这段赋值是真的神经中的神经，不知道Re是怎么做到写出这样的代码的，我敢肯定大抵不是反编译的问题
        Player player = Main.player[projectile.owner];
        if (player.active && Vector2.Distance(player.Center, projectile.Center) > 2000f)//如果离得太远就把ai数组两个元素设为0
        {
            projectile.ai[0] = 0f;
            projectile.ai[1] = 0f;
            projectile.netUpdate = true;
        }

        //赶路状态，不知道什么时候会设置成这个
        if (projectile.ai[0] == -1f)
        {
            projectile.AI_GetMyGroupIndexAndFillBlackList(blacklist, out var index, out var totalIndexesInGroup);//获取组下标总数以及黑名单
            projectile.AI_156_GetIdlePosition(index, totalIndexesInGroup, out var idleSpot, out var idleRotation);//根据下标和总数获取位置和夹角
            projectile.velocity = Vector2.Zero;
            projectile.Center = projectile.Center.MoveTowards(idleSpot, 32f);//向目标方向移动至多32像素
            projectile.rotation = projectile.rotation.AngleLerp(idleRotation, 0.2f);//逐渐过渡到目标角度
            if (projectile.Distance(idleSpot) < 2f)//距离闲置地点足够近的时候把ai0设为0
            {
                projectile.ai[0] = 0f;
                projectile.netUpdate = true;
            }

            return;
        }

        //闲置状态，随时准备抽取目标发起攻击
        if (projectile.ai[0] == 0f)
        {
            if (isBat)
            {
                projectile.AI_GetMyGroupIndexAndFillBlackList(blacklist, out var index2, out var totalIndexesInGroup2);
                projectile.AI_156_GetIdlePosition(index2, totalIndexesInGroup2, out var idleSpot2, out var _);
                projectile.velocity = Vector2.Zero;
                projectile.Center = Vector2.SmoothStep(projectile.Center, idleSpot2, 0.45f);
                if (Main.rand.NextBool(20))//1/20的概率脱离闲置状态，这个随机数应该是防止一涌而上用的
                {
                    int targetIndex = projectile.AI_156_TryAttackingNPCs(blacklist);//尝试搜索目标并开展攻击
                    if (targetIndex != -1)
                    {
                        projectile.AI_156_StartAttack();//进入攻击阶段
                        projectile.ai[0] = batTotalTime;
                        projectile.ai[1] = targetIndex;
                        projectile.netUpdate = true;
                        return;
                    }
                }
            }
            else if (isSword)
            {

                //和蝙蝠的基本一致，多了个夹角的过渡
                projectile.AI_GetMyGroupIndexAndFillBlackList(blacklist, out var index3, out var totalIndexesInGroup3);
                projectile.AI_156_GetIdlePosition(index3, totalIndexesInGroup3, out var idleSpot3, out var idleRotation3);
                projectile.velocity = Vector2.Zero;
                projectile.Center = Vector2.SmoothStep(projectile.Center, idleSpot3, 0.45f);
                projectile.rotation = projectile.rotation.AngleLerp(idleRotation3, 0.45f);
                if (Main.rand.NextBool(20))
                {
                    int targetIndex = projectile.AI_156_TryAttackingNPCs(blacklist);
                    if (targetIndex != -1)
                    {
                        projectile.AI_156_StartAttack();
                        projectile.ai[0] = Main.rand.NextFromList<int>(batTotalTime, swordTotalTime);//哦淦，随机这一下意义是什么
                        projectile.ai[0] = swordTotalTime;
                        projectile.ai[1] = targetIndex;
                        projectile.netUpdate = true;
                    }
                }
            }
            return;
        }

        //蝙蝠的其它ai，剩下的都是泰拉棱镜的
        if (isBat)
        {
            int targetIndex = (int)projectile.ai[1];
            if (!Main.npc.IndexInRange(targetIndex))//如果下标超出范围了
            {
                projectile.ai[0] = 0f;//回到闲置状态
                projectile.netUpdate = true;
                return;
            }

            NPC nPC = Main.npc[targetIndex];
            if (!nPC.CanBeChasedBy(projectile))//如果目标不可用
            {
                projectile.ai[0] = 0f;//回到闲置状态
                projectile.netUpdate = true;
                return;
            }

            projectile.ai[0] -= 1f;
            if (projectile.ai[0] >= (float)batTimerMax)
            {
                projectile.velocity *= 0.8f;
                if (projectile.ai[0] == (float)batTimerMax)
                {
                    projectile.localAI[0] = projectile.Center.X;
                    projectile.localAI[1] = projectile.Center.Y;
                }

                return;
            }

            float factor = Utils.GetLerpValue(batTimerMax, 0f, projectile.ai[0], clamped: true);
            Vector2 projCenter = new(projectile.localAI[0], projectile.localAI[1]);
            if (factor >= 0.5f)
                projCenter = Main.player[projectile.owner].Center;

            Vector2 targetCenter = nPC.Center;
            float angle = (targetCenter - projCenter).ToRotation();
            float assistAngle = ((targetCenter.X > projCenter.X) ? (-(float)Math.PI) : ((float)Math.PI));
            float rotation = assistAngle + (0f - assistAngle) * factor * 2f;//其实就是插值，顺时针或者逆时针旋转
            Vector2 spinningpoint = rotation.ToRotationVector2();
            spinningpoint.Y *= (float)Math.Sin((float)projectile.identity * 2.3f) * 0.5f;//根据弹幕的唯一编号(?)来改变轨迹？
            spinningpoint = spinningpoint.RotatedBy(angle);//讲椭圆旋转至所需角度
            float halfLongAxis = (targetCenter - projCenter).Length() / 2f;
            Vector2 center2 = Vector2.Lerp(projCenter, targetCenter, 0.5f) + spinningpoint * halfLongAxis;
            projectile.Center = center2;//赋上椭圆轨迹
            Vector2 velocity = MathHelper.WrapAngle(angle + rotation + 0f).ToRotationVector2() * 10f;
            projectile.velocity = velocity;//这里这个速度只是方便后续处理使用？
            projectile.position -= projectile.velocity;
            if (projectile.ai[0] == 0f)
            {
                int newTarget = projectile.AI_156_TryAttackingNPCs(blacklist);
                if (newTarget != -1)
                {
                    projectile.ai[0] = batTotalTime;
                    projectile.ai[1] = newTarget;
                    projectile.AI_156_StartAttack();
                    projectile.netUpdate = true;
                    return;
                }

                projectile.ai[1] = 0f;//清除目标
                projectile.netUpdate = true;
            }
        }

        else if (isSword)
        {

            bool skipBodyCheck = true;
            int mode = 0;
            int currentModeTimeMax = batTimerMax;
            int checkTime = 0;
            if (projectile.ai[0] >= (float)timeMax)
            {
                mode = 1;
                currentModeTimeMax = swordTimeMax;
                checkTime = timeMax;
            }

            //下面这两个老样子，目标没用就找新的，不然就飞回来
            int targetIndex = (int)projectile.ai[1];
            if (!Main.npc.IndexInRange(targetIndex))
            {
                int newTarget = projectile.AI_156_TryAttackingNPCs(blacklist, skipBodyCheck);
                if (newTarget != -1)
                {
                    projectile.ai[0] = Main.rand.NextFromList<int>(batTotalTime, swordTotalTime);
                    projectile.ai[1] = newTarget;
                    projectile.AI_156_StartAttack();
                    projectile.netUpdate = true;
                }
                else
                {
                    projectile.ai[0] = -1f;//如果没找到目标就暂且换成快速移动回来的状态
                    projectile.ai[1] = 0f;
                    projectile.netUpdate = true;
                }

                return;
            }

            NPC nPC2 = Main.npc[targetIndex];
            if (!nPC2.CanBeChasedBy(projectile))
            {
                int newTarget = projectile.AI_156_TryAttackingNPCs(blacklist, skipBodyCheck);
                if (newTarget != -1)
                {
                    projectile.ai[0] = Main.rand.NextFromList<int>(batTotalTime, swordTotalTime);
                    projectile.AI_156_StartAttack();
                    projectile.ai[1] = newTarget;
                    projectile.netUpdate = true;
                }
                else
                {
                    projectile.ai[0] = -1f;
                    projectile.ai[1] = 0f;
                    projectile.netUpdate = true;
                }

                return;
            }

            projectile.ai[0] -= 1f;
            if (projectile.ai[0] >= (float)currentModeTimeMax)
            {
                projectile.direction = ((projectile.Center.X < nPC2.Center.X) ? 1 : (-1));
                if (projectile.ai[0] == (float)currentModeTimeMax)
                {
                    projectile.localAI[0] = projectile.Center.X;
                    projectile.localAI[1] = projectile.Center.Y;
                }
            }

            float factor = Utils.GetLerpValue(currentModeTimeMax, checkTime, projectile.ai[0], clamped: true);
            //挥砍模式
            //也许叫旋转模式更合适
            if (mode == 0)
            {
                Vector2 vector3 = new(projectile.localAI[0], projectile.localAI[1]);
                if (factor >= 0.5f)
                    vector3 = Vector2.Lerp(nPC2.Center, Main.player[projectile.owner].Center, 0.5f);

                Vector2 center3 = nPC2.Center;
                float num20 = (center3 - vector3).ToRotation();
                float num21 = ((projectile.direction == 1) ? (-(float)Math.PI) : ((float)Math.PI));
                float num22 = num21 + (0f - num21) * factor * 2f;
                Vector2 spinningpoint2 = num22.ToRotationVector2();
                spinningpoint2.Y *= 0.5f;
                spinningpoint2.Y *= 0.8f + (float)Math.Sin((float)projectile.identity * 2.3f) * 0.2f;
                spinningpoint2 = spinningpoint2.RotatedBy(num20);
                float num23 = (center3 - vector3).Length() / 2f;
                Vector2 center4 = Vector2.Lerp(vector3, center3, 0.5f) + spinningpoint2 * num23;
                projectile.Center = center4;
                float num24 = MathHelper.WrapAngle(num20 + num22 + 0f);
                projectile.rotation = num24 + (float)Math.PI / 2f;
                Vector2 vector4 = num24.ToRotationVector2() * 10f;
                projectile.velocity = vector4;
                projectile.position -= projectile.velocity;
            }

            //戳刺模式
            //处于这个模式时会以冲向目标的形式发起攻击
            if (mode == 1)
            {
                Vector2 vector5 = new(projectile.localAI[0], projectile.localAI[1]);
                vector5 += new Vector2(0f, Utils.GetLerpValue(0f, 0.4f, factor, clamped: true) * -100f);
                Vector2 v = nPC2.Center - vector5;
                Vector2 vector6 = v.SafeNormalize(Vector2.Zero) * MathHelper.Clamp(v.Length(), 60f, 150f);
                Vector2 value = nPC2.Center + vector6;
                float lerpValue3 = Utils.GetLerpValue(0.4f, 0.6f, factor, clamped: true);
                float lerpValue4 = Utils.GetLerpValue(0.6f, 1f, factor, clamped: true);
                float targetAngle = v.SafeNormalize(Vector2.Zero).ToRotation() + (float)Math.PI / 2f;
                projectile.rotation = projectile.rotation.AngleTowards(targetAngle, (float)Math.PI / 5f);
                projectile.Center = Vector2.Lerp(vector5, nPC2.Center, lerpValue3);
                if (lerpValue4 > 0f)
                    projectile.Center = Vector2.Lerp(nPC2.Center, value, lerpValue4);
            }

            if (projectile.ai[0] == (float)checkTime)
            {
                int newTarget = projectile.AI_156_TryAttackingNPCs(blacklist, skipBodyCheck);
                if (newTarget != -1)
                {
                    projectile.ai[0] = Main.rand.NextFromList<int>(batTotalTime, swordTotalTime);
                    projectile.ai[1] = newTarget;
                    projectile.AI_156_StartAttack();
                    projectile.netUpdate = true;
                }
                else
                {
                    projectile.ai[0] = -1f;
                    projectile.ai[1] = 0f;
                    projectile.netUpdate = true;
                }
            }
        }
    }
    //进攻...?
    //不是哥们，怎么是清除一下本地的免疫无敌帧就走人了
    public static void AI_156_StartAttack(this Projectile projectile)
    {
        for (int i = 0; i < projectile.localNPCImmunity.Length; i++)
        {
            projectile.localNPCImmunity[i] = 0;
        }
    }

    //尝试找到目标并发起攻击
    //目标需要是boss或者不在黑名单里面
    //需要足够近(小于1000像素
    //需要通过身体检测(或者跳过
    public static int AI_156_TryAttackingNPCs(this Projectile projectile, List<int> blackListedTargets, bool skipBodyCheck = false)
    {
        Vector2 center = Main.player[projectile.owner].Center;
        int result = -1;
        float num = -1f;
        NPC ownerMinionAttackTargetNPC = projectile.OwnerMinionAttackTargetNPC;//获取弹幕拥有者标记的NPC
        if (ownerMinionAttackTargetNPC != null && ownerMinionAttackTargetNPC.CanBeChasedBy(projectile))
        {
            bool flag = true;
            if (!ownerMinionAttackTargetNPC.boss && blackListedTargets.Contains(ownerMinionAttackTargetNPC.whoAmI))//是boss或者不在黑名单里面
                flag = false;

            if (ownerMinionAttackTargetNPC.Distance(center) > 1000f)//离目标距离小于1000像素
                flag = false;

            if (!skipBodyCheck && !projectile.CanHitWithOwnBody(ownerMinionAttackTargetNPC))//跳过身体检测或者检测通过
                flag = false;

            if (flag)
                return ownerMinionAttackTargetNPC.whoAmI;//直接将标记目标作为攻击目标
        }

        for (int i = 0; i < 200; i++)
        {
            NPC nPC = Main.npc[i];
            if (nPC.CanBeChasedBy(projectile) && (nPC.boss || !blackListedTargets.Contains(i)))//类似上面的检测条件
            {
                float num2 = nPC.Distance(center);
                if (!(num2 > 1000f) && (!(num2 > num) || num == -1f) && (skipBodyCheck || projectile.CanHitWithOwnBody(nPC)))
                {
                    num = num2;
                    result = i;
                }
            }
        }

        return result;
    }

    //获取下标 总数，填充黑名单
    //但是事实上，黑名单没有任何改动？？
    public static void AI_GetMyGroupIndexAndFillBlackList(this Projectile proj, List<int> blackListedTargets, out int index, out int totalIndexesInGroup)
    {
        index = 0;
        totalIndexesInGroup = 0;
        for (int i = 0; i < 1000; i++)
        {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == proj.owner && projectile.type == proj.type && (projectile.type != 759 || projectile.frame == Main.projFrames[projectile.type] - 1))
            {
                if (proj.whoAmI > i)
                    index++;

                totalIndexesInGroup++;
            }
        }
    }

    //获取闲置位置
    //蝙蝠就单纯正圆
    //泰拉棱镜会稍微复杂一点
    public static void AI_156_GetIdlePosition(this Projectile projectile, int stackedIndex, int totalIndexes, out Vector2 idleSpot, out float idleRotation)
    {
        Player player = Main.player[projectile.owner];
        bool isBat = projectile.type == 755;
        bool isSword = projectile.type == 946;
        idleRotation = 0f;
        idleSpot = Vector2.Zero;
        if (isBat)
        {
            float num2 = ((float)totalIndexes - 1f) / 2f;
            idleSpot = player.Center + -Vector2.UnitY.RotatedBy(4.3982296f / (float)totalIndexes * ((float)stackedIndex - num2)) * 40f;
            idleRotation = 0f;
        }

        if (isSword)
        {
            int num3 = stackedIndex + 1;
            idleRotation = (float)num3 * ((float)Math.PI * 2f) * (1f / 60f) * (float)player.direction + (float)Math.PI / 2f;
            idleRotation = MathHelper.WrapAngle(idleRotation);
            int num4 = num3 % totalIndexes;
            Vector2 vector = new Vector2(0f, 0.5f).RotatedBy((player.miscCounterNormalized * (2f + (float)num4) + (float)num4 * 0.5f + (float)player.direction * 1.3f) * ((float)Math.PI * 2f)) * 4f;
            idleSpot = idleRotation.ToRotationVector2() * 10f + player.MountedCenter + new Vector2(player.direction * (num3 * -6 - 16), player.gravDir * -15f);
            idleSpot += vector;
            idleRotation += (float)Math.PI / 2f;
        }
    }
}

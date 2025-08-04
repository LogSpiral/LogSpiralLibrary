using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using System.Linq;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 泰拉棱镜???!!!
/// </summary>
public class TerraprismaInfo : VanillaMelee
{
    #region 辅助字段
    public NPC target;
    public Vector2 realCenter;
    public Vector2 assistVelocity;
    public float[] assistParas = new float[2];
    public float realRotation;
    public Vector2[] oldCenters = new Vector2[45];
    public float[] oldRotations = new float[45];
    public UltraSwoosh ultra;
    #endregion

    #region 重写属性
    public override float offsetRotation => realRotation;
    public override Vector2 offsetCenter => realCenter - Owner.Center;
    public override bool Attacktive => target != null;
    public override bool OwnerHitCheek => true;
    #endregion

    #region 辅助函数
    private void FindTarget()
    {
        foreach (var npc in Main.npc)
            if (npc.CanBeChasedBy() && !npc.friendly && Vector2.Distance(npc.Center, Owner.Center) < 1024f)
            {
                target = npc;
                break;
            }
    }
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        var verS = standardInfo.VertexStandard;
        if (Owner is Player plr)
            plr.direction = Math.Sign(plr.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition.X - plr.Center.X);
        if (ultra != null) ultra.autoUpdate = false;
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
        if (Main.dedServ) return;

        ultra.timeLeftMax = timerMax;
        ultra.timeLeft = (int)(timerMax * Math.Pow(Factor, 0.25));
        ultra.center = Owner.Center;

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

    public override void OnDeactive()
    {
        if (ultra != null)
        {
            ultra.timeLeft = 0;
            ultra = null;
        }
        base.OnDeactive();
    }

    public override void OnStartSingle()
    {
        var verS = standardInfo.VertexStandard;
        var pair = verS.swooshTexIndex ?? (3, 7);
        if (Main.netMode != NetmodeID.Server)
        {
            var u = ultra = UltraSwoosh.NewUltraSwoosh(verS.canvasName, timerMax, 1f, default, (0, 0));
            u.heatMap = verS.heatMap;
            u.aniTexIndex = pair.Item1;
            u.baseTexIndex = pair.Item2;
            u.ColorVector = verS.colorVec;
            u.autoUpdate = false;
            u.timeLeft = 1;
            u.ApplyStdValueToVtxEffect(standardInfo);
        }

        base.OnStartSingle();
        Rotation = 0;
        realRotation = MathHelper.PiOver2;
        Projectile.ownerHitCheck = false;
        assistParas = new float[5];
        realCenter = Owner.Center;
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
                    oldCenters[n] + oldRotations[n].ToRotationVector2() * standardInfo.VertexStandard.scaler * offsetSize * sc, 48f, ref point1);
            if (flag)
            {
                if (Owner is Player player && Main.rand.NextBool(5))
                {
                    Vector2 orig = player.Center;
                    player.Center = realCenter;
                    player.ItemCheck_Shoot(player.whoAmI, player.HeldItem, CurrentDamage);
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
            var currentVertex = DrawingMethods.GetItemVertexes(finalOrigin, standardInfo.standardRotation, oldRotations[n], Rotation, texture, KValue, offsetSize * ModifyData.actionOffsetSize * sc, oldCenters[n], !flip, (n == 0 ? 1 : (45f - n) / 90f) * k, standardInfo.frame);
            result = result.Concat(currentVertex);
        }
        return [.. result];
    }
    #endregion
}

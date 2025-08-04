using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;
/// <summary>
/// 天顶
/// </summary>
public class ZenithInfo : VanillaMelee
{
    #region 辅助字段
    public float dist;
    public UltraSwoosh[] ultras = new UltraSwoosh[3];
    #endregion

    #region 重写属性
    public override float offsetRotation => MathHelper.SmoothStep(1f, -1f, MathHelper.Clamp((1 - Factor) * 2, 0, 1)) * (flip ? 1 : -1) * MathHelper.Pi;
    public override Vector2 offsetCenter => Rotation.ToRotationVector2() * dist * .5f + (offsetRotation.ToRotationVector2() * new Vector2(dist * .5f, 100 / KValue)).RotatedBy(Rotation);
    public override bool Attacktive => true;
    public override bool OwnerHitCheek => false;
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        if (Main.dedServ) return;
        if (timer == timerMax)
        {
            for (int n = 0; n < 3; n++)
                ultras[n].timeLeft = ultras[n].timeLeftMax = timerMax;
        }
        for (int n = 0; n < 3; n++)
            if (ultras[n] != null) ultras[n].autoUpdate = false;
        timer--;
        var origf = fTimer;
        for (int n = 0; n < 3; n++)
        {
            var origf_s = fTimer;
            fTimer += timerMax / 4f * n;
            float alphaG = 1 - MathF.Pow(n / 3f, 4);
            UltraSwoosh u = ultras[n];
            u.timeLeft--;
            u.center = Owner.Center + Rotation.ToRotationVector2() * dist * .5f;
            var vertex = u.VertexInfos;
            for (int i = 0; i < u.Counts; i++)
            {
                float alphaT = MathHelper.Clamp((1 - Factor) * 2, 0, 1);
                alphaT = MathHelper.Clamp(alphaT * (1 - alphaT) * 8, 0, 1);
                var curTex = base.GetWeaponVertex(TextureAssets.Item[standardInfo.itemType].Value, 1f);
                var f = i / (u.Counts - 1f);
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

    public override void OnDeactive()
    {
        foreach (var u in ultras)
        {
            if (u != null)
                u.timeLeft = 0;
        }
        base.OnDeactive();
    }

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
        var verS = standardInfo.VertexStandard;
        if (Main.netMode != NetmodeID.Server)
        {
            for (int n = 0; n < 3; n++)
            {
                var pair = verS.swooshTexIndex ?? (3, 7);
                var u = ultras[n] = UltraSwoosh.NewUltraSwoosh(verS.canvasName, timerMax, 1, default, (0, 0));
                u.heatMap = verS.heatMap;
                u.aniTexIndex = pair.Item1;
                u.baseTexIndex = pair.Item2;
                u.ColorVector = verS.colorVec;
                u.autoUpdate = false;
                u.timeLeft = 1;
                u.ApplyStdValueToVtxEffect(standardInfo);
            }
            SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
        }

        base.OnStartSingle();
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

                return true;
            }
            fTimer += timerMax / 4f;
        }
        fTimer = origf_s;
        return false;
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        if (Owner is Player plr && Main.rand.NextBool(5))
        {
            Vector2 orig = plr.Center;
            plr.Center += offsetCenter;
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
        return [.. result];
    }
    #endregion
}

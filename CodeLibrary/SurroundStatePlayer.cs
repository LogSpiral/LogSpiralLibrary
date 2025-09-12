using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary;

// TODO 重做周围环境状态判定条件
public enum SurroundState
{
    /// <summary>
    /// 周围没有敌怪，氨泉滴狠呐
    /// </summary>
    None,

    /// <summary>
    /// 离地面比较近
    /// </summary>
    CloseToGround,

    /// <summary>
    /// 半空
    /// </summary>
    MidAir,

    /// <summary>
    /// 直面你的梦魇！
    /// </summary>
    FrontThreat,

    /// <summary>
    /// 四面楚歌
    /// </summary>
    SurroundThreat
}

public class SurroundStatePlayer : ModPlayer
{
    public SurroundState state;
    public List<Entity> frontTargets = [];
    public List<Entity> otherTargets = [];
    public Vector2 targetFront;

    public void UpdateData()
    {
        if (Player.whoAmI != Main.myPlayer) return;

        #region 环境检测

        frontTargets.Clear();
        otherTargets.Clear();
        float distanceMax = 200;//Player.velocity.Y != 0 ? 1024 :
        float distanceMaxF = distanceMax;
        Vector2 targetF = default;
        //float distanceMaxO = 200;
        foreach (var entity in Main.npc.Union<Entity>(Main.projectile).Union(Main.player))
        {
            var distance = Vector2.Distance(entity.Center, Player.Center);
            bool flag = entity switch
            {
                NPC npc => !npc.friendly,
                Projectile proj => proj.hostile,
                Player plr => plr.hostile && plr.team != Player.team,
                _ => false
            };
            if (flag && entity.active && distance < distanceMax)
            {
                if (VectorMethods.Cos(entity.Center, Main.MouseWorld, Player.Center) > 0.5f)
                {
                    frontTargets.Add(entity);
                    if (distance < distanceMaxF)
                    {
                        distanceMaxF = distance;
                        targetF = entity.Center;
                    }
                }
                else
                {
                    otherTargets.Add(entity);
                    //if (distance < distanceMaxO) distanceMaxO = distance;
                }
            }
        }
        if (frontTargets.Count == 0)
        {
            targetF = Main.MouseWorld;
        }
        else
        {
            var (frontAvg, frontStd) = (from npc in frontTargets where true select npc.Center).AvgStd();
            if (frontStd <= 256) targetF = frontAvg;
        }
        targetFront = targetF;

        #endregion 环境检测

        #region 切换状态

        if (Player.velocity.Y != 0)
        {
            int h = 0;
            Point coord = Player.Center.ToTileCoordinates();
            while (h < 5 && !Framing.GetTileSafely(coord + new Point(0, h)).HasTile)
            {
                h++;
            }
            state = h == 5 ? SurroundState.MidAir : SurroundState.CloseToGround;
            return;
        }
        if (frontTargets.Count + otherTargets.Count == 0)
        {
            state = SurroundState.None;
            return;
        }
        state = otherTargets.Count > frontTargets.Count ? SurroundState.SurroundThreat : SurroundState.FrontThreat;

        #endregion 切换状态
    }

    public override void ResetEffects()
    {
        if ((int)GlobalTimeSystem.GlobalTimePaused % SurroundStateConfig.Instance.CheckCycleLength == 0)
            UpdateData();
        base.ResetEffects();
    }
}

public class SurroundStateConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Range(1, 5)]
    [DefaultValue(1)]
    public int CheckCycleLength;

    public static SurroundStateConfig Instance => ModContent.GetInstance<SurroundStateConfig>();
}
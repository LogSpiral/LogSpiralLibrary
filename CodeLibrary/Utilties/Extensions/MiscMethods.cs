using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Reflection;
using Terraria.Graphics.Shaders;
using static LogSpiralLibrary.LogSpiralLibraryMod;

namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

/// <summary>
/// 我实在不知道该往哪里丢了
/// </summary>
public static class MiscMethods
{
    private static MethodInfo GetInstanceMethod => field ??= typeof(ModContent).GetMethod(nameof(ModContent.GetInstance), BindingFlags.Static | BindingFlags.Public);
    public static object GetInstanceViaType(Type type)
    {
        return GetInstanceMethod?.MakeGenericMethod(type)?.Invoke(null, []);
    }

    public static Dust FastDust(Vector2 Center, Vector2 velocity, Color color, float scaler, int? shaderID = null)
    {
        var hsl = Main.rgbToHsl(color);//Color.MediumPurple
        var dustColor = Color.Lerp(Main.hslToRgb(Vector3.Clamp(hsl * new Vector3(1, 2, Main.rand.NextFloat(0.85f, 1.15f)), default, Vector3.One)), Color.White, Main.rand.NextFloat(0, 0.3f));
        Dust dust = Dust.NewDustPerfect(Center, 278, velocity, 0, dustColor);
        dust.scale = 0.4f + Main.rand.NextFloat(-1, 1) * 0.1f;
        dust.scale *= Main.rand.NextFloat(1, 2f) * scaler;
        dust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.3f;
        dust.fadeIn *= .5f;
        dust.noGravity = true;
        if (shaderID is > 0)
            dust.shader = GameShaders.Armor._shaderData[shaderID.Value - 1];
        return dust;
    }

    public static Dust FastDust(Vector2 Center, Vector2 velocity, Color color) => FastDust(Center, velocity, color, 1f);

    public static T HardmodeValue<T>(T normalValue, T expertValue, T masterValue)
    {
        return Main.expertMode ? Main.masterMode ? masterValue : expertValue : normalValue;
    }

    #region 线条粒子

    public static void LinerDust(Vector2 vec1, Vector2 vec2, int type = MyDustId.Fire, float step = 2)
    {
        for (float n = 0; n <= (vec1 - vec2).Length(); n += step)
        {
            Dust.NewDustPerfect(Vector2.Lerp(vec1, vec2, n / (vec1 - vec2).Length()), type, newColor: Color.White).noGravity = true;
        }
    }

    public static void LinerDust(Vector3 vec1, Vector3 vec2, float height, Vector2 projCenter = default, Vector2 drawOffset = default, int type = MyDustId.Fire, float step = 2)
    {
        var v1 = vec1.Projectile(height, projCenter);
        var v2 = vec2.Projectile(height, projCenter);
        for (float n = 0; n <= (v1 - v2).Length(); n += step)
        {
            Dust.NewDustPerfect(Vector2.Lerp(v1, v2, n / (v1 - v2).Length()) + drawOffset, type, newColor: Color.White).noGravity = true;
        }
    }

    public static void LinerDust(Vector4 vec1, Vector4 vec2, float heightZ, float heightW, Vector2 drawOffset = default, Vector2 projCenter = default, int type = MyDustId.Fire, float step = 2)
    {
        var v1 = vec1.Projectile(heightW, new Vector3(projCenter, 0)).Projectile(heightZ, projCenter);
        var v2 = vec2.Projectile(heightW, new Vector3(projCenter, 0)).Projectile(heightZ, projCenter);
        for (float n = 0; n <= (v1 - v2).Length(); n += step)
        {
            Dust.NewDustPerfect(Vector2.Lerp(v1, v2, n / (v1 - v2).Length()) + drawOffset, type, newColor: Color.White).noGravity = true;
        }
    }

    public static void LinerDust(Vector4 vec1, Vector4 vec2, float heightZ, float heightW, Action<Dust> action, Vector2 drawOffset = default, Vector2 projCenter = default, int type = MyDustId.Fire, float step = 2)
    {
        var v1 = vec1.Projectile(heightW, new Vector3(projCenter, 0)).Projectile(heightZ, projCenter);
        var v2 = vec2.Projectile(heightW, new Vector3(projCenter, 0)).Projectile(heightZ, projCenter);
        for (float n = 0; n <= (v1 - v2).Length(); n += step)
        {
            var d = Dust.NewDustPerfect(Vector2.Lerp(v1, v2, n / (v1 - v2).Length()) + drawOffset, type, newColor: Color.White);
            action?.Invoke(d);
        }
    }

    #endregion 线条粒子

    public static Vector2 GetPlayerArmPosition(Projectile proj)
    {
        Player player = Main.player[proj.owner];
        Vector2 vector = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
        if (player.direction != 1)
        {
            vector.X = player.bodyFrame.Width - vector.X;
        }
        if (player.gravDir != 1f)
        {
            vector.Y = player.bodyFrame.Height - vector.Y;
        }
        vector -= new Vector2(player.bodyFrame.Width - player.width, player.bodyFrame.Height - 42) / 2f;
        return player.RotatedRelativePoint(player.MountedCenter - new Vector2(20f, 42f) / 2f + vector + Vector2.UnitY * player.gfxOffY);
    }

    public static void GetBoundPoints(Vector2 vs, ref Vector2 ve, out Vector2 start, out Vector2 end)
    {
        if ((vs - ve).Length() < 1f)
        {
            ve = vs + new Vector2(32, 0).RotatedBy(Main.rand.NextFloat(0, MathHelper.TwoPi));
        }
        if (vs.X - ve.X == 0)
        {
            start = new Vector2(vs.X, vs.Y > ve.Y ? -560 : 560);
            end = new Vector2(vs.X, vs.Y > ve.Y ? 560 : -560);
            return;
        }
        if (vs.Y - ve.Y == 0)
        {
            start = new Vector2(vs.X > ve.X ? -960 : 960, vs.Y);
            end = new Vector2(vs.X > ve.X ? 960 : -960, vs.Y);
            return;
        }
        float k = (vs.Y - ve.Y) / (vs.X - ve.X);
        float b = vs.Y - vs.X * (vs.Y - ve.Y) / (vs.X - ve.X);
        Vector2?[] vec1 = new Vector2?[] { new Vector2(-960, k * -960 + b), new Vector2(960, k * 960 + b), new Vector2((-560 - b) / k, -560), new Vector2((560 - b) / k, 560) };
        for (int n = 0; n < 4; n++)
        {
            if (n < 2)
            {
                if (vec1[n].Value.Y > 560 || vec1[n].Value.Y < -560)
                {
                    vec1[n] = null;
                }
            }
            else
            {
                if (vec1[n].Value.X > 960 || vec1[n].Value.X < -960)
                {
                    vec1[n] = null;
                }
            }
        }
        Vector2[] vecs2 = new Vector2[2];
        for (int i = 0; i < 2; i++)
        {
            for (int n = 0; n < 4; n++)
            {
                if (vec1[n].HasValue)
                {
                    vecs2[i] = vec1[n].Value;
                    vec1[n] = null;
                    break;
                }
            }
        }
        if (vs.X > ve.X)
        {
            start = vecs2[0].X > vecs2[1].X ? vecs2[1] : vecs2[0];
            end = vecs2[0].X > vecs2[1].X ? vecs2[0] : vecs2[1];
        }
        else
        {
            start = vecs2[0].X > vecs2[1].X ? vecs2[0] : vecs2[1];
            end = vecs2[0].X > vecs2[1].X ? vecs2[1] : vecs2[0];
        }
    }

    public static void ProjFrameChanger(this Projectile projectile, int frames, int time)
    {
        Main.projFrames[projectile.type] = frames;
        projectile.frame += (int)ModTime % time == 0 ? 1 : 0;
        projectile.frame %= frames;
    }

    public static bool ZoneForest(this Player player)
    {
        if (player.ZoneSkyHeight)
        {
            return false;
        }
        if (player.ZoneSnow)
        {
            return false;
        }
        if (player.ZoneDesert)
        {
            return false;
        }
        if (player.ZoneJungle)
        {
            return false;
        }
        //if (player.GetModPlayer<IllusionBoundPlayer>().ZoneStorm)
        //{
        //    return false;
        //}
        if (player.ZoneUnderworldHeight)
        {
            return false;
        }
        if (player.ZoneDungeon || player.zone4[5])
        {
            return false;
        }
        if (player.ZoneHallow)
        {
            return false;
        }
        if (player.ZoneBeach)
        {
            return false;
        }
        if (player.ZoneCorrupt || player.ZoneCrimson)
        {
            return false;
        }
        return true;
    }

    public static Vector2[] EasierVec2Array(params float[] v)
    {
        var len = v.Length;
        if (len < 1)
        {
            return null;
        }

        var l = new List<Vector2>();
        for (int n = 0; n < len / 2; n++)
        {
            float y = 2 * n + 1 == len ? 0 : v[2 * n + 1];
            l.Add(new Vector2(v[2 * n], y));
        }
        return [.. l];
    }

    #region 判定

    public static bool PointHit(this Rectangle target, Func<float, Vector2> vectorFunc, int times = 25)
    {
        if (vectorFunc == null)
        {
            return false;
        }

        for (int n = 0; n < times; n++)
        {
            var p = vectorFunc.Invoke(n / (times - 1f)).ToPoint();
            if (target.Contains(p.X, p.Y))
            {
                return true;
            }
        }
        return false;
    }

    public static bool RectangleHit(this Rectangle target, Func<float, Vector2> vectorFunc, Point size, int times = 25)
    {
        if (vectorFunc == null)
        {
            return false;
        }

        for (int n = 0; n < times; n++)
        {
            if (vectorFunc.Invoke(n / (times - 1f)).RectangleHit(target, size))
            {
                return true;
            }
        }
        return false;
    }

    public static bool RectangleHit(this Func<float, Vector2> vectorFunc, Rectangle target, int width = 4, int height = 4, int times = 25)
    {
        if (vectorFunc == null)
        {
            return false;
        }

        for (int n = 0; n < times; n++)
        {
            if (vectorFunc.Invoke(n / (times - 1f)).RectangleHit(target, width, height))
            {
                return true;
            }
        }
        return false;
    }

    public static bool RectangleHit(this Vector2 vector, Rectangle target, Point size)
    {
        return vector.RectangleHit(target, size.X, size.Y);
    }

    public static bool RectangleHit(this Vector2 vector, Rectangle target, int width = 4, int height = 4)
    {
        return target.Intersects(new Rectangle((int)vector.X - width / 2, (int)vector.Y - height / 2, width, height));
    }

    #endregion 判定

    /// <summary>
    /// 威胁程度
    /// 未完工
    /// </summary>
    /// <param name="player"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float TreatDegree(this Player player, NPC target)
    {
        if (!target.active)
        {
            return 0;
        }

        float locationTreat = Vector2.Dot(player.Center - target.Center, target.velocity - player.velocity) / (target.Center - player.Center).LengthSquared();
        float baseDataTreat = target.life * target.defense / 10 * (1 / target.width / target.height / target.scale) + target.damage * (1 - 1 / target.width / target.height / target.scale);
        //Main.NewText(new Vector2(locationTreat, baseDataTreat));
        return locationTreat + baseDataTreat;
    }

    public static Vector2 RectangleCollision(Vector2 Position, Vector2 Velocity, int Width, int Height, Rectangle rectangle, bool fallThrough = false, bool fall2 = false, int gravDir = 1)
    {
        Vector2 result = Velocity;
        Vector2 nextPosition = Position + Velocity;
        Vector2 position = Position;
        if (position.X + Width < rectangle.X && nextPosition.X + Width > rectangle.X)
        {
            result.X = rectangle.X - nextPosition.X - Width;
            result.X -= 2;
        }
        if (position.X > rectangle.X + rectangle.Width && nextPosition.X < rectangle.X + rectangle.Width)
        {
            result.X = rectangle.X + rectangle.Width - nextPosition.X;
            result.X += 2;
        }
        if (position.Y + Height < rectangle.Y && nextPosition.Y + Height > rectangle.Y)
        {
            result.Y = 0;
        }
        if (position.Y > rectangle.Y + rectangle.Height && nextPosition.Y < rectangle.Y + rectangle.Height)
        {
            result.Y = 0;
        }
        return result;
    }

    public static bool EqualValue<T>(this IList<T> list, IList<T> target)
    {
        var count = list.Count;
        if (count != target.Count) return false;
        for (int n = 0; n < count; n++)
        {
            if (!list[n].Equals(target[n])) return false;
        }
        return true;
    }
}
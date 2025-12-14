using System.Collections.Generic;
using System.Linq;

namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

public static class VectorMethods
{
    #region 向量

    public static void GetClosestVectorsFromNPC(Vector2 center, int count, float maxDistance, out int[] indexs, out float[] Dists)
        => GetClosestVectors(center, from target in Main.npc
                                     where !target.friendly && target.CanBeChasedBy() && target.active
                                     select (target.Center, target.whoAmI), count, maxDistance, out indexs, out Dists);

    public static void GetClosestVectors(Vector2 center, IEnumerable<Vector2> vectors, int count, float maxDistance, out int[] indexs, out float[] Dists)
    {
        if (count < 1) throw new ArgumentException("count must be greater than zero.");
        indexs = new int[count];
        Dists = new float[count];
        Array.Fill(indexs, -1);
        Array.Fill(Dists, float.PositiveInfinity);
        int counter = -1;
        foreach (var target in vectors)
        {
            counter++;
            float d = (target - center).Length();
            if (d > maxDistance) continue;
            for (int k = 0; k < count; k++)
            {
                if (d < Dists[k])
                {
                    for (int j = count - 1 - k; j > 0; j--)
                    {
                        indexs[j] = indexs[j - 1];
                        Dists[j] = Dists[j - 1];
                    }
                    indexs[k] = counter;
                    Dists[k] = d;
                    break;
                }
            }
        }
    }

    public static void GetClosestVectors(Vector2 center, IEnumerable<(Vector2 vec, int index)> vectors, int count, float maxDistance, out int[] indexs, out float[] Dists)
    {
        if (count < 1) throw new ArgumentException("count must be greater than zero.");
        indexs = new int[count];
        Dists = new float[count];
        Array.Fill(indexs, -1);
        Array.Fill(Dists, float.PositiveInfinity);
        foreach (var (vec, index) in vectors)
        {
            float d = (vec - center).Length();
            if (d > maxDistance) continue;
            for (int k = 0; k < count; k++)
            {
                if (d < Dists[k])
                {
                    for (int j = count - 1 - k; j > 0; j--)
                    {
                        indexs[j] = indexs[j - 1];
                        Dists[j] = Dists[j - 1];
                    }
                    indexs[k] = index;
                    Dists[k] = d;
                    break;
                }
            }
        }
    }

    public static (Vector2, float) AvgStd(this IEnumerable<Vector2> vectors) => (vectors.Avg(), vectors.Std());

    /// <summary>
    /// 求向量的标准差
    /// </summary>
    /// <param name="vectors"></param>
    /// <returns></returns>
    public static float Std(this IEnumerable<Vector2> vectors)
    {
        Vector2 avg = vectors.Avg();
        float value = 0f;
        foreach (var vec in vectors)
            value += (vec - avg).LengthSquared();
        value /= vectors.Count();
        value = MathF.Sqrt(value);
        return value;
    }

    /// <summary>
    /// 求向量的平均值
    /// </summary>
    /// <param name="vectors"></param>
    /// <returns></returns>
    public static Vector2 Avg(this IEnumerable<Vector2> vectors)
    {
        Vector2 result = default;
        foreach (var vec in vectors)
            result += vec;
        result /= vectors.Count();
        return result;
    }

    public static float Cos(Vector2 a, Vector2 b, Vector2 center = default)
    {
        a -= center;
        b -= center;
        if (a == default || b == default) return 0;
        return Vector2.Dot(a, b) / a.Length() / b.Length();
    }

    public static Vector2 Lerp(Vector2 from, Vector2 to, Vector2 t, bool clamp = true)
    {
        Vector2 result = default;
        result.X = MathHelper.Lerp(from.X, to.X, t.X);
        result.Y = MathHelper.Lerp(from.Y, to.Y, t.Y);
        if (clamp)
            result = Vector2.Clamp(result, from, to);
        return result;
    }

    public static Vector3 Lerp(Vector3 from, Vector3 to, Vector3 t, bool clamp = true)
    {
        Vector3 result = default;
        result.X = MathHelper.Lerp(from.X, to.X, t.X);
        result.Y = MathHelper.Lerp(from.Y, to.Y, t.Y);
        result.Z = MathHelper.Lerp(from.Z, to.Z, t.Z);

        if (clamp)
            result = Vector3.Clamp(result, from, to);
        return result;
    }

    public static Vector4 Lerp(Vector4 from, Vector4 to, Vector4 t, bool clamp = true)
    {
        Vector4 result = default;
        result.X = MathHelper.Lerp(from.X, to.X, t.X);
        result.Y = MathHelper.Lerp(from.Y, to.Y, t.Y);
        result.Z = MathHelper.Lerp(from.Z, to.Z, t.Z);
        result.W = MathHelper.Lerp(from.W, to.W, t.W);
        if (clamp)
            result = Vector4.Clamp(result, from, to);
        return result;
    }

    public static Vector2 GetLerpValue(Vector2 from, Vector2 to, Vector2 t, bool clamped = false)
    {
        return new Vector2(
            Utils.GetLerpValue(from.X, to.X, t.X, clamped),
            Utils.GetLerpValue(from.Y, to.Y, t.Y, clamped)
            );
    }

    public static Vector3 GetLerpValue(Vector3 from, Vector3 to, Vector3 t, bool clamped = false)
    {
        return new Vector3(
            Utils.GetLerpValue(from.X, to.X, t.X, clamped),
            Utils.GetLerpValue(from.Y, to.Y, t.Y, clamped),
            Utils.GetLerpValue(from.Z, to.Z, t.Z, clamped)
            );
    }

    public static Vector4 GetLerpValue(Vector4 from, Vector4 to, Vector4 t, bool clamped = false)
    {
        return new Vector4(
            Utils.GetLerpValue(from.X, to.X, t.X, clamped),
            Utils.GetLerpValue(from.Y, to.Y, t.Y, clamped),
            Utils.GetLerpValue(from.Z, to.Z, t.Z, clamped),
            Utils.GetLerpValue(from.W, to.W, t.W, clamped)
            );
    }

    public static Vector2 Projectile(this Vector3 vector, float height, Vector2 center = default)
    {
        return (new Vector2(vector.X, vector.Y) - center) * height / (height - vector.Z) + center;
    }

    public static Vector3 Projectile(this Vector4 vector, float height, Vector3 center = default)
    {
        return (new Vector3(vector.X, vector.Y, vector.Z) - center) * height / (height - vector.W) + center;
    }

    /// <summary>
    /// 点关于线对称
    /// </summary>
    /// <param name="target"></param>
    /// <param name="lineStart"></param>
    /// <param name="lineEnd"></param>
    /// <returns></returns>
    public static Vector2 Symmetric(this Vector2 target, Vector2 lineStart, Vector2 lineEnd)
    {
        var n = lineStart - lineEnd;
        n = new Vector2(-n.Y, n.X);
        return target + 2 * Vector2.Dot(n, lineStart - target) / n.LengthSquared() * n;
        //return lineStart + lineEnd - target;
    }

    /// <summary>
    /// CatMullRom插值，但是是数组
    /// </summary>
    /// <param name="vecs"></param>
    /// <param name="extraLength">拓展长度</param>
    /// <returns></returns>
    public static Vector2[] CatMullRomCurve(this Vector2[] vecs, int extraLength)
    {
        int l = vecs.Length;
        extraLength += l;
        Vector2[] scVecs = new Vector2[extraLength];
        for (int n = 0; n < extraLength; n++)
        {
            float t = n / (float)extraLength;
            float k = (l - 1) * t;
            int i = (int)k;
            float vk = k % 1;
            if (i == 0)
            {
                scVecs[n] = Vector2.CatmullRom(2 * vecs[0] - vecs[1], vecs[0], vecs[1], vecs[2], vk);
            }
            else if (i == l - 2)
            {
                scVecs[n] = Vector2.CatmullRom(vecs[l - 3], vecs[l - 2], vecs[l - 1], 2 * vecs[l - 1] - vecs[l - 2], vk);
            }
            else
            {
                scVecs[n] = Vector2.CatmullRom(vecs[i - 1], vecs[i], vecs[i + 1], vecs[i + 2], vk);
            }
        }
        return scVecs;
    }

    /// <summary>
    /// CatMullRom插值，但是是数组,还可以选定范围！（那为什么不用数组的那个运算符
    /// </summary>
    /// <param name="vecs"></param>
    /// <param name="extraLength">拓展长度</param>
    /// <returns></returns>
    public static Vector2[] CatMullRomCurve(this Vector2[] vecs, int extraLength, (int start, int end) range)
    {
        if (range.start >= range.end)
        {
            throw new Exception("你丫的找茬是吧，起点下标(start)必须小于终点下标(end)");
        }

        var (s, e) = range;
        int l = e - s;
        extraLength += l;
        Vector2[] scVecs = new Vector2[extraLength];
        for (int n = 0; n < extraLength; n++)
        {
            float t = n / (float)extraLength;
            float k = (l - 1) * t;
            int i = (int)k;
            float vk = k % 1;
            if (i == 0)
            {
                scVecs[n] = Vector2.CatmullRom(2 * vecs[s] - vecs[1 + s], vecs[s], vecs[1 + s], vecs[2 + s], vk);
            }
            else if (i == l - 2)
            {
                scVecs[n] = Vector2.CatmullRom(vecs[l - 3 + s], vecs[l - 2 + s], vecs[l - 1 + s], 2 * vecs[l - 1 + s] - vecs[l - 2 + s], vk);
            }
            else
            {
                scVecs[n] = Vector2.CatmullRom(vecs[i - 1 + s], vecs[i + s], vecs[i + 1 + s], vecs[i + 2 + s], vk);
            }
        }
        return scVecs;
    }

    #endregion 向量

    #region 矩阵

    public static Vector2 ApplyMatrix(this Vector2 v, Matrix matrix)
    {
        return new Vector2(
            v.X * matrix.M11 + v.Y * matrix.M12,
            v.X * matrix.M21 + v.Y * matrix.M22
            );
    }

    public static Vector3 ApplyMatrix(this Vector3 v, Matrix matrix)
    {
        return new Vector3(
            v.X * matrix.M11 + v.Y * matrix.M12 + v.Z * matrix.M13,
            v.X * matrix.M21 + v.Y * matrix.M22 + v.Z * matrix.M23,
            v.X * matrix.M31 + v.Y * matrix.M32 + v.Z * matrix.M33
            );
    }

    public static Vector4 ApplyMatrix(this Vector4 v, Matrix matrix)
    {
        return new Vector4(
            v.X * matrix.M11 + v.Y * matrix.M12 + v.Z * matrix.M13 + v.W * matrix.M14,
            v.X * matrix.M21 + v.Y * matrix.M22 + v.Z * matrix.M23 + v.W * matrix.M24,
            v.X * matrix.M31 + v.Y * matrix.M32 + v.Z * matrix.M33 + v.W * matrix.M34,
            v.X * matrix.M41 + v.Y * matrix.M42 + v.Z * matrix.M43 + v.W * matrix.M44
            );
    }

    public static Vector2 ApplyMatrix(this Vector2 v, Vector2 i, Vector2 j)
    {
        return new Vector2(v.X * i.X + v.Y * j.X, v.X * i.Y + v.Y * j.Y);
    }

    public static Vector2 ApplyMatrix(this Vector2 v, float a, float b, float c, float d)
    {
        //(a b  (x
        // c d)  y)
        return new Vector2(v.X * a + v.Y + b, v.X * c + v.Y * d);
    }

    public static Matrix CreateRotationTransform(this Vector3 director, float rotation)
    {
        //var (s, c) = System.MathF.SinCos(rotation);
        var s = (float)Math.Sin(rotation);
        var c = (float)Math.Cos(rotation);
        var x = director.X;
        var y = director.Y;
        var z = director.Z;
        return new Matrix
        (
            x * x * (1 - c) + c, x * y * (1 - c) - z * s, x * z * (1 - c) + y * s, 0,
            x * y * (1 - c) + z * s, y * y * (1 - c) + c, y * z * (1 - c) - x * s, 0,
            x * z * (1 - c) - y * s, y * z * (1 - c) + x * s, z * z * (1 - c) + c, 0,
            0, 0, 0, 1
        );
    }

    #endregion 矩阵

#if false
    #region 抽象向量

    public static T Sum<T, TValue>(T[] values) where T : IVector<T, TValue>
    {
        var vec = values[0];
        for (int n = 1; n < values.Length; n++)
            vec.Add(values[n]);
        return vec;
    }

    #endregion 抽象向量
#endif

    #region 参数或者结果里有向量就姑且塞着了

    public static Vector2[] EdgePoints(this Vector2[] vecs, out Vector2 left)
    {
        if (vecs.Length < 3)
        {
            throw new ArgumentException("兄啊，三个点都没有，计算个锤子的凸包");
        }

        int index = -1;
        float? leftcoord = null;
        for (int n = 0; n < vecs.Length; n++)
        {
            if (leftcoord == null || leftcoord > vecs[n].X)
            {
                leftcoord = vecs[n].X;
                index = n;
            }
            //leftcoord = (leftcoord == null || leftcoord > vecs[n].X) ? vecs[n].X : leftcoord;
            //index = n;
        }
        var vec = vecs[index];
        left = vec;
        List<Vector2> result = [vec];
        do
        {
            Vector2 dir = new(0, -1);
            float value = -20000;
            foreach (var v in vecs)//.DifferenceSet(result)
            {
                if (v != vec)
                {
                    Vector2 _dir = v - vec;
                    float dot = Vector2.Dot(_dir, dir) / _dir.Length();
                    if (dot > value)
                    {
                        value = dot;
                        dir = _dir;
                        vec = v;
                    }
                }
            }
            if (vec != result[0])
            {
                result.Add(vec);
            }
        }
        while (vec == result[0]);
        return [.. result];
    }

    public static Vector2[] EdgePoints(this Vector2[] vecs)
    {
        if (vecs.Length < 3)
        {
            throw new ArgumentException("兄啊，三个点都没有，计算个锤子的凸包");
        }

        try
        {
            int index = -1;
            float? leftcoord = null;
            for (int n = 0; n < vecs.Length; n++)
            {
                if (leftcoord == null || leftcoord > vecs[n].X)
                {
                    leftcoord = vecs[n].X;
                    index = n;
                }
                //leftcoord = (leftcoord == null || leftcoord > vecs[n].X) ? vecs[n].X : leftcoord;
                //index = n;
            }
            var vec = vecs[index];
            List<Vector2> result = [vec];
            int count = 0;
            do
            {
                //foreach (var v in vecs.DifferenceSet(new Vector2[] { vec }))//.DifferenceSet(result)
                //{
                //    bool flag = true;
                //    foreach (var v2 in vecs.DifferenceSet(new Vector2[] { vec, v }))
                //    {
                //        flag &= (v - vec).CrossLength(v2 - vec) < 0;
                //    }
                //    if (flag) result.Add(v);
                //}
                var set1 = vecs.DifferenceSet(new Vector2[] { vec });
                for (int n = 0; n < set1.Length; n++)
                {
                    bool flag = true;
                    var set2 = vecs.DifferenceSet(new Vector2[] { vec, set1[n] });
                    for (int i = 0; i < set2.Length; i++)
                    {
                        try
                        {
                            flag &= (set1[n] - vec).CrossLength(set2[i] - vec) <= 0;
                        }
                        catch
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        vec = set1[n];
                        if (vec != result[0])
                        {
                            result.Add(vec);
                        }
                    }
                }
                count++;
                if (count > 100)
                {
                    throw new Exception("我抄发生什么事了" + result.Count);
                }
                //if (vec != result[0])
                //    result.Add(vec);
            }
            while (vec != result[0]);
            return [.. result];
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public static void RotatedBy(this Vector2[] vecs, float rotation, Vector2 center = default)
    {
        for (int n = 0; n < vecs.Length; n++)
        {
            vecs[n] = vecs[n].RotatedBy(rotation, center);
        }
    }

    public static void MulY(this Vector2[] vecs, float sclar)
    {
        for (int n = 0; n < vecs.Length; n++)
        {
            vecs[n].Y *= sclar;
        }
    }

    public static void MulX(this Vector2[] vecs, float sclar)
    {
        for (int n = 0; n < vecs.Length; n++)
        {
            vecs[n].X *= sclar;
        }
    }

    public static void Mul(this Vector2[] vecs, float sclar)
    {
        for (int n = 0; n < vecs.Length; n++)
        {
            vecs[n] *= sclar;
        }
    }

    public static void Mul(this Vector2[] vecs, Vector2 sclar)
    {
        for (int n = 0; n < vecs.Length; n++)
        {
            vecs[n] *= sclar;
        }
    }

    #endregion 参数或者结果里有向量就姑且塞着了
}
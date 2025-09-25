using System.Collections.Generic;
using Terraria.Utilities;

namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

public static class MathMethods
{
    #region 希尔伯特曲线生成，一时兴起

    private static List<int> FuncForHBC_1(this List<int> l1)
    {
        List<int> l = l1.CopyList();
        //ForeachFunc(l, (int v) => { v = 4 - v; });
        //l.ForEach((int v) => { v = 4 - v; });
        for (int n = 0; n < l.Count; n++)
        {
            //l[n] = 4 - l[n];
            l[n]++;
            l[n] %= 4;
            if (l[n] % 2 == 0)
            {
                l[n] = (l[n] + 2) % 4;
            }
            l[n] += 2;
            l[n] %= 4;
        }
        return l;
    }

    private static List<int> FuncForHBC_2(this List<int> l1)
    {
        List<int> l = l1.CopyList();
        //ForeachFunc(l, (int v) => { v = (v + 1) % 2 + v / 2 * 2; });
        //l.ForEach((int v) => { v = (v + 1) % 2 + v / 2 * 2; });
        for (int n = 0; n < l.Count; n++)
        {
            //l[n] = (l[n] + 1) % 2 + l[n] / 2 * 2;
            l[n]++;
            l[n] %= 4;
            if (l[n] % 2 == 0)
            {
                l[n] = (l[n] + 2) % 4;
            }
        }
        return l;
    }

    private static void Add<T>(this List<T> l1, List<T> l2)
    {
        l2.ForEach(l1.Add);
    }

    public static List<int> HBCIndex(int t = 1)
    {
        if (t < 1)
        {
            return null;
        }
        else if (t == 1)
        {
            return [0, 1, 2];
        }
        else
        {
            var l = HBCIndex(t - 1);
            var ml = new List<int>
                {
                    l.FuncForHBC_2(),
                    0,
                    l,
                    1,
                    l,
                    2,
                    l.FuncForHBC_1()
                };
            return ml;
        }
    }

    public static List<Vector2> HBCPoint(this List<int> index)
    {
        var l = new List<Vector2>() { default };
        for (int n = 0; n < index.Count; n++)
        {
            Vector2 vec;
            switch (index[n])
            {
                case 0:
                    {
                        vec = new Vector2(0, 1);
                        break;
                    }
                case 1:
                    {
                        vec = new Vector2(1, 0);
                        break;
                    }
                case 2:
                    {
                        vec = new Vector2(0, -1);
                        break;
                    }
                case 3:
                    {
                        vec = new Vector2(-1, 0);
                        break;
                    }
                default:
                    {
                        vec = new Vector2(0, 1);
                        break;
                    }
            }
            l.Add(vec + l[n]);
        }
        //index.ForEach((int v) =>
        //{
        //	switch (v)
        //	{
        //		case 0:
        //			{
        //				l.Add(new Vector2(0, 1));
        //				break;
        //			}
        //		case 1:
        //			{
        //				l.Add(new Vector2(1, 0));
        //				break;
        //			}
        //		case 2:
        //			{
        //				l.Add(new Vector2(0, -1));
        //				break;
        //			}
        //		case 3:
        //			{
        //				l.Add(new Vector2(-1, 0));
        //				break;
        //			}
        //		default:
        //			{
        //				l.Add(new Vector2(0, 1));
        //				break;
        //			}
        //	}
        //});
        //ForeachFunc(index, (int v) =>
        // {
        //	 switch (v)
        //	 {
        //		 case 0:
        //			 {
        //				 l.Add(new Vector2(0, 1));
        //				 break;
        //			 }
        //		 case 1:
        //			 {
        //				 l.Add(new Vector2(1, 0));
        //				 break;
        //			 }
        //		 case 2:
        //			 {
        //				 l.Add(new Vector2(0, -1));
        //				 break;
        //			 }
        //		 case 3:
        //			 {
        //				 l.Add(new Vector2(-1, 0));
        //				 break;
        //			 }
        //		 default:
        //			 {
        //				 l.Add(new Vector2(0, 1));
        //				 break;
        //			 }
        //	 }
        // });
        return l;
    }

    /// <summary>
    /// 0到1从正方形的左下角到正方形右下角，n阶伪希尔伯特曲线轨迹（这个适合用来求单个点
    /// </summary>
    /// <param name="fac">0到1的一个浮点数</param>
    /// <param name="t">阶数，至少且默认为1</param>
    /// <returns></returns>
    public static Vector2 HBCFacFunc(this float fac, int t = 1)
    {
        if (t < 1)
        {
            return default;
        }

        return fac.ArrayLerp(HBCIndex(t).HBCPoint().ToArray());
    }

    #endregion 希尔伯特曲线生成，一时兴起

    public static float CrossLength(this Vector2 O, Vector2 A)
    {
        return O.X * A.Y - O.Y * A.X;
    }

    public static bool InTriangle(this Vector2 O, Vector2 A, Vector2 B, Vector2 C)
    {
        Vector2 v1 = O - A;
        Vector2 v2 = O - B;
        Vector2 v3 = O - C;
        Vector2 v4 = B - A;
        Vector2 v5 = C - B;
        Vector2 v6 = A - C;
        bool flag1 = v1.CrossLength(v4) >= 0 && v2.CrossLength(v5) >= 0 && v3.CrossLength(v6) >= 0;
        //bool flag2 = O.CrossLength(A) <= 0 && O.CrossLength(B) <= 0 && O.CrossLength(C) <= 0;
        return flag1;
    }

    /// <summary>
    /// 求t对应的贝塞尔曲线坐标
    /// </summary>
    /// <param name="pos">节点</param>
    /// <param name="t">时间</param>
    /// <returns></returns>
    public static Vector2 BesselCurve(this Vector2[] pos, double t)
    {
        int n = pos.Length - 1;
        if (n == 0)
        {
            return Vector2.Zero;
        }

        if (t == 0)
        {
            return pos[0];
        }
        Vector2 p = Vector2.Zero;
        for (int i = 0; i <= n; i++)
        {
            p += pos[i] * Combination(i, n) * (float)(Math.Pow(1 - t, n - i) * Math.Pow(t, i));
        }
        return p;
    }

    public static Vector2[] BesselCurve(this Vector2[] pos, int length)
    {
        Vector2[] curvePoses = new Vector2[length];
        for (int n = 0; n < length; n++)
        {
            curvePoses[n] = pos.BesselCurve(n / (length - 1f));
        }
        return curvePoses;
    }

    public static int NextPow(this UnifiedRandom rand, int min, int max, int times, bool aMax = false)
    {
        for (int n = 0; n < times - 1; n++)
        {
            if (aMax)
            {
                max = rand.Next(min, max);
            }
            else
            {
                min = rand.Next(min, max);
            }
        }
        return rand.Next(min, max);
    }

    public static double GaussianRandom(this UnifiedRandom random, double mu, double sigma)
    {
        double u = -2 * Math.Log(random.NextDouble());
        double v = 2 * Math.PI * random.NextDouble();
        return Math.Sqrt(u) * Math.Cos(v) * sigma + mu;
    }

    /// <summary>
    /// 计算阶乘
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static int Factorial(this int n)
    {
        if (n < 0) throw new ArgumentException("N must be an unsigned integer.");
        if (n == 0) return 1;
        var result = 1;
        for (int k = 2; k < n + 1; k++) result *= k;
        return result;
    }

    /// <summary>
    /// 计算组合数
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>a取b，即a的阶乘除以b的阶乘乘(a-b)的阶乘的积</returns>
    public static int Combination(int a, int b) => a.Factorial() / b.Factorial() / (a - b).Factorial();

    #region 银月的碰撞检测

    public static bool LineCheck(Vector2 start, Vector2 end, float width, Rectangle hitbox)
    {
        Vector2 v = end - start;
        v.Normalize();
        v = new Vector2(v.Y, -v.X);
        Triangle t1 = new(start + v * width, start - v * width, end + v * width);
        Triangle t2 = new(end + v * width, end - v * width, start - v * width);
        Triangle t3 = new(new Vector2(hitbox.X, hitbox.Y),
            new Vector2(hitbox.X, hitbox.Y + hitbox.Height),
            new Vector2(hitbox.X + hitbox.Width, hitbox.Y + hitbox.Width));
        Triangle t4 = new(new Vector2(hitbox.X, hitbox.Y),
            new Vector2(hitbox.X + hitbox.Width, hitbox.Y),
            new Vector2(hitbox.X + hitbox.Width, hitbox.Y + hitbox.Width));
        if (Triangle.Intersect(t1, t3))
        {
            return true;
        }
        if (Triangle.Intersect(t1, t4))
        {
            return true;
        }
        if (Triangle.Intersect(t2, t3))
        {
            return true;
        }
        if (Triangle.Intersect(t2, t4))
        {
            return true;
        }
        return false;
    }

    //三角类
    private class Triangle
    {
        private Vector2 vertex1;
        private Vector2 vertex2;
        private Vector2 vertex3;
        private Vector2 center;
        private Line_Segment line1, line2, line3;

        public Triangle(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            vertex1 = v1;
            vertex2 = v2;
            vertex3 = v3;
            Reset();
        }

        public Vector2 Vertex1
        {
            get
            {
                return vertex1;
            }
            set
            {
                vertex1 = value;
                Reset();
            }
        }

        public Vector2 Vertex2
        {
            get
            {
                return vertex2;
            }
            set
            {
                vertex2 = value;
                Reset();
            }
        }

        public Vector2 Vertex3
        {
            get
            {
                return vertex3;
            }
            set
            {
                vertex3 = value;
                Reset();
            }
        }

        public Vector2 Center
        {
            get
            {
                return center;
            }
        }

        public static bool Intersect(Triangle triangle1, Triangle triangle2)
        {
            if (Line_Segment.Intersect(triangle1.line1, triangle2.line1))
            {
                return true;
            }
            if (Line_Segment.Intersect(triangle1.line1, triangle2.line2))
            {
                return true;
            }
            if (Line_Segment.Intersect(triangle1.line1, triangle2.line3))
            {
                return true;
            }
            if (Line_Segment.Intersect(triangle1.line2, triangle2.line1))
            {
                return true;
            }
            if (Line_Segment.Intersect(triangle1.line2, triangle2.line2))
            {
                return true;
            }
            if (Line_Segment.Intersect(triangle1.line2, triangle2.line3))
            {
                return true;
            }
            if (Line_Segment.Intersect(triangle1.line3, triangle2.line1))
            {
                return true;
            }
            if (Line_Segment.Intersect(triangle1.line3, triangle2.line2))
            {
                return true;
            }
            if (Line_Segment.Intersect(triangle1.line3, triangle2.line3))
            {
                return true;
            }
            if (Point_In_Triangle(triangle1.Vertex1, triangle2) || Point_In_Triangle(triangle1.Vertex2, triangle2) || Point_In_Triangle(triangle1.Vertex3, triangle2))
            {
                return true;
            }
            if (Point_In_Triangle(triangle2.Vertex1, triangle1) || Point_In_Triangle(triangle2.Vertex2, triangle1) || Point_In_Triangle(triangle2.Vertex3, triangle1))
            {
                return true;
            }
            return false;
        }

        public static bool Point_In_Triangle(Vector2 point, Triangle triangle)
        {
            bool flag1, flag2, flag3;
            flag1 = Line_Segment.Is_Same_Side(point, triangle.Center, triangle.line1);
            flag2 = Line_Segment.Is_Same_Side(point, triangle.Center, triangle.line2);
            flag3 = Line_Segment.Is_Same_Side(point, triangle.Center, triangle.line3);
            if (flag1 && flag2 && flag3)
            {
                return true;
            }
            return false;
        }

        private void Reset()
        {
            ResetCenter();
            ResetLine();
        }

        private void ResetCenter()
        {
            center = (vertex1 + vertex2 + vertex3) / 3;
        }

        private void ResetLine()
        {
            line1 = new Line_Segment(vertex1, vertex2);
            line2 = new Line_Segment(vertex2, vertex3);
            line3 = new Line_Segment(vertex3, vertex1);
        }
    }

    //线段类
    private class Line_Segment
    {
        private Vector2 startpos, endpos;
        private float a, b, c;

        public Line_Segment(Vector2 start, Vector2 end)
        {
            startpos = start;
            endpos = end;
            ABC();
        }

        public Vector2 StartPos
        {
            get
            {
                return startpos;
            }
            set
            {
                startpos = value;
                ABC();
            }
        }

        public Vector2 EndPos
        {
            get
            {
                return endpos;
            }
            set
            {
                endpos = value;
                ABC();
            }
        }

        public static bool Intersect(Line_Segment line1, Line_Segment line2)
        {
            float x = (line2.B * line1.C - line1.B * line2.C) / (line1.B * line2.A - line2.B * line1.A);
            bool flag1 = Math.Min(line1.StartPos.X, line1.EndPos.X) < x;
            bool flag2 = Math.Max(line1.startpos.X, line1.EndPos.X) > x;
            bool flag3 = Math.Min(line2.StartPos.X, line2.EndPos.X) < x;
            bool flag4 = Math.Max(line2.StartPos.X, line2.EndPos.X) > x;
            if (flag1 && flag2 && flag3 && flag4)
            {
                return true;
            }
            return false;
        }

        public static bool Is_Same_Side(Vector2 pos1, Vector2 pos2, Line_Segment line)
        {
            return (line.A * pos1.X + line.B * pos1.Y + line.C) * (line.A * pos2.X + line.B * pos2.Y + line.C) > 0;
        }

        public float A
        {
            get
            {
                return a;
            }
        }

        public float B
        {
            get
            {
                return b;
            }
        }

        public float C
        {
            get
            {
                return c;
            }
        }

        private void ABC()
        {
            a = endpos.Y - startpos.Y;
            b = startpos.X - endpos.X;
            c = endpos.X * startpos.Y - endpos.Y * startpos.X;
        }
    }

    #endregion 银月的碰撞检测
}
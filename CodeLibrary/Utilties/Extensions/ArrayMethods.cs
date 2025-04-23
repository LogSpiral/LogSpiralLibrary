using System.Collections.Generic;
using System.Linq;
namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

public static class ArrayMethods
{
    public static List<T> CopyList<T>(this List<T> l)
    {
        var l1 = new List<T>();
        l.ForEach(l1.Add);
        return l1;
    }
    public static void ForeachFunc<T>(this IEnumerable<T> array, Action<T> action)
    {
        foreach (var elem in array)
            action.Invoke(elem);
    }
    public static void ForeachFunc<T>(this IEnumerable<T> array, Action<T, int> action)
    {
        int counter = 0;
        foreach (var elem in array)
            action.Invoke(elem, counter++);
    }
    public static T[] CloneArray<T>(this T[] ts)
    {
        T[] myArray = new T[ts.Length];
        for (int n = 0; n < ts.Length; n++)
            myArray[n] = ts[n];
        return myArray;
    }
    public static void UpdateArray<T>(this T[] array, T newValue, T defaultValue, bool when = true)
    {
        int length = array.Length;
        bool checkZero = true;
        for (int n = 0; n < length; n++)
        {
            checkZero &= array[n].GetHashCode() == default(T).GetHashCode();
        }
        if (checkZero)
        {
            for (int n = 0; n < length; n++)
            {
                array[n] = defaultValue;
            }
        }
        else
        {
            if (when)
            {
                for (int n = length - 1; n > 0; n--)
                {
                    array[n] = array[n - 1];
                }
                array[0] = newValue;
            }

        }
    }
    public static void UpdateArray<T>(this T[] array, T newValue, bool when = true)
    {
        if (when)
        {
            for (int n = array.Length - 1; n > 0; n--)
            {
                array[n] = array[n - 1];
            }
            array[0] = newValue;
        }
    }
    private static void InsertSort(float[] arr)
    {
        // 检查数据合法性
        if (arr == null)
        {
            return;
        }
        for (int i = 1; i < arr.Length; i++)
        {
            float tmp = arr[i];
            int j;
            for (j = i - 1; j >= 0; j--)
            {
                //如果比tmp大把值往后移动一位
                if (arr[j] > tmp)
                {
                    arr[j + 1] = arr[j];
                }
                else
                {
                    break;
                }
            }
            arr[j + 1] = tmp;
        }
    }
    public static void Reverse<T>(this T[] values)
    {
        var backup = values.CloneArray();
        for (int n = 0; n < values.Length; n++)
        {
            values[n] = backup[values.Length - n - 1];
        }
    }
    public static Vector2[] ClockwiseSorting(this Vector2[] vectors)
    {
        var result = new Vector2[vectors.Length];
        float? value = null;
        //Vector2 vec = default;
        int index = -1;
        for (int n = 0; n < vectors.Length; n++)
        {
            if (value == null || vectors[n].X < value)
            {
                value = vectors[n].X;
                //vec = vectors[n];
                index = n;
            }
        }
        result[0] = vectors[index];
        Dictionary<float, Vector2> myDic = [];
        for (int n = 0; n < vectors.Length; n++)
        {
            if (n != index)
            {
                myDic.Add(Vector2.Dot(new Vector2(0, 1), vectors[n] - result[0]) / (vectors[n] - result[0]).Length(), vectors[n]);
            }
        }
        var myArray = myDic.Keys.ToArray();
        InsertSort(myArray);
        myArray.Reverse();
        for (int n = 0; n < myArray.Length; n++)
        {
            result[n + 1] = myDic[myArray[n]];
        }
        return result;
    }
    public static T[] DifferenceSet<T>(this T[] A, IEnumerable<T> B)
    {
        List<T> result = [];
        for (int n = 0; n < A.Length; n++)
        {
            T item = A[n];
            if (!B.Contains(item))
            {
                result.Add(item);
            }
        }
        return [.. result];
    }
    public static T[] DelRepeatData<T>(this T[] array)
    {
        return [.. array.GroupBy(p => p).Select(p => p.Key)];
    }
    public static List<Vector2> CalcConvexHull(this List<Vector2> list)
    {
        List<Vector2> resPoint = [];
        //查找最小坐标点
        int minIndex = 0;
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].Y < list[minIndex].Y)
            {
                minIndex = i;
            }
        }
        Vector2 minPoint = list[minIndex];
        resPoint.Add(list[minIndex]);
        list.RemoveAt(minIndex);
        //坐标点排序
        list.Sort(
            delegate (Vector2 p1, Vector2 p2)
            {
                Vector2 baseVec;
                baseVec.X = 1;
                baseVec.Y = 0;

                Vector2 p1Vec;
                p1Vec.X = p1.X - minPoint.X;
                p1Vec.Y = p1.Y - minPoint.Y;

                Vector2 p2Vec;
                p2Vec.X = p2.X - minPoint.X;
                p2Vec.Y = p2.Y - minPoint.Y;

                double up1 = p1Vec.X * baseVec.X;
                double down1 = Math.Sqrt(p1Vec.X * p1Vec.X + p1Vec.Y * p1Vec.Y);

                double up2 = p2Vec.X * baseVec.X;
                double down2 = Math.Sqrt(p2Vec.X * p2Vec.X + p2Vec.Y * p2Vec.Y);


                double cosP1 = up1 / down1;
                double cosP2 = up2 / down2;

                if (cosP1 > cosP2)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            );
        resPoint.Add(list[0]);
        resPoint.Add(list[1]);
        for (int i = 2; i < list.Count; i++)
        {
            Vector2 basePt = resPoint[resPoint.Count - 2];
            Vector2 v1;
            v1.X = list[i - 1].X - basePt.X;
            v1.Y = list[i - 1].Y - basePt.Y;

            Vector2 v2;
            v2.X = list[i].X - basePt.X;
            v2.Y = list[i].Y - basePt.Y;

            if (v1.X * v2.Y - v1.Y * v2.X < 0)
            {
                resPoint.RemoveAt(resPoint.Count - 1);
                while (true)
                {
                    Vector2 basePt2 = resPoint[resPoint.Count - 2];
                    Vector2 v12;
                    v12.X = resPoint[resPoint.Count - 1].X - basePt2.X;
                    v12.Y = resPoint[resPoint.Count - 1].Y - basePt2.Y;
                    Vector2 v22;
                    v22.X = list[i].X - basePt2.X;
                    v22.Y = list[i].Y - basePt2.Y;
                    if (v12.X * v22.Y - v12.Y * v22.X < 0)
                    {
                        resPoint.RemoveAt(resPoint.Count - 1);
                    }
                    else
                    {
                        break;
                    }
                }
                resPoint.Add(list[i]);
            }
            else
            {
                resPoint.Add(list[i]);
            }
        }
        return resPoint;
    }
}

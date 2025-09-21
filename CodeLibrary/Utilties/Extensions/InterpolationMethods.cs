namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

/// <summary>
/// 插值辅助函数
/// </summary>
public static class InterpolationMethods
{
    /// <summary>
    /// 水滴插值，f(4x)的图像最像水滴，由一个四分之一圆和一段正弦波缝合
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static float WaterDropFactor(this float value)
    {
        value = MathHelper.Clamp(value, 0, 1);
        value *= 4;
        value -= 1;
        if (value < 0)
        {
            return MathF.Sqrt(1 - value * value);
        }
        return (MathF.Cos(MathHelper.Pi / 3 * value) + 1) * .5f;
    }

    /// <summary>
    /// 线性插值，我不知道为什么我还为这个写了个拓展函数
    /// </summary>
    /// <param name="t"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="clamp"></param>
    /// <returns></returns>
    public static float Lerp(this float t, float from, float to, bool clamp = false)
    {
        if (clamp) t = MathHelper.Clamp(t, 0, 1);
        return (1 - t) * from + t * to;
    }

    /// <summary>
    /// 折线，以1为周期，0取到0，0.5时取到1
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static float UpAndDown(this float t) => 1 - 2 * Math.Abs(t - (int)t - 0.5f);//这里是旧版实现，别问为什么有反三角MathF.Acos(MathF.Sin(MathHelper.Pi * (2 * t + 0.5f))) / MathHelper.Pi

    /// <summary>
    /// 更加柔和的floor
    /// 会有用到这货的那一天吗
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static float SmoothFloor(this float t)
    {
        t += .5f;
        var f = (int)Math.Floor(t);
        var g = MathF.Sin(MathHelper.TwoPi * t) / MathHelper.TwoPi + t - .5f;
        return MathHelper.Lerp(f, g, Math.Abs(f - g) * 2);
    }

    public static float SmoothSymmetricFactor(this float value, float whenGetMax)
    {
        return MathHelper.SmoothStep(0, 1, value.SymmetricalFactor(0.5f, whenGetMax));
    }

    /// <summary>
    /// 阿汪超喜欢用的插值函数，获得一个先上后下的插值
    /// </summary>
    /// <param name="value">丢进去的变量，取值范围一般是[0,2*center]</param>
    /// <param name="center">中间值，或者说最大值点</param>
    /// <param name="whenGetMax">决定丢进去的值与最大值的比值为多少时第一次达到最大值(1)，一般取(0,0.5f]</param>
    /// <returns>自己画函数图像去，不是三角形就是梯形(</returns>
    public static float SymmetricalFactor2(this float value, float center, float whenGetMax)
    {
        //return Clamp((center - Math.Abs(center - value)) / center / whenGetMax, 0, 1);
        return value.SymmetricalFactor(center, whenGetMax * center * 2);
    }

    /// <summary>
    /// 阿汪超喜欢用的插值函数，获得一个先上后下的插值
    /// </summary>
    /// <param name="value">丢进去的变量，取值范围一般是[0,2*center]</param>
    /// <param name="center">中间值，或者说最大值点</param>
    /// <param name="whenGetMax">决定丢进去的值为多少时第一次达到最大值(1)，一般取(0,center]</param>
    /// <returns>自己画函数图像去，不是三角形就是梯形(</returns>
    public static float SymmetricalFactor(this float value, float center, float whenGetMax)
    {
        return MathHelper.Clamp((center - Math.Abs(center - value)) / whenGetMax, 0, 1);
    }

    /// <summary>
    /// 阿汪超喜欢用的插值函数，获得一个先迅速增加再慢慢变小的插值
    /// </summary>
    /// <param name="value">丢进去的变量，取值范围一般是[0,maxTimeWhen]</param>
    /// <param name="maxTimeWhen">什么时候插值结束呢</param>
    /// <returns>自己画函数图像去，真的像是一个小山丘一样(</returns>
    public static float HillFactor2(this float value, float maxTimeWhen = 1) => MathF.Sqrt(value / maxTimeWhen).CosFactor();

    public static float CosFactor(this float value, float maxTimeWhen = 1)
    {
        return (1 - (float)Math.Cos(MathHelper.TwoPi * value / maxTimeWhen)) * 0.5f;
    }

    /// <summary>
    /// 阿汪超喜欢用的插值函数，获得一个先迅速增加再慢慢变小的插值
    /// </summary>
    /// <param name="value">丢进去的变量，取值范围一般是[0,maxTimeWhen]</param>
    /// <param name="maxTimeWhen">什么时候插值结束呢</param>
    /// <returns>自己画函数图像去，真的像是一个小山丘一样(</returns>
    public static float HillFactor(this float value, float maxTimeWhen = 1)
    {
        //return Clamp((center - Math.Abs(center - value)) / center / whenGetMax, 0, 1);
        return (float)Math.Sin(MathHelper.Pi * Math.Sqrt(value / maxTimeWhen));
    }

    #region 数组插值

    public static float ArrayLerp(this float factor, params float[] values)
    {
        if (factor <= 0)
        {
            return values[0];
        }
        else if (factor >= 1)
        {
            return values[values.Length - 1];
        }
        else
        {
            int c = values.Length - 1;
            int tier = (int)(c * factor);
            return MathHelper.Lerp(values[tier], values[tier + 1], c * factor % 1);
        }
    }

    public static Vector2 ArrayLerp(this float factor, params Vector2[] values)
    {
        if (factor <= 0)
        {
            return values[0];
        }
        else if (factor >= 1)
        {
            return values[values.Length - 1];
        }
        else
        {
            int c = values.Length - 1;
            int tier = (int)(c * factor);
            return Vector2.Lerp(values[tier], values[tier + 1], c * factor % 1);
        }
    }

    public static Vector4 ArrayLerp(this float factor, params Vector4[] values)
    {
        if (factor <= 0)
        {
            return values[0];
        }
        else if (factor >= 1)
        {
            return values[values.Length - 1];
        }
        else
        {
            int c = values.Length - 1;
            int tier = (int)(c * factor);
            return Vector4.Lerp(values[tier], values[tier + 1], c * factor % 1);
        }
    }

    public static Color ArrayLerp(this float factor, params Color[] values)
    {
        if (factor <= 0)
        {
            return values[0];
        }
        else if (factor >= 1)
        {
            return values[values.Length - 1];
        }
        else
        {
            int c = values.Length - 1;
            int tier = (int)(c * factor);
            return Color.Lerp(values[tier], values[tier + 1], c * factor % 1);
        }
    }

    public static float ArrayLerp_Loop(this float factor, params float[] values)
    {
        if (factor <= 0 || factor >= 1)
        {
            return values[0];
        }
        else
        {
            int c = values.Length;
            int tier = (int)(c * factor);
            return MathHelper.Lerp(values[tier], values[tier + 1 == c ? 0 : tier + 1], c * factor % 1);
        }
    }

    public static Vector2 ArrayLerp_Loop(this float factor, params Vector2[] values)
    {
        if (factor <= 0 || factor >= 1)
        {
            return values[0];
        }
        else
        {
            int c = values.Length;
            int tier = (int)(c * factor);
            return Vector2.Lerp(values[tier], values[tier + 1 == c ? 0 : tier + 1], c * factor % 1);
        }
    }

    public static Vector4 ArrayLerp_Loop(this float factor, params Vector4[] values)
    {
        if (factor <= 0 || factor >= 1)
        {
            return values[0];
        }
        else
        {
            int c = values.Length;
            int tier = (int)(c * factor);
            return Vector4.Lerp(values[tier], values[tier + 1 == c ? 0 : tier + 1], c * factor % 1);
        }
    }

    #endregion 数组插值
}
using Terraria.Graphics.Light;

namespace LogSpiralLibrary;

public partial class LogSpiralLibraryMod
{
    #region Renders

    /// <summary>
    /// 用来缓存某渲染链输入的图像内容
    /// </summary>
    public RenderTarget2D RenderOrig
    {
        get => field ??= DirectlyCreateNewRender();
        private set;
    }

    /// <summary>
    /// 缓存画布
    /// </summary>
    public RenderTarget2D Render
    {
        get => field ??= DirectlyCreateNewRender();
        private set;
    }

    /// <summary>
    /// 缓存画布二号
    /// </summary>
    public RenderTarget2D Render_Swap
    {
        get => field ??= DirectlyCreateNewRender();
        private set;
    }

    /// <summary>
    /// 缓存画布三号
    /// </summary>
    public RenderTarget2D Render_Swap2
    {
        get => field ??= DirectlyCreateNewRender();
        private set;
    }

    /// <summary>
    /// 降采样中号缓存画布
    /// <br>长宽都只有完整窗口的一半大</br>
    /// </summary>
    public RenderTarget2D Render_Tiny
    {
        get => field ??= DirectlyCreateNewRender(1);
        private set;
    }

    /// <summary>
    /// 降采样缓存画布二号
    /// <br>长宽都只有完整窗口的一半大</br>
    /// </summary>
    public RenderTarget2D Render_Tiny_Swap
    {
        get => field ??= DirectlyCreateNewRender(1);
        private set;
    }

    /// <summary>
    /// 降采样小号缓存画布
    /// <br>长宽都只有完整窗口的四分之一大</br>
    /// </summary>
    public RenderTarget2D Render_Tiniest
    {
        get => field ??= DirectlyCreateNewRender(2);
        private set;
    }

    /// <summary>
    /// 降采样小号缓存画布
    /// <br>长宽都只有完整窗口的四分之一大</br>
    /// </summary>
    public RenderTarget2D Render_Tiniest_Swap
    {
        get => field ??= DirectlyCreateNewRender(2);
        private set;
    }

    public RenderTarget2D RenderScreenCapture
    {
        get => field ??= DirectlyCreateNewRender();
        private set;
    }

    #endregion Renders

    #region 辅助函数

    /// <summary>
    /// 根据降采样等级构造<see cref="RenderTarget2D"/>实例
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private static RenderTarget2D DirectlyCreateNewRender(int level = 0)
        => new(Main.graphics.GraphicsDevice, Main.screenTarget.Width >> level, Main.screenTarget.Height >> level);

    /// <summary>
    /// 因为分辨率改变所以要重新实例化
    /// </summary>
    private static void CreateRender()
    {
        var instance = Instance;
        if (instance == null) return;

        instance.RenderOrig?.Dispose();
        instance.RenderOrig = DirectlyCreateNewRender();

        instance.Render?.Dispose();
        instance.Render = DirectlyCreateNewRender();

        instance.Render_Swap?.Dispose();
        instance.Render_Swap = DirectlyCreateNewRender();

        instance.Render_Swap2?.Dispose();
        instance.Render_Swap2 = DirectlyCreateNewRender();

        instance.Render_Tiny?.Dispose();
        instance.Render_Tiny = DirectlyCreateNewRender(1);

        instance.Render_Tiny_Swap?.Dispose();
        instance.Render_Tiny_Swap = DirectlyCreateNewRender(1);

        instance.Render_Tiniest?.Dispose();
        instance.Render_Tiniest = DirectlyCreateNewRender(2);

        instance.Render_Tiniest_Swap?.Dispose();
        instance.Render_Tiniest_Swap = DirectlyCreateNewRender(2);

        instance.RenderScreenCapture?.Dispose();
        instance.RenderScreenCapture = DirectlyCreateNewRender();
    }

    /// <summary>
    /// 在主线程上进行重新实例化画布
    /// </summary>
    /// <param name="useless"></param>
    private static void OnResolutionChanged_RenderCreate(Vector2 useless) => Main.RunOnMainThread(CreateRender);

    /// <summary>
    /// 将钩子挂给<see cref="Main.OnResolutionChanged"/>
    /// </summary>
    private static void AddOnResolutionChangedHook() => Main.OnResolutionChanged += OnResolutionChanged_RenderCreate;

    #endregion 辅助函数

    /// <summary>
    /// 用来判定当前是否能够使用<see cref="Main.screenTarget"/>
    /// <br>即光照模式需要不是复古或迷幻，同时开启水波质量</br>
    /// </summary>
    public static bool CanUseRender =>
        Lighting.Mode is not LightMode.Retro or LightMode.Trippy &&
        Main.WaveQuality != 0;
}
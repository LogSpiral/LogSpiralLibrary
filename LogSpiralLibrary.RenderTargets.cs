using Terraria.Graphics.Light;
namespace LogSpiralLibrary;

partial class LogSpiralLibraryMod
{
    #region Renders
    public RenderTarget2D Render
    {
        get => render ??= DirectCreateNewRender();
        set
        {
            render = value;
        }
    }
    private RenderTarget2D render;
    public RenderTarget2D Render_Swap
    {
        get => render_Swap ??= DirectCreateNewRender();
        set
        {
            render_Swap = value;
        }
    }
    private RenderTarget2D render_Swap;
    public RenderTarget2D Render_Swap2
    {
        get => render_Swap2 ??= DirectCreateNewRender();
        set
        {
            render_Swap2 = value;
        }
    }
    private RenderTarget2D render_Swap2;
    /// <summary>
    /// 处理Bloom效果专用的小RenderTarget2D
    /// </summary>
    public RenderTarget2D Render_Tiny
    {
        get => render_Tiny ??= DirectCreateNewRender(1);
        set
        {
            render_Tiny = value;
        }
    }
    private RenderTarget2D render_Tiny;

    /// <summary>
    /// 处理Bloom效果专用的小RenderTarget2D 2号
    /// </summary>
    public RenderTarget2D Render_Tiny_Swap
    {
        get => render_Tiny_Swap ??= DirectCreateNewRender(1);
        set
        {
            render_Tiny_Swap = value;
        }
    }
    private RenderTarget2D render_Tiny_Swap;

    /// <summary>
    /// 处理Bloom效果专用的小RenderTarget2D
    /// </summary>
    public RenderTarget2D Render_Tiniest
    {
        get => render_Tiniest ??= DirectCreateNewRender(2);
        set
        {
            render_Tiniest = value;
        }
    }
    private RenderTarget2D render_Tiniest;

    /// <summary>
    /// 处理Bloom效果专用的小RenderTarget2D 2号
    /// </summary>
    public RenderTarget2D Render_Tiniest_Swap
    {
        get => render_Tiniest_Swap ??= DirectCreateNewRender(2);
        set
        {
            render_Tiniest_Swap = value;
        }
    }
    private RenderTarget2D render_Tiniest_Swap;
    #endregion

    #region 辅助函数
    static RenderTarget2D DirectCreateNewRender(int level = 0)
    {
        int invert = 1 << level;
        if (level != 0)
            return new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenTarget.Width / invert, Main.screenTarget.Height / invert);
        else
            return new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenTarget.Width, Main.screenTarget.Height);
    }
    static void CreateRender()
    {
        var instance = Instance;
        instance.Render?.Dispose();
        instance.Render = DirectCreateNewRender();

        instance.Render_Swap?.Dispose();
        instance.Render_Swap = DirectCreateNewRender();

        instance.Render_Swap2?.Dispose();
        instance.Render_Swap2 = DirectCreateNewRender();

        instance.Render_Tiny?.Dispose();
        instance.Render_Tiny = DirectCreateNewRender(1);

        instance.Render_Tiny_Swap?.Dispose();
        instance.Render_Tiny_Swap = DirectCreateNewRender(1);

        instance.Render_Tiniest?.Dispose();
        instance.Render_Tiniest = DirectCreateNewRender(2);

        instance.Render_Tiniest_Swap?.Dispose();
        instance.Render_Tiniest_Swap = DirectCreateNewRender(2);
    }
    static void OnResolutionChanged_RenderCreate(Vector2 useless) => Main.RunOnMainThread(CreateRender);
    static void AddOnResolutionChangedHook() => Main.OnResolutionChanged += OnResolutionChanged_RenderCreate;
    #endregion

    public static bool CanUseRender => 
        Lighting.Mode is not LightMode.Retro or LightMode.Trippy && 
        Main.WaveQuality != 0;
}

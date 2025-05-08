using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
namespace LogSpiralLibrary;

public class RenderCanvasSystem : ModSystem
{
    #region 常量
    public const string DEFAULTCANVASNAME = "Default Canvas";
    #endregion

    #region 字段/属性
    static readonly Dictionary<string, RenderingCanvas> _renderingCanvases = [];

    static readonly Dictionary<string, Func<RenderingCanvas>> _renderCanvasFctory = [];

    public static Dictionary<Type, RenderDrawingContent> RenderDrawingContentInstance { get; } = [];

    public static IReadOnlyDictionary<string, RenderingCanvas> RenderingCanvases => _renderingCanvases;
    #endregion

    #region 公开函数
    public static RenderingCanvas CreateAndActivateCanvas(string canvasName)
    {
        if (_renderCanvasFctory.TryGetValue(canvasName, out var factory))
            return _renderingCanvases[canvasName] = factory?.Invoke();
        else
            throw new ArgumentException($"Could not find the factory of the canvas named {canvasName}");
    }

    public static void RegisterCanvasFactory(string name, Func<RenderingCanvas> factory)
    {
        if (_renderCanvasFctory.ContainsKey(name))
            LogSpiralLibraryMod.Instance.Logger.Warn($"Already registered the factory named {name}\n The previous one is replaced.");
        _renderCanvasFctory[name] = factory;
    }

    public static void RemoveCanvasFactory(string name)
    {
        if (!_renderCanvasFctory.Remove(name))
            LogSpiralLibraryMod.Instance.Logger.Warn($"The factory named {name} not Found. Nothing happens after calling this method.");
    }

    public static void AddRenderDrawingContent(string canvasName, IRenderDrawingContent content)
    {
        if (!_renderingCanvases.TryGetValue(canvasName, out var canvas))
            canvas = CreateAndActivateCanvas(canvasName);

        canvas.Add(content);
    }
    #endregion

    #region 重写函数
    static void UpdateCanvases()
    {

        HashSet<string> pendingRemoveCanvasName = [];
        foreach (var pair in _renderingCanvases)
        {
            Main.NewText(pair.Key);
            var canvas = pair.Value;
            canvas.Update();
            if (!canvas.ShouldExist) // && pair.Key != DEFAULTCANVASNAME // 是否有必要保留一个画布实例在？
                pendingRemoveCanvasName.Add(pair.Key);
        }
        foreach (var pendingName in pendingRemoveCanvasName)
            _renderingCanvases.Remove(pendingName);
    }

    public override void PostUpdateEverything() => UpdateCanvases();

    public override void Load()
    {
        // 服务器端大黑框自然用不到这些
        if (Main.netMode == NetmodeID.Server) return;
        // 挂起矩阵更新，下一次要用的时候就会先计算一下然后缓存着
        Main.OnPostDraw += delegate { _pendingUpdateViewMatrix = true; };
        // Filters.Scene.OnPostDraw += () => { _pendingUpdateViewMatrix = true; }; // 这个仅在色彩或白光模式下有

        // 注册默认画布
        // 默认画布上不会有任何渲染特效
        RegisterCanvasFactory(DEFAULTCANVASNAME, () => new RenderingCanvas());
        base.Load();
    }
    #endregion

    #region Matrix
    // 已替换为field关键字
    // static Matrix _uTransformCache;

    // static Matrix _uTransformUILayerCache;

    static bool _pendingUpdateViewMatrix;

    static bool _pendingUpdateUIMatrix;

    /// <summary>
    /// 控制是否计算ui层绘制矩阵
    /// </summary>
    static bool uiDrawing;

    /// <summary>
    /// spbdraw那边的矩阵
    /// </summary>
    public static Matrix TransformationMatrix
    {
        get
        {
            if (Main.gameMenu)
            {
                return Matrix.CreateScale(Main.instance.Window.ClientBounds.Width / (float)Main.screenWidth, Main.instance.Window.ClientBounds.Height / (float)Main.screenHeight, 1);
            }
            else if (uiDrawing)
            {
                return Matrix.Identity;
            }
            return Main.GameViewMatrix?.TransformationMatrix ?? Matrix.Identity;
        }
    }
    static Matrix Projection => Main.gameMenu ? Matrix.CreateOrthographicOffCenter(0, Main.instance.Window.ClientBounds.Width, Main.instance.Window.ClientBounds.Height, 0, 0, 1) : Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);//Main.screenWidth  Main.screenHeight
    static Matrix Model => Matrix.CreateTranslation(uiDrawing ? default : new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));

    /// <summary>
    /// 丢给顶点坐标变换的矩阵
    /// <br>先右乘<see cref="Model"/>将世界坐标转屏幕坐标</br>
    /// <br>再右乘<see cref="TransformationMatrix"/>进行画面缩放等</br>
    /// <br>最后右乘<see cref="Projection"/>将坐标压缩至[0,1]</br>
    /// </summary>
    public static Matrix uTransform
    {
        get
        {
            if (_pendingUpdateViewMatrix)
            {
                uiDrawing = false;
                field = Model * TransformationMatrix * Projection;
                _pendingUpdateViewMatrix = false;
            }
            return field;
        }
    }

    /// <summary>
    /// UI层变换矩阵
    /// </summary>
    public static Matrix uTransformUILayer
    {
        get
        {
            if (_pendingUpdateUIMatrix)
            {
                uiDrawing = true;
                field = Model * TransformationMatrix * Projection;
                _pendingUpdateUIMatrix = false;
            }
            return field;
        }
    }
    #endregion
}
public class RenderCanvasDrawing : RenderBasedDrawing
{
    static void DrawCanvases(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        foreach (var renderCanvas in RenderCanvasSystem.RenderingCanvases.Values)
            if (renderCanvas.RenderDrawingContents.Count > 0)
                renderCanvas.DrawContents(spriteBatch, graphicsDevice);
    }

    public override void CommonDrawingMethods(SpriteBatch spriteBatch)
        => DrawCanvases(spriteBatch, Main.graphics.GraphicsDevice);

    public override void RenderDrawingMethods(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        => DrawCanvases(spriteBatch, graphicsDevice);
}

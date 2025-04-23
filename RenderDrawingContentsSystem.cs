using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
namespace LogSpiralLibrary;

public class RenderDrawingContentsSystem : ModSystem
{
    public static Dictionary<Type, RenderDrawingContent> RenderDrawingContentInstance { get; } = [];
    public static RenderDrawingContent[] RenderDrawingContents { get; } = new RenderDrawingContent[1000];
    public override void PostUpdateEverything()
    {
        UpdateVertexInfo();
    }
    public static void UpdateVertexInfo() => RenderDrawingContents.UpdateVertexInfo();


    static bool _pendingUpdateViewMatrix;
    static bool uiDrawing;
    public static bool UIDrawing
    {
        get => uiDrawing;
        set
        {
            uiDrawing = value;
            _pendingUpdateViewMatrix = true;
        }
    }

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
            else if (UIDrawing)
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
                _uTransformCache = Model * TransformationMatrix * Projection;
                _pendingUpdateViewMatrix = false;
            }
            return _uTransformCache;
        }
    }
    static Matrix _uTransformCache;

    public override void Load()
    {
        Filters.Scene.OnPostDraw += () => { _pendingUpdateViewMatrix = true; };
        base.Load();
    }
}
public class LogSpitalLibraryRenderDrawing : RenderBasedDrawing
{
    static void DrawVertexEffects(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
    {
        Dictionary<Type, List<RenderDrawingContent>> dict = [];
        foreach (var instance in RenderDrawingContentsSystem.RenderDrawingContentInstance.Values)
        {
            dict.Add(instance.GetType(), []);
        }
        foreach (var renderDrawContent in RenderDrawingContentsSystem.RenderDrawingContents)
        {
            if (renderDrawContent != null && renderDrawContent.Active && dict.TryGetValue(renderDrawContent.GetType(), out var list))
            {
                list.Add(renderDrawContent);
            }
        }
        foreach (var pair in dict)
        {
            if (pair.Value.Count > 0)
            {
                RenderDrawingContent.DrawRenderDrawingContents(pair.Value, pair.Key, spriteBatch, graphicsDevice, render, renderAirDistort);

            }
        }
    }

    public override void CommonDrawingMethods(SpriteBatch spriteBatch)
        => DrawVertexEffects(spriteBatch, null, null, null);

    public override void RenderDrawingMethods(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
        => DrawVertexEffects(spriteBatch, graphicsDevice, render, renderAirDistort);

}

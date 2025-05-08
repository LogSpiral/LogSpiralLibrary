using System.Collections.Generic;
using Terraria.Graphics.Effects;
namespace LogSpiralLibrary;

public class RenderBasedDrawingSystem : ModSystem
{
    public static List<IRenderBasedDrawing> RenderBasedDrawings { get; } = [];

    public override void Load()
    {
        On_FilterManager.EndCapture += AddRenderBasedDrawings;
        On_Main.DrawProjectiles += AddNoRenderDrawings;
    }

    static void AddRenderBasedDrawings(On_FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
    {
        if (!LogSpiralLibraryMod.CanUseRender) goto label;
        foreach (var renderDrawing in RenderBasedDrawings)
        {
            try
            {
                renderDrawing.RenderDrawingMethods(Main.spriteBatch, Main.instance.GraphicsDevice, LogSpiralLibraryMod.Instance.Render, LogSpiralLibraryMod.Instance.Render_Swap);
            }
            catch
            {
                goto label;
            }
        }
    label:
        orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, clearColor);
    }

    static void AddNoRenderDrawings(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig.Invoke(self);
        if (LogSpiralLibraryMod.CanUseRender) return;
        foreach (var renderDrawing in RenderBasedDrawings)
            renderDrawing.CommonDrawingMethods(Main.spriteBatch);
    }
}

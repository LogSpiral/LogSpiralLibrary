using MonoMod.Cil;
using System.Collections.Generic;
using Terraria.Graphics.Effects;

namespace LogSpiralLibrary;

public class RenderBasedDrawingSystem : ModSystem
{
    public static List<IRenderBasedDrawing> RenderBasedDrawings { get; } = [];

    public override void Load()
    {
        On_FilterManager.EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 += AddRenderBasedDrawings;
        // IL_FilterManager.EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 += AddRenderBasedDrawings;
        On_Main.DrawProjectiles += AddNoRenderDrawings;
    }

    public override void Unload()
    {
        On_FilterManager.EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 -= AddRenderBasedDrawings;
        // IL_FilterManager.EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 -= AddRenderBasedDrawings;
        On_Main.DrawProjectiles -= AddNoRenderDrawings;
    }

    private static void AddRenderBasedDrawings(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchBr(out _))) return;
        cursor.EmitDelegate(() =>
        {
            if (!LogSpiralLibraryMod.CanUseRender || LogSpiralLibraryMod.Instance is not { } instance) return;
            foreach (var renderDrawing in RenderBasedDrawings)
            {
                try
                {
                    renderDrawing.RenderDrawingMethods(Main.spriteBatch, Main.instance.GraphicsDevice, instance.Render, instance.Render_Swap);
                }
                catch
                {
                    return;
                }
            }
        });
    }

    private static void AddRenderBasedDrawings(On_FilterManager.orig_EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Vector2 screenSize, Vector2 sceneSize, Vector2 sceneOffset)
    {
        if (!LogSpiralLibraryMod.CanUseRender || LogSpiralLibraryMod.Instance is not { } instance) goto label;
        foreach (var renderDrawing in RenderBasedDrawings)
        {
            try
            {
                renderDrawing.RenderDrawingMethods(Main.spriteBatch, Main.instance.GraphicsDevice, instance.Render, instance.Render_Swap);
            }
            catch
            {
                goto label;
            }
        }
    label:
        orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, screenSize, sceneSize, sceneOffset);
    }

    private static void AddNoRenderDrawings(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig.Invoke(self);
        if (LogSpiralLibraryMod.CanUseRender) return;
        foreach (var renderDrawing in RenderBasedDrawings)
            renderDrawing.CommonDrawingMethods(Main.spriteBatch);
    }
}
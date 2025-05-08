using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;

public class UltraCanvas : RenderDrawingContent
{
    public delegate void CanvasDrawing(UltraCanvas canvas, float scaler);

    public event CanvasDrawing OnDrawing;

    public override void Draw(SpriteBatch spriteBatch) => OnDrawing?.Invoke(this, AirDistortEffect.DistortScaler);

    public override void Update() => timeLeft--;

    public static UltraCanvas NewUltraCanvas(string canvasName, int timeLeft, CanvasDrawing drawingDelegate) 
    {
        var content = new UltraCanvas();
        content.timeLeftMax = content.timeLeft = timeLeft;
        content.OnDrawing += drawingDelegate;
        RenderCanvasSystem.AddRenderDrawingContent(canvasName, content);
        return content;
    }

    public static UltraCanvas NewUltraCanvasOnDefaultCanvas(int timeLeft, CanvasDrawing drawingDelegate)
        => NewUltraCanvas(RenderCanvasSystem.DEFAULTCANVASNAME, timeLeft, drawingDelegate);
}
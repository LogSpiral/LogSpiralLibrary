namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;

public interface IRenderDrawingContent
{
    /// <summary>
    /// 用于<see cref="SpriteBatch.Begin()">之类
    /// </summary>
    void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice);

    /// <summary>
    /// 正式讲内容绘制到画布上
    /// </summary>
    void Draw(SpriteBatch spriteBatch);

    /// <summary>
    /// 用于<see cref="SpriteBatch.End()">之类
    /// </summary>
    void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice);

    bool Active { get; }
}
namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;

public interface IRenderEffect
{
    /// <summary>
    /// 当前是否有效
    /// </summary>
    bool Active { get; }

    /// <summary>
    /// 是否绘制实际内容
    /// </summary>
    bool DoRealDraw { get; }

    /// <summary>
    /// 是否有重绘内容的需求，目前还未能作到多个渲染效果兼容...
    /// <br>不过好消息是目前应该也就只有空气扭曲用得到这玩意</br>
    /// </summary>
    bool RedrawContents(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) { return false; }

    /// <summary>
    /// 对输入的画布进行自己的加工处理
    /// </summary>
    void ProcessRender(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice,ref RenderTarget2D contentRender,ref RenderTarget2D assistRender);

}
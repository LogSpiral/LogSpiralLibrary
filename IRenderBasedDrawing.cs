namespace LogSpiralLibrary;

/// <summary>
/// 表示一个需要用到<see cref="Main.screenTarget"/>的渲染内容
/// <br>直接实现这个接口的话需要手动添加单例到<see cref="RenderBasedDrawingSystem.RenderBasedDrawings"/></br>
/// <br>可以考虑直接使用<see cref="RenderBasedDrawing"/></br>
/// </summary>
public interface IRenderBasedDrawing
{
    /// <summary>
    /// 对需要Render的弹幕进行合批绘制！！
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="graphicsDevice"></param>
    /// <param name="render">缓存画布</param>
    /// <param name="renderSwap">缓存画布二号</param>
    public abstract void RenderDrawingMethods(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap);

    /// <summary>
    /// 哦但是，如果那些要用到Render的弹幕在Render不可用的时候应该怎么办呢？？
    /// <br>这个函数用于在弹幕绘制完成后进行普通绘制</br>
    /// <br>硬要在这里塞含<see cref="Main.screenTarget"/>的内容也不是不行，只是不在捕获期间或者没有完全绘制</br>
    /// </summary>
    /// <param name="spriteBatch"></param>
    public abstract void CommonDrawingMethods(SpriteBatch spriteBatch);
}

/// <summary>
/// <see cref="IRenderBasedDrawing"/>的一个实现类型
/// <br>区别在于会自动添加注册</br>
/// <br>如果自己实现了别的需要使用<see cref="RenderTarget2D"/>的渲染内容</br>
/// <br>就还请自行添加到<see cref="RenderBasedDrawingSystem.RenderBasedDrawings"/>里吧</br>
/// </summary>
public abstract class RenderBasedDrawing : ModType, IRenderBasedDrawing
{
    public abstract void RenderDrawingMethods(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap);

    public abstract void CommonDrawingMethods(SpriteBatch spriteBatch);

    public override sealed void Register()
    {
        ModTypeLookup<RenderBasedDrawing>.Register(this);
        RenderBasedDrawingSystem.RenderBasedDrawings.Add(this);
    }
}
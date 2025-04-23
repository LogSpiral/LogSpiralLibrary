using System;
using System.Collections.Generic;
using System.Linq;
namespace LogSpiralLibrary;
// 改为接口类型了，之前继承一个ModType然后注册感觉挺意义不明
public interface IRenderBasedDrawing
{
    /// <summary>
    /// 对需要Render的弹幕进行合批绘制！！
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="graphicsDevice"></param>
    /// <param name="render">超级画布布！！</param>
    /// <param name="renderAirDistort">专门给空气扭曲用的Render,和上面那个没区别其实</param>
    public abstract void RenderDrawingMethods(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort);
    /// <summary>
    /// 哦但是，如果那些要用到Render的弹幕在Render不可用的时候应该怎么办呢？？
    /// </summary>
    /// <param name="spriteBatch"></param>
    public abstract void CommonDrawingMethods(SpriteBatch spriteBatch);
}
public abstract class RenderBasedDrawing : ModType, IRenderBasedDrawing 
{
    public abstract void RenderDrawingMethods(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort);
    public abstract void CommonDrawingMethods(SpriteBatch spriteBatch);
    public override sealed void Register()
    {
        ModTypeLookup<RenderBasedDrawing>.Register(this);
        RenderBasedDrawingSystem.RenderBasedDrawings.Add(this);
    }
}

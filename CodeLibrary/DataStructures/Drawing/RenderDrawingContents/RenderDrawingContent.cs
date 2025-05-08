using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;

public abstract class RenderDrawingContent : ModType, IRenderDrawingContentUpdatable
{
    /// <summary>
    /// 存在时长
    /// </summary>
    public int timeLeft;
    /// <summary>
    /// 最大存在时长，用于插值
    /// </summary>
    public int timeLeftMax;
    /// <summary>
    /// 是否处于激活状态
    /// </summary>
    public bool Active => timeLeft > 0;
    public float Factor => timeLeft / (float)timeLeftMax;
    public bool OnSpawn => timeLeft == timeLeftMax;
    /// <summary>
    /// 用于在特效与物体未解除绑定时，与物体的动画同步
    /// </summary>
    public bool autoUpdate = true;
    public sealed override void Register()
    {
        ModTypeLookup<RenderDrawingContent>.Register(this);
        RenderCanvasSystem.RenderDrawingContentInstance.Add(GetType(), this);
    }

    /// <summary>
    /// 代表元
    /// </summary>
    public RenderDrawingContent Representative => RenderCanvasSystem.RenderDrawingContentInstance[GetType()];


    /// <summary>
    /// 合批绘制前做的事情，这东西是静态性质的实例函数(?
    /// <br>实际上仅由<see cref="GlobalTimeSystem.vertexDrawInfoInstance">中的对应的实例执行</br>
    /// <br>需要<see cref="SpriteBatch.Begin"/>，可以在这里开启画布然后把东西画上之类</br>
    /// </summary>
    /// <param name="spriteBatch"></param>
    public virtual void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        spriteBatch.Begin(SpriteSortMode.Immediate, LogSpiralLibraryMod.NonPremultipliedFullAlpha, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, null, RenderCanvasSystem.TransformationMatrix);
    }
    /// <summary>
    /// 单个绘制
    /// </summary>
    /// <param name="spriteBatch"></param>
    public abstract void Draw(SpriteBatch spriteBatch);
    /// <summary>
    /// 合批绘制完毕记得<see cref="SpriteBatch.End"/>
    /// </summary>
    /// <param name="spriteBatch"></param>
    public virtual void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        spriteBatch.End();
    }
    /// <summary>
    /// 顶点数据更新
    /// </summary>
    public abstract void Update();


}

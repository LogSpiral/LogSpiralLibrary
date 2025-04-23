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

public abstract class RenderDrawingContent : ModType
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
    public float factor => timeLeft / (float)timeLeftMax;
    public bool OnSpawn => timeLeft == timeLeftMax;
    /// <summary>
    /// 用于在特效与物体未解除绑定时，与物体的动画同步
    /// </summary>
    public bool autoUpdate = true;
    public sealed override void Register()
    {
        ModTypeLookup<RenderDrawingContent>.Register(this);
        RenderDrawingContentsSystem.RenderDrawingContentInstance.Add(GetType(), this);
    }

    public IRenderDrawInfo[][] RenderDrawInfos = [[new AirDistortEffectInfo()], [new MaskEffectInfo(), new BloomEffectInfo()]];

    /// <summary>
    /// 代表元
    /// </summary>
    public RenderDrawingContent Representative => RenderDrawingContentsSystem.RenderDrawingContentInstance[GetType()];

    public void ModityAllRenderInfo(params IRenderDrawInfo[][] newInfos)
    {
        if (newInfos == null)
        {
            ResetAllRenderInfo();
            return;
        }
        var array = Representative.RenderDrawInfos;
        for (int n = 0; n < newInfos.Length; n++)
            array[n] = newInfos[n];
        OnModifyRenderInfo(newInfos);
    }
    public void ResetAllRenderInfo()
    {
        var array = Representative.RenderDrawInfos;
        for (int i = 0; i < array.Length; i++)
        {
            for (int j = 0; j < array[i].Length; j++)
                array[i][j].Reset();
        }
        OnModifyRenderInfo(array);
    }
    public virtual void OnModifyRenderInfo(IRenderDrawInfo[][] infos)
    {

    }

    public static void DrawRenderDrawingContents(IEnumerable<RenderDrawingContent> infos, Type type, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderSwap)
    {
        if (!RenderDrawingContentsSystem.RenderDrawingContentInstance.TryGetValue(type, out var instance)) return;
        var newInfos = from info in infos where info != null && info.Active select info;
        if (!newInfos.Any()) return;
        List<List<IRenderDrawInfo>> renderPipeLines = [];
        foreach (var pipe in instance.RenderDrawInfos)
        {
            var list = new List<IRenderDrawInfo>();
            foreach (var rInfo in pipe)
            {
                if (rInfo.Active)
                    list.Add(rInfo);
            }
            if (list.Count != 0)
                renderPipeLines.Add(list);
        }
        if (renderPipeLines.Count == 0 || !LogSpiralLibraryMod.CanUseRender || graphicsDevice == null)
        {
            instance.PreDraw(spriteBatch, graphicsDevice, render, renderSwap);
            foreach (var info in newInfos) info.Draw(spriteBatch);
            instance.PostDraw(spriteBatch, graphicsDevice, render, renderSwap);
        }
        else
        {
            bool realDraw = false;

            foreach (var pipeLine in renderPipeLines)
            {
                var realLine = pipeLine;
                if (!realLine.Any()) continue;
                int counter = 0;
                foreach (var renderEffect in realLine)
                {
                    if (counter == 0)
                    {
                        instance.PreDraw(spriteBatch, graphicsDevice, render, renderSwap);
                        renderEffect.PreDraw(spriteBatch, graphicsDevice, render, renderSwap);
                        foreach (var info in newInfos) info.Draw(spriteBatch);
                        instance.PostDraw(spriteBatch, graphicsDevice, render, renderSwap);
                    }
                    renderEffect.PostDraw(spriteBatch, graphicsDevice, render, renderSwap);
                    counter++;
                    if (counter == realLine.Count())
                    {
                        renderEffect.DrawToScreen(spriteBatch, graphicsDevice, render, renderSwap);
                    }
                    realDraw |= renderEffect.DoRealDraw;
                }

            }
            if (!realDraw)
            {
                instance.PreDraw(spriteBatch, graphicsDevice, render, renderSwap);
                foreach (var info in newInfos) info.Draw(spriteBatch);
                instance.PostDraw(spriteBatch, graphicsDevice, render, renderSwap);
            }
        }
    }

    /// <summary>
    /// 合批绘制前做的事情，这东西是静态性质的实例函数(?
    /// <br>实际上仅由<see cref="GlobalTimeSystem.vertexDrawInfoInstance">中的对应的实例执行</br>
    /// <br>需要<see cref="SpriteBatch.Begin"/>，可以在这里开启画布然后把东西画上之类</br>
    /// </summary>
    /// <param name="spriteBatch"></param>
    public virtual void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
    {
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone);
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
    public virtual void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D render, RenderTarget2D renderAirDistort)
    {
        spriteBatch.End();
    }
    /// <summary>
    /// 顶点数据更新
    /// </summary>
    public abstract void Uptate();


}

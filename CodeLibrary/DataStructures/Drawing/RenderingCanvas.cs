using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;

public sealed class RenderingCanvas // 应该没有继承的必要所以就直接sealed了
{
    #region 常量

    const int TIMELEFT = 180;

    #endregion

    #region 字段/属性

    int _timeLeft = TIMELEFT;

    IRenderEffect[][] _renderEffects = [];

    readonly Dictionary<Type, HashSet<IRenderDrawingContent>> _renderDrawingContents = [];

    public bool IsUILayer { get; init; }

    /// <summary>
    /// 如果什么东西都没有了就会开始倒计时
    /// <br>持续三秒钟没有任何东西就会从<see cref="RenderCanvasSystem._renderingCanvases">中清除掉</br>
    /// <br>目前和这个字典是耦合着的关系，不知道有没有更好的组织方式</br>
    /// </summary>
    public bool ShouldExist
    {
        get
        {
            if (_timeLeft > 0)
                return true;
            if (_renderDrawingContents.Count > 0)
            {
                _timeLeft = TIMELEFT;
                return true;
            }
            return false;
        }
    }

    HashSet<HashSet<IRenderEffect>> ActiveRenderEffects
    {
        get
        {
            HashSet<HashSet<IRenderEffect>> result = [];
            foreach (var pipe in _renderEffects)
            {
                var list = new HashSet<IRenderEffect>();
                foreach (var rInfo in pipe)
                    if (rInfo.Active)
                        list.Add(rInfo);
                if (list.Count != 0)
                    result.Add(list);
            }
            return result;
        }
    }

    public IReadOnlyDictionary<Type, HashSet<IRenderDrawingContent>> RenderDrawingContents => _renderDrawingContents;

    #endregion

    #region 构造函数

    public RenderingCanvas() { }

    public RenderingCanvas(IRenderEffect[][] renderEffects) => _renderEffects = renderEffects;

    #endregion

    /// <summary>
    /// 遍历各类型绘制对象并更新那些需要更新的
    /// 找出那些需要移除的绘制对象
    /// 如果每种绘制对象都已经空了，画布会开始倒计时
    /// 持续三秒以上就可以判定画布可以临时移除了
    /// </summary>
    public void Update()
    {
        if (RenderDrawingContents.Count == 0)
            _timeLeft--;
        HashSet<Type> pendingRemoveSets = [];
        foreach (var pair in RenderDrawingContents)
        {
            var sets = pair.Value;
            if (sets.Count == 0)
                goto label;
            HashSet<IRenderDrawingContent> pendingRemoveContents = [];
            foreach (var content in sets)
            {
                if (content is IRenderDrawingContentUpdatable updatableContent)
                    updatableContent.Update();
                if (!content.Active)
                    pendingRemoveContents.Add(content);
            }
            sets.ExceptWith(pendingRemoveContents);
            if (sets.Count == 0)
                goto label;
            continue;
        label:
            pendingRemoveSets.Add(pair.Key);
        }
        foreach (var pendings in pendingRemoveSets)
            _renderDrawingContents.Remove(pendings);

    }

    /// <summary>
    /// 暂且没想到更好的做法，就姑且直接覆盖原有的了
    /// </summary>
    /// <param name="newRenderEffects"></param>
    public void ModifyRenderEffects(IRenderEffect[][] newRenderEffects)
    {
        _renderEffects = newRenderEffects;
    }

    /// <summary>
    /// 将绘制内容添加到画布上，并根据类型自动分类
    /// 同时刷新画布的清除倒计时
    /// </summary>
    /// <param name="content"></param>
    public void Add(IRenderDrawingContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var type = content.GetType();
        if (!_renderDrawingContents.TryGetValue(content.GetType(), out var set) || set is null)
            set = _renderDrawingContents[type] = [];
        set.Add(content);
        _timeLeft = TIMELEFT;
    }

    static void CanvasPreDraw(bool isUILayer)
    {
        if (isUILayer)
            LogSpiralLibraryMod.ShaderSwooshUL.Parameters["uTransform"].SetValue(RenderCanvasSystem.uTransformUILayer);
    }

    static void DirectlyDrawSingleGroup(IEnumerable<IRenderDrawingContent> drawingContents, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, bool isUILayer)
    {
        var instance = drawingContents.First();
        instance.PreDraw(spriteBatch, graphicsDevice);
        CanvasPreDraw(isUILayer);
        foreach (var info in drawingContents) info.Draw(spriteBatch);
        instance.PostDraw(spriteBatch, graphicsDevice);
    }

    void DirectlyDrawAllGroups(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        foreach (var drawingContentGroup in _renderDrawingContents.Values)
        {
            if (drawingContentGroup.Count == 0) continue;
            DirectlyDrawSingleGroup(drawingContentGroup, spriteBatch, graphicsDevice, IsUILayer);
        }
    }

    public void DrawContents(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        HashSet<HashSet<IRenderEffect>> renderPipeLines = ActiveRenderEffects;

        if (renderPipeLines.Count == 0 || !LogSpiralLibraryMod.CanUseRender)// || GlobalTimeSystem.GlobalTime % 60 < 30 // 调试用
            DirectlyDrawAllGroups(spriteBatch, graphicsDevice);
        else
        {
            var origRender = LogSpiralLibraryMod.Instance.RenderOrig;
            var contentRender = LogSpiralLibraryMod.Instance.Render;
            var assistRender = LogSpiralLibraryMod.Instance.Render_Swap;

            spriteBatch.Begin();
            graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(origRender);
            graphicsDevice.Clear(Color.Transparent);

            DirectlyDrawAllGroups(spriteBatch, graphicsDevice);

            int counter = 0;

            foreach (var pipeLine in renderPipeLines)
            {
                if (pipeLine.Count == 0) continue;

                foreach (var renderEffects in pipeLine)
                {

                    // 如果这个渲染线确实有效果需要重绘内容，就另外执行绘制
                    if (renderEffects.RedrawContents(spriteBatch, graphicsDevice))
                    {
                        graphicsDevice.SetRenderTarget(contentRender);
                        graphicsDevice.Clear(Color.Transparent);
                        DirectlyDrawAllGroups(spriteBatch, graphicsDevice);
                    }
                    // 否则将原画布上的内容拷贝到内容画布上
                    else
                    {
                        graphicsDevice.SetRenderTarget(contentRender);
                        graphicsDevice.Clear(Color.Transparent);
                        spriteBatch.Begin();
                        spriteBatch.Draw(origRender, Vector2.Zero, Color.White);
                        spriteBatch.End();
                    }
                }

                foreach (var renderEffect in pipeLine)
                    renderEffect.ProcessRender(spriteBatch, graphicsDevice, ref contentRender, ref assistRender);

                var last = pipeLine.Last();
                // last.DrawToScreenTarget(spriteBatch, graphicsDevice, contentRender, assistRender);
                // 似乎没必要自行绘制到屏幕上，统一处理即可

                if (last.DoRealDraw)
                {
                    bool flip = counter % 2 == 1;
                    graphicsDevice.SetRenderTarget(flip ? Main.screenTargetSwap : Main.screenTarget);
                    graphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    spriteBatch.Draw(flip ? Main.screenTarget : Main.screenTargetSwap, Vector2.Zero, Color.White);
                    spriteBatch.Draw(contentRender, Vector2.Zero, Color.White);
                    spriteBatch.End();
                    counter++;
                }
            }

            // 如果未曾绘制过实体就将原始内容绘制上一次
            if (counter == 0)
            {
                graphicsDevice.SetRenderTarget(Main.screenTarget);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                spriteBatch.Draw(origRender, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
            else if (counter % 2 == 0)
            {
                graphicsDevice.SetRenderTarget(Main.screenTarget);
                graphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin();
                spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
        }
    }
}

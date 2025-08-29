using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.UIBase.InsertablePanel;
using ReLogic.Graphics;
using LogSpiralLibrary.CodeLibrary.Utilties;
namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public class WrapperDecorator : IInsertPanelDecorator
{
    public Wrapper Wrapper { get; set; }
    public InsertablePanel.InsertablePanel InsertablePanel { get; set; }
    void IInsertPanelDecorator.Decorate(InsertablePanel.InsertablePanel panel)
    {
        InsertablePanel = panel;
        //panel.DrawAction += DrawGroupArg;
    }

    void IInsertPanelDecorator.UnloadDecorate(InsertablePanel.InsertablePanel panel)
    {
        //panel.DrawAction -= DrawGroupArg;
    }
    void DrawGroupArg(GameTime gameTime, SpriteBatch spriteBatch)
    {
        //string msg = "我不知道";
        //if (Wrapper.Element is { } element)
        //{
        //    msg = $"元素{element}";
        //}
        //else if (Wrapper.RefSequenceFullName is { } refName)
        //{
        //    msg = $"引用序列{refName}";
        //}
        //else if (Wrapper.IsUnload)
        //{
        //    msg = $"未加载组";
        //}
        //else if (Wrapper.Sequence != null)
        //{
        //    msg = $"值序列？！";

        //}
        spriteBatch.DrawString(FontAssets.MouseText.Value, /*$"我是{msg}"*/"□", InsertablePanel.Bounds.GetPercentedCoord(0, 0.67f), Color.Orange);
    }
}

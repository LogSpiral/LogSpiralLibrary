using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using LogSpiralLibrary.UIBase.InsertablePanel;
using LogSpiralLibrary.CodeLibrary.Utilties;
using ReLogic.Graphics;
namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public class GroupLineDecorator : IInsertPanelDecorator
{
    GroupPanel GroupPanel { get; set; }
    void DrawLine(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (GroupPanel == null) return;
        var bounds = GroupPanel.OuterBounds;
        Vector2 leftCenter = bounds.LeftCenter;
        Vector2 rightCenter = bounds.RightCenter;

        foreach (var inners in GroupPanel.SubInsertablePanels) 
        {
            var leftPanelCenter = InsertablePanelUtils.InsertablePanelLeftCenter(inners);
            var rightPanelCenter = InsertablePanelUtils.InsertablePanelRightCenter(inners);
            spriteBatch.DrawHorizonBLine(leftCenter, leftPanelCenter, Color.White * .5f, 1, 2);
            spriteBatch.DrawHorizonBLine(rightCenter, rightPanelCenter, Color.White * .5f, 1, 2);
        }
        // spriteBatch.DrawString(FontAssets.MouseText.Value, GroupPanel.SubInsertablePanels.Count.ToString(), leftCenter, Color.Red);
    }


    void IInsertPanelDecorator.Decorate(InsertablePanel.InsertablePanel panel)
    {
        if (panel is not GroupPanel groupPanel) return;
        GroupPanel = groupPanel;
        panel.DrawAction += DrawLine;
    }

    void IInsertPanelDecorator.UnloadDecorate(InsertablePanel.InsertablePanel panel)
    {
        if (panel is not GroupPanel groupPanel) return;
        panel.DrawAction -= DrawLine;
    }
}

using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using LogSpiralLibrary.UIBase.InsertablePanel;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public class SequenceLineDecorator : IInsertPanelDecorator
{
    SequencePanel SequencePanel { get; set; }
    void DrawLine(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (SequencePanel == null) return;
        var list = SequencePanel.SubInsertablePanels;
        int count = list.Count;
        for (int n = 0; n < count - 1; n++) 
        {
            spriteBatch.DrawLine(
                InsertablePanelUtils.InsertablePanelRightCenter(list[n]),
                InsertablePanelUtils.InsertablePanelLeftCenter(list[n+1]), 
                Color.White * .5f, 2);
        }
        // spriteBatch.DrawString(FontAssets.MouseText.Value, SequencePanel.SubInsertablePanels.Count.ToString(), SequencePanel.Bounds.LeftCenter, Color.Cyan);
    }


    void IInsertPanelDecorator.Decorate(InsertablePanel.InsertablePanel panel)
    {
        if (panel is not SequencePanel sequencePanel) return;
        SequencePanel = sequencePanel;
        panel.DrawAction += DrawLine;
    }

    void IInsertPanelDecorator.UnloadDecorate(InsertablePanel.InsertablePanel panel)
    {
        if (panel is not SequencePanel sequencePanel) return;
        panel.DrawAction -= DrawLine;
    }
}

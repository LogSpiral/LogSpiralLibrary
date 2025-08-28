using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.UIBase.InsertablePanel;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public class WrapperDecorator : IInsertPanelDecorator
{
    public Wrapper Wrapper { get; set; }
    void IInsertPanelDecorator.Decorate(InsertablePanel.InsertablePanel panel)
    {
    }

    void IInsertPanelDecorator.UnloadDecorate(InsertablePanel.InsertablePanel panel)
    {
    }
}

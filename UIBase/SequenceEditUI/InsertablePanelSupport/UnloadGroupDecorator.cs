using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Unloads;
using LogSpiralLibrary.UIBase.InsertablePanel;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;
public class UnloadGroupDecorator:IInsertPanelDecorator
{
    public UnloadGroup UnloadGroup { get; set; }
    public UnloadSingleGroup UnloadSingleGroup { get; set; }
    public void Decorate(InsertablePanel.InsertablePanel panel)
    {
    }

    public void UnloadDecorate(InsertablePanel.InsertablePanel panel)
    {
    }
}

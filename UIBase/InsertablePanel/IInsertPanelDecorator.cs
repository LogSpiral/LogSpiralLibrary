namespace LogSpiralLibrary.UIBase.InsertablePanel;

public interface IInsertPanelDecorator
{
    public void Decorate(InsertablePanel panel);

    public void UnloadDecorate(InsertablePanel panel);
}
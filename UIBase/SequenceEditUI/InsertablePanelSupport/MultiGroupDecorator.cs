using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.UIBase.InsertablePanel;
using PropertyPanelLibrary.EntityDefinition;
using ReLogic.Graphics;
namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public class MultiGroupDecorator : IInsertPanelDecorator
{
    [CustomEntityDefinitionHandler<MultiGroupDefinitionHandler>]
    public MultiGroupDefinition Definition { get; set; }
    void DrawImGroup(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(FontAssets.MouseText.Value, "我是多组！！", PendingPanel.Bounds.LeftTop, Color.Cyan);
    }
    InsertablePanel.InsertablePanel PendingPanel { get; set; }
    public void Decorate(InsertablePanel.InsertablePanel panel)
    {
        PendingPanel = panel;
        panel.DrawAction += DrawImGroup;
    }

    public void UnloadDecorate(InsertablePanel.InsertablePanel panel)
    {
        panel.DrawAction -= DrawImGroup;
    }
}

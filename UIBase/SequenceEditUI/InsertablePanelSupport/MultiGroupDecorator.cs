using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.UIBase.InsertablePanel;
using PropertyPanelLibrary.EntityDefinition;
using ReLogic.Graphics;
namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public class MultiGroupDecorator : IInsertPanelDecorator
{
    [CustomEntityDefinitionHandler<MultiGroupDefinitionHandler>]
    public MultiGroupDefinition Definition
    {
        get;
        set
        {
            field = value;
            if (PendingPanel != null && PendingPanel is GroupPanel groupPanel)
            {
                var groupDummy = Activator.CreateInstance(value.GroupType) as IGroup;
                foreach (var sub in groupPanel.SubInsertablePanels)
                    if (sub.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg))
                        arg.Argument = GroupArgumentUtils.ConvertArgument(arg.Argument, groupDummy.ArgType);
            }
        }
    }
    void DrawImGroup(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(FontAssets.MouseText.Value, "□", PendingPanel.Bounds.LeftTop, Color.Cyan);
    }
    InsertablePanel.InsertablePanel PendingPanel { get; set; }
    public void Decorate(InsertablePanel.InsertablePanel panel)
    {
        PendingPanel = panel;
        //panel.DrawAction += DrawImGroup;
    }

    public void UnloadDecorate(InsertablePanel.InsertablePanel panel)
    {
        //panel.DrawAction -= DrawImGroup;
    }
}

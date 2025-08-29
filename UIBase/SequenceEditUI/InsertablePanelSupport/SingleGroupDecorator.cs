using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.UIBase.InsertablePanel;
using PropertyPanelLibrary.EntityDefinition;
using ReLogic.Graphics;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public class SingleGroupDecorator : IInsertPanelDecorator
{
    [CustomEntityDefinitionHandler<SingleGroupDefinitionHandler>]
    public SingleGroupDefinition Definition
    {
        get;
        set
        {
            field = value;
            if (PendingPanel != null && PendingPanel.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg))
            {
                var groupDummy = Activator.CreateInstance(value.GroupType) as IGroup;
                arg.Argument = GroupArgumentUtils.ConvertArgument(arg.Argument, groupDummy.ArgType);
            }
        }
    }

    void DrawImGroup(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(FontAssets.MouseText.Value, "□", PendingPanel.Bounds.LeftBottom, Color.Red);
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

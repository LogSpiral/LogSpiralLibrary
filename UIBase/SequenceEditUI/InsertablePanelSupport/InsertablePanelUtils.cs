using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.UIBase.InsertablePanel;
using LogSpiralLibrary.CodeLibrary.Utilties;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public static class InsertablePanelUtils
{
    static Color ElementColor { get; } = Color.Cyan * .1f;
    static Color SequenceColor { get; } = Color.MediumPurple * .1f;
    static Color GroupColor { get; } = Color.Blue * .1f;
    public static Vector2 InsertablePanelLeftCenter(InsertablePanel.InsertablePanel panel) 
    {
        if (panel is GroupPanel)
            return panel.OuterBounds.LeftCenter;
        if (panel is SequencePanel sequence)
            return InsertablePanelLeftCenter(sequence.SubInsertablePanels[0]);
        return panel.Bounds.LeftCenter;
    }
    public static Vector2 InsertablePanelRightCenter(InsertablePanel.InsertablePanel panel)
    {
        if (panel is GroupPanel)
            return panel.OuterBounds.RightCenter;
        if (panel is SequencePanel sequence)
            return InsertablePanelRightCenter(sequence.SubInsertablePanels[^1]);
        return panel.Bounds.RightCenter;
    }
    static void InsertablePanelCommonSet(InsertablePanel.InsertablePanel panel)
    {
        panel.OnAppendingToGroup += AppendToGroupCommon;
        panel.OnAppendingToSequence += AppendToSequenceCommon;
    }
    static void AppendToGroupCommon(InsertablePanel.InsertablePanel self, GroupPanel group)
    {
        InsertablePanelCommonSet(group);
        group.BackgroundColor = GroupColor;
        group.DecoratorManager += new GroupLineDecorator();
    }
    static void AppendToSequenceCommon(InsertablePanel.InsertablePanel self, SequencePanel sequence)
    {
        InsertablePanelCommonSet(sequence);
        sequence.BackgroundColor = SequenceColor;
        sequence.DecoratorManager += new SequenceLineDecorator();
    }

    public static TextTitledInsertablePanel ElementTypeToPanel(Type type)
    {
        var element = Activator.CreateInstance(type) as ISequenceElement;
        TextTitledInsertablePanel insertablePanel = new()
        {
            BackgroundColor = ElementColor
        };
        InsertablePanelCommonSet(insertablePanel);
        insertablePanel.TitleText.Text = element.ToString();
        insertablePanel.DecoratorManager += new GroupArgumentDecorator() { Pair = new() { Wrapper = new(element), Argument = new ConditionArg() } };
        return insertablePanel;
    }
    public static TextTitledInsertablePanel SequenceRefKeyToPanel(string refName)
    {
        var sequence = SequenceGlobalManager.SequenceLookup[refName];
        TextTitledInsertablePanel insertablePanel = new()
        {
            BackgroundColor = SequenceColor
        };
        InsertablePanelCommonSet(insertablePanel);
        insertablePanel.TitleText.Text = sequence.Data.DisplayName;
        insertablePanel.DecoratorManager += new GroupArgumentDecorator() { Pair = new() { Wrapper = new(refName), Argument = new ConditionArg() } };
        return insertablePanel;
    }
    static TextTitledInsertablePanel BasicToInsertablePanel(Wrapper wrapper)
    {
        var isRef = wrapper.RefSequenceFullName != null;
        TextTitledInsertablePanel insertablePanel = new()
        {
            BackgroundColor = (isRef ? SequenceColor : ElementColor)
        };
        insertablePanel.TitleText.Text = isRef
            ? (wrapper.Sequence as Sequence).Data.DisplayName
            : wrapper.Element.ToString();
        InsertablePanelCommonSet(insertablePanel);
        return insertablePanel;
    }
    static InsertablePanel.InsertablePanel WrapperPairToPanel(IWrapperArgPair<IGroupArgument> pair)
    {
        var argument = pair.Argument;
        var wrapper = pair.Wrapper;
        var isRef = wrapper.RefSequenceFullName != null;
        InsertablePanel.InsertablePanel panel;
        if (wrapper.Element != null || (isRef && !SequenceGlobalManager.UnloadSequences.Contains(wrapper.RefSequenceFullName)))
            panel = BasicToInsertablePanel(wrapper);
        else if (wrapper.IsUnload || (isRef && SequenceGlobalManager.UnloadSequences.Contains(wrapper.RefSequenceFullName)))
        {
            TextTitledInsertablePanel insertablePanel = new() { BackgroundColor = Color.Gray * .25f };
            insertablePanel.TitleText.Text = "未加载Wrapper";
            InsertablePanelCommonSet(insertablePanel);
            panel = insertablePanel;
        }
        else if (wrapper.Sequence != null)
        {
            panel = SequenceToInsertablePanel(wrapper.Sequence as Sequence);
        }
        else
        {
            // TODO 标记无效Wrapper对象
            panel = new InsertablePanel.InsertablePanel();
        }
        // TODO 克隆一份而非直接赋值
        panel.DecoratorManager += new GroupArgumentDecorator() { Pair = new() { Wrapper = wrapper, Argument = argument } };

        return panel;
    }
    static InsertablePanel.InsertablePanel GroupToPanel(IGroup group)
    {
        // TODO 添加对未加载组的支持
        if (group.Contents.Count == 0)
        {
            TextTitledInsertablePanel insertablePanel = new() { BackgroundColor = Color.Gray * .25f };
            insertablePanel.TitleText.Text = "未加载组";
            InsertablePanelCommonSet(insertablePanel);
            return insertablePanel;
        }
        else if (group.Contents.Count == 1)
        {
            return WrapperPairToPanel(group.Contents[0]);
        }
        else
        {
            GroupPanel groupPanel = new()
            {
                BackgroundColor = GroupColor
            };
            groupPanel.DecoratorManager += new GroupLineDecorator();
            foreach (var pair in group.Contents)
                groupPanel.InsertContainerPanel.Add(WrapperPairToPanel(pair));
            InsertablePanelCommonSet(groupPanel);
            return groupPanel;
        }
    }
    public static InsertablePanel.InsertablePanel SequenceToInsertablePanel(Sequence sequence)
    {
        if (sequence.Groups.Count == 1)
        {
            return GroupToPanel(sequence.Groups[0]);
        }
        else
        {
            SequencePanel sequencePanel = new()
            {
                BackgroundColor = SequenceColor
            };
            sequencePanel.DecoratorManager += new SequenceLineDecorator();
            foreach (var group in sequence.Groups)
                sequencePanel.InsertContainerPanel.Add(GroupToPanel(group));
            InsertablePanelCommonSet(sequencePanel);
            return sequencePanel;
        }
    }

    public static void InsertablePanelToSequence(InsertablePanel.InsertablePanel panel, Sequence sequence)
    {
    }
}

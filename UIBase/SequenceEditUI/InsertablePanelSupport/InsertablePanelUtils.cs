using Humanizer;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.UI.SequenceEditUI;
using LogSpiralLibrary.UIBase.InsertablePanel;
using LogSpiralLibrary.UIBase.SequenceEditUI.PropertyPanelSupport;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Panel.Fillers;
using System.Collections.Generic;

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
    static void MarkPending()
    {
        if (UI.SequenceEditUI.SequenceEditUI.AutoLoadingPanels) return;
        var instance = UI.SequenceEditUI.SequenceEditUI.Instance;
        if (instance != null && instance.CurrentPage is { } page)
        {
            page.PendingModified = true;
            if (instance.OpenedSequences.TryGetValue(page.NameIndex, out var sequence))
                instance.PendingSequences.TryAdd(page.NameIndex, sequence);
            if (instance.OpenedPanels.TryGetValue(page.NameIndex, out var root))
                instance.PendingPanels.TryAdd(page.NameIndex, root);
        }
    }
    static void SwitchCurrentPageRoot(InsertablePanel.InsertablePanel self, InsertablePanel.InsertablePanel target)
    {
        if (!UI.SequenceEditUI.SequenceEditUI.AutoLoadingPanels && self == self.BaseView.RootElement)
        {
            var instance = UI.SequenceEditUI.SequenceEditUI.Instance;
            if (instance != null && instance.CurrentPage is { } page)
            {
                if (instance.OpenedPanels.ContainsKey(page.NameIndex))
                    instance.OpenedPanels[page.NameIndex] = target;
                if (instance.PendingPanels.ContainsKey(page.NameIndex))
                    instance.PendingPanels[page.NameIndex] = target;
            }
        }
    }
    static void InsertablePanelCommonSet(InsertablePanel.InsertablePanel panel)
    {
        panel.OnAppendingToGroup += AppendToGroupCommon;
        panel.OnAppendingToSequence += AppendToSequenceCommon;
        panel.RightMouseClick += (elem, evt) =>
        {
            if (evt.Source != elem) return;
            if (!UI.SequenceEditUI.SequenceEditUI.Active) return;
            if (!panel.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var decorator)) return;

            var instance = UI.SequenceEditUI.SequenceEditUI.Instance;
            instance.PropertyPanelConfig.Filler = new GroupArgumentPairFiller(decorator.Pair);
            instance.CurrentEditTarget = panel;
        };
        panel.OnDraggingOut += (elem, evt) =>
        {
            if (evt.Source != elem) return;
            if (!UI.SequenceEditUI.SequenceEditUI.Active) return;
            MarkPending();
            var instance = UI.SequenceEditUI.SequenceEditUI.Instance;
            if (instance.CurrentEditTarget == panel)
            {
                instance.PropertyPanelConfig.Filler = NoneFiller.Instance;
                instance.CurrentEditTarget = null;
            }
        };
        MarkPending();
    }
    static void AppendToGroupCommon(InsertablePanel.InsertablePanel self, GroupPanel group)
    {
        SwitchCurrentPageRoot(self, group);

        InsertablePanelCommonSet(group);
        group.BackgroundColor = GroupColor;
        group.DecoratorManager += new GroupLineDecorator();
        group.DecoratorManager += new GroupArgumentDecorator() { Pair = new() { Wrapper = new(""), Argument = new ConditionArg() } };
        group.DecoratorManager += new MultiGroupDecorator();
        group.OnInsertPanelToInnerContainer += delegate
        {
            MarkPending();
        };
        group.OnDeconstructContainer += (container, draggingOut, left) =>
        {
            SwitchCurrentPageRoot(container,left);
        };
    }
    static void AppendToSequenceCommon(InsertablePanel.InsertablePanel self, SequencePanel sequence)
    {
        SwitchCurrentPageRoot(self, sequence);

        InsertablePanelCommonSet(sequence);
        sequence.BackgroundColor = SequenceColor;
        sequence.DecoratorManager += new SequenceLineDecorator();
        sequence.DecoratorManager += new GroupArgumentDecorator() { Pair = new() { Wrapper = new(""), Argument = new ConditionArg() } };
        sequence.DecoratorManager += new SingleGroupDecorator();
        sequence.OnInsertPanelToInnerContainer += delegate
        {
            MarkPending();
        };
        sequence.OnDeconstructContainer += (container, draggingOut, left) =>
        {
            SwitchCurrentPageRoot(container, left);
        };
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
        panel.DecoratorManager += new GroupArgumentDecorator() { Pair = new() { Wrapper = wrapper, Argument = argument is NoneArg ? new ConditionArg() : argument } };

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
            groupPanel.OnInsertPanelToInnerContainer += delegate
            {
                MarkPending();
            };
            groupPanel.OnDeconstructContainer += (container, draggingOut, left) =>
            {
                SwitchCurrentPageRoot(container, left);
            };
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
            sequencePanel.OnInsertPanelToInnerContainer += delegate
            {
                MarkPending();
            };
            sequencePanel.OnDeconstructContainer += (container, draggingOut, left) =>
            {
                SwitchCurrentPageRoot(container, left);
            };
            foreach (var group in sequence.Groups)
                sequencePanel.InsertContainerPanel.Add(GroupToPanel(group));
            InsertablePanelCommonSet(sequencePanel);
            return sequencePanel;
        }
    }

    public static void RefillSequenceViaInsertablePanel(InsertablePanel.InsertablePanel panel, Sequence sequence)
    {
        var result = InsertablePanelToSequence(panel);
        sequence.Groups = result.Groups;

    }

    static Sequence InsertablePanelToSequence(InsertablePanel.InsertablePanel panel) 
    {
        var result = new Sequence();
        if (panel is SequencePanel sequencePanel)
            foreach (var inners in sequencePanel.SubInsertablePanels)
                result.Groups.Add(InsertablePanelToGroup(inners));
        else result.Groups.Add(InsertablePanelToGroup(panel));
        return result;
    }

    static IGroup InsertablePanelToGroup(InsertablePanel.InsertablePanel panel) 
    {

        // 如果是多组面板则获取多组类型装饰器，构建之后解析组内的每一个元素
        if (panel is GroupPanel groupPanel && panel.DecoratorManager.TryFindFirst<MultiGroupDecorator>(out var multiDecorator))
        {
            var multiGroup = Activator.CreateInstance(multiDecorator.Definition.GroupType) as IGroup;
            foreach (var inners in groupPanel.SubInsertablePanels) 
            {
                if (!inners.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var argumentDecorator)) continue;
                var attributes = new Dictionary<string, string>();
                argumentDecorator.Pair.Argument.WriteAttributes(attributes);
                multiGroup.AppendWrapper(InsertablePanelToWrapper(inners), attributes);
            }
            return multiGroup;
        }

        // 如果是序列面板或者基本面板则构建单组并赋上单组参数
        else if (panel.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var singleDecorator) && panel.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var argumentDecorator))
        {
            var singleGroup = Activator.CreateInstance(singleDecorator.Definition.GroupType) as IGroup;
            var attributes = new Dictionary<string, string>();
            argumentDecorator.Pair.Argument.WriteAttributes(attributes);
            singleGroup.AppendWrapper(InsertablePanelToWrapper(panel), attributes);
            return singleGroup;
        }

        // 如果是未加载组就获取未加载数据然后塞上
        else if (panel.DecoratorManager.TryFindFirst<UnloadGroupDecorator>(out var unloadDecorator))
        {
            if(unloadDecorator.UnloadGroup != null)
                return unloadDecorator.UnloadGroup;
            return unloadDecorator.UnloadSingleGroup;
        }

        // 基本元素作为根元素的时候作为无参单组返回
        else
        {
            return new SingleWrapperGroup(InsertablePanelToWrapper(panel));
        }
    }

    static Wrapper InsertablePanelToWrapper(InsertablePanel.InsertablePanel panel) 
    {
        // 序列面板和组面板都读取为序列
        if (panel is SequencePanel || panel is GroupPanel)
        {
            var sequence = InsertablePanelToSequence(panel);
            return new Wrapper(sequence);
        }

        // 如果是未加载组就获取未加载数据然后塞上
        else if (panel.DecoratorManager.TryFindFirst<UnloadGroupDecorator>(out var unloadDecorator))
        {
            var sequence = new Sequence();

            if (unloadDecorator.UnloadGroup is var unloadGroup)
                sequence.Groups.Add(unloadGroup);
            else if (unloadDecorator.UnloadSingleGroup is var unloadSingleGroup)
                sequence.Groups.Add(unloadSingleGroup);

            return new Wrapper(sequence);
        }

        // 能到这里说明已经是基本面板了，直接返回它的Wrapper
        else if (panel.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var groupArgumentDecorator)) 
        {
            return groupArgumentDecorator.Pair.Wrapper;
        }

        // 我还能说什么呢
        return null;
    }
}

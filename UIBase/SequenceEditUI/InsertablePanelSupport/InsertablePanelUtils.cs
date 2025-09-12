using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Unloads;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.UIBase.InsertablePanel;
using LogSpiralLibrary.UIBase.SequenceEditUI.PropertyPanelSupport;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Panel.Fillers;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces.Panel;
using System.Collections.Generic;
using Terraria.Localization;
using InsPanel = LogSpiralLibrary.UIBase.InsertablePanel.InsertablePanel;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public static class InsertablePanelUtils
{
    static Color ElementColor { get; } = Color.Cyan * .1f;
    static Color SequenceColor { get; } = Color.MediumPurple * .1f;
    static Color GroupColor { get; } = Color.Blue * .1f;
    public static CombinedFiller InsertablePanelToFiller(InsPanel panel)
    {
        List<IPropertyOptionFiller> fillers = [];
        if (panel.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var single))
        {
            fillers.Add(new GroupFiller(single));
        }
        if (panel.DecoratorManager.TryFindFirst<MultiGroupDecorator>(out var multi))
        {
            fillers.Add(new GroupFiller(multi));
        }
        if (panel.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg))
        {
            fillers.Add(new GroupFiller(arg.Argument));
        }
        if (panel.DecoratorManager.TryFindFirst<WrapperDecorator>(out var wrapper) && wrapper.Wrapper.Element is { } element)
        {
            fillers.Add(new SequenceElementFiller(element));
            // TODO 添加一个转到目标序列页面的选项
            // 使用装饰器实现？还是填充器？
        }
        if (fillers.Count == 0) return null;
        return new CombinedFiller(fillers);
    }
    public static Vector2 InsertablePanelLeftCenter(InsPanel panel)
    {
        if (panel is GroupPanel)
            return panel.OuterBounds.LeftCenter;
        if (panel is SequencePanel sequence)
            return InsertablePanelLeftCenter(sequence.SubInsertablePanels[0]);
        return panel.Bounds.LeftCenter;
    }
    public static Vector2 InsertablePanelRightCenter(InsPanel panel)
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
    static void SwitchCurrentPageRoot(InsPanel self, InsPanel target)
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
    static void GroupPanelCommonSet(GroupPanel group)
    {
        group.BackgroundColor = GroupColor;

        group.DecoratorManager += new GroupLineDecorator();

        group.OnInsertPanelToInnerContainer += (container, panel) =>
        {
            MarkPending();

            // B: 添加组参数默认值
            // F: 组参数转换成目标类型
            if (container.DecoratorManager.TryFindFirst<MultiGroupDecorator>(out var multiDecorator))
            {
                var groupDummy = Activator.CreateInstance(multiDecorator.Definition.GroupType) as IGroup;
                if (panel.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg))
                    arg.Argument = GroupArgumentUtils.ConvertArgument(arg.Argument, groupDummy.ArgType);
                else
                    panel.DecoratorManager += new GroupArgumentDecorator() { Argument = Activator.CreateInstance(groupDummy.ArgType) as IGroupArgument };
            }
        };
        group.OnDeconstructContainer += (container, draggingOut, left) =>
        {
            var instance = UI.SequenceEditUI.SequenceEditUI.Instance;
            if (instance.CurrentEditTarget == container)
            {
                instance.PropertyPanelConfig.Filler = NoneFiller.Instance;
                instance.CurrentEditTarget = null;
            }
            var insParent = container.Parent?.Parent;
            if (insParent is GroupPanel)
            {
                // L & T: 剩余方继承
                if (container.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg))
                {
                    if (left.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var leftArg))
                        leftArg.Argument = arg.Argument;
                    else
                        left.DecoratorManager += new GroupArgumentDecorator() { Argument = arg.Argument };

                    if (left.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var single))
                        left.DecoratorManager -= single;

                    if (draggingOut.DecoratorManager.TryFindFirst(out single))
                        left.DecoratorManager -= single;
                }
            }
            else // if (insParent is SequencePanel)
            {
                // P & X: 剩余组转单组，自身保留组参数装饰器
                if (!left.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var leftArg))
                {
                    leftArg = new GroupArgumentDecorator() { Argument = NoneArg.Instance };
                    left.DecoratorManager += leftArg;
                }

                if (!SequenceGlobalManager.GroupArgToSingleGroup.TryGetValue(leftArg.Argument.GetType(), out var singleType))
                    singleType = typeof(SingleWrapperGroup);
                var groupDummy = Activator.CreateInstance(singleType) as IGroup;

                leftArg.Argument = GroupArgumentUtils.ConvertArgument(leftArg.Argument, groupDummy.ArgType);
                left.DecoratorManager += new SingleGroupDecorator() { Definition = new(groupDummy) };
            }
            SwitchCurrentPageRoot(container, left);
        };
    }
    static void SequencePanelCommonSet(SequencePanel sequence)
    {
        sequence.BackgroundColor = SequenceColor;

        sequence.DecoratorManager += new SequenceLineDecorator();

        sequence.OnInsertPanelToInnerContainer += (container, panel) =>
        {
            MarkPending();

            // A: 直接插入
            // E: 组参数决定单组类型后插入
            if (panel.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg))
            {
                if (panel is not GroupPanel)
                {
                    if (!SequenceGlobalManager.GroupArgToSingleGroup.TryGetValue(arg.Argument.GetType(), out var singleType))
                        singleType = typeof(SingleWrapperGroup);
                    var groupDummy = Activator.CreateInstance(singleType) as IGroup;
                    arg.Argument = GroupArgumentUtils.ConvertArgument(arg.Argument, groupDummy.ArgType);
                    panel.DecoratorManager += new SingleGroupDecorator() { Definition = new(groupDummy) };
                }
                else
                    panel.DecoratorManager -= arg;
            }
        };
        sequence.OnDeconstructContainer += (container, draggingOut, left) =>
        {
            var instance = UI.SequenceEditUI.SequenceEditUI.Instance;
            if (instance.CurrentEditTarget == container)
            {
                instance.PropertyPanelConfig.Filler = NoneFiller.Instance;
                instance.CurrentEditTarget = null;
            }
            // 第一个是MultiPanel的内部容器，第二个才是MultiPanel自身
            var insParent = container.Parent?.Parent;
            if (insParent is GroupPanel)
            {
                // K & S: 剩余方继承，有单组则移除
                if (container.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg))
                {
                    if (left.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var leftArg))
                        leftArg.Argument = arg.Argument;
                    else
                        left.DecoratorManager += new GroupArgumentDecorator() { Argument = leftArg.Argument };
                    if (left.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var single))
                        left.DecoratorManager -= single;
                }
            }
            else if (insParent is SequencePanel)
            {
                // W: 移除单组后移除
                if (draggingOut.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var single))
                    draggingOut.DecoratorManager -= single;
            }
            // O: 直接解除

            SwitchCurrentPageRoot(container, left);
        };
    }
    static void InsertablePanelCommonSet(InsPanel panel)
    {
        panel.OnAppendingToGroup += AppendToGroupCommon;
        panel.OnAppendingToSequence += AppendToSequenceCommon;
        panel.RightMouseClick += (elem, evt) =>
        {
            if (evt.Source != elem) return;
            if (!UI.SequenceEditUI.SequenceEditUI.Active) return;

            var combinedFiller = InsertablePanelToFiller(panel);
            if (combinedFiller == null) return;
            var instance = UI.SequenceEditUI.SequenceEditUI.Instance;
            instance.PropertyPanelConfig.Filler = combinedFiller;
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

            // 第一个是MultiPanel的内部容器，第二个才是MultiPanel自身
            var insPanel = elem?.Parent?.Parent;

            if (insPanel is SequencePanel)
            {
                // C: 直接移除
                if (elem is not GroupPanel)
                {
                    // G: 保留组参数，移除单组装饰器
                    if (elem is InsPanel ins && ins.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var single))
                        ins.DecoratorManager -= single;
                }
            }
            else if (insPanel is GroupPanel)
            {
                //if (elem is GroupPanel)
                //{
                //    // D: 移除组参数后移除
                //    if (elem is InsPanel ins && ins.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg))
                //        ins.DecoratorManager -= arg;
                //}
                // D: 直接移除
                // H: 直接移除
            }
        };
        MarkPending();
    }
    static void AppendToGroupCommon(InsPanel self, InsPanel pending, GroupPanel group)
    {
        SwitchCurrentPageRoot(self, group);

        InsertablePanelCommonSet(group);

        // 第一个Parent是预览容器，第二个是MultiPanel的内部容器，第三个才是MultiPanel自身
        var insParent = self.Parent?.Parent?.Parent;
        if (true/*insParent is MultiPanel && *//*self.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg)*/)
        {
            bool noArg = !self.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg);
            if (insParent is GroupPanel)
            {
                // J & R: 目标的组参数转交新组
                // 理论上不应当在此处存在无条件参数的情况
                // 但是懒得处理成默认条件了
                group.DecoratorManager += new GroupArgumentDecorator() { Argument = noArg ? NoneArg.Instance : arg.Argument };
                if (!noArg)
                    self.DecoratorManager -= arg;

                // 新组使用默认条件组，内部元素使用默认参数
                group.DecoratorManager += new MultiGroupDecorator() { Definition = new(nameof(ConditionalMultiGroup)) };
                self.DecoratorManager += new GroupArgumentDecorator() { Argument = new ConditionArg() };
                pending.DecoratorManager += new GroupArgumentDecorator() { Argument = new ConditionArg() };
            }
            else // if (/*insParent is SequencePanel && */self.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var single))
            {
                if (self.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var single))
                    self.DecoratorManager -= single;

                // N: 目标单组转多组，自身转换类型后插入
                var singleType = single?.Definition?.GroupType;

                // V：目标方优先，都无参则条件组
                if (pending is not GroupPanel
                    && (singleType == typeof(SingleWrapperGroup) || singleType == null)
                    && pending.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var pendingArgument)
                    && SequenceGlobalManager.GroupArgToSingleGroup.TryGetValue(pendingArgument.Argument.GetType(), out var pendingSingleType))
                    singleType = pendingSingleType;

                if (singleType == null)
                    singleType = typeof(SingleWrapperGroup);

                if (!SequenceGlobalManager.SingleGroupToMultiGroup.TryGetValue(singleType, out var multiType))
                    multiType = typeof(ConditionalMultiGroup);
                var groupDummy = Activator.CreateInstance(multiType) as IGroup;
                group.DecoratorManager += new MultiGroupDecorator() { Definition = new(groupDummy) };
                if (noArg)
                {
                    self.DecoratorManager += 
                        new GroupArgumentDecorator() 
                        {
                            Argument = 
                            Activator.CreateInstance(groupDummy.ArgType) 
                            as IGroupArgument 
                        };
                }
                else 
                {
                    arg.Argument = 
                        GroupArgumentUtils
                        .ConvertArgument(
                            arg.Argument,
                            groupDummy.ArgType);
                }
                if (!pending.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var pendingArg))
                {
                    pending.DecoratorManager +=
                        new GroupArgumentDecorator()
                        {
                            Argument = 
                            Activator.CreateInstance(groupDummy.ArgType) 
                            as IGroupArgument
                        };
                }
                else 
                {
                    pendingArg.Argument = 
                        GroupArgumentUtils
                        .ConvertArgument(
                            pendingArg.Argument,
                            groupDummy.ArgType);
                }
            }
        }

        GroupPanelCommonSet(group);
    }
    static void AppendToSequenceCommon(InsPanel self, InsPanel pending, SequencePanel sequence)
    {
        SwitchCurrentPageRoot(self, sequence);

        InsertablePanelCommonSet(sequence);

        // 第一个Parent是预览容器，第二个是MultiPanel的内部容器，第三个才是MultiPanel自身
        var insParent = self.Parent?.Parent?.Parent;
        if (insParent is GroupPanel && self.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var arg))
        {
            // I: 目标的组参数转交新序列
            sequence.DecoratorManager += new GroupArgumentDecorator() { Argument = arg.Argument };
            self.DecoratorManager -= arg;

            if (!self.DecoratorManager.TryFindFirst<MultiGroupDecorator>(out var multi))
                self.DecoratorManager += new SingleGroupDecorator() { Definition = new(nameof(SingleWrapperGroup)) };

            self.DecoratorManager += new GroupArgumentDecorator() { Argument = NoneArg.Instance };

            // Q: 目标的组参数转交新序列, 自身添加单组装饰器
            if (pending is not GroupPanel && pending.DecoratorManager.TryFindFirst(out arg))
            {
                if (!SequenceGlobalManager.GroupArgToSingleGroup.TryGetValue(arg.Argument.GetType(), out var singleType))
                    singleType = typeof(SingleWrapperGroup);
                var groupDummy = Activator.CreateInstance(singleType) as IGroup;
                arg.Argument = GroupArgumentUtils.ConvertArgument(arg.Argument, groupDummy.ArgType);
                pending.DecoratorManager += new SingleGroupDecorator() { Definition = new(groupDummy) };
            }
        }
        else
        //if (insParent is SequencePanel)
        {
            // M: 序列面板加上单组装饰器，使用默认单组
            // U: 序列面板加上单组装饰器，使用默认单组, 组参数决定单组而后插入
            if (pending is not GroupPanel && pending.DecoratorManager.TryFindFirst(out arg))
            {
                if (!SequenceGlobalManager.GroupArgToSingleGroup.TryGetValue(arg.Argument.GetType(), out var singleType))
                    singleType = typeof(SingleWrapperGroup);
                var groupDummy = Activator.CreateInstance(singleType) as IGroup;
                arg.Argument = GroupArgumentUtils.ConvertArgument(arg.Argument, groupDummy.ArgType);
                pending.DecoratorManager += new SingleGroupDecorator() { Definition = new(groupDummy) };
            }
            if (!sequence.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var single))
                sequence.DecoratorManager += new SingleGroupDecorator() { Definition = new(nameof(SingleWrapperGroup)) };
        }

        SequencePanelCommonSet(sequence);
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
        insertablePanel.DecoratorManager += new GroupArgumentDecorator() { Argument = NoneArg.Instance };
        insertablePanel.DecoratorManager += new WrapperDecorator() { Wrapper = new(element) };
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
        insertablePanel.DecoratorManager += new GroupArgumentDecorator() { Argument = NoneArg.Instance };
        insertablePanel.DecoratorManager += new WrapperDecorator() { Wrapper = new(refName) };
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
        insertablePanel.DecoratorManager += new WrapperDecorator() { Wrapper = wrapper };
        InsertablePanelCommonSet(insertablePanel);
        return insertablePanel;
    }
    static InsPanel WrapperPairToPanel(IWrapperArgPair<IGroupArgument> pair)
    {
        var argument = pair.Argument;
        var wrapper = pair.Wrapper;
        var isRef = wrapper.RefSequenceFullName != null;
        InsPanel panel;
        if (wrapper.Element != null || (isRef && !SequenceGlobalManager.UnloadSequences.Contains(wrapper.RefSequenceFullName)))
            panel = BasicToInsertablePanel(wrapper);
        else if (wrapper.IsUnload || (isRef && SequenceGlobalManager.UnloadSequences.Contains(wrapper.RefSequenceFullName)))
        {
            TextTitledInsertablePanel insertablePanel = new() { BackgroundColor = Color.Gray * .25f };
            insertablePanel.TitleText.Text = Language.GetTextValue($"Mods.LogSpiralLibrary.SequenceUI.Unload{(wrapper.IsUnload ? "Element" : "Sequence")}");
            insertablePanel.DecoratorManager += new WrapperDecorator() { Wrapper = wrapper };
            InsertablePanelCommonSet(insertablePanel);
            panel = insertablePanel;
        }
        else if (wrapper.Sequence != null)
            panel = SequenceToInsertablePanel(wrapper.Sequence as Sequence);
        else
        {
            // TODO 标记无效Wrapper对象
            panel = new InsPanel();
        }
        panel.DecoratorManager += new GroupArgumentDecorator() { Argument = argument };

        return panel;
    }
    static InsPanel GroupToPanel(IGroup group)
    {
        if (group.Contents.Count == 0)
        {
            TextTitledInsertablePanel insertablePanel = new() { BackgroundColor = Color.Gray * .25f };
            var unloadDecorator = new UnloadGroupDecorator();
            if (group is UnloadGroup unloadGroup)
                unloadDecorator.UnloadGroup = unloadGroup;
            else if (group is UnloadSingleGroup unloadSingleGroup)
                unloadDecorator.UnloadSingleGroup = unloadSingleGroup;
            insertablePanel.DecoratorManager += unloadDecorator;
            insertablePanel.TitleText.Text = Language.GetTextValue("Mods.LogSpiralLibrary.SequenceUI.UnloadGroup");
            InsertablePanelCommonSet(insertablePanel);
            return insertablePanel;
        }
        else if (group.Contents.Count == 1)
        {
            var result = WrapperPairToPanel(group.Contents[0]);
            result.DecoratorManager += new SingleGroupDecorator() { Definition = new SingleGroupDefinition(group) };
            return result;
        }
        else
        {
            GroupPanel groupPanel = new();
            GroupPanelCommonSet(groupPanel);
            groupPanel.DecoratorManager += new MultiGroupDecorator() { Definition = new MultiGroupDefinition(group) };
            foreach (var pair in group.Contents)
                groupPanel.InsertContainerPanel.Add(WrapperPairToPanel(pair));
            InsertablePanelCommonSet(groupPanel);
            return groupPanel;
        }
    }
    public static InsPanel SequenceToInsertablePanel(Sequence sequence)
    {
        if (sequence.Groups.Count == 1)
        {
            return GroupToPanel(sequence.Groups[0]);
        }
        else
        {
            SequencePanel sequencePanel = new();
            SequencePanelCommonSet(sequencePanel);
            foreach (var group in sequence.Groups)
                sequencePanel.InsertContainerPanel.Add(GroupToPanel(group));
            InsertablePanelCommonSet(sequencePanel);
            return sequencePanel;
        }
    }
}
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.UIBase.InsertablePanel;
using System.Collections.Generic;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

public static class InsertPanelToSequenceUtils
{
    public static void RefillSequenceViaInsertablePanel(InsertablePanel.InsertablePanel panel, Sequence sequence)
    {
        var result = InsertablePanelToSequence(panel);
        sequence.Groups = result.Groups;
    }

    private static Sequence InsertablePanelToSequence(InsertablePanel.InsertablePanel panel)
    {
        var result = new Sequence();
        if (panel is SequencePanel sequencePanel)
            foreach (var inners in sequencePanel.SubInsertablePanels)
                result.Groups.Add(InsertablePanelToGroup(inners));
        else result.Groups.Add(InsertablePanelToGroup(panel));
        return result;
    }

    private static IGroup InsertablePanelToGroup(InsertablePanel.InsertablePanel panel)
    {
        // 如果是多组面板则获取多组类型装饰器，构建之后解析组内的每一个元素
        if (panel is GroupPanel groupPanel && panel.DecoratorManager.TryFindFirst<MultiGroupDecorator>(out var multiDecorator))
        {
            var multiGroup = Activator.CreateInstance(multiDecorator.Definition.GroupType) as IGroup;
            foreach (var inners in groupPanel.SubInsertablePanels)
            {
                if (!inners.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var argumentDecorator)) continue;
                var attributes = new Dictionary<string, string>();
                argumentDecorator.Argument.WriteAttributes(attributes);
                multiGroup.AppendWrapper(InsertablePanelToWrapper(inners), attributes);
            }
            return multiGroup;
        }

        // 如果是序列面板或者基本面板则构建单组并赋上单组参数
        else if (panel.DecoratorManager.TryFindFirst<SingleGroupDecorator>(out var singleDecorator) && panel.DecoratorManager.TryFindFirst<GroupArgumentDecorator>(out var argumentDecorator))
        {
            var singleGroup = Activator.CreateInstance(singleDecorator.Definition.GroupType) as IGroup;
            var attributes = new Dictionary<string, string>();
            argumentDecorator.Argument.WriteAttributes(attributes);
            singleGroup.AppendWrapper(InsertablePanelToWrapper(panel), attributes);
            return singleGroup;
        }

        // 如果是未加载组就获取未加载数据然后塞上
        else if (panel.DecoratorManager.TryFindFirst<UnloadGroupDecorator>(out var unloadDecorator))
        {
            if (unloadDecorator.UnloadGroup != null)
                return unloadDecorator.UnloadGroup;
            return unloadDecorator.UnloadSingleGroup;
        }

        // 基本元素作为根元素的时候作为无参单组返回
        else
        {
            return new SingleWrapperGroup(InsertablePanelToWrapper(panel));
        }
    }

    private static Wrapper InsertablePanelToWrapper(InsertablePanel.InsertablePanel panel)
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
        else if (panel.DecoratorManager.TryFindFirst<WrapperDecorator>(out var wrapper))
        {
            return wrapper.Wrapper;
        }

        // 我还能说什么呢
        return null;
    }
}
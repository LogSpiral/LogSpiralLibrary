using LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

public class InsertPanelDecoratorManager
{
    /// <summary>
    /// 全部的装饰器
    /// </summary>
    private HashSet<IInsertPanelDecorator> Decorators { get; } = [];

    /// <summary>
    /// 已经激活了的装饰器
    /// </summary>
    private HashSet<IInsertPanelDecorator> ActiveDecorators { get; } = [];

    public bool PendingModified { get; private set; }

    public static InsertPanelDecoratorManager operator +(InsertPanelDecoratorManager collection, IInsertPanelDecorator decorator)
    {
        collection.Decorators.Add(decorator);
        collection.PendingModified = true;
        return collection;
    }
    public static InsertPanelDecoratorManager operator -(InsertPanelDecoratorManager collection, IInsertPanelDecorator decorator)
    {
        collection.Decorators.Remove(decorator);
        collection.PendingModified = true;
        return collection;
    }

    public void Update(InsertablePanel panel)
    {
        Dictionary<IInsertPanelDecorator, bool> pendings = [];
        foreach (var decorator in from news in Decorators where !ActiveDecorators.Contains(news) select news)
        {
            decorator.Decorate(panel);
            pendings.Add(decorator, true);
        }
        foreach (var decorator in from olds in ActiveDecorators where !Decorators.Contains(olds) select olds)
        {
            decorator.UnloadDecorate(panel);
            pendings.Add(decorator, false);
        }
        foreach (var pair in pendings)
        {
            if (pair.Value)
                ActiveDecorators.Add(pair.Key);
            else
                ActiveDecorators.Remove(pair.Key);
        }
        PendingModified = false;
    }



    /*public void Apply(InsertablePanel panel) 
    {
        foreach(var decorator in Decorators)
            decorator.Decorate(panel);
    }

    public void Disapply(InsertablePanel panel) 
    {
        foreach(var decorator in Decorators)
            decorator.UnloadDecorate(panel);
    }*/

    public bool TryFindFirst<T>(out T decorator)  where T : IInsertPanelDecorator
    {
        decorator = default;
        foreach (var dummy in Decorators)
            if (dummy is T result) 
            {
                decorator = result;
                return true;
            }
        return false;
    }
}

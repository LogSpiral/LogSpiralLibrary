using System.Collections.Generic;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

public class InsertPanelDecoratorManager
{
    private HashSet<IInsertPanelDecorator> Decorators { get; } = [];
    public static InsertPanelDecoratorManager operator +(InsertPanelDecoratorManager collection, IInsertPanelDecorator decorator)
    {
        collection.Decorators.Add(decorator);
        return collection;
    }
    public static InsertPanelDecoratorManager operator -(InsertPanelDecoratorManager collection, IInsertPanelDecorator decorator)
    {
        collection.Decorators.Remove(decorator);
        return collection;
    }

    public void Apply(InsertablePanel panel) 
    {
        foreach(var decorator in Decorators)
            decorator.Decorate(panel);
    }

    public void Disapply(InsertablePanel panel) 
    {
        foreach(var decorator in Decorators)
            decorator.UnloadDecorate(panel);
    }

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

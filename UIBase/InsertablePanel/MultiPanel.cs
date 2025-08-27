using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using LogSpiralLibrary.CodeLibrary.Utilties;
namespace LogSpiralLibrary.UIBase.InsertablePanel;

file class InsertContainer(List<InsertablePanel> innerPanels, InsertBasePanel baseView, UIElementGroup mask) : UIElementGroup
{
    private List<InsertablePanel> InnerPanels { get; } = innerPanels;
    public InsertBasePanel BaseView { private get; set; } = baseView;
    public UIElementGroup Mask { private get; set; } = mask;
    private static bool RemoveLock { get; set; }
    public override void Add(UIView child, int? index = null)
    {
        base.Add(child, index);
        if (InsertablePanel.PreviewProtect) return;
        if (child is InsertablePanel insertablePanel)
        {
            if (index != null)
            {
                int idx = 0;
                for (int i = 0; i < index; i++)
                    if (ElementsCache[i] is InsertablePanel)
                        idx++;
                InnerPanels.Insert(idx, insertablePanel);
            }
            else
                InnerPanels.Add(insertablePanel);
            insertablePanel.BaseView = BaseView;
            insertablePanel.Mask = Mask;
        }
    }

    public override void RemoveChild(UIView child)
    {
        base.RemoveChild(child);

        if (InsertablePanel.PreviewProtect) return;
        if (RemoveLock) return;

        if (child is InsertablePanel insertablePanel)
        {
            InnerPanels.Remove(insertablePanel);

            if (InnerPanels.Count < 2)
            {
                if (InnerPanels.Count == 1)
                {
                    var parent = Parent.Parent;
                    var sub = InnerPanels[0];
                    if (parent == BaseView)
                    {
                        Vector2 pos = new(Parent.Left.Pixels + Parent.Padding.Left + Parent.Border, Parent.Top.Pixels + Parent.Padding.Top + Parent.Border);
                        sub.SetLeft(pos.X);
                        sub.SetTop(pos.Y);
                        BaseView.RootElement = sub;
                    }
                    RemoveLock = true;
                    parent.AddBefore(sub, Parent);
                    RemoveLock = false;
                }
                Parent?.Remove();
            }
        }
    }
}
public abstract class MultiPanel : InsertablePanel
{

    public override InsertBasePanel BaseView
    {
        get;
        set
        {
            field = value;
            InsertContainer container = InsertContainerPanel as InsertContainer;
            container?.BaseView = value;
            foreach (var panel in InnerPanels)
                panel?.BaseView = value;
        }
    }

    public override UIElementGroup Mask
    {
        get;
        set
        {
            field = value;
            InsertContainer container = InsertContainerPanel as InsertContainer;
            container?.Mask = value;
            foreach (var panel in InnerPanels)
                panel?.Mask = value;
        }
    }

    /// <summary>
    /// 实际插入容器，因为面板本体可能还需要后续装饰
    /// </summary>
    public UIElementGroup InsertContainerPanel { get; }

    protected List<InsertablePanel> InnerPanels { get; } = [];

    public IReadOnlyList<InsertablePanel> SubInsertablePanels => InnerPanels;

    public event Action<MultiPanel, InsertablePanel> OnInsertPanelToInnerContainer;

    public bool RemoveFromInnerListManually(InsertablePanel panel) => InnerPanels.Remove(panel);
    public MultiPanel()
    {
        FitHeight = true;
        FitWidth = true;
        FlexDirection = FlexDirection.Column;
        //MainAlignment = MainAlignment.Center;
        //CrossAlignment = CrossAlignment.Center;

        //Padding = new(8f);
        //Gap = new(16);
        InsertContainerPanel = new InsertContainer(InnerPanels, BaseView, Mask)
        {
            FitHeight = true,
            FitWidth = true,
            MainAlignment = MainAlignment.Center,
            CrossAlignment = CrossAlignment.Center,
            Gap = new(16),
            Padding = new(8f),
            IgnoreMouseInteraction = true
        };
        InsertContainerPanel.Join(this);
    }
    protected Vector2 _origSize;
    protected override void RecordCachedData()
    {
        _origSize = InnerBounds.Size;
        base.RecordCachedData();
    }
    protected override void Inserting(UIMouseEvent evt)
    {
        if (_pvState >= 4)
        {
            InsertContainerPanel.AddBefore(PendingPanel, _pvView);
            OnInsertPanelToInnerContainer?.Invoke(this, PendingPanel);
        }

        base.Inserting(evt);
    }

    public override InsertablePanel GetInnerInsertablePanelAt(Vector2 mousePosition)
    {
        foreach (var inners in InnerPanels)
            if (inners.GetInnerInsertablePanelAt(mousePosition) is { } target)
                return target;

        return base.GetInnerInsertablePanelAt(mousePosition);
    }

    protected override int UpdateInsertPreviewState()
    {
        var result = base.UpdateInsertPreviewState();
        if (result != -1) return result;
        if (InnerPanels.Count < 2) return -1;
        return HandlePVState();
    }

    protected abstract int HandlePVState();

    protected override void HandlePreview() => HandlePreviewAnimation();

    protected abstract void HandlePreviewAnimation();
}
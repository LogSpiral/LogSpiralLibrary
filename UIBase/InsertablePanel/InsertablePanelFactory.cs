using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Extensions;
using System.Linq;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

[XmlElementMapping(nameof(InsertablePanelFactory))]
[JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
public class InsertablePanelFactory : UIElementGroup
{
    public InsertablePanelFactory()
    {
        FitHeight = FitWidth = true;
    }

    // 按说这个得是在构造函数里，或者required然后在初始化器里
    // 但是不好和源生成器兼容，就还是干脆get set得了
    public Func<InsertablePanel> PanelFactory
    {
        get;
        set
        {
            field = value;
            DummyPanel = PanelFactory.Invoke();
            DummyPanel.Join(this);
            DummyPanel.IgnoreMouseInteraction = true;
        }
    }

    public InsertablePanel DummyPanel { get; private set; }

    public override void OnLeftMouseDown(UIMouseEvent evt)
    {
        var mousePanel = PanelFactory.Invoke();
        mousePanel.Bounds = DummyPanel.Bounds;
        SilkyUI.SetFocus(mousePanel);
        SilkyUI.MouseElement[MouseButtonType.Left] = mousePanel;
        mousePanel.StartDragging();
        base.OnLeftMouseDown(evt);
    }
}
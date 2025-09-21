using SilkyUIFramework;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
public class TextTitledInsertablePanel : InsertablePanel
{
    public UITextView TitleText { get; set; }
    public TextTitledInsertablePanel()
    {
        var mask = new UIElementGroup()
        {
            FitWidth = true,
            FitHeight = true,
            IgnoreMouseInteraction = true
        };
        TitleText = new UITextView()
        {
            Margin = new Margin(8),
            TextAlign = new Vector2(0, .5f),
            IgnoreMouseInteraction = true
        };
        TitleText.Join(mask);
        mask.Join(this);
        FitWidth = FitHeight = true;
    }
}

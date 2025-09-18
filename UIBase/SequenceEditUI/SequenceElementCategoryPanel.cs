using LogSpiralLibrary.UI.SequenceEditUI;
using SilkyUIFramework;
using SilkyUIFramework.Elements;

namespace LogSpiralLibrary.UIBase.SequenceEditUI;

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
public partial class SequenceElementCategoryPanel : UIElementGroup
{
    public SequenceElementCategoryPanel(SequenceElementCategory category)
    {
        InitializeComponent();
        DisplayName.Text = category.CategoryName.Value;
        IconImage.Texture2D = category.Icon;
        IconImage.ImageAlign = new(.5f);
        Category = category;
    }

    public SequenceElementCategory Category { get; }
}
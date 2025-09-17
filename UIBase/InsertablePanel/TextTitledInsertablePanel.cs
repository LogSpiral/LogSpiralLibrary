using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

[JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
[ExtendsFromMod(nameof(SilkyUIFramework))]
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
            Margin = new(8),
            TextAlign = new(0, .5f),
            IgnoreMouseInteraction = true
        };
        TitleText.Join(mask);
        mask.Join(this);
        FitWidth = FitHeight = true;
    }
}

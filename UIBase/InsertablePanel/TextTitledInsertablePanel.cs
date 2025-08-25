using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

public class TextTitledInsertablePanel : InsertablePanel
{
    public UITextView TitleText { get; set; }
    public TextTitledInsertablePanel()
    {
        TitleText = new UITextView()
        {
            Margin = new(8),
            TextAlign = new(0,.5f)
        };
        TitleText.Join(this);
        FitWidth = FitHeight = true;
    }
}

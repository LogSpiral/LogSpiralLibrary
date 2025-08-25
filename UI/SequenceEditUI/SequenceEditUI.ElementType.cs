using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public partial class SequenceEditUI
{
#nullable enable

    public static SequenceElementCategory CurrentCategory
    {
        get;
        set
        {
            field = value;
            if (value == null && Active)
            {
                Close(true);
                SequenceTypeSelectUI.Open();
            }
            if (value != null && !Active)
            {
                Open();
                SequenceTypeSelectUI.Close(true);
            }
        }
    } = SequenceSystem.MeleeActionCategoryInstance;

#nullable disable
}
using LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public class SequenceUIPlayer : ModPlayer
{
    private static ModKeybind OpenSequenceEditor { get; set; }

    public override void Load()
    {
        OpenSequenceEditor = KeybindLoader.RegisterKeybind(Mod, nameof(OpenSequenceEditor), Keys.Y);
        base.Load();
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (OpenSequenceEditor.JustPressed)
        {
            if (SequenceEditUI.CurrentCategory == null)
            {
                if (SequenceTypeSelectUI.Active)
                    SequenceTypeSelectUI.Close();
                else
                    SequenceTypeSelectUI.Open();
            }
            else
            {
                if (SequenceEditUI.Active)
                    SequenceEditUI.Close();
                else
                    SequenceEditUI.Open();
            }
        }
        base.ProcessTriggers(triggersSet);
    }
}
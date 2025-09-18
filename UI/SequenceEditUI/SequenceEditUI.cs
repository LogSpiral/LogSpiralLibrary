using LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;
using Terraria.Audio;

namespace LogSpiralLibrary.UI.SequenceEditUI;

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
[RegisterUI("Vanilla: Radial Hotbars", $"{nameof(LogSpiralLibrary)}: {nameof(SequenceEditUI)}")]
public partial class SequenceEditUI : BaseBody
{
    public static SequenceEditUI? Instance { get; private set; }

    public static bool Active { get; set; }

    public override bool Enabled
    {
        get => Active || !SwitchTimer.IsReverseCompleted;
        set => Active = value;
    }

    private void ReloadContent()
    {
        RemoveAllChildren();
        _contentLoaded = false;
        InitializeComponent();
        InitializeComponentExtra();
    }

    public static void Open()
    {
        // 为了方便测试用
        Instance?.ReloadContent();
        if (!Active)
            SoundEngine.PlaySound(SoundID.MenuOpen);
        Active = true;
        foreach (var pair in Instance.PendingSequences)
        {
            if (!CurrentCategory.Maganger.Sequences.ContainsKey(pair.Key)) continue;
            var page = MenuHelper.AppendPage(Instance, pair.Key, pair.Value, false);
            page.PendingModified = true;
        }
        Instance?.CurrentEditTarget = null;
    }

    public static void Close(bool silent = false)
    {
        if (Active && !silent)
            SoundEngine.PlaySound(SoundID.MenuClose);
        Active = false;
        Instance!.OpenedPages.Clear();
        SequenceEditHelperUI.Close();
        SequenceCreateNewUI.Close();
    }
}
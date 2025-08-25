using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using Terraria.Audio;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public partial class SequenceEditUI
{
    private static class SequenceEditUIHelper
    {
        public static UIElementGroup NewDownlistMask()
        {
            var mask = new UIElementGroup()
            {
                Width = new(-16, 1),
                FitHeight = true,
                BackgroundColor = Color.Black * .2f,
                BorderRadius = new(16, 0, 0, 16),
                Padding = new(8),
                Margin = new(8, 8, 8, 0),
                OverflowHidden = true
            };
            mask.OnUpdateStatus += delegate
            {
                if (mask.HoverTimer.IsCompleted) return;
                mask.BackgroundColor = mask.HoverTimer.Lerp(Color.Black * .2f, Color.White * .1f);
            };
            mask.MouseEnter += delegate { SoundEngine.PlaySound(SoundID.MenuTick); };
            return mask;
        }

        public static void HoverCommon(UIView view)
        {
            if (view.HoverTimer.IsCompleted) return;
            view.BackgroundColor = Color.White * (view.HoverTimer.Schedule * .1f);
        }
    }
}

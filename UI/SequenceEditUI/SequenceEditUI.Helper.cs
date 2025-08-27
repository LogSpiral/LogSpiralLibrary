using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using Terraria.Audio;
using Terraria.Localization;

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
            mask.MouseEnter += delegate { SoundEngine.PlaySound(SoundID.MenuTick); };
            return mask;
        }

        public static void HoverColor(UIView view,Color From,Color To)
        {
            if (view.HoverTimer.IsCompleted) return;
            view.BackgroundColor = view.HoverTimer.Lerp(From, To);
        }

        public static string GetText(string suffix) => Language.GetTextValue($"Mods.{nameof(LogSpiralLibrary)}.SequenceUI.{suffix}");

        public static void RecoverPreviousActivePageColor(SequenceEditUI instance) 
        {
            if (instance._currentPageFullName != null && instance.OpenedPages.TryGetValue(instance._currentPageFullName, out var previous))
            {
                previous.BackgroundColor = default;
                var prevIndex = instance.PagePanel.GetInnerChildIndex(previous);
                instance.PagePanel.PendingChildren[prevIndex - 1].BackgroundColor = Color.Black * .25f;
                instance.PagePanel.PendingChildren[prevIndex + 1].BackgroundColor = Color.Black * .25f;
            }
        }

        public static UIElementGroup FastIconTextButton(Asset<Texture2D> icon,string localizationSuffix) 
        {
            var result = new UIElementGroup()
            {
                FitHeight = true,
                FitWidth = true,
                BorderRadius = new(8f),
                Padding = new(8),
                BackgroundColor = Color.Black * .2f,
                BorderColor = Color.Black,
                Border = 1,
                Gap = new(8)
            };
            result.OnUpdateStatus += delegate { HoverColor(result, Color.Black * .2f, Color.White * .1f); };

            var iconView = new SUIImage(icon);
            iconView.Join(result);

            var textView = new UITextView()
            {
                Text = GetText(localizationSuffix)
            };
            textView.Join(result);

            return result;
        }
    }
}

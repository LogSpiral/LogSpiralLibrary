using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Extensions;
using System.Linq;
using LogSpiralLibrary.CodeLibrary.Utilties;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

[XmlElementMapping(nameof(GroupPanel))]
public class GroupPanel : MultiPanel
{
    public GroupPanel() => InsertContainerPanel.FlexDirection = FlexDirection.Column;

    protected override int HandlePVState()
    {
        var mousePos = Main.MouseScreen;
        var innerBounds = InsertContainerPanel.OuterBounds;
        var padding = Padding;

        if (mousePos.X < innerBounds.Left || mousePos.X > innerBounds.Right)
            return -1;

        if (mousePos.Y < InnerPanels[0].OuterBounds.Top && mousePos.Y > innerBounds.Top - padding.Top)
            return 4;

        int count = InnerPanels.Count;
        if (mousePos.Y > InnerPanels[^1].OuterBounds.Bottom && mousePos.Y < innerBounds.Bottom + padding.Bottom)
            return 4 + count;

        for (int n = 0; n < count - 1; n++)
            if (mousePos.Y > InnerPanels[n].OuterBounds.Bottom && mousePos.Y < InnerPanels[n + 1].OuterBounds.Top)
                return 5 + n;

        return -1;
    }

    protected override void HandlePreviewAnimation()
    {
        var index = _pvState - 4;

        if (InsertContainerPanel.Children.Count <= index || InsertContainerPanel.Children[index] != _pvView)
            InsertContainerPanel.Add(_pvView, Math.Min(index, InsertContainerPanel.Children.Count));

        var factor = _pvTimer.Schedule;
        if (_pvContainer.Parent != null)
        {
            var parent = _pvContainer.Parent;
            _pvContainer.Remove();
            this.Join(parent);
        }
        Vector2 deltaSize = _pvSize - _origSize;
        if (deltaSize.X < 0) deltaSize.X = 0;
        if (deltaSize.Y < 0) deltaSize.Y = 0;
        _pvView.SetWidth(_pvSize.X - deltaSize.X * (1 - factor));
        _pvView.SetHeight(_pvSize.Y * factor);
        float gapHeight = InsertContainerPanel.Gap.Height;
        if (index == 0)
        {
            _pvView.Margin = new(
                _pvMargin.Left * factor,
                _pvMargin.Top * factor,
                _pvMargin.Right * factor,
                _pvMargin.Bottom * factor - gapHeight * (1 - factor));

            BaseView.FixPointOffset = new Vector2(0, -gapHeight * factor - factor * _pvSize.Y);
        }
        else if (index < InnerPanels.Count)
        {
            _pvView.Margin = new(
                _pvMargin.Left * factor,
                _pvMargin.Top * factor - gapHeight * (1 - factor) * .5f,
                _pvMargin.Right * factor,
                _pvMargin.Bottom * factor - gapHeight * (1 - factor) * .5f);

            BaseView.FixPointOffset = new Vector2(0, -gapHeight * factor - factor * _pvSize.Y);
        }
        else
        {
            _pvView.Margin = new(
                _pvMargin.Left * factor,
                _pvMargin.Top * factor - gapHeight * (1 - factor),
                _pvMargin.Right * factor,
                _pvMargin.Bottom * factor);

            BaseView.FixPointOffset = default;
        }

        _pvView.Border = factor;
    }
}
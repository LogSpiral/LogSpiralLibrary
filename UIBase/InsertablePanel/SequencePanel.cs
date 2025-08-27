using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Extensions;
using System.Linq;
using LogSpiralLibrary.CodeLibrary.Utilties;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

[XmlElementMapping(nameof(SequencePanel))]
public class SequencePanel : MultiPanel
{
    public SequencePanel() => InsertContainerPanel.FlexDirection = FlexDirection.Row;

    protected override int HandlePVState()
    {
        var mousePos = Main.MouseScreen;
        var innerBounds = InsertContainerPanel.OuterBounds;
        var padding = Padding;

        //Main.NewText((mousePos, innerBounds, innerBounds.Left - padding.Left));

        if (mousePos.Y < innerBounds.Top || mousePos.Y > innerBounds.Bottom)
            return -1;

        if (mousePos.X < InnerPanels[0].OuterBounds.Left && mousePos.X > innerBounds.Left - padding.Left)
            return 4;

        int count = InnerPanels.Count;
        if (mousePos.X > InnerPanels[^1].OuterBounds.Right && mousePos.X < innerBounds.Right + padding.Right)
            return 4 + count;

        for (int n = 0; n < count - 1; n++)
            if (mousePos.X > InnerPanels[n].OuterBounds.Right && mousePos.X < InnerPanels[n + 1].OuterBounds.Left)
                return 5 + n;

        return -1;
    }

    protected override void HandlePreviewAnimation()
    {
        var index = _pvState - 4;
        if (InsertContainerPanel.PendingChildren.Count <= index || InsertContainerPanel.PendingChildren[index] != _pvView)
            InsertContainerPanel.Add(_pvView, Math.Min(index, InsertContainerPanel.PendingChildren.Count));

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
        _pvView.SetWidth(_pvSize.X * factor);
        _pvView.SetHeight(_pvSize.Y - deltaSize.Y * (1 - factor));
        float gapWidth = InsertContainerPanel.Gap.Width;
        if (index == 0)
        {
            _pvView.Margin = new(
                _pvMargin.Left * factor,
                _pvMargin.Top,
                _pvMargin.Right * factor - gapWidth * (1 - factor),
                _pvMargin.Bottom);

            BaseView.FixPointOffset = new Vector2(-gapWidth * factor - factor * _pvSize.X, 0) * .5f;
        }
        else if (index < InnerPanels.Count)
        {
            _pvView.Margin = new(
                _pvMargin.Left * factor - gapWidth * (1 - factor) * .5f,
                _pvMargin.Top,
                _pvMargin.Right * factor - gapWidth * (1 - factor) * .5f,
                _pvMargin.Bottom);

            BaseView.FixPointOffset = new Vector2(-gapWidth * factor - factor * _pvSize.X, 0) * .5f;
        }
        else
        {
            _pvView.Margin = new(
                _pvMargin.Left * factor - gapWidth * (1 - factor),
                _pvMargin.Top,
                _pvMargin.Right * factor,
                _pvMargin.Bottom);

            BaseView.FixPointOffset = default;
        }

        _pvView.Border = factor;
    }
}
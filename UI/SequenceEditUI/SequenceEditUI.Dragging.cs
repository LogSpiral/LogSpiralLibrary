using SilkyUIFramework;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public partial class SequenceEditUI
{
    private Vector2 _dragOffset;
    private bool _fixSize;
    private bool _dragging;

    public override void OnLeftMouseDown(UIMouseEvent evt)
    {
        if (evt.Source == this)
        {
            var bounds = Bounds;
            var coord = (Main.MouseScreen - bounds.Position) / (Vector2)bounds.Size;
            _fixSize = Vector2.Dot(coord, Vector2.One) < 1;
            _dragging = true;
            _dragOffset = Main.MouseScreen - (_fixSize ? new Vector2(Left.Pixels, Top.Pixels) : new Vector2(Width.Pixels, Height.Pixels));
        }
        base.OnLeftMouseDown(evt);
    }

    public override void OnLeftMouseUp(UIMouseEvent evt)
    {
        _dragging = false;
        base.OnLeftMouseUp(evt);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (_dragging)
        {
            var vec = Main.MouseScreen - _dragOffset;
            if (_fixSize)
            {
                SetLeft(vec.X);
                SetTop(vec.Y);
            }
            else
            {
                SetSize(
                    MathF.Max(vec.X, MinWidth.Pixels - Main.screenWidth),
                    MathF.Max(vec.Y, MinHeight.Pixels - Main.screenHeight)
                    );
            }
        }
    }
}

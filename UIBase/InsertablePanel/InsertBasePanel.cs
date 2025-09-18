using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;
using Terraria.Audio;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
[XmlElementMapping("InsertBasePanel")]
public class InsertBasePanel : UIElementGroup
{
    public InsertablePanel RootElement
    {
        get;
        set;
    }

    public InsertablePanel GetInsertablePanelAt(Vector2 mousePosition)
    {
        if (Invalid) return null;

        if (OverflowHidden && !ContainsPoint(mousePosition)) return null;

        return RootElement?.GetInnerInsertablePanelAt(mousePosition);
    }

    #region Fixing

    private Vector2 FixedPoint { get; set; }

    public Vector2 FixPointOffset { get; set; }

#nullable enable

    public bool RecoverAfterFixing { get; set; } = true;

    /// <summary>
    /// 固定目标
    /// </summary>
    public UIView? FixedTarget
    {
        get;
        set
        {

            if (PendingRecoverPositionRejected)
                PendingRecoverPositionRejected = false;
            else if (value == null && field != null && RecoverAfterFixing)
            {
                RootElement.SetLeft(_origPosition.X);
                RootElement.SetTop(_origPosition.Y);
            }
            RecoverAfterFixing = true;
            if (value != null)
            {
                _origPosition = new(RootElement.Left.Pixels, RootElement.Top.Pixels);
                FixedPoint = value.Bounds.Position;
            }

            field = value;
        }
    }

    /// <summary>
    /// 预览时根元素，仅用作固定
    /// </summary>
    public UIElementGroup? PVRoot { get; set; }

    private Vector2 _origPosition;

    public bool PendingRecoverPositionRejected { get; set; }

#nullable disable

    public override void UpdatePosition()
    {
        base.UpdatePosition();

        if (FixedTarget == null)
            return;

        Vector2 target = FixedPoint + FixPointOffset - FixedTarget.Bounds.Position;

        if (target == default)
            return;

        var root = PVRoot ?? RootElement;
        root.SetLeft(root.Left.Pixels + target.X, 0, 0);
        root.SetTop(root.Top.Pixels + target.Y, 0, 0);
        root.UpdatePosition();
    }

    #endregion Fixing

    #region Dragging

    private bool _dragging;
    private Vector2 _offset;

    public override void OnLeftMouseDown(UIMouseEvent evt)
    {
        if (evt.Source == this && RootElement != null)
        {
            _dragging = true;
            _offset = evt.MousePosition - new Vector2(RootElement.Left.Pixels, RootElement.Top.Pixels);
        }
        base.OnLeftMouseDown(evt);
    }

    public override void OnLeftMouseUp(UIMouseEvent evt)
    {
        _dragging = false;
        base.OnLeftMouseUp(evt);
    }
    public override void OnRightMouseClick(UIMouseEvent evt)
    {
        if (evt.Source == this && RootElement != null)
        {
            SoundEngine.PlaySound(SoundID.Chat);
            RootElement.SetLeft(Bounds.Width * .25f);
            RootElement.SetTop(Bounds.Height * .25f);
        }
        base.OnRightMouseClick(evt);
    }
    protected override void Update(GameTime gameTime)
    {
        if (RootElement == null) goto label;
        if (_dragging)
        {
            var pos = Main.MouseScreen - _offset;
            RootElement.SetLeft(pos.X, 0, 0);
            RootElement.SetTop(pos.Y, 0, 0);
        }
        if (MathF.Abs(_scrollDeltaX) > 1E-3f)
        {
            var delta = _scrollDeltaX * .2f;
            _scrollDeltaX -= delta;
            RootElement.SetLeft(RootElement.Left.Pixels + delta, 0, 0);
        }
        if (MathF.Abs(_scrollDeltaY) > 1E-3f)
        {
            var delta = _scrollDeltaY * .2f;
            _scrollDeltaY -= delta;
            RootElement.SetTop(RootElement.Top.Pixels + delta, 0, 0);
        }
    label:
        base.Update(gameTime);
    }

    #endregion Dragging

    #region Scroll
    float _scrollDeltaX;
    float _scrollDeltaY;
    public override void OnMouseWheel(UIScrollWheelEvent evt)
    {
        if (RootElement == null) return;
        if (Main.keyState.PressingControl())
            _scrollDeltaX += evt.ScrollDelta;
        else if (Main.keyState.PressingShift())
            _scrollDeltaY += evt.ScrollDelta;

        base.OnMouseWheel(evt);
    }
    #endregion

}
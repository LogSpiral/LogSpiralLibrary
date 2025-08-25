using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.BasicElements;

namespace LogSpiralLibrary.UIBase;

[XmlElementMapping(nameof(CompressLine))]
public class CompressLine : UIView
{
    public Direction Direction
    {
        get;
        set
        {
            bool flag = field != value;
            field = value;
            switch (value)
            {
                case Direction.Horizontal:
                    SetWidth(0, 1);
                    SetHeight(8, 0);
                    if (TargetView != null)
                    {
                        if (TargetDimension != default && flag)
                            TargetView.Width = TargetDimension;
                        if (TargetDimensionMax != default && flag)
                            TargetView.MaxWidth = TargetDimensionMax;
                        TargetDimension = TargetView.Height;
                        TargetDimensionMax = TargetView.MaxHeight;
                    }
                    break;

                case Direction.Vertical:
                    SetWidth(8, 0);
                    SetHeight(0, 1);
                    if (TargetView != null)
                    {
                        if (TargetDimension != default && flag)
                            TargetView.Height = TargetDimension;
                        if (TargetDimensionMax != default && flag)
                            TargetView.MaxHeight = TargetDimensionMax;
                        TargetDimension = TargetView.Width;
                        TargetDimensionMax = TargetView.MaxWidth;
                    }
                    break;

                default:
                    break;
            }
        }
    }

    public bool IsExpand
    {
        get;
        set
        {
            field = value;
            if (value)
                _expandTimer.StartUpdate();
            else
            {
                _expandTimer.StartReverseUpdate();
                if (TargetView != null)
                {
                    TargetDimension = Direction == Direction.Horizontal ? TargetView.Height : TargetView.Width;
                    TargetDimensionMax = Direction == Direction.Horizontal ? TargetView.MaxHeight : TargetView.MaxWidth;
                }
            }
        }
    } = true;

    public UIView TargetView { get; set; }
    private Dimension TargetDimension { get; set; }
    private Dimension TargetDimensionMax { get; set; }
    private readonly AnimationTimer _expandTimer = new(5);

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);
        if (TargetView == null) return;
        _expandTimer.Update(gameTime);
        if (TargetDimension == default)
        {
            TargetDimension = Direction == Direction.Horizontal ? TargetView.Height : TargetView.Width;
            TargetDimensionMax = Direction == Direction.Horizontal ? TargetView.MaxHeight : TargetView.MaxWidth;
            if (IsExpand) _expandTimer.ImmediateCompleted();
            else _expandTimer.ImmediateReverseCompleted();
        }
        var factor = _expandTimer.Schedule;
        switch (Direction)
        {
            case Direction.Horizontal:
                TargetView.SetHeight(factor * TargetDimension.Pixels, factor * TargetDimension.Percent);
                TargetView.SetMaxHeight(factor * TargetDimensionMax.Pixels, factor * TargetDimensionMax.Percent);
                break;

            case Direction.Vertical:
                TargetView.SetWidth(factor * TargetDimension.Pixels, factor * TargetDimension.Percent);
                TargetView.SetMaxWidth(factor * TargetDimensionMax.Pixels, factor * TargetDimensionMax.Percent);
                break;

            default:
                break;
        }
    }

    public override void OnLeftMouseClick(UIMouseEvent evt)
    {
        base.OnLeftMouseClick(evt);
        IsExpand = !IsExpand;
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);
    }
}
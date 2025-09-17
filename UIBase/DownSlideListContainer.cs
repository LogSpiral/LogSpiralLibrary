using LogSpiralLibrary.CodeLibrary.Utilties;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.BasicElements;

namespace LogSpiralLibrary.UIBase;

// [JITWhenModsEnabled("SilkyUIFramework")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
public partial class DownSlideListContainer : UIElementGroup
{
    public DownSlideListContainer()
    {
        InitializeComponent();

        Title.LeftMouseClick += delegate
        {
            _expanded = !_expanded;
            if (_expanded)
                _expandTimer.StartUpdate();
            else
                _expandTimer.StartReverseUpdate();
        };
        List.Container.Padding = new(0, 8, 0, 8);
    }

    protected readonly AnimationTimer _expandTimer = new(3);
    private bool _expanded;
    private float _targetHeight;
    protected bool ForcedUpdateHeight { get; set; }
    public override void HandleUpdateStatus(GameTime gameTime)
    {
        base.HandleUpdateStatus(gameTime);
        _targetHeight = List.Container.OuterBounds.Height;
        if (Parent is SUIScrollContainer)
            _targetHeight = MathF.Min(400, _targetHeight);
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        _expandTimer.Update(gameTime);
        if (!_expandTimer.IsCompleted || ForcedUpdateHeight)
        {
            ForcedUpdateHeight = false;
            var factor = _expandTimer.Schedule;

            List.SetHeight(factor * _targetHeight);
        }
        base.UpdateStatus(gameTime);
    }

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.HandleDraw(gameTime, spriteBatch);

        Vector2[] trianglePercentCoord = new Vector2[3];
        for (int n = 0; n < 3; n++)
            trianglePercentCoord[n] = _expandTimer.Lerp(CloseStateCoord[n], OpenStateCoord[n]);

        Vector2 pos = Title.Bounds.RightCenter + new Vector2(-32, -8);
        Vector2 size = new(16);

        SDFGraphics.NoBorderTriangle(
            pos + size * trianglePercentCoord[0],
            pos + size * trianglePercentCoord[1],
            pos + size * trianglePercentCoord[2],
            Color.White,
            SDFGraphics.GetMatrix(true));
    }

    private Vector2[] OpenStateCoord { get; } = [new(0.0f, 0.3f), new(0.5f, 0.8f), new(1.0f, 0.3f)];
    private Vector2[] CloseStateCoord { get; } = [new(0.7f, 0.0f), new(0.2f, 0.5f), new(0.7f, 1.0f)];
}
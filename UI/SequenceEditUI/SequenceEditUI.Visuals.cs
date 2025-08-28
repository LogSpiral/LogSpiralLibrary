using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;
using LogSpiralLibrary.UIBase.InsertablePanel;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Graphics2D;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public partial class SequenceEditUI
{
    private AnimationTimer SwitchTimer { get; init; } = new(3);

    internal InsertablePanel CurrentEditTarget { get; set; }

    protected override void UpdateStatus(GameTime gameTime)
    {
        if (Active) SwitchTimer.StartUpdate();
        else SwitchTimer.StartReverseUpdate();

        SwitchTimer.Update(gameTime);

        UseRenderTarget = SwitchTimer.IsUpdating;
        Opacity = SwitchTimer.Lerp(0f, 1f);

        var center = Bounds.Center * Main.UIScale;
        RenderTargetMatrix =
            Matrix.CreateTranslation(-center.X, -center.Y, 0) *
            Matrix.CreateScale(SwitchTimer.Lerp(0.95f, 1f), SwitchTimer.Lerp(0.95f, 1f), 1) *
            Matrix.CreateTranslation(center.X, center.Y, 0);

        BlackMaskTimer.Update(gameTime);
        if (BlackMaskTimer.IsUpdating)
            BlackMask.BackgroundColor = Color.Black * (.25f * BlackMaskTimer.Schedule);
        if (BlackMaskTimer.IsReverseCompleted && BlackMask.Parent != null)
            BlackMask.Remove();
        base.UpdateStatus(gameTime);
    }
    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {



        if (BlurMakeSystem.BlurAvailable)
        {
            if (BlurMakeSystem.SingleBlur)
            {
                var batch = Main.spriteBatch;
                batch.End();
                BlurMakeSystem.KawaseBlur();
                batch.Begin();
            }
            if (MainContainer == null) return;
            SDFRectangle.SampleVersion(BlurMakeSystem.BlurRenderTarget,
                MainContainer.Bounds.Position * Main.UIScale, MainContainer.Bounds.Size * Main.UIScale, MainContainer.BorderRadius * Main.UIScale, Matrix.Identity);
        }

        base.Draw(gameTime, spriteBatch);

        if (CurrentEditTarget != null && PropertyPanelConfig != null && _currentPageFullName != null && DataLine.ExpandFactor > 0)
        {
            var factor = DataLine.ExpandFactor * .25f;
            var start = CurrentEditTarget.InnerBounds.LeftTop;
            if (!BasePanel.ContainsPoint(start)) return;
            var end = PropertyPanelConfig.InnerBounds.RightTop;
            spriteBatch.DrawHorizonBLine(start, end, Main.DiscoColor with { A = 0 } * factor, 1, 4);
            spriteBatch.DrawHorizonBLine(start, end, Color.White * factor, 1, 2);

        }

    }
}

using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.UIBase.InsertablePanel;
using ReLogic.Graphics;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Layout;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
internal class GroupArgumentDecorator : IInsertPanelDecorator
{
    private AnimationTimer HiddenTimer { get; } = new();
    public IGroupArgument Argument { get; set; }
    public HorizontalRule HorizontalRule { get; set; }
    public UITextView ArgumentText { get; set; }
    public UIElementGroup Mask { get; set; }
    public UIElementGroup InnerContainer { get; set; }

    public InsertablePanel.InsertablePanel InsertablePanel { get; set; }

    public void Decorate(InsertablePanel.InsertablePanel panel)
    {
        InsertablePanel = panel;
        panel.FlexDirection = FlexDirection.Column;
        HorizontalRule = new HorizontalRule
        {
            Margin = new Margin(16, 0, 4, 0),
            IgnoreMouseInteraction = true
        };
        panel.AddChild(HorizontalRule);
        var mask = Mask = new UIElementGroup
        {
            OverflowHidden = true,
            IgnoreMouseInteraction = true
        };
        var container = new UIElementGroup()
        {
            FitWidth = true,
            FitHeight = true,
            IgnoreMouseInteraction = true
        };
        container.Join(mask);
        InnerContainer = container;
        ArgumentText = new UITextView
        {
            Margin = new Margin(16, 0, 4, 8),
            TextAlign = new Vector2(0, .5f),
            Top = new Anchor(0, 0, .5f),
            IgnoreMouseInteraction = true
        };
        ArgumentText.ContentChanged += delegate
        {
            UpdateVisuals();
        };
        if (Argument != null)
        {
            if (Argument.IsHidden)
                HiddenTimer.ImmediateReverseCompleted();
            else
                HiddenTimer.ImmediateCompleted();
            ArgumentText.Text = $"->{Argument}";
        }
        ArgumentText.OnUpdateStatus += (gameTime) =>
        {
            HiddenTimer.Update(gameTime);
            if (Argument != null)
            {
                if (!Argument.IsHidden && HiddenTimer.IsReverse)
                    HiddenTimer.StartUpdate();
                if (Argument.IsHidden && HiddenTimer.IsForward)
                    HiddenTimer.StartReverseUpdate();

                ArgumentText.Text = $"->{Argument}";
            }
            UpdateVisuals();
        };
        container.AddChild(ArgumentText);

        panel.AddChild(mask);

        // panel.DrawAction += DrawGroupArg;
    }

    private void UpdateVisuals()
    {
        var factor = HiddenTimer.Schedule;
        HorizontalRule.SetHeight(factor * 4, 0);
        HorizontalRule.Border = factor;
        Mask.SetHeight(InnerContainer.Bounds.Height * factor, 0);
        Mask.SetWidth(InnerContainer.Bounds.Width * factor, 0);
    }

    public void UnloadDecorate(InsertablePanel.InsertablePanel panel)
    {
        HorizontalRule.RemoveFromParent();
        ArgumentText.RemoveFromParent();
        // panel.DrawAction -= DrawGroupArg;
    }

    private void DrawGroupArg(GameTime gameTime, SpriteBatch spriteBatch)
    {
#if false
        spriteBatch.DrawString(FontAssets.MouseText.Value, Argument.ToString(), InsertablePanel.Bounds.GetPercentedCoord(0, .33f), Color.Lime);
#else
        spriteBatch.DrawString(FontAssets.MouseText.Value, "□", InsertablePanel.Bounds.GetPercentedCoord(0, .33f), Color.Lime);
#endif
    }
}
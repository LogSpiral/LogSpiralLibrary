using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.UIBase.InsertablePanel;
using Microsoft.Build.Utilities;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;

internal class GroupArgumentDecorator : IInsertPanelDecorator
{
    AnimationTimer HiddenTimer { get; } = new();

    public WrapperArgPair Pair { get; set; }

    public HorizontalRule HorizontalRule { get; set; }
    public UITextView ArgumentText { get; set; }
    public UIElementGroup Mask { get; set; }
    public UIElementGroup InnerContainer { get; set; }
    public void Decorate(InsertablePanel.InsertablePanel panel)
    {
        panel.FlexDirection = FlexDirection.Column;
        HorizontalRule = new()
        {
            Margin = new(16,0,4,0),
            IgnoreMouseInteraction = true
        };
        panel.Add(HorizontalRule);
        var mask = Mask = new()
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
        ArgumentText = new()
        {
            Margin = new(16, 0, 4, 8),
            TextAlign = new(0, .5f),
            Top = new(0, 0, .5f),
            IgnoreMouseInteraction = true
        };
        if (Pair != null)
        {
            ArgumentText.Text = $"->{Pair.Argument.ToString()}";
            if (Pair.Argument.IsHidden)
                HiddenTimer.ImmediateReverseCompleted();
            else
                HiddenTimer.ImmediateCompleted();
        }
        ArgumentText.ContentChanged += delegate
        {
            UpdateVisuals();
        };
        ArgumentText.OnUpdateStatus += (gameTime) =>
        {

            HiddenTimer.Update(gameTime);
            if (Pair != null)
            {
                if (!Pair.Argument.IsHidden && HiddenTimer.IsReverse)
                    HiddenTimer.StartUpdate();
                if (Pair.Argument.IsHidden && HiddenTimer.IsForward)
                    HiddenTimer.StartReverseUpdate();

                ArgumentText.Text = $"->{Pair.Argument.ToString()}";
            }
            UpdateVisuals();

            // Main.NewText((HorizontalRule.Width, HorizontalRule.Parent.Bounds, HorizontalRule.Bounds));
        };
        container.Add(ArgumentText);

        panel.Add(mask);
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
        HorizontalRule.Remove();
        ArgumentText.Remove();
    }


}

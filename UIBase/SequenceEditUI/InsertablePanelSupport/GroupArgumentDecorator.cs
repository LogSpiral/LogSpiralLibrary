using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.UIBase.InsertablePanel;
using Microsoft.Build.Utilities;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
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
    public void Decorate(InsertablePanel.InsertablePanel panel)
    {
        panel.FlexDirection = FlexDirection.Column;
        HorizontalRule = new();
        panel.Add(HorizontalRule);
        var mask = new UIElementGroup()
        {
            FitWidth = true,
            FitHeight = true,
            OverflowHidden = true
        };
        Mask = mask;
        ArgumentText = new()
        {
            Margin = new(16, 0, 4, 8),
            TextAlign = new(0, .5f),
            Top = new(0, 0, .5f)
        };
        if (Pair != null)
        {
            ArgumentText.Text = $"->{Pair.Argument.ToString()}";
            if (Pair.Argument.IsHidden)
                HiddenTimer.ImmediateReverseCompleted();
            else
                HiddenTimer.ImmediateCompleted();
            UpdateVisuals();
        }

        ArgumentText.OnUpdateStatus += (gameTime) =>
        {
            if (Pair?.Argument?.IsHidden is false && HiddenTimer.IsReverse)
                HiddenTimer.StartUpdate();
            if (Pair?.Argument?.IsHidden is true && HiddenTimer.IsForward)
                HiddenTimer.StartReverseUpdate();
            HiddenTimer.Update(gameTime);
            if (Pair != null)
                ArgumentText.Text = $"->{Pair.Argument.ToString()}";
            if (HiddenTimer.IsCompleted) return;
            UpdateVisuals();

        };
        mask.Add(ArgumentText);

        panel.Add(mask);
    }

    private void UpdateVisuals() 
    {
        var factor = HiddenTimer.Schedule;
        HorizontalRule.SetWidth(114514);
        HorizontalRule.SetHeight(factor * 4, 0);
        HorizontalRule.Border = factor;
        var bounds = HorizontalRule.Bounds;
        Mask.SetMaxHeight((ArgumentText.TextSize.Y * ArgumentText.TextScale + ArgumentText.Margin.Vertical) * factor, 0);
        Mask.SetMaxWidth((ArgumentText.TextSize.X * ArgumentText.TextScale + ArgumentText.Margin.Horizontal) * factor, 0);
        HorizontalRule.SetMaxWidth(Mask.MaxWidth.Pixels * .8f);
    }

    public void UnloadDecorate(InsertablePanel.InsertablePanel panel)
    {
        HorizontalRule.Remove();
        ArgumentText.Remove();
    }


}

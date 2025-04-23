using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UIBase;
public static class SequenceDrawHelper
{
    public static Vector2 WrapperSize(this WraperBox wrapBox)
    {
        Vector2 curr = wrapBox.GetSize();
        if (curr == default || wrapBox.CacheRefresh)
        {
            wrapBox.CacheRefresh = false;
            Vector2 delta;
            var wraper = wrapBox.wraper;
            var desc = wraper.conditionDefinition;
            if (wraper.IsSequence && wrapBox.sequenceBox.Expand && wraper.SequenceInfo.FileName == Sequence.SequenceDefaultName)
            {
                delta = wrapBox.sequenceBox.SequenceSize();
                if (wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey)
                {
                    var textSize = FontAssets.MouseText.Value.MeasureString("→" + desc.DisplayName);
                    delta.Y += textSize.Y;
                    delta.X = Math.Max(textSize.X, delta.X);
                }
                delta += new Vector2(32, 32);
                wrapBox.sequenceBox.SetSize(delta);
                wrapBox.sequenceBox.Recalculate();
            }
            else
            {
                var font = FontAssets.MouseText.Value;
                var name = wraper.Name;
                if (wraper.IsSequence && SequenceSystem.sequenceInfos.TryGetValue(wraper.SequenceInfo.KeyName, out var value))
                    name = value.DisplayName;
                //if (name == "挥砍" && desc != "Always") 
                //{
                //    Main.NewText((font.MeasureString(name), font.MeasureString(desc)));
                //}
                Vector2 textSize = font.MeasureString(name);
                Vector2 boxSize = textSize;
                if (wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey)
                {
                    Vector2 descSize = font.MeasureString("→" + desc.DisplayName);
                    boxSize.Y += descSize.Y;
                    boxSize.X = Math.Max(textSize.X, descSize.X);
                }
                boxSize += Vector2.One * 32;
                delta = boxSize;
            }
            wrapBox.SetSize(delta);
            wrapBox.Recalculate();

            return delta;
        }
        return curr;
    }
    public static Vector2 GroupSize(this GroupBox groupBox)
    {
        Vector2 curr = groupBox.GetSize();
        if (curr == default || groupBox.CacheRefresh)
        {
            groupBox.CacheRefresh = false;
            Vector2 result = default;
            foreach (var wrapper in groupBox.wraperBoxes)
            {
                //if (!wrapper.wraper.Available) continue;
                Vector2 delta = wrapper.WrapperSize();
                result.Y += delta.Y + SequenceConfig.Instance.Step.Y;
                if (delta.X > result.X) result.X = delta.X;
            }
            groupBox.SetSize(result);
            groupBox.Recalculate();
            return result;
        }
        return curr;
    }
    public static Vector2 SequenceSize(this SequenceBox sequencebox, bool start = false)
    {

        Vector2 curr = sequencebox.GetSize();
        if (curr == default || sequencebox.CacheRefresh)
        {
            sequencebox.CacheRefresh = false;
            Vector2 result = new(start ? 32 : 0, 0);//sequencebox.startSequence // SequenceConfig.Instance.Step.X * .5f
            foreach (var group in sequencebox.groupBoxes)
            {
                Vector2 delta = group.GroupSize();
                result.X += delta.X + SequenceConfig.Instance.Step.X;
                if (delta.Y > result.Y) result.Y = delta.Y;
            }

            sequencebox.SetSize(result);
            sequencebox.Recalculate();
            //if (Main.chatMonitor is RemadeChatMonitor remade)
            //{
            //    remade._showCount = 40;
            //}
            return result;
        }
        return curr;
    }
    //static void DrawWrapper(this SpriteBatch spriteBatch, MeleeSequence.MeleeSAWraper meleeSAWraper, Vector2 position)
    //{
    //    /*
    //    Vector2 size = MeleeWrapperSize(meleeSAWraper);
    //    if (meleeSAWraper.IsSequence)
    //    {

    //    }
    //    else 
    //    {
    //        var font = FontAssets.MouseText.Value;
    //        var name = wrapper.attackInfo.GetType().Name;
    //        Vector2 textSize = font.MeasureString(name);
    //        Vector2 boxSize = textSize;
    //        if (desc != "Always")
    //        {
    //            Vector2 descSize = font.MeasureString(desc);
    //            boxSize.Y += descSize.Y;
    //            boxSize.X = Math.Max(textSize.X, descSize.X);
    //        }
    //        boxSize += Vector2.One * 32;
    //        #region 框框
    //        ComplexPanelInfo panel = new ComplexPanelInfo();
    //        panel.destination = Utils.CenteredRectangle(positionHere + boxSize * .5f - new Vector2(16), boxSize);
    //        panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_0").Value;
    //        panel.glowEffectColor = Color.White with { A = 0 };
    //        panel.glowShakingStrength = 0;
    //        panel.glowHueOffsetRange = 0;
    //        panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
    //        panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
    //        panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
    //        panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
    //        panel.DrawComplexPanel(spriteBatch);
    //        #endregion
    //        spriteBatch.DrawString(FontAssets.MouseText.Value, name, positionHere, wrapper.attackInfo.timerMax > 0 ? Color.Cyan : Color.Gray, 0, default, 1f, 0, 0);
    //        if (desc != "Always")
    //        {
    //            spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, positionHere + textSize.Y * Vector2.UnitY, wrapper.condition.IsMet() ? Color.MediumPurple : Color.Gray);
    //        }
    //    }
    //}
    //public static void DrawMeleeSequence(this SpriteBatch spriteBatch, MeleeSequence meleeSequence, Vector2 position)
    //{
    //    int counterX = 0;
    //    foreach (var group in meleeSequence.MeleeGroups)
    //    {
    //        int counterY = 0;
    //        foreach (var wrapper in group.wrapers)
    //        {
    //            Vector2 positionHere = new Vector2(counterX, counterY) * new Vector2(128, 64) * 2 + position;
    //            var desc = wrapper.condition.Description.Value;

    //            if (wrapper.IsElement)
    //            {
    //                //if (wrapper.attackInfo.SkipCheck()) break;
    //                var font = FontAssets.MouseText.Value;
    //                var name = wrapper.attackInfo.GetType().Name;
    //                Vector2 textSize = font.MeasureString(name);
    //                Vector2 boxSize = textSize;
    //                if (desc != "Always")
    //                {
    //                    Vector2 descSize = font.MeasureString(desc);
    //                    boxSize.Y += descSize.Y;
    //                    boxSize.X = Math.Max(textSize.X, descSize.X);
    //                }
    //                boxSize += Vector2.One * 32;
    //                #region 框框
    //                ComplexPanelInfo panel = new ComplexPanelInfo();
    //                panel.destination = Utils.CenteredRectangle(positionHere + boxSize * .5f - new Vector2(16), boxSize);
    //                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_0").Value;
    //                panel.glowEffectColor = Color.White with { A = 0 };
    //                panel.glowShakingStrength = 0;
    //                panel.glowHueOffsetRange = 0;
    //                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
    //                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
    //                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
    //                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
    //                panel.DrawComplexPanel(spriteBatch);
    //                #endregion
    //                spriteBatch.DrawString(FontAssets.MouseText.Value, name, positionHere, wrapper.attackInfo.timerMax > 0 ? Color.Cyan : Color.Gray, 0, default, 1f, 0, 0);
    //                if (desc != "Always")
    //                {
    //                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, positionHere + textSize.Y * Vector2.UnitY, wrapper.condition.IsMet() ? Color.MediumPurple : Color.Gray);

    //                }

    //            }
    //            if (wrapper.IsSequence)
    //            {

    //                spriteBatch.DrawMeleeSequence(wrapper.sequenceInfo, positionHere/*, depth + 1, out Vector2 _finalSize*/);
    //                if (desc != "Always")
    //                {
    //                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, positionHere + new Vector2(0, 64), wrapper.condition.IsMet() ? Color.MediumPurple : Color.Gray);
    //                }
    //            }
    //            counterY++;
    //        }
    //        counterX++;
    //    }
    //}
}

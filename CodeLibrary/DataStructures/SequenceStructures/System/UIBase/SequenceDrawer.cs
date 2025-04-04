using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using ReLogic.Graphics;
using Terraria.UI;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UIBase;
public class SequenceDrawer : UIElement
{
    public SequenceBox box;
    public override void OnInitialize()
    {
        //Append(box);
        base.OnInitialize();
    }
    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (box != null)
        {
            DrawSequence(box, GetDimensions().Position(), default, box.sequenceBase.Active, true);
            box.sequenceBase.Active = false;
        }
        base.DrawSelf(spriteBatch);
    }
    public static void DrawWraper(WraperBox wraperBox, Vector2 position, float offset, bool active)
    {
        var pos = position;
        //position += SequenceConfig.Instance.Step * new Vector2(0, .5f);
        var spriteBatch = Main.spriteBatch;
        var desc = wraperBox.wraper.conditionDefinition.DisplayName;
        bool flag = wraperBox.wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey;
        if (wraperBox.wraper.IsSequence && wraperBox.sequenceBox.Expand)
        {
            ComplexPanelInfo panel = new();
            var boxSize = wraperBox.GetSize();
            //if (flag)
            //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f + new Vector2(0, 16), boxSize + new Vector2(32, 64));
            //else
            //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f, boxSize + new Vector2(32, 32));
            float offY = flag ? FontAssets.MouseText.Value.MeasureString(desc).Y : 0;
            panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X * .5f, 0), boxSize);
            panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
            panel.glowEffectColor = Color.MediumPurple with { A = 0 };
            panel.glowShakingStrength = .1f;
            panel.glowHueOffsetRange = 0.1f;
            panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
            panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
            panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
            panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
            panel.DrawComplexPanel(spriteBatch);
            DrawSequence(wraperBox.sequenceBox, position - offY * .5f * Vector2.UnitY, offY * .5f * Vector2.UnitY, active, false);
            if (flag)
            {
                var cen = position + boxSize * Vector2.UnitY * .5f - offY * 1.5f * Vector2.UnitY + new Vector2(16, 0);
                if (wraperBox.wraper.Condition.IsMet())
                {
                    for (int n = 0; n < 3; n++)
                        spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f), new Color(0.8f, 0.2f, 1f, 0f), 0, default, 1f, 0, 0);
                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen, Color.White, 0, default, 1f, 0, 0);
                }
                else
                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen, Color.Gray);
            }
        }
        else
        {
            //position.Y += wraperBox.GetSize().Y * .5f;
            var font = FontAssets.MouseText.Value;
            var name = wraperBox.wraper.Name;
            Vector2 textSize = font.MeasureString(name);
            Vector2 boxSize = wraperBox.GetSize();
            #region 框框
            ComplexPanelInfo panel = new();
            panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X, 0) * .5f, boxSize);
            panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
            panel.glowEffectColor = Color.Cyan with { A = 0 };
            panel.glowShakingStrength = .05f;
            panel.glowHueOffsetRange = 0.05f;
            panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
            panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
            panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
            panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
            panel.DrawComplexPanel(spriteBatch);
            #endregion
            var cen = position + new Vector2(16) - boxSize * Vector2.UnitY * .5f;
            if (active)
            {
                //var fontOff = font.MeasureString(name) * .5f;
                //spriteBatch.DrawString(font, name, cen + fontOff, Color.Cyan, 0, fontOff, 1.1f, 0, 0);
                //spriteBatch.DrawString(font, name, cen + fontOff, Color.Cyan, 0, fontOff, 0.9f, 0, 0);
                for (int n = 0; n < 3; n++)
                    spriteBatch.DrawString(font, name, cen + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f), new Color(0.2f, 0.8f, 1f, 0f), 0, default, 1f, 0, 0);
                spriteBatch.DrawString(font, name, cen, Color.White, 0, default, 1f, 0, 0);

            }
            else
                spriteBatch.DrawString(font, name, cen, Color.Gray, 0, default, 1f, 0, 0);
            cen += textSize * Vector2.UnitY;
            if (flag)
            {
                if (wraperBox.wraper.Condition.IsMet())
                {

                    for (int n = 0; n < 3; n++)
                        spriteBatch.DrawString(font, "→" + desc, cen + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f), new Color(0.8f, 0.2f, 1f, 0f), 0, default, 1f, 0, 0);
                    spriteBatch.DrawString(font, "→" + desc, cen, Color.White, 0, default, 1f, 0, 0);

                }
                else
                    spriteBatch.DrawString(font, "→" + desc, cen, Color.Gray);
            }
            //spriteBatch.DrawRectangle(panel.destination, Color.MediumPurple);

        }
        if (SequenceConfig.Instance.ShowWrapBox)
            Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + wraperBox.GetSize() * Vector2.UnitX * .5f, wraperBox.GetSize()), Color.MediumPurple * .5f, 8);

        //锚点
        //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.MediumPurple * .5f, 0, new Vector2(.5f), 16, 0, 0);


    }
    public static void DrawGroup(GroupBox groupBox, Vector2 position, bool active)
    {
        Color GroupColor = Color.Cyan;
        var pos = position;
        var size = groupBox.GetSize();
        position.Y -= size.Y * .5f;
        var tarCen1 = pos + new Vector2(-SequenceConfig.Instance.Step.X * .25f, 0);
        var tarCen2 = pos + new Vector2(SequenceConfig.Instance.Step.X * .25f + size.X, 0);
        int c = 0;
        var font = FontAssets.MouseText.Value;
        foreach (var w in groupBox.wraperBoxes)
        {
            var desc = w.wraper.conditionDefinition;
            float offY = w.wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey ? font.MeasureString("→" + desc.DisplayName).Y : 0;
            Vector2 wsize = w.GetSize();
            position.Y += (wsize.Y + SequenceConfig.Instance.Step.Y - offY) * .5f;
            var offset = size.X - wsize.X;
            var scale = 1 + ((float)LogSpiralLibraryMod.ModTime / 180).CosFactor();
            //scale = 1;
            position.X = pos.X + offset * .5f;

            var tarCen3 = position;
            //var tarCen4 = w.wraper.IsSequence ? position + wsize * new Vector2(1, 0) + new Vector2(offset * 1.5f, 0) * .5f : position + new Vector2(offset * .5f + wsize.X, 0);
            var tarCen4 = tarCen3 + wsize.X * Vector2.UnitX;
            Main.spriteBatch.DrawHorizonBLine(tarCen3, tarCen1, Color.White, scale);
            Main.spriteBatch.DrawHorizonBLine(tarCen4, tarCen2, Color.White, scale);
            DrawWraper(w, position + new Vector2(0, offY * .5f), offset, active && groupBox.group.Index == c);//
            c++;
            position.Y += (wsize.Y + SequenceConfig.Instance.Step.Y + offY) * .5f;


        }
        //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.DarkCyan, 0, new Vector2(.5f), 16, 0, 0);
        //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.Cyan, 0, new Vector2(.5f), 12, 0, 0);
        if (SequenceConfig.Instance.ShowGroupBox)
            Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + groupBox.GetSize() * Vector2.UnitX * .5f, groupBox.GetSize()), GroupColor * .5f, 6);

        //锚点
        //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.Cyan * .5f, 0, new Vector2(.5f), 12, 0, 0);


    }
    public static void DrawSequence(SequenceBox sequenceBox, Vector2 position, Vector2 offsetFrame, bool active, bool start)
    {
        Color SequenceColor = Color.Red;
        var pos = position;
        if (!start)
            position.X += 16;
        int counter = 0;
        position.X += SequenceConfig.Instance.Step.X * .5f;//16
        Main.spriteBatch.DrawLine(pos, position - SequenceConfig.Instance.Step.X * .25f * Vector2.UnitX, Color.White);
        foreach (var g in sequenceBox.groupBoxes)
        {
            //绘制组之间的连接线
            if (counter < sequenceBox.groupBoxes.Count - 1)
            {
                var p = position + (g.GetSize().X + SequenceConfig.Instance.Step.X * .25f) * Vector2.UnitX;// + 16
                                                                                                           //if (LogSpiralLibraryMod.ModTime % 60 < 30)
                Main.spriteBatch.DrawLine(p, p + SequenceConfig.Instance.Step.X * .5f * Vector2.UnitX, Color.White);// 1f - 32
            }
            //绘制组，添加位置偏移
            DrawGroup(g, position, active && counter == sequenceBox.sequenceBase.Counter % sequenceBox.sequenceBase.GroupBases.Count);
            if (counter < sequenceBox.groupBoxes.Count - 1)
                position.X += g.GetSize().X + SequenceConfig.Instance.Step.X;
            else
                position.X += g.GetSize().X + SequenceConfig.Instance.Step.X;

            //position.X += g.GetSize().X + offset + SequenceConfig.Instance.Step.X;


            //计数器自增
            counter++;

        }
        Main.spriteBatch.DrawLine(pos + new Vector2(sequenceBox.GetSize().X, 0), position + new Vector2(-SequenceConfig.Instance.Step.X * .75f, 0), Color.White);//32


        //锚点
        //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), SequenceColor * .5f, 0, new Vector2(.5f), 8, 0, 0);
        //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos + new Vector2(sequenceBox.GetSize().X + (start ? 32 : 0), 0), new Rectangle(0, 0, 1, 1), Color.Red * .5f, 0, new Vector2(.5f), 8, 0, 0);

        if (SequenceConfig.Instance.ShowSequenceBox)
            Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + sequenceBox.GetSize() * Vector2.UnitX * .5f + offsetFrame, sequenceBox.GetSize()), SequenceColor * .5f);//以pos为左侧中心绘制矩形框框
    }
}

using Humanizer;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.UI;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UI;
using ReLogic.Graphics;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.ComplexPanel;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UIBase;

public class WraperBox : UIElement
{
    public UIPanel panel;
    public Sequence.WraperBase wraper;
    public SequenceBox sequenceBox;
    public bool CacheRefresh;
    public bool chosen;
    public bool Dragging;
    public bool IsClone;
    public WraperBox Clone()
    {
        return new WraperBox(wraper.Clone());
    }
    public WraperBox(Sequence.WraperBase wraperBase)
    {
        wraper = wraperBase;
        if (wraper.IsSequence)
            sequenceBox = new SequenceBox(wraper.SequenceInfo);

    }
    public override void RightClick(UIMouseEvent evt)
    {
        if (!wraper.Available)
            return;

        if (IsClone)
        {
            Main.NewText(Language.GetOrRegister(SequenceUI.localizationPath + ".CantEditHere").Value);
            return;
        }
        if (evt.Target != this && !(this.BelongToMe(evt.Target) && evt.Target is not WraperBox)) return;
        //Main.NewText((GetHashCode(), evt.Target.GetHashCode()));
        var ui = SequenceSystem.instance.sequenceUI;
        var list = ui.propList;
        list.Clear();
        if (ui.currentWraper != null)
            ui.currentWraper.chosen = false;
        ui.currentWraper = this;
        chosen = true;
        SoundEngine.PlaySound(SoundID.MenuOpen);
        int top = 0;
        int order = 0;
        {
            var fieldInfo = wraper.GetType().GetField("conditionDefinition", BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo != null)
                SequenceSystem.WrapIt(list, ref top, new PropertyFieldWrapper(fieldInfo), wraper, order);
        }
        if (wraper.IsSequence)
        {
            if (!sequenceBox.Expand || sequenceBox.sequenceBase.FileName != Sequence.SequenceDefaultName)
            {
                UIButton<string> TurnToButton = new(Language.GetOrRegister(SequenceUI.localizationPath + ".SwitchToSequencePage").Value);
                TurnToButton.SetSize(new Vector2(0, 32), 0.8f, 0f);
                TurnToButton.HAlign = 0.5f;
                list.Add(TurnToButton);
                TurnToButton.OnLeftClick += (evt, elem) =>
                {
                    ui.SequenceToPage(sequenceBox);
                    ui.SwitchToSequencePage(sequenceBox);
                };
            }
        }
        else
        {
            //wraper.SetConfigPanel(list);
            var props = ConfigManager.GetFieldsAndProperties(wraper.Element);
            foreach (PropertyFieldWrapper variable in props)
            {
                if (!Attribute.IsDefined(variable.MemberInfo, typeof(ElementCustomDataAttribute)) || Attribute.IsDefined(variable.MemberInfo, typeof(ElementCustomDataAbabdonedAttribute)))
                    continue;
                if (variable.Type == typeof(SeqDelegateDefinition))
                    continue;
                var (container, elem) = SequenceSystem.WrapIt(list, ref top, variable, wraper.Element, order++);
            }
            foreach (PropertyFieldWrapper variable in props)
            {
                if (variable.Type != typeof(SeqDelegateDefinition))
                    continue;
                var (container, elem) = SequenceSystem.WrapIt(list, ref top, variable, wraper.Element, order++);
            }
        }
        base.RightClick(evt);
    }
    public override void MouseOver(UIMouseEvent evt)
    {
        //if (Dragging)
        //{
        //    Main.NewText((evt.Target.GetHashCode(),this.GetHashCode(),this.BelongToMe(evt.Target)));
        //}
        base.MouseOver(evt);
    }
    public void ParentCacheRefreshSet()
    {
        if (Parent is GroupBox gb)
        {
            gb.CacheRefresh = true;
            var sb = gb.Parent as SequenceBox;
            sb.CacheRefresh = true;
            if (sb.Parent is WraperBox wb)
            {
                wb.CacheRefresh = true;
                wb.ParentCacheRefreshSet();
            }
        }
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (SequenceSystem.instance.sequenceUI.WorkingPlacePanel.Elements[0] is not SequenceBox box)
            return;
        SoundEngine.PlaySound(SoundID.CoinPickup);
        if (IsClone)
        {
            WraperBox copy = Clone();
            if (wraper.IsSequence)
                copy.sequenceBox.Expand = false;
            copy.Dragging = true;
            copy.OnInitialize();
            SequenceSystem.instance.sequenceUI.OuterWorkingPanel.Append(copy);
            var vec = Main.MouseScreen - copy.Parent.GetDimensions().Position() - copy.GetSize() * .5f;
            copy.Top.Set(vec.X, 0);
            copy.Left.Set(vec.X, 0);
            copy.Recalculate();
            SequenceSystem.instance.userInterfaceSequence.LeftMouse.LastDown = copy;
        }
        else
        {
            if (Parent.Parent.GetHashCode() == box.GetHashCode() && box.groupBoxes.Count == 1 && box.groupBoxes.First().wraperBoxes.Count == 1)
            {
                Main.NewText(Language.GetOrRegister(SequenceUI.localizationPath + ".LeastOneElementHint"));
                return;

            }
            Dragging = true;

            if (Parent is GroupBox group)
            {
                group.wraperBoxes.Remove(this);
                Remove();
                group.CacheRefresh = true;

                SequenceBox sBox = group.Parent as SequenceBox;
                sBox.sequenceBase.Remove(wraper, group.group);
                sBox.CacheRefresh = true;
                if (group.wraperBoxes.Count == 0)
                {
                    sBox.groupBoxes.Remove(group);
                    group.Remove();


                }
                if (sBox.Parent is WraperBox wb)
                {
                    wb.CacheRefresh = true;
                    wb.WrapperSize();
                    wb.ParentCacheRefreshSet();
                }
                SequenceBox mainBox = SequenceSystem.instance.sequenceUI.WorkingPlacePanel.Elements[0] as SequenceBox;
                mainBox.Elements.Clear();
                mainBox.CacheRefresh = true;
                mainBox.OnInitialize();
                mainBox.Recalculate();
            }
            Remove();
            SequenceSystem.instance.sequenceUI.OuterWorkingPanel.Append(this);
        }
        base.LeftMouseDown(evt);
    }
    public override void LeftMouseUp(UIMouseEvent evt)
    {
        if (!Dragging) return;
        if (!this.BelongToMe(evt.Target))
            return;
        SequenceUI sequenceUI = SequenceSystem.instance.sequenceUI;
        if (sequenceUI.WorkingPlacePanel.Elements[0] is not SequenceBox box)
        {
            return;
        }
        sequenceUI.OuterWorkingPanel.RemoveChild(this);
        box.InsertWraper(this, evt.MousePosition);
        sequenceUI.PendingModify = true;


        Dragging = false;
        base.LeftMouseUp(evt);
    }
    public override void Update(GameTime gameTime)
    {
        if (Dragging)
        {
            var vec = Main.MouseScreen - Parent.GetDimensions().Position() - this.GetSize() * .5f;
            Left.Set(vec.X, 0f);
            Top.Set(vec.Y, 0f);
            Recalculate();
        }
        MaxWidth = MaxHeight = new StyleDimension(223214514, 0);
        base.Update(gameTime);
    }
    public override void OnInitialize()
    {
        Vector2 size = this.GetSize();
        panel = new LogSpiralLibraryPanel();
        panel.SetSize(size);
        if (wraper.IsSequence && sequenceBox.Expand && wraper.SequenceInfo.FileName == Sequence.SequenceDefaultName)
        {
            var desc = wraper.conditionDefinition;
            if (wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey)
            {
                sequenceBox.offY = FontAssets.MouseText.Value.MeasureString("→" + desc.DisplayName).Y * -.5f;
            }
            //sequenceBox.Left.Set(SequenceConfig.Instance.Step.X * .5f, 0);
            sequenceBox.OnInitialize();
            Append(sequenceBox);
        }
        base.OnInitialize();
    }
    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        //Vector4 vec = new Vector4(0,0, 0, 1.5f + MathF.Cos((float)LogSpiralLibrarySystem.ModTime / 60) * .5f);// MathF.Cos((float)LogSpiralLibrarySystem.ModTime / 60) * 60
        //object m = spriteBatch.GetType().GetField("transformMatrix", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(spriteBatch);
        //spriteBatch.GetType().GetField("transformMatrix", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(spriteBatch, Main.UIScaleMatrix with { /*M14 = vec.X,*/ M24 = vec.Y, M34 = vec.Z, M44 = vec.W });
        if (SequenceConfig.Instance.ShowWrapBox)
            spriteBatch.DrawRectangle(GetDimensions().ToRectangle(), Color.MediumPurple, 12);
        var desc = wraper.conditionDefinition.DisplayName;
        bool flag = wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey;
        var wraperBox = this;
        var position = GetDimensions().Position() + new Vector2(0, GetDimensions().Height * .5f);
        if (wraperBox.wraper.IsSequence && wraperBox.sequenceBox.Expand && wraperBox.wraper.SequenceInfo.FileName == Sequence.SequenceDefaultName)
        {
            ComplexPanelInfo panel = new();
            var boxSize = wraperBox.WrapperSize();
            //if (flag)
            //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f + new Vector2(0, 16), boxSize + new Vector2(32, 64));
            //else
            //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f, boxSize + new Vector2(32, 32));
            float offY = flag ? FontAssets.MouseText.Value.MeasureString(desc).Y : 0;
            panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X * .5f, 0), boxSize);
            panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
            panel.glowEffectColor = chosen ? Main.DiscoColor with { A = 0 } : Color.MediumPurple with { A = 102 };
            panel.glowShakingStrength = .1f;
            panel.glowHueOffsetRange = 0.1f;
            panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
            panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
            panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
            panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
            panel.DrawComplexPanel(spriteBatch);
            //DrawSequence(wraperBox.sequenceBox, position - offY * .5f * Vector2.UnitY, offY * .5f * Vector2.UnitY, active, false);
            if (flag)
            {
                var cen = position + boxSize * Vector2.UnitY * .5f - offY * 1.5f * Vector2.UnitY + new Vector2(16, 0);
                spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen, Color.White);
            }
        }
        else
        {
            //position.Y += wraperBox.GetSize().Y * .5f;
            var font = FontAssets.MouseText.Value;
            var name = wraperBox.wraper.Name;
            if (wraper.IsSequence && SequenceSystem.sequenceInfos.TryGetValue(wraper.SequenceInfo.KeyName, out var value))
                name = value.DisplayName;
            Vector2 textSize = font.MeasureString(name);
            Vector2 boxSize = wraperBox.WrapperSize();
            #region 框框
            ComplexPanelInfo panel = new();
            panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X, 0) * .5f, boxSize);
            panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
            if (!wraper.Available)
                panel.glowEffectColor = Color.Gray;
            else if (wraper.IsSequence)
                panel.glowEffectColor = chosen ? Color.Lerp(Color.Blue, Color.Purple, 0.5f) with { A = 0 } : Color.MediumPurple with { A = 102 };
            else
                panel.glowEffectColor = (chosen ? Color.Red : Color.Cyan) with { A = 0 };
            panel.glowShakingStrength = .05f;
            panel.glowHueOffsetRange = 0.05f;
            panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
            panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
            panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
            panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
            panel.DrawComplexPanel(spriteBatch);
            #endregion
            var cen = position + new Vector2(16) - boxSize * Vector2.UnitY * .5f;
            spriteBatch.DrawString(font, name, cen, Color.White, 0, default, 1f, 0, 0);
            cen += textSize * Vector2.UnitY;
            if (flag)
            {
                spriteBatch.DrawString(font, "→" + desc, cen, Color.White);
            }
            //spriteBatch.DrawRectangle(panel.destination, Color.MediumPurple);
            if (chosen)
            {
                var tarVec = SequenceSystem.instance.sequenceUI.propList.GetDimensions().ToRectangle().TopRight();
                spriteBatch.DrawHorizonBLine(tarVec, position, Color.White with { A = 0 } * .125f);
                spriteBatch.DrawHorizonBLine(tarVec + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4), position + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4), Main.DiscoColor with { A = 0 } * .125f, 1, 6);

            }
        }

        base.DrawSelf(spriteBatch);
    }
}

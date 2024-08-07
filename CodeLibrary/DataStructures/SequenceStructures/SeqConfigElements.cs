﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameInput;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Config;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;
using ReLogic.Graphics;

//螺线懒，他用UIConfig的WrapIt来生成UI控件方便调参
//螺线勤奋，他复制并微调了TML自带的几个控件，以更适合序列界面的编辑((
//螺线最坏乐哈哈哈哈
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{
    public class DateTimeElement : ConfigElement<DateTime>
    {
        public override void OnBind()
        {
            //UIText text = new UIText(Value.ToString());
            base.OnBind();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {

            //Main.NewText(Value.ToString() + "2333");
            base.Draw(spriteBatch);

            var font = FontAssets.MouseText.Value;
            string content = Value.ToString();
            Vector2 size = font.MeasureString(content);
            Rectangle rect = GetDimensions().ToRectangle();
            spriteBatch.DrawString(font, Value.ToString(), rect.TopRight() - Vector2.UnitX * size - new Vector2(4, (size.Y - rect.Height) * .5f), Color.White);
            //spriteBatch.DrawRectangle(GetDimensions().ToRectangle(), Main.DiscoColor);
        }
    }
    public abstract class SeqConfigElement : ConfigElement
    {
        public override void SetObject(object value)
        {
            if (List != null)
            {
                List[Index] = value;
                SequenceSystem.SetSequenceUIPending();
                return;
            }

            if (!MemberInfo.CanWrite)
                return;

            MemberInfo.SetValue(Item, value);
            SequenceSystem.SetSequenceUIPending();
        }
    }
    public abstract class SeqConfigElement<T> : SeqConfigElement
    {
        protected virtual T Value
        {
            get => (T)GetObject();
            set => SetObject(value);
        }
    }
    public class SeqIntInputElement : SeqConfigElement
    {
        public IList<int> IntList { get; set; }
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 100;
        public int Increment { get; set; } = 1;

        public override void OnBind()
        {
            base.OnBind();

            IntList = (IList<int>)List;

            if (IntList != null)
            {
                TextDisplayFunction = () => Index + 1 + ": " + IntList[Index];
            }

            if (RangeAttribute != null && RangeAttribute.Min is int && RangeAttribute.Max is int)
            {
                Min = (int)RangeAttribute.Min;
                Max = (int)RangeAttribute.Max;
            }
            if (IncrementAttribute != null && IncrementAttribute.Increment is int)
            {
                Increment = (int)IncrementAttribute.Increment;
            }

            UIPanel textBoxBackground = new UIPanel();
            textBoxBackground.SetPadding(0);
            UIFocusInputTextField uIInputTextField = new UIFocusInputTextField("Type here");
            textBoxBackground.Top.Set(0f, 0f);
            textBoxBackground.Left.Set(-130, 1f);
            textBoxBackground.Width.Set(120, 0f);
            textBoxBackground.Height.Set(30, 0f);
            Append(textBoxBackground);

            uIInputTextField.SetText(GetValue().ToString());
            uIInputTextField.Top.Set(5, 0f);
            uIInputTextField.Left.Set(10, 0f);
            uIInputTextField.Width.Set(-42, 1f); // allow space for arrows
            uIInputTextField.Height.Set(20, 0);
            uIInputTextField.OnTextChange += (a, b) =>
            {
                if (int.TryParse(uIInputTextField.CurrentString, out int val))
                {
                    SetValue(val);
                }
                //else /{
                //	Interface.modConfig.SetMessage($"{uIInputTextField.currentString} isn't a valid value.", Color.Green);
                //}
            };
            uIInputTextField.OnUnfocus += (a, b) => uIInputTextField.SetText(GetValue().ToString());
            textBoxBackground.Append(uIInputTextField);

            UIModConfigHoverImageSplit upDownButton = new UIModConfigHoverImageSplit(UpDownTexture, "+" + Increment, "-" + Increment);
            upDownButton.Recalculate();
            upDownButton.Top.Set(4f, 0f);
            upDownButton.Left.Set(-30, 1f);
            upDownButton.OnLeftClick += (a, b) =>
            {
                Rectangle r = b.GetDimensions().ToRectangle();
                if (a.MousePosition.Y < r.Y + r.Height / 2)
                {
                    SetValue(Utils.Clamp(GetValue() + Increment, Min, Max));
                }
                else
                {
                    SetValue(Utils.Clamp(GetValue() - Increment, Min, Max));
                }
                uIInputTextField.SetText(GetValue().ToString());
            };
            textBoxBackground.Append(upDownButton);
            Recalculate();
        }

        protected virtual int GetValue() => (int)GetObject();

        protected virtual void SetValue(int value) => SetObject(Utils.Clamp(value, Min, Max));
    }
    public class SeqStringInputElement : SeqConfigElement<string>
    {
        class SeqUIFocusInputTextField : UIElement
        {
            public delegate void EventHandler(object sender, EventArgs e);

            private bool _isWrapped;

            public void HeightCheck()
            {
                string text = CurrentString;
                float w = GetInnerDimensions().Width;
                if (IsWrapped)
                    text = FontAssets.MouseText.Value.CreateWrappedText(text, GetInnerDimensions().Width);
                Vector2 vector = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, new Vector2(1));
                float WrappedTextBottomPadding = 0f;
                Vector2 vector2 = IsWrapped ? new Vector2(vector.X, vector.Y + WrappedTextBottomPadding) : new Vector2(vector.X, 16);
                if (!IsWrapped)
                { // TML: IsWrapped when true should prevent changing MinWidth, otherwise can't shrink in width due to logic.
                    MinWidth.Set(vector2.X + PaddingLeft + PaddingRight, 0f);
                }
                MinHeight.Set(vector2.Y + PaddingTop + PaddingBottom, 0f);
                Recalculate();
            }

            public bool IsWrapped
            {
                get
                {
                    return _isWrapped;
                }
                set
                {
                    _isWrapped = value;
                    if (value)
                        MinWidth.Set(0, 0); // TML: IsWrapped when true should prevent changing MinWidth, otherwise can't shrink in width due to CreateWrappedText+GetInnerDimensions logic. IsWrapped is false in ctor, so need to undo changes.

                    HeightCheck();


                }
            }

            public bool Focused;

            public string CurrentString = "";

            public readonly string _hintText;

            public int _textBlinkerCount;

            public int _textBlinkerState;

            public bool UnfocusOnTab
            {
                get;
                set;
            }

            public event EventHandler OnTextChange;

            public event EventHandler OnUnfocus;

            public event EventHandler OnTab;

            public SeqUIFocusInputTextField(string hintText)
            {
                _hintText = hintText;
            }

            public void SetText(string text)
            {
                if (text == null)
                {
                    text = "";
                }

                if (CurrentString != text)
                {
                    CurrentString = text;
                    HeightCheck();
                    this.OnTextChange?.Invoke(this, new EventArgs());
                }
            }

            public override void LeftClick(UIMouseEvent evt)
            {
                Main.clrInput();
                Focused = true;
            }

            public override void Update(GameTime gameTime)
            {
                Vector2 point = new Vector2(Main.mouseX, Main.mouseY);
                if (!ContainsPoint(point) && Main.mouseLeft)
                {
                    Focused = false;
                    this.OnUnfocus?.Invoke(this, new EventArgs());
                }

                base.Update(gameTime);
            }

            public static bool JustPressed(Keys key)
            {
                if (Main.inputText.IsKeyDown(key))
                {
                    return !Main.oldInputText.IsKeyDown(key);
                }

                return false;
            }

            public override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (Focused)
                {
                    PlayerInput.WritingText = true;
                    Main.instance.HandleIME();
                    string inputText = Main.GetInputText(CurrentString);
                    if (Main.inputTextEscape)
                    {
                        Main.inputTextEscape = false;
                        Focused = false;
                        this.OnUnfocus?.Invoke(this, new EventArgs());
                    }

                    if (!inputText.Equals(CurrentString))
                    {
                        CurrentString = inputText;
                        HeightCheck();
                        this.OnTextChange?.Invoke(this, new EventArgs());
                    }
                    else
                    {
                        CurrentString = inputText;
                    }

                    if (JustPressed(Keys.Tab))
                    {
                        if (UnfocusOnTab)
                        {
                            Focused = false;
                            this.OnUnfocus?.Invoke(this, new EventArgs());
                        }

                        this.OnTab?.Invoke(this, new EventArgs());
                    }

                    if (++_textBlinkerCount >= 20)
                    {
                        _textBlinkerState = (_textBlinkerState + 1) % 2;
                        _textBlinkerCount = 0;
                    }
                }

                string text = CurrentString;
                if (_textBlinkerState == 1 && Focused)
                {
                    text += "|";
                }
                if (IsWrapped)
                    text = FontAssets.MouseText.Value.CreateWrappedText(text, GetInnerDimensions().Width);
                CalculatedStyle dimensions = GetDimensions();
                if (CurrentString.Length == 0 && !Focused)
                {
                    Utils.DrawBorderString(spriteBatch, _hintText, new Vector2(dimensions.X, dimensions.Y), Color.Gray);
                }
                else
                {
                    Utils.DrawBorderString(spriteBatch, text, new Vector2(dimensions.X, dimensions.Y), Color.White);
                }
            }
        }
        void HeightCheck()
        {
            Value = uIInputTextField.CurrentString;
            uIInputTextField.HeightCheck();
            MinHeight = textBoxBackground.MinHeight = uIInputTextField.MinHeight with { Pixels = uIInputTextField.MinHeight.Pixels + 10 };
            if (Parent?.Parent?.Parent is UIList list)
            {
                Parent.MinHeight = MinHeight;
                list.Recalculate();
            }
            else
                Recalculate();
            SequenceSystem.SetSequenceUIPending();

        }
        UIPanel textBoxBackground;
        SeqUIFocusInputTextField uIInputTextField;
        bool resetted;
        public override void OnBind()
        {
            base.OnBind();

            textBoxBackground = new UIPanel();
            textBoxBackground.SetPadding(0);
            uIInputTextField = new SeqUIFocusInputTextField(Language.GetTextValue($"Mods.LogSpiralLibrary.Configs.StringInputHint"));
            textBoxBackground.Top.Set(0f, 0f);
            textBoxBackground.Left.Set(-190, 1f);
            textBoxBackground.Width.Set(180, 0f);
            textBoxBackground.Height.Set(90, 0f);

            Append(textBoxBackground);
            uIInputTextField.Top.Set(5, 0f);
            uIInputTextField.Left.Set(10, 0f);
            uIInputTextField.Width.Set(-20, 1f);
            uIInputTextField.Height.Set(80, 0);
            uIInputTextField.IsWrapped = true;
            uIInputTextField.SetText(Value);
            uIInputTextField.OnTextChange += (a, b) => HeightCheck();
            textBoxBackground.Append(uIInputTextField);

            resetted = false;
        }
        public override void Update(GameTime gameTime)
        {
            if (!resetted)
            {
                resetted = true;
                HeightCheck();
                SequenceSystem.SetSequenceUIPending(false);
            }
            base.Update(gameTime);
        }
    }
    public abstract class SeqRangeElement : SeqConfigElement
    {
        private static SeqRangeElement rightLock;
        private static SeqRangeElement rightHover;

        protected Color SliderColor { get; set; } = Color.White;
        protected Utils.ColorLerpMethod ColorMethod { get; set; }

        internal bool DrawTicks { get; set; }

        public abstract int NumberTicks { get; }
        public abstract float TickIncrement { get; }

        protected abstract float Proportion { get; set; }

        public SeqRangeElement()
        {
            ColorMethod = new Utils.ColorLerpMethod((percent) => Color.Lerp(Color.Black, SliderColor, percent));
        }

        public override void OnBind()
        {
            base.OnBind();

            DrawTicks = Attribute.IsDefined(MemberInfo.MemberInfo, typeof(DrawTicksAttribute));
            SliderColor = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SliderColorAttribute>(MemberInfo, Item, List)?.Color ?? Color.White;
        }

        public float DrawValueBar(SpriteBatch sb, float scale, float perc, int lockState = 0, Utils.ColorLerpMethod colorMethod = null)
        {
            perc = Utils.Clamp(perc, -.05f, 1.05f);

            if (colorMethod == null)
                colorMethod = new Utils.ColorLerpMethod(Utils.ColorLerp_BlackToWhite);

            Texture2D colorBarTexture = TextureAssets.ColorBar.Value;
            Vector2 vector = new Vector2((float)colorBarTexture.Width - 60, (float)colorBarTexture.Height) * scale;
            IngameOptions.valuePosition.X -= (float)((int)vector.X);
            Rectangle rectangle = new Rectangle((int)IngameOptions.valuePosition.X, (int)IngameOptions.valuePosition.Y - (int)vector.Y / 2, (int)vector.X, (int)vector.Y);
            Rectangle destinationRectangle = rectangle;
            int num = 107;
            float num2 = rectangle.X + 5f * scale;
            float num3 = rectangle.Y + 4f * scale;

            if (DrawTicks)
            {
                int numTicks = NumberTicks;
                if (numTicks > 1)
                {
                    for (int tick = 0; tick < numTicks; tick++)
                    {
                        float percent = tick * TickIncrement;

                        if (percent <= 1f)
                            sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(num2 + num * percent * scale), rectangle.Y - 2, 2, rectangle.Height + 4), Color.White);
                    }
                }
            }

            sb.Draw(colorBarTexture, rectangle, Color.White);

            for (float num4 = 0f; num4 < (float)num; num4 += 1f)
            {
                float percent = num4 / (float)num;
                sb.Draw(TextureAssets.ColorBlip.Value, new Vector2(num2 + num4 * scale, num3), null, colorMethod(percent), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            rectangle.Inflate((int)(-5f * scale), 2);

            //rectangle.X = (int)num2;
            //rectangle.Y = (int)num3;

            bool flag = rectangle.Contains(new Point(Main.mouseX, Main.mouseY));

            if (lockState == 2)
            {
                flag = false;
            }

            if (flag || lockState == 1)
            {
                sb.Draw(TextureAssets.ColorHighlight.Value, destinationRectangle, Main.OurFavoriteColor);
            }

            var colorSlider = TextureAssets.ColorSlider.Value;

            sb.Draw(colorSlider, new Vector2(num2 + num * scale * perc, num3 + 4f * scale), null, Color.White, 0f, colorSlider.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            if (Main.mouseX >= rectangle.X && Main.mouseX <= rectangle.X + rectangle.Width)
            {
                IngameOptions.inBar = flag;
                return (Main.mouseX - rectangle.X) / (float)rectangle.Width;
            }

            IngameOptions.inBar = false;

            if (rectangle.X >= Main.mouseX)
            {
                return 0f;
            }

            return 1f;
        }

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            float num = 6f;
            int num2 = 0;

            rightHover = null;

            if (!Main.mouseLeft)
            {
                rightLock = null;
            }

            if (rightLock == this)
            {
                num2 = 1;
            }
            else if (rightLock != null)
            {
                num2 = 2;
            }

            CalculatedStyle dimensions = GetDimensions();
            float num3 = dimensions.Width + 1f;
            Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
            bool flag2 = IsMouseHovering;

            if (num2 == 1)
            {
                flag2 = true;
            }

            if (num2 == 2)
            {
                flag2 = false;
            }

            Vector2 vector2 = vector;
            vector2.X += 8f;
            vector2.Y += 2f + num;
            vector2.X -= 17f;
            //TextureAssets.ColorBar.Value.Frame(1, 1, 0, 0);
            vector2 = new Vector2(dimensions.X + dimensions.Width - 10f, dimensions.Y + 10f + num);
            IngameOptions.valuePosition = vector2;
            float obj = DrawValueBar(spriteBatch, 1f, Proportion, num2, ColorMethod);

            if (IngameOptions.inBar || rightLock == this)
            {
                rightHover = this;
                if (PlayerInput.Triggers.Current.MouseLeft && rightLock == this)
                {
                    Proportion = obj;
                }
            }

            if (rightHover != null && rightLock == null && PlayerInput.Triggers.JustPressed.MouseLeft)
            {
                rightLock = rightHover;
            }
        }
    }
    public abstract class SeqPrimitiveRangeElement<T> : SeqRangeElement where T : IComparable<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }
        public T Increment { get; set; }
        public IList<T> TList { get; set; }

        public override void OnBind()
        {
            base.OnBind();

            TList = (IList<T>)List;
            TextDisplayFunction = () => MemberInfo.Name + ": " + GetValue();

            if (TList != null)
            {
                TextDisplayFunction = () => Index + 1 + ": " + TList[Index];
            }

            if (Label != null)
            { // Problem with Lists using ModConfig Label.
                TextDisplayFunction = () => Label + ": " + GetValue();
            }

            if (RangeAttribute != null && RangeAttribute.Min is T && RangeAttribute.Max is T)
            {
                Min = (T)RangeAttribute.Min;
                Max = (T)RangeAttribute.Max;
            }
            if (IncrementAttribute != null && IncrementAttribute.Increment is T)
            {
                Increment = (T)IncrementAttribute.Increment;
            }
        }

        protected virtual T GetValue() => (T)GetObject();

        protected virtual void SetValue(object value)
        {
            if (value is T t)
                SetObject(Utils.Clamp(t, Min, Max));
        }
    }
    public class SeqFloatElement : SeqPrimitiveRangeElement<float>
    {
        public override int NumberTicks => (int)((Max - Min) / Increment) + 1;
        public override float TickIncrement => (Increment) / (Max - Min);

        protected override float Proportion
        {
            get => (GetValue() - Min) / (Max - Min);
            set => SetValue((float)Math.Round((value * (Max - Min) + Min) * (1 / Increment)) * Increment);
        }

        public SeqFloatElement()
        {
            Min = 0;
            Max = 1;
            Increment = 0.01f;
        }
    }
    public class ActionModifyDataElement : SeqConfigElement
    {
        class DataObject
        {
            private readonly PropertyFieldWrapper memberInfo;
            private readonly object item;
            private readonly IList<ActionModifyData> array;
            private readonly int index;

            private ActionModifyData current;

            //[LabelKey("缩放系数")]
            const string key = "$Mods.LogSpiralLibrary.Configs.ActionModifyData.";
            [LabelKey($"{key}sizeScaler.Label")]
            [CustomModConfigItem(typeof(SeqFloatElement))]
            public float actionOffsetSize
            {
                get => current.actionOffsetSize;
                set
                {
                    current.actionOffsetSize = value;
                    Update();
                }
            }

            //[LabelKey("时长系数")]
            [LabelKey($"{key}timeScaler.Label")]
            [CustomModConfigItem(typeof(SeqFloatElement))]
            public float actionOffsetTimeScaler
            {
                get => current.actionOffsetTimeScaler;
                set
                {
                    current.actionOffsetTimeScaler = value;
                    Update();
                }
            }

            //[LabelKey("击退系数")]
            [LabelKey($"{key}knockBack.Label")]
            [CustomModConfigItem(typeof(SeqFloatElement))]
            public float actionOffsetKnockBack
            {
                get => current.actionOffsetKnockBack;
                set
                {
                    current.actionOffsetKnockBack = value;
                    Update();
                }
            }
            //[LabelKey("伤害系数")]
            [LabelKey($"{key}damage.Label")]
            [CustomModConfigItem(typeof(SeqFloatElement))]
            public float actionOffsetDamage
            {
                get => current.actionOffsetDamage;
                set
                {
                    current.actionOffsetDamage = value;
                    Update();
                }
            }
            //[LabelKey("暴击率增益")]
            [LabelKey($"{key}critAdd.Label")]
            [CustomModConfigItem(typeof(SeqIntInputElement))]
            public int actionOffsetCritAdder
            {
                get => current.actionOffsetCritAdder;
                set
                {
                    current.actionOffsetCritAdder = value;
                    Update();
                }
            }
            //[LabelKey("暴击率系数")]
            [LabelKey($"{key}critMul.Label")]
            [CustomModConfigItem(typeof(SeqFloatElement))]
            public float actionOffsetCritMultiplyer
            {
                get => current.actionOffsetCritMultiplyer;
                set
                {
                    current.actionOffsetCritMultiplyer = value;
                    Update();
                }
            }

            private void Update()
            {
                if (array == null)
                    memberInfo.SetValue(item, current);
                else
                    array[index] = current;

                Interface.modConfig.SetPendingChanges();
            }
            public DataObject(PropertyFieldWrapper memberInfo, object item)
            {
                this.item = item;
                this.memberInfo = memberInfo;
                current = (ActionModifyData)memberInfo.GetValue(item);
            }

            public DataObject(IList<ActionModifyData> array, int index)
            {
                current = array[index];
                this.array = array;
                this.index = index;
            }
        }
        private int height;
        private DataObject c;
        private float min = 0;
        private float max = 10;
        private float increment = 0.01f;

        public IList<ActionModifyData> DataList { get; set; }

        public override void OnBind()
        {
            base.OnBind();

            DataList = (IList<ActionModifyData>)List;

            if (DataList != null)
            {
                DrawLabel = false;
                height = 30;
                c = new DataObject(DataList, Index);
            }
            else
            {
                height = 30;
                c = new DataObject(MemberInfo, Item);
            }

            if (RangeAttribute != null && RangeAttribute.Min is float && RangeAttribute.Max is float)
            {
                max = (float)RangeAttribute.Max;
                min = (float)RangeAttribute.Min;
            }

            if (IncrementAttribute != null && IncrementAttribute.Increment is float)
            {
                increment = (float)IncrementAttribute.Increment;
            }

            int order = 0;
            foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(c))
            {
                var wrapped = UIModConfig.WrapIt(this, ref height, variable, c, order++);

                // Can X and Y inherit range and increment automatically? Pass in "fake PropertyFieldWrapper" to achieve? Real one desired for label.
                if (wrapped.Item2 is FloatElement floatElement)
                {
                    floatElement.Min = min;
                    floatElement.Max = max;
                    floatElement.Increment = increment;
                    floatElement.DrawTicks = Attribute.IsDefined(MemberInfo.MemberInfo, typeof(DrawTicksAttribute));
                }

                if (DataList != null)
                {
                    wrapped.Item1.Left.Pixels -= 20;
                    wrapped.Item1.Width.Pixels += 20;
                }
            }


            #region 华生你发现了盲点
            //string path = "假装我写了路径参数";
            //Type elemType = typeof(MeleeAction);//我知道你想吐槽这个typeof，但是好像没有"类型参数数组"这种东西，实际用的时候是从一个Type链表里面取出
            //Type seqType = typeof(SequenceBase<>);
            //seqType = seqType.MakeGenericType(elemType);
            //var method = seqType.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, [typeof(string)]);
            //SequenceBase sequenceBase = (SequenceBase)method.Invoke(null, [path]);
            //SequenceBase<elemType> sequenceBase = SequenceBase<elemType>.Load(path);
            #endregion

        }
        // Draw axis? ticks?
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //CalculatedStyle dimensions = base.GetInnerDimensions();
            //Rectangle rectangle = dimensions.ToRectangle();
            //rectangle = new Rectangle(rectangle.Right - 30, rectangle.Y, 30, 30);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.AliceBlue);

            //float x = (c.X - min) / (max - min);
            //float y = (c.Y - min) / (max - min);

            //var position = rectangle.TopLeft();
            //position.X += x * rectangle.Width;
            //position.Y += y * rectangle.Height;
            //var blipRectangle = new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4);

            //if (x >= 0 && x <= 1 && y >= 0 && y <= 1)
            //    spriteBatch.Draw(TextureAssets.MagicPixel.Value, blipRectangle, Color.Black);

            //if (IsMouseHovering && rectangle.Contains((Main.MouseScreen).ToPoint()) && Main.mouseLeft)
            //{
            //    float newPerc = (Main.mouseX - rectangle.X) / (float)rectangle.Width;
            //    newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
            //    c.X = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;

            //    newPerc = (Main.mouseY - rectangle.Y) / (float)rectangle.Height;
            //    newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
            //    c.Y = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;
            //}
        }

        internal float GetHeight()
        {
            return height;
        }
    }
}

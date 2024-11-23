//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.GameInput;
//using Terraria.ModLoader.Config.UI;
//using Terraria.ModLoader.Config;
//using Terraria.UI;
//using Microsoft.Xna.Framework.Input;
//using Terraria.GameContent.UI.Elements;
//using Terraria.Localization;
//using Terraria.ModLoader.UI;
//using Terraria.UI.Chat;
//using ReLogic.Graphics;
//using ReLogic.Content;
//using System.Collections;
//using System.Reflection;
//using LogSpiralLibrary.CodeLibrary.UIGenericConfig;

////螺线懒，他用UIConfig的WrapIt来生成UI控件方便调参
////螺线勤奋，他复制并微调了TML自带的几个控件，以更适合序列界面的编辑((
////螺线最坏乐哈哈哈哈
//namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
//{
//    public abstract class SeqConfigElement<T> : SeqConfigElement
//    {
//        public virtual T Value
//        {
//            get => (T)GetObject();
//            set => SetObject(value);
//        }
//    }
//    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
//    public class CustomSeqConfigItemAttribute : Attribute
//    {
//        public Type Type { get; }

//        public CustomSeqConfigItemAttribute(Type type)
//        {
//            Type = type;
//        }
//    }
//    public abstract class SeqConfigElement : ConfigElement
//    {
//        public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1)
//            => GenericConfigElement.WrapIt(parent, ref top, memberInfo, item, order, list, arrayType, index, (configElem, flag) => SequenceSystem.SetSequenceUIPending(flag));
//        /*
//        public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1)
//        {
//            //TODO 添加对更对类型的支持 尽管我觉着应该不会设计出要那么复杂参数的元素吧(?
//            int elementHeight;
//            Type type = memberInfo.Type;

//            if (arrayType != null)
//            {
//                type = arrayType;
//            }

//            UIElement e;

//            // TODO: Other common structs? -- Rectangle, Point
//            var customUI = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomSeqConfigItemAttribute>(memberInfo, null, null);//是否对该成员有另外实现的UI支持

//            if (customUI != null)
//            {
//                Type customUIType = customUI.Type;

//                if (typeof(SeqConfigElement).IsAssignableFrom(customUIType))
//                {
//                    ConstructorInfo ctor = customUIType.GetConstructor(Array.Empty<Type>());

//                    if (ctor != null)
//                    {
//                        object instance = ctor.Invoke(new object[0]);//执行相应UI的构造函数
//                        e = instance as UIElement;
//                    }
//                    else
//                    {
//                        e = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not have an empty constructor.");
//                    }
//                }
//                else
//                {
//                    e = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not inherit from ConfigElement.");
//                }
//            }
//            else if (item.GetType() == typeof(HeaderAttribute))
//            {
//                e = new HeaderElement((string)memberInfo.GetValue(item));
//            }
//            //基于成员类型添加上默认的编辑UI
//            //else if (type == typeof(ItemDefinition))
//            //{
//            //    e = new ItemDefinitionElement();
//            //}
//            //else if (type == typeof(ProjectileDefinition))
//            //{
//            //    e = new ProjectileDefinitionElement();
//            //}
//            //else if (type == typeof(NPCDefinition))
//            //{
//            //    e = new NPCDefinitionElement();
//            //}
//            //else if (type == typeof(PrefixDefinition))
//            //{
//            //    e = new PrefixDefinitionElement();
//            //}
//            //else if (type == typeof(BuffDefinition))
//            //{
//            //    e = new BuffDefinitionElement();
//            //}
//            //else if (type == typeof(TileDefinition))
//            //{
//            //    e = new TileDefinitionElement();
//            //}
//            //else if (type == typeof(Color))
//            //{
//            //    e = new ColorElement();
//            //}
//            //else if (type == typeof(Vector2))
//            //{
//            //    e = new Vector2Element();
//            //}
//            else if (type == typeof(bool)) // isassignedfrom?
//            {
//                e = new SeqBooleanElement();
//            }
//            else if (type == typeof(float))
//            {
//                e = new SeqFloatElement();
//            }
//            else if (type == typeof(DateTime))
//            {
//                e = new SeqDateTimeElement();
//            }
//            else if (type == typeof(ActionModifyData))
//            {
//                e = new SeqActionModifyDataElement();
//            }
//            else if (type == typeof(SeqDelegateDefinition))
//            {
//                e = new SeqDelegateDefinitionElement();
//            }
//            //else if (type == typeof(byte))
//            //{
//            //    e = new ByteElement();
//            //}
//            //else if (type == typeof(uint))
//            //{
//            //    e = new UIntElement();
//            //}
//            else if (type == typeof(int))
//            {
//                SliderAttribute sliderAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SliderAttribute>(memberInfo, item, list);

//                //if (sliderAttribute != null)
//                //    e = new IntRangeElement();
//                //else
//                e = new SeqIntInputElement();
//            }
//            else if (type == typeof(string))
//            {
//                OptionStringsAttribute ost = ConfigManager.GetCustomAttributeFromMemberThenMemberType<OptionStringsAttribute>(memberInfo, item, list);
//                //if (ost != null)
//                //    e = new StringOptionElement();
//                //else
//                e = new SeqStringInputElement();
//            }
//            else if (type.IsEnum)
//            {
//                if (list != null)
//                    e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}).");
//                else
//                    e = new SeqEnumElement();
//            }
//            //else if (type.IsArray)
//            //{
//            //    e = new ArrayElement();
//            //}
//            //else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
//            //{
//            //    e = new ListElement();
//            //}
//            //else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
//            //{
//            //    e = new SetElement();
//            //}
//            //else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
//            //{
//            //    e = new DictionaryElement();
//            //}
//            //else if (type.IsClass)
//            //{
//            //    e = new ObjectElement(/*, ignoreSeparatePage: ignoreSeparatePage);
//            //}
//            else if (type.IsValueType && !type.IsPrimitive)
//            {
//                e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}) Structs need special UI.");
//        //e.Top.Pixels += 6;
//        e.Height.Pixels += 6;
//                e.Left.Pixels += 4;

//                //object subitem = memberInfo.GetValue(item);
//            }
//            else
//            {
//                e = new UIText($"{memberInfo.Name} not handled yet ({type.Name})");
//    e.Top.Pixels += 6;
//                e.Left.Pixels += 4;
//            }

//if (e != null)
//{
//    if (e is SeqConfigElement configElement)
//    {
//        configElement.Bind(memberInfo, item, (IList)list, index);//将UI控件与成员信息及实例绑定
//        configElement.OnBind();
//    }

//    e.Recalculate();

//    elementHeight = (int)e.GetOuterDimensions().Height;

//    var container = UIModConfig.GetContainer(e, index == -1 ? order : index);
//    container.Height.Pixels = elementHeight;

//    if (parent is UIList uiList)
//    {
//        uiList.Add(container);
//        uiList.GetTotalHeight();
//    }
//    else
//    {
//        // Only Vector2 and Color use this I think, but modders can use the non-UIList approach for custom UI and layout.
//        container.Top.Pixels = top;
//        container.Width.Pixels = -20;
//        container.Left.Pixels = 20;
//        top += elementHeight + 4;
//        parent.Append(container);
//        parent.Height.Set(top, 0);
//    }

//    var tuple = new Tuple<UIElement, UIElement>(container, e);

//    //if (parent == Interface.modConfig.mainConfigList)
//    //{
//    //    Interface.modConfig.mainConfigItems.Add(tuple);
//    //}

//    return tuple;
//}
//return null;
//        }
//        */
//        public SeqConfigElement() : base()
//        {
//        }
//        public override void SetObject(object value)
//        {
//            if (List != null)
//            {
//                List[Index] = value;
//                SequenceSystem.SetSequenceUIPending();
//                return;
//            }

//            if (!MemberInfo.CanWrite)
//                return;

//            MemberInfo.SetValue(Item, value);
//            SequenceSystem.SetSequenceUIPending();
//        }
//    }
//    public class SeqBooleanElement : SeqConfigElement<bool>
//    {
//        private Asset<Texture2D> _toggleTexture;

//        // TODO. Display status string? (right now only on/off texture, but True/False, Yes/No, Enabled/Disabled options)
//        public override void OnBind()
//        {
//            base.OnBind();
//            _toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

//            OnLeftClick += (ev, v) => Value = !Value;
//        }
//        public override void DrawSelf(SpriteBatch spriteBatch)
//        {
//            base.DrawSelf(spriteBatch);
//            CalculatedStyle dimensions = base.GetDimensions();
//            // "Yes" and "No" since no "True" and "False" translation available
//            Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Value ? Lang.menu[126].Value : Lang.menu[124].Value, new Vector2(dimensions.X + dimensions.Width - 60, dimensions.Y + 8f), Color.White, 0f, Vector2.Zero, new Vector2(0.8f));
//            Rectangle sourceRectangle = new Rectangle(Value ? ((_toggleTexture.Width() - 2) / 2 + 2) : 0, 0, (_toggleTexture.Width() - 2) / 2, _toggleTexture.Height());
//            Vector2 drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, dimensions.Y + 8f);
//            spriteBatch.Draw(_toggleTexture.Value, drawPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
//        }
//    }
//    public class SeqDateTimeElement : SeqConfigElement<DateTime>
//    {
//        public override void OnBind()
//        {
//            //UIText text = new UIText(Value.ToString());
//            base.OnBind();
//        }
//        public override void Draw(SpriteBatch spriteBatch)
//        {

//            //Main.NewText(Value.ToString() + "2333");
//            base.Draw(spriteBatch);

//            var font = FontAssets.MouseText.Value;
//            string content = Value.ToString();
//            Vector2 size = font.MeasureString(content);
//            Rectangle rect = GetDimensions().ToRectangle();
//            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, content, rect.TopRight() - Vector2.UnitX * size - new Vector2(4, (size.Y - rect.Height) * .5f), Color.White, 0, default, new Vector2(1f));
//            //spriteBatch.DrawString(font, Value.ToString(), rect.TopRight() - Vector2.UnitX * size - new Vector2(4, (size.Y - rect.Height) * .5f), Color.White);
//            //spriteBatch.DrawRectangle(GetDimensions().ToRectangle(), Main.DiscoColor);
//        }
//    }
//    public class SeqIntInputElement : SeqConfigElement
//    {
//        public IList<int> IntList { get; set; }
//        public int Min { get; set; } = 0;
//        public int Max { get; set; } = 100;
//        public int Increment { get; set; } = 1;

//        public override void OnBind()
//        {
//            base.OnBind();

//            IntList = (IList<int>)List;

//            if (IntList != null)
//            {
//                TextDisplayFunction = () => Index + 1 + ": " + IntList[Index];
//            }

//            if (RangeAttribute != null && RangeAttribute.Min is int && RangeAttribute.Max is int)
//            {
//                Min = (int)RangeAttribute.Min;
//                Max = (int)RangeAttribute.Max;
//            }
//            if (IncrementAttribute != null && IncrementAttribute.Increment is int)
//            {
//                Increment = (int)IncrementAttribute.Increment;
//            }

//            UIPanel textBoxBackground = new UIPanel();
//            textBoxBackground.SetPadding(0);
//            UIFocusInputTextField uIInputTextField = new UIFocusInputTextField("Type here");
//            textBoxBackground.Top.Set(0f, 0f);
//            textBoxBackground.Left.Set(-130, 1f);
//            textBoxBackground.Width.Set(120, 0f);
//            textBoxBackground.Height.Set(30, 0f);
//            Append(textBoxBackground);
//            string l = Label;
//            uIInputTextField.SetText(GetValue().ToString());
//            uIInputTextField.Top.Set(5, 0f);
//            uIInputTextField.Left.Set(10, 0f);
//            uIInputTextField.Width.Set(-42, 1f); // allow space for arrows
//            uIInputTextField.Height.Set(20, 0);
//            uIInputTextField.OnTextChange += (a, b) =>
//            {
//                if (int.TryParse(uIInputTextField.CurrentString, out int val))
//                {
//                    SetValue(val);
//                }
//                //else /{
//                //	Interface.modConfig.SetMessage($"{uIInputTextField.currentString} isn't a valid value.", Color.Green);
//                //}
//            };
//            uIInputTextField.OnUnfocus += (a, b) => uIInputTextField.SetText(GetValue().ToString());
//            textBoxBackground.Append(uIInputTextField);

//            UIModConfigHoverImageSplit upDownButton = new UIModConfigHoverImageSplit(UpDownTexture, "+" + Increment, "-" + Increment);
//            upDownButton.Recalculate();
//            upDownButton.Top.Set(4f, 0f);
//            upDownButton.Left.Set(-30, 1f);
//            upDownButton.OnLeftClick += (a, b) =>
//            {
//                Rectangle r = b.GetDimensions().ToRectangle();
//                if (a.MousePosition.Y < r.Y + r.Height / 2)
//                {
//                    SetValue(Utils.Clamp(GetValue() + Increment, Min, Max));
//                }
//                else
//                {
//                    SetValue(Utils.Clamp(GetValue() - Increment, Min, Max));
//                }
//                uIInputTextField.SetText(GetValue().ToString());
//            };
//            textBoxBackground.Append(upDownButton);
//            Recalculate();
//        }

//        public virtual int GetValue() => (int)GetObject();

//        public virtual void SetValue(int value) => SetObject(Utils.Clamp(value, Min, Max));
//    }
//    public class SeqStringInputElement : SeqConfigElement<string>
//    {
//        class SeqUIFocusInputTextField : UIElement
//        {
//            public delegate void EventHandler(object sender, EventArgs e);

//            private bool _isWrapped;

//            public void HeightCheck()
//            {
//                string text = CurrentString;
//                float w = GetInnerDimensions().Width;
//                if (IsWrapped)
//                    text = FontAssets.MouseText.Value.CreateWrappedText(text, GetInnerDimensions().Width);
//                Vector2 vector = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, new Vector2(1));
//                float WrappedTextBottomPadding = 0f;
//                Vector2 vector2 = IsWrapped ? new Vector2(vector.X, vector.Y + WrappedTextBottomPadding) : new Vector2(vector.X, 16);
//                if (!IsWrapped)
//                { // TML: IsWrapped when true should prevent changing MinWidth, otherwise can't shrink in width due to logic.
//                    MinWidth.Set(vector2.X + PaddingLeft + PaddingRight, 0f);
//                }
//                MinHeight.Set(vector2.Y + PaddingTop + PaddingBottom, 0f);
//                Recalculate();
//            }

//            public bool IsWrapped
//            {
//                get
//                {
//                    return _isWrapped;
//                }
//                set
//                {
//                    _isWrapped = value;
//                    if (value)
//                        MinWidth.Set(0, 0); // TML: IsWrapped when true should prevent changing MinWidth, otherwise can't shrink in width due to CreateWrappedText+GetInnerDimensions logic. IsWrapped is false in ctor, so need to undo changes.

//                    HeightCheck();


//                }
//            }

//            public bool Focused;

//            public string CurrentString = "";

//            public readonly string _hintText;

//            public int _textBlinkerCount;

//            public int _textBlinkerState;

//            public bool UnfocusOnTab
//            {
//                get;
//                set;
//            }

//            public event EventHandler OnTextChange;

//            public event EventHandler OnUnfocus;

//            public event EventHandler OnTab;

//            public SeqUIFocusInputTextField(string hintText)
//            {
//                _hintText = hintText;
//            }

//            public void SetText(string text)
//            {
//                if (text == null)
//                {
//                    text = "";
//                }

//                if (CurrentString != text)
//                {
//                    CurrentString = text;
//                    HeightCheck();
//                    this.OnTextChange?.Invoke(this, new EventArgs());
//                }
//            }

//            public override void LeftClick(UIMouseEvent evt)
//            {
//                Main.clrInput();
//                Focused = true;
//            }

//            public override void Update(GameTime gameTime)
//            {
//                Vector2 point = new Vector2(Main.mouseX, Main.mouseY);
//                if (!ContainsPoint(point) && Main.mouseLeft)
//                {
//                    Focused = false;
//                    this.OnUnfocus?.Invoke(this, new EventArgs());
//                }

//                base.Update(gameTime);
//            }

//            public static bool JustPressed(Keys key)
//            {
//                if (Main.inputText.IsKeyDown(key))
//                {
//                    return !Main.oldInputText.IsKeyDown(key);
//                }

//                return false;
//            }

//            public override void DrawSelf(SpriteBatch spriteBatch)
//            {
//                if (Focused)
//                {
//                    PlayerInput.WritingText = true;
//                    Main.instance.HandleIME();
//                    string inputText = Main.GetInputText(CurrentString);
//                    if (Main.inputTextEscape)
//                    {
//                        Main.inputTextEscape = false;
//                        Focused = false;
//                        this.OnUnfocus?.Invoke(this, new EventArgs());
//                    }

//                    if (!inputText.Equals(CurrentString))
//                    {
//                        CurrentString = inputText;
//                        HeightCheck();
//                        this.OnTextChange?.Invoke(this, new EventArgs());
//                    }
//                    else
//                    {
//                        CurrentString = inputText;
//                    }

//                    if (JustPressed(Keys.Tab))
//                    {
//                        if (UnfocusOnTab)
//                        {
//                            Focused = false;
//                            this.OnUnfocus?.Invoke(this, new EventArgs());
//                        }

//                        this.OnTab?.Invoke(this, new EventArgs());
//                    }

//                    if (++_textBlinkerCount >= 20)
//                    {
//                        _textBlinkerState = (_textBlinkerState + 1) % 2;
//                        _textBlinkerCount = 0;
//                    }
//                }

//                string text = CurrentString;
//                if (_textBlinkerState == 1 && Focused)
//                {
//                    text += "|";
//                }
//                if (IsWrapped)
//                    text = FontAssets.MouseText.Value.CreateWrappedText(text, GetInnerDimensions().Width);
//                CalculatedStyle dimensions = GetDimensions();
//                if (CurrentString.Length == 0 && !Focused)
//                {
//                    Utils.DrawBorderString(spriteBatch, _hintText, new Vector2(dimensions.X, dimensions.Y), Color.Gray);
//                }
//                else
//                {
//                    Utils.DrawBorderString(spriteBatch, text, new Vector2(dimensions.X, dimensions.Y), Color.White);
//                }
//            }
//        }
//        void HeightCheck()
//        {
//            Value = uIInputTextField.CurrentString;
//            uIInputTextField.HeightCheck();
//            MinHeight = textBoxBackground.MinHeight = uIInputTextField.MinHeight with { Pixels = uIInputTextField.MinHeight.Pixels + 10 };
//            if (Parent?.Parent?.Parent is UIList list)
//            {
//                Parent.MinHeight = MinHeight;
//                list.Recalculate();
//            }
//            else
//                Recalculate();
//            SequenceSystem.SetSequenceUIPending();

//        }
//        UIPanel textBoxBackground;
//        SeqUIFocusInputTextField uIInputTextField;
//        bool resetted;
//        public override void OnBind()
//        {
//            base.OnBind();

//            textBoxBackground = new UIPanel();
//            textBoxBackground.SetPadding(0);
//            uIInputTextField = new SeqUIFocusInputTextField(Language.GetTextValue($"Mods.LogSpiralLibrary.Configs.StringInputHint"));
//            textBoxBackground.Top.Set(0f, 0f);
//            textBoxBackground.Left.Set(-190, 1f);
//            textBoxBackground.Width.Set(180, 0f);
//            textBoxBackground.Height.Set(90, 0f);

//            Append(textBoxBackground);
//            uIInputTextField.Top.Set(5, 0f);
//            uIInputTextField.Left.Set(10, 0f);
//            uIInputTextField.Width.Set(-20, 1f);
//            uIInputTextField.Height.Set(80, 0);
//            uIInputTextField.IsWrapped = true;
//            uIInputTextField.SetText(Value);
//            uIInputTextField.OnTextChange += (a, b) => HeightCheck();
//            textBoxBackground.Append(uIInputTextField);

//            resetted = false;
//        }
//        public override void Update(GameTime gameTime)
//        {
//            if (!resetted)
//            {
//                resetted = true;
//                HeightCheck();
//                SequenceSystem.SetSequenceUIPending(false);
//            }
//            base.Update(gameTime);
//        }
//    }
//    public abstract class SeqRangeElement : SeqConfigElement
//    {
//        private static SeqRangeElement rightLock;
//        private static SeqRangeElement rightHover;

//        public Color SliderColor { get; set; } = Color.White;
//        public Utils.ColorLerpMethod ColorMethod { get; set; }

//        internal bool DrawTicks { get; set; }

//        public abstract int NumberTicks { get; }
//        public abstract float TickIncrement { get; }

//        public abstract float Proportion { get; set; }

//        public SeqRangeElement()
//        {
//            ColorMethod = new Utils.ColorLerpMethod((percent) => Color.Lerp(Color.Black, SliderColor, percent));
//        }

//        public override void OnBind()
//        {
//            base.OnBind();

//            DrawTicks = Attribute.IsDefined(MemberInfo.MemberInfo, typeof(DrawTicksAttribute));
//            SliderColor = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SliderColorAttribute>(MemberInfo, Item, List)?.Color ?? Color.White;
//        }

//        public float DrawValueBar(SpriteBatch sb, float scale, float perc, int lockState = 0, Utils.ColorLerpMethod colorMethod = null)
//        {
//            perc = Utils.Clamp(perc, -.05f, 1.05f);

//            if (colorMethod == null)
//                colorMethod = new Utils.ColorLerpMethod(Utils.ColorLerp_BlackToWhite);

//            Texture2D colorBarTexture = TextureAssets.ColorBar.Value;
//            Vector2 vector = new Vector2((float)colorBarTexture.Width - 60, (float)colorBarTexture.Height) * scale;
//            IngameOptions.valuePosition.X -= (float)((int)vector.X);
//            Rectangle rectangle = new Rectangle((int)IngameOptions.valuePosition.X, (int)IngameOptions.valuePosition.Y - (int)vector.Y / 2, (int)vector.X, (int)vector.Y);
//            Rectangle destinationRectangle = rectangle;
//            int num = 107 + (int)Math.Cos(LogSpiralLibraryMod.ModTime / 60) * 30;
//            float num2 = rectangle.X + 5f * scale;
//            float num3 = rectangle.Y + 4f * scale;

//            if (DrawTicks)
//            {
//                int numTicks = NumberTicks;
//                if (numTicks > 1)
//                {
//                    for (int tick = 0; tick < numTicks; tick++)
//                    {
//                        float percent = tick * TickIncrement;

//                        if (percent <= 1f)
//                            sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(num2 + num * percent * scale), rectangle.Y - 2, 2, rectangle.Height + 4), Color.White);
//                    }
//                }
//            }

//            sb.Draw(colorBarTexture, rectangle, Color.White);

//            for (float num4 = 0f; num4 < (float)num; num4 += 1f)
//            {
//                float percent = num4 / (float)num;
//                sb.Draw(TextureAssets.ColorBlip.Value, new Vector2(num2 + num4 * scale, num3), null, colorMethod(percent), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
//            }

//            rectangle.Inflate((int)(-5f * scale), 2);

//            //rectangle.X = (int)num2;
//            //rectangle.Y = (int)num3;

//            bool flag = rectangle.Contains(new Point(Main.mouseX, Main.mouseY));

//            if (lockState == 2)
//            {
//                flag = false;
//            }

//            if (flag || lockState == 1)
//            {
//                sb.Draw(TextureAssets.ColorHighlight.Value, destinationRectangle, Main.OurFavoriteColor);
//            }

//            var colorSlider = TextureAssets.ColorSlider.Value;

//            sb.Draw(colorSlider, new Vector2(num2 + num * scale * perc, num3 + 4f * scale), null, Color.White, 0f, colorSlider.Size() * 0.5f, scale, SpriteEffects.None, 0f);

//            if (Main.mouseX >= rectangle.X && Main.mouseX <= rectangle.X + rectangle.Width)
//            {
//                IngameOptions.inBar = flag;
//                return (Main.mouseX - rectangle.X) / (float)rectangle.Width;
//            }

//            IngameOptions.inBar = false;

//            if (rectangle.X >= Main.mouseX)
//            {
//                return 0f;
//            }

//            return 1f;
//        }

//        public override void DrawSelf(SpriteBatch spriteBatch)
//        {
//            base.DrawSelf(spriteBatch);

//            float num = 6f;
//            int num2 = 0;

//            rightHover = null;

//            if (!Main.mouseLeft)
//            {
//                rightLock = null;
//            }

//            if (rightLock == this)
//            {
//                num2 = 1;
//            }
//            else if (rightLock != null)
//            {
//                num2 = 2;
//            }

//            CalculatedStyle dimensions = GetDimensions();
//            float num3 = dimensions.Width + 1f;
//            Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
//            bool flag2 = IsMouseHovering;

//            if (num2 == 1)
//            {
//                flag2 = true;
//            }

//            if (num2 == 2)
//            {
//                flag2 = false;
//            }

//            Vector2 vector2 = vector;
//            vector2.X += 8f;
//            vector2.Y += 2f + num;
//            vector2.X -= 17f;
//            //TextureAssets.ColorBar.Value.Frame(1, 1, 0, 0);
//            vector2 = new Vector2(dimensions.X + dimensions.Width - 10f, dimensions.Y + 10f + num);
//            IngameOptions.valuePosition = vector2;
//            float obj = DrawValueBar(spriteBatch, 1f, Proportion, num2, ColorMethod);

//            if (IngameOptions.inBar || rightLock == this)
//            {
//                rightHover = this;
//                if (PlayerInput.Triggers.Current.MouseLeft && rightLock == this)
//                {
//                    Proportion = obj;
//                }
//            }

//            if (rightHover != null && rightLock == null && PlayerInput.Triggers.JustPressed.MouseLeft)
//            {
//                rightLock = rightHover;
//            }
//        }
//    }
//    public abstract class SeqPrimitiveRangeElement<T> : SeqRangeElement where T : IComparable<T>
//    {
//        public T Min { get; set; }
//        public T Max { get; set; }
//        public T Increment { get; set; }
//        public IList<T> TList { get; set; }

//        public override void OnBind()
//        {
//            base.OnBind();

//            TList = (IList<T>)List;
//            TextDisplayFunction = () => MemberInfo.Name + ": " + GetValue();

//            if (TList != null)
//            {
//                TextDisplayFunction = () => Index + 1 + ": " + TList[Index];
//            }

//            if (Label != null)
//            { // Problem with Lists using ModConfig Label.
//                TextDisplayFunction = () => Label + ": " + GetValue();
//            }

//            if (RangeAttribute != null && RangeAttribute.Min is T && RangeAttribute.Max is T)
//            {
//                Min = (T)RangeAttribute.Min;
//                Max = (T)RangeAttribute.Max;
//            }
//            if (IncrementAttribute != null && IncrementAttribute.Increment is T)
//            {
//                Increment = (T)IncrementAttribute.Increment;
//            }
//        }

//        public virtual T GetValue() => (T)GetObject();

//        public virtual void SetValue(object value)
//        {
//            if (value is T t)
//                SetObject(Utils.Clamp(t, Min, Max));
//        }
//    }
//    public class SeqFloatElement : SeqPrimitiveRangeElement<float>
//    {
//        public override int NumberTicks => (int)((Max - Min) / Increment) + 1;
//        public override float TickIncrement => (Increment) / (Max - Min);

//        public override float Proportion
//        {
//            get => (GetValue() - Min) / (Max - Min);
//            set => SetValue((float)Math.Round((value * (Max - Min) + Min) * (1 / Increment)) * Increment);
//        }

//        public SeqFloatElement()
//        {
//            Min = 0;
//            Max = 1;
//            Increment = 0.01f;
//        }
//    }
//    public class SeqEnumElement : SeqRangeElement
//    {
//        private Func<object> _getValue;
//        private Func<object> _getValueString;
//        private Func<int> _getIndex;
//        private Action<int> _setValue;
//        private int max;
//        private string[] valueStrings;

//        public override int NumberTicks => valueStrings.Length;
//        public override float TickIncrement => 1f / (valueStrings.Length - 1);

//        public override float Proportion
//        {
//            get => _getIndex() / (float)(max - 1);
//            set => _setValue((int)(Math.Round(value * (max - 1))));
//        }

//        public override void OnBind()
//        {
//            base.OnBind();
//            valueStrings = Enum.GetNames(MemberInfo.Type);

//            // Retrieve individual Enum member labels
//            for (int i = 0; i < valueStrings.Length; i++)
//            {
//                var enumFieldFieldInfo = MemberInfo.Type.GetField(valueStrings[i]);
//                if (enumFieldFieldInfo != null)
//                {
//                    string name = ConfigManager.GetLocalizedLabel(new PropertyFieldWrapper(enumFieldFieldInfo));
//                    valueStrings[i] = name;
//                }
//            }

//            max = valueStrings.Length;

//            //valueEnums = Enum.GetValues(variable.Type);

//            TextDisplayFunction = () => MemberInfo.Name + ": " + _getValueString();
//            _getValue = () => DefaultGetValue();
//            _getValueString = () => DefaultGetStringValue();
//            _getIndex = () => DefaultGetIndex();
//            _setValue = (int value) => DefaultSetValue(value);

//            /*
//            if (array != null) {
//                _GetValue = () => array[index];
//                _SetValue = (int valueIndex) => { array[index] = (Enum)Enum.GetValues(memberInfo.Type).GetValue(valueIndex); Interface.modConfig.SetPendingChanges(); };
//                _TextDisplayFunction = () => index + 1 + ": " + _GetValueString();
//            }
//            */

//            if (Label != null)
//            {
//                TextDisplayFunction = () => Label + ": " + _getValueString();
//            }
//        }

//        private void DefaultSetValue(int index)
//        {
//            if (!MemberInfo.CanWrite)
//                return;

//            MemberInfo.SetValue(Item, Enum.GetValues(MemberInfo.Type).GetValue(index));
//            SequenceSystem.SetSequenceUIPending();
//            //Interface.modConfig.SetPendingChanges();
//        }

//        private object DefaultGetValue()
//        {
//            return MemberInfo.GetValue(Item);
//        }

//        private int DefaultGetIndex()
//        {
//            return Array.IndexOf(Enum.GetValues(MemberInfo.Type), _getValue());
//        }

//        private string DefaultGetStringValue()
//        {
//            return valueStrings[_getIndex()];
//        }
//    }
//    public class SeqActionModifyDataElement : SeqConfigElement
//    {
//        class DataObject
//        {
//            private readonly PropertyFieldWrapper memberInfo;
//            private readonly object item;
//            private readonly IList<ActionModifyData> array;
//            private readonly int index;

//            private ActionModifyData current;

//            //[LabelKey("缩放系数")]
//            const string key = "$Mods.LogSpiralLibrary.Configs.ActionModifyData.";
//            [LabelKey($"{key}sizeScaler.Label")]
//            [CustomSeqConfigItem(typeof(SeqFloatElement))]
//            [Range(0.01f, 3f)]
//            [Increment(0.1f)]
//            public float actionOffsetSize
//            {
//                get => current.actionOffsetSize;
//                set
//                {
//                    current.actionOffsetSize = value;
//                    Update();
//                }
//            }

//            //[LabelKey("时长系数")]
//            [LabelKey($"{key}timeScaler.Label")]
//            [CustomSeqConfigItem(typeof(SeqFloatElement))]
//            [Range(0.01f, 4f)]
//            [Increment(0.05f)]
//            public float actionOffsetTimeScaler
//            {
//                get => current.actionOffsetTimeScaler;
//                set
//                {
//                    current.actionOffsetTimeScaler = value;
//                    Update();
//                }
//            }

//            //[LabelKey("击退系数")]
//            [LabelKey($"{key}knockBack.Label")]
//            [CustomSeqConfigItem(typeof(SeqFloatElement))]
//            [Range(0.01f, 10f)]
//            [Increment(0.05f)]
//            public float actionOffsetKnockBack
//            {
//                get => current.actionOffsetKnockBack;
//                set
//                {
//                    current.actionOffsetKnockBack = value;
//                    Update();
//                }
//            }
//            //[LabelKey("伤害系数")]
//            [LabelKey($"{key}damage.Label")]
//            [CustomSeqConfigItem(typeof(SeqFloatElement))]
//            [Range(0.01f, 10f)]
//            [Increment(0.05f)]
//            public float actionOffsetDamage
//            {
//                get => current.actionOffsetDamage;
//                set
//                {
//                    current.actionOffsetDamage = value;
//                    Update();
//                }
//            }
//            //[LabelKey("暴击率增益")]
//            [LabelKey($"{key}critAdd.Label")]
//            [CustomSeqConfigItem(typeof(SeqIntInputElement))]
//            [Range(1, 100)]
//            [Increment(1)]
//            public int actionOffsetCritAdder
//            {
//                get => current.actionOffsetCritAdder;
//                set
//                {
//                    current.actionOffsetCritAdder = value;
//                    Update();
//                }
//            }
//            //[LabelKey("暴击率系数")]
//            [LabelKey($"{key}critMul.Label")]
//            [CustomSeqConfigItem(typeof(SeqFloatElement))]
//            [Range(0.01f, 10f)]
//            [Increment(0.05f)]
//            public float actionOffsetCritMultiplyer
//            {
//                get => current.actionOffsetCritMultiplyer;
//                set
//                {
//                    current.actionOffsetCritMultiplyer = value;
//                    Update();
//                }
//            }

//            private void Update()
//            {
//                if (array == null)
//                    memberInfo.SetValue(item, current);
//                else
//                    array[index] = current;
//                SequenceSystem.SetSequenceUIPending();
//            }
//            public DataObject(PropertyFieldWrapper memberInfo, object item)
//            {
//                this.item = item;
//                this.memberInfo = memberInfo;
//                current = (ActionModifyData)memberInfo.GetValue(item);
//            }

//            public DataObject(IList<ActionModifyData> array, int index)
//            {
//                current = array[index];
//                this.array = array;
//                this.index = index;
//            }
//        }
//        private int height;
//        private DataObject c;
//        private float min = 0;
//        private float max = 10;
//        private float increment = 0.01f;

//        public IList<ActionModifyData> DataList { get; set; }

//        public override void OnBind()
//        {
//            base.OnBind();

//            DataList = (IList<ActionModifyData>)List;

//            if (DataList != null)
//            {
//                DrawLabel = false;
//                height = 30;
//                c = new DataObject(DataList, Index);
//            }
//            else
//            {
//                height = 30;
//                c = new DataObject(MemberInfo, Item);
//            }

//            if (RangeAttribute != null && RangeAttribute.Min is float && RangeAttribute.Max is float)
//            {
//                max = (float)RangeAttribute.Max;
//                min = (float)RangeAttribute.Min;
//            }

//            if (IncrementAttribute != null && IncrementAttribute.Increment is float)
//            {
//                increment = (float)IncrementAttribute.Increment;
//            }

//            int order = 0;
//            foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(c))
//            {
//                var wrapped = SeqConfigElement.WrapIt(this, ref height, variable, c, order++);

//                // Can X and Y inherit range and increment automatically? Pass in "fake PropertyFieldWrapper" to achieve? Real one desired for label.
//                if (wrapped.Item2 is FloatElement floatElement)
//                {
//                    floatElement.Min = min;
//                    floatElement.Max = max;
//                    floatElement.Increment = increment;
//                    floatElement.DrawTicks = Attribute.IsDefined(MemberInfo.MemberInfo, typeof(DrawTicksAttribute));
//                }

//                if (DataList != null)
//                {
//                    wrapped.Item1.Left.Pixels -= 20;
//                    wrapped.Item1.Width.Pixels += 20;
//                }
//            }


//            #region 华生你发现了盲点
//            //string path = "假装我写了路径参数";
//            //Type elemType = typeof(MeleeAction);//我知道你想吐槽这个typeof，但是好像没有"类型参数数组"这种东西，实际用的时候是从一个Type链表里面取出
//            //Type seqType = typeof(SequenceBase<>);
//            //seqType = seqType.MakeGenericType(elemType);
//            //var method = seqType.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, [typeof(string)]);
//            //SequenceBase sequenceBase = (SequenceBase)method.Invoke(null, [path]);
//            //SequenceBase<elemType> sequenceBase = SequenceBase<elemType>.Load(path);
//            #endregion

//        }
//        // Draw axis? ticks?
//        public override void Draw(SpriteBatch spriteBatch)
//        {
//            base.Draw(spriteBatch);

//            //CalculatedStyle dimensions = base.GetInnerDimensions();
//            //Rectangle rectangle = dimensions.ToRectangle();
//            //rectangle = new Rectangle(rectangle.Right - 30, rectangle.Y, 30, 30);
//            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.AliceBlue);

//            //float x = (c.X - min) / (max - min);
//            //float y = (c.Y - min) / (max - min);

//            //var position = rectangle.TopLeft();
//            //position.X += x * rectangle.Width;
//            //position.Y += y * rectangle.Height;
//            //var blipRectangle = new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4);

//            //if (x >= 0 && x <= 1 && y >= 0 && y <= 1)
//            //    spriteBatch.Draw(TextureAssets.MagicPixel.Value, blipRectangle, Color.Black);

//            //if (IsMouseHovering && rectangle.Contains((Main.MouseScreen).ToPoint()) && Main.mouseLeft)
//            //{
//            //    float newPerc = (Main.mouseX - rectangle.X) / (float)rectangle.Width;
//            //    newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
//            //    c.X = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;

//            //    newPerc = (Main.mouseY - rectangle.Y) / (float)rectangle.Height;
//            //    newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
//            //    c.Y = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;
//            //}
//        }

//        internal float GetHeight()
//        {
//            return height;
//        }
//    }
//}

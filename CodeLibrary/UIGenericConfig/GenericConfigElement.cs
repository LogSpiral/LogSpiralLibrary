using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;
using Terraria.UI;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;
using Newtonsoft.Json;
using Terraria.Audio;
using Terraria.GameContent.UI.States;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System.Runtime.CompilerServices;
using LogSpiralLibrary.CodeLibrary.ConfigModification;

namespace LogSpiralLibrary.CodeLibrary.UIGenericConfig
{
    public abstract class GenericConfigElement<T> : GenericConfigElement
    {
        public virtual T Value
        {
            get => (T)GetObject();
            set => SetObject(value);
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
    public class CustomGenericConfigItemAttribute : Attribute
    {
        public Type Type { get; }

        public CustomGenericConfigItemAttribute(Type type)
        {
            Type = type;
        }
    }
    public class CustomGenericConfigItemAttribute<T> : CustomGenericConfigItemAttribute where T : GenericConfigElement
    {
        public CustomGenericConfigItemAttribute() : base(typeof(T))
        {
        }
    }
    public abstract class GenericConfigElement : ConfigElement
    {
        public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1, Action<GenericConfigElement, bool> onSetObj = null, UIElement owner = null)
        {
            //TODO 添加对更对类型的支持 尽管我觉着应该不会设计出要那么复杂参数的元素吧(?
            int elementHeight;
            Type type = memberInfo.Type;

            if (arrayType != null)
            {
                type = arrayType;
            }

            UIElement e;

            // TODO: Other common structs? -- Rectangle, Point
            var customUI = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomGenericConfigItemAttribute>(memberInfo, null, null);//是否对该成员有另外实现的UI支持

            if (customUI != null)
            {
                Type customUIType = customUI.Type;

                if (typeof(GenericConfigElement).IsAssignableFrom(customUIType))
                {
                    ConstructorInfo ctor = customUIType.GetConstructor([]);

                    if (ctor != null)
                    {
                        object instance = ctor.Invoke([]);//执行相应UI的构造函数
                        e = instance as GenericConfigElement;
                    }
                    else
                    {
                        e = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not have an empty constructor.");
                    }
                }
                else
                {
                    e = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not inherit from ConfigElement.");
                }
            }
            else if (item.GetType() == typeof(HeaderAttribute))
            {
                e = new HeaderElement((string)memberInfo.GetValue(item));
            }
            //基于成员类型添加上默认的编辑UI
            //else if (type == typeof(ItemDefinition))
            //{
            //    e = new ItemDefinitionElement();
            //}
            //else if (type == typeof(ProjectileDefinition))
            //{
            //    e = new ProjectileDefinitionElement();
            //}
            //else if (type == typeof(NPCDefinition))
            //{
            //    e = new NPCDefinitionElement();
            //}
            //else if (type == typeof(PrefixDefinition))
            //{
            //    e = new PrefixDefinitionElement();
            //}
            //else if (type == typeof(BuffDefinition))
            //{
            //    e = new BuffDefinitionElement();
            //}
            //else if (type == typeof(TileDefinition))
            //{
            //    e = new TileDefinitionElement();
            //}
            else if (type == typeof(Color))
            {
                e = new GenericColorElement();
            }
            //else if (type == typeof(Vector2))
            //{
            //    e = new Vector2Element();
            //}
            else if (type == typeof(ModDefinition))
            {
                e = new ModDefinitionElement();
            }
            else if (type == typeof(ConditionDefinition))
            {
                e = new ConditionDefinitionElement();
            }
            else if (type == typeof(bool)) // isassignedfrom?
            {
                e = new GenericBooleanElement();
            }
            else if (type == typeof(float))
            {
                e = new GenericFloatElement();
            }
            else if (type == typeof(DateTime))
            {
                e = new GenericDateTimeElement();
            }
            else if (type == typeof(ActionModifyData))
            {
                e = new SeqActionModifyDataElement();
            }
            else if (type == typeof(SeqDelegateDefinitionElement))
            {
                e = new SeqDelegateDefinitionElement();
            }
            else if (type == typeof(byte))
            {
                e = new GenericByteElement();
            }
            else if (type == typeof(uint))
            {
                e = new GenericUIntElement();
            }
            else if (type == typeof(int))
            {
                SliderAttribute sliderAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SliderAttribute>(memberInfo, item, list);

                if (sliderAttribute != null)
                    e = new GenericIntRangeElement();
                else
                    e = new GenericIntInputElement();
            }
            else if (type == typeof(string))
            {
                //OptionStringsAttribute ost = ConfigManager.GetCustomAttributeFromMemberThenMemberType<OptionStringsAttribute>(memberInfo, item, list);
                //if (ost != null)
                //    e = new StringOptionElement();
                //else
                e = new GenericStringInputElement();
            }
            else if (type.IsEnum)
            {
                if (list != null)
                    e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}).");
                else
                    e = new GenericEnumElement();
            }
            else if (type.IsArray)
            {
                e = new UIText($"{memberInfo.Name} not handled yet ({type.Name})");
                e.Top.Pixels += 6;
                e.Left.Pixels += 4;
                //e = new ArrayElement();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                e = new GenericListElement();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                e = new UIText($"{memberInfo.Name} not handled yet ({type.Name})");
                e.Top.Pixels += 6;
                e.Left.Pixels += 4;
                //e = new SetElement();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                e = new UIText($"{memberInfo.Name} not handled yet ({type.Name})");
                e.Top.Pixels += 6;
                e.Left.Pixels += 4;
                //e = new DictionaryElement();
            }
            else if (type.IsClass)
            {
                e = new GenericObjectElement(/*, ignoreSeparatePage: ignoreSeparatePage*/);
            }
            else if (type.IsValueType && !type.IsPrimitive)
            {
                e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}) Structs need special UI.");
                //e.Top.Pixels += 6;
                e.Height.Pixels += 6;
                e.Left.Pixels += 4;

                //object subitem = memberInfo.GetValue(item);
            }
            else
            {
                e = new UIText($"{memberInfo.Name} not handled yet ({type.Name})");
                e.Top.Pixels += 6;
                e.Left.Pixels += 4;
            }

            if (e != null)
            {
                if (e is GenericConfigElement configElement)
                {
                    configElement.OnSetObjectDelegate = onSetObj;
                    configElement.Owner = owner;
                    configElement.Bind(memberInfo, item, (IList)list, index);//将UI控件与成员信息及实例绑定
                    configElement.OnBind();
                }

                e.Recalculate();

                elementHeight = (int)e.GetOuterDimensions().Height;

                var container = UIModConfig.GetContainer(e, index == -1 ? order : index);
                container.Height.Pixels = elementHeight;

                if (parent is UIList uiList)
                {
                    uiList.Add(container);
                    uiList.GetTotalHeight();
                }
                else
                {
                    // Only Vector2 and Color use this I think, but modders can use the non-UIList approach for custom UI and layout.
                    container.Top.Pixels = top;
                    container.Width.Pixels = -20;
                    container.Left.Pixels = 20;
                    top += elementHeight + 4;
                    parent.Append(container);
                    parent.Height.Set(top, 0);
                }

                var tuple = new Tuple<UIElement, UIElement>(container, e);

                //if (parent == Interface.modConfig.mainConfigList)
                //{
                //    Interface.modConfig.mainConfigItems.Add(tuple);
                //}

                return tuple;
            }
            return null;
        }
        public GenericConfigElement() : base()
        {
        }
        public override void SetObject(object value)
        {
            if (List != null)
            {
                List[Index] = value;
                InternalOnSetObject();
                return;
            }

            if (!MemberInfo.CanWrite)
                return;

            MemberInfo.SetValue(Item, value);
            InternalOnSetObject();
        }
        protected void InternalOnSetObject(bool setPending = true, bool recalculateChildrenOfOwner = false)
        {
            //OnSetObject(setPending);
            OnSetObjectDelegate?.Invoke(this, setPending);
            if (Owner != null && recalculateChildrenOfOwner)
                Owner.RecalculateChildren();
        }
        public Action<GenericConfigElement, bool> OnSetObjectDelegate;
        //public virtual void OnSetObject(bool setPending = false)
        //{

        //}
        public UIElement Owner;
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            //base.DrawSelf(spriteBatch);
            //return;
            CalculatedStyle dimensions = GetDimensions();
            float num = dimensions.Width + 1f;
            Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
            Vector2 baseScale = new Vector2(0.8f);
            Color baseColor = (base.IsMouseHovering ? Color.White : Color.White);
            if (!MemberInfo.CanWrite)
            {
                baseColor = Color.Gray;
            }

            Color color = (base.IsMouseHovering ? backgroundColor : backgroundColor.MultiplyRGBA(new Color(180, 180, 180)));
            Vector2 position = vector;
            //DrawPanel2(spriteBatch, position, TextureAssets.SettingsPanel.Value, num, dimensions.Height, color);

            var bkColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
            //float T = Main.GlobalTimeWrappedHourly * .5f + dimensions.Y * .0015f;
            //bkColor = new Vector3(MathF.Cos(T * MathHelper.TwoPi) + .5f, MathF.Cos((T + 1 / 3f) * MathHelper.TwoPi) * .5f + .5f, MathF.Cos((T + 2 / 3f) * MathHelper.TwoPi) * .5f + .5f).ToColor();
            if (IsMouseHovering)
                bkColor = Color.Lerp(bkColor, Color.White with { A = 0 } * .5f, .5f);
            ComplexPanelInfo panel = new ComplexPanelInfo
            {
                destination = new Rectangle((int)position.X, (int)position.Y, (int)num, (int)dimensions.Height),
                StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_1").Value,
                glowEffectColor = IsMouseHovering ? Color.MediumPurple with { A = 0 } * .125f : default,
                glowShakingStrength = 1f,
                glowHueOffsetRange = 0.2f,
                backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value,
                backgroundFrame = new Rectangle(4, 4, 28, 28),
                backgroundUnitSize = new Vector2(28, 28) * 2f,
                backgroundColor = bkColor
            };
            panel.DrawComplexPanel(spriteBatch);
            if (DrawLabel)
            {
                position.X += 8f;
                position.Y += 8f;
                string text = TextDisplayFunction();
                if (ReloadRequired && ValueChanged)
                {
                    text = text + " - [c/FF0000:" + Language.GetTextValue("tModLoader.ModReloadRequired") + "]";
                }

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text, position, baseColor, 0f, Vector2.Zero, baseScale, num);
            }
            var pvAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomPreviewAttribute>(MemberInfo, Item, List);
            if (pvAttribute != null && IsMouseHovering)
            {
                var drawer = (ICustomConfigPreview)Activator.CreateInstance(pvAttribute.pvType);
                if (drawer.usePreview)
                    drawer.Draw(spriteBatch, this);
            }
            //if (base.IsMouseHovering && TooltipFunction != null)
            //{
            //    string text2 = TooltipFunction();
            //    if (ShowReloadRequiredTooltip)
            //    {
            //        text2 += (string.IsNullOrEmpty(text2) ? "" : "\n");
            //        text2 = text2 + "[c/" + Color.Orange.Hex3() + ":" + Language.GetTextValue("tModLoader.ModReloadRequiredMemberTooltip") + "]";
            //    }

            //    UIModConfig.Tooltip = text2;
            //}
        }
    }
    public class GenericBooleanElement : GenericConfigElement<bool>
    {
        private Asset<Texture2D> _toggleTexture;

        // TODO. Display status string? (right now only on/off texture, but True/False, Yes/No, Enabled/Disabled options)
        public override void OnBind()
        {
            base.OnBind();
            _toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

            OnLeftClick += (ev, v) => Value = !Value;
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            CalculatedStyle dimensions = base.GetDimensions();
            // "Yes" and "No" since no "True" and "False" translation available
            Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Value ? Lang.menu[126].Value : Lang.menu[124].Value, new Vector2(dimensions.X + dimensions.Width - 60, dimensions.Y + 8f), Color.White, 0f, Vector2.Zero, new Vector2(0.8f));
            Rectangle sourceRectangle = new Rectangle(Value ? ((_toggleTexture.Width() - 2) / 2 + 2) : 0, 0, (_toggleTexture.Width() - 2) / 2, _toggleTexture.Height());
            Vector2 drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, dimensions.Y + 8f);
            spriteBatch.Draw(_toggleTexture.Value, drawPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
        }
    }
    public class GenericDateTimeElement : GenericConfigElement<DateTime>
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
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, content, rect.TopRight() - Vector2.UnitX * size - new Vector2(4, (size.Y - rect.Height) * .5f), Color.White, 0, default, new Vector2(1f));
            //spriteBatch.DrawString(font, Value.ToString(), rect.TopRight() - Vector2.UnitX * size - new Vector2(4, (size.Y - rect.Height) * .5f), Color.White);
            //spriteBatch.DrawRectangle(GetDimensions().ToRectangle(), Main.DiscoColor);
        }
    }
    public class GenericIntInputElement : GenericConfigElement
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
            string l = Label;
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

        public virtual int GetValue() => (int)GetObject();

        public virtual void SetValue(int value) => SetObject(Utils.Clamp(value, Min, Max));
    }
    public class GenericStringInputElement : GenericConfigElement<string>
    {
        class GenericUIFocusInputTextField : UIElement
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

            public GenericUIFocusInputTextField(string hintText)
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
            InternalOnSetObject();

        }
        UIPanel textBoxBackground;
        GenericUIFocusInputTextField uIInputTextField;
        bool resetted;
        public override void OnBind()
        {
            base.OnBind();

            textBoxBackground = new UIPanel();
            textBoxBackground.SetPadding(0);
            uIInputTextField = new GenericUIFocusInputTextField(Language.GetTextValue($"Mods.LogSpiralLibrary.Configs.StringInputHint"));
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
                InternalOnSetObject(false);
            }
            base.Update(gameTime);
        }
    }
    public abstract class GenericRangeElement : GenericConfigElement
    {
        private static GenericRangeElement rightLock;
        private static GenericRangeElement rightHover;

        public Color SliderColor { get; set; } = Color.White;
        public Utils.ColorLerpMethod ColorMethod { get; set; }

        internal bool DrawTicks { get; set; }

        public abstract int NumberTicks { get; }
        public abstract float TickIncrement { get; }

        public abstract float Proportion { get; set; }

        public GenericRangeElement()
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
            int num = 107 + (int)Math.Cos(LogSpiralLibraryMod.ModTime / 60) * 30;
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
    public abstract class GenericPrimitiveRangeElement<T> : GenericRangeElement where T : IComparable<T>
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

        public virtual T GetValue() => (T)GetObject();

        public virtual void SetValue(object value)
        {
            if (value is T t)
                SetObject(Utils.Clamp(t, Min, Max));
        }
    }
    public class GenericFloatElement : GenericPrimitiveRangeElement<float>
    {
        public override int NumberTicks => (int)((Max - Min) / Increment) + 1;
        public override float TickIncrement => (Increment) / (Max - Min);

        public override float Proportion
        {
            get => (GetValue() - Min) / (Max - Min);
            set => SetValue((float)Math.Round((value * (Max - Min) + Min) * (1 / Increment)) * Increment);
        }

        public GenericFloatElement()
        {
            Min = 0;
            Max = 1;
            Increment = 0.01f;
        }
    }
    public class GenericEnumElement : GenericRangeElement
    {
        private Func<object> _getValue;
        private Func<object> _getValueString;
        private Func<int> _getIndex;
        private Action<int> _setValue;
        private int max;
        private string[] valueStrings;

        public override int NumberTicks => valueStrings.Length;
        public override float TickIncrement => 1f / (valueStrings.Length - 1);

        public override float Proportion
        {
            get => _getIndex() / (float)(max - 1);
            set => _setValue((int)(Math.Round(value * (max - 1))));
        }

        public override void OnBind()
        {
            base.OnBind();
            valueStrings = Enum.GetNames(MemberInfo.Type);

            // Retrieve individual Enum member labels
            for (int i = 0; i < valueStrings.Length; i++)
            {
                var enumFieldFieldInfo = MemberInfo.Type.GetField(valueStrings[i]);
                if (enumFieldFieldInfo != null)
                {
                    string name = ConfigManager.GetLocalizedLabel(new PropertyFieldWrapper(enumFieldFieldInfo));
                    valueStrings[i] = name;
                }
            }

            max = valueStrings.Length;

            //valueEnums = Enum.GetValues(variable.Type);

            TextDisplayFunction = () => MemberInfo.Name + ": " + _getValueString();
            _getValue = () => DefaultGetValue();
            _getValueString = () => DefaultGetStringValue();
            _getIndex = () => DefaultGetIndex();
            _setValue = (int value) => DefaultSetValue(value);

            /*
            if (array != null) {
                _GetValue = () => array[index];
                _SetValue = (int valueIndex) => { array[index] = (Enum)Enum.GetValues(memberInfo.Type).GetValue(valueIndex); Interface.modConfig.SetPendingChanges(); };
                _TextDisplayFunction = () => index + 1 + ": " + _GetValueString();
            }
            */

            if (Label != null)
            {
                TextDisplayFunction = () => Label + ": " + _getValueString();
            }
        }

        private void DefaultSetValue(int index)
        {
            if (!MemberInfo.CanWrite)
                return;

            MemberInfo.SetValue(Item, Enum.GetValues(MemberInfo.Type).GetValue(index));
            InternalOnSetObject();
            //Interface.modConfig.SetPendingChanges();
        }

        private object DefaultGetValue()
        {
            return MemberInfo.GetValue(Item);
        }

        private int DefaultGetIndex()
        {
            return Array.IndexOf(Enum.GetValues(MemberInfo.Type), _getValue());
        }

        private string DefaultGetStringValue()
        {
            return valueStrings[_getIndex()];
        }
    }
    public class SeqActionModifyDataElement : GenericConfigElement
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
            [Range(0.01f, 3f)]
            [Increment(0.1f)]
            public float actionOffsetSize
            {
                get => current.actionOffsetSize;
                set
                {
                    current.actionOffsetSize = value;
                    OnSetValue();
                }
            }

            //[LabelKey("时长系数")]
            [LabelKey($"{key}timeScaler.Label")]
            [Range(0.01f, 4f)]
            [Increment(0.05f)]
            public float actionOffsetTimeScaler
            {
                get => current.actionOffsetTimeScaler;
                set
                {
                    current.actionOffsetTimeScaler = value;
                    OnSetValue();
                }
            }

            //[LabelKey("击退系数")]
            [LabelKey($"{key}knockBack.Label")]
            [Range(0.01f, 10f)]
            [Increment(0.05f)]
            public float actionOffsetKnockBack
            {
                get => current.actionOffsetKnockBack;
                set
                {
                    current.actionOffsetKnockBack = value;
                    OnSetValue();
                }
            }
            //[LabelKey("伤害系数")]
            [LabelKey($"{key}damage.Label")]
            [Range(0.01f, 10f)]
            [Increment(0.05f)]
            public float actionOffsetDamage
            {
                get => current.actionOffsetDamage;
                set
                {
                    current.actionOffsetDamage = value;
                    OnSetValue();
                }
            }
            //[LabelKey("暴击率增益")]
            [LabelKey($"{key}critAdd.Label")]
            [Range(1, 100)]
            [Increment(1)]
            [Slider]
            public int actionOffsetCritAdder
            {
                get => current.actionOffsetCritAdder;
                set
                {
                    current.actionOffsetCritAdder = value;
                    OnSetValue();
                }
            }
            //[LabelKey("暴击率系数")]
            [LabelKey($"{key}critMul.Label")]
            [Range(0.01f, 10f)]
            [Increment(0.05f)]
            public float actionOffsetCritMultiplyer
            {
                get => current.actionOffsetCritMultiplyer;
                set
                {
                    current.actionOffsetCritMultiplyer = value;
                    OnSetValue();
                }
            }
            SeqActionModifyDataElement owner;
            private void OnSetValue()
            {
                if (array == null)
                    memberInfo.SetValue(item, current);
                else
                    array[index] = current;
                owner?.InternalOnSetObject();
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
                var wrapped = GenericConfigElement.WrapIt(this, ref height, variable, c, order++, onSetObj: OnSetObjectDelegate, owner: Owner);

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

        }
        internal float GetHeight()
        {
            return height;
        }
    }

    public class GenericIntRangeElement : GenericPrimitiveRangeElement<int>
    {
        public override int NumberTicks => ((Max - Min) / Increment) + 1;
        public override float TickIncrement => (float)(Increment) / (Max - Min);

        public override float Proportion
        {
            get => (GetValue() - Min) / (float)(Max - Min);
            set => SetValue((int)Math.Round((value * (Max - Min) + Min) * (1f / Increment)) * Increment);
        }

        public GenericIntRangeElement()
        {
            Min = 0;
            Max = 100;
            Increment = 1;
        }
    }

    public class GenericUIntElement : GenericPrimitiveRangeElement<uint>
    {
        public override int NumberTicks => (int)((Max - Min) / Increment) + 1;
        public override float TickIncrement => (float)(Increment) / (Max - Min);

        public override float Proportion
        {
            get => (GetValue() - Min) / (float)(Max - Min);
            set => SetValue((uint)Math.Round((value * (Max - Min) + Min) * (1f / Increment)) * Increment);
        }

        public GenericUIntElement()
        {
            Min = 0;
            Max = 100;
            Increment = 1;
        }
    }

    public class GenericByteElement : GenericPrimitiveRangeElement<byte>
    {
        public override int NumberTicks => (int)((Max - Min) / Increment) + 1;
        public override float TickIncrement => (float)(Increment) / (Max - Min);

        public override float Proportion
        {
            get => (GetValue() - Min) / (float)(Max - Min);
            set => SetValue(Convert.ToByte((int)Math.Round((value * (Max - Min) + Min) * (1f / Increment)) * Increment));
        }

        public GenericByteElement()
        {
            Min = 0;
            Max = 255;
            Increment = 1;
        }
    }

    public class GenericObjectElement : GenericConfigElement<object>
    {
        protected Func<string> AbridgedTextDisplayFunction { get; set; }

        private readonly bool ignoreSeparatePage;
        //private SeparatePageAttribute separatePageAttribute;
        //private object data;
        private bool separatePage;
        private bool pendingChanges;
        private bool expanded = true;
        private NestedUIList dataList;
        private UIModConfigHoverImage initializeButton;
        private UIModConfigHoverImage deleteButton;
        private UIModConfigHoverImage expandButton;
        private UIPanel separatePagePanel;
        private UITextPanel<FuncStringWrapper> separatePageButton;

        // Label:
        //  Members
        //  Members
        public GenericObjectElement(bool ignoreSeparatePage = false)
        {
            this.ignoreSeparatePage = ignoreSeparatePage;
        }

        public override void OnBind()
        {
            base.OnBind();

            if (List != null)
            {
                // TODO: only do this if ToString is overriden.

                var listType = MemberInfo.Type.GetGenericArguments()[0];

                System.Reflection.MethodInfo methodInfo = listType.GetMethod("ToString", Array.Empty<Type>());
                bool hasToString = methodInfo != null && methodInfo.DeclaringType != typeof(object);

                if (hasToString)
                {
                    TextDisplayFunction = () => Index + 1 + ": " + (List[Index]?.ToString() ?? "null");
                    AbridgedTextDisplayFunction = () => (List[Index]?.ToString() ?? "null");
                }
                else
                {
                    TextDisplayFunction = () => Index + 1 + ": ";
                }
            }
            else
            {
                bool hasToString = MemberInfo.Type.GetMethod("ToString", Array.Empty<Type>()).DeclaringType != typeof(object);

                if (hasToString)
                {
                    TextDisplayFunction = () => Label + (Value == null ? "" : ": " + Value.ToString());
                    AbridgedTextDisplayFunction = () => Value?.ToString() ?? "";
                }
            }

            // Null values without AllowNullAttribute aren't allowed, but could happen with modder mistakes, so not automatically populating will hint to modder the issue.
            if (Value == null && List != null)
            {
                // This should never actually happen, but I guess a bad Json file could.
                object data = Activator.CreateInstance(MemberInfo.Type, true);
                string json = JsonDefaultValueAttribute?.Json ?? "{}";

                JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

                Value = data;
            }

            separatePage = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SeparatePageAttribute>(MemberInfo, Item, List) != null;

            //separatePage = separatePage && !ignoreSeparatePage;
            //separatePage = (SeparatePageAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(SeparatePageAttribute)) != null;

            if (separatePage && !ignoreSeparatePage)
            {
                // TODO: UITextPanel doesn't update...
                separatePageButton = new UITextPanel<FuncStringWrapper>(new FuncStringWrapper(TextDisplayFunction));
                separatePageButton.HAlign = 0.5f;
                //e.Recalculate();
                //elementHeight = (int)e.GetOuterDimensions().Height;
                separatePageButton.OnLeftClick += (a, c) =>
                {
                    UIModConfig.SwitchToSubConfig(this.separatePagePanel);
                    /*	Interface.modConfig.uIElement.RemoveChild(Interface.modConfig.configPanelStack.Peek());
                        Interface.modConfig.uIElement.Append(separateListPanel);
                        Interface.modConfig.configPanelStack.Push(separateListPanel);*/
                    //separateListPanel.SetScrollbar(Interface.modConfig.uIScrollbar);

                    //UIPanel panel = new UIPanel();
                    //panel.Width.Set(200, 0);
                    //panel.Height.Set(200, 0);
                    //panel.Left.Set(200, 0);
                    //panel.Top.Set(200, 0);
                    //Interface.modConfig.Append(panel);

                    //Interface.modConfig.subMenu.Enqueue(subitem);
                    //Interface.modConfig.DoMenuModeState();
                };
                //e = new UIText($"{memberInfo.Name} click for more ({type.Name}).");
                //e.OnLeftClick += (a, b) => { };
            }

            //data = _GetValue();// memberInfo.GetValue(this.item);
            //drawLabel = false;

            if (List == null)
            {
                // Member > Class
                var expandAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ExpandAttribute>(MemberInfo, Item, List);
                if (expandAttribute != null)
                    expanded = expandAttribute.Expand;
            }
            else
            {
                // ListMember's ExpandListElements > Class
                var listType = MemberInfo.Type.GetGenericArguments()[0];
                var expandAttribute = (ExpandAttribute)Attribute.GetCustomAttribute(listType, typeof(ExpandAttribute), true);
                if (expandAttribute != null)
                    expanded = expandAttribute.Expand;
                expandAttribute = (ExpandAttribute)Attribute.GetCustomAttribute(MemberInfo.MemberInfo, typeof(ExpandAttribute), true);
                if (expandAttribute != null && expandAttribute.ExpandListElements.HasValue)
                    expanded = expandAttribute.ExpandListElements.Value;
            }

            dataList = new NestedUIList();
            dataList.Width.Set(-14, 1f);
            dataList.Left.Set(14, 0f);
            dataList.Height.Set(-30, 1f);
            dataList.Top.Set(30, 0);
            dataList.ListPadding = 5f;

            if (expanded)
                Append(dataList);

            //string name = memberInfo.Name;
            //if (labelAttribute != null) {
            //	name = labelAttribute.Label;
            //}
            if (List == null)
            {
                // drawLabel = false; TODO uncomment
            }

            initializeButton = new UIModConfigHoverImage(PlayTexture, Language.GetTextValue("tModLoader.ModConfigInitialize"));
            initializeButton.Top.Pixels += 4;
            initializeButton.Left.Pixels -= 3;
            initializeButton.HAlign = 1f;
            initializeButton.OnLeftClick += (a, b) =>
            {
                SoundEngine.PlaySound(21);

                object data = Activator.CreateInstance(MemberInfo.Type, true);
                string json = JsonDefaultValueAttribute?.Json ?? "{}";

                JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

                Value = data;

                //SeparatePageAttribute here?

                pendingChanges = true;
                //RemoveChild(initializeButton);
                //Append(deleteButton);
                //Append(expandButton);

                SetupList();

                InternalOnSetObject(true, true);//TODO 增加重计算子元素
                //Interface.modConfig.RecalculateChildren();
                //Interface.modConfig.SetPendingChanges();
            };

            expandButton = new UIModConfigHoverImage(expanded ? ExpandedTexture : CollapsedTexture, expanded ? Language.GetTextValue("tModLoader.ModConfigCollapse") : Language.GetTextValue("tModLoader.ModConfigExpand"));
            expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
            expandButton.Left.Set(-52, 1f);
            expandButton.OnLeftClick += (a, b) =>
            {
                expanded = !expanded;
                pendingChanges = true;
            };

            deleteButton = new UIModConfigHoverImage(DeleteTexture, Language.GetTextValue("tModLoader.ModConfigClear"));
            deleteButton.Top.Set(4, 0f);
            deleteButton.Left.Set(-25, 1f);
            deleteButton.OnLeftClick += (a, b) =>
            {
                Value = null;
                pendingChanges = true;

                SetupList();
                //Interface.modConfig.RecalculateChildren();
                InternalOnSetObject();
                //Interface.modConfig.SetPendingChanges();
            };

            if (Value != null)
            {
                //Append(expandButton);
                //Append(deleteButton);
                SetupList();
            }
            else
            {
                Append(initializeButton);
                //sortedContainer.Append(initializeButton);
            }

            pendingChanges = true;
            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!pendingChanges)
                return;
            pendingChanges = false;
            DrawLabel = !separatePage || ignoreSeparatePage;

            RemoveChild(deleteButton);
            RemoveChild(expandButton);
            RemoveChild(initializeButton);
            RemoveChild(dataList);
            if (separatePage && !ignoreSeparatePage)
                RemoveChild(separatePageButton);
            if (Value == null)
            {
                Append(initializeButton);
                DrawLabel = true;
            }
            else
            {
                if (List == null && !(separatePage && ignoreSeparatePage) && NullAllowed)
                    Append(deleteButton);

                if (!separatePage || ignoreSeparatePage)
                {
                    if (!ignoreSeparatePage)
                        Append(expandButton);
                    if (expanded)
                    {
                        Append(dataList);
                        expandButton.HoverText = Language.GetTextValue("tModLoader.ModConfigCollapse");
                        expandButton.SetImage(ExpandedTexture);
                    }
                    else
                    {
                        RemoveChild(dataList);
                        expandButton.HoverText = Language.GetTextValue("tModLoader.ModConfigExpand");
                        expandButton.SetImage(CollapsedTexture);
                    }
                }
                else
                {
                    Append(separatePageButton);
                }
            }
        }

        private void SetupList()
        {
            dataList.Clear();

            object data = Value;

            if (data != null)
            {
                if (separatePage && !ignoreSeparatePage)
                {
                    separatePagePanel = UIModConfig.MakeSeparateListPanel(Item, data, MemberInfo, List, Index, AbridgedTextDisplayFunction);
                }
                else
                {
                    int order = 0;
                    foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(data))
                    {
                        if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                            continue;

                        int top = 0;

                        UIModConfig.HandleHeader(dataList, ref top, ref order, variable);

                        var wrapped = GenericConfigElement.WrapIt(dataList, ref top, variable, data, order++, onSetObj: OnSetObjectDelegate, owner: Owner);

                        if (List != null)
                        {
                            //wrapped.Item1.Left.Pixels -= 20;
                            wrapped.Item1.Width.Pixels += 20;
                        }
                        else
                        {
                            //wrapped.Item1.Left.Pixels += 20;
                            //wrapped.Item1.Width.Pixels -= 20;
                        }
                    }
                }
            }
        }

        public override void Recalculate()
        {
            base.Recalculate();

            float defaultHeight = separatePage ? 40 : 30;
            float h = dataList.Parent != null ? dataList.GetTotalHeight() + defaultHeight : defaultHeight;

            Height.Set(h, 0f);

            if (Parent != null && Parent is UISortableElement)
            {
                Parent.Height.Set(h, 0f);
            }
        }
    }

    public class GenericColorElement : GenericConfigElement
    {
        private class ColorObject
        {
            private readonly PropertyFieldWrapper memberInfo;
            private readonly object item;
            private readonly IList<Color> array;
            private readonly int index;

            internal Color current;

            [LabelKey("$Config.Color.Red.Label")]
            public byte R
            {
                get => current.R;
                set
                {
                    current.R = value;
                    Update();
                }
            }

            [LabelKey("$Config.Color.Green.Label")]
            public byte G
            {
                get => current.G;
                set
                {
                    current.G = value;
                    Update();
                }
            }

            [LabelKey("$Config.Color.Blue.Label")]
            public byte B
            {
                get => current.B;
                set
                {
                    current.B = value;
                    Update();
                }
            }

            [LabelKey("$Config.Color.Hue.Label")]
            public float Hue
            {
                get => Main.rgbToHsl(current).X;
                set
                {
                    byte a = A;
                    current = Main.hslToRgb(value, Saturation, Lightness);
                    current.A = a;
                    Update();
                }
            }

            [LabelKey("$Config.Color.Saturation.Label")]
            public float Saturation
            {
                get => Main.rgbToHsl(current).Y;
                set
                {
                    byte a = A;
                    current = Main.hslToRgb(Hue, value, Lightness);
                    current.A = a;
                    Update();
                }
            }

            [LabelKey("$Config.Color.Lightness.Label")]
            public float Lightness
            {
                get => Main.rgbToHsl(current).Z;
                set
                {
                    byte a = A;
                    current = Main.hslToRgb(Hue, Saturation, value);
                    current.A = a;
                    Update();
                }
            }

            [LabelKey("$Config.Color.Alpha.Label")]
            public byte A
            {
                get => current.A;
                set
                {
                    current.A = value;
                    Update();
                }
            }

            private void Update()
            {
                if (array == null)
                    memberInfo.SetValue(item, current);
                else
                    array[index] = current;
            }

            public ColorObject(PropertyFieldWrapper memberInfo, object item)
            {
                this.item = item;
                this.memberInfo = memberInfo;
                current = (Color)memberInfo.GetValue(item);
            }

            public ColorObject(IList<Color> array, int index)
            {
                current = array[index];
                this.array = array;
                this.index = index;
            }
        }

        private int height;
        private ColorObject c;

        public IList<Color> ColorList { get; set; }

        public override void OnBind()
        {
            base.OnBind();

            ColorList = (IList<Color>)List;

            if (ColorList != null)
            {
                DrawLabel = false;//处于颜色链表中
                height = 30;
                c = new ColorObject(ColorList, Index);
            }
            else
            {
                height = 30;
                c = new ColorObject(MemberInfo, Item);
            }

            // TODO: Draw the sliders in the same manner as vanilla.
            var colorHSLSliderAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ColorHSLSliderAttribute>(MemberInfo, Item, List);
            bool useHue = colorHSLSliderAttribute != null;
            bool showSaturationAndLightness = colorHSLSliderAttribute?.ShowSaturationAndLightness ?? false;
            bool noAlpha = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ColorNoAlphaAttribute>(MemberInfo, Item, List) != null;

            var skip = new List<string>();

            if (noAlpha)
                skip.Add(nameof(ColorObject.A));
            if (useHue)
                skip.AddRange(new[] { nameof(ColorObject.R), nameof(ColorObject.G), nameof(ColorObject.B) });
            else
                skip.AddRange(new[] { nameof(ColorObject.Hue), nameof(ColorObject.Saturation), nameof(ColorObject.Lightness) });

            if (useHue && !showSaturationAndLightness)
                skip.AddRange(new[] { nameof(ColorObject.Saturation), nameof(ColorObject.Lightness) });

            int order = 0;

            foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(c))
            {
                if (skip.Contains(variable.Name))
                    continue;
                var wrapped = GenericConfigElement.WrapIt(this, ref height, variable, c, order++, onSetObj: OnSetObjectDelegate, owner: Owner);

                if (ColorList != null)
                {
                    wrapped.Item1.Left.Pixels -= 20;
                    wrapped.Item1.Width.Pixels += 20;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Rectangle hitbox = GetInnerDimensions().ToRectangle();
            hitbox = new Rectangle(hitbox.X + hitbox.Width / 2, hitbox.Y, hitbox.Width / 2, 30);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, hitbox, c.current);
        }

        internal float GetHeight()
        {
            return height;
        }
    }


    public abstract class GenericCollectionElement : GenericConfigElement
    {
        private UIModConfigHoverImage initializeButton;
        private UIModConfigHoverImage addButton;
        private UIModConfigHoverImage deleteButton;
        private UIModConfigHoverImage expandButton;
        private UIModConfigHoverImageSplit upDownButton;
        private bool expanded = true;
        private bool pendingChanges = false;

        protected object Data { get; set; }
        protected UIElement DataListElement { get; set; }
        protected NestedUIList DataList { get; set; }
        protected float Scale { get; set; } = 1f;
        protected DefaultListValueAttribute DefaultListValueAttribute { get; set; }
        protected JsonDefaultListValueAttribute JsonDefaultListValueAttribute { get; set; }

        protected virtual bool CanAdd => true;

        public override void OnBind()
        {
            base.OnBind();

            var expandAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ExpandAttribute>(MemberInfo, Item, List);
            if (expandAttribute != null)
                expanded = expandAttribute.Expand;

            Data = MemberInfo.GetValue(Item);
            DefaultListValueAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<DefaultListValueAttribute>(MemberInfo, null, null);

            MaxHeight.Set(300, 0f);
            DataListElement = new UIElement();
            DataListElement.Width.Set(-10f, 1f);
            DataListElement.Left.Set(10f, 0f);
            DataListElement.Height.Set(-30, 1f);
            DataListElement.Top.Set(30f, 0f);

            //panel.SetPadding(0);
            //panel.BackgroundColor = Microsoft.Xna.Framework.Color.Transparent;
            //panel.BorderColor =  Microsoft.Xna.Framework.Color.Transparent;

            if (Data != null && expanded)
                Append(DataListElement);

            DataListElement.OverflowHidden = true;

            DataList = new NestedUIList();
            DataList.Width.Set(-20, 1f);
            DataList.Left.Set(0, 0f);
            DataList.Height.Set(0, 1f);
            DataList.ListPadding = 5f;
            DataListElement.Append(DataList);

            UIScrollbar scrollbar = new UIScrollbar();
            scrollbar.SetView(100f, 1000f);
            scrollbar.Height.Set(-16f, 1f);
            scrollbar.Top.Set(6f, 0f);
            scrollbar.Left.Pixels -= 3;
            scrollbar.HAlign = 1f;
            DataList.SetScrollbar(scrollbar);
            DataListElement.Append(scrollbar);

            PrepareTypes();
            // allow null collections to simplify modder code for OnDeserialize and allow null and empty lists to have different meanings, etc.
            SetupList();

            if (CanAdd)
            {
                initializeButton = new UIModConfigHoverImage(PlayTexture, Language.GetTextValue("tModLoader.ModConfigInitialize"));
                initializeButton.Top.Pixels += 4;
                initializeButton.Left.Pixels -= 3;
                initializeButton.HAlign = 1f;
                initializeButton.OnLeftClick += (a, b) =>
                {
                    SoundEngine.PlaySound(SoundID.Tink);
                    InitializeCollection();
                    SetupList();
                    //Interface.modConfig.RecalculateChildren(); // not needed?
                    //Interface.modConfig.SetPendingChanges();
                    InternalOnSetObject(true, true);
                    expanded = true;
                    pendingChanges = true;
                };

                addButton = new UIModConfigHoverImage(PlusTexture, Language.GetTextValue("tModLoader.ModConfigAdd"));
                addButton.Top.Set(4, 0f);
                addButton.Left.Set(-52, 1f);
                addButton.OnLeftClick += (a, b) =>
                {
                    SoundEngine.PlaySound(SoundID.Tink);
                    AddItem();
                    SetupList();
                    //Interface.modConfig.RecalculateChildren();
                    //Interface.modConfig.SetPendingChanges();
                    InternalOnSetObject(true, true);
                    expanded = true;
                    pendingChanges = true;
                };

                deleteButton = new UIModConfigHoverImage(DeleteTexture, Language.GetTextValue("tModLoader.ModConfigClear"));
                deleteButton.Top.Set(4, 0f);
                deleteButton.Left.Set(-25, 1f);
                deleteButton.OnLeftClick += (a, b) =>
                {
                    SoundEngine.PlaySound(SoundID.Tink);
                    if (NullAllowed)
                        NullCollection();
                    else
                        ClearCollection();
                    SetupList();
                    //Interface.modConfig.RecalculateChildren();
                    //Interface.modConfig.SetPendingChanges();
                    InternalOnSetObject(true, true);
                    pendingChanges = true;
                };
            }

            expandButton = new UIModConfigHoverImage(expanded ? ExpandedTexture : CollapsedTexture, expanded ? Language.GetTextValue("tModLoader.ModConfigCollapse") : Language.GetTextValue("tModLoader.ModConfigExpand"));
            expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
            expandButton.Left.Set(-79, 1f);
            expandButton.OnLeftClick += (a, b) =>
            {
                expanded = !expanded;
                pendingChanges = true;
            };

            upDownButton = new UIModConfigHoverImageSplit(UpDownTexture, Language.GetTextValue("tModLoader.ModConfigScaleUp"), Language.GetTextValue("tModLoader.ModConfigScaleDown"));
            upDownButton.Top.Set(4, 0f);
            upDownButton.Left.Set(-106, 1f);
            upDownButton.OnLeftClick += (a, b) =>
            {
                Rectangle r = b.GetDimensions().ToRectangle();

                if (a.MousePosition.Y < r.Y + r.Height / 2)
                {
                    Scale = Math.Min(2f, Scale + 0.5f);
                }
                else
                {
                    Scale = Math.Max(1f, Scale - 0.5f);
                }
                //dataListPanel.RecalculateChildren();
                ////dataList.RecalculateChildren();
                //float h = dataList.GetTotalHeight();
                //MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
                //Recalculate();
                //if (Parent != null && Parent is UISortableElement) {
                //	Parent.Height.Pixels = GetOuterDimensions().Height;
                //}
            };
            //Append(upButton);

            //var aasdf = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png"));
            //for (int i = 0; i < 100; i++) {
            //	var vb = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png"));
            //}

            pendingChanges = true;
            Recalculate(); // Needed?
        }

        protected object CreateCollectionElementInstance(Type type)
        {
            object toAdd;

            if (DefaultListValueAttribute != null)
            {
                toAdd = DefaultListValueAttribute.Value;
            }
            else
            {
                toAdd = ConfigManager.AlternateCreateInstance(type);

                if (!type.IsValueType && type != typeof(string))
                {
                    string json = JsonDefaultListValueAttribute?.Json ?? "{}";

                    JsonConvert.PopulateObject(json, toAdd, ConfigManager.serializerSettings);
                }
            }

            return toAdd;
        }

        // SetupList called in base.ctor, but children need Types.
        protected abstract void PrepareTypes();

        protected abstract void AddItem();

        protected abstract void InitializeCollection();

        protected virtual void NullCollection()
        {
            Data = null;
            SetObject(Data);
        }
        protected abstract void ClearCollection();

        protected abstract void SetupList();

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!pendingChanges)
                return;
            pendingChanges = false;

            if (CanAdd)
            {
                RemoveChild(initializeButton);
                RemoveChild(addButton);
                RemoveChild(deleteButton);
            }
            RemoveChild(expandButton);
            RemoveChild(upDownButton);
            RemoveChild(DataListElement);

            if (Data == null)
            {
                Append(initializeButton);
            }
            else
            {
                if (CanAdd)
                {
                    Append(addButton);
                    Append(deleteButton);
                }
                Append(expandButton);
                if (expanded)
                {
                    Append(upDownButton);
                    Append(DataListElement);
                    expandButton.HoverText = Language.GetTextValue("tModLoader.ModConfigCollapse");
                    expandButton.SetImage(ExpandedTexture);
                }
                else
                {
                    expandButton.HoverText = Language.GetTextValue("tModLoader.ModConfigExpand");
                    expandButton.SetImage(CollapsedTexture);
                }
            }
        }

        public override void Recalculate()
        {
            base.Recalculate();

            float defaultHeight = 30;
            float h = DataListElement.Parent != null ? DataList.GetTotalHeight() + defaultHeight : defaultHeight; // 24 for UIElement

            h = Utils.Clamp(h, 30, 300 * Scale);

            MaxHeight.Set(300 * Scale, 0f);
            Height.Set(h, 0f);

            if (Parent != null && Parent is UISortableElement)
            {
                Parent.Height.Set(h, 0f);
            }
        }
    }

    public class GenericListElement : GenericCollectionElement
    {
        private Type listType;

        protected override void PrepareTypes()
        {
            listType = MemberInfo.Type.GetGenericArguments()[0];
            JsonDefaultListValueAttribute = ConfigManager.GetCustomAttributeFromCollectionMemberThenElementType<JsonDefaultListValueAttribute>(MemberInfo.MemberInfo, listType);
        }

        protected override void AddItem()
        {
            ((IList)Data).Add(CreateCollectionElementInstance(listType));
        }

        protected override void InitializeCollection()
        {
            Data = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
            SetObject(Data);
        }

        protected override void ClearCollection()
        {
            ((IList)Data).Clear();
        }

        protected override void SetupList()
        {
            DataList.Clear();

            int top = 0;

            if (Data != null)
            {
                for (int i = 0; i < ((IList)Data).Count; i++)
                {
                    int index = i;
                    var wrapped = GenericConfigElement.WrapIt(DataList, ref top, MemberInfo, Item, 0, Data, listType, index, OnSetObjectDelegate, Owner);

                    wrapped.Item2.Left.Pixels += 24;
                    wrapped.Item2.Width.Pixels -= 30;

                    // Add delete button.
                    UIModConfigHoverImage deleteButton = new UIModConfigHoverImage(DeleteTexture, Language.GetTextValue("tModLoader.ModConfigRemove"));
                    deleteButton.VAlign = 0.5f;
                    deleteButton.OnLeftClick += (a, b) => { ((IList)Data).RemoveAt(index); SetupList(); InternalOnSetObject(); /*Interface.modConfig.SetPendingChanges();*/ };
                    wrapped.Item1.Append(deleteButton);
                }
            }
        }
    }
}

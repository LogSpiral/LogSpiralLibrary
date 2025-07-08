using Microsoft.Xna.Framework.Graphics;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Graphics2D;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Terraria.ID;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using static System.Net.Mime.MediaTypeNames;

namespace LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel.Components;

public class OptionSlider : OptionBase //去掉了sealed
{
    private class SlideBox : UIView
    {
        private Vector2 MousePositionRelative => Main.MouseScreen - this.Position;
        public event Action ValueChangeCallback;
        public event Action EndDraggingCallback;
        private float _value;
        private bool _dragging;

        public Utils.ColorLerpMethod ColorMethod;
        public bool colorMethodPendingModified = true;
        Texture2D colorBar;


        public float Value
        {
            get => Math.Clamp(_value, 0, 1);
            set
            {
                float wantedValue = Math.Clamp(value, 0, 1);
                if (Math.Clamp(_value, 0, 1) != wantedValue)
                {
                    _value = Math.Clamp(value, 0, 1);
                    ValueChangeCallback?.Invoke();
                }
            }
        }

        public SlideBox()
        {
            BackgroundColor = Color.Black * 0.3f;
            BorderRadius = new Vector4(14f);
            SetSize(80f, 28f);
            SetTop(alignment: .5f);
        }

        ~SlideBox() => Main.RunOnMainThread(() => colorBar?.Dispose());

        public override void OnLeftMouseDown(UIMouseEvent evt)
        {
            base.OnLeftMouseDown(evt);
            UpdateDragging();
            _dragging = true;
        }

        public override void OnLeftMouseUp(UIMouseEvent evt)
        {
            base.OnLeftMouseUp(evt);
            if (_dragging)
            {
                _dragging = false;
                EndDraggingCallback?.Invoke();
            }
        }

        private void UpdateDragging()
        {
            var size = this.Size;
            float roundRadius = size.Y / 2f - 2f; // 半径
            float roundDiameter = roundRadius * 2; // 直径
            float moveWidth = size.X - roundDiameter; // 移动宽度
            float mousePositionX = MousePositionRelative.X;
            float adjustedMousePosX = mousePositionX - roundRadius; // 让圆心保持在鼠标位置
            Value = adjustedMousePosX / moveWidth;
        }
        
        public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // 基本字段
            var position = this.Position;
            var size = this.Size;
            var center = position + size * .5f;
            if (ColorMethod != null && !IgnoreMouseInteraction)
            {
                if (colorBar == null)
                {
                    try
                    {
                        colorBar = new Texture2D(Main.graphics.GraphicsDevice, 300, 1);
                    }
                    catch
                    {
                        Texture2D texdummy = null;
                        Main.RunOnMainThread(() => { texdummy = new Texture2D(Main.graphics.GraphicsDevice, 300, 1); });
                        colorBar = texdummy;
                    }
                }
                if (colorMethodPendingModified)
                {
                    Color[] colors = new Color[300];
                    for (int n = 0; n < 300; n++)
                        colors[n] = ColorMethod.Invoke(n / 299f);
                    colorBar.SetData(colors);
                    colorMethodPendingModified = false;
                }
                // SDFRectangle.BarColor(position, size, Rounded, colorBar, Vector2.UnitX / size.X, 0, Main.UIScaleMatrix);//-Main.GlobalTimeWrappedHourly

                //SDFRectangle.HasBorder(position, size, Rounded, Color.Red, 4f, Color.White, Main.UIScaleMatrix);
            }
            else
                base.HandleDraw(gameTime,spriteBatch);

            if (_dragging)
                UpdateDragging();



            // 圆的属性
            float roundRadius = size.Y / 2f - 2f; // 半径
            float roundDiameter = roundRadius * 2; // 直径
            float moveWidth = size.X - roundDiameter; // 移动宽度
            var roundCenter = position + new Vector2(Value * moveWidth, 0f);
            roundCenter.Y = center.Y;
            roundCenter.X += roundRadius;
            var roundLeftTop = roundCenter - new Vector2(roundRadius);

            // 颜色选择
            var innerColor = ColorMethod != null ? ColorMethod.Invoke(_value) : UIStyle.SliderRound;
            //var borderColor = innerColor;
            var borderColor = UIStyle.SliderRound;
            if (Utilties.Extensions.UIMethods.MouseInRound(roundCenter, (int)roundRadius))
                borderColor = UIStyle.SliderRoundHover;


            if (IgnoreMouseInteraction)
            {
                borderColor = Color.Gray * 0.6f;
                innerColor = Color.Gray * 0.6f;
            }

            // 绘制
            Utilties.SDFGraphics.HasBorderRound(roundLeftTop, default, roundDiameter, innerColor, 2f, borderColor, Utilties.SDFGraphics.GetMatrix(true));
        }

        public void OutSideEditEnd() => EndDraggingCallback?.Invoke();
    }
    private Utils.ColorLerpMethod _colorLerpMethod;
    public void SetColorMethod(Utils.ColorLerpMethod colorMethod)
    {
        _colorLerpMethod = colorMethod;
        _slideBox.ColorMethod = colorMethod;
    }
    public void SetColorPendingModified() => _slideBox.colorMethodPendingModified = true;
    private SUISplitButton _splitButton;
    private SlideBox _slideBox;
    private UINumericTextView _numericTextBox;
    public readonly static Type[] SupportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)];
    private readonly static Type[] FractionTypes = [typeof(float), typeof(double), typeof(decimal)];
    protected override void OnBind()
    {
        CheckValid();
        var v = GetValue();
        var box = new UIElementGroup();
        box.SetLeft(alignment: 1);
        box.SetTop(null,1f,.5f);
        box.Join(this);
        AddUpDown(box);
        AddTextBox(box);
        AddSlideBox(box);
    }
    internal double Min = 0;
    internal double Max = 1;
    internal double Default = 1;
    internal double? Increment = null;
    protected override void CheckAttributes()
    {
        base.CheckAttributes();
        var type = VarType;
        var pair = (Min, Max);
        if (IsInt)
            pair = (0.0, 100.0);


        if (type == typeof(byte))
            pair = (0.0, 255.0);
        else if (type == typeof(sbyte))
            pair = (-128.0, 127.0);
        if (VariableInfo.IsProperty && false) //如果是属性就可以玩得花一点(x
        {
            if (type == typeof(short))
                pair = (-32768.0, 32767.0);
            else if (type == typeof(ushort))
                pair = (0.0, 65536.0);
            else if (type == typeof(int))
                pair = (-2147483648.0, 2147483647.0);
            else if (type == typeof(uint))
                pair = (0.0, 4294967295);
        }
        (Min, Max) = pair;


        var rangeAttribute = GetAttribute<RangeAttribute>();
        if (rangeAttribute != null)
        {
            Max = Convert.ToDouble(rangeAttribute.Max);
            Min = Convert.ToDouble(rangeAttribute.Min);
        }


        var defaultValueAttribute = GetAttribute<DefaultValueAttribute>();
        if (defaultValueAttribute != null)
            Default = Convert.ToDouble(defaultValueAttribute.Value);

        var incrementAttribute = GetAttribute<IncrementAttribute>();
        if (incrementAttribute != null)
            Increment = Convert.ToDouble(incrementAttribute.Increment);
        if (Increment == null && IsInt)
            Increment = 1.0;

        var customConfigAttribute = GetAttribute<CustomModConfigItemAttribute>();
        if (customConfigAttribute != null)
        {
            var elem = Activator.CreateInstance(customConfigAttribute.Type);
            if (elem is RangeElement range)
            {
                if (range.ColorMethod.Invoke(0.0f) != Color.Black && range.ColorMethod.Invoke(1.0f) != Color.White)
                    _colorLerpMethod = range.ColorMethod;
            }
        }
        var sliderColor = GetAttribute<SliderColorAttribute>()?.Color;
        if (_colorLerpMethod == null && sliderColor != null)
            _colorLerpMethod = t => Color.Lerp(Color.Black * 0.3f, sliderColor.Value, t * t);// 
    }
    protected virtual void CheckValid()
    {
        if (!SupportedTypes.Contains(VarType))
            throw new Exception($"Field \"{OptionName}\" is not a supported type. OptionSlider supports all build-in-types except for Boolean, IntPtr and UIntPtr");
    }
    private void AddUpDown(UIElementGroup box)
    {
        //if (Increment == null) return;
        _splitButton = new SUISplitButton()
        {
            RelativeMode = RelativeMode.Horizontal,
            BgColor = Color.Black * 0.4f,
            Rounded = new Vector4(14f),
            VAlign = 0.5f,
            buttonBorderColor = Color.Black,
            buttonColor = Color.Black * 0.4f,
            Width = new(25, .0f),
            Height = new(25, .0f)
        };
        _splitButton.OnLeftMouseDown += (evt, elem) =>
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            var btn = elem as SUISplitButton;
            double value = Min + (Max - Min) * _slideBox.Value + (btn.IsUP ? 1 : -1) * Increment.Value;
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: true);
        };
        _splitButton.Join(box);
    }

    private void AddTextBox(UIElementGroup box)
    {
        bool isInt = IsInt;
        
        _numericTextBox = new UINumericTextView
        {
            BackgroundColor = Color.Black * 0.4f,
            BorderRadius = new Vector4(14f),
            MinValue = Min,
            MaxValue = Max,

            TextAlign = new Vector2(0.5f, 0.5f),
            TextOffset = new Vector2(0f, -2f),
            MaxWordLength = isInt ? 12 : 4,
            MaxLines = 1,
            WordWrap = false,

            DefaultValue = Default,
            Format = isInt ? "0" : "0.00"
        };
        _numericTextBox.SetTop(alignment: .5f);
        _numericTextBox.OnTextChanged += delegate
        {
            var text = _numericTextBox.Text;
            if (!double.TryParse(text, out var value))
                return;
            if (!_numericTextBox.IsFocus) return; // 有可能是属性导致跟着另一个变了
            // if (Increment is double d)
            //     value = Math.Round((value - Min) / d) * d + Min;
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: false);
        };
        _numericTextBox.OnEnterKeyDown += () =>
        {
            if (!_numericTextBox.IsValueSafe)
                return;
            var value = _numericTextBox.Value;
            // if (Increment is double d)
            //     value = Math.Round((value - Min) / d) * d + Min;
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: true);
        };
        _numericTextBox.SetPadding(2); // Padding影响里面的文字绘制
        _numericTextBox.SetSize(50, 28);
        _numericTextBox.Join(box);
    }

    private void AddSlideBox(UIElementGroup box)
    {
        _slideBox = new SlideBox();
        _slideBox.ColorMethod = _colorLerpMethod;
        _slideBox.ValueChangeCallback += () =>
        {
            double value = Min + (Max - Min) * _slideBox.Value;
            // 更好的体验不要Increment检测
            //if (Config.Mod.Name is not "ImproveGame" && Increment is double d && d != 0)
            //{
            //    value = Math.Round(value / d) * d;
            //}
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: false);
        };
        _slideBox.EndDraggingCallback += () =>
        {
            double value = Min + (Max - Min) * _slideBox.Value;
            //if (Config.Mod.Name is not "ImproveGame" && Increment is double d && d != 0)
            //{
            //    value = Math.Round(value / d) * d;
            //}
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: true);
        };
        _slideBox.Join(box);
    }

    private void SetConfigValue(double value, bool broadcast)
    {
        //if (!Interactable) return;
        object realValue = Convert.ChangeType(IsFractional ? value : Math.Round(value), VarType);
        SetValueDirect(realValue, broadcast);
        //ConfigHelper.SetConfigValue(Config, VariableInfo, realValue, Item, broadcast, path: path);
    }
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Increment == null)
            _splitButton?.Remove();
        _splitButton.IgnoresMouseInteraction = !Interactable;
        _slideBox.IgnoreMouseInteraction = !Interactable;
        _numericTextBox.IgnoreMouseInteraction = !Interactable;

        // 简直是天才的转换
        var value = float.Parse(GetValue()!.ToString()!);
        if (!_numericTextBox.IsFocus)
            _numericTextBox.Value = value;
        _slideBox.Value = InverseLerp((float)Min, (float)Max, value);
    }

    private static float InverseLerp(float a, float b, float value) => (value - a) / (b - a);
    // 为什么不用Utils.GetLerpValue() //((

    private bool IsFractional => FractionTypes.Contains(VarType);
    private bool IsInt => !IsFractional;
    public float LerpValue
    {
        get => _slideBox.Value;
        set
        {
            if (!_numericTextBox.IsFocus)
                _numericTextBox.Value = MathHelper.Lerp((float)Min, (float)Max, value);
            _slideBox.Value = value;
        }
    }
    public void OutSideEditEnd() => _slideBox?.OutSideEditEnd();
    protected override void OnSetValueExternal(object value)
    {
        base.OnSetValueExternal(value);
    }
}
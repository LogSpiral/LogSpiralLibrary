﻿using ImproveGame.UIFramework.BaseViews;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI.Chat;

namespace LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel.Components;

/// <summary>
/// 下拉框，用于Enum类型的字段
/// </summary>
public class OptionDropdownList : OptionBase
{
    private bool DropdownListPersists => ConfigOptionsPanel.Instance.DropdownList.DropdownCaller == this;
    private TimerView _textBox;
    private SlideText _textElement;
    private string[] _valueStrings;
    private string[] _valueTooltips;
    bool IsStringOption;
    private float _maxTextWidth;
    public override int labelReservedWidth => 70;
    protected override void OnBind()
    {
        CheckValid();

        var box = new View
        {
            IsAdaptiveWidth = true,
            HAlign = 1f,
            VAlign = 0.5f,
            Height = StyleDimension.Fill
        };
        box.JoinParent(this);

        GetOptions();

        _textBox = new TimerView
        {
            RelativeMode = RelativeMode.Horizontal,
            BgColor = Color.Black * 0.3f,
            Rounded = new Vector4(6f),
            VAlign = 0.5f
        };
        _textBox.OnLeftMouseDown += (_, _) =>
        {
            if (!Interactable) return;

            SoundEngine.PlaySound(SoundID.MenuTick);
            var dimensions = _textBox.GetDimensions();
            float x = dimensions.X;
            float y = dimensions.Y + 29;
            float width = _textBox.Width();
            var dropdownList = ConfigOptionsPanel.Instance.DropdownList;
            dropdownList.BuildDropdownList(x, y, width, _valueStrings, GetString(), this);
            dropdownList.OptionSelectedCallback = s =>
            {

                if (IsStringOption)
                {
                    SetValueDirect(s);
                    return;
                }

                int index = Array.IndexOf(_valueStrings, s);
                SetConfigValue(index);
            };
            dropdownList.DrawCallback = () =>
            {
                TooltipPanel.SetText(Tooltip);
            };
            dropdownList.HoverOnOptionCallback = s =>
            {
                int index = Array.IndexOf(_valueStrings, s);
                string tooltip = _valueTooltips[index];
                if (!string.IsNullOrWhiteSpace(tooltip))
                    TooltipPanel.SetText($"{Tooltip}\n[c/0099ff:{s}]: {tooltip}");
                // 这里不需要else了，因为DrawCallback会先执行
            };
        };
        float width = Math.Min(320, _maxTextWidth + 48);
        _textBox.SetPadding(0); // Padding影响里面的文字绘制
        _textBox.SetSizePixels(width, 28);
        _textBox.JoinParent(box);

        _textElement = new SlideText(GetString(), 30)
        {
            VAlign = 0.5f,
            Left = { Pixels = 8 },
            RelativeMode = RelativeMode.None
        };
        _textElement.JoinParent(_textBox);
    }
    private void GetOptions()
    {
        if (!IsStringOption)
            _valueStrings = Enum.GetNames(VarType);
        _valueTooltips = new string[_valueStrings.Length];

        if (!IsStringOption)
            for (int i = 0; i < _valueStrings.Length; i++)
            {
                var enumFieldFieldInfo = VarType.GetField(_valueStrings[i]);
                if (enumFieldFieldInfo is null)
                    continue;

                string name = ConfigManager.GetLocalizedLabel(new PropertyFieldWrapper(enumFieldFieldInfo));
                _valueStrings[i] = name;
                string tooltip = ConfigManager.GetLocalizedTooltip(new PropertyFieldWrapper(enumFieldFieldInfo));
                _valueTooltips[i] = tooltip;
            }

        _maxTextWidth = _valueStrings.Max(i => ChatManager.GetStringSize(FontAssets.MouseText.Value, i, Vector2.One).X);
    }

    private void CheckValid()
    {
        var strOptionAttribute = GetAttribute<OptionStringsAttribute>();
        if (strOptionAttribute != null)
        {
            _valueStrings = strOptionAttribute.OptionLabels;
            IsStringOption = true;
            return;
        }
        if (!VarType.IsEnum && !IsStringOption)
            throw new Exception($"Field \"{OptionName}\" is not a enum type");
    }

    private void SetConfigValue(int index)
    {
        //if (!Interactable) return;
        var value = Enum.GetValues(VarType).GetValue(index);
        SetValueDirect(value);
        //ConfigHelper.SetConfigValue(Config, VariableInfo, value, Item, broadcast, path: path);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // 就算没有Hover，要是下拉框打开了也要高光（调整AnimationTimer）
        if (!IsMouseHovering)
        {
            if (DropdownListPersists)
            {
                HoverTimer.ImmediateOpen();
            }
        }

        _textBox.IgnoresMouseInteraction = !Interactable;
        _textBox.BgColor = _textBox.HoverTimer.Lerp(Color.Black * 0.4f, Color.Black * 0.2f);
        _textElement.DisplayText = GetString();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        // 被Attribute禁止的情况
        if (Height.Pixels is 0) return;

        var textBoxRect = _textBox.GetDimensions().ToRectangle();
        var tex = ModAsset.DropdownListMark.Value;
        var effects = DropdownListPersists
            ? SpriteEffects.FlipVertically
            : SpriteEffects.None;
        spriteBatch.Draw(tex, new Vector2(textBoxRect.Right - 16, textBoxRect.Center.Y + 1),
            null, Color.White, 0f, tex.Size() / 2f, Vector2.One, effects, 0f);
    }

    private int GetIndex() => IsStringOption ? Array.IndexOf(_valueStrings, GetValue()) : Array.IndexOf(Enum.GetValues(VarType), GetValue());
    private string GetString() => IsStringOption ? GetValue().ToString() : _valueStrings[GetIndex()];
}
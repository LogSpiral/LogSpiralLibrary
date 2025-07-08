using LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel.Components;
using Newtonsoft.Json;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Graphics2D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel;

public class OptionBase : UIElementGroup, ILoadable
{
    protected IPropertyPanelHandler Handler { get; set; }
    protected bool Interactable => Handler.Interactable();
    public virtual void Register(Mod mod)
    {

    }
    public void Load(Mod mod)
    {
        PropertyPanelSystem.RegisterOption(this);
        Register(mod);
    }
    public virtual void Unload()
    {

    }
    public virtual int labelReservedWidth => 70;
    public Type VarType => List != null ? List[index].GetType() : VariableInfo.Type;

    // 原来的构造函数改成OnBind了
    // 因为现在要构造出来另外赋一些值再Bind，都写构造函数太杂乱了
    public void Bind(ModConfig config, PropertyFieldWrapper propertyFieldWrapper, Action<OptionBase> preLabelAppend)
    {
        Config = config;
        OptionName = propertyFieldWrapper.Name;
        VariableInfo = propertyFieldWrapper;
        Item ??= ConfigOptionsPanel.GlobalItem ?? config;
        if (ConfigOptionsPanel.GlobalPath != null)
            path ??= [.. ConfigOptionsPanel.GlobalPath];
        RelativeMode = RelativeMode.Vertical;
        OverflowHidden = true;
        SetWidth(0, 1f);
        SetHeight(46, 0f);
        
        Rounded = new Vector4(12f);
        SetPadding(12, 4);
        oldValue = GetValue();
        if (GetAttribute<CustomModConfigItemAttribute>() != null)
        {
            var customButton = new SUIDrawingImage(v =>
            {
                var dimension = v.GetDimensions();
                if (v.IsMouseHovering)
                    SDFGraphics.BarStarX(dimension.Center(), new(.5f), dimension.Width * .5f, 4, .5f, TextureAssets.Extra[180].Value, Main.GlobalTimeWrappedHourly, 0.15f, GetMatrix(true));
                else
                    SDFGraphics.NoBorderStarX(dimension.Center(), new(.5f), dimension.Width * .5f, 4, .5f, Color.Yellow, GetMatrix(true));
                //SDFRectangle.HasBorder(dimension.Position(),dimension.Size(),v.Rounded,v.BgColor,v.);
            })
            {
                RelativeMode = RelativeMode.None,
                //BgColor = Color.Black * 0.4f,
                //Rounded = new Vector4(4f),
                //Left = new(-120, 1),
                Top = new(6, 0),
                Width = new(25, .0f),
                Height = new(25, .0f)
            };
            customButton.OnLeftClick += (evt, elem) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                _modInMemory = ModernConfigUI.Instance.currentMod;
                _categoryInMemory = ConfigOptionsPanel.CurrentCategory;
                Config.Open(() =>
                {
                    ModernConfigUI.Instance.Open(_modInMemory);
                    ConfigOptionsPanel.CurrentCategory = _categoryInMemory;
                }, OptionName);
            };
            customButton.OnMouseOver += (evt, elem) =>
            {
                string info = GetText("ModernConfig.CustomItemTip");
                ModernConfigUI.PopNewInfo(info, elem.GetDimensions().Position() - FontAssets.MouseText.Value.MeasureString(info), Color.Cyan);
            };

            // 这个按钮是如果有CustomModConfigItemAttribute的话，点击就会跳转过去，但是我觉得这个功能没什么用，而且和整体风格不匹配，所以删了
            // 取消注释下面的代码就可以恢复
            // customButton.JoinParent(this);
        }

        preLabelAppend?.Invoke(this);

        var labelElement = new OptionLabelElement(config, OptionName, labelReservedWidth, Label)
        {
            RelativeMode = RelativeMode.Horizontal,
            IgnoresMouseInteraction = true,
            ResetAnotherPosition = true,
        };
        labelElement.OnUpdate += _ =>
        {
            labelElement.TextColor = MarkedAsFavorite
                ? Color.Gold
                : Color.White;
            if (ReloadRequired)
                labelElement.DisplayText = ConvertLeftRight(Label) + (ValueChanged ? $" - [c/FF0000:{Language.GetTextValue("tModLoader.ModReloadRequired")}]" : "");
            //else if (labelElement.DisplayText == "")
            //    labelElement.DisplayText = ConvertLeftRight(Label);
            else
                labelElement.DisplayText = ConvertLeftRight(Label);

        };
        labelElement.JoinParent(this);
        CheckAttributes();
        //ResetDebugText();
        OnBind();
    }

    protected virtual void OnBind()
    {


    }
    public void SetValueDirect(object value, bool broadCast = true)
    {
        blockNextCheck = true;

        if (item.GetType().IsValueType)//VariableInfo.Type
        {
            VariableInfo.SetValue(item, value);
            owner?.SetValueDirect(item, broadCast);
        }
        else
            PropertyPanelHelper.SetConfigValue(Config, VariableInfo, value, Item, broadCast, path, List, index);

    }
    protected T GetAttribute<T>() where T : Attribute => ConfigManager.GetCustomAttributeFromMemberThenMemberType<T>(VariableInfo, Item, List);
    protected object GetValue()
    {
        if (List != null)
            return List[index];

        return VariableInfo.GetValue(Item);
    }
    // 为了让UI之间实际上无间隔，防止鼠标滑过时Tooltip文字闪现，这里重写绘制，而不使用Spacing
    protected override void Draw(GameTime gameTime,SpriteBatch spriteBatch)
    {
        var position = this.Position;
        var size = this.Size;

        // 这里修改这两个值，而不使用Spacing
        position.Y += 3f;
        size.Y -= 6f;

        // 背景板
        //var panelColor = HoverTimer.Lerp(UIStyle.PanelBgLight, UIStyle.PanelBgLightHover);
        Color panelColor;
        if (BackgroundColor != Color.Transparent)
            panelColor = HoverTimer.Lerp(BackgroundColor.MultiplyRGBA(new Color(100, 100, 100)), BackgroundColor);
        else
            panelColor = HoverTimer.Lerp(UIStyle.ListItemColor * 0.2f, UIStyle.ListItemColor * 0.4f);

        if (Highlighted)
        {
            SDFRectangle.DrawHasBorder(position, size, new Vector4(8f), panelColor, UIStyle.ItemSlotBorderSize, UIStyle.ItemSlotBorderFav * 0.8f, Main.UIScaleMatrix);
        }
        else
        {
            SDFRectangle.DrawNoBorder(position, size, new Vector4(8f), panelColor * 0.8f, Main.UIScaleMatrix);
        }

    }

    /// <summary>
    /// 外部操作(如联机同步)写入新值时的更新
    /// </summary>
    /// <param name="value"></param>
    protected virtual void OnSetValueExternal(object value)
    {

    }

    public override void OnRightMouseDown(UIMouseEvent evt)
    {
        base.OnRightMouseDown(evt);
        if (evt.Source != this || !Interactable) return;
        var defaultValueAttribute = GetAttribute<DefaultValueAttribute>();
        bool cantSetValue = false;
        object defaultValue = defaultValueAttribute?.Value; // 有默认值标签就默认值优先
        if (defaultValue == null || index > -1)
        {
            string json = "{}";
            var dummyConfig = Handler.CloneObject(Config);
            JsonConvert.PopulateObject(json, dummyConfig, ConfigManager.serializerSettings); // 否则从默认的config里面摘录出来

            var item = PropertyPanelHelper.GetItemViaPathForSetDefault(dummyConfig, path, out cantSetValue);
            // 会有一个专门的ForSetDefault版本是因为Config这里实现的特殊性
            // 像列表那些，下标在原始config中有的就采取那个的值，否则采取list内新添加元素的默认值
            // 除此之外，如果点到了字典里的pair，那么还要另外转字典内那个代理元素
            var itemType = item?.GetType();
            if (itemType != VarType)//path == null || path.Count == 0 || !int.TryParse(path[^1], out _) ||
            {
                if (item == null)
                    cantSetValue = true;
                else
                {
                    if (item is Color color)
                        item = Activator.CreateInstance(VariableInfo.MemberInfo.DeclaringType, [color]);
                    // 欸，这里转ColorHandler为什么不像字典的那个一样在上面函数里就转好，我不知道，当时做昏头了有点，但是下面还会有别的特殊处理的，嗯

                    //if (itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    //    defaultValue = itemType.GetProperty(VariableInfo.Name, BindingFlags.Instance | BindingFlags.Public).GetValue(item);
                    //else
                    defaultValue = VariableInfo.GetValue(item);

                }
            }
            else
                defaultValue = item;

        }
        if (!cantSetValue)
        {
            // 直接交由子元素代理赋值了
            switch (defaultValue)
            {
                case ISetElementWrapper setWrapper:
                    {
                        if (this is OptionObject objectOption && PropertyPanelHelper.GetItemViaPath(objectOption, ["OptionView", "ListView", "Elements"], true) is IEnumerable elems)
                            foreach (var elem in elems)
                            {
                                if (elem is OptionBase subOption && subOption.VariableInfo.Name == "Value")
                                {
                                    subOption.SetValueDirect(setWrapper.Value);
                                    break;
                                }
                            }
                        break;
                    }
                case IDictionaryElementWrapper dictWrapper:
                    {
                        if (this is OptionObject objectOption && PropertyPanelHelper.GetItemViaPath(objectOption, ["OptionView", "ListView", "Elements"], true) is IEnumerable elems)
                            foreach (var elem in elems)
                            {
                                if (elem is OptionBase subOption)
                                {
                                    if (subOption.VariableInfo.Name == "Value")
                                        subOption.SetValueDirect(dictWrapper.Value);
                                    else if (subOption.VariableInfo.Name == "Key")
                                        subOption.SetValueDirect(dictWrapper.Key);
                                }
                            }
                        break;
                    }
                default:
                    {
                        SetValueDirect(defaultValue);
                        break;
                    }
            }
            OnSetValueExternal(defaultValue);
            //Recalculate();
            SoundEngine.PlaySound(SoundID.Chat);
        }
    }

    public override void OnMiddleMouseDown(UIMouseEvent evt) => Handler.HandleMiddleClick();


    protected virtual void CheckAttributes()
    {
        ReloadRequired = GetAttribute<ReloadRequiredAttribute>() is not null;
        if (ReloadRequired && List == null && Item is ModConfig modConfig)
        {
            ModConfig loadTimeConfig = ConfigManager.GetLoadTimeConfig(modConfig.Mod, modConfig.Name);
            LoadTimeValue = VariableInfo.GetValue(loadTimeConfig);
        }
        var colorAttribute = GetAttribute<BackgroundColorAttribute>();
        if (colorAttribute != null)
            BackgroundColor = colorAttribute.Color;
    }
    protected override void Update(GameTime gameTime)
    {
        if (CheckExternalModify)
        {
            var value = GetValue();

            if (oldValue?.Equals(value) != true && !(oldValue == null && value == null) && !blockNextCheck)
                OnSetValueExternal(value);
            oldValue = value;
            blockNextCheck = false;
        }

        base.Update(gameTime);
    }


    /// <summary>
    /// 是否被高光显示，用于搜索
    /// </summary>
    public bool Highlighted;


    public virtual string Label => List != null ? (index + 1).ToString() : (ConfigManager.GetLocalizedText<LabelKeyAttribute, LabelArgsAttribute>(VariableInfo, "Label") ?? ConfigHelper.GetLabel(Config, OptionName));

    //public FieldInfo FieldInfo { get; }
    public PropertyFieldWrapper VariableInfo { get; private set; }
    public string DebugText { get; set; }
    // 属性面板：修改为object
    public object Config { get; private set; }
    public string OptionName { get; private set; }

    internal bool ReloadRequired;
    internal LabelKeyAttribute LabelKeyAttribute;

    // 是的是的，我不仅把原版那一套抄了一点过来让一切变得更混乱，还自己加了点更奇怪的东西
    /// <summary>
    /// 指向的目标所属的List
    /// </summary>
    public IList List { get; set; }
    /// <summary>
    /// List中的索引
    /// </summary>
    public int index = -1;
    /// <summary>
    /// 当前设置块指向的目标
    /// <br>为了和object等适配，不能直接在config改了</br>
    /// </summary>
    public object Item
    {
        get => item;
        set => item = value;
    }
    object item;
    /// <summary>
    /// 导航到目标字段的路径，具体ctrl点进来看注释
    /// </summary>
    public List<string> path;
    // 到当前目标的字段/属性路径  直接在某config下是null 在它的字段myField下是["myField"]，再在字段myField2下是["myField","myField2"]，依此类推
    // 是为了和联机同步那边的代码实现妥协的产物，那边之前直接是给config的某个字段设置就直接很多，这里不得不记录下字段路径了
    // 原版的做法似乎是直接把整个config都重新写入了一遍？
    public OptionBase owner;//当前选项所属的设置选项
    object LoadTimeValue;
    bool blockNextCheck;
    object oldValue;//上一帧的值，用于检测是否被外部修改
    protected bool CheckExternalModify = true;
    protected bool ValueChanged => !ConfigManager.ObjectEquals(LoadTimeValue, GetValue());
}

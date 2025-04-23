using LogSpiralLibrary.CodeLibrary.UIGenericConfig;
using Newtonsoft.Json;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Config;
using Terraria.Audio;
using Terraria.ModLoader.UI;

namespace LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;

//[CustomModConfigItem(typeof(AvailableConfigElement))]
/// <summary>
/// 因为一些讨厌的原因，必须用上<see cref="CustomModConfigItemAttribute"/>在相应的字段/属性处
/// </summary>
public interface IAvailabilityChangableConfig
{
    public bool Available { get; set; }
}
/// <summary>
/// 请不要把这玩意和<see cref="SeparatePageAttribute"/>一起用，目前会出冲突(x
/// </summary>
public class AvailableConfigElement : ConfigElement<IAvailabilityChangableConfig>
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
    public AvailableConfigElement()
    {
    }

    public override void OnBind()
    {
        base.OnBind();

        if (List != null)
        {
            // TODO: only do this if ToString is overriden.

            var listType = MemberInfo.Type.GetGenericArguments()[0];

            MethodInfo methodInfo = listType.GetMethod("ToString", Array.Empty<Type>());
            bool hasToString = methodInfo != null && methodInfo.DeclaringType != typeof(object);

            if (hasToString)
            {
                TextDisplayFunction = () => Index + 1 + ": " + (List[Index]?.ToString() ?? "null");
                AbridgedTextDisplayFunction = () => List[Index]?.ToString() ?? "null";
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

            Value = (IAvailabilityChangableConfig)data;
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
                UIModConfig.SwitchToSubConfig(separatePagePanel);
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

        dataList = [];
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

        initializeButton = new UIModConfigHoverImage(PlayTexture, "Initialize");
        initializeButton.Top.Pixels += 4;
        initializeButton.Left.Pixels -= 3;
        initializeButton.HAlign = 1f;
        initializeButton.OnLeftClick += (a, b) =>
        {
            SoundEngine.PlaySound(SoundID.Tink);

            object data = Activator.CreateInstance(MemberInfo.Type, true);
            string json = JsonDefaultValueAttribute?.Json ?? "{}";

            JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

            Value = (IAvailabilityChangableConfig)data;

            //SeparatePageAttribute here?

            pendingChanges = true;
            //RemoveChild(initializeButton);
            //Append(deleteButton);
            //Append(expandButton);

            SetupList();
            Interface.modConfig.RecalculateChildren();
            Interface.modConfig.SetPendingChanges();
        };
        expandButton = new UIModConfigHoverImage(expanded ? ExpandedTexture : CollapsedTexture, expanded ? "Collapse" : "Expand");
        expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
        expandButton.Left.Set(-52, 1f);
        expandButton.OnLeftClick += (a, b) =>
        {
            expanded = !expanded;
            pendingChanges = true;
        };

        deleteButton = new UIModConfigHoverImage(DeleteTexture, "Clear");
        deleteButton.Top.Set(4, 0f);
        deleteButton.Left.Set(-25, 1f);
        deleteButton.OnLeftClick += (a, b) =>
        {
            Value = null;
            pendingChanges = true;

            SetupList();
            //Interface.modConfig.RecalculateChildren();
            Interface.modConfig.SetPendingChanges();
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
                    expandButton.HoverText = "Collapse";
                    expandButton.SetImage(ExpandedTexture);
                }
                else
                {
                    RemoveChild(dataList);
                    expandButton.HoverText = "Expand";
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
                var variables = ConfigManager.GetFieldsAndProperties(data);
                int order = 0;
                {
                    PropertyInfo propInfo = data.GetType().GetProperty(nameof(IAvailabilityChangableConfig.Available), BindingFlags.Public | BindingFlags.Instance);
                    PropertyFieldWrapper availableProp = new(propInfo);
                    int top = 0;
                    UIModConfig.HandleHeader(dataList, ref top, ref order, availableProp);
                    var wrapped = UIModConfig.WrapIt(dataList, ref top, availableProp, data, order++);
                    wrapped.Item2.OnLeftClick += (e, element) =>
                    {
                        pendingChanges = true;
                        SetupList();
                    };
                }
                if (Value.Available)
                    foreach (PropertyFieldWrapper variable in variables)
                    {
                        if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) || variable.Name == nameof(IAvailabilityChangableConfig.Available))
                            continue;


                        int top = 0;

                        UIModConfig.HandleHeader(dataList, ref top, ref order, variable);

                        var wrapped = UIModConfig.WrapIt(dataList, ref top, variable, data, order++);
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

/// <summary>
/// 请不要把这玩意和<see cref="SeparatePageAttribute"/>一起用，目前会出冲突(x
/// </summary>
public class GenericAvailableConfigElement : GenericConfigElement<IAvailabilityChangableConfig>
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
    public GenericAvailableConfigElement()
    {
    }

    public override void OnBind()
    {
        base.OnBind();

        if (List != null)
        {
            // TODO: only do this if ToString is overriden.

            var listType = MemberInfo.Type.GetGenericArguments()[0];

            MethodInfo methodInfo = listType.GetMethod("ToString", Array.Empty<Type>());
            bool hasToString = methodInfo != null && methodInfo.DeclaringType != typeof(object);

            if (hasToString)
            {
                TextDisplayFunction = () => Index + 1 + ": " + (List[Index]?.ToString() ?? "null");
                AbridgedTextDisplayFunction = () => List[Index]?.ToString() ?? "null";
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

            Value = (IAvailabilityChangableConfig)data;
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
                UIModConfig.SwitchToSubConfig(separatePagePanel);
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

        dataList = [];
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

        initializeButton = new UIModConfigHoverImage(PlayTexture, "Initialize");
        initializeButton.Top.Pixels += 4;
        initializeButton.Left.Pixels -= 3;
        initializeButton.HAlign = 1f;
        initializeButton.OnLeftClick += (a, b) =>
        {
            SoundEngine.PlaySound(SoundID.Tink);

            object data = Activator.CreateInstance(MemberInfo.Type, true);
            string json = JsonDefaultValueAttribute?.Json ?? "{}";

            JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

            Value = (IAvailabilityChangableConfig)data;

            //SeparatePageAttribute here?

            pendingChanges = true;
            //RemoveChild(initializeButton);
            //Append(deleteButton);
            //Append(expandButton);

            SetupList();
            //Interface.modConfig.RecalculateChildren();
            InternalOnSetObject();
            //Interface.modConfig.SetPendingChanges();
        };
        expandButton = new UIModConfigHoverImage(expanded ? ExpandedTexture : CollapsedTexture, expanded ? "Collapse" : "Expand");
        expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
        expandButton.Left.Set(-52, 1f);
        expandButton.OnLeftClick += (a, b) =>
        {
            expanded = !expanded;
            pendingChanges = true;
        };

        deleteButton = new UIModConfigHoverImage(DeleteTexture, "Clear");
        deleteButton.Top.Set(4, 0f);
        deleteButton.Left.Set(-25, 1f);
        deleteButton.OnLeftClick += (a, b) =>
        {
            Value = null;
            pendingChanges = true;

            SetupList();
            //Interface.modConfig.RecalculateChildren();
            //Interface.modConfig.SetPendingChanges();
            InternalOnSetObject();
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
                    expandButton.HoverText = "Collapse";
                    expandButton.SetImage(ExpandedTexture);
                }
                else
                {
                    RemoveChild(dataList);
                    expandButton.HoverText = "Expand";
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
                var variables = ConfigManager.GetFieldsAndProperties(data);
                int order = 0;
                {
                    PropertyInfo propInfo = data.GetType().GetProperty(nameof(IAvailabilityChangableConfig.Available), BindingFlags.Public | BindingFlags.Instance);
                    PropertyFieldWrapper availableProp = new(propInfo);
                    int top = 0;
                    UIModConfig.HandleHeader(dataList, ref top, ref order, availableProp);
                    var wrapped = WrapIt(dataList, ref top, availableProp, data, order++, onSetObj: OnSetObjectDelegate, owner: Owner);
                    wrapped.Item2.OnLeftClick += (e, element) =>
                    {
                        pendingChanges = true;
                        SetupList();
                    };
                }
                if (Value.Available)
                    foreach (PropertyFieldWrapper variable in variables)
                    {
                        if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) || variable.Name == nameof(IAvailabilityChangableConfig.Available))
                            continue;


                        int top = 0;

                        UIModConfig.HandleHeader(dataList, ref top, ref order, variable);

                        var wrapped = WrapIt(dataList, ref top, variable, data, order++, onSetObj: OnSetObjectDelegate, owner: Owner);
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

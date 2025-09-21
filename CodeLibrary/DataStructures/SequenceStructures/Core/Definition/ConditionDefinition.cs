using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using PropertyPanelLibrary.EntityDefinition;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SilkyUIFramework;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;

[TypeConverter(typeof(ToFromStringConverter<ConditionDefinition>))]
public class ConditionDefinition : EntityDefinition
{
    public override int Type
    {
        get
        {
            var list = SequenceSystem.Conditions.ToList();
            for (int n = 0; n < list.Count; n++)
            {
                if (Name == list[n].Key)
                    return n;
            }
            return -1;
        }
    }

    public override bool IsUnloaded => Type < 0;

    public ConditionDefinition() : base()
    {
    }

    public ConditionDefinition(int type) : base(SequenceSystem.Conditions.Count == 0 ? "" : SequenceSystem.Conditions.ToList()[type].Key)
    {
    }

    public ConditionDefinition(string key) : base(key)
    {
    }

    public ConditionDefinition(string mod, string name) : base(mod, name)
    {
    }

    public static ConditionDefinition FromString(string s) => new(s);

    public static ConditionDefinition Load(TagCompound tag) => new(tag.GetString("mod"), tag.GetString("name"));

    public static readonly Func<TagCompound, ConditionDefinition> DESERIALIZER = Load;

    public override string DisplayName => IsUnloaded ? Language.GetTextValue("LegacyInterface.23") : (Name == "None" ? "None" : SequenceSystem.Conditions[Name].Description.ToString());

    public Condition ToCondition() => IsUnloaded ? SequenceSystem.AlwaysCondition : SequenceSystem.Conditions[Name];
}

/*
public class ConditionDefinitionElement : DefinitionElement<ConditionDefinition>
{
    public bool resetted;
    public override void Update(GameTime gameTime)
    {
        if (!resetted)
        {
            resetted = true;
            TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Label + ": " + OptionChoice.Tooltip, GetDimensions().Width);
            if (List != null)
            {
                TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Index + ": " + OptionChoice.Tooltip, GetDimensions().Width);
            }
            var str = TextDisplayFunction.Invoke();

            Height = MinHeight = new StyleDimension(Math.Max(Height.Pixels, FontAssets.MouseText.Value.MeasureString(str).Y + 80 * OptionScale), 0);
            if (Parent?.Parent?.Parent is UIList list)
            {
                Parent.MinHeight = MinHeight;
                Parent.Height = Height;
                list.Recalculate();
            }
            else
                Recalculate();
        }

        base.Update(gameTime);
    }
    public override DefinitionOptionElement<ConditionDefinition> CreateDefinitionOptionElement() => new ConditionDefinitionOptionElement(Value, .8f);

    public override void TweakDefinitionOptionElement(DefinitionOptionElement<ConditionDefinition> optionElement)
    {
        optionElement.Top.Set(-40f, 1f);
        //optionElement.Left.Set(-124, 1f);
        optionElement.HAlign = 0.5f;
        optionElement.Left.Set(0f, 0f);
    }

    public override List<DefinitionOptionElement<ConditionDefinition>> CreateDefinitionOptionElementList()
    {
        OptionScale = 0.8f;

        var options = new List<DefinitionOptionElement<ConditionDefinition>>();

        for (int i = 0; i < SequenceSystem.Conditions.Count; i++)
        {
            ConditionDefinitionOptionElement optionElement;

            //if (i == 0)
            //    optionElement = new ConditionDefinitionOptionElement(new ConditionDefinition("Terraria", "None"), OptionScale);
            //else
            optionElement = new ConditionDefinitionOptionElement(new ConditionDefinition(i), OptionScale);

            optionElement.OnLeftClick += (a, b) =>
            {
                Value = optionElement.Definition;
                UpdateNeeded = true;
                SelectionExpanded = false;
                InternalOnSetObject();
            };

            options.Add(optionElement);
        }

        return options;
    }

    public override List<DefinitionOptionElement<ConditionDefinition>> GetPassedOptionElements()
    {
        var passed = new List<DefinitionOptionElement<ConditionDefinition>>();

        foreach (var option in Options)
        {
            if (option.Definition.DisplayName.IndexOf(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            string modname = option.Definition.Mod;

            if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            passed.Add(option);
        }

        return passed;
    }
}
*/

//public class ConditionDefinitionOptionElement : DefinitionOptionElement<ConditionDefinition>
//{
//    private readonly UIAutoScaleTextTextPanel<string> text;

//    public ConditionDefinitionOptionElement(ConditionDefinition definition, float scale = .75f) : base(definition, scale)
//    {
//        NullID = -1;
//        SetItem(definition);
//        Scale = scale;
//        Width.Set(280 * scale, 0f);
//        Height.Set(40 * scale, 0f);
//        text = new UIAutoScaleTextTextPanel<string>(Definition.DisplayName)
//        {
//            Width = { Percent = 1f },
//            Height = { Percent = 1f },
//        };

//        Append(text);
//    }

//    public override void SetItem(ConditionDefinition item)
//    {
//        base.SetItem(item);

//        text?.SetText(item.DisplayName);
//        Tooltip = item.DisplayName;
//    }

//    public override void SetScale(float scale)
//    {
//        base.SetScale(scale);

//        Width.Set(280 * scale, 0f);
//        Height.Set(40 * scale, 0f);
//    }

//    public override void DrawSelf(SpriteBatch spriteBatch)
//    {
//        if (IsMouseHovering)
//            UIModConfig.Tooltip = Tooltip;
//    }
//}
// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(PropertyPanelLibrary))]
public class ConditionDefinitionOptionElement : SUIEntityDefinitionOption
{
    public ConditionDefinitionOptionElement()
    {
        Padding = new Margin(4);
        BorderRadius = new Vector4(8);
        FitWidth = true;
        FitHeight = true;
        BackgroundColor = Color.Black * .25f;
        NameText = new UITextView
        {
            TextAlign = new Vector2(.5f),
        };
        NameText.Join(this);
    }

    private UITextView NameText { get; set; }

    public override void OnSetDefinition(EntityDefinition current, EntityDefinition previous)
    {
        base.OnSetDefinition(current, previous);
        Tooltip = current.DisplayName;
        NameText.Text = current.DisplayName;
    }
}

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(PropertyPanelLibrary))]
public class ConditionDefinitionHandler : EntityDefinitionCommonHandler
{
    public override UIView CreateChoiceView(PropertyOption.IMetaDataHandler metaData)
    {
        return OptionChoice = new ConditionDefinitionOptionElement() { Definition = metaData.GetValue() as EntityDefinition };
    }

    protected override void FillingOptionList(List<SUIEntityDefinitionOption> options)
    {
        for (int i = 0; i < SequenceSystem.Conditions.Count; i++)
            options.Add(new ConditionDefinitionOptionElement() { Definition = new ConditionDefinition(i) });
    }
}
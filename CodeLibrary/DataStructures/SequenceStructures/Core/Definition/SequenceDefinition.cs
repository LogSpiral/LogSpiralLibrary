using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.Linq;
using PropertyPanelLibrary.EntityDefinition;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using SilkyUIFramework.Elements;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;

//[System.ComponentModel.TypeConverter(typeof(ToFromStringConverter<SequenceDefinition<T>>))]
public class SequenceDefinition<T> : EntityDefinition where T : ISequenceElement
{
    public override int Type
    {
        get
        {
            // var list = from pair in SequenceManager<T>.Sequences select pair.Value;
            int n = 0;
            foreach (var pair in SequenceManager<T>.Instance.Sequences.Keys)
            {
                if ($"{Mod}/{typeof(T).Name}/{Name}" == pair)
                    return n;
                n++;
            }
            return -1;
        }
    }

    public override bool IsUnloaded => Type < 0;

    public SequenceDefinition() : base()
    {
    }

    public SequenceDefinition(int type) : base(SequenceManager<T>.Instance.Sequences.ToList()[type].Key)
    {
    }

    public SequenceDefinition(string key)
    {
        SequenceData.ParseKeyName(key, out Mod, out _, out Name);
    }

    public SequenceDefinition(string mod, string name) : base(mod, name)
    {
    }

    public static SequenceDefinition<T> FromString(string s) => new(s);

    public static SequenceDefinition<T> Load(TagCompound tag) => new(tag.GetString("mod"), tag.GetString("name"));

    public static readonly Func<TagCompound, SequenceDefinition<T>> DESERIALIZER = Load;
    public override string DisplayName => IsUnloaded ? Language.GetTextValue("LegacyInterface.23") : (Name == "None" ? "None" : SequenceManager<T>.Instance.Sequences[$"{Mod}/{typeof(T).Name}/{Name}"].Data.DisplayName);
}
public class SequenceDefinitionElement<T> : DefinitionElement<SequenceDefinition<T>> where T : ISequenceElement
{
    public bool resetted;

    public override void Update(GameTime gameTime)
    {
        if (!resetted)
        {
            resetted = true;
            TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Label + ": " + OptionChoice.Tooltip, GetDimensions().Width - 130 * OptionScale);
            if (List != null)
            {
                TextDisplayFunction = () => FontAssets.MouseText.Value.CreateWrappedText(Index + ": " + OptionChoice.Tooltip, GetDimensions().Width - 130 * OptionScale);
            }
            var str = TextDisplayFunction.Invoke();

            Height = MinHeight = new StyleDimension(Math.Max(Height.Pixels, FontAssets.MouseText.Value.MeasureString(str).Y), 0);
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
    public override DefinitionOptionElement<SequenceDefinition<T>> CreateDefinitionOptionElement() => new SequenceDefinitionOptionElement<T>(Value, .8f);

    public override void TweakDefinitionOptionElement(DefinitionOptionElement<SequenceDefinition<T>> optionElement)
    {
        optionElement.Top.Set(0f, 0f);
        optionElement.Left.Set(-124, 1f);
    }

    public override List<DefinitionOptionElement<SequenceDefinition<T>>> CreateDefinitionOptionElementList()
    {
        OptionScale = 0.8f;

        var options = new List<DefinitionOptionElement<SequenceDefinition<T>>>();

        foreach (var key in SequenceManager<T>.Instance.Sequences.Keys)
        {
            SequenceDefinitionOptionElement<T> optionElement;

            optionElement = new SequenceDefinitionOptionElement<T>(new SequenceDefinition<T>(key), OptionScale);

            optionElement.OnLeftClick += (a, b) =>
            {
                Value = optionElement.Definition;
                UpdateNeeded = true;
                SelectionExpanded = false;
                Interface.modConfig.SetPendingChanges();
            };

            options.Add(optionElement);
        }
        return options;
    }

    public override List<DefinitionOptionElement<SequenceDefinition<T>>> GetPassedOptionElements()
    {
        var passed = new List<DefinitionOptionElement<SequenceDefinition<T>>>();

        foreach (var option in Options)
        {
            if (option.Definition.DisplayName.IndexOf(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            string modname = option.Definition.Mod;

            if (!modname.Contains(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase))
                continue;

            passed.Add(option);
        }

        return passed;
    }
}

public class SequenceDefinitionOptionElement<T> : DefinitionOptionElement<SequenceDefinition<T>> where T : ISequenceElement
{
    private readonly UIAutoScaleTextTextPanel<string> text;

    public SequenceDefinitionOptionElement(SequenceDefinition<T> definition, float scale = .75f) : base(definition, scale)
    {
        NullID = -1;
        SetItem(definition);
        Scale = scale;
        Width.Set(150 * scale, 0f);
        Height.Set(40 * scale, 0f);
        text = new UIAutoScaleTextTextPanel<string>(Definition.DisplayName)
        {
            Width = { Percent = 1f },
            Height = { Percent = 1f },
        };
        Append(text);
    }

    public override void SetItem(SequenceDefinition<T> item)
    {
        base.SetItem(item);

        text?.SetText(item.DisplayName);
    }

    public override void SetScale(float scale)
    {
        base.SetScale(scale);

        Width.Set(150 * scale, 0f);
        Height.Set(40 * scale, 0f);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (IsMouseHovering)
            UIModConfig.Tooltip = Tooltip;
    }
}

public class SequenceDefinitionHandler<T> : EntityDefinitionCommonHandler where T : ISequenceElement
{
    public override UIView CreateChoiceView(PropertyOption.IMetaDataHandler metaData)
    {
        return OptionChoice = new SUIDEfinitionTextOption() { Definition = metaData.GetValue() as EntityDefinition };
    }

    protected override void FillingOptionList(List<SUIEntityDefinitionOption> options)
    {
        foreach (var pair in SequenceManager<T>.Instance.Sequences)
            options.Add(new SUIDEfinitionTextOption() { Definition = new SequenceDefinition<T>(pair.Key) });
    }
}
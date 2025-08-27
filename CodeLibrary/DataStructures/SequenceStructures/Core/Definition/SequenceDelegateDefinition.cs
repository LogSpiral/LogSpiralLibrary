using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using Microsoft.Extensions.Options;
using PropertyPanelLibrary.EntityDefinition;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;

[TypeConverter(typeof(ToFromStringConverter<SequenceDelegateDefinition>))]
public class SequenceDelegateDefinition : EntityDefinition
{
    public override int Type
    {
        get
        {
            var list = SequenceSystem.elementDelegates.ToList();
            for (int n = 0; n < list.Count; n++)
            {
                if (Key == list[n].Key)
                    return n;
            }
            return -1;
        }
    }

    public override bool IsUnloaded => Type < 0;
    public string Key => $"{Mod}/{Name}";

    public SequenceDelegateDefinition() : base(SequenceSystem.NoneDelegateKey)
    {
    }

    public SequenceDelegateDefinition(int type) : base(SequenceSystem.elementDelegates.ToList()[type].Key)
    {
    }

    public SequenceDelegateDefinition(string key) : base(key)
    {
    }

    public SequenceDelegateDefinition(string mod, string name) : base(mod, name)
    {
    }

    public static SequenceDelegateDefinition FromString(string s) => new(s);

    public static SequenceDelegateDefinition Load(TagCompound tag) => new(tag.GetString("mod"), tag.GetString("name"));

    public static readonly Func<TagCompound, SequenceDelegateDefinition> DESERIALIZER = Load;

    public override string DisplayName => IsUnloaded ? Language.GetTextValue("LegacyInterface.23") : (Name == "None" ? "None" : Name);
}

public class DelegateDefinitionOptionElement : SUIEntityDefinitionOption
{
    public DelegateDefinitionOptionElement()
    {
        Padding = new(4);
        BorderRadius = new(2);
        FitWidth = true;
        FitHeight = true;
        BackgroundColor = Color.Black * .1f;
        NameText = new()
        {
            TextAlign = new(.5f),
        };
        NameText.Join(this);
    }
    UITextView NameText { get; set; }
    public override void OnSetDefinition(EntityDefinition current, EntityDefinition previous)
    {
        base.OnSetDefinition(current, previous);
        Tooltip = current.DisplayName;
        NameText.Text = current.DisplayName;
    }
}

public class SequenceDelegateDefinitionHandler : EntityDefinitionCommonHandler
{
    public override UIView CreateChoiceView(PropertyOption.IMetaDataHandler metaData)
    {
        return OptionChoice = new DelegateDefinitionOptionElement() { Definition = metaData.GetValue() as EntityDefinition };
    }

    protected override void FillingOptionList(List<SUIEntityDefinitionOption> options)
    {
        for (int i = 0; i < SequenceSystem.elementDelegates.Count; i++)
            options.Add(new DelegateDefinitionOptionElement() { Definition = new SequenceDelegateDefinition(i) });
    }
}
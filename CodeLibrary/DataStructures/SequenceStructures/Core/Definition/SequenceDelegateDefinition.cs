using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using PropertyPanelLibrary.EntityDefinition;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using SilkyUIFramework.Elements;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;

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

    public override string DisplayName => IsUnloaded ? Language.GetTextValue("LegacyInterface.23") : (Name == "None" ? "None" : Name);
}

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(PropertyPanelLibrary))]
public class SequenceDelegateDefinitionHandler : EntityDefinitionCommonHandler
{
    public override UIView CreateChoiceView(PropertyOption.IMetaDataHandler metaData)
    {
        return OptionChoice = new SUIDEfinitionTextOption() { Definition = metaData.GetValue() as EntityDefinition };
    }

    protected override void FillingOptionList(List<SUIEntityDefinitionOption> options)
    {
        for (int i = 0; i < SequenceSystem.elementDelegates.Count; i++)
            options.Add(new SUIDEfinitionTextOption() { Definition = new SequenceDelegateDefinition(i) });
    }
}
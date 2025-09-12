using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using PropertyPanelLibrary.EntityDefinition;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using SilkyUIFramework.BasicElements;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;

public class SequenceDataDefinition : EntityDefinition
{
    public override int Type
    {
        get
        {
            int counter = 0;
            foreach (var pair in SequenceGlobalManager.SingleGroupTypeLookup)
            {
                if (Key == pair.Key)
                    return counter;
                counter++;
            }
            return -1;
        }
    }
    public override bool IsUnloaded => Type < 0;
    string Key => Mod == nameof(LogSpiralLibrary) ? Name : $"{Mod}/{Name}";

    public Type DataType => SequenceGlobalManager.DataTypeLookup[Key];

    public override string DisplayName => Language.GetOrRegister($"Mods.{Mod}.Sequence.SequenceData.{Name}", () => Key).Value;

    public SequenceDataDefinition(string key)
    {
        var datas = key.Split('/');

        if (datas.Length == 1)
        {
            Mod = nameof(LogSpiralLibrary);
            Name = key;
        }
        else
        {
            Mod = datas[0];
            Name = datas[1];
        }
    }
}
[JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
public class SequenceDataDefinitionHandler : EntityDefinitionCommonHandler
{
    public override UIView CreateChoiceView(PropertyOption.IMetaDataHandler metaData)
    {
        return OptionChoice = new SUIDEfinitionTextOption() { Definition = metaData.GetValue() as EntityDefinition };
    }

    protected override void FillingOptionList(List<SUIEntityDefinitionOption> options)
    {
        foreach (var pair in SequenceGlobalManager.DataTypeLookup)
            options.Add(new SUIDEfinitionTextOption() { Definition = new SequenceDataDefinition(pair.Key) });
    }
}
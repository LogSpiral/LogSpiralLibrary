using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using PropertyPanelLibrary.EntityDefinition;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using SilkyUIFramework.BasicElements;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.Config;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
public class SingleGroupDefinition : EntityDefinition
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
    string Key => Mod == nameof(LogSpiralLibrary) ? Name : $"{Mod}/{Name}";

    public Type GroupType => SequenceGlobalManager.SingleGroupTypeLookup[Key];

    public override string DisplayName => Language.GetOrRegister($"Mods.{Mod}.Sequence.Groups.{Name}",()=>Key).Value;
    public SingleGroupDefinition(string key)
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
public class SingleGroupDefinitionHandler : EntityDefinitionCommonHandler
{
    public override UIView CreateChoiceView(PropertyOption.IMetaDataHandler metaData)
    {
        return OptionChoice = new SUIDEfinitionTextOption() { Definition = metaData.GetValue() as EntityDefinition };
    }

    protected override void FillingOptionList(List<SUIEntityDefinitionOption> options)
    {
        foreach (var pair in SequenceGlobalManager.SingleGroupTypeLookup)
            options.Add(new SUIDEfinitionTextOption() { Definition = new SingleGroupDefinition(pair.Key) });
    }
}
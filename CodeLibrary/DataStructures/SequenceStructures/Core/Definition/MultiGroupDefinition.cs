using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using Microsoft.Xna.Framework.Input;
using PropertyPanelLibrary.EntityDefinition;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using SilkyUIFramework.BasicElements;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.Config;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
public class MultiGroupDefinition : EntityDefinition
{
    public override int Type
    {
        get
        {
            int counter = 0;
            foreach (var pair in SequenceGlobalManager.MultiGroupTypeLookup)
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

    public Type GroupType => SequenceGlobalManager.MultiGroupTypeLookup[Key];

    public override string DisplayName => Language.GetOrRegister($"Mods.{Mod}.Sequence.Groups.{Name}", () => Key).Value;

    public MultiGroupDefinition(string key)
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

    public MultiGroupDefinition(IGroup group)
    {
        var type = group.GetType();
        foreach (var pair in SequenceGlobalManager.MultiGroupTypeLookup)
        {
            if (pair.Value == type)
            {
                var datas = pair.Key.Split('/');

                if (datas.Length == 1)
                {
                    Mod = nameof(LogSpiralLibrary);
                    Name = pair.Key;
                }
                else
                {
                    Mod = datas[0];
                    Name = datas[1];
                }
                break;
            }
        }
    }
}
// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(PropertyPanelLibrary))]
public class MultiGroupDefinitionHandler : EntityDefinitionCommonHandler
{
    public override UIView CreateChoiceView(PropertyOption.IMetaDataHandler metaData)
    {
        return OptionChoice = new SUIDEfinitionTextOption() { Definition = metaData.GetValue() as EntityDefinition };
    }

    protected override void FillingOptionList(List<SUIEntityDefinitionOption> options)
    {
        foreach (var pair in SequenceGlobalManager.MultiGroupTypeLookup)
            options.Add(new SUIDEfinitionTextOption() { Definition = new MultiGroupDefinition(pair.Key) });
    }
}
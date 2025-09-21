using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using PropertyPanelLibrary.EntityDefinition;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces;
using System.Collections.Generic;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;

public class ConditionArg(ConditionDefinition definition) : IGroupArgument, IMemberLocalized
{
    public ConditionArg() : this(new ConditionDefinition(0))
    {
    }

    public ConditionArg(string fullName) : this(fullName == null ? new ConditionDefinition(0) : new ConditionDefinition(fullName))
    {
    }

    [CustomEntityDefinitionHandler<ConditionDefinitionHandler>]
    public ConditionDefinition ConditionDefinition { get => field ??= new ConditionDefinition(0); set; } = definition;

    public bool IsHidden => ConditionDefinition.Type == 0;

    public void SetDefault() => ConditionDefinition = new ConditionDefinition(0);

    public override string ToString() => Language.GetTextValue("Mods.LogSpiralLibrary.Sequence.GroupArgs.Condition", ConditionDefinition.ToCondition().Description.Value);

    public void LoadAttributes(Dictionary<string, string> attributes)
    {
        if (attributes.Remove("condition", out string conditionName))
            ConditionDefinition = new ConditionDefinition(conditionName);
    }

    public void WriteAttributes(Dictionary<string, string> attributes)
    {
        if (ConditionDefinition.Type != 0)
            attributes["condition"] = Name;
    }

    public IGroupArgument Clone() => new ConditionArg(new ConditionDefinition(ConditionDefinition.Mod, ConditionDefinition.Name));

    public Condition Condition => ConditionDefinition.ToCondition();
    public string Name => ConditionDefinition.Name;

    string IMemberLocalized.LocalizationRootPath => "Mods.LogSpiralLibrary.Sequence.GroupArgs.ConditionArg";
    private static string[] Suffixes { get; } = ["Label"];
    IReadOnlyList<string> IMemberLocalized.LocalizationSuffixes => Suffixes;
}
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using PropertyPanelLibrary.EntityDefinition;
using System.Collections.Generic;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;

public class ConditionWeightArg(ConditionDefinition definition, float weight) : IGroupArgument
{
    public ConditionWeightArg() : this(new ConditionDefinition(0), 1)
    {
    }

    public ConditionWeightArg(string fullName) : this(new ConditionDefinition(fullName), 1)
    {
    }

    public ConditionWeightArg(float weight) : this(new ConditionDefinition(0), weight)
    {
    }

    [CustomEntityDefinitionHandler<ConditionDefinitionHandler>]
    public ConditionDefinition ConditionDefinition { get; set; } = definition;
    public float Weight { get; set; } = weight;
    public bool IsHidden => false;

    public void SetDefault()
    {
        ConditionDefinition = new(0);
        Weight = 1;
    }

    public override string ToString()
    {
        return Language.GetTextValue("Mods.LogSpiralLibrary.Sequence.GroupArgs.ConditionWeight", ConditionDefinition.ToCondition().Description.Value, Weight.ToString("0.00"));
    }

    public void LoadAttributes(Dictionary<string, string> attributes)
    {
        if (attributes.Remove("condition", out string conditionName))
            ConditionDefinition = new(conditionName);
        if (attributes.Remove("weight", out string weight) && float.TryParse(weight, out var value))
            Weight = value;
    }

    public void WriteAttributes(Dictionary<string, string> attributes)
    {
        attributes["condition"] = Name;
        attributes["weight"] = Weight.ToString("0.00");
    }

    public IGroupArgument Clone() => new ConditionWeightArg(new ConditionDefinition(ConditionDefinition.Mod, ConditionDefinition.Name), Weight);

    public Condition Condition => ConditionDefinition.ToCondition();
    public string Name => ConditionDefinition.Name;
}
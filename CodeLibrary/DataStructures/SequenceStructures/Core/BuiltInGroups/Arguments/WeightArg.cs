using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;

public class WeightArg(float weight) : IGroupArgument
{
    public WeightArg() : this(1)
    {
    }

    public float Weight { get; set; } = weight;

    public bool IsHidden => false;

    public void SetDefault()
    {
        Weight = 1f;
    }

    public override string ToString()
    {
        return Language.GetTextValue("Mods.LogSpiralLibrary.Sequence.GroupArgs.Weight", Weight);
    }

    public void LoadAttributes(Dictionary<string, string> attributes)
    {
        if (attributes.Remove("weight", out string weight) && float.TryParse(weight, out var value))
            Weight = value;
    }

    public void WriteAttributes(Dictionary<string, string> attributes)
    {
        attributes["wWeight"] = Weight.ToString("0.00");
    }
}
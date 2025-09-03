using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces;
using System.Collections.Generic;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;

public class WeightArg(float weight) : IGroupArgument, IMemberLocalized
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
        return Language.GetTextValue("Mods.LogSpiralLibrary.Sequence.GroupArgs.Weight", Weight.ToString("0.00"));
    }

    public void LoadAttributes(Dictionary<string, string> attributes)
    {
        if (attributes.Remove("weight", out string weight) && float.TryParse(weight, out var value))
            Weight = value;
    }

    public void WriteAttributes(Dictionary<string, string> attributes)
    {
        if (Weight != 1)
            attributes["weight"] = Weight.ToString("0.00");
    }

    public IGroupArgument Clone() => new WeightArg(Weight);

    string IMemberLocalized.LocalizationRootPath => "Mods.LogSpiralLibrary.Sequence.GroupArgs.WeightArg";
    private static string[] Suffixes { get; } = ["Label"];
    IReadOnlyList<string> IMemberLocalized.LocalizationSuffixes => Suffixes;
}
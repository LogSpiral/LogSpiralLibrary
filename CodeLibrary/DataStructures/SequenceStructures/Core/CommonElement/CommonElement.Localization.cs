using PropertyPanelLibrary.PropertyPanelComponents.Interfaces;
using System.Collections.Generic;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.CommonElement;

public partial class CommonElement
{
    public abstract string Category { get; }
    public abstract string LocalizationCategory { get; }
    string IMemberLocalized.LocalizationRootPath => Mod.GetLocalizationKey($"{LocalizationCategory}.{Name}");
    private static string[] Suffixes { get; } = ["Label"];
    IReadOnlyList<string> IMemberLocalized.LocalizationSuffixes => Suffixes;
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", () => GetType().Name);

    public override string ToString() => DisplayName.ToString();
}
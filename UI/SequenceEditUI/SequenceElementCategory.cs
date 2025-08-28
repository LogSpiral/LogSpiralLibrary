using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using ReLogic.Content;
using SilkyUIFramework;
using System.Collections.Generic;
using Terraria.Localization;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public class SequenceElementCategory
{
    private SequenceElementCategory()
    { }

    public LocalizedText CategoryName { get; init; }
    public Asset<Texture2D> Icon { get; init; }
    public Color ThemeColor { get; init; }
    public string ElementName { get; init; }
    public SequenceManager Maganger { get; init; }
    public ISequenceElement DefaultElement { get; init; }
    #region Statics

    public static readonly Dictionary<string, SequenceElementCategory> CategoryLookup = [];

    public static SequenceElementCategory RegisterCategory<T>(Mod mod, Asset<Texture2D> icon, T defaultElement, Color? themeColor = null) where T : ISequenceElement
    {
        var name = typeof(T).Name;
        var result = new SequenceElementCategory()
        {
            CategoryName = Language.GetOrRegister($"Mods.{mod.Name}.SequenceElements.{name}.DisplayName"),
            Icon = icon,
            ThemeColor = themeColor ?? SUIColor.Background * .25f,
            ElementName = name,
            Maganger = SequenceManager<T>.Instance,
            DefaultElement = defaultElement
        };
        CategoryLookup.TryAdd(name, result);
        return result;
    }

    #endregion Statics
}
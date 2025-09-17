using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using ReLogic.Content;
using SilkyUIFramework;
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
    public Type ElementType { get; init; }

    #region Statics
    // [JITWhenModsEnabled(nameof(SilkyUIFramework))]
    // private static Color SUIDefault => SUIColor.Background * .25f;
    // private static Color GetDefaultColor() 
    // {
    //     if (ModLoader.HasMod(nameof(SilkyUIFramework))) 
    //     {
    //         return SUIDefault;
    //     }
    //     return default;
    // }
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
            DefaultElement = defaultElement,
            ElementType = typeof(T)
        };
        SequenceSystem.CategoryLookup.TryAdd(name, result);
        return result;
    }

    #endregion Statics
}
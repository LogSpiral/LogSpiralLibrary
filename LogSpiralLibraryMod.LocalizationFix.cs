using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader.Core;
namespace LogSpiralLibrary;

partial class LogSpiralLibraryMod
{
    static string SourceFolderFix(Func<Mod, string> orig, Mod self)
    {
        var result = orig.Invoke(self);
        if (result.Length == 0)
            result = Path.Combine(ModCompile.ModSourcePath, self.Name);
        return result;
    }
    static List<(string key, string value)> LocalizationLoadFix(Func<Mod, GameCulture, List<(string key, string value)>> orig, Mod mod, GameCulture culture)
        => orig.Invoke(mod, culture);

    static void LocalizationFix() 
    {
        MonoModHooks.Add(typeof(Mod).GetMethod("get_SourceFolder"), SourceFolderFix);
        MonoModHooks.Add(typeof(LocalizationLoader).GetMethod("LoadTranslations", BindingFlags.Static | BindingFlags.NonPublic), LocalizationLoadFix);
    }
}

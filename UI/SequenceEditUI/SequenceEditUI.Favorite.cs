using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.IO;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public partial class SequenceEditUI
{
    static HashSet<string> Favorites { get; } = [];

    static void LoadFromFile() 
    {
        Favorites.Clear();
        var favPath = Path.Combine(SequenceSystem.SequenceSavePath, CurrentCategory.ElementName, "Favorites.txt");
        if (File.Exists(favPath)) 
        {
            var contents = File.ReadAllLines(favPath);
            foreach (var content in contents)
                Favorites.Add(content);
        }
    }

    static void SaveAsFile() 
    {
        var favPath = Path.Combine(SequenceSystem.SequenceSavePath, CurrentCategory.ElementName, "Favorites.txt");
        Directory.CreateDirectory(Path.Combine(SequenceSystem.SequenceSavePath, CurrentCategory.ElementName));
        File.WriteAllLines(favPath, Favorites);
    }


}

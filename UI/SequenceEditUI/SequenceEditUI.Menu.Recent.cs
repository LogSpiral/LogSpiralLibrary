using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.IO;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public partial class SequenceEditUI
{
    static List<string> Recents { get; } = [];

    static void LoadRecentListFromFile()
    {
        Recents.Clear();
        var favPath = Path.Combine(SequenceSystem.SequenceSavePath, CurrentCategory!.ElementName, "Recents.txt");
        if (File.Exists(favPath))
        {
            var contents = File.ReadAllLines(favPath);
            foreach (var content in contents)
                Recents.Add(content);
        }
    }

    static void SaveRecentListAsFile()
    {
        var favPath = Path.Combine(SequenceSystem.SequenceSavePath, CurrentCategory!.ElementName, "Recents.txt");
        Directory.CreateDirectory(Path.Combine(SequenceSystem.SequenceSavePath, CurrentCategory.ElementName));
        File.WriteAllLines(favPath, Recents);
    }
}

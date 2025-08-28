using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.IO;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;

public static class SequenceSaveHelper
{
    public static void SaveSequence(Sequence loadingSequence,string elementName) 
    {
        var modPath = Path.Combine(SequenceSystem.SequenceSavePath, elementName, loadingSequence.Data.ModDefinition.Mod);
        var folders = loadingSequence.Data.FileName.Split('/');
        if (folders.Length > 1)
            Directory.CreateDirectory(Path.Combine(modPath, Path.Combine(folders[..^1])));

        var finalPath = Path.Combine(modPath, $"{loadingSequence.Data.FileName}_Resaved.xml");
        using XmlWriter writer = XmlWriter.Create(finalPath, SequenceGlobalManager.WriterSettings);
        loadingSequence.Data.ModifyTime = DateTime.Now;
        SequenceGlobalManager.Serializer.Serialize(writer, loadingSequence);
    }
}

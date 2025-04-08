using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;

public static class SequenceManager<T> where T : ISequenceElement
{
    public static Dictionary<string, Sequence<T>> sequences = [];
    public static bool loaded;
    public static void Load()
    {
        if (loaded) return;
        loaded = true;

        List<string> keys = [nameof(LogSpiralLibrary)];
        foreach (var localMod in ModDefinitionElement.locals)
        {
            var refMods = from refMod in localMod.properties.modReferences where refMod.mod == nameof(LogSpiralLibrary) select refMod;
            if (refMods.Any())
            {
                keys.Add(localMod.Name);
            }
        }

        foreach (var key in keys)
        {
            if (ModLoader.TryGetMod(key, out Mod mod))
                foreach (var name in mod.GetFileNames())
                {
                    if (name.StartsWith($"PresetSequences/{typeof(T).Name}") && name.EndsWith(".xml"))
                    {
                        var keyName = $"{key}/{Path.GetFileNameWithoutExtension(name)}";
                        if (!sequences.TryGetValue(keyName, out var seq))
                            seq = new Sequence<T>();

                        try
                        {
                            Sequence<T>.Load(name, mod,null, seq);
                            sequences[seq.KeyName] = seq;
                        }
                        catch
                        {
                            mod.Logger.Error("Couldn't load sequence: " + name);
                        }
                    }
                }

        }



        //foreach (var type in AvailableElementBaseTypes)
        //{
        //    var list = GetAllFilesByDir($"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{type.Name}");
        //    foreach (FileInfo file in list)
        //    {
        //        var seqType = typeof(SequenceBase<>);
        //        seqType = seqType.MakeGenericType(type);
        //        var loadMethod = seqType.GetMethod("Load", BindingFlags.Public | BindingFlags.Static, [typeof(string)]);
        //        sequenceBases.Add(file.Name, (SequenceBase)loadMethod.Invoke(null, [file.FullName]));
        //    }
        //}
        var path = $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{typeof(T).Name}";
        if (!Directory.Exists(path)) return;
        var list = SequenceSystem.GetAllFilesByDir(path);
        foreach (FileInfo file in list)
        {
            var modName = file.FullName.Split('\\', '/')[^2];
            bool flag = ModLoader.TryGetMod(modName, out var mod);
            if (flag)
            {
                string key = $"{modName}/{Path.GetFileNameWithoutExtension(file.Name)}";
                try
                {
                    Sequence<T> instance = Sequence<T>.Load(file.FullName);
                    sequences[key] = instance;
                }
                catch
                {
                    mod.Logger.Error("Couldn't load sequence: " + file.Name);
                }
            }
        }
    }
    public static void WriteAllToPacket(ModPacket packet)
    {
        packet.Write((byte)sequences.Count);
        foreach (var pair in sequences)
        {
            using MemoryStream stream = new();
            XmlWriterSettings settings = new();
            settings.Indent = true;
            settings.Encoding = new UTF8Encoding(false);
            settings.NewLineChars = Environment.NewLine;
            XmlWriter xmlWriter = XmlWriter.Create(stream, settings);
            pair.Value.WriteContent(xmlWriter);
            xmlWriter.Dispose();
            packet.Write((int)stream.Length);
            packet.Write(pair.Key);
            packet.Write(stream.ToArray());
        }
    }
}

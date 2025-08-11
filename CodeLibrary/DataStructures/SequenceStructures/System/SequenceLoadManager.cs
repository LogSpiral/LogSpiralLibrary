using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Unloads;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
public static class SequenceGlobalManager
{
    /// <summary>
    /// 注册在案的序列字典
    /// <br>可编辑，可查看，可被引用</br>
    /// </summary>
    public static Dictionary<string, Sequence> SequenceLookup { get; } = [];

    /// <summary>
    /// 被引用了但是未被加载的序列的键
    /// 后续如果加载了会从这里面移除
    /// </summary>
    public static HashSet<string> UnloadSequences { get; } = [];

    /// <summary>
    /// 序列数据的注册类型集合
    /// <br>因为默认是<see cref="SequenceData"/>所以不会注册在内</br>
    /// <br>用于管理未查找到类型的<see cref="UnloadData"/>也不会注册在内</br>
    /// </summary>
    public static Dictionary<string, Type> DataTypeLookup { get; } = [];

    /// <summary>
    /// 组的注册类型集合
    /// <br>使用"模组名/类名"就能查找到对应的Type, 前置库内的可以省略模组名</br>
    /// <br>用于管理未查找到类型的<see cref="UnloadGroup"/>不会注册在内</br>
    /// </summary>
    public static Dictionary<string, Type> GroupTypeLookup { get; } = [];

    /// <summary>
    /// 元素的注册类型集合
    /// <br>使用"模组名/类名"就能查找到对应的Type, 前置库内的可以省略模组名</br>
    /// </summary>
    public static Dictionary<string, Type> ElementTypeLookup { get; } = [];

    /// <summary>
    /// 序列化和反序列化器
    /// </summary>
    public static XmlSerializer Serializer { get => field ??= new(typeof(Sequence)); }
}
public static class SequenceManager<T> where T : ISequenceElement
{
    public static Dictionary<string, Sequence> Sequences { get; } = [];
    private static bool _loaded;
    public static void Load()
    {
        if (_loaded) return;
        _loaded = true;

        #region 加载模组预设序列

        List<string> pendingLoadMods = [nameof(LogSpiralLibrary)];
        foreach (var localMod in ModDefinitionElement.locals)
            foreach (var refMod in localMod.properties.modReferences)
                if (refMod.mod == nameof(LogSpiralLibrary))
                {
                    pendingLoadMods.Add(localMod.Name);
                    break;
                }

        foreach (var modName in pendingLoadMods)
        {
            if (!ModLoader.TryGetMod(modName, out Mod mod))
                continue;

            foreach (var name in mod.GetFileNames())
            {
                if (!name.StartsWith($"PresetSequences/{typeof(T).Name}") || !name.EndsWith(".xml"))
                    continue;

                var fullName = $"{modName}/{Path.GetFileNameWithoutExtension(name)}";
                using MemoryStream stream = new(mod.GetFileBytes(name));
                RegisterSingleSequence(fullName, stream);
            }
        }
        #endregion

        #region 加载本地文件夹目录文件
        var path = $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{typeof(T).Name}";
        if (!Directory.Exists(path)) return;
        var list = SequenceLoadingHelper.GetAllFilesByDir(path);
        foreach (FileInfo file in list)
        {
            var modName = file.Directory.Name;
            if (!ModLoader.TryGetMod(modName, out var mod))
                continue;

            string fullName = $"{modName}/{Path.GetFileNameWithoutExtension(file.Name)}";
            using FileStream stream = new(file.FullName, FileMode.Open);
            RegisterSingleSequence(fullName, stream);
        }
        #endregion
    }

    public static Sequence RegisterSingleSequence(string fullName, Stream stream)
    {
        SequenceGlobalManager.UnloadSequences.Remove(fullName);
        if (!SequenceGlobalManager.SequenceLookup.TryGetValue(fullName, out Sequence sequence))
            sequence = new Sequence();

        try
        {
            var loadedSequence = (Sequence)SequenceGlobalManager.Serializer.Deserialize(stream);
            sequence.Groups = loadedSequence.Groups;
            Sequences[fullName] = sequence;
            SequenceGlobalManager.SequenceLookup[fullName] = sequence;
            return sequence;
        }
        catch(Exception e)
        {
            LogSpiralLibraryMod.Instance.Logger.Error($"Couldn't load sequence: {nameof(T)}-{fullName}\n Message as follows:{e.Message}");
            return null;
        }
    }

    public static void RegisterSingleSequence(string fullName, Sequence sequence) 
    {
        SequenceGlobalManager.UnloadSequences.Remove(fullName);
        Sequences[fullName] = sequence;
        SequenceGlobalManager.SequenceLookup[fullName] = sequence;
    }
}


file static class SequenceLoadingHelper 
{
    /// <summary>
    /// 获得指定目录下的所有文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static List<FileInfo> GetFilesByDir(string path)
    {
        DirectoryInfo di = new(path);

        //找到该目录下的文件
        FileInfo[] fi = di.GetFiles();

        //把FileInfo[]数组转换为List
        List<FileInfo> list = [.. fi];
        return list;
    }

    /// <summary>
    /// 获得指定目录及其子目录的所有文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static List<FileInfo> GetAllFilesByDir(string path)
    {
        DirectoryInfo dir = new(path);

        //找到该目录下的文件
        FileInfo[] fi = dir.GetFiles();

        //把FileInfo[]数组转换为List
        List<FileInfo> list = [.. fi];

        //找到该目录下的所有目录里的文件
        DirectoryInfo[] subDir = dir.GetDirectories();
        foreach (DirectoryInfo d in subDir)
        {
            List<FileInfo> subList = GetFilesByDir(d.FullName);
            foreach (FileInfo subFile in subList)
            {
                list.Add(subFile);
            }
        }
        return list;
    }
}
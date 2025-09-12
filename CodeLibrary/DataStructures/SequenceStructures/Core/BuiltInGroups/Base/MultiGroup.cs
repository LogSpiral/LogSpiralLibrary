using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Terraria.ModLoader;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;

public abstract class MultiGroup<T> : IGroup where T : class, IGroupArgument, new()
{
    public Type ArgType => typeof(T);

    public List<WrapperArgPair<T>> DataList { get; private set; } = [];

    public IReadOnlyList<IWrapperArgPair<IGroupArgument>> Contents => DataList;

    public void AppendWrapper(Wrapper wrapper, Dictionary<string, string> attributes)
    {
        T argument = new();
        argument.LoadAttributes(attributes);
        DataList.Add(new() { Wrapper = wrapper, Argument = argument });
    }

    private static readonly Dictionary<string, string> _attributeDict = [];

    public void WriteXml(XmlWriter writer)
    {

        var type = GetType();
        var mod = (MiscMethods.GetInstanceViaType(type) as MultiGroup<T>).Mod;
        var key = mod.Name == nameof(LogSpiralLibrary) ? type.Name : $"{mod.Name}/{type.Name}";

        writer.WriteAttributeString("FullName", key);

        foreach (var pair in DataList)
        {
            var (wrapper, argument) = pair.Deconstruct();
            _attributeDict.Clear();
            argument.WriteAttributes(_attributeDict);
            wrapper.WriteXml(writer, argument.IsHidden ? [] : _attributeDict);
        }
    }

    public abstract Wrapper GetWrapper();

    public IGroup Clone()
    {
        var result = MemberwiseClone() as MultiGroup<T>;
        result.DataList = [];
        foreach (var pair in DataList)
            result.DataList.
                Add(pair.Clone());

        return result;
    }

    void ILoadable.Load(Mod mod)
    {
        Mod = mod;
        var type = GetType();
        var key = mod.Name == nameof(LogSpiralLibrary) ? type.Name : $"{mod.Name}/{type.Name}";
        SequenceGlobalManager.GroupTypeLookup.Add(key, type);
        SequenceGlobalManager.MultiGroupTypeLookup.Add(key, type);
    }
    private Mod Mod { get; set; }
}
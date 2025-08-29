using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;

public abstract class SingleGroup<T> : IGroup where T : class, IGroupArgument, new()
{
    public Type ArgType => typeof(T);

    public WrapperArgPair<T> Data { get; set; }

    public IReadOnlyList<IWrapperArgPair<IGroupArgument>> Contents => [Data];

    public bool ReadSingleWrapper => true;

    public void AppendWrapper(Wrapper wrapper, Dictionary<string, string> attributes)
    {
        attributes.Remove("SingleGroupFullName");
        T argument = new();
        argument.LoadAttributes(attributes);
        Data = new() { Wrapper = wrapper, Argument = argument };
    }

    private static readonly Dictionary<string, string> _attributeDict = [];

    public void WriteXml(XmlWriter writer)
    {
        _attributeDict.Clear();

        var type = GetType();
        var mod = (MiscMethods.GetInstanceViaType(type) as SingleGroup<T>).Mod;
        var key = mod.Name == nameof(LogSpiralLibrary) ? type.Name : $"{mod.Name}/{type.Name}";

        _attributeDict["SingleGroupFullName"] =  key;

        var (wrapper, argument) = Data.Deconstruct();
        argument.WriteAttributes(_attributeDict);
        wrapper.WriteXml(writer, argument.IsHidden ? [] : _attributeDict);
    }

    public abstract Wrapper GetWrapper();

    public IGroup Clone()
    {
        var result = MemberwiseClone() as SingleGroup<T>;
        result.Data = Data.Clone();
        return result;
    }

    void ILoadable.Load(Mod mod)
    {
        Mod = mod;
        var type = GetType();
        var key = mod.Name == nameof(LogSpiralLibrary) ? type.Name : $"{mod.Name}/{type.Name}";
        SequenceGlobalManager.GroupTypeLookup.Add(key, type);
        SequenceGlobalManager.SingleGroupTypeLookup.Add(key, type);
    }
    private Mod Mod { get; set; }

}
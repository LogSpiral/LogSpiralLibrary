using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;
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
        T argument = new();
        argument.LoadAttributes(attributes);
        Data = new() { Wrapper = wrapper, Argument = argument };
    }

    private static readonly Dictionary<string, string> _attributeDict = [];

    public void WriteXml(XmlWriter writer)
    {
        _attributeDict.Clear();
        writer.WriteAttributeString("FullName", GetType().Name);

        var (wrapper, argument) = Data.Deconstruct();
        argument.WriteAttributes(_attributeDict);
        wrapper.WriteXml(writer, argument.IsHidden ? [] : _attributeDict);
    }

    public abstract Wrapper GetWrapper();
}
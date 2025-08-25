using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;

public abstract class MultiGroup<T> : IGroup where T : class, IGroupArgument, new()
{
    public Type ArgType => typeof(T);

    public List<WrapperArgPair<T>> DataList { get; } = [];

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
        _attributeDict.Clear();
        writer.WriteAttributeString("FullName", GetType().Name);

        foreach (var pair in DataList)
        {
            var (wrapper, argument) = pair.Deconstruct();
            argument.WriteAttributes(_attributeDict);
            wrapper.WriteXml(writer, argument.IsHidden ? [] : _attributeDict);
        }
    }

    public abstract Wrapper GetWrapper();
}
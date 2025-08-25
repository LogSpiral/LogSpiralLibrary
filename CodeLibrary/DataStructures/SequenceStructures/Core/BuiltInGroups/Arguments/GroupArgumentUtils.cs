using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;

public static class GroupArgumentUtils
{
    private static readonly Dictionary<string, string> _attributeDict = [];

    public static TResult ConvertArgument<TSource, TResult>(this TSource source) where TSource : IGroupArgument where TResult : IGroupArgument, new()
    {
        _attributeDict.Clear();
        source.WriteAttributes(_attributeDict);
        var result = new TResult();
        result.LoadAttributes(_attributeDict);
        return result;
    }

    public static IGroupArgument ConvertArgument(IGroupArgument source, Type resultType)
    {
        _attributeDict.Clear();
        var result = Activator.CreateInstance(resultType) as IGroupArgument;
        source.WriteAttributes(_attributeDict);
        result.LoadAttributes(_attributeDict);
        return result;
    }
}
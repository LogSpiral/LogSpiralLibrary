using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;

public class ConditionalGroup : IGroup
{
    public List<(Wrapper wrapper, Condition condition, string conditionName)> DataList { get; } = [];

    public Wrapper GetWrapper()
    {
        Wrapper result = null;
        foreach (var pair in DataList)
        {
            if ((pair.condition?.IsMet() ?? false) && pair.wrapper != null)
            {
                result = pair.wrapper;
                break;
            }
        }
        return result;
    }

    public void AppendWrapper(Wrapper wrapper, Dictionary<string, string> attributes)
    {
        if (!attributes.Remove("condition", out string conditionName) || conditionName == null || !SequenceSystem.Conditions.TryGetValue(conditionName, out var condition))
            condition = SequenceSystem.AlwaysCondition;
        DataList.Add((wrapper, condition, conditionName));
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("FullName", nameof(ConditionalGroup));

        foreach (var (wrapper, condition, conditionName) in DataList)
        {
            wrapper.WriteXml(writer, conditionName == null ? [] : new Dictionary<string, string>() { { "condition", conditionName } });
        }
    }
}

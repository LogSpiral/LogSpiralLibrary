using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;

public class ConditionalSingleGroup(Wrapper wrapper, Condition condition, string conditionName) : IGroup
{
    public ConditionalSingleGroup() : this(null!, null!, null!)
    {

    }
    Wrapper IGroup.GetWrapper() => (_condition?.IsMet() ?? false) ? _wrapper : null;
    public bool ReadSingleWrapper => true;
    private Wrapper _wrapper = wrapper;
    private string _conditionName = conditionName;
    private Condition _condition = condition;
    public void AppendWrapper(Wrapper wrapper, Dictionary<string, string> attributes)
    {
        _wrapper = wrapper;
        if (!attributes.Remove("condition", out string conditionName) || !SequenceSystem.Conditions.TryGetValue(conditionName, out var condition))
            condition = SequenceSystem.AlwaysCondition;
        _conditionName = conditionName;
        _condition = condition;
    }

    public void WriteXml(XmlWriter writer)
    {
        _wrapper.WriteXml(writer, new Dictionary<string, string>() { { "condition", _conditionName } });
    }
}

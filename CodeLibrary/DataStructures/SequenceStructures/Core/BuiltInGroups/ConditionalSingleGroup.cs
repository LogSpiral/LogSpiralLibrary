using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;

public class ConditionalSingleGroup() : SingleGroup<ConditionArg>
{
    public override Wrapper GetWrapper() => Data.Argument.Condition.IsMet() ? Data.Wrapper : null;

    public ConditionalSingleGroup(Wrapper wrapper, string conditionKey) : this()
    {
        Data = new()
        {
            Wrapper = wrapper,
            Argument = new(conditionKey)
        };
    }
}
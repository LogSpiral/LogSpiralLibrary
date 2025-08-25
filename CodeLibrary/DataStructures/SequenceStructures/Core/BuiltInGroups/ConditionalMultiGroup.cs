using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;

public class ConditionalMultiGroup : MultiGroup<ConditionArg>
{
    public override Wrapper GetWrapper()
    {
        Wrapper result = null;
        foreach (var pair in DataList)
        {
            var (wrapper, argument) = pair.Deconstruct();
            if (argument.Condition.IsMet() && wrapper != null)
            {
                result = wrapper;
                break;
            }
        }
        return result;
    }
}
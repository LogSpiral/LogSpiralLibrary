using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;

public class SingleWrapperGroup() : SingleGroup<NoneArg>
{
    public override Wrapper GetWrapper() => Data.Wrapper;

    public SingleWrapperGroup(Wrapper wrapper) : this()
    {
        Data = new()
        {
            Wrapper = wrapper,
            Argument = new()
        };
    }
}
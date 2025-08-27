using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;

public interface IWrapperArgPair<out T> where T : IGroupArgument
{
    public T Argument { get; }
    public Wrapper Wrapper { get; }
}

public sealed class WrapperArgPair<T>() : IWrapperArgPair<T> where T : IGroupArgument
{
    public T Argument { get; set; }

    public Wrapper Wrapper { get; set; }

    public (Wrapper, T) Deconstruct() => (Wrapper, Argument);

    public WrapperArgPair ToNonGeneric() => new() { Argument = Argument, Wrapper = Wrapper };

    public WrapperArgPair<T> Clone() => new() { Argument = (T)Argument.Clone(), Wrapper = Wrapper.Clone() };
}

public class WrapperArgPair 
{
    public IGroupArgument Argument { get; set; }
    public Wrapper Wrapper { get; set; }
}
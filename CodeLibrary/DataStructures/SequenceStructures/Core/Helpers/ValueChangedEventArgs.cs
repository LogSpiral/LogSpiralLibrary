namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;

public class ValueChangedEventArgs<T>(T oldValue, T newValue) : EventArgs
{
    public T OldValue { get; } = oldValue;
    public T NewValue { get; } = newValue;
}
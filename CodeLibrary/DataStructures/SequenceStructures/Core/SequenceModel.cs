using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

public class SequenceModel
{
    private sealed class SequenceData(ISequence sequence)
    {
        public ISequence Sequence { get; init; } = sequence;
        public int Index { get; set; }
        public bool IsCompleted => Index >= Sequence.Count;

        public Wrapper GetNextAvailableWrapper()
        {
            Wrapper result = null;
            while (!IsCompleted && (result == null || !result.Available))
                result = Sequence.GetWrapperAt(Index++);
            return result;
        }
    }

    private readonly Stack<SequenceData> SequenceStack = [];

    public SequenceModel(ISequence sequence)
    {
        SequenceStack.Push(new SequenceData(sequence));
    }

    public void Update()
    {
        if (IsCompleted) return;
        if (CurrentElement == null || CurrentElement.IsCompleted)
        {
        label:
            Wrapper wrapper = null;
            int counter = 0;
            while (wrapper == null && counter < 1000)
            {
                while (SequenceStack.Peek().IsCompleted)
                {
                    if (SequenceStack.Count > 1)
                        SequenceStack.Pop();
                    else
                    {
                        SequenceStack.Peek().Index = 0;
                        IsCompleted = true;
                        return;
                    }
                }
                wrapper = SequenceStack.Peek().GetNextAvailableWrapper();
            }
            if (counter == 1000 && wrapper == null)
                throw new Exception("Too Deep");

            if (wrapper != null)
            {
                if (wrapper.Sequence != null)
                {
                    SequenceStack.Push(new SequenceData(wrapper.Sequence));
                    goto label;
                }
                else if (wrapper.Element != null)
                {
                    var oldElement = CurrentElement;
                    var element = CurrentElement = wrapper.Element.CloneInstance();
                    OnElementChanged?.Invoke(this, new ValueChangedEventArgs<ISequenceElement>(oldElement, element));
                    OnInitializeElement?.Invoke(element);
                    element.Initialize();
                }
            }
        }
        CurrentElement?.Update();
    }

    public event Action<ISequenceElement> OnInitializeElement;

    public event EventHandler<ValueChangedEventArgs<ISequenceElement>> OnElementChanged;

    public ISequenceElement CurrentElement { get; private set; }

    public bool IsCompleted { get; set; }
}
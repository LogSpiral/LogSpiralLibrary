using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;

public static class SequenceCheckHelper
{
    public static void FillUnloadWrapperList(Sequence sequence, List<Wrapper> wrappers)
    {
        foreach (var group in sequence.Groups) 
        {
            foreach (var pair in group.Contents) 
            {
                var wrapper = pair.Wrapper;
                if (wrapper.Sequence is Sequence { } subSequence) 
                {
                    if (wrapper.RefSequenceFullName == null)
                        FillUnloadWrapperList(subSequence, wrappers);
                    else if (SequenceGlobalManager.UnloadSequences.Contains(wrapper.RefSequenceFullName))
                        wrappers.Add(wrapper);
                }
            }
        }
    }

    public static List<Wrapper> GetUnloadWrapperList(Sequence sequence) 
    {
        List<Wrapper> result = [];
        FillUnloadWrapperList(sequence, result);
        return result;
    }
}

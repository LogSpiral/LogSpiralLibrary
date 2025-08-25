using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;

public class NoneArg : IGroupArgument
{
    public bool IsHidden => true;

    public void LoadAttributes(Dictionary<string, string> attributes)
    {
    }

    public void SetDefault()
    {
    }

    public void WriteAttributes(Dictionary<string, string> attributes)
    {
    }
}
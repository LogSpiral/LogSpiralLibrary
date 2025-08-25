using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;

public interface IGroupArgument
{
    bool IsHidden { get; }

    string ToString();

    void SetDefault();

    void LoadAttributes(Dictionary<string, string> attributes);

    void WriteAttributes(Dictionary<string, string> attributes);
}
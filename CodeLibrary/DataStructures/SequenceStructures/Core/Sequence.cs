using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

public partial class Sequence(params List<IGroup> groups) : ISequence
{
    public Sequence() : this([])
    {
    }

    public SequenceData? Data { get; set; }

    public List<IGroup> Groups { get; set; } = groups;

    int ISequence.Count => Groups.Count;

    Wrapper ISequence.GetWrapperAt(int index) => Groups[index].GetWrapper();


    public Sequence Clone() 
    {
        var result = new Sequence()
        {
            Data = Data.Clone()
        };
        foreach (var group in Groups) 
            result.Groups.Add(group.Clone());
        return result;
    }
}
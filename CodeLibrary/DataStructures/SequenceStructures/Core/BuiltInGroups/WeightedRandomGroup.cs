using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Base;
using Terraria.Utilities;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;

public class WeightedRandomGroup : MultiGroup<WeightArg>
{
    public override Wrapper GetWrapper()
    {
        var rand = new WeightedRandom<Wrapper>(Main.rand);
        foreach (var pair in DataList)
            rand.Add(pair.Wrapper, pair.Argument.Weight);
        return rand.Get();
    }
}
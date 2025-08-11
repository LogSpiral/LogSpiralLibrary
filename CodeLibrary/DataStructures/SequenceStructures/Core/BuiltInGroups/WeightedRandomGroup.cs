using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;
using System.Xml;
using Terraria.Utilities;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;
public class WeightedRandomGroup : IGroup
{
    #region Core
    public List<(Wrapper wrapper, float weight)> DataList { get; } = [];
    public Wrapper GetWrapper() => WeightedRandom(DataList, Main.rand);

    public static T WeightedRandom<T>(IList<(T content, float weight)> dataList, UnifiedRandom random)
    {
        int count = dataList.Count;
        if (count == 1) return dataList[0].content;

        List<float> sumList = [];
        for (int n = 0; n < count; n++)
        {
            float c = dataList[n].weight;
            if (n > 0)
                c += sumList[n - 1];
            sumList.Add(c);
        }
        float randValue = random.NextFloat() * sumList[^1];

        int index = 0;
        for (int n = count - 2; n >= 0; n--)
        {
            if (randValue > sumList[n])
            {
                index = n + 1;
                break;
            }
        }
        return dataList[index].content;
    }

    #endregion

    #region IO
    public void AppendWrapper(Wrapper wrapper, Dictionary<string, string> attributes)
    {
        if (!attributes.Remove("weight", out string? weight))
            weight = "1";
        DataList.Add((wrapper, float.Parse(weight)));
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("FullName", nameof(WeightedRandomGroup));


        foreach (var (wrapper, weight) in DataList)
        {
            wrapper.WriteXml(writer, weight == 1 ? [] : new Dictionary<string, string>() { { "weight", weight.ToString() } });
        }
    }
    #endregion
}
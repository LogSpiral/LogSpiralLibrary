using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Unloads;

public class UnloadData : SequenceData,ILoadable
{
    public string FullName { private get; set; }
    private List<XElement> ExtraElements { get; } = [];
    protected override void Load(XmlElementReader elementReader)
    {
        ExtraElements.AddRange(elementReader.GetElements());
    }
    protected override void Save(XmlWriter writer)
    {
        foreach (var element in ExtraElements)
            element.WriteTo(writer);
    }
    public override string GetFullName => FullName!;

    void ILoadable.Load(Mod mod) 
    {

    }
}

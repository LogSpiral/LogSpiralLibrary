using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;

public interface ISequenceElement
{
    void Initialize();

    void Update();

    bool IsCompleted { get; }

    ISequenceElement CloneInstance();

    void ReadXml(XmlReader reader)
    {
        SequenceElementIOHelper.LoadElements(this, reader);
    }

    void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("FullName", $"{(MiscMethods.GetInstanceViaType(GetType()) as ISequenceElement).Mod.Name}/{GetType().Name}");
        SequenceElementIOHelper.SaveElements(this, writer);
    }

    Mod Mod { get; }
    // public void ReadAttributes(IReadOnlyDictionary<string, string> attributes);
}
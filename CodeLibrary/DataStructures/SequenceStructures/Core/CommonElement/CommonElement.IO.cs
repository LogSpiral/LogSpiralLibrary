using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.CommonElement;

public partial class CommonElement
{
    void ISequenceElement.ReadXml(XmlReader reader)
    {
        SequenceElementIOHelper.LoadElements(this, reader);
    }

    void ISequenceElement.WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("FullName", $"{Mod.Name}/{GetType().Name}");
        SequenceElementIOHelper.SaveElements(this, writer);
    }
}
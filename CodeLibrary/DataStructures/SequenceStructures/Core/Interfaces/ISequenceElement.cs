using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;

public interface ISequenceElement
{
    void Initialize();

    void Update();

    bool IsCompleted { get; }

    ISequenceElement CloneInstance();

    void ReadXml(XmlReader reader);

    void WriteXml(XmlWriter writer);
}
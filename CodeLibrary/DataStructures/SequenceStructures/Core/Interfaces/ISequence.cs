using System.Xml.Serialization;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;

public interface ISequence : IXmlSerializable
{
    int Count { get; }

    Wrapper GetWrapperAt(int index);
}
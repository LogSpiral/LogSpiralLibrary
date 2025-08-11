using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using System.Collections.Generic;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;

public interface ISequenceElement
{
    void Initialize();

    void Update();

    bool IsCompleted { get; }

    void ReadXml(XmlReader reader) 
    {
        SequenceElementIOHelper.LoadElements(this, reader);
    }

    void WriteXml(XmlWriter writer) 
    {
        SequenceElementIOHelper.SaveElements(this, writer);
    }

    // public void ReadAttributes(IReadOnlyDictionary<string, string> attributes);
}


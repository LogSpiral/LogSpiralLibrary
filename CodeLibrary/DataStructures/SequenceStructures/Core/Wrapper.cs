using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

public partial class Wrapper
{
    public ISequence? Sequence { get; init; }

    public ISequenceElement? Element { get; init; }

    public string? RefSequenceFullName { get; set; }

    public bool Available 
    {
        get 
        {
            if (RefSequenceFullName != null)
                return !SequenceGlobalManager.UnloadSequences.Contains(RefSequenceFullName);
            return Element != null || Sequence != null;
        }
    }

    public Wrapper(ISequence sequence)
    {
        Sequence = sequence;
    }

    public Wrapper(ISequenceElement element)
    {
        Element = element;
    }

    public Wrapper(string refSequenceFullName)
    {
        RefSequenceFullName = refSequenceFullName;
        if (!SequenceGlobalManager.SequenceLookup.TryGetValue(refSequenceFullName, out Sequence sequence))
        {
            SequenceGlobalManager.UnloadSequences.Add(refSequenceFullName);
            SequenceGlobalManager.SequenceLookup[refSequenceFullName] = sequence = new Sequence();
        }
        Sequence = sequence;
    }

    public void ReadAttributes(IReadOnlyDictionary<string, string> attributes)
    {
        Attributes = [];
        foreach (var (key, value) in attributes)
            Attributes.Add(key, value);
    }

    public void WriteXml(XmlWriter writer, IReadOnlyDictionary<string, string> attributes)
    {
        writer.WriteStartElement(Sequence != null ? "Sequence" : "Element");

        if (attributes != null)
            foreach (var (key, value) in attributes)
                writer.WriteAttributeString(key, value);

        if (Sequence != null)
        {
            if (RefSequenceFullName != null)
                writer.WriteAttributeString("FullName", RefSequenceFullName);
            else
                Sequence.WriteXml(writer);
        }
        else if (Element != null)
            Element.WriteXml(writer);
        else
            WriteUnloadData(writer);

        writer.WriteEndElement();
    }
}
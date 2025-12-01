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
        if (string.IsNullOrEmpty(refSequenceFullName)) return;
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
        if (Sequence == null)
            writer.WriteStartElement("Element");
        else if (Sequence.Count != 1 || RefSequenceFullName != null)
            writer.WriteStartElement("Sequence");
        else
            writer.WriteStartElement("Group");
        // 只有一个Group的时候Sequence省略，直接只写一个Group元素，但是这样就没法将特性写给Group
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

        // if(Sequence is not { Count: 1 } || RefSequenceFullName != null)
        writer.WriteEndElement();
    }


    public Wrapper Clone()
    {
        if (Element is { } element)
            return new Wrapper(element.CloneInstance());
        if (RefSequenceFullName is { } refName)
            return new Wrapper(refName);
        if (Sequence is { } sequence && sequence is Sequence standardSequence)
            return new Wrapper(standardSequence.Clone());
        if (IsUnload)
            return new Wrapper(ExtraElements, Attributes);
        return null;
    }
}
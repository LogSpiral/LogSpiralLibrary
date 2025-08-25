using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;

public class XmlElementReader
{
    private Dictionary<string, XElement> Elements { get; } = [];

    // Dictionary<string, List<XElement>> MultiElements { get; } = [];
    private readonly bool _popMode;

    public XmlElementReader(XmlReader reader, bool popMode = true)
    {
        while (reader.NodeType == XmlNodeType.Element)
        {
            using (var subtree = reader.ReadSubtree())
            {
                var element = XElement.Load(subtree);
                Elements[element.Name.LocalName] = element;
                // if (!MultiElements.TryGetValue(element.Name.LocalName, out var list))
                //     MultiElements[element.Name.LocalName] = list = [];
                // list.Add(element);
            }
            reader.Read();
        }

        _popMode = popMode;
    }

    public XElement this[string name]
    {
        get
        {
            if (_popMode)
                return Elements.Remove(name, out var result) ? result : null;
            else
                return Elements.TryGetValue(name, out var result) ? result : null;
        }
    }

    public List<XElement> GetElements()
    {
        List<XElement> result = [];
        foreach (var element in Elements.Values)
            result.Add(element);
        if (_popMode) Elements.Clear();
        return result;
    }

    //public List<XElement> GetMultiElements(string name)
    //{
    //    if (_popMode)
    //        return MultiElements.Remove(name, out var result) ? result : null;
    //    else
    //        return MultiElements.TryGetValue(name, out var result) ? result : null;
    //}
}
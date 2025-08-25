using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

public partial class Wrapper
{
    private List<XElement>? ExtraElements;
    private Dictionary<string, string>? Attributes;
    public bool IsUnload => ExtraElements != null;
    private void ReadUnloadData(XmlReader reader)
    {
        ExtraElements = [];
        while (reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                // 使用子树读取完整元素
                using (var subtree = reader.ReadSubtree())
                {
                    var element = XElement.Load(subtree);
                    ExtraElements.Add(element);
                }
                reader.Read(); // 移动到下一个节点
            }
            else
            {
                reader.Read(); // 跳过非元素节点
            }
        }
    }

    private void WriteUnloadData(XmlWriter writer)
    {
        if (Attributes != null)
            foreach (var (key, value) in Attributes)
                writer.WriteAttributeString(key, value);

        if (ExtraElements != null)
            foreach (var element in ExtraElements)
            {
                element.WriteTo(writer);
            }
    }

    public Wrapper(XmlReader reader)
    {
        if (!reader.IsEmptyElement)
        {
            reader.ReadStartElement("Element");
            ReadUnloadData(reader);
            reader.ReadEndElement();
        }
        else 
        {
            reader.Read();
            ExtraElements = [];
        }
    }
}
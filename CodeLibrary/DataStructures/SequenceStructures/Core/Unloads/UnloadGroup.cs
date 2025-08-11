using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Unloads;

public class UnloadGroup : IGroup
{
    #region Core
    static readonly Wrapper NotAvailableInstance = new(default(ISequenceElement)!);
    Wrapper IGroup.GetWrapper() => NotAvailableInstance;
    #endregion

    #region IO
    public string FullName { private get; set; }
    private List<XElement>? ExtraElements;

    public void ReadXml(XmlReader reader)
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

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("FullName", FullName);

        if (ExtraElements != null)
            foreach (var element in ExtraElements)
            {
                element.WriteTo(writer);
            }
    }

    public void AppendWrapper(Wrapper wrapper, Dictionary<string, string> attributes)
    {
        throw new NotImplementedException();
    }
    #endregion


    public void Load(Mod mod) 
    {

    }
}

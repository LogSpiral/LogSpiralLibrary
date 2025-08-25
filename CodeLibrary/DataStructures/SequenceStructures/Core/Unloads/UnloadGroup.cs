using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Unloads;

public class UnloadGroup : IGroup
{
    #region Core

    private static readonly Wrapper NotAvailableInstance = new(default(ISequenceElement)!);

    Wrapper IGroup.GetWrapper() => NotAvailableInstance;

    #endregion Core

    #region IO

    public string FullName { private get; set; }

    public Type ArgType => null;

    public IReadOnlyList<IWrapperArgPair<IGroupArgument>> Contents => [];

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

    #endregion IO

    public void Load(Mod mod)
    {
    }
}
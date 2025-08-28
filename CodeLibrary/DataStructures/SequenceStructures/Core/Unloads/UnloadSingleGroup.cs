using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Unloads;

public class UnloadSingleGroup : IGroup
{
    Type IGroup.ArgType => null;
    public bool ReadSingleWrapper => true;

    IReadOnlyList<IWrapperArgPair<IGroupArgument>> IGroup.Contents => [];

    private XElement UnloadElementData { get; set; }

    public void ReadXml(XmlReader reader)
    {
        if (reader.NodeType == XmlNodeType.Element)
        {
            // 使用子树读取完整元素
            using (var subtree = reader.ReadSubtree())
            {
                var element = XElement.Load(subtree);
                UnloadElementData = element;
            }
            reader.Read(); // 移动到下一个节点
        }
        else
        {
            reader.Read(); // 跳过非元素节点
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        UnloadElementData?.WriteTo(writer);
    }

    public void AppendWrapper(Wrapper wrapper, Dictionary<string, string> attributes)
    {
        throw new NotImplementedException();
    }

    public void Load(Mod mod)
    {
    }

    IGroup IGroup.Clone() => this;

    private static readonly Wrapper NotAvailableInstance = new(default(ISequenceElement)!);
    Wrapper IGroup.GetWrapper() => NotAvailableInstance;
}

using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.Xml;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;

public interface IGroup : ILoadable
{
    Type? ArgType { get; }

    IReadOnlyList<IWrapperArgPair<IGroupArgument>> Contents { get; }

    Wrapper GetWrapper();

    void AppendWrapper(Wrapper wrapper, Dictionary<string, string> attributes);

    public bool ReadSingleWrapper => false;

    public void ReadXml(XmlReader reader)
    {
        while (reader.IsStartElement("Element") || reader.IsStartElement("Sequence"))
        {
            bool isSequence = reader.IsStartElement("Sequence");
            bool isEmptyElement = reader.IsEmptyElement;
            string fullName = reader["FullName"]!;
            Dictionary<string, string> attributes = [];
            if (reader.HasAttributes)
            {
                // 移动到第一个属性
                reader.MoveToFirstAttribute();

                do
                {
                    // 添加属性到字典
                    attributes[reader.Name] = reader.Value;
                } while (reader.MoveToNextAttribute());

                // 移回元素节点
                reader.MoveToElement();
            }

            if (isSequence)
            {
                if (fullName != null)
                {
                    Wrapper wrapper = new(fullName);
                    reader.Read();
                    AppendWrapper(wrapper, attributes);
                }
                else
                {
                    Sequence sequence = new();
                    sequence.ReadXml(reader);
                    AppendWrapper(new Wrapper(sequence), attributes);
                }
            }
            else
            {
                if (!SequenceGlobalManager.ElementTypeLookup.TryGetValue(fullName, out Type type))
                {
                    Wrapper unloadWrapper = new(reader);
                    AppendWrapper(unloadWrapper, attributes);
                    unloadWrapper.ReadAttributes(attributes);
                }
                else
                {
                    ISequenceElement element = (ISequenceElement)Activator.CreateInstance(type)!;
                    if (!isEmptyElement)
                    {
                        reader.ReadStartElement("Element");
                        element.ReadXml(reader);
                        reader.ReadEndElement();
                    }
                    else
                        reader.Read();
                    AppendWrapper(new Wrapper(element), attributes);
                    // element.ReadAttributes(attributes);
                }
            }
            if (ReadSingleWrapper)
                break;
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("FullName", GetType().Name);
    }

    void ILoadable.Load(Mod mod)
    {
        if (ReadSingleWrapper) return;
        var type = GetType();
        var key = mod.Name == nameof(LogSpiralLibrary) ? type.Name : $"{mod.Name}/{type.Name}";
        SequenceGlobalManager.GroupTypeLookup.Add(key, type);
    }

    void ILoadable.Unload()
    {
    }

    IGroup Clone();
}
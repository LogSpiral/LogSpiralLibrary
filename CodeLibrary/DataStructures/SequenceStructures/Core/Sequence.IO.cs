using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Unloads;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

public partial class Sequence
{
    XmlSchema? IXmlSerializable.GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        if (reader["FileName"] is not null) // 包含旧版Attribute, 改用旧版读取方式
        {
            ReadSequenceOld(reader);
        }
        else
        {
            reader.ReadStartElement("Sequence");
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.LocalName)
                    {
                        case "Element":
                        case "Sequence":
                            {
                                ParseSingle(reader);
                                break;
                            }
                        case "Group":
                            {
                                ParseGroup(reader);
                                break;
                            }
                        case "Data":
                            {
                                ParseData(reader);
                                break;
                            }
                    }
                }
                else
                {
                    reader.Read(); // 跳过非元素节点
                }
            }

            reader.ReadEndElement();
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        if (Data != null)
        {
            writer.WriteStartElement("Data");
            Data.WriteXml(writer);
            writer.WriteEndElement();
        }

        foreach (var group in Groups)
        {
            bool single = group.ReadSingleWrapper || group.Contents.Count == 1;
            if (!single)
                writer.WriteStartElement("Group");
            group.WriteXml(writer);
            if (!single)
                writer.WriteEndElement();
        }
    }

    private void ParseSingle(XmlReader reader)
    {
        IGroup group;
        if (reader["SingleGroupFullName"] is { } fullname)
        {
            if (!SequenceGlobalManager.SingleGroupTypeLookup.TryGetValue(fullname, out Type type))
                type = typeof(UnloadSingleGroup);
            group = (IGroup)Activator.CreateInstance(type)!;
        }
        else
        {
            if (reader["condition"] != null)
                group = new ConditionalSingleGroup();
            else
                group = new SingleWrapperGroup();
        }

        group.ReadXml(reader);
        Groups.Add(group);
    }

    private void ParseGroup(XmlReader reader)
    {
        if (ParseGroupToInstance(reader) is { } group)
            Groups.Add(group);
    }

    public static IGroup ParseGroupToInstance(XmlReader reader)
    {
        if (reader.IsEmptyElement) return null;

        string fullName = reader["FullName"]!;
        if (!SequenceGlobalManager.GroupTypeLookup.TryGetValue(fullName, out Type type) && !SequenceGlobalManager.GroupTypeLookup.TryGetValue(fullName, out type))
            type = typeof(UnloadGroup);

        IGroup group = (IGroup)Activator.CreateInstance(type)!;

        reader.ReadStartElement("Group");
        group.ReadXml(reader);
        reader.ReadEndElement();

        if (group is UnloadGroup unload)
            unload.FullName = fullName;
        return group;
    }

    private void ParseData(XmlReader reader)
    {
        if (reader.IsEmptyElement) return;

        string? fullName = reader["FullName"];
        Type? type;
        if (fullName is null)
            type = typeof(SequenceData);
        else if (!SequenceGlobalManager.DataTypeLookup.TryGetValue(fullName, out type))
            type = typeof(UnloadData);

        SequenceData data = (SequenceData)Activator.CreateInstance(type ?? typeof(SequenceData))!;

        if (!reader.IsEmptyElement)
        {
            reader.ReadStartElement("Data");
            data.ReadXml(reader);
            reader.ReadEndElement();
        }
        else
        {
            reader.Read();
        }

        if (data is UnloadData unload)
            unload.FullName = fullName;

        Data = data;
    }

    private void ReadSequenceOld(XmlReader reader)
    {
        if (reader.Depth == 0)
            ReadDataOld(reader);

        reader.ReadStartElement("Sequence");

        while (reader.IsStartElement("Group"))
            ReadGroupOld(reader);

        reader.ReadEndElement();
    }

    private void ReadDataOld(XmlReader reader)
    {
        var data = new SequenceData
        {
            AuthorName = reader["AuthorName"] ?? "",
            DisplayName = reader["DisplayName"] ?? "",
            FileName = reader["FileName"] ?? "",
            Description = reader["Description"] ?? "",
            CreateTime = new DateTime(long.TryParse(reader["createTime"] ?? "0", out var tick) ? tick : 0),
            ModifyTime = new DateTime(long.TryParse(reader["lastModifyTime"] ?? "0", out tick) ? tick : 0),
            Finished = !bool.TryParse(reader["Finished"] ?? "True", out var finished) || finished
        };
        Data = data;
    }

    private void ReadGroupOld(XmlReader reader)
    {
        reader.ReadStartElement("Group");

        List<(Wrapper wrapper, string conditionKey)> Datas = [];
        while (reader.IsStartElement("Wraper"))
        {
            string condition = reader["condition"];
            var modName = reader["Mod"];
            if (modName != null)
            {
                // 为了适应新的序列引用键格式，手动补了MeleeAction
                // 虽然引用起来比之前麻烦了但是毕竟有UI编辑
                var fullName = $"{modName}/{nameof(MeleeAction)}/{reader.ReadElementContentAsString()}";
                Datas.Add((new Wrapper(fullName), condition));
            }
            else
            {
                reader.ReadStartElement("Wraper");
                Datas.Add((ReadWrapperOld(reader), condition));
                reader.ReadEndElement();
            }
        }
        if (Datas.Count == 1)
        {
            var (wrapper, conditionKey) = Datas[0];
            if (conditionKey is null)
                Groups.Add(new SingleWrapperGroup(wrapper));
            else
                Groups.Add(new ConditionalSingleGroup(wrapper, conditionKey));
        }
        else
        {
            var group = new ConditionalMultiGroup();
            foreach (var (wrapper, conditionKey) in Datas)
                group.DataList.Add(new WrapperArgPair<ConditionArg> { Wrapper = wrapper, Argument = new ConditionArg(conditionKey) });
            Groups.Add(group);
        }

        reader.ReadEndElement(); // 关闭 Member
    }

    private static Wrapper ReadWrapperOld(XmlReader reader)
    {
        if (reader.IsStartElement("Action"))
        {
            if (reader.IsEmptyElement)
            {
                if (!SequenceGlobalManager.ElementTypeLookup.TryGetValue(reader["name"], out var type))
                    return new Wrapper(reader);

                var element = Activator.CreateInstance(type);

                #region 自动读取特性

                var props = type.GetProperties();
                foreach (var prop in props)
                {
                    if (prop.GetCustomAttribute<ElementCustomDataAttribute>() != null && prop.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null)
                    {
                        var propName = prop.Name;
                        if (propName == "CounterMax")
                            propName = "Cycle";
                        if (reader[propName] is string content && content.Length != 0)
                        {
                            object dummy = prop.GetValue(element);
                            object value = dummy switch
                            {
                                int => int.Parse(content),
                                float => float.Parse(content),
                                double => double.Parse(content),
                                bool => bool.Parse(content),
                                byte => byte.Parse(content),
                                ActionModifyData => ActionModifyData.LoadFromString(content),
                                SequenceDelegateDefinition => new SequenceDelegateDefinition(content),
                                _ => null
                            };
                            if (value == null && prop.PropertyType.IsEnum)
                                value = Enum.GetValues(prop.PropertyType).GetValue(int.Parse(content));
                            if (value != null)
                                prop.SetValue(element, value);
                        }
                        else if (prop.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute)
                            prop.SetValue(element, defaultValueAttribute.Value);
                    }
                }
                foreach (var fld in type.GetFields())
                {
                    if (fld.GetCustomAttribute<ElementCustomDataAttribute>() != null && fld.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null)
                    {
                        if (reader[fld.Name] is string content && content.Length != 0)
                        {
                            object dummy = fld.GetValue(element);

                            object value = dummy switch
                            {
                                int => int.Parse(content),
                                float => float.Parse(content),
                                double => double.Parse(content),
                                bool => bool.Parse(content),
                                byte => byte.Parse(content),
                                ActionModifyData => ActionModifyData.LoadFromString(content),
                                SequenceDelegateDefinition => new SequenceDelegateDefinition(content),
                                _ => null
                            };
                            if (value == null && fld.FieldType.IsEnum)
                                value = Enum.GetValues(fld.FieldType).GetValue(int.Parse(content));
                            if (value != null)
                                fld.SetValue(element, value);
                        }
                        else if (fld.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute)
                            fld.SetValue(element, defaultValueAttribute.Value);
                    }
                }

                #endregion 自动读取特性

                reader.Read();
                return new Wrapper((ISequenceElement)element);
            }
            else
            {
                reader.Skip();
                return null; // 为什么旧版会出现非空标签写法啊
            }
        }
        else if (reader.IsStartElement("Sequence"))
        {
            var sequence = new Sequence();
            sequence.ReadSequenceOld(reader);
            return new Wrapper(sequence);
        }
        else
            return null;
    }
}
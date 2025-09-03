using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;

public static class SequenceElementIOHelper
{
    private static T GetCustomAttribute<T>(this PropertyFieldWrapper variableInfo) where T : Attribute
    {
        return variableInfo.propertyInfo?.GetCustomAttribute<T>() ?? variableInfo.fieldInfo?.GetCustomAttribute<T>();
    }

    public static void SaveElements(ISequenceElement element, XmlWriter xmlWriter)
    {
        foreach (var variableInfo in ConfigManager.GetFieldsAndProperties(element))
        {
            if (variableInfo.GetCustomAttribute<ElementCustomDataAttribute>() == null || variableInfo.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() != null)
                continue;

            object dummy = variableInfo.GetValue(element);
            if (variableInfo.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute && dummy.Equals(defaultValueAttribute.Value))
                continue;
            if (true)
            {
                var defInstance = Activator.CreateInstance(element.GetType());
                var defDummy = variableInfo.GetValue(defInstance);
                if (Equals(dummy, defDummy))
                    continue;
            }
            if (dummy is IXmlSerializable serializable)
            {
                xmlWriter.WriteStartElement(variableInfo.Name);
                serializable.WriteXml(xmlWriter);
                xmlWriter.WriteEndElement();
            }
            else
            {
                string content = dummy switch
                {
                    float f => f.ToString("0.00"),
                    double d => d.ToString("0.00"),
                    SequenceDelegateDefinition definition => definition.Key != SequenceSystem.NoneDelegateKey ? definition.Key : null,
                    _ => dummy.ToString()
                };
                if (content != null)
                    xmlWriter.WriteElementString(variableInfo.Name, content);
            }
        }
    }

    public static void LoadElements(ISequenceElement element, XmlReader xmlReader)
    {
        var elementReader = new XmlElementReader(xmlReader);
        foreach (var variableInfo in ConfigManager.GetFieldsAndProperties(element))
        {
            if (variableInfo.GetCustomAttribute<ElementCustomDataAttribute>() == null || variableInfo.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() != null)
                continue;
            if (elementReader[variableInfo.Name] is { } xmlElement)
            {
                if (xmlElement.HasElements)
                {
                    using XmlReader reader = xmlElement.CreateReader();
                    if (variableInfo.GetValue(element) is IXmlSerializable serializable)
                        serializable.ReadXml(reader);
                    else
                    {
                        var obj = Activator.CreateInstance(variableInfo.Type) as IXmlSerializable;
                        obj.ReadXml(reader);
                        variableInfo.SetValue(element, obj);
                    }
                }
                else
                {
                    string content = xmlElement.Value;
                    object dummy = variableInfo.GetValue(element);
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
                    if (value == null && variableInfo.Type.IsEnum)
                        value = Enum.GetValues(variableInfo.Type).GetValue(Enum.GetNames(variableInfo.Type).IndexOf(content));
                    if (value != null)
                        variableInfo.SetValue(element, value);
                }
            }
            else if (variableInfo.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute)
                variableInfo.SetValue(element, defaultValueAttribute.Value);
        }
    }
}
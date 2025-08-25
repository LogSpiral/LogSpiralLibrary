//using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
//using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.OldSources;

//partial interface ISequenceElement
//{
//    void SaveAttribute(XmlWriter xmlWriter)
//    {
//        var props = GetType().GetProperties();
//        foreach (var prop in props)
//        {
//            if (prop.GetCustomAttribute<ElementCustomDataAttribute>() != null && prop.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null)
//            {
//                object dummy = prop.GetValue(this);
//                if (prop.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute && dummy.Equals(defaultValueAttribute.Value))
//                    continue;
//                if (prop.PropertyType.IsEnum)
//                    dummy = (int)dummy;
//                string content = dummy switch
//                {
//                    float f => f.ToString("0.00"),
//                    double d => d.ToString("0.00"),
//                    SeqDelegateDefinition definition => definition.Key != SequenceSystem.NoneDelegateKey ? definition.Key : null,
//                    _ => dummy.ToString()
//                };
//                if (content != null)
//                    xmlWriter.WriteAttributeString(prop.Name, content);
//            }
//        }
//        foreach (var fld in GetType().GetFields())
//        {
//            if (fld.GetCustomAttribute<ElementCustomDataAttribute>() != null && fld.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null)
//            {
//                object dummy = fld.GetValue(this);
//                if (fld.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute && dummy.Equals(defaultValueAttribute.Value))
//                    continue;
//                if (fld.FieldType.IsEnum)
//                    dummy = (int)dummy;
//                string content = dummy switch
//                {
//                    float f => f.ToString("0.00"),
//                    double d => d.ToString("0.00"),
//                    SeqDelegateDefinition definition => definition.Key != SequenceSystem.NoneDelegateKey ? definition.Key : null,
//                    _ => dummy.ToString()
//                };
//                if (content != null)
//                    xmlWriter.WriteAttributeString(fld.Name, content);
//            }
//        }
//    }
//    void LoadAttribute(XmlReader xmlReader)
//    {
//        //Cycle = int.Parse(xmlReader["Cycle"]);
//        //ModifyData = ActionModifyData.LoadFromString(xmlReader["ModifyData"]);
//        var type = GetType();
//        var props = type.GetProperties();
//        foreach (var prop in props)
//        {
//            if (prop.GetCustomAttribute<ElementCustomDataAttribute>() != null && prop.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null)
//            {
//                if (xmlReader[prop.Name] is string content && content.Length != 0)
//                {
//                    object dummy = prop.GetValue(this);
//                    object value = dummy switch
//                    {
//                        int => int.Parse(content),
//                        float => float.Parse(content),
//                        double => double.Parse(content),
//                        bool => bool.Parse(content),
//                        byte => byte.Parse(content),
//                        ActionModifyData => ActionModifyData.LoadFromString(content),
//                        SeqDelegateDefinition => new SeqDelegateDefinition(content),
//                        _ => null
//                    };
//                    if (value == null && prop.PropertyType.IsEnum)
//                        value = Enum.GetValues(prop.PropertyType).GetValue(int.Parse(content));
//                    if (value != null)
//                        prop.SetValue(this, value);
//                }
//                else if (prop.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute)
//                    prop.SetValue(this, defaultValueAttribute.Value);
//            }
//        }
//        foreach (var fld in type.GetFields())
//        {
//            if (fld.GetCustomAttribute<ElementCustomDataAttribute>() != null && fld.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null)
//            {
//                if (xmlReader[fld.Name] is string content && content.Length != 0)
//                {
//                    object dummy = fld.GetValue(this);

//                    object value = dummy switch
//                    {
//                        int => int.Parse(content),
//                        float => float.Parse(content),
//                        double => double.Parse(content),
//                        bool => bool.Parse(content),
//                        byte => byte.Parse(content),
//                        ActionModifyData => ActionModifyData.LoadFromString(content),
//                        SeqDelegateDefinition => new SeqDelegateDefinition(content),
//                        _ => null
//                    };
//                    if (value == null && fld.FieldType.IsEnum)
//                        value = Enum.GetValues(fld.FieldType).GetValue(int.Parse(content));
//                    if (value != null)
//                        fld.SetValue(this, value);
//                }
//                else if (fld.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute)
//                    fld.SetValue(this, defaultValueAttribute.Value);

//            }

//        }
//    }
//    void NetSend(BinaryWriter writer);
//    void NetReceive(BinaryReader reader);
//}
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Panel.Fillers;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces.Panel;
using System.Collections.Generic;
using Terraria.ModLoader.Config;
namespace LogSpiralLibrary.UIBase.SequenceEditUI.PropertyPanelSupport;

// [ExtendsFromMod(nameof(PropertyPanelLibrary))]
public class SequenceElementFiller(ISequenceElement element) : IPropertyOptionFiller
{
    public ISequenceElement Element { get; set; } = element;

    void IPropertyOptionFiller.FillingOptionList(List<PropertyOption> list)
    {
        var variableInfos = ConfigManager.GetFieldsAndProperties(Element);
        foreach (var variableInfo in variableInfos)
        {
            if (!variableInfo.CanWrite
                || !Attribute.IsDefined(variableInfo.MemberInfo, typeof(ElementCustomDataAttribute))
                || Attribute.IsDefined(variableInfo.MemberInfo, typeof(ElementCustomDataAbabdonedAttribute))
                || variableInfo.Type == typeof(SequenceDelegateDefinition))
                continue;

            var option = ObjectMetaDataFiller.VariableInfoToOption(Element, variableInfo);
            list.Add(option);
        }
    }
}

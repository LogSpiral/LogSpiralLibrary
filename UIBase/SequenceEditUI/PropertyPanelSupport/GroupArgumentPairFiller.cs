using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Panel.Fillers;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces.Panel;
using System.Collections.Generic;
using Terraria.ModLoader.Config;
namespace LogSpiralLibrary.UIBase.SequenceEditUI.PropertyPanelSupport;

public class GroupArgumentPairFiller(WrapperArgPair pair) : IPropertyOptionFiller
{
    public WrapperArgPair Pair { get; set; } = pair;

    void IPropertyOptionFiller.FillingOptionList(List<PropertyOption> list)
    {
        if (Pair.Argument is { } argument)
        {
            var variableInfos = ConfigManager.GetFieldsAndProperties(argument);
            foreach (var variableInfo in variableInfos)
            {
                if (!variableInfo.CanWrite)
                    continue;

                var option = ObjectMetaDataFiller.VariableInfoToOption(argument, variableInfo);
                list.Add(option);
            }
        }
        if (Pair.Wrapper.Element is { } element)
        {
            var variableInfos = ConfigManager.GetFieldsAndProperties(element);
            foreach (var variableInfo in variableInfos)
            {
                if (!variableInfo.CanWrite
                    || !Attribute.IsDefined(variableInfo.MemberInfo, typeof(ElementCustomDataAttribute))
                    || Attribute.IsDefined(variableInfo.MemberInfo, typeof(ElementCustomDataAbabdonedAttribute))
                    || variableInfo.Type == typeof(SequenceDelegateDefinition))
                    continue;

                var option = ObjectMetaDataFiller.VariableInfoToOption(element, variableInfo);
                list.Add(option);
            }
        }
    }
}

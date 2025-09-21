using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Panel.Fillers;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces.Panel;
using System.Collections.Generic;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.UIBase.SequenceEditUI.PropertyPanelSupport;

// [ExtendsFromMod(nameof(PropertyPanelLibrary))]
public class GroupFiller(object groupContext) : IPropertyOptionFiller
{
    public object GroupContext { get; set; } = groupContext;

    void IPropertyOptionFiller.FillingOptionList(List<PropertyOption> list)
    {
        if (GroupContext is { } argument)
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
    }
}

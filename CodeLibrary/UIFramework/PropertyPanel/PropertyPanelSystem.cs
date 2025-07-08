using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Config;
using Terraria.UI;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework;
using LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel.Components;
//using LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel.Components;

namespace LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel;

public class PropertyPanelSystem : ModSystem
{
    #region Register
    static readonly HashSet<OptionBase> _registeredOptions = [];
    static readonly Dictionary<Type, Type> _simpleTypeOptionDictionary = [];
    static readonly Dictionary<Func<Type, bool>, Type> _complexDefaultTypeOptionDictionary = [];
    public static void RegisterOption(OptionBase option) => _registeredOptions.Add(option);
    public static void RegisterOptionToType(OptionBase option, Type variableType) => _simpleTypeOptionDictionary[option.GetType()] = variableType;
    public static void RegistreOptionToTypeComplex(Func<Type, bool> func, Type variableType) => _complexDefaultTypeOptionDictionary[func] = variableType;

    public override void Unload()
    {
        _simpleTypeOptionDictionary.Clear();
        _complexDefaultTypeOptionDictionary.Clear();
        _registeredOptions.Clear();
    }
    #endregion
    public static OptionBase WrapIt(UIElementGroup parent, ModConfig modConfig, PropertyFieldWrapper variable, object item, object list = null, Type arrayType = null, int index = -1, OptionBase owner = null, Action<OptionBase> preLabelAppend = null)
    {
        Type type = variable.Type;
        if (arrayType != null)
        {
            type = arrayType;
        }
        OptionBase option;
        Type optionType = typeof(OptionNotSupportText);
        var customOptionAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomOptionElementAttribute>(variable, item, list);
        if (customOptionAttribute != null)
            optionType = customOptionAttribute.Type;
        else if (_simpleTypeOptionDictionary.TryGetValue(type, out var resultOptionType))
            optionType = resultOptionType;
        else
            foreach (var pair in _complexDefaultTypeOptionDictionary)
                if (pair.Key.Invoke(type))
                {
                    optionType = pair.Value;
                    break;
                }
        option = Activator.CreateInstance(optionType) as OptionBase;


        option.index = index;
        option.List = (IList)list;
        option.Item = item;
        option.path = [];
        if (owner != null)
        {
            if (owner.path != null)
                option.path.AddRange(owner.path);
            if (owner.List == null)
                option.path.Add(owner.VariableInfo.Name);
            if (list != null)
                option.path.Add(index.ToString());
            option.owner = owner;
        }
        try
        {
            option.Bind(modConfig, variable, preLabelAppend);
        }
        catch
        {
            option = new OptionNotSupportText();
            option.Bind(modConfig, variable, preLabelAppend);
        }
        option.Join(parent);
        return option;
    }

}

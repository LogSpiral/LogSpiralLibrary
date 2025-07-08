using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class CustomOptionElementAttribute : Attribute
{
    public Type Type { get; init; }
    public CustomOptionElementAttribute(Type optionType)
    {
        if (!optionType.IsAssignableTo(typeof(OptionBase)))
            throw new ArgumentException("optionType is not OptionBase");
        Type = optionType;
    }
}


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class CustomOptionElementAttribute<T>() : CustomOptionElementAttribute(typeof(T)) where T : OptionBase
{
}
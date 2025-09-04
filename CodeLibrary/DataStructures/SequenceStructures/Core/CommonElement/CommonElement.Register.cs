using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces;
using System.Reflection;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.CommonElement;

public partial class CommonElement
{
    protected CommonElement()
    {
        if (SequenceGlobalManager
            .ElementInstances
            .TryGetValue(GetType(), out var instance)
            && instance is CommonElement common)
            Mod = common.Mod;
    }

    protected static void CommonRegister<T>(T instance) where T : CommonElement
    {
        var type = instance.GetType();
        ModTypeLookup<T>.Register(instance);
        if (instance.Name != typeof(T).Name)
        {
            // T自身原本作为抽象类使用，不参与元素库
            SequenceGlobalManager.ElementTypeLookup[instance.FullName] = type;
            SequenceGlobalManager.ElementInstances[type] = instance;
            SequenceManager<T>.Instance.ElementTypeLookup[instance.FullName] = type;
        }
    }

    /// <summary>
    /// 一般使用<see cref="CommonRegister{T}(T)"/>进行注册
    /// <br>子类中需要sealed</br>
    /// </summary>
    protected abstract void RootRegister();

    public override sealed void Register()
    {
        IMemberLocalized.InitializeCachedData(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), () => GetType().Name);
        var type = GetType();
        RootRegister();
        foreach (var fld in type.GetFields())
        {
            if (type != fld.DeclaringType
                || fld.GetCustomAttribute<ElementCustomDataAttribute>() == null
                || fld.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() != null)
                continue;

            // 自动注册字段键
            Language.GetOrRegister(this.GetLocalizationKey(fld.Name + ".Label"), () => fld.Name);
        }
        foreach (var property in type.GetProperties())
        {
            if (type != property.DeclaringType
                || property.GetCustomAttribute<ElementCustomDataAttribute>() == null
                || property.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() != null)
                continue;

            // 自动注册属性键
            Language.GetOrRegister(this.GetLocalizationKey(property.Name + ".Label"), () => property.Name);
        }

        // 自动注册分类键
        var categoryKey = $"Mods.{Mod.Name}.{LocalizationCategory}.Category.{Category}";
        //if (!Language.Exists(categoryKey))

        if (!string.IsNullOrEmpty(Category))
            Language.GetOrRegister(categoryKey, () => Category);
    }
}
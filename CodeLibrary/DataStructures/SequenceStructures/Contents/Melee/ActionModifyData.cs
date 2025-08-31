using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using PropertyPanelLibrary.PropertyPanelComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;


public struct ActionModifyData(float size = 1, float timeScaler = 1, float knockBack = 1, float damage = 1, int critAdder = 0, float critMultiplyer = 1) : IXmlSerializable, IMemberLocalized
{
    public float Size { get; set; } = size;
    public float TimeScaler { get; set; } = timeScaler;
    public float KnockBack { get; set; } = knockBack;
    public float Damage { get; set; } = damage;
    public int CritAdder { get; set; } = critAdder;
    public float CritMultiplyer { get; set; } = critMultiplyer;

    readonly string IMemberLocalized.LocalizationRootPath => "Mods.LogSpiralLibrary.Sequence.MeleeAction.ActionModifyData";
    private static string[] Suffixes { get; } = ["Label", "Tooltip"];
    readonly IReadOnlyList<string> IMemberLocalized.LocalizationSuffixes => Suffixes;

    /// <summary>
    /// 将除了速度以外的值赋给目标
    /// </summary>
    /// <param name="target"></param>
    public readonly void SetActionValue(ref ActionModifyData target)
    {
        float speed = target.TimeScaler;
        target = this with { TimeScaler = speed };
    }

    public readonly void SetActionSpeed(ref ActionModifyData target) => target.TimeScaler = TimeScaler;

    public override readonly string ToString()
    {
        //return (actionOffsetSize, actionOffsetTimeScaler, actionOffsetKnockBack, actionOffsetDamage, actionOffsetCritAdder, actionOffsetCritMultiplyer).ToString();
        var cultureInfo = GameCulture.KnownCultures.First().CultureInfo;
        var result = $"({Size.ToString("0.00", cultureInfo)}|{TimeScaler.ToString("0.00", cultureInfo)}|{KnockBack.ToString("0.00", cultureInfo)}|{Damage.ToString("0.00", cultureInfo)}|{CritAdder.ToString(cultureInfo)}|{CritMultiplyer.ToString("0.00", cultureInfo)})";
        return result;
    }

    public static ActionModifyData LoadFromString(string str)
    {
        var cultureInfo = GameCulture.KnownCultures.First().CultureInfo;
        var content = str.Remove(0, 1).Remove(str.Length - 2).Split('|', ',');
        var (size, timeScaler, knockBack, damage, critAdder, critMultiplyer) = (float.Parse(content[0], cultureInfo), float.Parse(content[1], cultureInfo), float.Parse(content[2], cultureInfo), float.Parse(content[3], cultureInfo), int.Parse(content[4], cultureInfo), float.Parse(content[5], cultureInfo));
        var result = new ActionModifyData(size, timeScaler, knockBack, damage, critAdder, critMultiplyer);
        return result;
    }

    readonly XmlSchema IXmlSerializable.GetSchema() => null;

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        reader.ReadStartElement();
        XmlElementReader elementReader = new(reader);
        Size = float.TryParse(elementReader["Size"]?.Value ?? "1.0", out var size) ? size : 1;
        TimeScaler = float.TryParse(elementReader["TimeScaler"]?.Value ?? "1.0", out var timeScaler) ? timeScaler : 1;
        KnockBack = float.TryParse(elementReader["KnockBack"]?.Value ?? "1.0", out var knockBack) ? knockBack : 1;
        Damage = float.TryParse(elementReader["Damage"]?.Value ?? "1.0", out var damage) ? damage : 1;
        CritAdder = int.TryParse(elementReader["CritAdder"]?.Value ?? "0", out var critAdder) ? critAdder : 0;
        CritMultiplyer = float.TryParse(elementReader["CritMultiplier"]?.Value ?? "1.0", out var critMultiplier) ? critMultiplier : 1;
        reader.ReadEndElement();
    }

    readonly void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        if (Size != 1)
            writer.WriteElementString("Size", Size.ToString("0.0"));
        if (TimeScaler != 1)
            writer.WriteElementString("TimeScaler", TimeScaler.ToString("0.0"));
        if (KnockBack != 1)
            writer.WriteElementString("KnockBack", KnockBack.ToString("0.0"));
        if (Damage != 1)
            writer.WriteElementString("Damage", Damage.ToString("0.0"));
        if (CritAdder != 1)
            writer.WriteElementString("CritAdder", CritAdder.ToString());
        if (CritMultiplyer != 1)
            writer.WriteElementString("CritMultiplier", CritMultiplyer.ToString("0.0"));
    }
}

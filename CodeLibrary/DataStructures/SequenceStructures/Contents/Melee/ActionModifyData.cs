using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInElements.Object;
using PropertyPanelLibrary.PropertyPanelComponents.Core;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

public struct ActionModifyData(float size = 1, float timeScaler = 1, float knockBack = 1, float damage = 1, int critAdder = 0, float critMultiplyer = 1) : IXmlSerializable
{
    public float actionOffsetSize = size;
    public float actionOffsetTimeScaler = timeScaler;
    public float actionOffsetKnockBack = knockBack;
    public float actionOffsetDamage = damage;
    public int actionOffsetCritAdder = critAdder;
    public float actionOffsetCritMultiplyer = critMultiplyer;

    /// <summary>
    /// 将除了速度以外的值赋给目标
    /// </summary>
    /// <param name="target"></param>
    public void SetActionValue(ref ActionModifyData target)
    {
        float speed = target.actionOffsetTimeScaler;
        target = this with { actionOffsetTimeScaler = speed };
    }

    public void SetActionSpeed(ref ActionModifyData target) => target.actionOffsetTimeScaler = actionOffsetTimeScaler;

    public override string ToString()
    {
        //return (actionOffsetSize, actionOffsetTimeScaler, actionOffsetKnockBack, actionOffsetDamage, actionOffsetCritAdder, actionOffsetCritMultiplyer).ToString();
        var cultureInfo = GameCulture.KnownCultures.First().CultureInfo;
        var result = $"({actionOffsetSize.ToString("0.00", cultureInfo)}|{actionOffsetTimeScaler.ToString("0.00", cultureInfo)}|{actionOffsetKnockBack.ToString("0.00", cultureInfo)}|{actionOffsetDamage.ToString("0.00", cultureInfo)}|{actionOffsetCritAdder.ToString(cultureInfo)}|{actionOffsetCritMultiplyer.ToString("0.00", cultureInfo)})";
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
        actionOffsetSize = float.TryParse(elementReader["Size"]?.Value ?? "1.0", out var size) ? size : 1;
        actionOffsetTimeScaler = float.TryParse(elementReader["TimeScaler"]?.Value ?? "1.0", out var timeScaler) ? timeScaler : 1;
        actionOffsetKnockBack = float.TryParse(elementReader["KnockBack"]?.Value ?? "1.0", out var knockBack) ? knockBack : 1;
        actionOffsetDamage = float.TryParse(elementReader["Damage"]?.Value ?? "1.0", out var damage) ? damage : 1;
        actionOffsetCritAdder = int.TryParse(elementReader["CritAdder"]?.Value ?? "0", out var critAdder) ? critAdder : 0;
        actionOffsetCritMultiplyer = float.TryParse(elementReader["CritMultiplier"]?.Value ?? "1.0", out var critMultiplier) ? critMultiplier : 1;
        reader.ReadEndElement();
    }

    readonly void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        if (actionOffsetSize != 1)
            writer.WriteElementString("Size", actionOffsetSize.ToString("0.0"));
        if (actionOffsetTimeScaler != 1)
            writer.WriteElementString("TimeScaler", actionOffsetTimeScaler.ToString("0.0"));
        if (actionOffsetKnockBack != 1)
            writer.WriteElementString("KnockBack", actionOffsetKnockBack.ToString("0.0"));
        if (actionOffsetDamage != 1)
            writer.WriteElementString("Damage", actionOffsetDamage.ToString("0.0"));
        if (actionOffsetCritAdder != 1)
            writer.WriteElementString("CritAdder", actionOffsetCritAdder.ToString());
        if (actionOffsetCritMultiplyer != 1)
            writer.WriteElementString("CritMultiplier", actionOffsetCritMultiplyer.ToString("0.0"));
    }
}

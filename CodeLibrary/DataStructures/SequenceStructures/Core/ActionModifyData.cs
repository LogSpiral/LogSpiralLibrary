using System.Linq;
using Terraria.Localization;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

public struct ActionModifyData(float size = 1, float timeScaler = 1, float knockBack = 1, float damage = 1, int critAdder = 0, float critMultiplyer = 1)
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

}

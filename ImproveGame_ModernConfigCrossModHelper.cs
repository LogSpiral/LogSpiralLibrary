using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
namespace LogSpiralLibrary;// ���Ƶ������Ŀ��֮��ǵ��Ҽ����������Դ�������е���ĿȻ��ͬ�������ռ�

//���ļ�������ģ�鿪�����ṩ�������ĵĿ�ģ��֧�֣����������Ϣ����Readme.md�Ŀ�ģ��֧�ֲ���
public static class ImproveGame_ModernConfigCrossModHelper
{
    #region ע������ϵ��

    /// <summary>
    /// ��������ṩ���������ע��֧��
    /// <br>����ͨ����ע���������qot�����ⲿ���е���<see cref="ModLoader.TryGetMod(string, out Mod)"/></br>
    /// <br>������һ�㣬<code>if(Main.netMode == NetmodeID.Server ||!ModLoader.TryGetMod("ImproveGame",out var qot)) <br>    return;</br></code></br>
    /// </summary>
    /// <returns>�Ƿ�ע��ɹ�</returns>
    public static bool RegisterCategory(Mod qot, Mod target, List<KeyValuePair<string, ModConfig>> variables, int itemIconID = 0,
Func<Texture2D> getIconTexture = null, Func<string> getLabel = null, Func<string> getTooltip = null)
=> (bool)qot.Call(nameof(RegisterCategory), target, variables, itemIconID, getIconTexture, getLabel, getTooltip);

    /// <summary>
    /// ����������ڸ���������ʵ�������������ѡ���һ�����࿨
    /// </summary>
    public static void RegisterCategory(Mod qot, Mod target, ModConfig modConfig, List<string> variables, int itemIconID = 0,
        Func<Texture2D> getIconTexture = null, Func<string> getLabel = null, Func<string> getTooltip = null)
        => RegisterCategory(qot, target, (from name in variables select new KeyValuePair<string, ModConfig>(name, modConfig)).ToList(),
            itemIconID, getIconTexture, getLabel, getTooltip);

    /// <summary>
    /// ����������ڸ��������ʵ�������������ѡ���һ�����࿨
    /// </summary>
    public static void RegisterCategory(Mod qot, Mod target, List<(ModConfig, List<string>)> variables, int itemIconID = 0, Func<Texture2D> getIconTexture = null,
        Func<string> getLabel = null, Func<string> getTooltip = null)
    {
        List<KeyValuePair<string, ModConfig>> list = [];
        foreach (var pair in variables)
            list.AddRange((from name in pair.Item2 select new KeyValuePair<string, ModConfig>(name, pair.Item1)).ToList());
        RegisterCategory(qot, target, list, itemIconID, getIconTexture, getLabel, getTooltip);
    }

    #endregion

    /// <summary>
    /// ���� �����ڡ� ҳ��
    /// </summary>
    public static bool SetAboutPage(Mod qot, Mod target, Func<string> getAboutText, int itemIconID = 0,
Func<Texture2D> getIconTexture = null, Func<string> getLabel = null, Func<string> getTooltip = null)
=> (bool)qot.Call(nameof(SetAboutPage), target, getAboutText, itemIconID, getIconTexture, getLabel, getTooltip);

    public static bool AddModernConfigTitle(Mod qot, Mod target, LocalizedText titleText)
        => (bool)qot.Call(nameof(AddModernConfigTitle), target, titleText);

    /// <summary>
    /// ע��ĳѡ���Ԥ������
    /// </summary>
    public static bool RegisterPreview(Mod qot, PropertyFieldWrapper variableInfo, Action<UIElement, ModConfig, PropertyFieldWrapper, object, IList, int> previewDrawingMethod)
        => (bool)qot.Call(nameof(RegisterPreview), variableInfo, previewDrawingMethod);

    /// <summary>
    /// ע��ȫ��Ԥ������
    /// </summary>
    public static bool OnGlobalConfigPreview(Mod qot, Action<UIElement, ModConfig, PropertyFieldWrapper, object, IList, int> previewDrawingMethod)
        => (bool)qot.Call(nameof(OnGlobalConfigPreview), previewDrawingMethod);
}
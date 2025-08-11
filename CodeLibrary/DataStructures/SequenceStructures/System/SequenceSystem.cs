using System.Collections.Generic;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;

public class SequenceSystem : ModSystem
{
    //TODO 可以给其它类型的序列用
    public static Dictionary<string, Action<ISequenceElement>> elementDelegates = [];
    public static SequenceSystem Instance { get; private set; }
    public static Dictionary<string, Condition> Conditions { get; } = [];
    public static Condition AlwaysCondition { get; private set; }
    public static HashSet<Type> AvailableElementBaseTypes { get; } = [];
    public const string NoneDelegateKey = $"{nameof(LogSpiralLibrary)}/None";
    public const string AlwaysConditionKey = "Mods.LogSpiralLibrary.Condition.Always";
    static Condition FastRegisterCondition(string Name, Func<bool> predicate)
    {
        var result = new Condition(Language.GetOrRegister($"Mods.LogSpiralLibrary.Condition.{Name}"), predicate);
        Conditions.Add(Name, result);
        return result;
    }
    public override void Load()
    {
        ModDefinitionElement.locals = ModOrganizer.FindAllMods();

        Instance = this;

        elementDelegates[NoneDelegateKey] = element => { };

        #region conditions的赋值
        AlwaysCondition = FastRegisterCondition("Always", () => true);

        // TODO 改为使用实体条件，兼容更多实体的判定
        FastRegisterCondition("MouseLeft", () => Main.LocalPlayer.controlUseItem);
        FastRegisterCondition("MouseRight", () => Main.LocalPlayer.controlUseTile);
        FastRegisterCondition("ControlUp", () => Main.LocalPlayer.controlUp);
        FastRegisterCondition("ControlDown", () => Main.LocalPlayer.controlDown);
        FastRegisterCondition("SurroundThreat", () => Main.LocalPlayer.GetModPlayer<SurroundStatePlayer>().state == SurroundState.SurroundThreat);
        FastRegisterCondition("FrontThreat", () => Main.LocalPlayer.GetModPlayer<SurroundStatePlayer>().state == SurroundState.FrontThreat);

        // 录入原版条件
        var fieldInfos = typeof(Condition).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var fieldInfo in fieldInfos)
        {
            Condition condition = (Condition)fieldInfo.GetValue(null);
            string key = condition.Description.Key.Split('.')[^1];
            Conditions.TryAdd(key, condition);
        }
        #endregion
    }
    public override void PostSetupContent()
    {
        // TODO 加入基本序列类型的专用注册
        AvailableElementBaseTypes.Add(typeof(MeleeAction));


        foreach (var type in AvailableElementBaseTypes)
            LoadSequenceWithType(type);

        base.PostSetupContent();
    }
    public static void LoadSequences<T>() where T : ISequenceElement => SequenceManager<T>.Load();
    public static void LoadSequenceWithType(Type type) => LoadSequencesMethod.MakeGenericMethod(type).Invoke(null, []);
    static MethodInfo LoadSequencesMethod { get => field ??= typeof(SequenceSystem).GetMethod("LoadSequences", BindingFlags.Static | BindingFlags.Public); }
    public override void Unload()
    {
        AvailableElementBaseTypes?.Clear();
        Instance = null;
        base.Unload();
    }
}







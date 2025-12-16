using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.BasicNotes;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups.Arguments;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.UI.SequenceEditUI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader.Core;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;

public class SequenceSystem : ModSystem
{
    //TODO 可以给其它类型的序列用
    public static Dictionary<string, Action<ISequenceElement>> elementDelegates = [];

    public static SequenceSystem Instance { get; private set; }
    public static Dictionary<string, Condition> Conditions { get; } = [];
    public static Condition AlwaysCondition { get; private set; }
    public static SequenceElementCategory MeleeActionCategoryInstance { get; private set; }

    public const string NoneDelegateKey = $"{nameof(LogSpiralLibrary)}/None";
    public const string AlwaysConditionKey = "Mods.LogSpiralLibrary.Condition.Always";
    public static string SequenceSavePath { get; } = $"{Main.SavePath}\\Mods\\LogSpiralLibrary\\Sequence";
    private static Condition FastRegisterCondition(string Name, Func<bool> predicate)
    {
        var result = new Condition(Language.GetOrRegister($"Mods.LogSpiralLibrary.Condition.{Name}"), predicate);
        Conditions.Add(Name, result);
        return result;
    }
    public static readonly Dictionary<string, SequenceElementCategory> CategoryLookup = [];

    public static LocalMod[] LocalMods { get; private set; }
    public static LocalMod ModToLocal(Mod mod)
    {
        // string fileName = $"{mod.Name}.tmod";
        // <LocalMod> targetLocals = [];
        foreach (var localsItem in LocalMods)
        {
            if (localsItem.Name == mod.Name)
                return localsItem;
                // targetLocals.Add(localsItem);
        }
        return null;
        // LocalMod localMod = targetLocals[^1];
        // return localMod;
    }
    public override void Load()
    {
        LocalMods = ModOrganizer.FindAllMods();

        #region 旧版位置转移至新版
        var path = Path.Combine(Main.SavePath, "Mods", "LogSpiralLibrary_Sequence");
        if (Directory.Exists(path))
        {
            Directory.CreateDirectory(Path.Combine(Main.SavePath, "Mods", "LogSpiralLibrary"));
            Directory.Move(path, SequenceSavePath);
        }
        #endregion 旧版位置转移至新版

        Instance = this;

        elementDelegates[NoneDelegateKey] = element => { };

        #region conditions的赋值

        AlwaysCondition = FastRegisterCondition("Always", () => true);

        // TODO 改为使用实体条件，兼容更多实体的判定
        FastRegisterCondition("MouseLeft", () => Main.LocalPlayer.controlUseItem && Main.LocalPlayer.altFunctionUse != 2);
        FastRegisterCondition("MouseRight", () => Main.LocalPlayer.controlUseTile || Main.LocalPlayer.altFunctionUse == 2);
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

        #endregion conditions的赋值
    }

    public override void PostSetupContent()
    {
        MeleeActionCategoryInstance = SequenceElementCategory.RegisterCategory<MeleeAction>(Mod, TextureAssets.Item[ItemID.WarriorEmblem], new SwooshInfo());
        SequenceElementCategory.RegisterCategory<NoteElement>(Mod, TextureAssets.Projectile[ProjectileID.QuarterNote], new GuitarNote(), Color.LightPink * .5f);

        SequenceGlobalManager.SingleGroupToMultiGroup.Add(typeof(ConditionalSingleGroup), typeof(ConditionalMultiGroup));
        SequenceGlobalManager.SingleGroupToMultiGroup.Add(typeof(SingleWrapperGroup), typeof(ConditionalMultiGroup));
        SequenceGlobalManager.MultiGroupToSingleGroup.Add(typeof(ConditionalMultiGroup), typeof(ConditionalSingleGroup));
        SequenceGlobalManager.MultiGroupToSingleGroup.Add(typeof(ConditionalWeightedGroup), typeof(ConditionalSingleGroup));
        SequenceGlobalManager.MultiGroupToSingleGroup.Add(typeof(WeightedRandomGroup), typeof(SingleWrapperGroup));
        SequenceGlobalManager.GroupArgToSingleGroup.Add(typeof(NoneArg), typeof(SingleWrapperGroup));
        SequenceGlobalManager.GroupArgToSingleGroup.Add(typeof(WeightArg), typeof(SingleWrapperGroup));
        SequenceGlobalManager.GroupArgToSingleGroup.Add(typeof(ConditionArg), typeof(ConditionalSingleGroup));
        SequenceGlobalManager.GroupArgToSingleGroup.Add(typeof(ConditionWeightArg), typeof(ConditionalSingleGroup));
        foreach (var category in CategoryLookup.Values)
            LoadSequenceWithType(category.ElementType);

        base.PostSetupContent();
    }

    public static void LoadSequences<T>() where T : ISequenceElement => SequenceManager<T>.Load();

    public static void LoadSequenceWithType(Type type) => LoadSequencesMethod.MakeGenericMethod(type).Invoke(null, []);

    private static MethodInfo LoadSequencesMethod => field ??= typeof(SequenceSystem).GetMethod("LoadSequences", BindingFlags.Static | BindingFlags.Public);

    public override void Unload()
    {
        Instance = null;
        SequenceSaveHelper.ClearSavingEvent();
        base.Unload();
    }
}
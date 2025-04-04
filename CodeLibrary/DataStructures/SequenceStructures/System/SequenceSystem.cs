using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using System.Collections;
using Terraria.ModLoader.UI;
using LogSpiralLibrary.CodeLibrary.UIElements;
using System.Xml;
using Terraria.ModLoader.Core;
using System.Text;
using LogSpiralLibrary.CodeLibrary.UIGenericConfig;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UI;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;



public class SequenceSystem : ModSystem
{
    public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1)
        => GenericConfigElement.WrapIt(parent, ref top, memberInfo, item, order, list, arrayType, index, (configElem, flag) => SetSequenceUIPending(flag), owner: instance.sequenceUI);
    public static Condition ToEntityCondition(string key, string LocalizationKey, Entity entity)
    {
        if (entityConditions.TryGetValue(key, out var func))
            return new Condition(Language.GetOrRegister(LocalizationKey), () => func(entity));
        return null;
    }
    public static void FastAddStandardEntityCondition(string LocalizationKey)
    {
        string key = LocalizationKey.Split('.')[^1];
        conditions.Add(key, ToEntityCondition(key, LocalizationKey, Main.LocalPlayer));
    }
    public static Dictionary<string, Func<Entity, bool>> entityConditions = [];
    public static Dictionary<Type, MethodInfo> seqLoadMethods = [];
    public static Dictionary<Type, MethodInfo> seqWriteAllMethods = [];
    public static Dictionary<Type, MethodInfo> seqWriteMethods = [];
    public static MethodInfo GetLoad(Type type)
    {
        if (!seqLoadMethods.TryGetValue(type, out var method))
        {
            var sType = typeof(Sequence<>).MakeGenericType(type);
            seqLoadMethods[type] = method = sType.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, [typeof(XmlReader), typeof(string)]);
        }
        return method;
    }
    public static MethodInfo GetWriteAll(Type type)
    {
        if (!seqWriteAllMethods.TryGetValue(type, out var method))
        {
            var sType = typeof(SequenceManager<>).MakeGenericType(type);
            seqWriteAllMethods[type] = method = sType.GetMethod("WriteAllToPacket", BindingFlags.Static | BindingFlags.Public);
        }
        return method;
    }
    public static MethodInfo GetWrite(Type type)
    {
        if (!seqLoadMethods.TryGetValue(type, out var method))
        {
            var sType = typeof(Sequence<>).MakeGenericType(type);
            seqLoadMethods[type] = method = sType.GetMethod("WriteContent", BindingFlags.Instance | BindingFlags.Public, [typeof(XmlWriter)]);
        }
        return method;
    }
    public static void SetSequenceUIPending(bool flag = true)
    {
        instance.sequenceUI.PendingModify = flag;
    }
    //TODO 可以给其它类型的序列用
    public static Dictionary<string, Action<ISequenceElement>> elementDelegates = [];
    public static Dictionary<Type, Dictionary<string, Sequence>> sequenceBases = [];
    public static Dictionary<string, SequenceBasicInfo> sequenceInfos = [];
    public SequenceUI sequenceUI;
    public UserInterface userInterfaceSequence;
    public static ModKeybind ShowSequenceKeybind { get; private set; }
    public static SequenceSystem instance;
    public static Dictionary<string, Condition> conditions = [];
    public static List<Type> AvailableElementBaseTypes = [];
    public const string NoneDelegateKey = $"{nameof(LogSpiralLibrary)}/None";
    public const string AlwaysConditionKey = "Mods.LogSpiralLibrary.Condition.Always";
    public static bool loaded = false;
    public override void Load()
    {
        if (loaded) return;
        loaded = true;
        ModDefinitionElement.locals = ModOrganizer.FindAllMods();

        if (Main.netMode != NetmodeID.Server)
        {
            instance = this;
            sequenceUI = new SequenceUI();
            userInterfaceSequence = new UserInterface();
            sequenceUI.Activate();
            userInterfaceSequence.SetState(sequenceUI);
            ShowSequenceKeybind = KeybindLoader.RegisterKeybind(Mod, "ShowSequenceModifier", "Y");
            //On_UIElement.DrawSelf += On_UIElement_DrawSelf;
        }

        elementDelegates[NoneDelegateKey] = element => { };

        #region conditions的赋值
        //var noneCondition = new Condition(Language.GetOrRegister("Mods.LogSpiralLibrary.Condition.None"), () => true);
        //conditions.Add("None", noneCondition);
        var alwaysCondition = new Condition(Language.GetOrRegister("Mods.LogSpiralLibrary.Condition.Always"), () => true);
        conditions.Add("Always", alwaysCondition);

        //TODO 增加其它类型实体的判别条件
        entityConditions.Add("MouseLeft", entity => entity switch { Player plr => plr.controlUseItem, _ => false });
        entityConditions.Add("MouseRight", entity => entity switch { Player plr => plr.controlUseTile, _ => false });
        entityConditions.Add("ControlUp", entity => entity switch { Player plr => plr.controlUp, _ => false });
        entityConditions.Add("ControlDown", entity => entity switch { Player plr => plr.controlDown, _ => false });
        entityConditions.Add("SurroundThreat", entity => entity switch { Player plr => plr.GetModPlayer<SurroundStatePlayer>().state == SurroundState.SurroundThreat, _ => false });
        entityConditions.Add("FrontThreat", entity => entity switch { Player plr => plr.GetModPlayer<SurroundStatePlayer>().state == SurroundState.FrontThreat, _ => false });



        FastAddStandardEntityCondition("Mods.LogSpiralLibrary.Condition.MouseLeft");
        FastAddStandardEntityCondition("Mods.LogSpiralLibrary.Condition.MouseRight");
        FastAddStandardEntityCondition("Mods.LogSpiralLibrary.Condition.ControlUp");
        FastAddStandardEntityCondition("Mods.LogSpiralLibrary.Condition.ControlDown");
        FastAddStandardEntityCondition("Mods.LogSpiralLibrary.Condition.SurroundThreat");
        FastAddStandardEntityCondition("Mods.LogSpiralLibrary.Condition.FrontThreat");
        var fieldInfos = typeof(Condition).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var fieldInfo in fieldInfos)
        {
            Condition condition = (Condition)fieldInfo.GetValue(null);
            string key = condition.Description.Key.Split('.')[^1];
            if (!conditions.ContainsKey(key))
                conditions.Add(key, condition);
        }//录入原版条件

        #endregion



    }
    public override void PostSetupContent()
    {
        AvailableElementBaseTypes.Add(typeof(MeleeAction));
        foreach (var type in AvailableElementBaseTypes)
            LoadSequenceWithType(type);
        base.PostSetupContent();
    }
    //private void On_UIElement_DrawSelf(On_UIElement.orig_DrawSelf orig, UIElement self, SpriteBatch spriteBatch)
    //{
    //    orig(self, spriteBatch);
    //    spriteBatch.DrawString(FontAssets.MouseText.Value, self.GetHashCode().ToString(), self.GetDimensions().Position(), Color.White);
    //}
    /// <summary>
    /// 获得指定目录下的所有文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static List<FileInfo> GetFilesByDir(string path)
    {
        DirectoryInfo di = new(path);

        //找到该目录下的文件
        FileInfo[] fi = di.GetFiles();

        //把FileInfo[]数组转换为List
        List<FileInfo> list = [.. fi];
        return list;
    }
    /// <summary>
    /// 获得指定目录及其子目录的所有文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static List<FileInfo> GetAllFilesByDir(string path)
    {
        DirectoryInfo dir = new(path);

        //找到该目录下的文件
        FileInfo[] fi = dir.GetFiles();

        //把FileInfo[]数组转换为List
        List<FileInfo> list = [.. fi];

        //找到该目录下的所有目录里的文件
        DirectoryInfo[] subDir = dir.GetDirectories();
        foreach (DirectoryInfo d in subDir)
        {
            List<FileInfo> subList = GetFilesByDir(d.FullName);
            foreach (FileInfo subFile in subList)
            {
                list.Add(subFile);
            }
        }
        return list;
    }
    public static void LoadSequences<T>() where T : ISequenceElement
    {
        SequenceManager<T>.Load();
        var seq = SequenceManager<T>.sequences;
        sequenceBases[typeof(T)] = (from s in seq select new KeyValuePair<string, Sequence>(s.Key, s.Value)).ToDictionary();
    }
    public static void LoadSequenceWithType(Type type) => typeof(SequenceSystem).GetMethod("LoadSequences", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type).Invoke(null, []);

    public override void Unload()
    {
        AvailableElementBaseTypes?.Clear();
        instance = null;
        base.Unload();
    }
    public override void UpdateUI(GameTime gameTime)
    {
        if (SequenceUI.Visible)
        {
            userInterfaceSequence?.Update(gameTime);
        }
        base.UpdateUI(gameTime);
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        //寻找一个名字为Vanilla: Mouse Text的绘制层，也就是绘制鼠标字体的那一层，并且返回那一层的索引
        int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        //寻找到索引时
        if (MouseTextIndex != -1)
        {
            //往绘制层集合插入一个成员，第一个参数是插入的地方的索引，第二个参数是绘制层
            layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
               //这里是绘制层的名字
               "LogSpiralLibrary:SequenceUI",
               //这里是匿名方法
               delegate
               {
                   //当Visible开启时（当UI开启时）
                   if (SequenceUI.Visible)
                       //绘制UI（运行exampleUI的Draw方法）
                       sequenceUI.Draw(Main.spriteBatch);
                   return true;
               },
               //这里是绘制层的类型
               InterfaceScaleType.UI)
           );
        }
        base.ModifyInterfaceLayers(layers);
    }
}







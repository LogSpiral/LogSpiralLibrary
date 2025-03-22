using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using System.Collections;
using Terraria.ModLoader.UI;
using LogSpiralLibrary.CodeLibrary.UIElements;
using System.Xml;
using Terraria.ModLoader.Core;
using System.Text;
using NetSimplified;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.UIGenericConfig;
using Microsoft.CodeAnalysis.CSharp;
using static Terraria.Localization.NetworkText;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{
    public class SequenceConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [DefaultValue(false)]
        public bool ShowWrapBox = false;
        [DefaultValue(false)]
        public bool ShowGroupBox = false;
        [DefaultValue(false)]
        public bool ShowSequenceBox = false;
        [DefaultValue(typeof(Vector2), "32, 16")]
        [Range(0f, 64f)]
        public Vector2 Step = new(32, 16);


        public static SequenceConfig Instance => ModContent.GetInstance<SequenceConfig>();
        public override void OnChanged()
        {
            //SequenceSystem.instance?.sequenceUI?.SetupConfigList();
            base.OnChanged();
        }
    }
    /*public class SyncTestPlayer : ModPlayer
    {
        public int hashCode;
        public SyncTestPlayer()
        {
            hashCode = this.GetHashCode();
        }
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            SpriteBatch spb = Main.spriteBatch;
            spb.DrawString(FontAssets.MouseText.Value, hashCode.ToString(), Player.Top + new Vector2(0, -64) - Main.screenPosition, Main.DiscoColor);
            base.ModifyDrawInfo(ref drawInfo);
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (!newPlayer) return;
            var packet = LogSpiralLibraryMod.Instance.GetPacket();
            packet.Write((byte)LogSpiralLibraryMod.MessageType.TestMessage);
            packet.Write((byte)Player.whoAmI);
            packet.Write(hashCode);
            packet.Send(toWho, fromWho);
            base.SyncPlayer(toWho, fromWho, newPlayer);
        }
        //public override void SendClientChanges(ModPlayer clientPlayer)
        //{
        //    if (clientPlayer is SyncTestPlayer syncCopy && syncCopy.hashCode != hashCode)
        //    {
        //        SyncPlayer(-1, Main.myPlayer, false);
        //    }
        //    base.SendClientChanges(clientPlayer);
        //}
        //public override void CopyClientState(ModPlayer targetCopy)
        //{
        //    if (targetCopy is SyncTestPlayer syncCopy) 
        //    {
        //        syncCopy.hashCode = hashCode;
        //    }
        //    base.CopyClientState(targetCopy);
        //}
    }*/
    public class SequencePlayer : ModPlayer
    {
        /// <summary>
        /// 挥砍等组件的缓存时间
        /// </summary>
        public double cachedTime;

        /// <summary>
        /// 挂起强制执行下一个组
        /// </summary>
        public bool PendingForcedNext;

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (SequenceSystem.ShowSequenceKeybind.JustPressed)
            {
                if (SequenceUI.Visible)
                    SequenceSystem.instance.sequenceUI.Close();
                else
                    SequenceSystem.instance.sequenceUI.Open();
            }
            base.ProcessTriggers(triggersSet);
        }
        public Dictionary<Type, Dictionary<string, Sequence>> plrLocSeq = null;
        public void InitPlrLocSeq()
        {
            plrLocSeq = [];
            foreach (var type in SequenceSystem.AvailableElementBaseTypes)
                plrLocSeq[type] = [];
        }
        public SequencePlayer()
        {
            InitPlrLocSeq();
        }
        public void ReceiveAllSeqFile(BinaryReader reader)
        {
            foreach (var type in SequenceSystem.AvailableElementBaseTypes)
            {
                byte seqCount = reader.ReadByte();
                var dict = plrLocSeq[type];
                var method = SequenceSystem.GetLoad(type);
                for (int u = 0; u < seqCount; u++)
                {
                    int bCount = reader.ReadInt32();
                    string keyName = reader.ReadString();
                    byte[] bytes = reader.ReadBytes(bCount);
                    using MemoryStream memoryStream = new(bytes);
                    using XmlReader xmlReader = XmlReader.Create(memoryStream);
                    var seq = method.Invoke(null, [xmlReader, keyName.Split('/')[0]]);
                    dict[keyName] = (Sequence)seq;
                }
            }
        }
        /*public void SendAllSeqFIle(int toWho, int fromWho)
        {
            var packet = LogSpiralLibraryMod.Instance.GetPacket();
            packet.Write((byte)LogSpiralLibraryMod.MessageType.SequenceSyncAll);
            packet.Write((byte)Player.whoAmI);

            foreach (var type in SequenceSystem.AvailableElementBaseTypes)
                SequenceSystem.GetWrite(type).Invoke(null, [packet]);
            packet.Send(toWho, fromWho);
        }*/
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            // SendAllSeqFIle(toWho, fromWho);


            SyncAllSequence.Get(Player.whoAmI, plrLocSeq).Send(toWho, fromWho);
            base.SyncPlayer(toWho, fromWho, newPlayer);
        }
    }
    public class SyncSingleSequence : NetModule
    {
        public Sequence Sequence;
        public int plrIndex;
        public int ElementTypeIndex;
        public Type ElementType;
        public static SyncSingleSequence Get(int plrIndex, Sequence sequence, Type ElementType)
        {
            SyncSingleSequence result = NetModuleLoader.Get<SyncSingleSequence>();
            result.Sequence = sequence;
            result.plrIndex = plrIndex;
            result.ElementTypeIndex = SequenceSystem.AvailableElementBaseTypes.IndexOf(ElementType);
            result.ElementType = ElementType;
            //result.ElementType = elementType;
            return result;
        }
        public override void Send(ModPacket p)
        {
            p.Write((byte)plrIndex);
            p.Write((byte)ElementTypeIndex);
            using MemoryStream memoryStream = new();
            XmlWriterSettings settings = new();
            settings.Indent = true;
            settings.Encoding = new UTF8Encoding(false);
            settings.NewLineChars = Environment.NewLine;
            XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings);
            Sequence.WriteContent(xmlWriter);
            //SequenceSystem.GetWrite(ElementType).Invoke(Sequence, [xmlWriter]);
            xmlWriter.Dispose();
            p.Write(Sequence.KeyName);
            p.Write((int)memoryStream.Length);
            p.Write(memoryStream.ToArray());
            base.Send(p);
        }
        public override void Read(BinaryReader r)
        {
            plrIndex = r.ReadByte();
            ElementTypeIndex = r.ReadByte();
            ElementType = SequenceSystem.AvailableElementBaseTypes[ElementTypeIndex];
            string keyName = r.ReadString();
            int bLength = r.ReadInt32();
            byte[] buffer = r.ReadBytes(bLength);
            using MemoryStream memoryStream = new(buffer);
            using XmlReader xmlReader = XmlReader.Create(memoryStream);
            var seq = SequenceSystem.GetLoad(ElementType).Invoke(null, [xmlReader, keyName.Split('/')[0]]);

            Sequence = Main.player[plrIndex].GetModPlayer<SequencePlayer>().plrLocSeq[ElementType][keyName] = (Sequence)seq;
            base.Read(r);
        }
        public override void Receive()
        {
            if (Main.dedServ)
            {
                Get(plrIndex, Sequence, ElementType).Send(-1, plrIndex);
            }
        }
    }
    public class SyncAllSequence : NetModule
    {
        public Dictionary<Type, Dictionary<string, Sequence>> plrLocSeq;
        public int plrIndex;
        public static SyncAllSequence Get(int plrIndex, Dictionary<Type, Dictionary<string, Sequence>> plrSeq)
        {
            SyncAllSequence result = NetModuleLoader.Get<SyncAllSequence>();
            result.plrLocSeq = plrSeq;
            result.plrIndex = plrIndex;
            return result;
        }
        public override void Send(ModPacket p)
        {
            p.Write((byte)plrIndex);
            foreach (var type in SequenceSystem.AvailableElementBaseTypes)
                SequenceSystem.GetWriteAll(type).Invoke(null, [p]);
        }
        public override void Read(BinaryReader r)
        {
            plrIndex = r.ReadByte();
            var seqPlr = Main.player[plrIndex].GetModPlayer<SequencePlayer>();
            seqPlr.ReceiveAllSeqFile(r);
            plrLocSeq = seqPlr.plrLocSeq;
        }
        public override void Receive()
        {
            //Main.player[plrIndex].GetModPlayer<SequencePlayer>().plrLocSeq = plrLocSeq;
            if (Main.dedServ)
            {
                Get(plrIndex, plrLocSeq).Send(-1, plrIndex);
            }
        }
    }
    public static class SequenceManager<T> where T : ISequenceElement
    {
        public static Dictionary<string, Sequence<T>> sequences = [];//{ { "LogSpiralLibrary/None", new SequenceBase<T>() { od = LogSpiralLibraryMod.Instance} } };
        public static bool loaded;
        public static void Load()
        {
            if (loaded) return;
            loaded = true;

            List<string> keys = [nameof(LogSpiralLibrary)];
            foreach (var localMod in ModDefinitionElement.locals)
            {
                var refMods = from refMod in localMod.properties.modReferences where refMod.mod == nameof(LogSpiralLibrary) select refMod;
                if (refMods.Any())
                {
                    keys.Add(localMod.Name);
                }
            }

            foreach (var key in keys)
            {
                if (ModLoader.TryGetMod(key, out Mod mod))
                    foreach (var name in mod.GetFileNames())
                    {
                        if (name.StartsWith($"PresetSequences/{typeof(T).Name}") && name.EndsWith(".xml"))
                        {
                            var keyName = $"{key}/{Path.GetFileNameWithoutExtension(name)}";
                            if (!sequences.TryGetValue(keyName, out var seq))
                                seq = new Sequence<T>();

                            try
                            {
                                Sequence<T>.Load(name, mod, seq);
                                sequences[seq.KeyName] = seq;
                            }
                            catch 
                            {
                                mod.Logger.Error("Couldn't load sequence: "+ name);
                            }
                        }
                    }

            }



            //foreach (var type in AvailableElementBaseTypes)
            //{
            //    var list = GetAllFilesByDir($"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{type.Name}");
            //    foreach (FileInfo file in list)
            //    {
            //        var seqType = typeof(SequenceBase<>);
            //        seqType = seqType.MakeGenericType(type);
            //        var loadMethod = seqType.GetMethod("Load", BindingFlags.Public | BindingFlags.Static, [typeof(string)]);
            //        sequenceBases.Add(file.Name, (SequenceBase)loadMethod.Invoke(null, [file.FullName]));
            //    }
            //}
            var path = $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{typeof(T).Name}";
            if (!Directory.Exists(path)) return;
            var list = SequenceSystem.GetAllFilesByDir(path);
            foreach (FileInfo file in list)
            {
                var modName = file.FullName.Split('\\', '/')[^2];
                bool flag = ModLoader.TryGetMod(modName, out var mod);
                if (flag)
                {
                    string key = $"{modName}/{Path.GetFileNameWithoutExtension(file.Name)}";
                    try
                    {
                        Sequence<T> instance = Sequence<T>.Load(file.FullName);
                        sequences[key] = instance;
                    }
                    catch
                    {
                        mod.Logger.Error("Couldn't load sequence: " + file.Name);
                    }
                }
            }
        }
        public static void WriteAllToPacket(ModPacket packet)
        {
            packet.Write((byte)sequences.Count);
            foreach (var pair in sequences)
            {
                using MemoryStream stream = new();
                XmlWriterSettings settings = new();
                settings.Indent = true;
                settings.Encoding = new UTF8Encoding(false);
                settings.NewLineChars = Environment.NewLine;
                XmlWriter xmlWriter = XmlWriter.Create(stream, settings);
                pair.Value.WriteContent(xmlWriter);
                xmlWriter.Dispose();
                packet.Write((int)stream.Length);
                packet.Write(pair.Key);
                packet.Write(stream.ToArray());
            }
        }
    }
    public class SequenceSystem : ModSystem
    {
        public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1)
            => GenericConfigElement.WrapIt(parent, ref top, memberInfo, item, order, list, arrayType, index, (configElem, flag) => SequenceSystem.SetSequenceUIPending(flag), owner: instance.sequenceUI);
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
    public class SequenceUI : UIState
    {
        public const string localizationPath = "Mods.LogSpiralLibrary.SequenceUI";
        public static bool Visible = false;
        public UIList actionLib;
        public UIList sequenceLib;
        public UIListByRow pageList;
        public UIPanel WorkingPlacePanel;
        public UIPanel OuterWorkingPanel;
        public WraperBox currentWraper;
        public UIList propList;
        public UIList infoList;
        public UIButton<string> saveButton;
        public UIButton<string> saveAsButton;
        public UIButton<string> revertButton;
        public UIPanel BasicInfoPanel;
        public Type CurrentSelectedType;
        public Dictionary<string, Sequence> currentSequences => (from pair in SequenceManager<MeleeAction>.sequences select new KeyValuePair<string, Sequence>(pair.Key, pair.Value)).ToDictionary();//SequenceSystem.sequenceBases[CurrentSelectedType];//(Dictionary<string, SequenceBase>)typeof(SequenceCollectionManager<>).MakeGenericType(CurrentSelectedType).GetField("sequences",BindingFlags.Static | BindingFlags.Public).GetValue(null);
        public bool Draggable;
        public bool Dragging;
        bool pendingModify;
        public bool PendingModify
        {
            get => pendingModify;
            set
            {
                pendingModifyChange = value | pendingModify;
                pendingModify = value;
            }
        }
        bool pendingModifyChange;
        public Vector2 Offset;
        string hintText;
        SequenceBasicInfo currentInfo;
        public void ReloadLib()
        {
            actionLib.Clear();
            sequenceLib.Clear();
            Type elemBaseType = CurrentSelectedType;
            Type type = typeof(ModTypeLookup<>);
            type = type.MakeGenericType(elemBaseType);
            //Main.NewText(type.GetField("dict",BindingFlags.Static|BindingFlags.NonPublic) == null);
            IDictionary dict = (IDictionary)type.GetField("dict", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            type = typeof(Sequence<>.Wraper);
            type = type.MakeGenericType(elemBaseType);

            Dictionary<string, (UIDownList list, Dictionary<string, UIDownList> dict)> Categories = [];
            List<WraperBox> NoCategoryWrapers = [];
            foreach (var v in dict.Values)
            {
                var e = (ISequenceElement)Activator.CreateInstance(v.GetType());
                WraperBox wraperBox = new((Sequence.WraperBase)Activator.CreateInstance(type, [e]));
                wraperBox.WrapperSize();
                //wraperBox.HAlign = 0.5f;
                wraperBox.IsClone = true;

                string modName = e.Mod.DisplayName;
                string categoryName = e.Category;

                if (!Categories.TryGetValue(modName, out var singleModCategory)) //如果当前mod没添加过
                {
                    UIDownList singleModList = new(modName)
                    {
                        Width = new(0, 1)
                    };
                    actionLib.Add(singleModList);
                    singleModList.OnInitialize();
                    singleModCategory = Categories[modName] = (singleModList, []);
                }
                if (categoryName != "")
                {
                    if (!singleModCategory.dict.TryGetValue(categoryName, out var categoriedList)) //如果当前分类没添加过
                    {
                        string titleName = categoryName;
                        string key = $"Mods.{e.Mod.Name}.{CurrentSelectedType.Name}.Category.{categoryName}";
                        if (Language.Exists(key))
                            titleName = Language.GetTextValue(key);
                        UIDownList list = new(titleName)
                        {
                            Width = new(0, 1)
                        };
                        singleModCategory.list.Add(list);
                        list.OnInitialize();
                        categoriedList = singleModCategory.dict[categoryName] = list;
                    }
                    categoriedList.Add(wraperBox);
                }
                else
                    NoCategoryWrapers.Add(wraperBox);
            }
            foreach (var box in NoCategoryWrapers)
                Categories[box.wraper.Element.Mod.DisplayName].list.Add(box);
            Categories = [];
            foreach (var s in currentSequences.Values)
            {
                WraperBox wraperBox = new((Sequence.WraperBase)Activator.CreateInstance(type, [s]));
                wraperBox.sequenceBox.Expand = false;
                wraperBox.WrapperSize();
                wraperBox.IsClone = true;


                string modName = s.Mod.DisplayName;
                string categoryName = SequenceSystem.sequenceInfos[s.KeyName].Finished ? "Finished" : "Library";

                if (!Categories.TryGetValue(modName, out var singleModCategory)) //如果当前mod没添加过
                {
                    UIDownList singleModList = new(modName)
                    {
                        Width = new(0, 1)
                    };
                    sequenceLib.Add(singleModList);
                    singleModList.OnInitialize();
                    singleModCategory = Categories[modName] = (singleModList, []);
                }
                if (!singleModCategory.dict.TryGetValue(categoryName, out var categoriedList)) //如果当前分类没添加过
                {
                    
                    UIDownList list = new(Language.GetOrRegister(localizationPath + (categoryName == "Finished" ? ".FinishedSequences" : ".LibrarySequences")).Value)
                    {
                        Width = new(0, 1)
                    };
                    singleModCategory.list.Add(list);
                    list.OnInitialize();
                    categoriedList = singleModCategory.dict[categoryName] = list;
                }
                categoriedList.Add(wraperBox);
                //sequenceLib.Add(wraperBox);
            }
        }
        public void ResetPage()
        {
            pageList.Clear();
            UIButton<string> defPage = new(Language.GetOrRegister(localizationPath + ".DefaultPage").Value);//"默认页面"
            defPage.SetSize(new Vector2(128, 0), 0, 1);
            pageList.Add(defPage);
            defPage.OnLeftClick += (e, evt) => { SwitchToDefaultPage(); };
            //UIText defText = new UIText();
            //defText.IgnoresMouseInteraction = true;
            //defText.SetSize(default, 1, 1);
            //defPage.Append(defText);
            UIButton<string> newPage = new("+");
            newPage.SetSize(new Vector2(32, 0), 0, 1);

            newPage.OnLeftClick += (e, evt) =>
            {
                var seq = new Sequence<MeleeAction>();
                seq.Add(new SwooshInfo());
                OpenBasicSetter(new SequenceBasicInfo() { createDate = DateTime.Now, lastModifyDate = DateTime.Now }, seq);
            };
            pageList.Add(newPage);
            //UIText newText = new UIText("+");
            //newText.IgnoresMouseInteraction = true;
            //newText.SetSize(default, 1, 1);
            //newPage.Append(newText);
            SwitchToDefaultPage();
        }
        public void SwitchToDefaultPage()
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Draggable = false;
            Offset = default;
            WorkingPlacePanel.Elements.Clear();
            if (currentWraper != null)
                currentWraper.chosen = false;
            currentWraper = null;
            propList.Clear();
            infoList.Clear();
            currentInfo = null;
            UIText info = new(Language.GetOrRegister(localizationPath + ".DefaultPageHint").Value);
            //            "你正处于默认页面，可以在这里选择一个序列以开始编辑。\n" +
            //"成品序列指适于直接给武器使用的完成序列\n" +
            //"库存序列指适于用来辅助组成成品序列可以反复调用的序列\n" +
            //"二者没有硬性区别，根据自己感觉进行标记即可。"
            var d = info.GetDimensions().Height;
            info.SetSize(new Vector2(-10, 1), 1, 0);
            infoList.Add(info);
            info.IsWrapped = true;
            UIPanel finishedSequencePanel = new LogSpiralLibraryPanel();
            finishedSequencePanel.SetSize(default, 0.5f, 1f);
            UIPanel libSequencePanel = new LogSpiralLibraryPanel();
            libSequencePanel.SetSize(default, 0.5f, 1f);
            libSequencePanel.Left.Set(0, 0.5f);
            WorkingPlacePanel.Append(libSequencePanel);
            WorkingPlacePanel.Append(finishedSequencePanel);
            UIText fTitle = new(Language.GetOrRegister(localizationPath + ".FinishedSequences"));
            fTitle.SetSize(new Vector2(0, 20), 1, 0);
            UIText lTitle = new(Language.GetOrRegister(localizationPath + ".LibrarySequences"));
            lTitle.SetSize(new Vector2(0, 20), 1, 0);
            finishedSequencePanel.Append(fTitle);
            libSequencePanel.Append(lTitle);
            UIList fList = [];
            fList.SetSize(0, -20, 1, 1);
            fList.Top.Set(20, 0);
            UIScrollbar fScrollbar = new();
            fScrollbar.Height.Set(0f, 1f);
            fScrollbar.HAlign = 1f;
            fScrollbar.SetView(100, 1000);
            fList.SetScrollbar(fScrollbar);
            finishedSequencePanel.Append(fScrollbar);
            UIList lList = [];
            lList.SetSize(0, -20, 1, 1);
            lList.Top.Set(20, 0);
            UIScrollbar lScrollbar = new();
            lScrollbar.Height.Set(0f, 1f);
            lScrollbar.HAlign = 1f;
            lScrollbar.SetView(100, 1000);
            lList.SetScrollbar(lScrollbar);
            libSequencePanel.Append(lScrollbar);
            finishedSequencePanel.Append(fList);
            libSequencePanel.Append(lList);
            foreach (var s in currentSequences.Values)
            {
                //var dic = SequenceSystem.sequenceInfos.Values;
                (SequenceSystem.sequenceInfos[s.KeyName].Finished ? fList : lList).Add(SequenceToButton(s.Clone()));

            }
        }
        public void SwitchToSequencePage(SequenceBox box)
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Draggable = true;
            Offset = default;
            WorkingPlacePanel.Elements.Clear();
            //WorkingPlacePanel.BackgroundColor = Color.White with { A = 0} * .25f;
            if (currentWraper != null)
                currentWraper.chosen = false;
            currentWraper = null;
            propList.Clear();
            infoList.Clear();
            int top = 0;
            int order = 0;
            SequenceBasicInfo info = currentInfo = SequenceSystem.sequenceInfos[box.sequenceBase.KeyName].Clone();
            foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(info))
            {
                if (variable.Name == "passWord" || Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                    continue;
                var (container, elem) = SequenceSystem.WrapIt(infoList, ref top, variable, info, order++);
            }
            WorkingPlacePanel.OverflowHidden = true;
            box.SequenceSize(true);
            box.OnInitialize();
            WorkingPlacePanel.Append(box);

            saveButton = new UIButton<string>(Language.GetOrRegister(localizationPath + ".Save").Value);
            saveButton.SetSize(64, 32);
            saveButton.Left.Set(32, 0);
            saveButton.Top.Set(-48, 1);
            saveButton.OnLeftClick += (evt, elem) =>
            {
                var sequence = box.sequenceBase;
                if (currentInfo != null)
                    SequenceSystem.sequenceInfos[sequence.KeyName] = currentInfo;
                var info = SequenceSystem.sequenceInfos[sequence.KeyName];
                info.FileName = sequence.FileName;
                info.lastModifyDate = DateTime.Now;
                sequence.Save();
                Type type = sequence.GetType();
                var method = type.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, [typeof(string), type]);

                Type mgrType = typeof(SequenceManager<>).MakeGenericType(CurrentSelectedType);

                var target = (mgrType.GetField("sequences", BindingFlags.Public | BindingFlags.Static).GetValue(null) as IDictionary)[sequence.KeyName];

                method.Invoke(null, [sequence.LocalPath, target]);
                currentSequences[sequence.KeyName] = sequence;
                SequenceSystem.sequenceInfos[box.sequenceBase.KeyName] = info;
                PendingModify = false;
                SoundEngine.PlaySound(SoundID.MenuOpen);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    SyncSingleSequence.Get(Main.myPlayer, sequence, CurrentSelectedType).Send();
                }
            };


            saveAsButton = new UIButton<string>(Language.GetOrRegister(localizationPath + ".SaveAs").Value);
            saveAsButton.SetSize(128, 32);
            saveAsButton.Left.Set(112, 0);
            saveAsButton.Top.Set(-48, 1);
            void revertSeqData()
            {
                propList.Clear();
                box.Elements.Clear();
                box.groupBoxes.Clear();
                var sequence = box.sequenceBase;
                box.sequenceBase = SequenceSystem.instance.sequenceUI.currentSequences[sequence.KeyName].Clone();
                sequence = box.sequenceBase;
                foreach (var g in sequence.GroupBases)
                {
                    var gbox = new GroupBox(g);

                    box.groupBoxes.Add(gbox);

                }
                box.CacheRefresh = true;
                box.OnInitialize();
                box.Recalculate();
                PendingModify = false;
                SoundEngine.PlaySound(SoundID.MenuClose);

                infoList.Clear();
                currentInfo = info = SequenceSystem.sequenceInfos[box.sequenceBase.KeyName].Clone();
                foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(info))
                {
                    if (variable.Name == "passWord" || Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                        continue;
                    var (container, _elem) = SequenceSystem.WrapIt(infoList, ref top, variable, info, order++);
                }
            };
            saveAsButton.OnLeftClick += (evt, elem) =>
            {
                var sequence = box.sequenceBase.Clone();
                var info = SequenceSystem.sequenceInfos[sequence.KeyName].Clone();
                info.lastModifyDate = DateTime.Now;
                OpenBasicSetter(info, sequence);

                revertSeqData();
            };

            revertButton = new UIButton<string>(Language.GetOrRegister(localizationPath + ".Revert").Value);
            revertButton.SetSize(128, 32);
            revertButton.Left.Set(256, 0);
            revertButton.Top.Set(-48, 1);
            revertButton.OnLeftClick += (evt, elem) =>
            {
                revertSeqData();
            };
            pendingModify = pendingModifyChange = false;
        }
        public void SequenceToPage(SequenceBox box)
        {
            SoundEngine.PlaySound(SoundID.Unlock);
            UIButton<string> seqPage = new(box.sequenceBase.DisplayName);
            seqPage.SetSize(new Vector2(128, 0), 0, 1);
            pageList.Insert(pageList.Count - 1, seqPage);
            string hint = Language.GetOrRegister(localizationPath + ".SaveOrRevertPlz").Value;
            seqPage.OnLeftClick += (_evt, _elem) => { if (!pendingModify) SwitchToSequencePage(box); else Main.NewText(hint); };
            seqPage.OnRightClick += (_evt, _elem) =>
            {
                if (!pendingModify)
                {
                    SwitchToDefaultPage();
                    pageList.Remove(_elem);
                }
                else Main.NewText(hint);
            };
            //UIText seqText = new UIText(box.sequenceBase.SequenceNameBase);
            //seqText.IgnoresMouseInteraction = true;
            //seqText.SetSize(default, 1, 1);
            //seqPage.Append(seqText);
        }
        public UIButton<string> SequenceToButton(Sequence sequence)
        {
            UIButton<string> panel = new(sequence.DisplayName);
            panel.SetSize(-20, 40, 1, 0);
            //UIText uIText = new UIText(sequence.SequenceNameBase);
            SequenceBox box = new(sequence);
            box.SequenceSize(true);
            //panel.Append(uIText);
            //panel.Append(box);
            //if (SequenceDrawer.box != null && SequenceDrawer.box.sequenceBase.SequenceNameBase == sequence.SequenceNameBase) SequenceDrawer.box = box;
            string filePath = $"PresetSequences/{sequence.ElementTypeName}/{sequence.FileName}.xml";
            bool resetable = sequence.Mod.GetFileNames().Contains(filePath);
            UIImageButton delete = new(resetable ? Main.Assets.Request<Texture2D>("Images/UI/CharCreation/HairStyle_Arrow") : Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete"));
            delete.OnLeftClick += (evt, elem) =>
            {

                if (resetable)
                {
                    sequence.Reset();
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        SyncSingleSequence.Get(Main.myPlayer, sequence, CurrentSelectedType).Send();
                    }
                    SwitchToDefaultPage();
                    //var list = panel.Parent.Parent as UIList;
                    //int index = list._items.IndexOf(panel);
                    //list.Remove(panel);

                    //var nPanel = SequenceToButton(currentSequences[sequence.KeyName]);

                    //list._items.Insert(index,nPanel);
                    //list._innerList.Append(nPanel);
                    //list.UpdateOrder();
                    //list._innerList.Recalculate();
                }
                else
                {
                    //bool finished = SequenceSystem.sequenceInfos[sequence.KeyName].Finished;
                    //(finished ? )
                    var list = panel.Parent.Parent as UIList;
                    list.Remove(panel);
                };
            };
            //delete.SetSize(20, 20, 0, 0);
            delete.Left.Set(-20, 1);
            //delete.VAlign = 0.5f;
            delete.Top.Set(-12, 0.5f);
            delete.OnUpdate += e =>
            {
                if (e.IsMouseHovering)
                    hintText = Language.GetOrRegister(localizationPath + (resetable ? ".resetSequence" : ".deleteSequence")).Value;
                //Main.instance.MouseText(Language.GetOrRegister(localizationPath + (resetable ? ".resetSequence" : ".deleteSequence")).Value);
            };
            if (resetable)//TODO 给其它序列添加游戏内删除功能
                panel.Append(delete);
            panel.OnLeftClick += (evt, elem) =>
            {
                if (evt.Target == elem)
                    SequenceToPage(box);
            };
            panel.OnRightClick += (evt, elem) =>
            {
                if (evt.Target == elem)
                {
                    SequenceToPage(box);
                    SwitchToSequencePage(box);
                }
            };

            panel.OnMouseOut += (evt, elem) =>
            {
                (elem as UIPanel).BackgroundColor = UICommon.DefaultUIBlueMouseOver;
            };
            panel.OnMouseOver += (evt, elem) =>
            {
                (elem as UIPanel).BackgroundColor = UICommon.DefaultUIBlue;
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            return panel;
        }
        public override void OnInitialize()
        {
            //CurrentSelectedType = SequenceSystem.AvailableElementBaseTypes[0];
            UIPanel BasePanel = new DraggableUIPanel();
            //BasePanel.SetSize(default, 0.8f, 0.75f);
            //BasePanel.Top.Set(0, 0.2f);
            //BasePanel.Left.Set(0, 0.05f);
            BasePanel.SetSize(Main.ScreenSize.ToVector2() * new Vector2(0.8f, 0.75f));
            BasePanel.Top.Set(Main.screenHeight * (Main.playerInventory ? .2f : .05f), 0);
            BasePanel.Left.Set(Main.screenWidth * .05f, 0);

            Append(BasePanel);
            UIPanel BottonPanel = new LogSpiralLibraryPanel();
            BottonPanel.SetSize(default, 1.0f, 0.05f);
            UIImageButton roseButton = new(ModAsset.Rose);
            roseButton.OnLeftClick += (evt, elem) =>
            {
                Utils.OpenToURL("https://space.bilibili.com/259264134");
                SoundEngine.PlaySound(SoundID.MenuOpen);
            };
            roseButton.OnUpdate += e =>
            {
                if (e.IsMouseHovering)
                    hintText = Language.GetOrRegister(localizationPath + ".OpenHomePage").Value;

                //Main.instance.MouseText(Language.GetOrRegister(localizationPath + ".OpenHomePage").Value);
            };
            BottonPanel.Append(roseButton);
            UIImageButton openFolderButton = new(ModAsset.Folder);
            openFolderButton.OnLeftClick += (evt, elem) =>
            {
                string path = $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                Utils.OpenFolder(path);
            };
            openFolderButton.OnUpdate += e =>
            {
                if (e.IsMouseHovering)
                    hintText = Language.GetOrRegister(localizationPath + ".OpenSquenceFolder").Value;

                //Main.instance.MouseText(Language.GetOrRegister(localizationPath + ".OpenSquenceFolder").Value);
            };
            //openFolderButton.SetSize(new Vector2(32));
            openFolderButton.Left.Set(48, 0);
            BottonPanel.Append(openFolderButton);
            BasePanel.Append(BottonPanel);


            UIPanel PagePanel = new LogSpiralLibraryPanel();
            PagePanel.SetSize(default, 0.85f, 0.05f);
            PagePanel.Top.Set(0, 0.05f);
            PagePanel.Left.Set(0, 0.15f);
            pageList = [];
            pageList.SetSize(default, 1, 1);
            PagePanel.Append(pageList);
            PagePanel.PaddingTop = PagePanel.PaddingBottom = PagePanel.PaddingLeft = PagePanel.PaddingRight = 6;
            BottonPanel.MinHeight = PagePanel.MinHeight = new StyleDimension(40, 0);
            //UIScrollbarByRow pageScrollbar = new UIScrollbarByRow();
            //pageScrollbar.SetView(100f, 1000f);
            //pageScrollbar.Width.Set(0f, 1f);
            //pageScrollbar.VAlign = 1f;
            //PagePanel.Append(pageScrollbar);
            //pageList.SetScrollbar(pageScrollbar);
            BasePanel.Append(PagePanel);
            UIPanel InfoPanel = new LogSpiralLibraryPanel();
            InfoPanel.SetSize(default, 0.15f, 0.35f);
            InfoPanel.Top.Set(0, 0.05f);
            infoList = [];
            infoList.SetSize(-20, -20, 1f, 1f);
            infoList.Top.Set(20, 0);
            UIScrollbar infoScrollBar = new();
            infoScrollBar.SetView(100f, 1000f);
            infoScrollBar.Height.Set(0f, 1f);
            infoScrollBar.HAlign = 1f;
            InfoPanel.Append(infoList);
            InfoPanel.Append(infoScrollBar);
            infoList.SetScrollbar(infoScrollBar);
            BasePanel.Append(InfoPanel);
            UIText infoTitle = new(Language.GetOrRegister(localizationPath + ".InfoTitle").Value);
            infoTitle.SetSize(0, 20, 1, 0);
            InfoPanel.Append(infoTitle);
            UIPanel PropertyPanel = new LogSpiralLibraryPanel();
            PropertyPanel.SetSize(default, 0.15f, 0.6f);
            PropertyPanel.Top.Set(0, 0.4f);
            BasePanel.Append(PropertyPanel);
            UIText propTitle = new(Language.GetOrRegister(localizationPath + ".PropTitle").Value);
            propTitle.SetSize(0, 20, 1, 0);
            PropertyPanel.Append(propTitle);
            propList = [];
            propList.SetSize(-20, -20, 1, 1);
            propList.Top.Set(20, 0);
            PropertyPanel.Append(propList);
            var propScrollbar = new UIScrollbar();
            propScrollbar.SetView(100f, 1000f);
            propScrollbar.Height.Set(0f, 1f);
            propScrollbar.HAlign = 1f;
            PropertyPanel.Append(propScrollbar);

            InfoPanel.MinWidth = PropertyPanel.MinWidth = new StyleDimension(300, 0);
            propList.SetScrollbar(propScrollbar);
            OuterWorkingPanel = new LogSpiralLibraryPanel();
            OuterWorkingPanel.SetSize(default, 0.85f, 0.9f);
            OuterWorkingPanel.Top.Set(0, 0.1f);
            OuterWorkingPanel.Left.Set(0, 0.15f);
            OuterWorkingPanel.OverflowHidden = true;
            BasePanel.Append(OuterWorkingPanel);
            WorkingPlacePanel = new LogSpiralLibraryPanel();
            WorkingPlacePanel.SetSize(default, 0.65f / 0.85f, 1f);
            WorkingPlacePanel.OverflowHidden = true;
            WorkingPlacePanel.OnLeftMouseDown +=
                (evt, elem) =>
                {
                    if (Draggable && (evt.Target == elem || (WorkingPlacePanel.Elements[0] is SequenceBox sb && sb.MouseCheckInEmptySpace(evt.MousePosition))))
                    {
                        var e = elem.Elements.FirstOrDefault();
                        Offset = new Vector2(evt.MousePosition.X - e.Left.Pixels, evt.MousePosition.Y - e.Top.Pixels);
                        Dragging = true;
                    }
                };
            WorkingPlacePanel.OnLeftMouseUp += (evt, elem) => { Dragging = false; };
            WorkingPlacePanel.OnUpdate +=
                (elem) =>
                {
                    if (Dragging)
                    {
                        var e = elem.Elements.FirstOrDefault();
                        e.Left.Set(Main.mouseX - Offset.X, 0f);
                        e.Top.Set(Main.mouseY - Offset.Y, 0f);
                        e.Recalculate();
                    }
                };

            OuterWorkingPanel.Append(WorkingPlacePanel);
            UIPanel ActionLibraryPanel = new LogSpiralLibraryPanel();
            ActionLibraryPanel.SetSize(default, 0.2f / 0.85f, 0.4f);
            ActionLibraryPanel.Left.Set(0, 0.65f / 0.85f);
            OuterWorkingPanel.Append(ActionLibraryPanel);
            actionLib = [];
            actionLib.SetSize(new Vector2(0, -20), 1, 1);
            actionLib.Top.Set(20, 0);
            actionLib.ListPadding = 16;
            ActionLibraryPanel.Append(actionLib);
            var actionScrollbar = new UIScrollbar();
            actionScrollbar.SetView(100f, 1000f);
            actionScrollbar.Height.Set(0f, 1f);
            actionScrollbar.HAlign = 1f;
            ActionLibraryPanel.Append(actionScrollbar);
            UIText actionTitle = new(Language.GetOrRegister(localizationPath + ".ActionLibrary").Value);
            actionTitle.SetSize(0, 20, 1, 0);
            ActionLibraryPanel.Append(actionTitle);
            actionLib.SetScrollbar(actionScrollbar);
            UIPanel SequenceLibraryPanel = new LogSpiralLibraryPanel();
            SequenceLibraryPanel.SetSize(default, 0.2f / 0.85f, 0.6f);
            SequenceLibraryPanel.Left.Set(0, 0.65f / 0.85f);
            SequenceLibraryPanel.Top.Set(0, 0.4f);
            sequenceLib = [];
            sequenceLib.SetSize(new Vector2(0, -20), 1, 1);
            sequenceLib.ListPadding = 16;
            SequenceLibraryPanel.Append(sequenceLib);
            var sequenceScrollbar = new UIScrollbar();
            sequenceScrollbar.SetView(100f, 1000f);
            sequenceScrollbar.Height.Set(0f, 1f);
            sequenceScrollbar.HAlign = 1f;
            SequenceLibraryPanel.Append(sequenceScrollbar);
            sequenceLib.SetScrollbar(sequenceScrollbar);
            sequenceLib.Top.Set(20, 0);
            UIText sequenceTitle = new(Language.GetOrRegister(localizationPath + ".SequenceLibrary").Value);
            sequenceTitle.SetSize(0, 20, 1, 0);
            SequenceLibraryPanel.Append(sequenceTitle);
            SequenceLibraryPanel.MinWidth = ActionLibraryPanel.MinWidth = new StyleDimension(400, 0);
            OuterWorkingPanel.Append(SequenceLibraryPanel);


            SetUpBasicInfoPanel();
            //ResetPage();

            /*
            UIPanel panel = UIPanel = new LogSpiralLibraryPanel();
            panel.SetSize(new Vector2(240, 300));
            panel.Top.Set(80, 0);
            panel.Left.Set(20, 0);
            Append(panel);

            UIPanel container = UIConfigSetterContainer = new LogSpiralLibraryPanel();
            container.SetSize(new Vector2(540, 400));
            container.Top.Set(400, 0);
            container.Left.Set(20, 0);
            //Append(container);
            ConfigElemList = new UIList();
            ConfigElemList.SetSize(540, 360);
            container.Append(ConfigElemList);

            UIList = new UIList();
            UIList.ListPadding = 24f;
            UIList.SetSize(200, 400);

            panel.Append(UIList);
            SequenceDrawer = new SequenceDrawer();
            SequenceDrawer.Top.Set(0, 0.25f);
            SequenceDrawer.Left.Set(0, 0.25f);
            Append(SequenceDrawer);*/
            base.OnInitialize();
        }
        public void SetUpBasicInfoPanel()
        {
            BasicInfoPanel = new LogSpiralLibraryPanel();
            BasicInfoPanel.SetSize(default, 0.5f, 0.5f);
            BasicInfoPanel.HAlign = BasicInfoPanel.VAlign = 0.5f;
            UIList infoList = [];
            infoList.Top.Set(32, 0);
            infoList.SetSize(new Vector2(-40, -64), 1, 1);

            BasicInfoPanel.Append(infoList);

            //UIButton<string> yesButton = new UIButton<string>("确定");
            UIButton<string> noButton = new(Language.GetOrRegister(localizationPath + ".Cancel").Value);
            noButton.BackgroundColor = Color.Red * .7f;
            //yesButton.SetSize(64, 32, 0, 0);
            noButton.SetSize(64, 32, 0, 0);
            //yesButton.Top.Set(-32, 1);
            noButton.Top.Set(-32, 1);
            //yesButton.HAlign = 0.125f;
            noButton.HAlign = 0.785f;
            //BasicInfoPanel.Append(yesButton);
            BasicInfoPanel.Append(noButton);
            noButton.OnLeftClick += (evt, elem) =>
            {
                BasicInfoPanel.Remove();
            };

        }
        public void OpenBasicSetter(SequenceBasicInfo info, Sequence sequence)//string modName,
        {
            var list = (BasicInfoPanel.Elements[0] as UIList);
            list.Clear();
            //wraper.SetConfigPanel(list);
            SoundEngine.PlaySound(SoundID.MenuOpen);
            int top = 0;
            int order = 0;
            WorkingPlacePanel.Append(BasicInfoPanel);
            foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(info))
            {
                if (variable.Type == typeof(DateTime) || variable.Name == "passWord" || Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                    continue;
                var (container, elem) = SequenceSystem.WrapIt(list, ref top, variable, info, order++);
                //elem.OnLeftClick += (_evt, uielem) => { };
            }
            UIScrollbar uIScrollbar = new();
            uIScrollbar.SetView(100f, 1000f);
            uIScrollbar.Height.Set(0f, 1f);
            uIScrollbar.HAlign = 1f;
            list.SetScrollbar(uIScrollbar);
            if (BasicInfoPanel.Elements.Count > 2 && BasicInfoPanel.Elements[2] is UIScrollbar oldBar)
                oldBar.Remove();
            BasicInfoPanel.Append(uIScrollbar);
            UIButton<string> yesButton = new(Language.GetOrRegister(localizationPath + ".OK").Value);
            yesButton.SetSize(64, 32, 0, 0);
            yesButton.Top.Set(-32, 1);
            yesButton.HAlign = 0.125f;
            if (BasicInfoPanel.Elements.Count > 3)

                BasicInfoPanel.Elements[2].Remove();

            BasicInfoPanel.Append(yesButton);
            yesButton.OnLeftClick += (evt, elem) =>
            {
                if (info.FileName == "")
                {
                    Main.NewText(Language.GetOrRegister(localizationPath + ".EmptyFileNameException").Value, Color.Red);
                    return;
                }
                if (info.DisplayName == "")
                {
                    Main.NewText(Language.GetOrRegister(localizationPath + ".EmptyDisplayNameException").Value, Color.Red);
                    return;
                }
                if (info.FileName == Sequence.SequenceDefaultName || currentSequences.ContainsKey(info.KeyName))
                {
                    Main.NewText(Language.GetOrRegister(localizationPath + ".InvalidFileNameException").Value, Color.Red);
                    return;
                }
                if (info.ModName == null || !ModLoader.TryGetMod(info.ModName, out _))
                {
                    Main.NewText(Language.GetOrRegister(localizationPath + ".UnknownModException").Value, Color.Red);
                    return;
                }
                pendingModify = false;
                yesButton.Remove();
                BasicInfoPanel.Remove();
                Main.NewText(Language.GetOrRegister(localizationPath + ".SaveSucceed").Value, Color.LimeGreen);
                var dnames = from i in SequenceSystem.sequenceInfos.Values select i.DisplayName;
                if (dnames.Contains(info.DisplayName))
                {
                    Main.NewText(Language.GetOrRegister(localizationPath + ".RepeatedNameHint").Value, Color.Orange);
                }
                //SequenceSystem.instance.sequenceUI.currentSequences[info.KeyName] = sequence;
                var dict = typeof(SequenceManager<>).MakeGenericType(CurrentSelectedType).GetField("sequences", BindingFlags.Static | BindingFlags.Public).GetValue(null) as IDictionary;
                dict[info.KeyName] = sequence;
                SequenceSystem.sequenceInfos[info.KeyName] = info;
                var box = new SequenceBox(sequence);
                sequence.SyncInfo(info);
                sequence.Save();



                SequenceToPage(box);
                SwitchToSequencePage(box);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    SyncSingleSequence.Get(Main.myPlayer, sequence, CurrentSelectedType).Send();
                }
            };
        }
        public void Open()
        {
            Visible = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            CurrentSelectedType = SequenceSystem.AvailableElementBaseTypes[0];
            //UIPanel BasePanel = Elements[0] as UIPanel;
            //BasePanel.SetSize(default, 0.8f, 0.75f);
            //BasePanel.Top.Set(0, 0.2f);
            //BasePanel.Left.Set(0, 0.05f);
            ////Elements[0].SetSize(2560, 1600 * 0.8f);
            ///

            Elements.Clear();
            OnInitialize();


            ResetPage();
            ReloadLib();
            Recalculate();
            //SequenceDrawer.Top.Set(240, 0);
            //SequenceDrawer.Left.Set(0, 1.25f);

            //if (SequenceDrawer.box != null)
            //{
            //    SequenceDrawer.box.SequenceSize();
            //}
            //UIConfigSetterContainer.Top.Set(400, 0);
            //SetupConfigList();
        }
        public void Close()
        {
            Visible = false;
            Main.blockInput = false;
            //SequenceDrawer.box = null;
            //currentWraper = null;
            //RemoveChild(UIConfigSetterContainer);
            //RemoveChild(currentBox);
            saveButton?.Remove();
            PendingModify = false;
            pendingModifyChange = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
        public override void Update(GameTime gameTime)
        {
            hintText = "";
            //if (ConfigElemList != null)
            //    foreach (var u in from uiS in ConfigElemList._innerList.Children select uiS.Children)
            //        foreach(var v in u)
            //        Main.NewText(v.GetType());
            if (Elements[0].ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
                PlayerInput.LockVanillaMouseScroll("LogSpiralLibrary:SequenceUI");
            }
            if (pendingModifyChange)
            {
                if (pendingModify)
                {
                    if (saveButton != null)
                        WorkingPlacePanel.Append(saveButton);
                    if (saveAsButton != null)

                        WorkingPlacePanel.Append(saveAsButton);
                    if (revertButton != null)
                        WorkingPlacePanel.Append(revertButton);

                }
                else
                {
                    saveButton?.Remove();
                    saveAsButton?.Remove();
                    revertButton?.Remove();
                }
                pendingModifyChange = false;
            }
            base.Update(gameTime);
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {

            base.DrawSelf(spriteBatch);
            if (hintText != string.Empty)
                UICommon.TooltipMouseText(hintText);
        }
    }
    public class SequenceBasicInfo
    {
        [JsonIgnore]
        public string KeyName => $"{ModName}/{FileName}";
        //[CustomSeqConfigItem(typeof(SeqStringInputElement))]
        public string AuthorName;
        //[CustomSeqConfigItem(typeof(SeqStringInputElement))]
        public string Description;
        //[CustomSeqConfigItem(typeof(SeqStringInputElement))]
        public string FileName;
        //[CustomSeqConfigItem(typeof(SeqStringInputElement))]
        public string DisplayName;
        //[CustomModConfigItem(typeof(SeqStringInputElement))]
        //public string ModName;
        //[CustomSeqConfigItem(typeof(ModDefinitionElement))]
        public ModDefinition ModDefinition = new("LogSpiralLibrary");
        [JsonIgnore]
        public string ModName => ModDefinition?.Name ?? "UnknownMod";
        //[CustomSeqConfigItem(typeof(SeqDateTimeElement))]
        public DateTime createDate;
        //[CustomSeqConfigItem(typeof(SeqDateTimeElement))]
        public DateTime lastModifyDate;
        //TODO RSA加密
        //[CustomSeqConfigItem(typeof(SeqStringInputElement))]
        public string passWord;
        public bool Finished;
        //public enum SequenceMode
        //{
        //    balanced = 0,
        //    expansion = 1,
        //    developer = 2,
        //    lunatic = 3
        //}
        public void Save(XmlWriter writer)
        {
            writer.WriteAttributeString("AuthorName", AuthorName);
            writer.WriteAttributeString("Description", Description);
            writer.WriteAttributeString("FileName", FileName);
            writer.WriteAttributeString("DisplayName", DisplayName);
            writer.WriteAttributeString("ModName", ModName);
            writer.WriteAttributeString("createTime", createDate.Ticks.ToString());
            writer.WriteAttributeString("lastModifyDate", lastModifyDate.Ticks.ToString());
            writer.WriteAttributeString("passWord", passWord);
            writer.WriteAttributeString("Finished", Finished.ToString());
        }
        public SequenceBasicInfo Load(XmlReader reader)
        {
            AuthorName = reader["AuthorName"] ?? "不认识的孩子呢";
            Description = reader["Description"] ?? "绝赞的描述";
            FileName = reader["FileName"] ?? "Error";
            DisplayName = reader["DisplayName"] ?? "出错的序列";
            ModDefinition = new ModDefinition(reader["ModName"]);
            //ModName = reader["ModName"] ?? "Unknown";//"LogSpiralLibrary";
            createDate = new DateTime(long.Parse(reader["createTime"] ?? "0"));
            lastModifyDate = new DateTime(long.Parse(reader["lastModifyDate"] ?? "0"));
            passWord = reader["passWord"] ?? "";
            Finished = bool.Parse(reader["Finished"] ?? "false");
            return this;
        }
        public SequenceBasicInfo Clone() => MemberwiseClone() as SequenceBasicInfo;
    }
    public class SequenceDrawer : UIElement
    {
        public SequenceBox box;
        public override void OnInitialize()
        {
            //Append(box);
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (box != null)
            {
                DrawSequence(box, this.GetDimensions().Position(), default, box.sequenceBase.Active, true);
                box.sequenceBase.Active = false;
            }
            base.DrawSelf(spriteBatch);
        }
        public static void DrawWraper(WraperBox wraperBox, Vector2 position, float offset, bool active)
        {
            var pos = position;
            //position += SequenceConfig.Instance.Step * new Vector2(0, .5f);
            var spriteBatch = Main.spriteBatch;
            var desc = wraperBox.wraper.conditionDefinition.DisplayName;
            bool flag = wraperBox.wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey;
            if (wraperBox.wraper.IsSequence && wraperBox.sequenceBox.Expand)
            {
                ComplexPanelInfo panel = new();
                var boxSize = wraperBox.GetSize();
                //if (flag)
                //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f + new Vector2(0, 16), boxSize + new Vector2(32, 64));
                //else
                //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f, boxSize + new Vector2(32, 32));
                float offY = flag ? FontAssets.MouseText.Value.MeasureString(desc).Y : 0;
                panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X * .5f, 0), boxSize);
                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
                panel.glowEffectColor = Color.MediumPurple with { A = 0 };
                panel.glowShakingStrength = .1f;
                panel.glowHueOffsetRange = 0.1f;
                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
                panel.DrawComplexPanel(spriteBatch);
                DrawSequence(wraperBox.sequenceBox, position - offY * .5f * Vector2.UnitY, offY * .5f * Vector2.UnitY, active, false);
                if (flag)
                {
                    var cen = position + boxSize * Vector2.UnitY * .5f - offY * 1.5f * Vector2.UnitY + new Vector2(16, 0);
                    if (wraperBox.wraper.Condition.IsMet())
                    {
                        for (int n = 0; n < 3; n++)
                            spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f), new Color(0.8f, 0.2f, 1f, 0f), 0, default, 1f, 0, 0);
                        spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen, Color.White, 0, default, 1f, 0, 0);
                    }
                    else
                        spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen, Color.Gray);
                }
            }
            else
            {
                //position.Y += wraperBox.GetSize().Y * .5f;
                var font = FontAssets.MouseText.Value;
                var name = wraperBox.wraper.Name;
                Vector2 textSize = font.MeasureString(name);
                Vector2 boxSize = wraperBox.GetSize();
                #region 框框
                ComplexPanelInfo panel = new();
                panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X, 0) * .5f, boxSize);
                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
                panel.glowEffectColor = Color.Cyan with { A = 0 };
                panel.glowShakingStrength = .05f;
                panel.glowHueOffsetRange = 0.05f;
                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
                panel.DrawComplexPanel(spriteBatch);
                #endregion
                var cen = position + new Vector2(16) - boxSize * Vector2.UnitY * .5f;
                if (active)
                {
                    //var fontOff = font.MeasureString(name) * .5f;
                    //spriteBatch.DrawString(font, name, cen + fontOff, Color.Cyan, 0, fontOff, 1.1f, 0, 0);
                    //spriteBatch.DrawString(font, name, cen + fontOff, Color.Cyan, 0, fontOff, 0.9f, 0, 0);
                    for (int n = 0; n < 3; n++)
                        spriteBatch.DrawString(font, name, cen + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f), new Color(0.2f, 0.8f, 1f, 0f), 0, default, 1f, 0, 0);
                    spriteBatch.DrawString(font, name, cen, Color.White, 0, default, 1f, 0, 0);

                }
                else
                    spriteBatch.DrawString(font, name, cen, Color.Gray, 0, default, 1f, 0, 0);
                cen += textSize * Vector2.UnitY;
                if (flag)
                {
                    if (wraperBox.wraper.Condition.IsMet())
                    {

                        for (int n = 0; n < 3; n++)
                            spriteBatch.DrawString(font, "→" + desc, cen + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f), new Color(0.8f, 0.2f, 1f, 0f), 0, default, 1f, 0, 0);
                        spriteBatch.DrawString(font, "→" + desc, cen, Color.White, 0, default, 1f, 0, 0);

                    }
                    else
                        spriteBatch.DrawString(font, "→" + desc, cen, Color.Gray);
                }
                //spriteBatch.DrawRectangle(panel.destination, Color.MediumPurple);

            }
            if (SequenceConfig.Instance.ShowWrapBox)
                Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + wraperBox.GetSize() * Vector2.UnitX * .5f, wraperBox.GetSize()), Color.MediumPurple * .5f, 8);

            //锚点
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.MediumPurple * .5f, 0, new Vector2(.5f), 16, 0, 0);


        }
        public static void DrawGroup(GroupBox groupBox, Vector2 position, bool active)
        {
            Color GroupColor = Color.Cyan;
            var pos = position;
            var size = groupBox.GetSize();
            position.Y -= size.Y * .5f;
            var tarCen1 = pos + new Vector2(-SequenceConfig.Instance.Step.X * .25f, 0);
            var tarCen2 = pos + new Vector2(SequenceConfig.Instance.Step.X * .25f + size.X, 0);
            int c = 0;
            var font = FontAssets.MouseText.Value;
            foreach (var w in groupBox.wraperBoxes)
            {
                var desc = w.wraper.conditionDefinition;
                float offY = w.wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey ? font.MeasureString("→" + desc.DisplayName).Y : 0;
                Vector2 wsize = w.GetSize();
                position.Y += (wsize.Y + SequenceConfig.Instance.Step.Y - offY) * .5f;
                var offset = size.X - wsize.X;
                var scale = 1 + ((float)LogSpiralLibraryMod.ModTime / 180).CosFactor();
                //scale = 1;
                position.X = pos.X + offset * .5f;

                var tarCen3 = position;
                //var tarCen4 = w.wraper.IsSequence ? position + wsize * new Vector2(1, 0) + new Vector2(offset * 1.5f, 0) * .5f : position + new Vector2(offset * .5f + wsize.X, 0);
                var tarCen4 = tarCen3 + wsize.X * Vector2.UnitX;
                Main.spriteBatch.DrawHorizonBLine(tarCen3, tarCen1, Color.White, scale);
                Main.spriteBatch.DrawHorizonBLine(tarCen4, tarCen2, Color.White, scale);
                DrawWraper(w, position + new Vector2(0, offY * .5f), offset, active && groupBox.group.Index == c);//
                c++;
                position.Y += (wsize.Y + SequenceConfig.Instance.Step.Y + offY) * .5f;


            }
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.DarkCyan, 0, new Vector2(.5f), 16, 0, 0);
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.Cyan, 0, new Vector2(.5f), 12, 0, 0);
            if (SequenceConfig.Instance.ShowGroupBox)
                Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + groupBox.GetSize() * Vector2.UnitX * .5f, groupBox.GetSize()), GroupColor * .5f, 6);

            //锚点
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.Cyan * .5f, 0, new Vector2(.5f), 12, 0, 0);


        }
        public static void DrawSequence(SequenceBox sequenceBox, Vector2 position, Vector2 offsetFrame, bool active, bool start)
        {
            Color SequenceColor = Color.Red;
            var pos = position;
            if (!start)
                position.X += 16;
            int counter = 0;
            position.X += SequenceConfig.Instance.Step.X * .5f;//16
            Main.spriteBatch.DrawLine(pos, position - SequenceConfig.Instance.Step.X * .25f * Vector2.UnitX, Color.White);
            foreach (var g in sequenceBox.groupBoxes)
            {
                //绘制组之间的连接线
                if (counter < sequenceBox.groupBoxes.Count - 1)
                {
                    var p = position + (g.GetSize().X + SequenceConfig.Instance.Step.X * .25f) * Vector2.UnitX;// + 16
                                                                                                               //if (LogSpiralLibraryMod.ModTime % 60 < 30)
                    Main.spriteBatch.DrawLine(p, p + (SequenceConfig.Instance.Step.X * .5f) * Vector2.UnitX, Color.White);// 1f - 32
                }
                //绘制组，添加位置偏移
                DrawGroup(g, position, active && counter == sequenceBox.sequenceBase.Counter % sequenceBox.sequenceBase.GroupBases.Count);
                if (counter < sequenceBox.groupBoxes.Count - 1)
                    position.X += g.GetSize().X + SequenceConfig.Instance.Step.X;
                else
                    position.X += g.GetSize().X + SequenceConfig.Instance.Step.X;

                //position.X += g.GetSize().X + offset + SequenceConfig.Instance.Step.X;


                //计数器自增
                counter++;

            }
            Main.spriteBatch.DrawLine(pos + new Vector2(sequenceBox.GetSize().X, 0), position + new Vector2(-SequenceConfig.Instance.Step.X * .75f, 0), Color.White);//32


            //锚点
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), SequenceColor * .5f, 0, new Vector2(.5f), 8, 0, 0);
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos + new Vector2(sequenceBox.GetSize().X + (start ? 32 : 0), 0), new Rectangle(0, 0, 1, 1), Color.Red * .5f, 0, new Vector2(.5f), 8, 0, 0);

            if (SequenceConfig.Instance.ShowSequenceBox)
                Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + sequenceBox.GetSize() * Vector2.UnitX * .5f + offsetFrame, sequenceBox.GetSize()), SequenceColor * .5f);//以pos为左侧中心绘制矩形框框
        }
    }
    public class WraperBox : UIElement
    {
        public UIPanel panel;
        public Sequence.WraperBase wraper;
        public SequenceBox sequenceBox;
        public bool CacheRefresh;
        public bool chosen;
        public bool Dragging;
        public bool IsClone;
        public WraperBox Clone()
        {
            return new WraperBox(wraper.Clone());
        }
        public WraperBox(Sequence.WraperBase wraperBase)
        {
            wraper = wraperBase;
            if (wraper.IsSequence)
                sequenceBox = new SequenceBox(wraper.SequenceInfo);

        }
        public override void RightClick(UIMouseEvent evt)
        {
            if (!wraper.Available)
                return;

            if (IsClone)
            {
                Main.NewText(Language.GetOrRegister(SequenceUI.localizationPath + ".CantEditHere").Value);
                return;
            }
            if (evt.Target != this && !(this.BelongToMe(evt.Target) && evt.Target is not WraperBox)) return;
            //Main.NewText((GetHashCode(), evt.Target.GetHashCode()));
            var ui = SequenceSystem.instance.sequenceUI;
            var list = ui.propList;
            list.Clear();
            if (ui.currentWraper != null)
                ui.currentWraper.chosen = false;
            ui.currentWraper = this;
            chosen = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            int top = 0;
            int order = 0;
            {
                var fieldInfo = wraper.GetType().GetField("conditionDefinition", BindingFlags.Instance | BindingFlags.Public);
                if (fieldInfo != null)
                    SequenceSystem.WrapIt(list, ref top, new PropertyFieldWrapper(fieldInfo), wraper, order);
            }
            if (wraper.IsSequence)
            {
                if (!sequenceBox.Expand || sequenceBox.sequenceBase.FileName != Sequence.SequenceDefaultName)
                {
                    UIButton<string> TurnToButton = new(Language.GetOrRegister(SequenceUI.localizationPath + ".SwitchToSequencePage").Value);
                    TurnToButton.SetSize(new Vector2(0, 32), 0.8f, 0f);
                    TurnToButton.HAlign = 0.5f;
                    list.Add(TurnToButton);
                    TurnToButton.OnLeftClick += (evt, elem) =>
                    {
                        ui.SequenceToPage(sequenceBox);
                        ui.SwitchToSequencePage(sequenceBox);
                    };
                }
            }
            else
            {
                //wraper.SetConfigPanel(list);
                var props = ConfigManager.GetFieldsAndProperties(wraper.Element);
                foreach (PropertyFieldWrapper variable in props)
                {
                    if (!Attribute.IsDefined(variable.MemberInfo, typeof(ElementCustomDataAttribute)) || Attribute.IsDefined(variable.MemberInfo, typeof(ElementCustomDataAbabdonedAttribute)))
                        continue;
                    if (variable.Type == typeof(SeqDelegateDefinition))
                        continue;
                    var (container, elem) = SequenceSystem.WrapIt(list, ref top, variable, wraper.Element, order++);
                }
                foreach (PropertyFieldWrapper variable in props)
                {
                    if (variable.Type != typeof(SeqDelegateDefinition))
                        continue;
                    var (container, elem) = SequenceSystem.WrapIt(list, ref top, variable, wraper.Element, order++);
                }
            }
            base.RightClick(evt);
        }
        public override void MouseOver(UIMouseEvent evt)
        {
            //if (Dragging)
            //{
            //    Main.NewText((evt.Target.GetHashCode(),this.GetHashCode(),this.BelongToMe(evt.Target)));
            //}
            base.MouseOver(evt);
        }
        public void ParentCacheRefreshSet()
        {
            if (Parent is GroupBox gb)
            {
                gb.CacheRefresh = true;
                var sb = gb.Parent as SequenceBox;
                sb.CacheRefresh = true;
                if (sb.Parent is WraperBox wb)
                {
                    wb.CacheRefresh = true;
                    wb.ParentCacheRefreshSet();
                }
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (SequenceSystem.instance.sequenceUI.WorkingPlacePanel.Elements[0] is not SequenceBox box)
                return;
            SoundEngine.PlaySound(SoundID.CoinPickup);
            if (IsClone)
            {
                WraperBox copy = Clone();
                if (wraper.IsSequence)
                    copy.sequenceBox.Expand = false;
                copy.Dragging = true;
                copy.OnInitialize();
                SequenceSystem.instance.sequenceUI.OuterWorkingPanel.Append(copy);
                var vec = Main.MouseScreen - copy.Parent.GetDimensions().Position() - copy.GetSize() * .5f;
                copy.Top.Set(vec.X, 0);
                copy.Left.Set(vec.X, 0);
                copy.Recalculate();
                SequenceSystem.instance.userInterfaceSequence.LeftMouse.LastDown = copy;
            }
            else
            {
                if (this.Parent.Parent.GetHashCode() == box.GetHashCode() && box.groupBoxes.Count == 1 && box.groupBoxes.First().wraperBoxes.Count == 1)
                {
                    Main.NewText(Language.GetOrRegister(SequenceUI.localizationPath + ".LeastOneElementHint"));
                    return;

                }
                Dragging = true;

                if (Parent is GroupBox group)
                {
                    group.wraperBoxes.Remove(this);
                    this.Remove();
                    group.CacheRefresh = true;

                    SequenceBox sBox = (group.Parent as SequenceBox);
                    sBox.sequenceBase.Remove(wraper, group.group);
                    sBox.CacheRefresh = true;
                    if (group.wraperBoxes.Count == 0)
                    {
                        sBox.groupBoxes.Remove(group);
                        group.Remove();


                    }
                    if (sBox.Parent is WraperBox wb)
                    {
                        wb.CacheRefresh = true;
                        wb.WrapperSize();
                        wb.ParentCacheRefreshSet();
                    }
                    SequenceBox mainBox = SequenceSystem.instance.sequenceUI.WorkingPlacePanel.Elements[0] as SequenceBox;
                    mainBox.Elements.Clear();
                    mainBox.CacheRefresh = true;
                    mainBox.OnInitialize();
                    mainBox.Recalculate();
                }
                Remove();
                SequenceSystem.instance.sequenceUI.OuterWorkingPanel.Append(this);
            }
            base.LeftMouseDown(evt);
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if (!Dragging) return;
            if (!this.BelongToMe(evt.Target))
                return;
            SequenceUI sequenceUI = SequenceSystem.instance.sequenceUI;
            if (sequenceUI.WorkingPlacePanel.Elements[0] is not SequenceBox box)
            {
                return;
            }
            sequenceUI.OuterWorkingPanel.RemoveChild(this);
            box.InsertWraper(this, evt.MousePosition);
            sequenceUI.PendingModify = true;


            Dragging = false;
            base.LeftMouseUp(evt);
        }
        public override void Update(GameTime gameTime)
        {
            if (Dragging)
            {
                var vec = Main.MouseScreen - Parent.GetDimensions().Position() - this.GetSize() * .5f;
                Left.Set(vec.X, 0f);
                Top.Set(vec.Y, 0f);
                Recalculate();
            }
            MaxWidth = MaxHeight = new StyleDimension(223214514, 0);
            base.Update(gameTime);
        }
        public override void OnInitialize()
        {
            Vector2 size = this.GetSize();
            panel = new LogSpiralLibraryPanel();
            panel.SetSize(size);
            if (wraper.IsSequence && (sequenceBox.Expand && wraper.SequenceInfo.FileName == Sequence.SequenceDefaultName))
            {
                var desc = wraper.conditionDefinition;
                if (wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey)
                {
                    sequenceBox.offY = FontAssets.MouseText.Value.MeasureString("→" + desc.DisplayName).Y * -.5f;
                }
                //sequenceBox.Left.Set(SequenceConfig.Instance.Step.X * .5f, 0);
                sequenceBox.OnInitialize();
                Append(sequenceBox);
            }
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            //Vector4 vec = new Vector4(0,0, 0, 1.5f + MathF.Cos((float)LogSpiralLibrarySystem.ModTime / 60) * .5f);// MathF.Cos((float)LogSpiralLibrarySystem.ModTime / 60) * 60
            //object m = spriteBatch.GetType().GetField("transformMatrix", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(spriteBatch);
            //spriteBatch.GetType().GetField("transformMatrix", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(spriteBatch, Main.UIScaleMatrix with { /*M14 = vec.X,*/ M24 = vec.Y, M34 = vec.Z, M44 = vec.W });
            if (SequenceConfig.Instance.ShowWrapBox)
                spriteBatch.DrawRectangle(this.GetDimensions().ToRectangle(), Color.MediumPurple, 12);
            var desc = wraper.conditionDefinition.DisplayName;
            bool flag = wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey;
            var wraperBox = this;
            var position = this.GetDimensions().Position() + new Vector2(0, this.GetDimensions().Height * .5f);
            if (wraperBox.wraper.IsSequence && wraperBox.sequenceBox.Expand && wraperBox.wraper.SequenceInfo.FileName == Sequence.SequenceDefaultName)
            {
                ComplexPanelInfo panel = new();
                var boxSize = wraperBox.WrapperSize();
                //if (flag)
                //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f + new Vector2(0, 16), boxSize + new Vector2(32, 64));
                //else
                //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f, boxSize + new Vector2(32, 32));
                float offY = flag ? FontAssets.MouseText.Value.MeasureString(desc).Y : 0;
                panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X * .5f, 0), boxSize);
                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
                panel.glowEffectColor = chosen ? Main.DiscoColor with { A = 0 } : Color.MediumPurple with { A = 102 };
                panel.glowShakingStrength = .1f;
                panel.glowHueOffsetRange = 0.1f;
                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
                panel.DrawComplexPanel(spriteBatch);
                //DrawSequence(wraperBox.sequenceBox, position - offY * .5f * Vector2.UnitY, offY * .5f * Vector2.UnitY, active, false);
                if (flag)
                {
                    var cen = position + boxSize * Vector2.UnitY * .5f - offY * 1.5f * Vector2.UnitY + new Vector2(16, 0);
                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen, Color.White);
                }
            }
            else
            {
                //position.Y += wraperBox.GetSize().Y * .5f;
                var font = FontAssets.MouseText.Value;
                var name = wraperBox.wraper.Name;
                if (wraper.IsSequence && SequenceSystem.sequenceInfos.TryGetValue(wraper.SequenceInfo.KeyName, out var value))
                    name = value.DisplayName;
                Vector2 textSize = font.MeasureString(name);
                Vector2 boxSize = wraperBox.WrapperSize();
                #region 框框
                ComplexPanelInfo panel = new();
                panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X, 0) * .5f, boxSize);
                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
                if (!wraper.Available)
                    panel.glowEffectColor = Color.Gray;
                else if (wraper.IsSequence)
                    panel.glowEffectColor = chosen ? Color.Lerp(Color.Blue, Color.Purple, 0.5f) with { A = 0 } : Color.MediumPurple with { A = 102 };
                else
                    panel.glowEffectColor = (chosen ? Color.Red : Color.Cyan) with { A = 0 };
                panel.glowShakingStrength = .05f;
                panel.glowHueOffsetRange = 0.05f;
                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
                panel.DrawComplexPanel(spriteBatch);
                #endregion
                var cen = position + new Vector2(16) - boxSize * Vector2.UnitY * .5f;
                spriteBatch.DrawString(font, name, cen, Color.White, 0, default, 1f, 0, 0);
                cen += textSize * Vector2.UnitY;
                if (flag)
                {
                    spriteBatch.DrawString(font, "→" + desc, cen, Color.White);
                }
                //spriteBatch.DrawRectangle(panel.destination, Color.MediumPurple);
                if (chosen)
                {
                    var tarVec = SequenceSystem.instance.sequenceUI.propList.GetDimensions().ToRectangle().TopRight();
                    spriteBatch.DrawHorizonBLine(tarVec, position, Color.White with { A = 0 } * .125f);
                    spriteBatch.DrawHorizonBLine(tarVec + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4), position + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4), Main.DiscoColor with { A = 0 } * .125f, 1, 6);

                }
            }

            base.DrawSelf(spriteBatch);
        }
    }
    public class GroupBox : UIElement
    {
        public GroupBox(Sequence.GroupBase groupBase)
        {
            group = groupBase;
            foreach (var w in group.Wrapers)
            {
                var wbox = new WraperBox(w);
                wraperBoxes.Add(wbox);

            }
        }
        public UIPanel panel;
        public Sequence.GroupBase group;
        public List<WraperBox> wraperBoxes = [];
        public bool CacheRefresh;
        //public void Add(WraperBox wraperBox)
        //{
        //    wraperBoxes.Add(wraperBox);
        //    Elements.Clear();
        //    group.Wrapers.Add(wraperBox.wraper);
        //    OnInitialize();
        //}
        public override void OnInitialize()
        {
            //this.IgnoresMouseInteraction = true;
            Vector2 size = this.GetSize();
            panel = new LogSpiralLibraryPanel();
            panel.SetSize(size);
            Elements.Clear();
            float offset = SequenceConfig.Instance.Step.Y * .5f;
            foreach (var w in wraperBoxes)
            {
                w.Top.Set(offset, 0f);
                Append(w);
                var dimension = w.GetDimensions();
                offset += dimension.Height + SequenceConfig.Instance.Step.Y;
                w.Left.Set((size.X - dimension.Width) * .5f, 0f);
                w.OnInitialize();
            }
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (SequenceConfig.Instance.ShowGroupBox)
                spriteBatch.DrawRectangle(this.GetDimensions().ToRectangle(), Color.Cyan * .75f, 8);
            var dimension = GetDimensions();
            var scale = 1 + ((float)LogSpiralLibraryMod.ModTime / 180).CosFactor();

            foreach (var w in wraperBoxes)
            {
                var wD = w.GetDimensions();
                var offY = w.wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey ? -FontAssets.MouseText.Value.MeasureString("→" + w.wraper.Condition.Description.Value).Y * .5f : 0;
                //谜题之我也不知道这一个像素的偏移是哪里来的
                spriteBatch.DrawHorizonBLine(dimension.Position() + new Vector2(-SequenceConfig.Instance.Step.X * .25f, dimension.Height * .5f) + new Vector2(-1, 0), wD.Position() + new Vector2(0, wD.Height * .5f + offY), Color.White, scale);
                spriteBatch.DrawHorizonBLine(dimension.Position() + new Vector2(SequenceConfig.Instance.Step.X * .25f + dimension.Width, dimension.Height * .5f) + new Vector2(-1, 0), wD.Position() + new Vector2(wD.Width, wD.Height * .5f + offY), Color.White, scale);

                //spriteBatch.Draw(TextureAssets.MagicPixel.Value, wD.Position() + new Vector2(0, wD.Height * .5f), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(0.5f), 16f, 0, 0);
            }
            base.DrawSelf(spriteBatch);
        }
        public override void Update(GameTime gameTime)
        {
            MaxWidth = MaxHeight = new StyleDimension(223214514, 0);

            base.Update(gameTime);
        }
    }
    public class SequenceBox : UIElement
    {
        bool expand = true;
        public bool Expand
        {
            get => expand;
            set
            {
                bool flag = expand ^ value;
                if (!flag) return;
                expand = value;
                if (expand)
                {
                    CacheRefresh = true;
                    OnInitialize();
                }
                else
                {
                    Elements.Clear();
                    this.CacheRefresh = true;
                    this.SequenceSize(true);
                }
            }
        }
        public UIPanel panel;
        Sequence seq;
        public Sequence sequenceBase
        {
            get => seq;
            set
            {
                string name = seq?.DisplayName;
                seq = value;
            }
        }
        public List<GroupBox> groupBoxes = [];
        public bool CacheRefresh;
        public float offY;
        //public bool startSequence;
        public SequenceBox(Sequence sequence)
        {
            sequenceBase = sequence;
            foreach (var g in sequence.GroupBases)
            {
                var gbox = new GroupBox(g);

                groupBoxes.Add(gbox);

            }
        }
        /// <summary>
        /// 检测鼠标是否在空隙中(也就是非打包器区域
        /// </summary>
        /// <returns></returns>
        public bool MouseCheckInEmptySpace(Vector2 position)
        {
            foreach (var g in groupBoxes)
            {
                foreach (var w in g.wraperBoxes)
                {
                    if (w.GetDimensions().ToRectangle().Contains(position.ToPoint()))
                        return false;
                }
            }
            return true;
        }
        public void InsertWraper(WraperBox wraper, Vector2 center)
        {
            if (!ContainsPoint(center))
                return;
            int gIndex = 0;
            bool inGroup = false;
            foreach (var g in groupBoxes)
            {
                if (center.X > g.GetDimensions().ToRectangle().Right)
                    gIndex++;
                else if (center.X > g.GetDimensions().ToRectangle().Left)
                    inGroup = true;
            }
            if (inGroup)
            {
                int wIndex = 0;
                bool inWraper = false;
                GroupBox groupBox = groupBoxes[gIndex];
                foreach (var w in groupBox.wraperBoxes)
                {
                    if (center.Y > w.GetDimensions().ToRectangle().Bottom)
                        wIndex++;
                    else if (center.Y > w.GetDimensions().ToRectangle().Top)
                        inWraper = true;
                }
                if (inWraper)
                {
                    WraperBox wraperBox = groupBox.wraperBoxes[wIndex];
                    if (wraperBox.wraper.IsSequence)
                    {
                        wraperBox.sequenceBox.InsertWraper(wraper, center);
                        wraperBox.SetSize(wraperBox.sequenceBox.GetSize());
                        groupBox.CacheRefresh = true;
                        groupBox.GroupSize();
                    }
                    else
                    {
                        //出口三，和先前的元素组成新的序列
                        //Main.NewText("这里本来有个出口三，但是我还没做完((");

                        bool flag = center.X < wraperBox.GetDimensions().Center().X;

                        var seq = (Sequence)Activator.CreateInstance(this.sequenceBase.GetType());
                        var nW = (Sequence.WraperBase)Activator.CreateInstance(wraper.wraper.GetType(), seq);
                        for (int n = 0; n < 2; n++)
                        {
                            seq.Add(flag ? wraper.wraper : wraperBox.wraper, out _);
                            flag = !flag;
                        }
                        groupBox.group.Replace(wIndex, nW);
                        groupBox.wraperBoxes[wIndex] = new WraperBox(nW);
                        groupBox.CacheRefresh = true;
                    }
                    wraperBox.ParentCacheRefreshSet();
                }
                else
                {
                    //出口二，作为独立单元并联
                    this.sequenceBase.GroupBases[gIndex].Insert(wIndex, wraper.wraper);
                    this.groupBoxes[gIndex].wraperBoxes.Insert(wIndex, wraper);
                    this.groupBoxes[gIndex].CacheRefresh = true;
                }
            }
            else
            {
                //出口一，作为独立单元串联
                this.sequenceBase.Insert(gIndex, wraper.wraper, out var nG);
                groupBoxes.Insert(gIndex, new GroupBox(nG));
                if (wraper.wraper.IsSequence)
                    groupBoxes[gIndex].wraperBoxes[0].sequenceBox.expand = wraper.sequenceBox.expand;

            }
            var box = this;
            box.Elements.Clear();
            box.CacheRefresh = true;
            box.OnInitialize();
            box.Recalculate();
        }
        public override void Update(GameTime gameTime)
        {
            if (CacheRefresh)
                Recalculate();
            MaxWidth = MaxHeight = new StyleDimension(223214514, 0);
            base.Update(gameTime);
        }
        public override void OnInitialize()
        {
            //this.IgnoresMouseInteraction = true;
            Vector2 size = this.SequenceSize(true);
            panel = new LogSpiralLibraryPanel();
            panel.SetSize(size);
            float offset = SequenceConfig.Instance.Step.X * .5f + 16;
            //if (!startSequence)
            //{
            //    offset += 16;
            //}
            //startSequence = false;
            foreach (var g in groupBoxes)
            {
                g.Left.Set(offset, 0f);
                Append(g);
                var dimension = g.GetDimensions();
                offset += dimension.Width + SequenceConfig.Instance.Step.X;
                g.Top.Set((size.Y - dimension.Height) * .5f + offY, 0f);
                g.OnInitialize();
            }
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dimension = GetDimensions();
            Vector2 pos = dimension.Position() + new Vector2(0, dimension.Height * .5f + offY);
            Vector2 startP = pos + new Vector2(SequenceConfig.Instance.Step.X * .25f + 16 - 1, 0);//(startSequence ? 0 : 16)
            spriteBatch.DrawLine(pos - Vector2.UnitX, startP, Color.White);
            int counter = 0;
            foreach (var g in groupBoxes)
            {
                counter++;
                startP += new Vector2(SequenceConfig.Instance.Step.X * .5f + g.GetDimensions().Width, 0);
                Vector2 endP = startP + new Vector2(SequenceConfig.Instance.Step.X * 0.5f, 0);
                if (counter == groupBoxes.Count)
                    endP = pos + new Vector2(dimension.Width - 1, 0);
                spriteBatch.DrawLine(startP, endP, Color.White);
                startP = endP;
            }
            if (SequenceConfig.Instance.ShowSequenceBox)
                spriteBatch.DrawRectangle(dimension.ToRectangle(), Color.Red * .5f);
            base.DrawSelf(spriteBatch);
        }
    }
    public static class SequenceDrawHelper
    {
        public static Vector2 WrapperSize(this WraperBox wrapBox)
        {
            Vector2 curr = wrapBox.GetSize();
            if (curr == default || wrapBox.CacheRefresh)
            {
                wrapBox.CacheRefresh = false;
                Vector2 delta;
                var wraper = wrapBox.wraper;
                var desc = wraper.conditionDefinition;
                if (wraper.IsSequence && wrapBox.sequenceBox.Expand && wraper.SequenceInfo.FileName == Sequence.SequenceDefaultName)
                {
                    delta = SequenceSize(wrapBox.sequenceBox);
                    if (wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey)
                    {
                        var textSize = FontAssets.MouseText.Value.MeasureString("→" + desc.DisplayName);
                        delta.Y += textSize.Y;
                        delta.X = Math.Max(textSize.X, delta.X);
                    }
                    delta += new Vector2(32, 32);
                    wrapBox.sequenceBox.SetSize(delta);
                    wrapBox.sequenceBox.Recalculate();
                }
                else
                {
                    var font = FontAssets.MouseText.Value;
                    var name = wraper.Name;
                    if (wraper.IsSequence && SequenceSystem.sequenceInfos.TryGetValue(wraper.SequenceInfo.KeyName, out var value))
                        name = value.DisplayName;
                    //if (name == "挥砍" && desc != "Always") 
                    //{
                    //    Main.NewText((font.MeasureString(name), font.MeasureString(desc)));
                    //}
                    Vector2 textSize = font.MeasureString(name);
                    Vector2 boxSize = textSize;
                    if (wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey)
                    {
                        Vector2 descSize = font.MeasureString("→" + desc.DisplayName);
                        boxSize.Y += descSize.Y;
                        boxSize.X = Math.Max(textSize.X, descSize.X);
                    }
                    boxSize += Vector2.One * 32;
                    delta = boxSize;
                }
                wrapBox.SetSize(delta);
                wrapBox.Recalculate();

                return delta;
            }
            return curr;
        }
        public static Vector2 GroupSize(this GroupBox groupBox)
        {
            Vector2 curr = groupBox.GetSize();
            if (curr == default || groupBox.CacheRefresh)
            {
                groupBox.CacheRefresh = false;
                Vector2 result = default;
                foreach (var wrapper in groupBox.wraperBoxes)
                {
                    //if (!wrapper.wraper.Available) continue;
                    Vector2 delta = WrapperSize(wrapper);
                    result.Y += delta.Y + SequenceConfig.Instance.Step.Y;
                    if (delta.X > result.X) result.X = delta.X;
                }
                groupBox.SetSize(result);
                groupBox.Recalculate();
                return result;
            }
            return curr;
        }
        public static Vector2 SequenceSize(this SequenceBox sequencebox, bool start = false)
        {

            Vector2 curr = sequencebox.GetSize();
            if (curr == default || sequencebox.CacheRefresh)
            {
                sequencebox.CacheRefresh = false;
                Vector2 result = new(start ? 32 : 0, 0);//sequencebox.startSequence // SequenceConfig.Instance.Step.X * .5f
                foreach (var group in sequencebox.groupBoxes)
                {
                    Vector2 delta = GroupSize(group);
                    result.X += delta.X + SequenceConfig.Instance.Step.X;
                    if (delta.Y > result.Y) result.Y = delta.Y;
                }

                sequencebox.SetSize(result);
                sequencebox.Recalculate();
                //if (Main.chatMonitor is RemadeChatMonitor remade)
                //{
                //    remade._showCount = 40;
                //}
                return result;
            }
            return curr;
        }
        //static void DrawWrapper(this SpriteBatch spriteBatch, MeleeSequence.MeleeSAWraper meleeSAWraper, Vector2 position)
        //{
        //    /*
        //    Vector2 size = MeleeWrapperSize(meleeSAWraper);
        //    if (meleeSAWraper.IsSequence)
        //    {

        //    }
        //    else 
        //    {
        //        var font = FontAssets.MouseText.Value;
        //        var name = wrapper.attackInfo.GetType().Name;
        //        Vector2 textSize = font.MeasureString(name);
        //        Vector2 boxSize = textSize;
        //        if (desc != "Always")
        //        {
        //            Vector2 descSize = font.MeasureString(desc);
        //            boxSize.Y += descSize.Y;
        //            boxSize.X = Math.Max(textSize.X, descSize.X);
        //        }
        //        boxSize += Vector2.One * 32;
        //        #region 框框
        //        ComplexPanelInfo panel = new ComplexPanelInfo();
        //        panel.destination = Utils.CenteredRectangle(positionHere + boxSize * .5f - new Vector2(16), boxSize);
        //        panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_0").Value;
        //        panel.glowEffectColor = Color.White with { A = 0 };
        //        panel.glowShakingStrength = 0;
        //        panel.glowHueOffsetRange = 0;
        //        panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
        //        panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
        //        panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
        //        panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
        //        panel.DrawComplexPanel(spriteBatch);
        //        #endregion
        //        spriteBatch.DrawString(FontAssets.MouseText.Value, name, positionHere, wrapper.attackInfo.timerMax > 0 ? Color.Cyan : Color.Gray, 0, default, 1f, 0, 0);
        //        if (desc != "Always")
        //        {
        //            spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, positionHere + textSize.Y * Vector2.UnitY, wrapper.condition.IsMet() ? Color.MediumPurple : Color.Gray);
        //        }
        //    }
        //}
        //public static void DrawMeleeSequence(this SpriteBatch spriteBatch, MeleeSequence meleeSequence, Vector2 position)
        //{
        //    int counterX = 0;
        //    foreach (var group in meleeSequence.MeleeGroups)
        //    {
        //        int counterY = 0;
        //        foreach (var wrapper in group.wrapers)
        //        {
        //            Vector2 positionHere = new Vector2(counterX, counterY) * new Vector2(128, 64) * 2 + position;
        //            var desc = wrapper.condition.Description.Value;

        //            if (wrapper.IsElement)
        //            {
        //                //if (wrapper.attackInfo.SkipCheck()) break;
        //                var font = FontAssets.MouseText.Value;
        //                var name = wrapper.attackInfo.GetType().Name;
        //                Vector2 textSize = font.MeasureString(name);
        //                Vector2 boxSize = textSize;
        //                if (desc != "Always")
        //                {
        //                    Vector2 descSize = font.MeasureString(desc);
        //                    boxSize.Y += descSize.Y;
        //                    boxSize.X = Math.Max(textSize.X, descSize.X);
        //                }
        //                boxSize += Vector2.One * 32;
        //                #region 框框
        //                ComplexPanelInfo panel = new ComplexPanelInfo();
        //                panel.destination = Utils.CenteredRectangle(positionHere + boxSize * .5f - new Vector2(16), boxSize);
        //                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_0").Value;
        //                panel.glowEffectColor = Color.White with { A = 0 };
        //                panel.glowShakingStrength = 0;
        //                panel.glowHueOffsetRange = 0;
        //                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
        //                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
        //                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
        //                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
        //                panel.DrawComplexPanel(spriteBatch);
        //                #endregion
        //                spriteBatch.DrawString(FontAssets.MouseText.Value, name, positionHere, wrapper.attackInfo.timerMax > 0 ? Color.Cyan : Color.Gray, 0, default, 1f, 0, 0);
        //                if (desc != "Always")
        //                {
        //                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, positionHere + textSize.Y * Vector2.UnitY, wrapper.condition.IsMet() ? Color.MediumPurple : Color.Gray);

        //                }

        //            }
        //            if (wrapper.IsSequence)
        //            {

        //                spriteBatch.DrawMeleeSequence(wrapper.sequenceInfo, positionHere/*, depth + 1, out Vector2 _finalSize*/);
        //                if (desc != "Always")
        //                {
        //                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, positionHere + new Vector2(0, 64), wrapper.condition.IsMet() ? Color.MediumPurple : Color.Gray);
        //                }
        //            }
        //            counterY++;
        //        }
        //        counterX++;
        //    }
        //}
    }


}
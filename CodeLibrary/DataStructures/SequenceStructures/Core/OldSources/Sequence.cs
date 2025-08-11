//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading;
//using System.Xml;
//using System.Xml.Serialization;
//using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
//using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
//using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
//using Microsoft.Xna.Framework.Input;
//using Terraria.Audio;
//using Terraria.GameContent.UI.Elements;
//using Terraria.Localization;
//using Terraria.ModLoader.Config;
//using Terraria.ModLoader.IO;
//namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.OldSources
//{
//    /// <summary>
//    /// 去泛型化的序列基类
//    /// </summary>
//    [XmlRoot("Sequence")]
//    public abstract class Sequence
//    {
//        //public abstract GroupBase CreateSimpleGroup(WraperBase wraperBase);
//        public const string SequenceDefaultName = "My Sequence";
//        public abstract void SyncInfo(SequenceBasicInfo info);
//        public abstract void Remove(WraperBase target, GroupBase owner);
//        public abstract void Add(WraperBase wraperBase, out GroupBase newGroup);
//        public abstract void Add(GroupBase groupBase);
//        public abstract void Insert(int index, WraperBase wraperBase, out GroupBase newGroup);
//        public abstract void Insert(int index, GroupBase groupBase);

//        public abstract void Save();

//        public abstract Sequence Clone();

//        public abstract void WriteContent(XmlWriter xmlWriter);

//        public abstract void Reset();

//        [XmlRoot("Group")]
//        public abstract class GroupBase
//        {
//            public abstract IReadOnlyList<WraperBase> Wrapers { get; }
//            public abstract int Index { get; }
//            public abstract void Insert(int index, WraperBase wraperBase);
//            public abstract void Replace(int index, WraperBase wraperBase);
//            //public abstract GroupBase CreateFromWraper(WraperBase wraper);
//        }
//        [XmlRoot("Sequence")]
//        public abstract class WraperBase
//        {
//            public abstract ISequenceElement Element { get; }
//            public abstract bool IsElement { get; }
//            public string LoadFailedMetaName = "";
//            public bool LoadFailedMetaSequence = false;
//            public Dictionary<string, string> LoadFailedMetaAttributes;
//            public abstract string Name { get; }
//            public abstract Sequence SequenceInfo { get; }
//            //public abstract void SetConfigPanel(UIList uIList);
//            public bool IsSequence
//            {
//                get
//                {
//                    if (LoadFailedMetaSequence)
//                        TryLoadSequenceAgain();
//                    return SequenceInfo != null;
//                }

//            }

//            public bool Available => IsSequence || IsElement;
//            //[CustomSeqConfigItem(typeof(ConditionDefinitionElement))]
//            public ConditionDefinition conditionDefinition = new("LogSpiralLibrary", "Always");
//            public Entity owner;
//            public Condition Condition => SequenceSystem.ToEntityCondition(
//                conditionDefinition.Name,
//                $"mods.{conditionDefinition.Mod}.Condition.{conditionDefinition.Name}",
//                owner) ??
//                SequenceSystem.Conditions[conditionDefinition.Name == "None" ? "Always" : conditionDefinition.Name];

//            //public Condition condition = new Condition("Always", () => true);
//            public bool Active { get; set; }
//            public abstract WraperBase Clone();
//            public abstract void TryLoadSequenceAgain();

//        }
//        public abstract IReadOnlyList<GroupBase> GroupBases { get; }
//        /// <summary>
//        /// 当前序列的名字
//        /// </summary>
//        [XmlAttribute("name")]
//        public abstract string LocalPath { get; }
//        public abstract string FileName { get; }
//        public abstract string DisplayName { get; }
//        public abstract string KeyName { get; }
//        /// <summary>
//        /// 目前执行到第几个组
//        /// </summary>
//        [XmlIgnore]
//        public abstract int Counter { get; }
//        public abstract Mod Mod { get; }
//        public abstract string ElementTypeName { get; }
//        public abstract bool Active { get; set; }

//    }
//    public class Sequence<T> : Sequence where T : ISequenceElement
//    {

//        //public override GroupBase CreateSimpleGroup(WraperBase wraperBase)
//        //{
//        //    var result = new Group();
//        //    result.wrapers.Add((Wraper)wraperBase);
//        //    return result;
//        //}
//        public override void Reset()
//        {
//            var seq = new Sequence<T>();
//            Load($"PresetSequences/{ElementTypeName}/{FileName}.xml", mod, null, seq);
//            SequenceManager<T>.Sequences[seq.KeyName] = seq;
//        }
//        public override string LocalPath => $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{ElementTypeName}/{mod.Name}/{sequenceName}.xml";
//        public override string KeyName => $"{mod.Name}/{sequenceName}";
//        public override string DisplayName => SequenceSystem.sequenceInfos[KeyName]?.DisplayName ?? sequenceName;
//        public override Sequence Clone()
//        {
//            const bool useTempPath = false;
//            if (!useTempPath)
//            {
//                XmlWriterSettings settings = new();
//                settings.Indent = true;
//                settings.Encoding = new UTF8Encoding(false);
//                settings.NewLineChars = Environment.NewLine;
//                MemoryStream stream = new();
//                XmlWriter xmlWriter = XmlWriter.Create(stream, settings);
//                WriteContent(xmlWriter);
//                xmlWriter.Dispose();
//                byte[] data = stream.ToArray();
//                stream.Dispose();

//                stream = new MemoryStream(data);
//                XmlReader xmlReader = XmlReader.Create(stream);
//                xmlReader.Read();//读取声明
//                xmlReader.Read();//读取空格
//                ReadSequence(xmlReader, mod.Name, out var result);
//                result.mod = mod;
//                xmlReader.Dispose();
//                stream.Dispose();
//                return result;
//            }
//            else
//            {
//                XmlWriterSettings settings = new();
//                settings.Indent = true;
//                settings.Encoding = new UTF8Encoding(false);
//                settings.NewLineChars = Environment.NewLine;
//                string dire = $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{ElementTypeName}/{Mod.Name}";
//                string tempPath = $"{dire}/{SequenceDefaultName}.xml";
//                if (!Directory.Exists(dire)) Directory.CreateDirectory(dire);
//                using XmlWriter xmlWriter = XmlWriter.Create(tempPath, settings);
//                WriteContent(xmlWriter);
//                xmlWriter.Dispose();
//                //TextReader reader = new StringReader(tempPath);
//                using XmlReader xmlReader = XmlReader.Create(tempPath);
//                xmlReader.Read();//读取声明
//                xmlReader.Read();//读取空格
//                ReadSequence(xmlReader, mod.Name, out var result);
//                result.mod = mod;
//                xmlReader.Dispose();
//                File.Delete(tempPath);
//                return result;
//            }

//        }

//        public Sequence<T> LocalSequenceClone(string inModDirectoryPath) 
//        {
//            XmlWriterSettings settings = new();
//            settings.Indent = true;
//            settings.Encoding = new UTF8Encoding(false);
//            settings.NewLineChars = Environment.NewLine;
//            MemoryStream stream = new();
//            XmlWriter xmlWriter = XmlWriter.Create(stream, settings);
//            WriteContent(xmlWriter);
//            xmlWriter.Dispose();
//            byte[] data = stream.ToArray();
//            stream.Dispose();

//            stream = new MemoryStream(data);
//            XmlReader xmlReader = XmlReader.Create(stream);
//            xmlReader.Read();//读取声明
//            xmlReader.Read();//读取空格
//            var result = new Sequence<T>();
//            ReadSequence(xmlReader, mod.Name,inModDirectoryPath, result);
//            result.mod = mod;
//            xmlReader.Dispose();
//            stream.Dispose();
//            return result;
//        }
//        //public void ReconstructCondition(Entity owner)
//        //{
//        //    foreach (var g in groups)
//        //    {
//        //        foreach (var w in g.wrapers)
//        //        {
//        //            if (w.IsSequence)
//        //            {
//        //                w.sequenceInfo.ReconstructCondition(owner);
//        //            }
//        //            string key = w.conditionDefinition.Name;
//        //            var condition = SequenceSystem.ToEntityCondition(key, $"", owner);
//        //            if (condition != null) w.SetCondition(condition);
//        //        }
//        //    }
//        //}

//        public override void WriteContent(XmlWriter xmlWriter)
//        {
//            try
//            {
//                xmlWriter.WriteStartElement("Sequence");
//                if (FileName != SequenceDefaultName)
//                {
//                    xmlWriter.WriteAttributeString("name", FileName);
//                    SequenceSystem.sequenceInfos[$"{Mod.Name}/{FileName}"].Save(xmlWriter);
//                }
//                for (int i = 0; i < Groups.Count; i++)
//                {
//                    xmlWriter.WriteStartElement("Group");
//                    Group group = Groups[i];
//                    for (int j = 0; j < group.wrapers.Count; j++)
//                    {
//                        Wraper wraper = group.wrapers[j];
//                        xmlWriter.WriteStartElement("Wraper");
//                        if (wraper.conditionDefinition.Name != "Always" && wraper.conditionDefinition.Name != "None")
//                            xmlWriter.WriteAttributeString("condition", wraper.Condition.Description.Key.Split('.')[^1]);
//                        //xmlWriter.WriteValue(wraper.Name);
//                        if (!wraper.Available)
//                        {
//                            if (wraper.LoadFailedMetaSequence)
//                            {
//                                xmlWriter.WriteAttributeString("IsSequence", "True");
//                                var metaName = wraper.LoadFailedMetaName;
//                                var subs = metaName.Split("/");
//                                var name = "";
//                                foreach (var sub in subs[1..])
//                                    name += sub;
//                                //if (wraper.SequenceInfo.Mod == null) Main.NewText(wraper.Name);
//                                xmlWriter.WriteAttributeString("Mod", subs[0]);
//                                xmlWriter.WriteValue(name);
//                            }
//                            else
//                            {
//                                xmlWriter.WriteStartElement("Action");
//                                foreach (var pair in wraper.LoadFailedMetaAttributes)
//                                    xmlWriter.WriteAttributeString(pair.Key, pair.Value);
//                                xmlWriter.WriteAttributeString("name", wraper.LoadFailedMetaName);
//                                xmlWriter.WriteEndElement();
//                            }
//                        }
//                        else if (wraper.IsSequence)
//                        {
//                            xmlWriter.WriteAttributeString("IsSequence", wraper.IsSequence.ToString());
//                            if (wraper.Name == SequenceDefaultName)
//                                ((Sequence<T>)wraper.SequenceInfo).WriteContent(xmlWriter);
//                            else
//                            {
//                                //if (wraper.SequenceInfo.Mod == null) Main.NewText(wraper.Name);
//                                xmlWriter.WriteAttributeString("Mod", wraper.SequenceInfo.Mod.Name);
//                                xmlWriter.WriteValue(wraper.sequenceInfo.sequenceName);
//                            }
//                        }
//                        else
//                        {
//                            xmlWriter.WriteStartElement("Action");
//                            wraper.elementInfo.SaveAttribute(xmlWriter);
//                            xmlWriter.WriteAttributeString("name", wraper.elementInfo.FullName);
//                            xmlWriter.WriteEndElement();
//                        }
//                        xmlWriter.WriteEndElement();
//                    }
//                    xmlWriter.WriteEndElement();
//                }
//                xmlWriter.WriteEndElement();
//            }
//            catch (Exception e)
//            {
//                var str = e.Message;
//            }
//        }
//        public override void Save() => Save($"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{ElementTypeName}/{Mod.Name}/{FileName}.xml");
//        public void Save(string path)
//        {
//            XmlWriterSettings settings = new();
//            //要求缩进
//            settings.Indent = true;
//            //注意如果不设置encoding默认将输出utf-16
//            //注意这儿不能直接用Encoding.UTF8如果用Encoding.UTF8将在输出文本的最前面添加4个字节的非xml内容
//            settings.Encoding = new UTF8Encoding(false);

//            //设置换行符
//            settings.NewLineChars = Environment.NewLine;
//            if (!Directory.Exists(Path.GetDirectoryName(path)))
//                Directory.CreateDirectory(Path.GetDirectoryName(path));
//            using XmlWriter xmlWriter = XmlWriter.Create(path, settings);
//            WriteContent(xmlWriter);

//        }
//        public bool active;
//        public override bool Active { get => active; set => active = value; }
//        public static void Load(string pathInMod, Mod mod, string inModDirectoryPath, Sequence<T> target)
//        {
//            Stream stream = mod.GetFileStream(pathInMod);
//            byte[] bytes;
//            using (MemoryStream memoryStream = new MemoryStream())
//            {
//                stream.CopyTo(memoryStream);
//                bytes = memoryStream.ToArray();
//            }
//            stream.Dispose();
//            using (MemoryStream memoryStream = new MemoryStream(bytes))
//            {
//                using XmlReader xmlReader = XmlReader.Create(memoryStream);
//                //stream.Close();
//                xmlReader.Read();//读取声明
//                xmlReader.Read();//读取空格
//                target.groups.Clear();
//                //var modName = path.Split('\\', '/')[^2];
//                ReadSequence(xmlReader, mod.Name, inModDirectoryPath, target);
//                target.mod = mod;
//            }
//        }
//        public static void Load(string path, Sequence<T> target)
//        {
//            using XmlReader xmlReader = XmlReader.Create(path);
//            xmlReader.Read();//读取声明
//            xmlReader.Read();//读取空格
//            target.groups.Clear();
//            var modName = path.Split('\\', '/')[^2];
//            ReadSequence(xmlReader, modName, null, target);
//            target.mod = ModLoader.GetMod(modName);
//        }
//        public static Sequence<T> Load(string path)
//        {
//            using XmlReader xmlReader = XmlReader.Create(path);
//            var modName = path.Split('\\', '/')[^2];
//            return Load(xmlReader, modName);
//        }
//        public static Sequence<T> Load(XmlReader xmlReader, string modName)
//        {
//            xmlReader.Read();//读取声明
//            xmlReader.Read();//读取空格
//            ReadSequence(xmlReader, modName, out var result);
//            result.mod = ModLoader.GetMod(modName);
//            return result;
//        }
//        public static bool ReadSequence(XmlReader xmlReader, string modName, string inModDirectoryPath, Sequence<T> empty)
//        {
//            xmlReader.Read();//读取序列节点开始部分
//            if (xmlReader.Name != "Sequence")
//            {
//                xmlReader.Read();
//                return false;
//            }
//            else
//            {
//                var fileName = empty.sequenceName = xmlReader["name"] ?? SequenceDefaultName;
//                if (fileName != SequenceDefaultName)
//                    SequenceSystem.sequenceInfos[$"{modName}/{fileName}"] = new SequenceBasicInfo().Load(xmlReader);
//                xmlReader.Read();//读取空格
//                while (Group.ReadGroup(xmlReader, inModDirectoryPath, out var groupResult))
//                {
//                    empty.Add(groupResult);
//                }
//                return true;
//            }
//        }
//        public static bool ReadSequence(XmlReader xmlReader, string modName, out Sequence<T> result)
//        {
//            result = new Sequence<T>();
//            if (!ReadSequence(xmlReader, modName, null, result))
//            {
//                result = null;
//                return false;
//            }
//            return true;
//        }
//        public class Group : GroupBase
//        {
//            //public override GroupBase CreateFromWraper(WraperBase wraper)
//            //{
//            //    return new Group();
//            //}
//            public override void Insert(int index, WraperBase wraperBase)
//            {
//                wrapers.Insert(index, (Wraper)wraperBase);
//            }
//            public override void Replace(int index, WraperBase wraperBase)
//            {
//                wrapers[index] = (Wraper)wraperBase;
//            }
//            public static bool ReadGroup(XmlReader xmlReader, string inModDirectoryPath, out Group result)
//            {
//                xmlReader.Read();//读取下一个位置
//                if (xmlReader.Name != "Group")//此时实际上是</Sequence>
//                {
//                    xmlReader.Read();//顺手把它的下一个空格读取了
//                    result = null;
//                    return false;
//                }
//                else
//                {
//                    xmlReader.Read();//读取空格
//                    result = new Group();
//                    while (Wraper.ReadWraper(xmlReader, inModDirectoryPath, out Wraper wraperResult))
//                    {
//                        result.wrapers.Add(wraperResult);
//                    }
//                    return true;
//                }
//            }
//            /// <summary>
//            /// 请不要对这个Add
//            /// TODO 改为ReadOnly
//            /// </summary>
//            public override IReadOnlyList<WraperBase> Wrapers => (from w in wrapers select (WraperBase)w).ToList();
//            public List<Wraper> wrapers = [];
//            public override int Index => index;
//            public int index;
//            public Wraper GetCurrentWraper()
//            {
//                int counter = 0;
//                foreach (var wraper in wrapers)
//                {
//                    if (wraper.Available && wraper.Condition.IsMet())
//                    {
//                        index = counter;
//                        return wraper;
//                    }
//                    counter++;
//                }
//                return null;
//            }
//            public bool ContainsSequence(Sequence<T> meleeSequence) => ContainsSequence(meleeSequence.GetHashCode());
//            public bool ContainsSequence(int hashCode)
//            {
//                foreach (var wraper in wrapers)
//                    if (wraper.ContainsSequence(hashCode))
//                        return true;
//                return false;
//            }
//        }
//        public class Wraper : WraperBase
//        {
//            //public override void SetConfigPanel(UIList uIList)
//            //{
//            //    elementInfo?.SetConfigPanel(uIList);
//            //}
//            public override WraperBase Clone()
//            {
//                if (IsSequence)
//                {
//                    return this;
//                }
//                else
//                {
//                    Type type = elementInfo.GetType();
//                    return new Wraper((T)Activator.CreateInstance(type));
//                }
//            }
//            public static bool ReadWraper(XmlReader xmlReader, string inModDirectoryPath, out Wraper result)
//            {
//                xmlReader.Read();
//                if (xmlReader.Name != "Wraper")
//                {
//                    xmlReader.Read();
//                    result = null;
//                    return false;
//                }
//                else
//                {
//                    var conditionKey = xmlReader["condition"];
//                    var ModName = xmlReader["Mod"];
//                    if (xmlReader["IsSequence"] == "True")
//                    {
//                        xmlReader.Read();
//                        if (xmlReader.Value.Contains('\n'))
//                        {
//                            ReadSequence(xmlReader, ModName, out Sequence<T> resultSequence);
//                            result = new Wraper(resultSequence);
//                        }
//                        else
//                        {
//                            if (SequenceManager<T>.Sequences.TryGetValue($"{ModName}/{xmlReader.Value}", out var sequence))
//                            {
//                                result = new Wraper(sequence);
//                            }
//                            else
//                            {

//                                result = null;
//                                if (inModDirectoryPath != null)
//                                {
//                                    string filePath = $"{inModDirectoryPath}/{xmlReader.Value}.xml";
//                                    var resultSequence = new Sequence<T>();
//                                    Load(filePath, ModLoader.GetMod(ModName), inModDirectoryPath, resultSequence);
//                                    result = new Wraper(resultSequence);
//                                }
//                                else
//                                {
//                                    string filePath = $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{typeof(T).Name}/{ModName}/{xmlReader.Value}.xml";
//                                    if (File.Exists(filePath))
//                                    {
//                                        var resultSequence = Load(filePath);
//                                        SequenceManager<T>.Sequences[$"{ModName}/{xmlReader.Value}"] = resultSequence;
//                                        result = new Wraper(resultSequence);
//                                    }
//                                }
//                                if(result == null)
//                                     result ??= new Wraper($"{ModName}/{xmlReader.Value}", true);

//                            }
//                        }
//                        xmlReader.Read();//节点结束
//                        xmlReader.Read();//空白
//                    }
//                    else
//                    {
//                        xmlReader.Read();//空白
//                        xmlReader.Read();//Action
//                        if (ModContent.TryFind(xmlReader["name"], out T instance))
//                        {
//                            var elem = (T)Activator.CreateInstance(instance.GetType());//获取元素实例
//                            result = new Wraper(elem);
//                            elem.LoadAttribute(xmlReader);
//                        }
//                        else
//                        {
//                            result = new Wraper(xmlReader["name"], false);
//                            var str = xmlReader.ToString();
//                            if (xmlReader.HasAttributes)
//                                while (xmlReader.MoveToNextAttribute())
//                                    if (xmlReader.Name != "name")
//                                        result.LoadFailedMetaAttributes[xmlReader.Name] = xmlReader.Value;
//                        }
//                        //throw new Exception($"Could not find {xmlReader["name"]}, please check your enabled mods");

//                        xmlReader.Read();//空白
//                        xmlReader.Read();//节点结束
//                        xmlReader.Read();//空白
//                    }
//                    if (conditionKey != null)
//                        result.SetCondition(SequenceSystem.Conditions[conditionKey]);
//                    return true;
//                }
//            }
//            public override ISequenceElement Element => elementInfo;
//            public readonly T elementInfo;
//            public Sequence<T> sequenceInfo;
//            public override Sequence SequenceInfo => sequenceInfo;
//            public bool finished;
//            public override bool IsElement => elementInfo != null && !IsSequence;
//            public override string Name => Available ? sequenceInfo?.sequenceName ?? elementInfo.GetLocalization("DisplayName", () => elementInfo.GetType().Name).ToString() : Language.GetTextValue($"Mods.LogSpiralLibrary.SequenceUI.{(LoadFailedMetaSequence ? "Sequence" : "Action")}Failed") + LoadFailedMetaName;//elementInfo.GetLocalization("DisplayName", () => elementInfo.GetType().Name).ToString()//elementInfo.GetType().Name
//            public Wraper(T sequenceELement)
//            {
//                elementInfo = sequenceELement;
//            }
//            public Wraper(Sequence<T> sequence)
//            {
//                sequenceInfo = sequence;
//            }
//            public Wraper(string metaName, bool isSequence)
//            {
//                LoadFailedMetaName = metaName;
//                LoadFailedMetaSequence = isSequence;
//                LoadFailedMetaAttributes = [];
//            }
//            public static implicit operator Wraper(Sequence<T> sequence) => new(sequence);
//            public bool ContainsSequence(Sequence<T> meleeSequence) => ContainsSequence(meleeSequence.GetHashCode());
//            public bool ContainsSequence(int hashCode)
//            {
//                if (!IsSequence) return false;
//                if (sequenceInfo.GetHashCode() == hashCode) return true;
//                foreach (var groups in sequenceInfo.groups)
//                {
//                    if (groups.ContainsSequence(hashCode)) return true;
//                }
//                return false;
//            }

//            public Wraper SetCondition(Condition _condition)
//            {
//                string key = _condition.Description.Key.Split('.')[^1];
//                if (!SequenceSystem.Conditions.Keys.Contains(key))
//                    SequenceSystem.Conditions[key] = _condition;
//                conditionDefinition = new ConditionDefinition(key);
//                //Condition = _condition;
//                return this;
//            }
//            /// <summary>
//            /// 内层元素的计时器，从最大值逐渐更新至0
//            /// </summary>
//            public int Timer { get => elementInfo.timer; set => elementInfo.timer = value; }
//            /// <summary>
//            /// 内层元素的计时器上限
//            /// </summary>
//            public int TimerMax { get => elementInfo.timerMax; set => elementInfo.timerMax = value; }
//            public bool Attacktive;

//            /// <summary>
//            /// 打包器的更新函数
//            /// </summary>
//            /// <param name="entity">主体</param>
//            /// <param name="projectile">序列类型控制的弹幕</param>
//            /// <param name="standardInfo">标准值</param>
//            /// <param name="triggered">是否处于触发状态</param>
//            /// <param name="meleeAttackData">传回的具体元素</param>
//            /// <returns>当前序列是否执行完毕</returns>
//            /// <exception cref="Exception"></exception>
//            public bool Update(Entity entity, Projectile projectile, StandardInfo standardInfo, bool triggered, ref T meleeAttackData)
//            {
//                if (!Available) throw new Exception("序列不可用");
//                if (finished) throw new Exception("咱已经干完活了");
//                owner = entity;
//                if (IsSequence)
//                {
//                    if (sequenceInfo.currentWrapper != null && sequenceInfo.counter >= sequenceInfo.groups.Count - 1 && sequenceInfo.currentWrapper.finished)
//                    //if (sequenceInfo.counter >= sequenceInfo.groups.Count)
//                    {
//                        //Main.NewText("好欸");
//                        Attacktive = false;
//                        sequenceInfo.currentWrapper.finished = false;
//                        sequenceInfo.currentWrapper = null;
//                        sequenceInfo.counter = 0;
//                        finished = true;
//                        Active = false;
//                        return true;
//                    }
//                    Active = true;
//                    sequenceInfo.Update(entity, projectile, standardInfo, triggered);
//                    meleeAttackData = sequenceInfo.currentData;//一路传出给到最外层，应该有更合理得多的写法，但是目前能跑就行((((
//                }
//                else
//                {
//                    if (Timer <= 0)//计时器小于等于0时
//                    {
//                        if (elementInfo.counter < elementInfo.Cycle || elementInfo.Cycle == 0)//如果没执行完所有次数
//                        {
//                            elementInfo.Owner = entity;
//                            elementInfo.Projectile = projectile;
//                            elementInfo.standardInfo = standardInfo;
//                            if (elementInfo.counter == 0)//标志着刚切换上
//                                elementInfo.OnActive();
//                            else elementInfo.OnEndSingle();
//                            elementInfo.OnStartSingle();
//                            var result = (int)(standardInfo.standardTimer * elementInfo.ModifyData.actionOffsetTimeScaler / elementInfo.Cycle);
//                            TimerMax = Timer = result;
//                            elementInfo.counter++;
//                            if (elementInfo.Attacktive)
//                                elementInfo.OnStartAttack();
//                        }
//                        //迁移至下方
//                        else
//                        {
//                            Active = false;
//                            elementInfo.OnEndSingle();
//                            elementInfo.OnDeactive();//要被换掉了
//                            elementInfo.OnEndAttack();
//                            Timer = 0;
//                            TimerMax = 0;
//                            elementInfo.counter = 0;
//                            finished = true;
//                            Attacktive = false;
//                            return false;
//                        }
//                    }
//                    if (elementInfo != null)
//                    {
//                        bool oldValue = Attacktive;
//                        Attacktive = elementInfo.Attacktive;
//                        if (!oldValue && Attacktive)
//                        {
//                            elementInfo.OnStartAttack();//TODO Attack相关钩子合理化挂载位置
//                        }
//                        if (oldValue && !Attacktive)
//                        {
//                            elementInfo.OnEndAttack();
//                        }
//                        if (Attacktive) elementInfo.OnAttack();
//                        else elementInfo.OnCharge();
//                        elementInfo.standardInfo = standardInfo;//elementInfo.standardInfo with { frame = standardInfo.frame };
//                        elementInfo.Update(triggered);
//                        Active = true;
//                        //Main.NewText(GetHashCode());
//                    }
//                    meleeAttackData = elementInfo;
//                }
//                return false;
//            }

//            public override void TryLoadSequenceAgain()
//            {
//                if (SequenceManager<T>.Sequences.TryGetValue(LoadFailedMetaName, out var value))
//                {
//                    sequenceInfo = value;
//                    LoadFailedMetaName = "";
//                    LoadFailedMetaSequence = false;
//                }
//            }
//        }
//        public override void Remove(WraperBase target, GroupBase owner)
//        {
//            Group instanceOwner = owner as Group;
//            instanceOwner.wrapers.Remove(target as Wraper);
//            if (owner.Wrapers.Count == 0)
//                groups.Remove(instanceOwner);
//        }
//        public void Add(T meleeAttackData)
//        {
//            Wraper wraper = new(meleeAttackData);
//            Add(wraper);
//        }
//        public void Add(Wraper wraper)
//        {
//            if (wraper.ContainsSequence(this))
//            {
//                Main.NewText("不可调用自己");
//                return;
//            }
//            Group group = new();
//            group.wrapers.Add(wraper);
//            Add(group);
//        }
//        public override void Add(WraperBase wraperBase, out GroupBase newGroup)
//        {
//            Wraper wraper = (Wraper)wraperBase;
//            if (wraper.ContainsSequence(this))
//            {
//                Main.NewText("不可调用自己");
//                newGroup = null;
//                return;
//            }
//            Group group = new();
//            group.wrapers.Add(wraper);
//            newGroup = group;
//            Add(group);
//        }
//        public void Add(Group group)
//        {
//            if (group.ContainsSequence(this))
//            {
//                Main.NewText("不可调用自己");
//                return;
//            }
//            groups.Add(group);
//        }
//        public override void Add(GroupBase groupBase) => Add((Group)groupBase);
//        public void Insert(int index, Wraper wraper)
//        {
//            if (wraper.ContainsSequence(this))
//            {
//                Main.NewText("不可调用自己");
//                return;
//            }
//            Group group = new();
//            group.wrapers.Add(wraper);
//            groups.Insert(index, group);
//        }
//        public void Insert(int index, Group group)
//        {
//            if (group.ContainsSequence(this))
//            {
//                Main.NewText("不可调用自己");
//                return;
//            }
//            groups.Insert(index, group);
//        }
//        public override void Insert(int index, GroupBase groupBase) => Insert(index, (Group)groupBase);
//        public override void Insert(int index, WraperBase wraperBase, out GroupBase newGroup)
//        {
//            Wraper wraper = (Wraper)wraperBase;
//            if (wraper.ContainsSequence(this))
//            {
//                Main.NewText("不可调用自己");
//                newGroup = null;
//                return;
//            }
//            Group group = new();
//            group.wrapers.Add(wraper);
//            newGroup = group;
//            Insert(index, group);
//        }
//        public string sequenceName = SequenceDefaultName;
//        public int counter;
//        public override int Counter => counter;
//        public Wraper currentWrapper;
//        public T currentData;
//        public List<Group> groups = [];
//        public IReadOnlyList<Group> Groups => groups;
//        public override string FileName => sequenceName;
//        public override IReadOnlyList<GroupBase> GroupBases => (from g in groups select (GroupBase)g).ToList();
//        public Mod mod;
//        public override Mod Mod => mod;
//        public override string ElementTypeName => typeof(T).Name;
//        void ChooseNewWrapper()
//        {
//            if (currentWrapper != null)
//            {
//                currentWrapper.finished = false;//重置先前打包器的完成状态
//                currentWrapper.Attacktive = false;
//                counter++;//计数器自增

//            }
//            int offsetor = 0;//偏移量
//            int maxCount = Groups.Count;
//            do
//            {
//                currentWrapper = Groups[(counter + offsetor) % maxCount].GetCurrentWraper();
//                if (currentWrapper != null)
//                {
//                    counter += offsetor;
//                    break;
//                }
//                offsetor++;
//            }
//            while (currentWrapper == null && offsetor < maxCount);//抽到一个能用的或者超过上限为止，一般来讲是前者截断
//            if (currentWrapper != null)
//            {
//                currentWrapper.finished = false;//重置先前打包器的完成状态
//            }
//        }
//        public void Update(Entity entity, Projectile projectile, StandardInfo standardInfo, bool triggered)
//        {
//        Label:
//            if ((currentWrapper == null || currentWrapper.finished) && triggered)//需要抽取新的打包器，并且处于触发状态
//                ChooseNewWrapper();

//            if (currentWrapper == null) return;
//            if (!currentWrapper.finished && currentWrapper.Update(entity, projectile, standardInfo, triggered, ref currentData))//只要没结束就继续执行更新
//                goto Label;
//        }
//        public void ResetCounter()
//        {
//            counter = 0;
//            currentWrapper = null;
//            currentData = default;
//            foreach (var g in groups)
//            {
//                foreach (var w in g.wrapers)
//                {
//                    if (!w.Available) continue;
//                    if (w.IsSequence)
//                        w.sequenceInfo.ResetCounter();
//                    else
//                    {
//                        w.Timer = w.TimerMax = 0;
//                        w.elementInfo.counter = 0;
//                    }
//                    w.owner = null;
//                    w.finished = false;
//                }
//            }
//        }
//        public void SetOwner(Entity owner)
//        {
//            foreach (var g in groups)
//                foreach (var w in g.wrapers)
//                {
//                    w.owner = owner;
//                    if (w.IsSequence) w.sequenceInfo.SetOwner(owner);
//                }
//        }

//        public override void SyncInfo(SequenceBasicInfo info)
//        {
//            sequenceName = info.FileName;
//            mod = ModLoader.GetMod(info.ModName);
//        }
//    }
//}

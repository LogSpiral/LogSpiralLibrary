using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using LogSpiralLibrary.CodeLibrary.DataStructures;
using Terraria.GameContent.UI.Elements;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ElementCustomDataAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ElementCustomDataAbabdonedAttribute : Attribute
    {
    }
    /// <summary>
    /// 物品相应顶点绘制特效的标准值
    /// </summary>
    public struct VertexDrawInfoStandardInfo
    {
        public bool active;
        public Texture2D heatMap;
        public int timeLeft;
        public float scaler;
        public VertexDrawInfo.IRenderDrawInfo[] renderInfos;
        public Vector3 colorVec;
    }
    /// <summary>
    /// 不同物品有自己独有的标准值
    /// </summary>
    public struct StandardInfo
    {

        /// <summary>
        /// 物品贴图朝向
        /// </summary>
        public float standardRotation = MathHelper.PiOver4;
        /// <summary>
        /// 物品手持中心
        /// </summary>
        public Vector2 standardOrigin = new Vector2(.1f, .9f);
        /// <summary>
        /// 标准持续时长
        /// </summary>
        public int standardTimer;
        /// <summary>
        /// 标准颜色
        /// </summary>
        public Color standardColor;
        /// <summary>
        /// 高亮贴图
        /// </summary>
        public Texture2D standardGlowTexture;
        public VertexDrawInfoStandardInfo vertexStandard = default;
        public StandardInfo()
        {
        }
        public StandardInfo(float rotation, Vector2 origin, int timer, Color color, Texture2D glow)
        {
            standardRotation = rotation;
            standardOrigin = origin;
            standardTimer = timer;
            standardColor = color;
            standardGlowTexture = glow;
        }
        //TODO 改成弹幕序列独有
    }
    public struct ActionModifyData
    {
        public float actionOffsetSize = 1;
        public float actionOffsetTimeScaler = 1;
        public float actionOffsetKnockBack = 1;
        public float actionOffsetDamage = 1;
        public int actionOffsetCritAdder = 0;
        public float actionOffsetCritMultiplyer = 1;

        public ActionModifyData(float size = 1, float timeScaler = 1, float knockBack = 1, float damage = 1, int critAdder = 0, float critMultiplyer = 1)
        {
            actionOffsetSize = size;
            actionOffsetTimeScaler = timeScaler;
            actionOffsetKnockBack = knockBack;
            actionOffsetDamage = damage;
            actionOffsetCritAdder = critAdder;
            actionOffsetCritMultiplyer = critMultiplyer;
        }
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
            return $"({actionOffsetSize:0.00},{actionOffsetTimeScaler:0.00},{actionOffsetKnockBack:0.00},{actionOffsetDamage:0.00},{actionOffsetCritAdder},{actionOffsetCritMultiplyer:0.00})";
        }
        public static ActionModifyData LoadFromString(string str)
        {
            var content = str.Remove(0, 1).Remove(str.Length - 2).Split(',');
            return new ActionModifyData(float.Parse(content[0]), float.Parse(content[1]), float.Parse(content[2]), float.Parse(content[3]), int.Parse(content[4]), float.Parse(content[5]));
        }
    }
    public interface ISequenceElement : ILocalizedModType, ILoadable
    {
        #region 属性
        #region 编排序列时调整
        //持续时间 角度 位移 修改数据
        /// <summary>
        /// 使用数据修改
        /// </summary>
        [ElementCustomData]
        ActionModifyData ModifyData => new ActionModifyData();
        /// <summary>
        /// 执行次数
        /// </summary>
        [ElementCustomData]
        int Cycle => 1;
        #endregion
        #region 动态调整，每次执行时重设
        bool flip { get; set; }
        /// <summary>
        /// 旋转角，非插值
        /// </summary>
        float Rotation { get; set; }
        /// <summary>
        /// 扁平程度？
        /// </summary>
        float KValue { get; set; }
        /// <summary>
        /// 执行第几次？
        /// </summary>
        int counter { get; set; }
        int timer { get; set; }
        int timerMax { get; set; }
        #endregion
        #region 插值生成，最主要的实现内容的地方
        /// <summary>
        /// 当前周期的进度
        /// </summary>
        float Factor { get; }
        /// <summary>
        /// 中心偏移量，默认零向量
        /// </summary>
        Vector2 offsetCenter => default;
        /// <summary>
        /// 原点偏移量，默认为贴图左下角(0.1f,0.9f),取值范围[0,1]
        /// </summary>
        Vector2 offsetOrigin => new Vector2(.1f, .9f);
        /// <summary>
        /// 旋转量
        /// </summary>
        float offsetRotation { get; }
        /// <summary>
        /// 大小
        /// </summary>
        float offsetSize { get; }
        /// <summary>
        /// 是否具有攻击性
        /// </summary>
        bool Attacktive { get; }
        #endregion
        #endregion
        #region 函数
        #region 切换
        /// <summary>
        /// 被切换时调用,脉冲性
        /// </summary>
        void OnActive();

        /// <summary>
        /// 被换走时调用,脉冲性
        /// </summary>
        void OnDeactive();
        #endregion

        #region 吟唱
        /// <summary>
        /// 攻击期间调用,持续性
        /// </summary>
        void OnAttack();

        /// <summary>
        /// 攻击以外时间调用,持续性
        /// </summary>
        void OnCharge();
        #endregion

        #region 每轮
        void OnStartSingle();
        void OnEndSingle();
        #endregion

        #region 每次攻击
        /// <summary>
        /// 结束时调用,脉冲性
        /// </summary>
        void OnEndAttack();

        /// <summary>
        /// 开始攻击时调用,脉冲性
        /// </summary>
        void OnStartAttack();
        #endregion

        #region 具体传入
        void Update(bool triggered);

        void Draw(SpriteBatch spriteBatch, Texture2D texture);

        #endregion

        #region SL
        void SaveAttribute(XmlWriter xmlWriter);
        void LoadAttribute(XmlReader xmlReader);
        #endregion

        #region UIConfig
        //void SetConfigPanel(UIList parent);
        #endregion
        #endregion
        #region 吃闲饭的
        Entity Owner { get; set; }
        Projectile Projectile { get; set; }
        StandardInfo standardInfo { get; set; }


        #endregion
    }
    /// <summary>
    /// 去泛型化的序列基类
    /// </summary>
    [XmlRoot("Sequence")]
    public abstract class SequenceBase
    {
        //public abstract GroupBase CreateSimpleGroup(WraperBase wraperBase);
        public const string SequenceDefaultName = "My Sequence";
        public abstract void Remove(WraperBase target, GroupBase owner);
        public abstract void Add(WraperBase wraperBase, out GroupBase newGroup);
        public abstract void Add(GroupBase groupBase);
        public abstract void Insert(int index, WraperBase wraperBase, out GroupBase newGroup);
        public abstract void Insert(int index, GroupBase groupBase);

        public abstract void Save();
        [XmlRoot("Group")]
        public abstract class GroupBase
        {
            public abstract IReadOnlyList<WraperBase> Wrapers { get; }
            public abstract int Index { get; }
            public abstract void Insert(int index, WraperBase wraperBase);
            //public abstract GroupBase CreateFromWraper(WraperBase wraper);
        }
        [XmlRoot("Sequence")]
        public abstract class WraperBase
        {
            public abstract ISequenceElement Element { get; }
            public abstract bool IsElement { get; }
            public abstract string Name { get; }
            public abstract SequenceBase SequenceInfo { get; }
            //public abstract void SetConfigPanel(UIList uIList);
            public bool IsSequence => SequenceInfo != null;
            public bool Available => IsSequence || IsElement;
            public Condition condition = new Condition("Always", () => true);
            public bool Active { get; set; }
            public abstract WraperBase Clone();
        }
        public abstract IReadOnlyList<GroupBase> GroupBases { get; }
        /// <summary>
        /// 当前序列的名字
        /// </summary>
        [XmlAttribute("name")]
        public abstract string SequenceNameBase { get; }
        /// <summary>
        /// 目前执行到第几个组
        /// </summary>
        [XmlIgnore]
        public abstract int Counter { get; }
        public abstract Mod Mod { get; }
        public abstract string ElementTypeName { get; }
        public abstract bool Active { get; set; }

    }
    public class SequenceBase<T> : SequenceBase where T : ISequenceElement
    {
        //public override GroupBase CreateSimpleGroup(WraperBase wraperBase)
        //{
        //    var result = new Group();
        //    result.wrapers.Add((Wraper)wraperBase);
        //    return result;
        //}
        public void WriteContent(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Sequence");
            if (SequenceNameBase != SequenceDefaultName)
                xmlWriter.WriteAttributeString("name", SequenceNameBase);
            for (int i = 0; i < Groups.Count; i++)
            {
                xmlWriter.WriteStartElement("Group");
                Group group = Groups[i];
                for (int j = 0; j < group.wrapers.Count; j++)
                {
                    Wraper wraper = group.wrapers[j];
                    xmlWriter.WriteStartElement("Wraper");
                    if (wraper.condition.Description.Value != "Always")
                        xmlWriter.WriteAttributeString("condition", wraper.condition.Description.Key.Split('.')[^1]);
                    //xmlWriter.WriteValue(wraper.Name);
                    if (wraper.IsSequence)
                    {
                        xmlWriter.WriteAttributeString("IsSequence", wraper.IsSequence.ToString());
                        if (wraper.Name == SequenceDefaultName)
                            ((SequenceBase<T>)wraper.SequenceInfo).WriteContent(xmlWriter);
                        else
                        {
                            if (wraper.SequenceInfo.Mod == null) Main.NewText(wraper.Name);
                            xmlWriter.WriteAttributeString("Mod", wraper.SequenceInfo.Mod.Name);
                            xmlWriter.WriteValue(wraper.sequenceInfo.sequenceName);
                        }
                    }
                    else
                    {
                        xmlWriter.WriteStartElement("Action");
                        wraper.elementInfo.SaveAttribute(xmlWriter);
                        xmlWriter.WriteAttributeString("name", wraper.elementInfo.FullName);
                        xmlWriter.WriteEndElement();

                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
        }
        public override void Save() => Save($"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{ElementTypeName}/{Mod.Name}/{SequenceNameBase}.xml");
        public void Save(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            //要求缩进
            settings.Indent = true;
            //注意如果不设置encoding默认将输出utf-16
            //注意这儿不能直接用Encoding.UTF8如果用Encoding.UTF8将在输出文本的最前面添加4个字节的非xml内容
            settings.Encoding = new UTF8Encoding(false);

            //设置换行符
            settings.NewLineChars = Environment.NewLine;
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            using XmlWriter xmlWriter = XmlWriter.Create(path, settings);
            WriteContent(xmlWriter);

        }
        public bool active;
        public override bool Active { get => active; set => active = value; }
        public static SequenceBase<T> Load(string path)
        {
            using XmlReader xmlReader = XmlReader.Create(path);
            xmlReader.Read();//读取声明
            xmlReader.Read();//读取空格
            ReadSequence(xmlReader, out var result);
            result.mod = ModLoader.GetMod(path.Split('\\')[^2]);
            return result;
        }
        public static bool ReadSequence(XmlReader xmlReader, out SequenceBase<T> result)
        {
            xmlReader.Read();//读取序列节点开始部分
            if (xmlReader.Name != "Sequence")
            {
                xmlReader.Read();
                result = null;
                return false;
            }
            else
            {
                result = new SequenceBase<T>();
                result.sequenceName = xmlReader["name"] ?? SequenceDefaultName;
                xmlReader.Read();//读取空格
                while (Group.ReadGroup(xmlReader, out var groupResult))
                {
                    result.Add(groupResult);
                }
                return true;
            }
        }
        public class Group : GroupBase
        {
            //public override GroupBase CreateFromWraper(WraperBase wraper)
            //{
            //    return new Group();
            //}
            public override void Insert(int index, WraperBase wraperBase)
            {
                wrapers.Insert(index, (Wraper)wraperBase);
            }
            public static bool ReadGroup(XmlReader xmlReader, out Group result)
            {
                xmlReader.Read();//读取下一个位置
                if (xmlReader.Name != "Group")//此时实际上是</Sequence>
                {
                    xmlReader.Read();//顺手把它的下一个空格读取了
                    result = null;
                    return false;
                }
                else
                {
                    xmlReader.Read();//读取空格
                    result = new Group();
                    while (Wraper.ReadWraper(xmlReader, out Wraper wraperResult))
                    {
                        result.wrapers.Add(wraperResult);
                    }
                    return true;
                }
            }
            /// <summary>
            /// 请不要对这个Add
            /// TODO 改为ReadOnly
            /// </summary>
            public override IReadOnlyList<WraperBase> Wrapers => (from w in wrapers select (WraperBase)w).ToList();
            public List<Wraper> wrapers = new List<Wraper>();
            public override int Index => index;
            public int index;
            public Wraper GetCurrentWraper()
            {
                int counter = 0;
                foreach (var wraper in wrapers)
                {
                    if (wraper.condition.IsMet())
                    {
                        index = counter;
                        return wraper;
                    }
                    counter++;
                }
                return null;
            }
            public bool ContainsSequence(SequenceBase<T> meleeSequence) => ContainsSequence(meleeSequence.GetHashCode());
            public bool ContainsSequence(int hashCode)
            {
                foreach (var wraper in wrapers)
                    if (wraper.ContainsSequence(hashCode))
                        return true;
                return false;
            }
        }
        public class Wraper : WraperBase
        {
            //public override void SetConfigPanel(UIList uIList)
            //{
            //    elementInfo?.SetConfigPanel(uIList);
            //}
            public override WraperBase Clone()
            {
                if (IsSequence)
                {
                    return this;
                }
                else
                {
                    Type type = elementInfo.GetType();
                    return new Wraper((T)Activator.CreateInstance(type));
                }
            }
            public static bool ReadWraper(XmlReader xmlReader, out Wraper result)
            {
                xmlReader.Read();
                if (xmlReader.Name != "Wraper")
                {
                    xmlReader.Read();
                    result = null;
                    return false;
                }
                else
                {
                    result = null;
                    var conditionKey = xmlReader["condition"];
                    if (xmlReader["IsSequence"] == "True")
                    {
                        xmlReader.Read();
                        if (xmlReader.Value.Contains("\n"))
                        {
                            ReadSequence(xmlReader, out SequenceBase<T> resultSequence);
                            result = new Wraper(resultSequence);
                        }
                        else
                        {
                            if (SequenceSystem.sequenceBases.TryGetValue(xmlReader.Value, out var sequence))
                            {
                                result = new Wraper((SequenceBase<T>)sequence);
                            }
                            else
                            {

                                var resultSequence = Load($"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{typeof(T).Name}/{xmlReader["Mod"]}/{xmlReader.Value}.xml");
                                SequenceSystem.sequenceBases[xmlReader.Value] = sequence;
                                result = new Wraper(resultSequence);
                            }
                        }
                        xmlReader.Read();//节点结束
                        xmlReader.Read();//空白
                    }
                    else
                    {
                        xmlReader.Read();//空白
                        xmlReader.Read();//Action
                        var elem = (T)Activator.CreateInstance(ModContent.Find<T>(xmlReader["name"]).GetType());//获取元素实例
                        result = new Wraper(elem);
                        elem.LoadAttribute(xmlReader);
                        xmlReader.Read();//空白
                        xmlReader.Read();//节点结束
                        xmlReader.Read();//空白
                        int k = 0;
                    }
                    if (conditionKey != null)
                        result.SetCondition(SequenceSystem.conditions[conditionKey]);
                    return true;
                }
            }
            public override ISequenceElement Element => elementInfo;
            public readonly T elementInfo;
            public readonly SequenceBase<T> sequenceInfo;
            public override SequenceBase SequenceInfo => sequenceInfo;
            public bool finished;
            public override bool IsElement => elementInfo != null && !IsSequence;
            public override string Name => sequenceInfo?.sequenceName ?? elementInfo.GetLocalization("DisplayName", () => elementInfo.GetType().Name).ToString();//elementInfo.GetLocalization("DisplayName", () => elementInfo.GetType().Name).ToString()//elementInfo.GetType().Name
            public Wraper(T sequenceELement)
            {
                elementInfo = sequenceELement;
            }
            public Wraper(SequenceBase<T> sequence)
            {
                sequenceInfo = sequence;
            }
            public static implicit operator Wraper(SequenceBase<T> sequence) => new Wraper(sequence);
            public bool ContainsSequence(SequenceBase<T> meleeSequence) => ContainsSequence(meleeSequence.GetHashCode());
            public bool ContainsSequence(int hashCode)
            {
                if (!IsSequence) return false;
                if (sequenceInfo.GetHashCode() == hashCode) return true;
                foreach (var groups in sequenceInfo.groups)
                {
                    if (groups.ContainsSequence(hashCode)) return true;
                }
                return false;
            }

            public Wraper SetCondition(Condition _condition)
            {
                condition = _condition;
                return this;
            }
            /// <summary>
            /// 内层元素的计时器，从最大值逐渐更新至0
            /// </summary>
            public int Timer { get => elementInfo.timer; set => elementInfo.timer = value; }
            /// <summary>
            /// 内层元素的计时器上限
            /// </summary>
            public int TimerMax { get => elementInfo.timerMax; set => elementInfo.timerMax = value; }
            public bool Attacktive;

            /// <summary>
            /// 打包器的更新函数
            /// </summary>
            /// <param name="entity">主体</param>
            /// <param name="projectile">序列类型控制的弹幕</param>
            /// <param name="standardInfo">标准值</param>
            /// <param name="triggered">是否处于触发状态</param>
            /// <param name="meleeAttackData">传回的具体元素</param>
            /// <returns>当前序列是否执行完毕</returns>
            /// <exception cref="Exception"></exception>
            public bool Update(Entity entity, Projectile projectile, StandardInfo standardInfo, bool triggered, ref T meleeAttackData)
            {
                if (!Available) throw new Exception("序列不可用");
                if (finished) throw new Exception("咱已经干完活了");
                if (IsSequence)
                {
                    if (sequenceInfo.counter >= sequenceInfo.groups.Count - 1 && sequenceInfo.currentWrapper.finished)
                    //if (sequenceInfo.counter >= sequenceInfo.groups.Count)
                    {
                        Attacktive = false;
                        sequenceInfo.currentWrapper.finished = false;
                        sequenceInfo.currentWrapper = null;
                        sequenceInfo.counter = 0;
                        finished = true;
                        Active = false;
                        return true;
                    }
                    Active = true;
                    sequenceInfo.Update(entity, projectile, standardInfo, triggered);
                    meleeAttackData = sequenceInfo.currentData;//一路传出给到最外层，应该有更合理得多的写法，但是目前能跑就行((((
                }
                else
                {
                    if (Timer <= 0)//计时器小于等于0时
                    {
                        if (elementInfo.counter < elementInfo.Cycle || elementInfo.Cycle == 0)//如果没执行完所有次数
                        {
                            elementInfo.Owner = entity;
                            if (elementInfo.counter == 0)//标志着刚切换上
                                elementInfo.OnActive();
                            else elementInfo.OnEndSingle();
                            elementInfo.OnStartSingle();
                            elementInfo.Projectile = projectile;
                            elementInfo.standardInfo = standardInfo;
                            var result = (int)(standardInfo.standardTimer * elementInfo.ModifyData.actionOffsetTimeScaler / elementInfo.Cycle);
                            TimerMax = Timer = result;
                            elementInfo.counter++;
                        }
                        //迁移至下方
                        else
                        {
                            Active = false;
                            elementInfo.OnEndSingle();
                            elementInfo.OnDeactive();//要被换掉了
                            elementInfo.OnEndAttack();
                            Timer = 0;
                            TimerMax = 0;
                            elementInfo.counter = 0;
                            finished = true;
                            Attacktive = false;
                            return false;
                        }
                    }
                    if (elementInfo != null)
                    {
                        bool oldValue = Attacktive;
                        Attacktive = elementInfo.Attacktive;
                        if (!oldValue && Attacktive)
                        {
                            elementInfo.OnStartAttack();//TODO Attack相关钩子合理化挂载位置
                        }
                        if (oldValue && !Attacktive)
                        {
                            elementInfo.OnEndAttack();
                        }
                        if (Attacktive) elementInfo.OnAttack();
                        else elementInfo.OnCharge();
                        elementInfo.Update(triggered);
                        Active = true;
                        //Main.NewText(GetHashCode());
                    }
                    meleeAttackData = elementInfo;
                }
                return false;
            }
        }
        public override void Remove(WraperBase target, GroupBase owner)
        {
            Group instanceOwner = owner as Group;
            instanceOwner.wrapers.Remove(target as Wraper);
            if (owner.Wrapers.Count == 0)
                groups.Remove(instanceOwner);
        }
        public void Add(T meleeAttackData)
        {
            Wraper wraper = new(meleeAttackData);
            Add(wraper);
        }
        public void Add(Wraper wraper)
        {
            if (wraper.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            Group group = new Group();
            group.wrapers.Add(wraper);
            Add(group);
        }
        public override void Add(WraperBase wraperBase,out GroupBase newGroup)
        {
            Wraper wraper = (Wraper)wraperBase;
            if (wraper.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                newGroup = null;
                return;
            }
            Group group = new Group();
            group.wrapers.Add(wraper);
            newGroup = group;
            Add(group);
        }
        public void Add(Group group)
        {
            if (group.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            groups.Add(group);
        }
        public override void Add(GroupBase groupBase) => Add((Group)groupBase);
        public void Insert(int index, Wraper wraper)
        {
            if (wraper.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            Group group = new Group();
            group.wrapers.Add(wraper);
            groups.Insert(index, group);
        }
        public void Insert(int index, Group group)
        {
            if (group.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            groups.Insert(index, group);
        }
        public override void Insert(int index, GroupBase groupBase) => Insert(index, (Group)groupBase);
        public override void Insert(int index, WraperBase wraperBase,out GroupBase newGroup)
        {
            Wraper wraper = (Wraper)wraperBase;
            if (wraper.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                newGroup = null;
                return;
            }
            Group group = new Group();
            group.wrapers.Add(wraper);
            newGroup = group;
            Insert(index, group);
        }
        public string sequenceName = SequenceDefaultName;
        public int counter;
        public override int Counter => counter;
        public Wraper currentWrapper;
        public T currentData;
        List<Group> groups = new List<Group>();
        public IReadOnlyList<Group> Groups => groups;
        public override string SequenceNameBase => sequenceName;
        public override IReadOnlyList<GroupBase> GroupBases => (from g in groups select (GroupBase)g).ToList();
        public Mod mod;
        public override Mod Mod => mod;
        public override string ElementTypeName => typeof(T).Name;
        void ChooseNewWrapper()
        {
            if (currentWrapper != null)
            {
                currentWrapper.finished = false;//重置先前打包器的完成状态
                currentWrapper.Attacktive = false;
                counter++;//计数器自增

            }
            int offsetor = 0;//偏移量
            int maxCount = Groups.Count;
            do
            {
                currentWrapper = Groups[(counter + offsetor) % maxCount].GetCurrentWraper();
                if (currentWrapper != null)
                {
                    counter += offsetor;
                    break;
                }
                offsetor++;
            }
            while (currentWrapper == null && offsetor < maxCount);//抽到一个能用的或者超过上限为止，一般来讲是前者截断
            if (currentWrapper != null)
            {
                currentWrapper.finished = false;//重置先前打包器的完成状态
            }
        }
        public void Update(Entity entity, Projectile projectile, StandardInfo standardInfo, bool triggered)
        {
        Label:
            if ((currentWrapper == null || currentWrapper.finished) && triggered)//需要抽取新的打包器，并且处于触发状态
                ChooseNewWrapper();

            if (currentWrapper == null) return;
            if (!currentWrapper.finished && currentWrapper.Update(entity, projectile, standardInfo, triggered, ref currentData))//只要没结束就继续执行更新
                goto Label;
        }
    }
}

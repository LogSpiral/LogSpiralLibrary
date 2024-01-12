using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.IMeleeAttackData;
using static LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.MeleeSequence;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{
    public struct StandardInfo
    {
        public float standardRotation = MathHelper.PiOver4;
        public Vector2 standardOrigin = new Vector2(.1f, .9f);
        public int standardTimer;
        public Color standardColor;
        public Texture2D standardGlowTexture;
        public StandardInfo()
        {
        }
        public StandardInfo(float rotation, Vector2 origin, int timer, Color color,Texture2D glow)
        {
            standardRotation = rotation;
            standardOrigin = origin;
            standardTimer = timer;
            standardColor = color;
            standardGlowTexture = glow;
        }
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
    }
    public interface ISequenceElement
    {
        #region 属性
        #region 编排序列时调整
        //持续时间 角度 位移 修改数据
        /// <summary>
        /// 使用数据修改
        /// </summary>
        ActionModifyData ModifyData => new ActionModifyData();
        /// <summary>
        /// 执行次数
        /// </summary>
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
        void Update();

        void Draw(SpriteBatch spriteBatch, Texture2D texture);

        #endregion
        #endregion
        #region 吃闲饭的
        Entity Owner { get; set; }
        Projectile Projectile { get; set; }
        StandardInfo standardInfo { get; set; }
        #endregion
    }
    public abstract class SequenceBase
    {
        public abstract class GroupBase
        {
            public abstract List<WraperBase> Wrapers { get; }
            public abstract int Index { get; }
        }
        public abstract class WraperBase
        {
            public abstract bool IsElement { get; }
            public abstract string Name { get; }
            public abstract SequenceBase SequenceInfo { get; }
            public bool IsSequence => SequenceInfo != null;
            public bool Available => IsSequence || IsElement;
            public Condition condition = new Condition("Always", () => true);
            public bool Active { get; set; }
        }
        public abstract IReadOnlyList<GroupBase> GroupBases { get; }
        public abstract string SequenceNameBase { get; }
        public abstract int Counter { get; }

    }
    public class SequenceBase<T> : SequenceBase where T : ISequenceElement
    {
        public class Group : GroupBase
        {
            public override List<WraperBase> Wrapers => (from w in wrapers select (WraperBase)w).ToList();
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
            public readonly T elementInfo;
            public readonly SequenceBase<T> sequenceInfo;
            public override SequenceBase SequenceInfo => sequenceInfo;
            public bool finished;
            public override bool IsElement => elementInfo != null && !IsSequence;
            public override string Name => sequenceInfo?.SequenceName ?? elementInfo.GetType().Name;
            public Wraper(T meleeAttackData)
            {
                elementInfo = meleeAttackData;
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
            public int timer { get => elementInfo.timer; set => elementInfo.timer = value; }
            public int timerMax { get => elementInfo.timerMax; set => elementInfo.timerMax = value; }
            public bool Attacktive;

            public void Update(Entity entity, Projectile projectile, StandardInfo standardInfo, ref T meleeAttackData)
            {
                if (!Available) throw new Exception("序列不可用");
                if (finished) throw new Exception("咱已经干完活了");
                object obj = new object();
                if (IsSequence)
                {
                    if (sequenceInfo.counter >= sequenceInfo.groups.Count)
                    {
                        sequenceInfo.currentWrapper = null;
                        sequenceInfo.counter = 0;
                        finished = true;
                        Active = false;
                        return;
                    }
                    Active = true;
                    sequenceInfo.Update(entity, projectile, standardInfo);
                    meleeAttackData = sequenceInfo.currentData;
                }
                else
                {
                    if (timer <= 0)//计时器小于等于0时
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
                            timerMax = timer = result;
                            elementInfo.counter++;
                        }
                        //迁移至下方
                        else
                        {
                            Active = false;
                            elementInfo.OnEndSingle();
                            elementInfo.OnDeactive();//要被换掉了
                            elementInfo.OnEndAttack();
                            timer = 0;
                            timerMax = 0;
                            elementInfo.counter = 0;
                            finished = true;
                            return;
                        }
                    }
                    if (elementInfo != null)
                    {
                        bool oldValue = Attacktive;
                        Attacktive = elementInfo.Attacktive;
                        if (!oldValue && Attacktive)
                        {
                            elementInfo.OnStartAttack();
                        }
                        if (oldValue && !Attacktive) 
                        {
                            elementInfo.OnEndAttack();
                        }
                        if (Attacktive) elementInfo.OnAttack();
                        else elementInfo.OnCharge();
                        elementInfo.Update();
                        Active = true;
                        //Main.NewText(GetHashCode());
                    }
                    meleeAttackData = elementInfo;
                }
            }
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
        public void Add(Group group)
        {
            if (group.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            groups.Add(group);
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
        public string SequenceName = "My MeleeSequence";
        public int counter;
        public override int Counter => counter;
        public Wraper currentWrapper;
        public T currentData;
        List<Group> groups = new List<Group>();
        public IReadOnlyList<Group> Groups => groups;
        public override string SequenceNameBase => SequenceName;
        public override IReadOnlyList<GroupBase> GroupBases => (from g in groups select (GroupBase)g).ToList();
        public void Update(Entity entity, Projectile projectile, StandardInfo standardInfo)
        {
            if ((currentWrapper == null || currentWrapper.finished))
            {

                if (currentWrapper != null)
                {
                    currentWrapper.finished = false;
                    counter++;
                }
                int offsetor = 0;
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
                while (currentWrapper == null && offsetor < maxCount);

            }
            currentWrapper.Update(entity, projectile, standardInfo, ref currentData);
        }
    }
}

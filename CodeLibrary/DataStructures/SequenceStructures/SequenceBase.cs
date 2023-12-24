using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.IMeleeAttackData;
using static LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.MeleeSequence;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{
    public interface ISequenceElement
    {

    }
    public class SequenceBase<T> where T : ISequenceElement
    {
        public class SequenceGroup
        {
            //TODO skipper的幽雅实现，改成null?
            public static MeleeSAWraper skipper = new MeleeSAWraper(new SkipAction(0));
            class SkipAction : NormalAttackAction
            {
                public SkipAction(int cycle, MeleeModifyData? data = null) : base(cycle, data)
                {
                }
                public override int Cycle => 1;
            }

            public List<SequenceWraper> wrapers = new List<SequenceWraper>();
            public SequenceWraper GetCurrentWraper()
            {
                foreach (var wraper in wrapers)
                {
                    if (wraper.condition.IsMet())
                        return wraper;
                }
                return skipper;
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
        public class SequenceWraper
        {
            public readonly T elementInfo;
            public readonly SequenceBase<T> sequenceInfo;
            public bool finished;
            public Condition condition = new Condition("Always", () => true);
            public bool IsElement => elementInfo != null && !IsSequence;
            public bool IsSequence => sequenceInfo != null;
            public bool Available => IsSequence || IsElement;
            public int timer { get => elementInfo.timer; set => elementInfo.timer = value; }
            public int timerMax { get => elementInfo.timerMax; set => elementInfo.timerMax = value; }
            public bool Attacktive;
            //public string Name => groupInfo?.GroupName ?? sequenceInfo.SequenceName ?? "null";//暂时用不到这b玩意好像
            public SequenceWraper(T meleeAttackData)
            {
                elementInfo = meleeAttackData;
            }

            public SequenceWraper(SequenceBase<T> sequence)
            {
                sequenceInfo = sequence;
            }
            //public static implicit operator MeleeSAWraper(IMeleeAttackData meleeAttackData) => new MeleeSAWraper(meleeAttackData);
            public static implicit operator SequenceWraper(SequenceBase<T> sequence) => new SequenceWraper(sequence);
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
            public void Update(Entity entity, Projectile projectile, StandardInfo standardInfo, ref T meleeAttackData)
            {
                if (!Available) throw new Exception("序列不可用");
                if (finished) throw new Exception("咱已经干完活了");
                if (IsSequence)
                {
                    if (sequenceInfo.counter >= sequenceInfo.groups.Count)
                    {
                        sequenceInfo.currentWrapper = null;
                        sequenceInfo.counter = 0;
                        finished = true;
                        return;
                    }
                    sequenceInfo.Update(entity, projectile, standardInfo);
                    meleeAttackData = sequenceInfo.currentData;
                }
                else
                {
                    if (elementInfo.SkipCheck())
                    {
                        timer = 0;
                        timerMax = 0;
                        elementInfo.counter = 0;
                        finished = true;
                    }
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
                            var result = (int)(standardInfo.standardTimer * elementInfo.ModifyData.actionOffsetSpeed / elementInfo.Cycle);
                            timerMax = timer = result;
                            elementInfo.counter++;
                        }
                        //迁移至下方
                        else
                        {
                            elementInfo.OnEndSingle();
                            elementInfo.OnDeactive();//要被换掉了
                            timer = 0;
                            timerMax = 0;
                            elementInfo.counter = 0;
                            finished = true;
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
                            elementInfo.OnEndAttack();
                        if (Attacktive) elementInfo.OnAttack();
                        else elementInfo.OnCharge();
                        elementInfo.Update();

                    }
                    meleeAttackData = elementInfo;
                }
            }
            public SequenceWraper SetCondition(Condition _condition)
            {
                condition = _condition;
                return this;
            }
        }
        public void Add(T meleeAttackData)
        {
            SequenceWraper wraper = new(meleeAttackData);
            Add(wraper);
        }
        public void Add(SequenceWraper wraper)
        {
            if (wraper.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            SequenceGroup group = new SequenceGroup();
            group.wrapers.Add(wraper);
            Add(group);
        }
        public void Add(SequenceGroup group)
        {
            if (group.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            groups.Add(group);
        }
        public void Insert(int index, SequenceGroup group)
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
        public SequenceWraper currentWrapper;
        public T currentData;
        List<SequenceGroup> groups = new List<SequenceGroup>();
        public IReadOnlyList<SequenceGroup> Groups => groups;
        public void Update(Entity entity, Projectile projectile, StandardInfo standardInfo)
        {
            if (currentWrapper == null || currentWrapper.finished)
            {
                if (currentWrapper != null)
                {
                    currentWrapper.finished = false;
                    counter++;
                }
                int offsetor = 0;
                int maxCount = meleeGroups.Count;
                while (currentWrapper == null && offsetor < maxCount)
                {
                    currentWrapper = meleeGroups[(counter + offsetor) % maxCount].GetCurrentWraper();
                    if (currentWrapper != null)
                    {
                        counter += offsetor;
                        break;
                    }
                    offsetor++;
                }
            }
            currentWrapper.Update(entity, projectile, standardInfo, ref currentData);
        }
    }
}

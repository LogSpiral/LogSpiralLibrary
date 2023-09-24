using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.UI;

namespace LogSpiralLibrary.CodeLibrary.DataStructures
{
    public struct MeleeModifyData
    {
        public float actionOffsetSize = 1;
        public float actionOffsetSpeed = 1;
        public float actionOffsetKnockBack = 1;
        public float actionOffsetDamage = 1;
        public int actionOffsetCritAdder = 0;
        public float actionOffsetCritMultiplyer = 1;

        public MeleeModifyData()
        {
        }
        /// <summary>
        /// 将除了速度以外的值赋给目标
        /// </summary>
        /// <param name="target"></param>
        public void SetActionValue(ref MeleeModifyData target)
        {
            float speed = target.actionOffsetSpeed;
            target = this with { actionOffsetSpeed = speed };
        }
        public void SetActionSpeed(ref MeleeModifyData target) => target.actionOffsetSpeed = this.actionOffsetSpeed;
    }

    public interface IMeleeAttackData
    {
        //持续时间 角度 位移 修改数据
        /// <summary>
        /// 近战数据修改
        /// </summary>
        MeleeModifyData ModifyData => new MeleeModifyData();
        /// <summary>
        /// 执行次数
        /// </summary>
        int Cycle => 1;
        /// <summary>
        /// 中心偏移量，默认零向量
        /// </summary>
        Vector2 offsetCenter => default;
        /// <summary>
        /// 原点偏移量，默认为贴图左下角(0.1f,0.9f),取值范围[0,1]
        /// </summary>
        Vector2 offsetOrigin => new Vector2(.1f, .9f);
        /// <summary>
        /// 当前周期的进度
        /// </summary>
        float Factor { get; }
        /// <summary>
        /// 旋转量
        /// </summary>
        float Rotation { get; }
        /// <summary>
        /// 大小
        /// </summary>
        float Size { get; }
        /// <summary>
        /// 是否具有攻击性
        /// </summary>
        bool Attacktive { get; }

        /// <summary>
        /// 被切换时调用,脉冲性
        /// </summary>
        void OnActive();

        /// <summary>
        /// 被换走时调用,脉冲性
        /// </summary>
        void OnDeactive();

        /// <summary>
        /// 结束时调用,脉冲性
        /// </summary>
        void OnEndAttack();

        /// <summary>
        /// 开始攻击时调用,脉冲性
        /// </summary>
        void OnStartAttack();

        /// <summary>
        /// 攻击期间调用,持续性
        /// </summary>
        void OnAttack();

        /// <summary>
        /// 攻击以外时间调用,持续性
        /// </summary>
        void OnCharge();
        Condition Condition { get; }
        void Update(ref int timer, out bool canReduceTimer);
    }

    public class MeleeSequence
    {
        public class MeleeGroup
        {
            public List<IMeleeAttackData> meleeAttackDatas = new List<IMeleeAttackData>();
            public IMeleeAttackData GetCurrentMeleeData()
            {
                IMeleeAttackData result = null;
                if (meleeAttackDatas == null || meleeAttackDatas.Count == 0) goto Label;
                foreach (var meleeAtack in meleeAttackDatas)
                {
                    if (meleeAtack.Condition.IsMet())
                        result = meleeAtack;
                }
                result ??= meleeAttackDatas.FirstOrDefault();
            Label:
                return result;
            }
            public string GroupName = "My MeleeGroup";
        }
        /// <summary>
        /// 把两个类打包在一个类的实用寄巧
        /// </summary>
        public class MeleeGSWraper
        {
            public readonly MeleeGroup groupInfo;
            public readonly MeleeSequence sequenceInfo;

            public bool IsGroup => groupInfo != null;
            public bool IsSequence => sequenceInfo != null;
            public string Name => groupInfo?.GroupName ?? sequenceInfo.SequenceName ?? "null";
            public List<MeleeGroup> GetMeleeGroups()
            {
                var result = new List<MeleeGroup>();
                if (IsGroup)
                {
                    result.Add(groupInfo);
                }
                else if (IsSequence)
                {
                    foreach (var wrap in sequenceInfo.meleeWraps)
                        result.AddRange(wrap.GetMeleeGroups());
                }
                return result;
            }
            public MeleeGSWraper(MeleeGroup group)
            {
                groupInfo = group;
            }

            public MeleeGSWraper(MeleeSequence sequence)
            {
                sequenceInfo = sequence;
            }
            public static implicit operator MeleeGSWraper(MeleeGroup group) => new MeleeGSWraper(group);
            public static implicit operator MeleeGSWraper(MeleeSequence sequence) => new MeleeGSWraper(sequence);

            public bool ContainsSequence(MeleeSequence meleeSequence) => ContainsSequence(meleeSequence.GetHashCode());
            public bool ContainsSequence(int hashCode)
            {
                if (IsGroup) return false;
                if (IsSequence)
                {
                    if (sequenceInfo.GetHashCode() == hashCode) return true;
                    foreach (var wraps in sequenceInfo.meleeWraps)
                    {
                        if (wraps.ContainsSequence(hashCode)) return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 重新导出动作序列
        /// </summary>
        public void Recalculate()
        {
            resultGroups.Clear();
            foreach (var wrap in meleeWraps)
                resultGroups.AddRange(wrap.GetMeleeGroups());
            counter = 0;
            timer = 0;
        }
        public void Add(MeleeGSWraper wraper)
        {
            if (wraper.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            meleeWraps.Add(wraper);
            Recalculate();
        }
        public void Insert(int index, MeleeGSWraper wraper)
        {
            if (wraper.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            meleeWraps.Insert(index, wraper);
            Recalculate();

        }
        public List<MeleeGroup> resultGroups = new List<MeleeGroup>();
        /// <summary>
        /// 请不要对这个列表直接add
        /// </summary>
        public List<MeleeGSWraper> meleeWraps = new List<MeleeGSWraper>();
        public IMeleeAttackData currentData;
        public string SequenceName = "My MeleeSequence";
        public int counter;
        public int timer;
        public bool Attacktive;
        public void Update(Player player)
        {
            if (timer <= 0 || currentData == null)
            {
                if (currentData != null) currentData.OnDeactive();
                currentData = resultGroups[counter % resultGroups.Count].GetCurrentMeleeData();
                timer = (int)(player.itemAnimationMax * currentData.ModifyData.actionOffsetSpeed);
                counter++;
            }
            if (currentData != null)
            {
                bool oldValue = Attacktive;
                Attacktive = currentData.Attacktive;
                if (!oldValue && Attacktive)
                {
                    currentData.OnStartAttack();
                }
                if (oldValue && !Attacktive)
                    currentData.OnEndAttack();
                if (Attacktive) currentData.OnAttack();
                else currentData.OnCharge();
                currentData.Update(ref timer, out bool flag);
                if (flag)
                    timer--;
            }
        }
    }
    public class SwooshInfo : IMeleeAttackData
    {
        public bool negativeDir = false;
        public float kValue = 1;

        public SwooshInfo()
        {
        }

        public virtual float Factor => throw new NotImplementedException();

        public float Rotation => throw new NotImplementedException();

        public float Size => throw new NotImplementedException();

        public bool Attacktive => throw new NotImplementedException();

        public Condition Condition { get; set; } = new Condition("Always", () => true);

        public virtual void OnAttack()
        {
            throw new NotImplementedException();
        }

        public virtual void OnCharge()
        {
            throw new NotImplementedException();
        }

        public virtual void OnEndAttack()
        {
            throw new NotImplementedException();
        }

        public virtual void OnStartAttack()
        {
            throw new NotImplementedException();
        }

        public virtual void OnActive()
        {
            throw new NotImplementedException();
        }

        public virtual void OnDeactive()
        {
            throw new NotImplementedException();
        }

        public virtual void Update(ref int timer, out bool canReduceTimer)
        {
            throw new NotImplementedException();
        }
    }
    public class StabInfo : IMeleeAttackData
    {
        public bool negativeDir = false;
        public float kValue = 1;

        public StabInfo()
        {
        }
        public int Cycle { get => realCycle; set => givenCycle = value; }
        protected int realCycle;
        protected int givenCycle;
        public virtual float Factor => throw new NotImplementedException();

        public float Rotation => throw new NotImplementedException();

        public float Size => throw new NotImplementedException();

        public bool Attacktive => throw new NotImplementedException();

        public Condition Condition { get; set; } = new Condition("Always", () => true);

        public Vector2 offsetCenter => throw new NotImplementedException();

        public virtual void OnAttack()
        {
            throw new NotImplementedException();
        }

        public virtual void OnCharge()
        {
            throw new NotImplementedException();
        }

        public virtual void OnEndAttack()
        {
            throw new NotImplementedException();
        }

        public virtual void OnStartAttack()
        {
            throw new NotImplementedException();
        }

        public virtual void OnActive()
        {
            throw new NotImplementedException();
        }

        public virtual void OnDeactive()
        {
            throw new NotImplementedException();
        }

        public virtual void Update(ref int timer, out bool canReduceTimer)
        {
            throw new NotImplementedException();
        }
    }
    public class RapidlyStabInfo : StabInfo
    {
        public (int min, int max) CycleOffsetRange 
        {
            get => range;
            set 
            {
                var v = value;
                v.min = Math.Clamp(v.min, int.MinValue, v.max);
                v.max = Math.Clamp(v.max, v.min, int.MaxValue);
                range = v;
            }
        }
        (int, int) range;
        void ResetCycle() => realCycle = Math.Clamp(givenCycle + Main.rand.Next(range.Item1, range.Item2), 1, int.MaxValue);
        public override void OnActive()
        {
            ResetCycle();
            base.OnActive();
        }
        public override void OnEndAttack()
        {
            ResetCycle();
            base.OnEndAttack();
        }
    }
    public class ConvoluteInfo : IMeleeAttackData
    {
        public float Factor => throw new NotImplementedException();

        public float Rotation => throw new NotImplementedException();

        public float Size => throw new NotImplementedException();

        public bool Attacktive => throw new NotImplementedException();

        public Condition Condition => throw new NotImplementedException();

        public void OnActive()
        {
            throw new NotImplementedException();
        }

        public void OnAttack()
        {
            throw new NotImplementedException();
        }

        public void OnCharge()
        {
            throw new NotImplementedException();
        }

        public void OnDeactive()
        {
            throw new NotImplementedException();
        }

        public void OnEndAttack()
        {
            throw new NotImplementedException();
        }

        public void OnStartAttack()
        {
            throw new NotImplementedException();
        }

        public void Update(ref int timer, out bool canReduceTimer)
        {
            throw new NotImplementedException();
        }
    }
    public class ShockingDashInfo : IMeleeAttackData
    {
        public float Factor => throw new NotImplementedException();

        public float Rotation => throw new NotImplementedException();

        public float Size => throw new NotImplementedException();

        public bool Attacktive => throw new NotImplementedException();

        public Condition Condition => throw new NotImplementedException();

        public void OnActive()
        {
            throw new NotImplementedException();
        }

        public void OnAttack()
        {
            throw new NotImplementedException();
        }

        public void OnCharge()
        {
            throw new NotImplementedException();
        }

        public void OnDeactive()
        {
            throw new NotImplementedException();
        }

        public void OnEndAttack()
        {
            throw new NotImplementedException();
        }

        public void OnStartAttack()
        {
            throw new NotImplementedException();
        }

        public void Update(ref int timer, out bool canReduceTimer)
        {
            throw new NotImplementedException();
        }
    }
}

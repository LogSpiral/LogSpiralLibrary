using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        MeleeModifyData ModifyData => new MeleeModifyData();

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

        float Rotation { get; }

        float Size { get; }

        bool Attacktive { get; }

        void OnEndAttack();

        Condition Condition { get; }
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
                    if(meleeAtack.Condition.IsMet())
                        result = meleeAtack;
                }
                result ??= meleeAttackDatas.FirstOrDefault();
                Label:
                return result;
            }
        }
        public List<MeleeGroup> meleeGroups = new List<MeleeGroup>();
        public IMeleeAttackData currentData;
    }
    public struct SwooshInfo : IMeleeAttackData
    {
        public bool negativeDir = false;
        public float kValue = 1;

        public SwooshInfo()
        {
        }

        public float Factor => throw new NotImplementedException();

        public float Rotation => throw new NotImplementedException();

        public float Size => throw new NotImplementedException();

        public bool Attacktive => throw new NotImplementedException();

        public Condition Condition { get; set; }


        public void OnEndAttack()
        {
            throw new NotImplementedException();
        }
    }
    public struct StabInfo : IMeleeAttackData 
    {
        public bool negativeDir = false;
        public float kValue = 1;

        public StabInfo()
        {
        }

        public float Factor => throw new NotImplementedException();

        public float Rotation => throw new NotImplementedException();

        public float Size => throw new NotImplementedException();

        public bool Attacktive => throw new NotImplementedException();

        public Condition Condition { get; set; }

        public Vector2 offsetCenter => throw new NotImplementedException();

        public void OnEndAttack()
        {
            throw new NotImplementedException();
        }
    }
}

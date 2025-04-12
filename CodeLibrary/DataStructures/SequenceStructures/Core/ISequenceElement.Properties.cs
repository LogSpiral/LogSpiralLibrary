using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

partial interface ISequenceElement
{
    #region 参数属性
    //持续时间 角度 位移 修改数据
    /// <summary>
    /// 使用数据修改
    /// </summary>
    [ElementCustomData]
    ActionModifyData ModifyData => new();
    /// <summary>
    /// 执行次数
    /// </summary>
    [ElementCustomData]
    int Cycle => 1;
    #endregion

    #region 逻辑属性
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

    #region 插值属性
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
    Vector2 offsetOrigin => new(.1f, .9f);
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

    #region 辅助属性
    Entity Owner { get; set; }
    Projectile Projectile { get; set; }
    StandardInfo standardInfo { get; set; }
    string Category { get; }
    #endregion
}

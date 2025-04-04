using System.IO;
using System.Xml;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

public interface ISequenceElement : ILocalizedModType, ILoadable
{
    #region 属性
    #region 编排序列时调整
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

    #region Net
    void NetSend(BinaryWriter writer);
    void NetReceive(BinaryReader reader);
    #endregion
    #endregion
    #region 吃闲饭的
    Entity Owner { get; set; }
    Projectile Projectile { get; set; }
    StandardInfo standardInfo { get; set; }

    string Category { get; }
    #endregion
}

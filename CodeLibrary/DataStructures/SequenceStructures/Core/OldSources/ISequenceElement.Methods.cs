//using System.IO;
//using System.Xml;
//namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.OldSources;


//public partial interface ISequenceElement : ILocalizedModType, ILoadable
//{
//    void Update(bool triggered);

//    /// <summary>
//    /// 被切换时调用,脉冲性
//    /// </summary>
//    void OnActive();

//    /// <summary>
//    /// 被换走时调用,脉冲性
//    /// </summary>
//    void OnDeactive();

//    /// <summary>
//    /// 开始执行单次时
//    /// </summary>
//    void OnStartSingle();

//    /// <summary>
//    /// 结束执行单次时
//    /// </summary>
//    void OnEndSingle();

//    /// <summary>
//    /// 攻击以外时间调用,持续性
//    /// </summary>
//    void OnCharge();

//    /// <summary>
//    /// 开始攻击时调用,脉冲性
//    /// </summary>
//    void OnStartAttack();

//    /// <summary>
//    /// 攻击期间调用,持续性
//    /// </summary>
//    void OnAttack();

//    /// <summary>
//    /// 结束时调用,脉冲性
//    /// </summary>
//    void OnEndAttack();

//    void Draw(SpriteBatch spriteBatch, Texture2D texture);
//}

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;

// 这个文件用于实现各种游戏实质性内容
public partial class SequencePlayer : ModPlayer
{
    /// <summary>
    /// 挥砍等组件的缓存时间
    /// </summary>
    public double cachedTime;

    /// <summary>
    /// 挂起强制执行下一个组
    /// </summary>
    public bool PendingForcedNext;
}

using System.IO;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

//这个文件包含了由弹幕接入序列元素代理的实现
partial class MeleeSequenceProj
{
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (currentData == null || !currentData.Attacktive) return false;
        return currentData.Collide(targetHitbox);
    }

    //下面这里实现手持弹幕的一些比较细枝末节的东西，像是绘制 攻击到目标的伤害修正之类
    public override bool PreDraw(ref Color lightColor)
    {
        if (currentData != null)
        {
            meleeSequence.active = true;
            currentData.Draw(Main.spriteBatch, TextureAssets.Projectile[Type].Value);
        }
        return false;
    }

    //这两个本来是做联机同步用的，但是后来发现我这里用不上
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        //meleeSequence?.currentData?.NetReceive(reader);
        base.ReceiveExtraAI(reader);
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        //meleeSequence?.currentData?.NetSend(writer);
        base.SendExtraAI(writer);
    }
}

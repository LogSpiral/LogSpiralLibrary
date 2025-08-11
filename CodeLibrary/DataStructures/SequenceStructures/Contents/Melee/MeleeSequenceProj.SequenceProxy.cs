using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.IO;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

//这个文件包含了由弹幕接入序列元素代理的实现
partial class MeleeSequenceProj
{
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CurrentElement == null || !CurrentElement.Attacktive) return false;
        return CurrentElement.Collide(targetHitbox);
    }

    //下面这里实现手持弹幕的一些比较细枝末节的东西，像是绘制 攻击到目标的伤害修正之类
    public override bool PreDraw(ref Color lightColor)
    {
        CurrentElement?.Draw(Main.spriteBatch, TextureAssets.Projectile[Type].Value);
        return false;
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        var elementName = reader.ReadString();
        if (!SequenceGlobalManager.ElementTypeLookup.TryGetValue(elementName, out var elementType)) return;
        var element = Activator.CreateInstance(elementType) as MeleeAction;
        element.NetReceive(reader);
        CurrentElement = element;
        base.ReceiveExtraAI(reader);
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        if (CurrentElement != null)
        {
            writer.Write(CurrentElement.FullName);
            CurrentElement.NetSend(writer);
        }
        else
        {
            writer.Write("NoneElement");
        }
        base.SendExtraAI(writer);
    }
}

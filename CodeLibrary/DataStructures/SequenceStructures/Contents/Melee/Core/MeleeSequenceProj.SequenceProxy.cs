using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties;
using ReLogic.Graphics;
using System.IO;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

//这个文件包含了由弹幕接入序列元素代理的实现
public partial class MeleeSequenceProj
{
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CurrentElement == null || CurrentElement.IsCompleted || !CurrentElement.Attacktive) return false;
        return CurrentElement.Collide(targetHitbox);
    }

    //下面这里实现手持弹幕的一些比较细枝末节的东西，像是绘制 攻击到目标的伤害修正之类
    public override bool PreDraw(ref Color lightColor)
    {
        DrawBlade(TextureAssets.Projectile[Type].Value);
        return false;
    }

    protected virtual void DrawBlade(Texture2D texture)
    {
        if (CurrentElement == null) return;
        if (!CurrentElement.IsCompleted)
            CurrentElement.Draw(Main.spriteBatch, texture);

        else
        {
            var element = CurrentElement;
            bool towradsRight = Player.direction == 1;

            var factor = Utils.GetLerpValue(30, 20, Projectile.timeLeft, true);
            factor = MathF.Sqrt(factor);
            factor = MathHelper.SmoothStep(0, 1, factor);
            var center = Vector2.Lerp(element.OffsetCenter, new Vector2(Player.direction * -16, 8), factor);
            var offRot = Utils.AngleLerp(element.OffsetRotation, 0, factor);
            var dirRot = Utils.AngleLerp(element.Rotation, towradsRight ? MathHelper.Pi / 16f : MathHelper.Pi * 15 / 16f, factor);

            Main.spriteBatch.Draw(texture,
                Player.MountedCenter + center - Main.screenPosition,
                StandardInfo.frame,
                Color.White,
                -StandardInfo.standardRotation,
                offRot,
                dirRot,
                (StandardInfo.standardOrigin + element.OffsetOrigin * (1 - factor))
                * (StandardInfo.frame?.Size() ?? texture.Size()),
                element.OffsetSize * new Vector2(1, factor + (1 - factor) * (1 / element.KValue)),
                towradsRight);


            // Main.spriteBatch.DrawString(FontAssets.MouseText.Value, "Idle", Player.MountedCenter - Vector2.UnitY * 64 - Main.screenPosition, Color.Red);
        }
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        var elementName = reader.ReadString();
        if (!SequenceGlobalManager.ElementTypeLookup.TryGetValue(elementName, out var elementType)) return;
        var element = Activator.CreateInstance(elementType) as MeleeAction;
        element.NetReceiveInitializeElement(reader);
        int timer = reader.ReadUInt16();
        if (IsLocalProj) return;
        CurrentElement = element;
        CurrentElement.StandardInfo = StandardInfo;
        StandardInfo.standardTimer = timer;
        CurrentElement.Owner = Player;
        CurrentElement.Projectile = Projectile;
        base.ReceiveExtraAI(reader);
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        if (CurrentElement != null)
        {
            writer.Write(CurrentElement.FullName);
            CurrentElement.NetSendInitializeElement(writer);
            writer.Write((ushort)StandardInfo.standardTimer);
        }
        else
        {
            writer.Write("NoneElement");
        }
        base.SendExtraAI(writer);
    }
}
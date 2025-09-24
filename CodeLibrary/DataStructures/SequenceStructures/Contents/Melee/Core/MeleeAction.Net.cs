using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using NetSimplified;
using System.IO;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

public partial class MeleeAction
{
    //public virtual void ReadAttributes(IReadOnlyDictionary<string, string> attributes)
    //{
    //}
    public virtual void NetSendInitializeElement(BinaryWriter writer)
    {
        writer.Write((byte)CounterMax);
        ModifyData.WriteBinary(writer);
        writer.Write(Rotation);
        writer.Write(KValue);
        writer.Write(Flip);
    }

    public virtual void NetReceiveInitializeElement(BinaryReader reader)
    {
        CounterMax = reader.ReadByte();
        ModifyData = ActionModifyData.ReadBinary(reader);
        Rotation = reader.ReadSingle();
        KValue = reader.ReadSingle();
        Flip = reader.ReadBoolean();
    }


    public bool NetUpdateNeeded { get; set; }

    /// <summary>
    /// 用于发送更新时需要同步的信息
    /// </summary>
    /// <param name="writer"></param>
    public virtual void NetSendUpdateElement(BinaryWriter writer)
    {
        writer.Write(Rotation);
        writer.Write(KValue);
        writer.Write(Flip);
        writer.Write((sbyte)Owner.direction);
    }

    public virtual void NetReceiveUpdateElement(BinaryReader reader)
    {
        Rotation = reader.ReadSingle();
        KValue = reader.ReadSingle();
        Flip = reader.ReadBoolean();
        var direction = reader.ReadSByte();
        Owner?.direction = direction;
    }

    private class MeleeActionUpdateSync : NetModule
    {
        private MeleeAction _action;

        public static MeleeActionUpdateSync Get(MeleeAction action)
        {
            var packet = NetModuleLoader.Get<MeleeActionUpdateSync>();
            packet._action = action;
            return packet;
        }
        public override void Send(ModPacket p)
        {
            p.Write((short)_action.Projectile.whoAmI);
            p.Write(_action.FullName);
            _action.NetSendUpdateElement(p);
        }
        public override void Read(BinaryReader r)
        {
            int index = r.ReadInt16();
            string name = r.ReadString();
            // TODO 与meleeSequenceProj解耦？
            if (Main.projectile[index].ModProjectile is not MeleeSequenceProj meleeSequenceProj
                || meleeSequenceProj.CurrentElement is not { } element || element.FullName != name)
                element = Activator.CreateInstance(SequenceGlobalManager.ElementTypeLookup[name]) as MeleeAction;

            element.NetReceiveUpdateElement(r);

            if (Main.dedServ)
                Get(element).Send(-1, Sender);
        }
        public override void Receive()
        {

        }
    }
}
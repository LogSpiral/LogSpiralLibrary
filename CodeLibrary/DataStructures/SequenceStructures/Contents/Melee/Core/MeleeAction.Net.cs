using System.IO;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

public partial class MeleeAction
{
    //public virtual void ReadAttributes(IReadOnlyDictionary<string, string> attributes)
    //{
    //}
    public virtual void NetSend(BinaryWriter writer)
    {
        if (Counter is 1 or 0) // 服务端转发的时候Counter应该还是0
        {
            writer.Write(true);
            writer.Write((byte)CounterMax);
            ModifyData.WriteBinary(writer);
        }
        else writer.Write(false);
        writer.Write(Rotation);
        writer.Write(KValue);
        writer.Write(Flip);

    }

    public virtual void NetReceive(BinaryReader reader)
    {
        bool start = reader.ReadBoolean();
        if (start)
        {
            CounterMax = reader.ReadByte();
            ModifyData.ReadBinary(reader);
        }
        Rotation = reader.ReadSingle();
        KValue = reader.ReadSingle();
        Flip = reader.ReadBoolean();
    }
}
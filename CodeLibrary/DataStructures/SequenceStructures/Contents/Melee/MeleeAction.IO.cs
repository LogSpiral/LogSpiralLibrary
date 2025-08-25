using System.IO;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

public partial class MeleeAction
{
    //public virtual void ReadAttributes(IReadOnlyDictionary<string, string> attributes)
    //{
    //}
    public virtual void NetSend(BinaryWriter writer)
    {
    }

    public virtual void NetReceive(BinaryReader reader)
    {
    }
}
using System.IO;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

partial class MeleeAction
{
    // 迁移至接口的默认实现
    /*public virtual void LoadAttribute(XmlReader xmlReader)
    {
        (this as ISequenceElement).LoadAttribute(xmlReader);
    }
    public virtual void SaveAttribute(XmlWriter xmlWriter)
    {
        (this as ISequenceElement).SaveAttribute(xmlWriter);
    }*/

    public virtual void NetSend(BinaryWriter writer)
    {

    }
    public virtual void NetReceive(BinaryReader reader)
    {

    }
}

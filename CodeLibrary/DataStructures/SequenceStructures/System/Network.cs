using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using NetSimplified;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
public class SyncSingleSequence : NetModule
{
    public Sequence Sequence;
    public int plrIndex;
    public int ElementTypeIndex;
    public Type ElementType;
    public static SyncSingleSequence Get(int plrIndex, Sequence sequence, Type ElementType)
    {
        SyncSingleSequence result = NetModuleLoader.Get<SyncSingleSequence>();
        result.Sequence = sequence;
        result.plrIndex = plrIndex;
        result.ElementTypeIndex = SequenceSystem.AvailableElementBaseTypes.IndexOf(ElementType);
        result.ElementType = ElementType;
        return result;
    }
    public override void Send(ModPacket p)
    {
        p.Write((byte)plrIndex);
        p.Write((byte)ElementTypeIndex);
        using MemoryStream memoryStream = new();
        XmlWriterSettings settings = new()
        {
            Indent = true,
            Encoding = new UTF8Encoding(false),
            NewLineChars = Environment.NewLine
        };
        XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings);
        Sequence.WriteContent(xmlWriter);
        xmlWriter.Dispose();
        p.Write(Sequence.KeyName);
        p.Write((int)memoryStream.Length);
        p.Write(memoryStream.ToArray());
        base.Send(p);
    }
    public override void Read(BinaryReader r)
    {
        plrIndex = r.ReadByte();
        ElementTypeIndex = r.ReadByte();
        ElementType = SequenceSystem.AvailableElementBaseTypes[ElementTypeIndex];
        string keyName = r.ReadString();
        int bLength = r.ReadInt32();
        byte[] buffer = r.ReadBytes(bLength);
        using MemoryStream memoryStream = new(buffer);
        using XmlReader xmlReader = XmlReader.Create(memoryStream);
        var seq = SequenceSystem.GetLoad(ElementType).Invoke(null, [xmlReader, keyName.Split('/')[0]]);

        Sequence = Main.player[plrIndex].GetModPlayer<SequencePlayer>().plrLocSeq[ElementType][keyName] = (Sequence)seq;
        base.Read(r);
    }
    public override void Receive()
    {
        if (Main.dedServ)
            Get(plrIndex, Sequence, ElementType).Send(-1, plrIndex);
    }
}
public class SyncAllSequence : NetModule
{
    public Dictionary<Type, Dictionary<string, Sequence>> plrLocSeq;
    public int plrIndex;
    public static SyncAllSequence Get(int plrIndex, Dictionary<Type, Dictionary<string, Sequence>> plrSeq)
    {
        SyncAllSequence result = NetModuleLoader.Get<SyncAllSequence>();
        result.plrLocSeq = plrSeq;
        result.plrIndex = plrIndex;
        return result;
    }
    public override void Send(ModPacket p)
    {
        p.Write((byte)plrIndex);
        foreach (var type in SequenceSystem.AvailableElementBaseTypes)
            SequenceSystem.GetWriteAll(type).Invoke(null, [p]);
    }
    public override void Read(BinaryReader r)
    {
        plrIndex = r.ReadByte();
        var seqPlr = Main.player[plrIndex].GetModPlayer<SequencePlayer>();
        seqPlr.ReceiveAllSeqFile(r);
        plrLocSeq = seqPlr.plrLocSeq;
    }
    public override void Receive()
    {
        if (Main.dedServ)
            Get(plrIndex, plrLocSeq).Send(-1, plrIndex);
    }
}

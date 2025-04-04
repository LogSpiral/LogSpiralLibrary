using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UI;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Terraria.GameInput;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;

// 这个文件用来实现读取 同步等逻辑实现
partial class SequencePlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (SequenceSystem.ShowSequenceKeybind.JustPressed)
        {
            if (SequenceUI.Visible)
                SequenceSystem.instance.sequenceUI.Close();
            else
                SequenceSystem.instance.sequenceUI.Open();
        }
        base.ProcessTriggers(triggersSet);
    }
    public Dictionary<Type, Dictionary<string, Sequence>> plrLocSeq = null;
    public void InitPlrLocSeq()
    {
        plrLocSeq = [];
        foreach (var type in SequenceSystem.AvailableElementBaseTypes)
            plrLocSeq[type] = [];
    }
    public SequencePlayer()
    {
        InitPlrLocSeq();
    }
    public void ReceiveAllSeqFile(BinaryReader reader)
    {
        foreach (var type in SequenceSystem.AvailableElementBaseTypes)
        {
            byte seqCount = reader.ReadByte();
            var dict = plrLocSeq[type];
            var method = SequenceSystem.GetLoad(type);
            for (int u = 0; u < seqCount; u++)
            {
                int bCount = reader.ReadInt32();
                string keyName = reader.ReadString();
                byte[] bytes = reader.ReadBytes(bCount);
                using MemoryStream memoryStream = new(bytes);
                using XmlReader xmlReader = XmlReader.Create(memoryStream);
                var seq = method.Invoke(null, [xmlReader, keyName.Split('/')[0]]);
                dict[keyName] = (Sequence)seq;
            }
        }
    }
    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        SyncAllSequence.Get(Player.whoAmI, plrLocSeq).Send(toWho, fromWho);
        base.SyncPlayer(toWho, fromWho, newPlayer);
    }
}

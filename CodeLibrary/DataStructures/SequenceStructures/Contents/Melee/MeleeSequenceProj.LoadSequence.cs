using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

partial class MeleeSequenceProj
{
    //初始化-加载序列数据
    /// <summary>
    /// 是否是本地序列的弹幕
    /// </summary>
    bool IsLocalProj => player.whoAmI == Main.myPlayer;
    /// <summary>
    /// 标记为完工，设置为true后将读取与文件同目录下同类名的xml文件(参考Texture默认读取
    /// </summary>
    public virtual bool LabeledAsCompleted => false;
    public static Dictionary<int, MeleeSequence> LocalMeleeSequence = [];
    protected MeleeSequence meleeSequence = null;
    public MeleeSequence MeleeSequenceData
    {
        get => meleeSequence;
    }

    //这两个函数是用来初始化执行的逻辑序列的
    //因为之前还没有UI编辑制作或者XML文件记录序列，所以之前是重写SetUpSequence来写入序列的具体内容
    public virtual void SetUpSequence(MeleeSequence sequence, string modName, string fileName)
    {
        if (LabeledAsCompleted)
        {
            string inModDirectoryPath = GetType().Namespace.Replace(Mod.Name + ".", "").Replace('.', '/');
            if (!LocalMeleeSequence.TryGetValue(Type, out MeleeSequence localSeq))
            {
                localSeq = new MeleeSequence();
                MeleeSequence.Load($"{inModDirectoryPath}/{Name}.xml", Mod, inModDirectoryPath, localSeq);
                LocalMeleeSequence[Type] = localSeq;
            }
            meleeSequence = localSeq.LocalSequenceClone(inModDirectoryPath);
            //meleeSequence = new MeleeSequence() { groups = ((MeleeSequence)LocalMeleeSequence.Clone()).groups };
            return;
        }
        if (IsLocalProj)
        {
            var path = $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{nameof(MeleeAction)}/{modName}/{fileName}.xml";
            if (File.Exists(path))
                MeleeSequence.Load(path, sequence);
            else
            {
                sequence = new MeleeSequence();
                sequence.Add(new SwooshInfo());
                sequence.mod = Mod;
                sequence.sequenceName = Name;
                SequenceSystem.sequenceInfos[sequence.KeyName] =
                    new SequenceBasicInfo()
                    {
                        AuthorName = "LSL",
                        Description = "Auto Spawn By LogSpiralLibrary.",
                        FileName = Name,
                        DisplayName = Name,
                        ModDefinition = new ModDefinition(Mod.Name),
                        createDate = DateTime.Now,
                        lastModifyDate = DateTime.Now,
                        Finished = true
                    };
                sequence.Save();
            }
            SequenceManager<MeleeAction>.sequences[sequence.KeyName] = sequence;
            meleeSequence = sequence;
        }
        else
        {
            var sPlayer = player.GetModPlayer<SequencePlayer>();
            var sDict = sPlayer.plrLocSeq;
            if (sDict == null)
            {
                sPlayer.InitPlrLocSeq();
                sDict = sPlayer.plrLocSeq;
                return;
            }
            var dict = sDict[typeof(MeleeAction)];
            var result = dict[$"{modName}/{fileName}"];
            meleeSequence = (MeleeSequence)result;
            //meleeSequence = new MeleeSequence() { groups = ((MeleeSequence)result).groups };
        }
    }
    public virtual void InitializeSequence(string modName, string fileName)
    {

        if (!LabeledAsCompleted && IsLocalProj && SequenceManager<MeleeAction>.sequences.TryGetValue($"{modName}/{fileName}", out var value) && value is MeleeSequence sequence)
        {
            meleeSequence = new MeleeSequence() { groups = sequence.groups };
        }
        else
        {
            //meleeSequence.sequenceName = Name;
            //meleeSequence.mod = Mod;
            SetUpSequence(meleeSequence, modName, fileName);
        }
        meleeSequence?.ResetCounter();
    }
    //public abstract void SetUpSequence(MeleeSequence meleeSequence);//也因此，以前这个是抽象函数，每个弹幕要自己写入组件数据

    public override void Load()
    {
        var methods = GetType().GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var method in methods)
        {
            if (!Attribute.IsDefined(method, typeof(SequenceDelegateAttribute)))
                continue;
            var paras = method.GetParameters();
            if (paras.Length != 1 || !paras[0].ParameterType.IsAssignableTo(typeof(ISequenceElement)))
                continue;
            SequenceSystem.elementDelegates[$"{Name}/{method.Name}"] = element =>
            {
                if (element is not MeleeAction action) return;
                method.Invoke(null, [element]);
            };
        }
        base.Load();
    }
}

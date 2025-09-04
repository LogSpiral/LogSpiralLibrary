using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.BuiltInGroups;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Interfaces;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

public partial class MeleeSequenceProj
{
    //初始化-加载序列数据
    /// <summary>
    /// 是否是本地序列的弹幕
    /// </summary>
    private bool IsLocalProj => Player.whoAmI == Main.myPlayer;

    /// <summary>
    /// 标记为完工，设置为true后将读取与文件同目录下同类名的xml文件(参考Texture默认读取
    /// </summary>
    public virtual bool LabeledAsCompleted => false;

    public static Dictionary<int, Sequence> LocalMeleeSequence { get; } = [];
    protected Sequence meleeSequence = null;
    public SequenceModel SequenceModel { get; protected set; }

    private static void LoadLocalLibrarySequences(Sequence sequence, string inModDirectoryPath, Mod mod)
    {
        var unloads = SequenceCheckHelper.GetUnloadWrapperList(sequence);
        foreach (var unloadWrapper in unloads)
        {
            var fullName = unloadWrapper.RefSequenceFullName;
            SequenceData.ParseKeyName(fullName, out _, out _, out string unloadFileName);
            var path = $"{inModDirectoryPath}/{unloadFileName}.xml";
            if (!mod.FileExists(path)) continue;
            using MemoryStream stream = new(mod.GetFileBytes(path));
            var libSequence = SequenceManager<MeleeAction>.RegisterSingleSequence($"{mod.Name}/{unloadFileName}", stream);

            LoadLocalLibrarySequences(libSequence, inModDirectoryPath, mod);
        }
    }

    //这两个函数是用来初始化执行的逻辑序列的
    //因为之前还没有UI编辑制作或者XML文件记录序列，所以之前是重写SetUpSequence来写入序列的具体内容
    public virtual void SetUpSequence(Sequence sequence, string modName, string fileName)
    {
        if (LabeledAsCompleted)
        {
            if (!LocalMeleeSequence.TryGetValue(Type, out Sequence localSeq))
            {
                string inModDirectoryPath = GetType().Namespace.Replace(Mod.Name + ".", "").Replace('.', '/');
                var fullName = FullName;
                using MemoryStream stream = new(Mod.GetFileBytes($"{inModDirectoryPath}/{Name}.xml"));
                LocalMeleeSequence[Type] = localSeq = SequenceManager<MeleeAction>.RegisterSingleSequence(fullName, stream);

                LoadLocalLibrarySequences(localSeq, inModDirectoryPath, Mod);
            }
            meleeSequence = localSeq;
            return;
        }
        if (IsLocalProj)
        {
            var path = Path.Combine(SequenceSystem.SequenceSavePath, nameof(MeleeAction), modName, $"{fileName}.xml");
            string fullName = $"{modName}/{fileName}";
            if (File.Exists(path))
            {
                using FileStream fs = new(path, FileMode.Open);
                meleeSequence = SequenceManager<MeleeAction>.RegisterSingleSequence(fullName, fs);
                meleeSequence.Data.ModDefinition = new(Mod.Name);
            }
            else
            {
                sequence = new Sequence();
                sequence.Groups.Add(new SingleWrapperGroup(new Wrapper(new SwooshInfo())));
                sequence.Data = new SequenceData()
                {
                    AuthorName = "LSL",
                    Description = "Auto Spawn By LogSpiralLibrary.",
                    FileName = Name,
                    DisplayName = Name,
                    // LoadTimeElementTypeName = nameof(MeleeAction),
                    ModDefinition = new ModDefinition(Mod.Name),
                    CreateTime = DateTime.Now,
                    ModifyTime = DateTime.Now,
                    Finished = true
                };
                SequenceManager<MeleeAction>.RegisterSingleSequence(fullName, sequence);
                meleeSequence = sequence;
            }
        }
        else
        {
            // 非本地序列代理弹幕没有序列实例，只有接收的元素实例
        }
    }

    public virtual void InitializeSequence(string modName, string fileName)
    {
        if (!LabeledAsCompleted
            && IsLocalProj
            && SequenceManager<MeleeAction>
                .Instance
                .Sequences
                .TryGetValue($"{modName}/{nameof(MeleeAction)}/{fileName}",
                out var value)
            && value is Sequence sequence)
        {
            meleeSequence = sequence;
        }
        else
        {
            SetUpSequence(meleeSequence, modName, fileName);
        }
        SequenceModel = new(meleeSequence);
        SequenceModel.OnInitializeElement += element =>
        {
            if (element is not MeleeAction action) return;
            action.StandardInfo = StandardInfo;
            action.Owner = Player;
            action.Projectile = Projectile;
            Projectile.netUpdate = true;
        };
    }

    public override void OnSpawn(IEntitySource source)
    {
        InitializeSequence(Mod.Name, Name);
        base.OnSpawn(source);
    }

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
                method.Invoke(null, [element]);
            };
        }
        base.Load();
    }
}
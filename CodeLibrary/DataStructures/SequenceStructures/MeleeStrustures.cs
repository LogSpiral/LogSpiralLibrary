using LogSpiralLibrary.CodeLibrary;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.WorldBuilding;
using static Terraria.NPC.NPCNameFakeLanguageCategoryPassthrough;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class SequenceDelegateAttribute : Attribute
    {
        public SequenceDelegateAttribute()
        {
        }
    }
    /// <summary>
    /// 来把基剑
    /// </summary>
    public abstract class MeleeSequenceProj : ModProjectile
    {
        bool IsLocalProj => player.whoAmI == Main.myPlayer;

        public MeleeSequence MeleeSequenceData
        {
            get => meleeSequence;
        }
        public virtual StandardInfo StandardInfo => new StandardInfo(-MathHelper.PiOver4, new Vector2(0.1f, 0.9f), player.itemAnimationMax, Color.White, null, ItemID.IronBroadsword);
        /// <summary>
        /// 标记为完工，设置为true后将读取与文件同目录下同类名的xml文件(参考Texture默认读取
        /// </summary>
        public virtual bool LabeledAsCompleted => false;
        public static MeleeSequence LocalMeleeSequence;
        //public abstract void SetUpSequence(MeleeSequence meleeSequence);
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            //meleeSequence?.currentData?.NetReceive(reader);
            base.ReceiveExtraAI(reader);
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            //meleeSequence?.currentData?.NetSend(writer);
            base.SendExtraAI(writer);
        }
        public virtual void SetUpSequence(MeleeSequence sequence, string modName, string fileName)
        {
            if (LabeledAsCompleted)
            {
                if (LocalMeleeSequence == null)
                {
                    LocalMeleeSequence = new MeleeSequence();
                    MeleeSequence.Load((GetType().Namespace.Replace(Mod.Name + ".", "") + "." + Name).Replace('.', '/') + ".xml", Mod, LocalMeleeSequence);
                }
                //meleeSequence = (MeleeSequence)LocalMeleeSequence.Clone();
                meleeSequence = new MeleeSequence() { groups = ((SequenceBase<MeleeAction>)LocalMeleeSequence.Clone()).groups };
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
                //meleeSequence = (MeleeSequence)result;
                meleeSequence = new MeleeSequence() { groups = ((SequenceBase<MeleeAction>)result).groups };
            }
        }
        protected MeleeSequence meleeSequence = null;
        public IMeleeAttackData currentData => meleeSequence.currentData;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 10;
            Projectile.width = Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.ownerHitCheck = true;
            if (!SequenceSystem.loaded) ModContent.GetInstance<SequenceSystem>().Load();
            if (!SequenceManager<MeleeAction>.loaded) SequenceManager<MeleeAction>.Load();
            //InitializeSequence(Mod.Name, Name);

            base.SetDefaults();
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
                    if (element is not MeleeAction action) return;
                    method.Invoke(null, [element]);
                };
            }
            base.Load();
        }
        public virtual void InitializeSequence(string modName, string fileName)
        {

            if (!LabeledAsCompleted && IsLocalProj && SequenceManager<MeleeAction>.sequences.TryGetValue($"{modName}/{fileName}", out var value) && value is SequenceBase<MeleeAction> sequence)
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
        public Player player => Main.player[Projectile.owner];
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            player.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake = Main.rand.NextFloat(0.85f, 1.15f) * (damageDone / MathHelper.Clamp(player.HeldItem.damage, 1, int.MaxValue));//
            base.OnHitNPC(target, hit, damageDone);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            player.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake = Main.rand.NextFloat(0.85f, 1.15f);

            base.OnHitPlayer(target, info);
        }
        public override void AI()
        {
            //Main.NewText((player.whoAmI, Main.myPlayer));
            player.heldProj = Projectile.whoAmI;
            Projectile.damage = player.GetWeaponDamage(player.HeldItem);
            Projectile.direction = player.direction;
            Projectile.velocity = (player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition - player.Center).SafeNormalize(default);
            if (meleeSequence == null)
            {
                InitializeSequence(Mod.Name, Name);
                meleeSequence.SetOwner(player);
            }
            if (player.DeadOrGhost) Projectile.Kill();
            if (Projectile.timeLeft == 10) return;


            if (meleeSequence.Groups.Count < 1) return;
            bool flag1 = player.controlUseItem || player.controlUseTile || currentData == null;//首要-触发条件
            bool flag2 = false;//次要-持续条件
            if (currentData != null)
            {
                flag2 = currentData.counter < currentData.Cycle;//我还没完事呢
                flag2 |= currentData.counter == currentData.Cycle && currentData.timer >= 0;//最后一次
                                                                                            //flag2 &= !meleeSequence.currentWrapper.finished;//如果当前打包器完工了就给我停下
                                                                                            //Main.NewText(currentData.Cycle);
            }
            if (
               flag1 || flag2// 
                )
            {
                if (flag1 || ((currentData.counter < currentData.Cycle || (currentData.counter == currentData.Cycle && currentData.timer > 0)) && !meleeSequence.currentWrapper.finished))
                    Projectile.timeLeft = 2;

                meleeSequence.Update(player, Projectile, StandardInfo, flag1);
            }
            if (currentData == null) return;


            Projectile.Center = player.Center + currentData.offsetCenter;
            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var spb = Main.spriteBatch;
            //spb.Draw(LogSpiralLibraryMod.AniTex[8].Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Red);
            if (currentData != null)
            {
                meleeSequence.active = true;
                currentData.Draw(Main.spriteBatch, TextureAssets.Projectile[Type].Value);
            }
            //spb.PushSprite(LogSpiralLibraryMod.AniTex[8].Value, 0, 0, 1, 1, 400, 400, 800, 800, Color.Red, Color.Green, Color.Blue, Color.White, 162.5f, 162.5f, 0, 1, 0, 0);
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (currentData == null || !currentData.Attacktive) return false;
            return currentData.Collide(targetHitbox);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (currentData == null) return;
            //target.life = target.lifeMax;
            var data = currentData.ModifyData;
            modifiers.SourceDamage *= data.actionOffsetDamage;
            modifiers.Knockback *= data.actionOffsetKnockBack;
            var _crit = player.GetWeaponCrit(player.HeldItem);
            _crit += data.actionOffsetCritAdder;
            _crit = (int)(_crit * data.actionOffsetCritMultiplyer);
            if (Main.rand.Next(100) < _crit)
            {
                modifiers.SetCrit();
            }
            else
            {
                modifiers.DisableCrit();
            }
            target.immune[player.whoAmI] = 0;
            base.ModifyHitNPC(target, ref modifiers);
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (currentData == null) return;
            var data = currentData.ModifyData;
            modifiers.SourceDamage *= data.actionOffsetDamage;
            modifiers.Knockback *= data.actionOffsetKnockBack;
            base.ModifyHitPlayer(target, ref modifiers);
        }
        public override void OnKill(int timeLeft)
        {
            //if (Main.netMode != NetmodeID.Server)
            meleeSequence?.ResetCounter();
            base.OnKill(timeLeft);
        }
    }
    //↓旧版代码
    /*public interface IMeleeAttackData
    {
        //持续时间 角度 位移 修改数据
        /// <summary>
        /// 近战数据修改
        /// </summary>
        MeleeModifyData ModifyData => new MeleeModifyData();
        /// <summary>
        /// 执行次数
        /// </summary>
        int Cycle => 1;
        /// <summary>
        /// 中心偏移量，默认零向量
        /// </summary>
        Vector2 offsetCenter => default;
        /// <summary>
        /// 原点偏移量，默认为贴图左下角(0.1f,0.9f),取值范围[0,1]
        /// </summary>
        Vector2 offsetOrigin => new Vector2(.1f, .9f);
        /// <summary>
        /// 当前周期的进度
        /// </summary>
        float Factor { get; set; }

        /// <summary>
        /// 旋转量
        /// </summary>
        float Rotation { get; }
        /// <summary>
        /// 大小
        /// </summary>
        float Size { get; }
        /// <summary>
        /// 是否具有攻击性
        /// </summary>
        bool Attacktive { get; }

        /// <summary>
        /// 被切换时调用,脉冲性
        /// </summary>
        void OnActive();

        /// <summary>
        /// 被换走时调用,脉冲性
        /// </summary>
        void OnDeactive();

        /// <summary>
        /// 结束时调用,脉冲性
        /// </summary>
        void OnEndAttack();

        /// <summary>
        /// 开始攻击时调用,脉冲性
        /// </summary>
        void OnStartAttack();

        /// <summary>
        /// 攻击期间调用,持续性
        /// </summary>
        void OnAttack();

        /// <summary>
        /// 攻击以外时间调用,持续性
        /// </summary>
        void OnCharge();
        Condition Condition { get; }
        void Update(ref int timer, int timerMax, ref bool canReduceTimer);
        Player Player { get; set; }
        Projectile Projectile { get; set; }
    }
    public class MeleeSequence
    {
        public class MeleeGroup
        {
            public List<IMeleeAttackData> meleeAttackDatas = new List<IMeleeAttackData>();
            public IMeleeAttackData GetCurrentMeleeData()
            {
                IMeleeAttackData result = null;
                if (meleeAttackDatas == null || meleeAttackDatas.Count == 0) goto Label;
                foreach (var meleeAtack in meleeAttackDatas)
                {
                    if (meleeAtack.Condition.IsMet())
                        result = meleeAtack;
                }
                result ??= meleeAttackDatas.FirstOrDefault();
            Label:
                return result;
            }
            public string GroupName = "My MeleeGroup";
        }
        /// <summary>
        /// 把两个类打包在一个类的实用寄巧
        /// </summary>
        public class MeleeGSWraper
        {
            public readonly MeleeGroup groupInfo;
            public readonly MeleeSequence sequenceInfo;

            public bool IsGroup => groupInfo != null;
            public bool IsSequence => sequenceInfo != null;
            public string Name => groupInfo?.GroupName ?? sequenceInfo.SequenceName ?? "null";
            public List<MeleeGroup> GetMeleeGroups()
            {
                var result = new List<MeleeGroup>();
                if (IsGroup)
                {
                    result.Add(groupInfo);
                }
                else if (IsSequence)
                {
                    foreach (var wrap in sequenceInfo.meleeWraps)
                        result.AddRange(wrap.GetMeleeGroups());
                }
                return result;
            }
            public MeleeGSWraper(MeleeGroup group)
            {
                groupInfo = group;
            }

            public MeleeGSWraper(MeleeSequence sequence)
            {
                sequenceInfo = sequence;
            }
            public static implicit operator MeleeGSWraper(MeleeGroup group) => new MeleeGSWraper(group);
            public static implicit operator MeleeGSWraper(MeleeSequence sequence) => new MeleeGSWraper(sequence);

            public bool ContainsSequence(MeleeSequence meleeSequence) => ContainsSequence(meleeSequence.GetHashCode());
            public bool ContainsSequence(int hashCode)
            {
                if (IsGroup) return false;
                if (IsSequence)
                {
                    if (sequenceInfo.GetHashCode() == hashCode) return true;
                    foreach (var wraps in sequenceInfo.meleeWraps)
                    {
                        if (wraps.ContainsSequence(hashCode)) return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 重新导出动作序列
        /// </summary>
        public void Recalculate()
        {
            resultGroups.Clear();
            foreach (var wrap in meleeWraps)
                resultGroups.AddRange(wrap.GetMeleeGroups());
            counter = 0;
            timer = 0;
            timerMax = 0;
        }
        public void Add(IMeleeAttackData meleeAttackData)
        {
            MeleeGroup groupStab = new MeleeGroup();
            groupStab.meleeAttackDatas.Add(meleeAttackData);
            Add(groupStab);
        }
        public void Add(MeleeGSWraper wraper)
        {
            if (wraper.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            meleeWraps.Add(wraper);
            Recalculate();
        }
        public void Insert(int index, MeleeGSWraper wraper)
        {
            if (wraper.ContainsSequence(this))
            {
                Main.NewText("不可调用自己");
                return;
            }
            meleeWraps.Insert(index, wraper);
            Recalculate();

        }
        public List<MeleeGroup> resultGroups = new List<MeleeGroup>();
        /// <summary>
        /// 请不要对这个列表直接add
        /// </summary>
        public List<MeleeGSWraper> meleeWraps = new List<MeleeGSWraper>();
        public IMeleeAttackData currentData;
        public string SequenceName = "My MeleeSequence";
        public int counter;
        public int timer;
        public int timerMax;
        public bool Attacktive;

        public void Update(Player player, Projectile projectile)
        {
            if (timer <= 0 || currentData == null)
            {
                if (currentData != null) currentData.OnDeactive();
                currentData = resultGroups[counter % resultGroups.Count].GetCurrentMeleeData();
                currentData.OnActive();
                currentData.Player = player;
                currentData.Projectile = projectile;
                timerMax = timer = (int)(player.itemAnimationMax * currentData.ModifyData.actionOffsetSpeed);
                counter++;
            }
            if (currentData != null)
            {
                bool oldValue = Attacktive;
                Attacktive = currentData.Attacktive;
                if (!oldValue && Attacktive)
                {
                    currentData.OnStartAttack();
                }
                if (oldValue && !Attacktive)
                    currentData.OnEndAttack();
                if (Attacktive) currentData.OnAttack();
                else currentData.OnCharge();
                bool flag = true;
                currentData.Update(ref timer, timerMax, ref flag);
                if (flag)
                    timer--;
            }
            player.itemTime = 2;
        }
    }

    //public interface IMeleeAttackData
    //{
    //    //持续时间 角度 位移 修改数据
    //    /// <summary>
    //    /// 近战数据修改
    //    /// </summary>
    //    MeleeModifyData ModifyData => new MeleeModifyData();
    //    /// <summary>
    //    /// 执行次数
    //    /// </summary>
    //    int Cycle => 1;
    //    /// <summary>
    //    /// 中心偏移量，默认零向量
    //    /// </summary>
    //    Vector2 offsetCenter => default;
    //    /// <summary>
    //    /// 原点偏移量，默认为贴图左下角(0.1f,0.9f),取值范围[0,1]
    //    /// </summary>
    //    Vector2 offsetOrigin => new Vector2(.1f, .9f);
    //    /// <summary>
    //    /// 当前周期的进度
    //    /// </summary>
    //    float Factor { get; set; }

    //    /// <summary>
    //    /// 旋转量
    //    /// </summary>
    //    float Rotation { get; }
    //    /// <summary>
    //    /// 大小
    //    /// </summary>
    //    float Size { get; }
    //    /// <summary>
    //    /// 是否具有攻击性
    //    /// </summary>
    //    bool Attacktive { get; }

    //    /// <summary>
    //    /// 被切换时调用,脉冲性
    //    /// </summary>
    //    void OnActive();

    //    /// <summary>
    //    /// 被换走时调用,脉冲性
    //    /// </summary>
    //    void OnDeactive();

    //    /// <summary>
    //    /// 结束时调用,脉冲性
    //    /// </summary>
    //    void OnEndAttack();

    //    /// <summary>
    //    /// 开始攻击时调用,脉冲性
    //    /// </summary>
    //    void OnStartAttack();

    //    /// <summary>
    //    /// 攻击期间调用,持续性
    //    /// </summary>
    //    void OnAttack();

    //    /// <summary>
    //    /// 攻击以外时间调用,持续性
    //    /// </summary>
    //    void OnCharge();
    //    void Update(ref int timer, int timerMax, ref bool canReduceTimer);
    //    //TODO 总有一天要去掉下面这两个b来获取更高的通用度(给npc用之类
    //    Player Player { get; set; }
    //    Projectile Projectile { get; set; }
    //}
    */
    public interface IMeleeAttackData : ISequenceElement
    {
        bool Collide(Rectangle rectangle);

    }
    public class MeleeSequence : SequenceBase<MeleeAction>
    {
        public IReadOnlyList<Group> MeleeGroups => Groups;
    }
    public abstract class MeleeAction : ModType, IMeleeAttackData
    {
        public virtual void NetSend(BinaryWriter writer)
        {

        }
        public virtual void NetReceive(BinaryReader reader)
        {

        }
        public MeleeAction()
        {
            foreach (var n in ModTypeLookup<MeleeAction>.dict.Values)
            {
                if (n.GetType() == this.GetType())
                {
                    this.Mod = n.Mod;
                }
            }
        }
        public Action<MeleeAction> _OnActive;
        public Action<MeleeAction> _OnAttack;
        public Action<MeleeAction> _OnCharge;
        public Action<MeleeAction> _OnDeactive;
        public Action<MeleeAction> _OnEndAttack;
        public Action<MeleeAction> _OnEndSingle;
        public Action<MeleeAction> _OnStartAttack;
        public Action<MeleeAction> _OnStartSingle;

        public SeqDelegateDefinition OnEndAttackDelegate { get; set; } = new SeqDelegateDefinition();

        public SeqDelegateDefinition OnStartAttackDelegate { get; set; } = new SeqDelegateDefinition();

        public SeqDelegateDefinition OnAttackDelegate { get; set; } = new SeqDelegateDefinition();

        public SeqDelegateDefinition OnChargeDelegate { get; set; } = new SeqDelegateDefinition();

        public SeqDelegateDefinition OnActiveDelegate { get; set; } = new SeqDelegateDefinition();

        public SeqDelegateDefinition OnDeactiveDelegate { get; set; } = new SeqDelegateDefinition();

        public SeqDelegateDefinition OnEndSingleDelegate { get; set; } = new SeqDelegateDefinition();

        public SeqDelegateDefinition OnStartSingleDelegate { get; set; } = new SeqDelegateDefinition();
        #region 加载 设置 写入
        public virtual void LoadAttribute(XmlReader xmlReader)
        {
            Cycle = int.Parse(xmlReader["Cycle"]);
            ModifyData = ActionModifyData.LoadFromString(xmlReader["ModifyData"]);

            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.PropertyType.IsAssignableTo(typeof(SeqDelegateDefinition)))
                {
                    var str = xmlReader[prop.Name];
                    if (str != null)
                        prop.SetValue(this, new SeqDelegateDefinition(str));
                }
            }

            //var str = xmlReader["OnActiveDelegate"];
            //if (str != null) 
            //{
            //    OnActiveDelegate = new (str);
            //}

            //str = xmlReader["OnAttackDelegate"];
            //if (str != null)
            //{
            //    OnAttackDelegate = new (str);
            //}

            //str = xmlReader["OnChargeDelegate"];
            //if (str != null)
            //{
            //    OnChargeDelegate = new (str);
            //}

            //str = xmlReader["OnEndAttackDelegate"];
            //if (str != null)
            //{
            //    OnEndAttackDelegate = new (str);
            //}

            //str = xmlReader["OnEndSingleDelegate"];
            //if (str != null)
            //{
            //    OnEndSingleDelegate = new (str);
            //}

            //str = xmlReader["OnStartAttackDelegate"];
            //if (str != null)
            //{
            //    OnStartAttackDelegate = new (str);
            //}

            //str = xmlReader["OnStartSingleDelegate"];
            //if (str != null)
            //{
            //    OnStartSingleDelegate = new (str);
            //}
        }
        public virtual void SaveAttribute(XmlWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Cycle", Cycle.ToString());
            xmlWriter.WriteAttributeString("ModifyData", ModifyData.ToString());


            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.PropertyType.IsAssignableTo(typeof(SeqDelegateDefinition)))
                {
                    SeqDelegateDefinition definition = (SeqDelegateDefinition)prop.GetValue(this, null);
                    if (definition != null && definition.Key != SequenceSystem.NoneDelegateKey)
                        xmlWriter.WriteAttributeString(prop.Name, definition.Key);
                }
            }
            //if (OnEndAttackDelegate != null && OnEndAttackDelegate.Key != SequenceSystem.NoneDelegateKey)
            //{
            //    xmlWriter.WriteAttributeString("OnEndAttackDelegate", OnEndAttackDelegate.Key);
            //}
            //if (OnStartAttackDelegate != null && OnStartAttackDelegate.Key != SequenceSystem.NoneDelegateKey)
            //{
            //    xmlWriter.WriteAttributeString("OnStartAttackDelegate", OnStartAttackDelegate.Key);
            //}
            //if (OnAttackDelegate != null && OnAttackDelegate.Key != SequenceSystem.NoneDelegateKey)
            //{
            //    xmlWriter.WriteAttributeString("OnAttackDelegate", OnAttackDelegate.Key);
            //}
            //if (OnChargeDelegate != null && OnChargeDelegate.Key != SequenceSystem.NoneDelegateKey)
            //{
            //    xmlWriter.WriteAttributeString("OnChargeDelegate", OnChargeDelegate.Key);
            //}
            //if (OnActiveDelegate != null && OnActiveDelegate.Key != SequenceSystem.NoneDelegateKey)
            //{
            //    xmlWriter.WriteAttributeString("OnActiveDelegate", OnActiveDelegate.Key);
            //}
            //if (OnDeactiveDelegate != null && OnDeactiveDelegate.Key != SequenceSystem.NoneDelegateKey)
            //{
            //    xmlWriter.WriteAttributeString("OnDeactiveDelegate", OnDeactiveDelegate.Key);
            //}
            //if (OnEndSingleDelegate != null && OnEndSingleDelegate.Key != SequenceSystem.NoneDelegateKey)
            //{
            //    xmlWriter.WriteAttributeString("OnEndSingleDelegate", OnEndSingleDelegate.Key);
            //}
            //if (OnStartSingleDelegate != null && OnStartSingleDelegate.Key != SequenceSystem.NoneDelegateKey)
            //{
            //    xmlWriter.WriteAttributeString("OnStartSingleDelegate", OnStartSingleDelegate.Key);
            //}
        }
        #endregion

        #region 属性
        #region 编排序列时调整
        //持续时间 角度 位移 修改数据
        /// <summary>
        /// 近战数据修改
        /// </summary>
        [ElementCustomData]
        //[CustomSeqConfigItem(typeof(SeqActionModifyDataElement))]
        public ActionModifyData ModifyData { get; set; } = new ActionModifyData(1);
        /// <summary>
        /// 执行次数
        /// </summary>
        [ElementCustomData]
        [CustomSeqConfigItem(typeof(SeqIntInputElement))]
        public virtual int Cycle { get; set; } = 1;
        #endregion
        #region 动态调整，每次执行时重设
        /// <summary>
        /// 旋转角，非插值
        /// </summary>
        public float Rotation { get; set; }
        /// <summary>
        /// 扁平程度？
        /// </summary>
        public float KValue { get; set; } = 1f;
        public int counter { get; set; }
        public float fTimer;
        public int timer
        {
            get => (int)fTimer; set => fTimer = value;
        }
        public int timerMax { get; set; }
        public bool flip { get; set; }
        #endregion
        #region 插值生成，最主要的实现内容的地方
        /// <summary>
        /// 当前周期的进度
        /// </summary>
        public virtual float Factor => fTimer / timerMax;
        //public virtual float Factor => timer / (float)timerMax;
        /// <summary>
        /// 中心偏移量，默认零向量
        /// </summary>
        public virtual Vector2 offsetCenter => default;
        /// <summary>
        /// 原点偏移量，默认为零向量,取值范围[0,1]
        /// </summary>
        public virtual Vector2 offsetOrigin => default;
        /// <summary>
        /// 旋转量
        /// </summary>
        public virtual float offsetRotation { get; }
        /// <summary>
        /// 大小
        /// </summary>
        public virtual float offsetSize => 1f;
        /// <summary>
        /// 是否具有攻击性
        /// </summary>
        public virtual bool Attacktive { get; }
        #endregion
        #endregion
        #region 函数
        public virtual void OnActive()
        {
            if (OnActiveDelegate != null && OnActiveDelegate.Key != SequenceSystem.NoneDelegateKey)
            {
                SequenceSystem.elementDelegates[OnActiveDelegate.Key].Invoke(this);
            }
            _OnActive?.Invoke(this);
        }

        public virtual void OnAttack()
        {
            if (OnAttackDelegate != null && OnAttackDelegate.Key != SequenceSystem.NoneDelegateKey)
            {
                SequenceSystem.elementDelegates[OnAttackDelegate.Key].Invoke(this);
            }
            _OnAttack?.Invoke(this);
        }

        public virtual void OnCharge()
        {

            if (OnChargeDelegate != null && OnChargeDelegate.Key != SequenceSystem.NoneDelegateKey)
            {
                SequenceSystem.elementDelegates[OnChargeDelegate.Key].Invoke(this);
            }
            _OnCharge?.Invoke(this);
        }

        public virtual void OnDeactive()
        {

            if (OnDeactiveDelegate != null && OnDeactiveDelegate.Key != SequenceSystem.NoneDelegateKey)
            {
                SequenceSystem.elementDelegates[OnDeactiveDelegate.Key].Invoke(this);
            }
            _OnDeactive?.Invoke(this);
        }

        public virtual void OnEndAttack()
        {

            if (OnEndAttackDelegate != null && OnEndAttackDelegate.Key != SequenceSystem.NoneDelegateKey)
            {
                SequenceSystem.elementDelegates[OnEndAttackDelegate.Key].Invoke(this);
            }
            _OnEndAttack?.Invoke(this);
        }

        public virtual void OnEndSingle()
        {

            if (OnEndSingleDelegate != null && OnEndSingleDelegate.Key != SequenceSystem.NoneDelegateKey)
            {
                SequenceSystem.elementDelegates[OnEndSingleDelegate.Key].Invoke(this);
            }
            _OnEndSingle?.Invoke(this);
        }

        public virtual void OnStartAttack()
        {

            if (OnStartAttackDelegate != null && OnStartAttackDelegate.Key != SequenceSystem.NoneDelegateKey)
            {
                SequenceSystem.elementDelegates[OnStartAttackDelegate.Key].Invoke(this);
            }
            _OnStartAttack?.Invoke(this);
        }

        public virtual void OnStartSingle()
        {
            if (OnStartSingleDelegate != null && OnStartSingleDelegate.Key != SequenceSystem.NoneDelegateKey)
            {
                SequenceSystem.elementDelegates[OnStartSingleDelegate.Key].Invoke(this);
            }
            _OnStartSingle?.Invoke(this);
            switch (Owner)
            {
                case Player player:
                    {
                        //SoundEngine.PlaySound(SoundID.Item71);
                        var tarpos = player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition;
                        player.direction = Math.Sign(tarpos.X - player.Center.X);
                        Rotation = (tarpos - Owner.Center).ToRotation();//TODO 给其它实体用的时候也有传入方向的手段
                        break;
                    }

            }
        }

        public virtual void Update(bool triggered)
        {
            timer--;
            switch (Owner)
            {
                case Player player:
                    {
                        player.itemTime = 2;
                        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, CompositeArmRotation);
                        break;
                    }
            }

        }
        public virtual float CompositeArmRotation => targetedVector.ToRotation() - MathHelper.PiOver2;
        /// <summary>
        /// 辅助用的量，指向末端
        /// </summary>
        public Vector2 targetedVector;
        public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {

            Vector2 finalOrigin = offsetOrigin + standardInfo.standardOrigin;
            float finalRotation = offsetRotation + standardInfo.standardRotation;
            #region 好久前的绘制代码，直接搬过来用用试试
            if (Owner == null)
            {
                return;
            }
            Vector2 drawCen = offsetCenter + Owner.Center;
            //if (Owner is Player plr)
            //{
            //    Vector2 adder = new Vector2(-2 * plr.direction, 0);
            //    drawCen += adder;
            //    spriteBatch.Draw(TextureAssets.MagicPixel.Value, Owner.Center + adder - Main.screenPosition, new Rectangle(0, 0, 1, 1), Main.DiscoColor, 0, new Vector2(.5f), 4f, 0, 0);
            //}
            float sc = 1;
            if (Owner is Player plr)
                sc = plr.GetAdjustedItemScale(plr.HeldItem);
            CustomVertexInfo[] c = DrawingMethods.GetItemVertexes(finalOrigin, finalRotation, Rotation, texture, KValue, offsetSize * ModifyData.actionOffsetSize * sc, drawCen, !flip);
            //bool flag = LogSpiralLibraryMod.ModTime / 60 % 2 < 1;
            //Effect ItemEffect = flag ? LogSpiralLibraryMod.ItemEffectEX : LogSpiralLibraryMod.ItemEffect;
            //if (flag)
            //{
            //    spriteBatch.DrawString(FontAssets.MouseText.Value, "好", Owner.Center + new Vector2(-600, 0) - Main.screenPosition, Main.DiscoColor);
            //}
            Effect ItemEffect = LogSpiralLibraryMod.ItemEffectEX;
            if (ItemEffect == null) return;
            SamplerState sampler = SamplerState.AnisotropicWrap;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
            var trans = Main.GameViewMatrix != null ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity;
            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
            Matrix result = model * trans * projection;
            //int _counter = 0;
            //foreach (var pass in ItemEffect.CurrentTechnique.Passes)
            //{

            //    spriteBatch.DrawString(FontAssets.MouseText.Value, pass.Name, new Vector2(200, 200 + _counter * 20), Main.DiscoColor);
            //    _counter++;
            //}
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);
            ItemEffect.Parameters["uTransform"].SetValue(result);
            ItemEffect.Parameters["uTime"].SetValue((float)LogSpiralLibraryMod.ModTime / 60f % 1);
            ItemEffect.Parameters["uItemColor"].SetValue(Lighting.GetColor((Owner.Center / 16).ToPoint().X, (Owner.Center / 16).ToPoint().Y).ToVector4());
            ItemEffect.Parameters["uItemGlowColor"].SetValue(new Color(250, 250, 250, 0).ToVector4());
            Main.graphics.GraphicsDevice.Textures[0] = texture;
            Main.graphics.GraphicsDevice.Textures[1] = LogSpiralLibraryMod.Misc[0].Value;
            Main.graphics.GraphicsDevice.Textures[2] = LogSpiralLibraryMod.BaseTex[15].Value;
            Main.graphics.GraphicsDevice.Textures[3] = standardInfo.standardGlowTexture;

            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[3] = sampler;
            ItemEffect.CurrentTechnique.Passes[2].Apply();
            for (int n = 0; n < c.Length; n++) c[n].Color = standardInfo.standardColor;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, c, 0, 2);
            Main.graphics.GraphicsDevice.RasterizerState = originalState;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);
            #endregion
            targetedVector = c[4].Position - drawCen;
            //if (standardInfo.vertexStandard.scaler > 0)
            //{
            //    targetedVector.Normalize();
            //    targetedVector *= standardInfo.vertexStandard.scaler * offsetSize * ModifyData.actionOffsetSize * sc;
            //}

            #region 显示弹幕碰撞区域
            //spriteBatch.DrawLine(Projectile.Center, targetedVector, Color.Red, 4, true, -Main.screenPosition);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Cyan, 0, new Vector2(.5f), 8, 0, 0);
            #endregion

        }
        public virtual bool Collide(Rectangle rectangle)
        {
            if (Attacktive)
            {
                Projectile.localNPCHitCooldown = Math.Clamp(timerMax / 2, 1, 514);

                int t = timer;
                float sc = 1;
                if (Owner is Player plr)
                    sc = plr.GetAdjustedItemScale(plr.HeldItem);
                for (int n = 0; n < 10; n++)
                {
                    fTimer = t - n * .1f;
                    Vector2 finalOrigin = offsetOrigin + standardInfo.standardOrigin;
                    float finalRotation = offsetRotation + standardInfo.standardRotation;
                    Vector2 drawCen = offsetCenter + Owner.Center;

                    float k = 1f;
                    if (standardInfo.vertexStandard.scaler > 0)
                    {
                        k = standardInfo.vertexStandard.scaler / TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value.Size().Length();
                    }
                    CustomVertexInfo[] c = DrawingMethods.GetItemVertexes(finalOrigin, finalRotation, Rotation, TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value, KValue, offsetSize * ModifyData.actionOffsetSize * sc * k, drawCen, !flip);

                    float point = 0f;
                    Vector2 tar = c[4].Position - drawCen;
                    if (Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), Projectile.Center,
                        tar + Projectile.Center, 48f, ref point))
                    {
                        fTimer = t;
                        return true;
                    }
                }
                fTimer = t;

            }
            return false;
        }
        #endregion
        #region 吃闲饭的
        public Entity Owner { get; set; }
        public Projectile Projectile { get; set; }
        public StandardInfo standardInfo { get; set; }

        public string LocalizationCategory => nameof(MeleeAction);
        public sealed override void Register()
        {
            ModTypeLookup<MeleeAction>.Register(this);
        }
        public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", () => GetType().Name);
        #endregion
    }
    public class SwooshInfo : MeleeAction
    {

        public enum SwooshMode
        {
            Chop,
            Slash,
            //Storm
        }
        public override void NetReceive(BinaryReader reader)
        {
            Rotation = reader.ReadSingle();
            KValue = reader.ReadSingle();
            base.NetReceive(reader);
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Rotation);
            writer.Write(KValue);
            base.NetSend(writer);
        }
        public override void LoadAttribute(XmlReader xmlReader)
        {
            mode = (SwooshMode)int.Parse(xmlReader["mode"] ?? "0");
            base.LoadAttribute(xmlReader);
        }
        public override void SaveAttribute(XmlWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("mode", ((int)mode).ToString());
            base.SaveAttribute(xmlWriter);
        }

        public override float Factor => base.Factor;

        //public virtual bool useTransition => false;
        float TimeToAngle(float t)
        {
            float max = timerMax;
            var fac = t / max;
            if (max > cutTime)
            {
                float tier2 = (max - cutTime) * k;
                float tier1 = tier2 + cutTime;
                if (t > tier1)
                    fac = MathHelper.SmoothStep(mode == SwooshMode.Chop ? 160 / 99f : 1, 1.125f, Utils.GetLerpValue(max, tier1, t, true));
                else if (t < tier2)
                    fac = 0;
                else
                    fac = MathHelper.SmoothStep(0, 1.125f, Utils.GetLerpValue(tier2, tier1, t, true));
            }
            else
            {
                fac = MathHelper.SmoothStep(0, 1.125f, fac);
            }
            fac = flip ? 1 - fac : fac;
            float start = -.75f;
            float end = .625f;
            return MathHelper.Lerp(end, start, fac) * MathHelper.Pi;
        }

        [ElementCustomData]
        [CustomSeqConfigItem(typeof(SeqEnumElement))]
        public SwooshMode mode;
        int cutTime => 8;
        float k => 0.25f;
        public override float offsetRotation => TimeToAngle(fTimer);
        public override float offsetSize => base.offsetSize;

        public override bool Attacktive
        {
            get
            {
                float t = (timerMax - cutTime) * k;
                return fTimer > t && fTimer < t + cutTime;
            }
        }
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
            if (Owner is Player plr)
            {
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));

            }
            base.OnStartAttack();
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            Rotation += Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6);
            KValue = Main.rand.NextFloat(1, 2);
            //if (Projectile.owner == Main.myPlayer)
            //{
            //    Rotation += Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6);
            //    KValue = Main.rand.NextFloat(1, 2);
            //    Projectile.netImportant = true;
            //}
            if (mode == SwooshMode.Slash)
                flip ^= true;
            NewSwoosh();
        }
        public override void OnDeactive()
        {
            if (mode == SwooshMode.Slash)
                flip ^= true;
            base.OnDeactive();
        }
        public override void OnActive()
        {
            //flip = Main.rand.NextBool();
            flip = Owner.direction == -1;

            if (Attacktive)

                base.OnActive();
        }
        UltraSwoosh swoosh;
        UltraSwoosh subSwoosh;
        public void NewSwoosh()
        {
            var verS = standardInfo.vertexStandard;
            if (verS.active)
            {
                UltraSwoosh u = null;
                swoosh = subSwoosh = null;
                var range = mode switch
                {
                    SwooshMode.Chop => (.875f, -1f),
                    SwooshMode.Slash => (.625f, -.75f),
                    //SwooshMode.Storm or _ => (.625f, -.75f)
                };
                bool f = mode switch
                {
                    SwooshMode.Chop => !flip,
                    _ => flip
                };
                float size = verS.scaler * ModifyData.actionOffsetSize * offsetSize;
                var pair = standardInfo.vertexStandard.swooshTexIndex;
                if (standardInfo.itemType == ItemID.TrueExcalibur)
                {
                    u = UltraSwoosh.NewUltraSwoosh(Color.Pink, (int)(verS.timeLeft * 1.2f), size, Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, f, Rotation, KValue, (range.Item1 + 0.125f, range.Item2 - 0.125f), pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                    subSwoosh = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size * .67f, Owner.Center, verS.heatMap, f, Rotation, KValue, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);

                }
                else
                {
                    u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size, Owner.Center, verS.heatMap, f, Rotation, KValue, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                }
                if (verS.renderInfos == null)
                    u.ResetAllRenderInfo();
                else
                {
                    u.ModityAllRenderInfo(verS.renderInfos);
                }
                swoosh = u;
                u.weaponTex = TextureAssets.Item[standardInfo.itemType].Value;
                if (subSwoosh != null)
                    subSwoosh.weaponTex = TextureAssets.Item[standardInfo.itemType].Value;
                //return u;
            }
            //return null;
        }
        public override void OnEndAttack()
        {
            //NewSwoosh();
            base.OnEndAttack();
        }
        void UpdateSwoosh(UltraSwoosh swoosh, (float, float) range)
        {
            if (swoosh == null)
                return;
            //swoosh.timeLeft = standardInfo.vertexStandard.timeLeft;
            //var fac = 0f;
            //switch (mode)
            //{
            //    case SwooshMode.Chop:
            //        {
            //            fac = Utils.GetLerpValue(MathHelper.Lerp(cutTime, timerMax, 1 - k), MathHelper.Lerp(cutTime, timerMax, k), timer, true);
            //            break;
            //        }
            //    default:
            //        {
            //            fac = timer > MathHelper.Lerp(cutTime, timerMax, k) ? 0 : 1;
            //            break;
            //        }
            //}
            var fac = cutTime > timerMax ? 1 : Utils.GetLerpValue(MathHelper.Lerp(cutTime, timerMax, 1 - k), MathHelper.Lerp(cutTime, timerMax, k), timer, true);
            swoosh.timeLeft = (int)MathHelper.Lerp(1, standardInfo.vertexStandard.timeLeft, MathHelper.SmoothStep(0, 1, fac));
            swoosh.center = Owner.Center;
            swoosh.rotation = Rotation;
            swoosh.negativeDir = flip;
            swoosh.angleRange = range;
            if (flip)
                swoosh.angleRange = (swoosh.angleRange.from, -swoosh.angleRange.to);
        }
        public override void Update(bool triggered)
        {
            if (timer > (timerMax - cutTime) * k)
            {
                //UpdateSwoosh(swoosh, (offsetRotation / MathF.PI, mode == SwooshMode.Chop ? 0.625f : -0.75f));
                //UpdateSwoosh(subSwoosh, (offsetRotation / MathF.PI, mode == SwooshMode.Chop ? 0.75f : -0.875f));
                timer--;
                UpdateSwoosh(swoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : -0.75f, offsetRotation / MathF.PI));
                UpdateSwoosh(subSwoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : -0.75f, offsetRotation / MathF.PI));
                timer++;
            }
            else
            {
                swoosh = null;
                subSwoosh = null;

            }
            base.Update(triggered);
        }
        public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            base.Draw(spriteBatch, texture);
        }
        public override void OnAttack()
        {
            for (int n = 0; n < 30 * (1 - Factor) * standardInfo.dustAmount; n++)
            {
                var Center = Owner.Center + offsetCenter + targetedVector * Main.rand.NextFloat(0.5f, 1f);//
                var velocity = -Owner.velocity * 2 + targetedVector.RotatedBy(MathHelper.PiOver2 * (flip ? -1 : 1) + Main.rand.NextFloat(-MathHelper.Pi / 12, MathHelper.Pi / 12)) * Main.rand.NextFloat(.125f, .25f);
                OtherMethods.FastDust(Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), velocity * .25f, standardInfo.standardColor);


            }
            base.OnAttack();
        }
    }
    public class StabInfo : MeleeAction
    {
        public override void NetReceive(BinaryReader reader)
        {
            Rotation = reader.ReadSingle();
            KValue = reader.ReadSingle();
            base.NetReceive(reader);
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Rotation);
            writer.Write(KValue);
            base.NetSend(writer);
        }
        public override Vector2 offsetCenter => default;//new Vector2(64 * Factor, 0).RotatedBy(Rotation);
        public override Vector2 offsetOrigin => new Vector2(Factor * .4f, 0).RotatedBy(standardInfo.standardRotation);
        public override bool Attacktive => timer <= MathF.Sqrt(timerMax);
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.SwooshNormal_1, Owner?.Center);
            if (Owner is Player plr)
            {
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
                for (int n = 0; n < 30; n++)
                {
                    if (Main.rand.NextFloat(0, 1) < standardInfo.dustAmount)
                        for (int k = 0; k < 2; k++)
                        {
                            var flag = k == 0;
                            var unit = ((MathHelper.TwoPi / 30 * n).ToRotationVector2() * new Vector2(1, .75f)).RotatedBy(Rotation) * (flag ? 2 : 1) * .5f;
                            var Center = Owner.Center + offsetCenter + targetedVector * .75f;
                            var velocity = -Owner.velocity * 2 + unit - targetedVector * .125f;
                            velocity *= 2;
                            OtherMethods.FastDust(Center, velocity, standardInfo.standardColor);

                        }
                }
            }
            base.OnStartAttack();
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            KValue = Main.rand.NextFloat(1f, 2.4f);
            Rotation += Main.rand.NextFloat(0, Main.rand.NextFloat(0, MathHelper.Pi / 6)) * Main.rand.Next(new int[] { -1, 1 });
            //if (Projectile.owner == Main.myPlayer)
            //{
            //    KValue = Main.rand.NextFloat(1f, 2.4f);
            //    Rotation += Main.rand.NextFloat(0, Main.rand.NextFloat(0, MathHelper.Pi / 6)) * Main.rand.Next(new int[] { -1, 1 });
            //    Projectile.netImportant = true;
            //}

            flip ^= true;
        }
        public virtual UltraStab NewStab()
        {
            var verS = standardInfo.vertexStandard;
            if (verS.active)
            {
                UltraStab u = null;
                var pair = standardInfo.vertexStandard.stabTexIndex;
                if (standardInfo.itemType == ItemID.TrueExcalibur)
                {
                    float size = verS.scaler * ModifyData.actionOffsetSize * offsetSize * 1.25f;
                    u = UltraStab.NewUltraStab(standardInfo.standardColor, (int)(verS.timeLeft * 1.2f), size,
                    Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, flip, Rotation, 2, pair?.Item1 ?? -3, pair?.Item2 ?? 8, colorVec: verS.colorVec);
                    var su = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, size * .67f,
                    Owner.Center + Rotation.ToRotationVector2() * size * .2f, verS.heatMap, !flip, Rotation, 2, pair?.Item1 ?? -3, pair?.Item2 ?? 8, colorVec: verS.colorVec);
                    su.weaponTex = TextureAssets.Item[standardInfo.itemType].Value;
                }
                else
                {
                    u = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize * 1.25f,
                    Owner.Center, verS.heatMap, flip, Rotation, 2, pair?.Item1 ?? -3, pair?.Item2 ?? 8, colorVec: verS.colorVec);
                }
                //Main.NewText(Owner.Center);
                //var u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize, Owner.Center, verS.heatMap, this.flip, Rotation, KValue, (.625f, -.75f), colorVec: verS.colorVec);
                if (verS.renderInfos == null)
                    u.ResetAllRenderInfo();
                else
                {
                    u.ModityAllRenderInfo(verS.renderInfos);
                }
                u.weaponTex = TextureAssets.Item[standardInfo.itemType].Value;
                return u;
            }
            return null;
        }
        public override void OnEndAttack()
        {
            NewStab();
            //Projectile.NewProjectile(Owner.GetSource_FromThis(), Owner.Center, Rotation.ToRotationVector2() * 16, ProjectileID.TerraBeam, 100, 1, Owner.whoAmI);
            base.OnEndAttack();
        }
        public override float Factor
        {
            get
            {
                float k = MathF.Sqrt(timerMax);
                float max = timerMax;
                float t = timer;
                if (t >= k)
                {
                    return MathHelper.SmoothStep(1, 1.125f, Terraria.Utils.GetLerpValue(max, k, t, true));
                }
                else
                {
                    //return MathHelper.SmoothStep(0, 1.125f, t / k);
                    return MathHelper.Hermite(0, -5, 1.125f, 0, t / k);

                }
                float fac = base.Factor;
                fac = 1 - fac;
                fac *= fac;
                return fac.CosFactor();
            }
        }
    }
    public class RapidlyStabInfo : StabInfo
    {
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((byte)realCycle);
            base.NetSend(writer);
        }
        public override void NetReceive(BinaryReader reader)
        {
            realCycle = reader.ReadByte();
            base.NetReceive(reader);
        }
        public RapidlyStabInfo()
        {

        }
        public (int min, int max) CycleOffsetRange
        {
            get => (rangeOffsetMin, rangeOffsetMax);
            set
            {
                var v = value;
                v.min = Math.Clamp(v.min, 1 - givenCycle, v.max);
                v.max = Math.Clamp(v.max, v.min, int.MaxValue);
                (rangeOffsetMin, rangeOffsetMax) = v;
                ResetCycle();
            }
        }
        [ElementCustomData]
        [CustomSeqConfigItem(typeof(SeqIntInputElement))]
        public int rangeOffsetMin;
        [ElementCustomData]
        [CustomSeqConfigItem(typeof(SeqIntInputElement))]
        public int rangeOffsetMax;
        public override void LoadAttribute(XmlReader xmlReader)
        {
            givenCycle = int.Parse(xmlReader["givenCycle"]);
            ModifyData = ActionModifyData.LoadFromString(xmlReader["ModifyData"]);
            rangeOffsetMin = int.Parse(xmlReader["rangeOffsetMin"]);
            rangeOffsetMax = int.Parse(xmlReader["rangeOffsetMax"]);
        }
        public override void SaveAttribute(XmlWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("givenCycle", givenCycle.ToString());
            xmlWriter.WriteAttributeString("ModifyData", ModifyData.ToString());
            xmlWriter.WriteAttributeString("rangeOffsetMin", rangeOffsetMin.ToString());
            xmlWriter.WriteAttributeString("rangeOffsetMax", rangeOffsetMax.ToString());

        }
        [ElementCustomDataAbabdoned]
        public override int Cycle { get => realCycle; set => givenCycle = value; }
        public int realCycle;
        [ElementCustomData]
        [CustomSeqConfigItem(typeof(SeqIntInputElement))]
        [Range(1, 10)]
        public int givenCycle;
        void ResetCycle()
        {
            realCycle = rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : Math.Clamp(givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);

            //if (Projectile.owner == Main.myPlayer)
            //{
            //    realCycle = rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : Math.Clamp(givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);
            //    Projectile.netImportant = true;
            //}

        }
        public override void OnActive()
        {
            ResetCycle();
            base.OnActive();
        }
    }
    public class ConvoluteInfo : MeleeAction
    {
        public override Vector2 offsetCenter => unit * Factor.CosFactor() * 512;
        public Vector2 unit;
        public override bool Attacktive => Factor >= .25f;
        public override float CompositeArmRotation => Owner.direction;
        public override float offsetRotation => Factor * MathHelper.TwoPi * 2;
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            KValue = 1.5f;
            unit = Rotation.ToRotationVector2();
        }
        public override void OnAttack()
        {
            if ((int)LogSpiralLibraryMod.ModTime2 % 6 == 0)
                SoundEngine.PlaySound(MySoundID.BoomerangRotating, Owner?.Center);

            base.OnAttack();
        }
        public override void OnStartAttack()
        {
            base.OnStartAttack();
        }
    }
    /*public class ShockingDashInfo : MeleeAction
    {
    }*/
}

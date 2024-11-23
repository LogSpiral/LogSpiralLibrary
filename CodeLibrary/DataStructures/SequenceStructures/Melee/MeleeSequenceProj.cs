using LogSpiralLibrary.CodeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Melee
{
    /// <summary>
    /// 来把基剑
    /// </summary>
    public abstract class MeleeSequenceProj : ModProjectile
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
        public static MeleeSequence LocalMeleeSequence;
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
                if (LocalMeleeSequence == null)
                {
                    LocalMeleeSequence = new MeleeSequence();
                    MeleeSequence.Load((GetType().Namespace.Replace(Mod.Name + ".", "") + "." + Name).Replace('.', '/') + ".xml", Mod, LocalMeleeSequence);
                }
                meleeSequence = (MeleeSequence)LocalMeleeSequence.Clone();
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

        //你只需要知道上面这一段负责 *记录* 我们弹幕具体是怎么运行

        //弹幕发射和弹幕加载时的初始化，这部分和别的弹幕大差不差
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


        //这里算是这个弹幕的核心部分
        //分别是标准参数
        //目前执行组件
        //更新函数
        public virtual StandardInfo StandardInfo => new StandardInfo(-MathHelper.PiOver4, new Vector2(0.1f, 0.9f), player.itemAnimationMax, Color.White, null, ItemID.IronBroadsword);
        public MeleeAction currentData => meleeSequence.currentData;
        public override void AI()
        {
            //这里是手持弹幕的一些常规检测和赋值
            player.heldProj = Projectile.whoAmI;
            Projectile.damage = player.GetWeaponDamage(player.HeldItem);
            Projectile.direction = player.direction;
            Projectile.velocity = (player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition - player.Center).SafeNormalize(default);
            if (meleeSequence == null)//这里确保弹幕的执行序列加载到了
            {
                InitializeSequence(Mod.Name, Name);
                meleeSequence.SetOwner(player);
            }
            if (player.DeadOrGhost)
            {
                currentData?.OnDeactive();
                Projectile.Kill();
            }
            if (Projectile.timeLeft == 10) return;

            //这里就是由弹幕检测玩家是否符合执行条件，符合就更新状态
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
                if (flag1 || (currentData.counter < currentData.Cycle || currentData.counter == currentData.Cycle && currentData.timer > 0) && !meleeSequence.currentWrapper.finished)
                    Projectile.timeLeft = 2;

                meleeSequence.Update(player, Projectile, StandardInfo, flag1);
            }
            if (currentData == null) return;

            //依旧是常规赋值，但是要中间那段执行正常才应当执行
            Projectile.Center = player.Center + currentData.offsetCenter;
            if (player.itemAnimation < 2)
                player.itemAnimation = 2;
            if (player.itemTime < 2)
                player.itemTime = 2;
            base.AI();
        }

        //辅助更新函数使用的属性
        public Player player => Main.player[Projectile.owner];


        //下面这里实现手持弹幕的一些比较细枝末节的东西，像是绘制 攻击到目标的伤害修正之类
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
        public override void OnKill(int timeLeft)
        {
            //if (Main.netMode != NetmodeID.Server)
            meleeSequence?.ResetCounter();
            base.OnKill(timeLeft);
        }
        //还有这个是阻止弹幕自行更新位置的，因为我们会在核心逻辑那里写入弹幕的位置
        public override bool ShouldUpdatePosition()
        {
            return false;
        }


        //这两个本来是做联机同步用的，但是后来发现我这里用不上
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
    }
}

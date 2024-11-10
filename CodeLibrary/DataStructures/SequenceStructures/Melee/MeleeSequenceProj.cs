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
            if (player.DeadOrGhost) 
            {
                currentData?.OnDeactive();
                Projectile.Kill();
            }
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
                if (flag1 || (currentData.counter < currentData.Cycle || currentData.counter == currentData.Cycle && currentData.timer > 0) && !meleeSequence.currentWrapper.finished)
                    Projectile.timeLeft = 2;

                meleeSequence.Update(player, Projectile, StandardInfo, flag1);
            }
            if (currentData == null) return;


            Projectile.Center = player.Center + currentData.offsetCenter;
            if (player.itemAnimation < 2)
                player.itemAnimation = 2;
            if(player.itemTime < 2)
                player.itemTime = 2;
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
}

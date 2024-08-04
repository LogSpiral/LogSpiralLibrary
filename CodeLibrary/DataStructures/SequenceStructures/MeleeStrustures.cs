using LogSpiralLibrary.CodeLibrary;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.WorldBuilding;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{


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
        #region 加载 设置 写入
        public virtual void LoadAttribute(XmlReader xmlReader)
        {
            Cycle = int.Parse(xmlReader["Cycle"]);
            ModifyData = ActionModifyData.LoadFromString(xmlReader["ModifyData"]);
        }
        //public virtual void SetConfigPanel(UIList parent)
        //{
        //    IntRangeElement cycleElement = new IntRangeElement();
        //    cycleElement.Bind(new PropertyFieldWrapper(this.GetType().GetProperty("Cycle")), this, null, -1);
        //    parent.Add(cycleElement);
        //    Main.NewText(cycleElement.GetType().Name);
        //}
        public virtual void SaveAttribute(XmlWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("Cycle", Cycle.ToString());
            xmlWriter.WriteAttributeString("ModifyData", ModifyData.ToString());
        }
        #endregion

        #region 属性
        #region 编排序列时调整
        //持续时间 角度 位移 修改数据
        /// <summary>
        /// 近战数据修改
        /// </summary>
        [ElementCustomData]
        [CustomModConfigItem(typeof(ActionModifyDataElement))]
        public ActionModifyData ModifyData { get; set; } = new ActionModifyData(1);
        /// <summary>
        /// 执行次数
        /// </summary>
        [ElementCustomData]
        [CustomModConfigItem(typeof(SeqIntInputElement))]
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
        public int timer { get; set; }
        public int timerMax { get; set; }
        public bool flip { get; set; }
        #endregion
        #region 插值生成，最主要的实现内容的地方
        /// <summary>
        /// 当前周期的进度
        /// </summary>
        public virtual float Factor => timer / (float)timerMax;
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

            _OnActive?.Invoke(this);
        }

        public virtual void OnAttack()
        {
            _OnAttack?.Invoke(this);
        }

        public virtual void OnCharge()
        {
            _OnCharge?.Invoke(this);
        }

        public virtual void OnDeactive()
        {
            _OnDeactive?.Invoke(this);
        }

        public virtual void OnEndAttack()
        {
            _OnEndAttack?.Invoke(this);
        }

        public virtual void OnEndSingle()
        {
            _OnEndSingle?.Invoke(this);
        }

        public virtual void OnStartAttack()
        {
            _OnStartAttack?.Invoke(this);
        }

        public virtual void OnStartSingle()
        {
            _OnStartSingle?.Invoke(this);
            switch (Owner)
            {
                case Player player:
                    {
                        //SoundEngine.PlaySound(SoundID.Item71);
                        player.direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
                        Rotation = (Main.MouseWorld - Owner.Center).ToRotation();//TODO 给其它实体用的时候也有传入方向的手段
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
            //spriteBatch.Draw(LogSpiralLibraryMod.Misc[24].Value, new Vector2(0,0), Color.White);
            Vector2 finalOrigin = offsetOrigin + standardInfo.standardOrigin;
            float finalRotation = offsetRotation + standardInfo.standardRotation;
            #region 好久前的绘制代码，直接搬过来用用试试
            if (Owner == null)
            {
                return;
            }
            Vector2 drawCen = offsetCenter + Owner.Center;
            CustomVertexInfo[] c = DrawingMethods.GetItemVertexes(finalOrigin, finalRotation, Rotation, texture, KValue, offsetSize * ModifyData.actionOffsetSize, drawCen, flip);
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
            #region 显示弹幕碰撞区域
            //spriteBatch.DrawLine(Projectile.Center, targetedVector, Color.Red, 4, true, -Main.screenPosition);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Cyan, 0, new Vector2(.5f), 8, 0, 0);
            #endregion

        }
        public virtual bool Collide(Rectangle rectangle)
        {
            if (Attacktive)
            {
                float point = 0f;

                return Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), Projectile.Center,
                    Projectile.Center + targetedVector, 48f, ref point);
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

        public override float Factor => base.Factor;

        public override float offsetRotation
        {
            get
            {
                var fac = Factor;
                int TimeToCutThem = 8;
                if (timerMax > TimeToCutThem)
                {
                    float k = TimeToCutThem;
                    float max = timerMax;
                    float t = timer;
                    float v = 0.1f;
                    float tier2 = (max - k) * v;
                    float tier1 = MathHelper.Lerp(max, k, 1 - v);
                    if (false)//(negativeDir == oldNegativeDir && swingCount > 0) && player.itemAnimation > tier1
                    {
                        fac = MathHelper.SmoothStep(160 / 99f, 1.125f, Utils.GetLerpValue(max, tier1, t, true));
                    }
                    else
                    {
                        if (t > tier1)
                            fac = MathHelper.SmoothStep(1, 1.125f, Utils.GetLerpValue(max, tier1, t, true));
                        else if (t < tier2)
                            fac = 0;
                        else
                            fac = MathHelper.SmoothStep(0, 1.125f, Utils.GetLerpValue(tier2, tier1, t, true));
                    }
                }
                else
                {
                    fac = MathHelper.SmoothStep(0, 1.125f, fac);

                }
                fac = flip ? 1 - fac : fac;
                float start = -.75f;
                if (Main.gamePaused) start += Main.rand.NextFloat(-.05f, .05f);
                float end = .625f;
                return MathHelper.Lerp(end, start, fac) * MathHelper.Pi;
            }
        }

        public override float offsetSize => base.offsetSize;

        public override bool Attacktive => timer < 8;
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(MySoundID.Scythe);
            base.OnStartAttack();
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            Rotation += Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6);
            flip ^= true;
            KValue = Main.rand.NextFloat(1, 2);
        }
        public override void OnDeactive()
        {
            flip ^= true;
            base.OnDeactive();
        }
        public override void OnActive()
        {
            flip = Main.rand.NextBool();
            base.OnActive();
        }
        public virtual UltraSwoosh NewSwoosh()
        {
            var verS = standardInfo.vertexStandard;
            if (verS.active)
            {
                var u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize, Owner.Center, verS.heatMap, this.flip, Rotation, KValue, (.625f, -.75f), colorVec: verS.colorVec);
                if (verS.renderInfos == null)
                    u.ResetAllRenderInfo();
                else
                {
                    u.ModityAllRenderInfo(verS.renderInfos);
                }
                return u;
            }
            return null;
        }
        public override void OnEndAttack()
        {
            NewSwoosh();
            base.OnEndAttack();
        }
    }
    public class StabInfo : MeleeAction
    {
        public override Vector2 offsetCenter => default;//new Vector2(64 * Factor, 0).RotatedBy(Rotation);
        public override Vector2 offsetOrigin => new Vector2(Factor * .4f, 0).RotatedBy(standardInfo.standardRotation);
        public override bool Attacktive => timer <= MathF.Sqrt(timerMax);
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(MySoundID.SwooshNormal_1);
            base.OnStartAttack();
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            KValue = Main.rand.NextFloat(1f, 2.4f);
            Rotation += Main.rand.NextFloat(0, Main.rand.NextFloat(0, MathHelper.Pi / 6)) * Main.rand.Next(new int[] { -1, 1 });
            flip ^= true;
        }
        public virtual UltraStab NewStab()
        {
            var verS = standardInfo.vertexStandard;
            if (verS.active)
            {
                var u = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize,
                    Owner.Center, verS.heatMap, flip, Rotation, 2, -3, 8, colorVec: verS.colorVec);
                //Main.NewText(Owner.Center);
                //var u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize, Owner.Center, verS.heatMap, this.flip, Rotation, KValue, (.625f, -.75f), colorVec: verS.colorVec);
                if (verS.renderInfos == null)
                    u.ResetAllRenderInfo();
                else
                {
                    u.ModityAllRenderInfo(verS.renderInfos);
                }
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
        [CustomModConfigItem(typeof(SeqIntInputElement))]
        public int rangeOffsetMin;
        [ElementCustomData]
        [CustomModConfigItem(typeof(SeqIntInputElement))]
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
        [CustomModConfigItem(typeof(SeqIntInputElement))]
        public int givenCycle;
        void ResetCycle()
        {
            realCycle = rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : Math.Clamp(givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);
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
                SoundEngine.PlaySound(MySoundID.BoomerangRotating);

            base.OnAttack();
        }
        public override void OnStartAttack()
        {
            base.OnStartAttack();
        }
    }
    public class ShockingDashInfo : MeleeAction
    {
    }
}

using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
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
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using static Terraria.NPC.NPCNameFakeLanguageCategoryPassthrough;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Melee
{

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class SequenceDelegateAttribute : Attribute
    {
        public SequenceDelegateAttribute()
        {
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
    //public interface IMeleeAttackData : ISequenceElement
    //{
    //    bool Collide(Rectangle rectangle);

    //}
    //public class MeleeSequence : Sequence<MeleeAction>
    //{
    //    public IReadOnlyList<Group> MeleeGroups => Groups;
    //}
    public abstract class MeleeAction : ModType, ISequenceElement
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
                if (n.GetType() == GetType())
                {
                    Mod = n.Mod;
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

            var props = GetType().GetProperties();
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


            var props = GetType().GetProperties();
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
        //[CustomSeqConfigItem(typeof(SeqIntInputElement))]
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
        public virtual CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            Vector2 finalOrigin = offsetOrigin + standardInfo.standardOrigin;
            float finalRotation = offsetRotation + standardInfo.standardRotation;
            Vector2 drawCen = offsetCenter + Owner.Center;
            float sc = 1;
            if (Owner is Player plr)
                sc = plr.GetAdjustedItemScale(plr.HeldItem);
            return DrawingMethods.GetItemVertexes(finalOrigin, finalRotation, Rotation, texture, KValue, offsetSize * ModifyData.actionOffsetSize * sc, drawCen, !flip, alpha, standardInfo.frame);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            #region 好久前的绘制代码，直接搬过来用用试试
            if (Owner == null)
            {
                return;
            }
            //List<CustomVertexInfo> vertexInfos = new List<CustomVertexInfo>();
            //float origFTimer = fTimer;
            //for (int n = 0; n < 10; n++) 
            //{
            //    fTimer += .1f;
            //    vertexInfos.AddRange(CustomVertexInfos(texture, 1 - n / 10f));
            //}
            //fTimer = origFTimer;
            //CustomVertexInfo[] c = vertexInfos.ToArray();
            CustomVertexInfo[] c = GetWeaponVertex(texture, 1f);
            Effect ItemEffect = LogSpiralLibraryMod.ItemEffectEX;
            if (ItemEffect == null) return;
            SamplerState sampler = SamplerState.AnisotropicWrap;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
            var trans = Main.GameViewMatrix != null ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity;
            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
            Matrix result = model * trans * projection;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);
            ItemEffect.Parameters["uTransform"].SetValue(result);
            ItemEffect.Parameters["uTime"].SetValue((float)LogSpiralLibraryMod.ModTime / 60f % 1);
            ItemEffect.Parameters["uItemColor"].SetValue(Lighting.GetColor((Owner.Center + offsetCenter).ToTileCoordinates()).ToVector4());
            ItemEffect.Parameters["uItemGlowColor"].SetValue(Vector4.One);
            if (standardInfo.frame != null)
            {
                Rectangle frame = standardInfo.frame.Value;
                Vector2 size = texture.Size();
                ItemEffect.Parameters["uItemFrame"].SetValue(new Vector4(frame.TopLeft() / size, frame.Width / size.X, frame.Height / size.Y));
            }
            else
                ItemEffect.Parameters["uItemFrame"].SetValue(new Vector4(0, 0, 1, 1));
            Main.graphics.GraphicsDevice.Textures[0] = texture;
            Main.graphics.GraphicsDevice.Textures[1] = LogSpiralLibraryMod.Misc[0].Value;
            Main.graphics.GraphicsDevice.Textures[2] = LogSpiralLibraryMod.BaseTex[15].Value;
            Main.graphics.GraphicsDevice.Textures[3] = standardInfo.standardGlowTexture;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
            Main.graphics.GraphicsDevice.SamplerStates[3] = sampler;
            ItemEffect.CurrentTechnique.Passes[0].Apply();
            for (int n = 0; n < c.Length; n++) c[n].Color = standardInfo.standardColor * standardInfo.extraLight;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, c, 0, c.Length / 3);
            Main.graphics.GraphicsDevice.RasterizerState = originalState;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);
            #endregion
            targetedVector = c[4].Position - (offsetCenter + Owner.Center);
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


                /*float point1 = 0f;
                return Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), Projectile.Center,
                        targetedVector + Projectile.Center, 48f, ref point1);*/
                float t = fTimer;
                float sc = 1;
                if (Owner is Player plr)
                    sc = plr.GetAdjustedItemScale(plr.HeldItem);
                for (int n = 0; n < 5; n++)
                {
                    fTimer = t + n * .2f;
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
                    if (Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), c[0].Position,
                        c[4].Position, 48f, ref point))
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

}

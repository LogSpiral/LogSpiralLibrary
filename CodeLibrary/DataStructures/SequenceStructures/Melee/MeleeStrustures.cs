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

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Melee
{

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class SequenceDelegateAttribute : Attribute
    {
        public SequenceDelegateAttribute()
        {
        }
    }
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

        [ElementCustomData]
        public SeqDelegateDefinition OnEndAttackDelegate { get; set; } = new SeqDelegateDefinition();

        [ElementCustomData]
        public SeqDelegateDefinition OnStartAttackDelegate { get; set; } = new SeqDelegateDefinition();

        [ElementCustomData]
        public SeqDelegateDefinition OnAttackDelegate { get; set; } = new SeqDelegateDefinition();

        [ElementCustomData]
        public SeqDelegateDefinition OnChargeDelegate { get; set; } = new SeqDelegateDefinition();

        [ElementCustomData]
        public SeqDelegateDefinition OnActiveDelegate { get; set; } = new SeqDelegateDefinition();

        [ElementCustomData]
        public SeqDelegateDefinition OnDeactiveDelegate { get; set; } = new SeqDelegateDefinition();

        [ElementCustomData]
        public SeqDelegateDefinition OnEndSingleDelegate { get; set; } = new SeqDelegateDefinition();

        [ElementCustomData]
        public SeqDelegateDefinition OnStartSingleDelegate { get; set; } = new SeqDelegateDefinition();

        [ElementCustomData]
        public SeqDelegateDefinition OnHitTargetDelegate { get; set; } = new SeqDelegateDefinition();

        #region 加载 设置 写入
        public virtual void LoadAttribute(XmlReader xmlReader)
        {
            //Cycle = int.Parse(xmlReader["Cycle"]);
            //ModifyData = ActionModifyData.LoadFromString(xmlReader["ModifyData"]);
            var type = GetType();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<ElementCustomDataAttribute>() != null && prop.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null && xmlReader[prop.Name] is string content && content.Length != 0)
                {
                    object dummy = prop.GetValue(this);
                    object value = dummy switch
                    {
                        int => int.Parse(content),
                        float => float.Parse(content),
                        double => double.Parse(content),
                        bool => bool.Parse(content),
                        byte => byte.Parse(content),
                        ActionModifyData => ActionModifyData.LoadFromString(content),
                        SeqDelegateDefinition => new SeqDelegateDefinition(content),
                        _ => null
                    };
                    if (value == null && prop.PropertyType.IsEnum)
                        value = Enum.GetValues(prop.PropertyType).GetValue(int.Parse(content));
                    if (value != null)
                        prop.SetValue(this, value);
                }
            }
            foreach (var fld in type.GetFields())
            {
                if (fld.GetCustomAttribute<ElementCustomDataAttribute>() != null && fld.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null && xmlReader[fld.Name] is string content && content.Length != 0)
                {
                    object dummy = fld.GetValue(this);

                    object value = dummy switch
                    {
                        int => int.Parse(content),
                        float => float.Parse(content),
                        double => double.Parse(content),
                        bool => bool.Parse(content),
                        byte => byte.Parse(content),
                        ActionModifyData => ActionModifyData.LoadFromString(content),
                        SeqDelegateDefinition => new SeqDelegateDefinition(content),
                        _ => null
                    };
                    if (value == null && fld.FieldType.IsEnum)
                        value = Enum.GetValues(fld.FieldType).GetValue(int.Parse(content));
                    if (value != null)
                        fld.SetValue(this, value);
                }
            }
        }
        public virtual void SaveAttribute(XmlWriter xmlWriter)
        {
            var props = GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<ElementCustomDataAttribute>() != null && prop.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null)
                {
                    object dummy = prop.GetValue(this);

                    if (prop.PropertyType.IsEnum)
                        dummy = (int)dummy;
                    string content = dummy switch
                    {
                        float f => f != 0 ? f.ToString("0.00") : null,
                        double d => d != 0 ? d.ToString("0.00") : null,
                        SeqDelegateDefinition definition => definition.Key != SequenceSystem.NoneDelegateKey ? definition.Key : null,
                        _ => dummy.ToString()
                    };
                    if (content != null)
                        if (content.Length == 0)
                        {
                            int u = 0;
                        }
                        else
                            xmlWriter.WriteAttributeString(prop.Name, content);
                }
            }
            foreach (var fld in GetType().GetFields())
            {
                if (fld.GetCustomAttribute<ElementCustomDataAttribute>() != null && fld.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() == null)
                {
                    object dummy = fld.GetValue(this);

                    if (fld.FieldType.IsEnum)
                        dummy = (int)dummy;
                    string content = dummy switch
                    {
                        float f => f != 0 ? f.ToString("0.00") : null,
                        double d => d != 0 ? d.ToString("0.00") : null,
                        SeqDelegateDefinition definition => definition.Key != SequenceSystem.NoneDelegateKey ? definition.Key : null,
                        _ => dummy.ToString()
                    };
                    if (content != null)
                        if (content.Length == 0)
                        {
                            int u = 0;
                        }
                        else
                            xmlWriter.WriteAttributeString(fld.Name, content);
                }
            }
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

        /// <summary>
        /// 伤害
        /// </summary>
        public virtual float offsetDamage => 1f;
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
            {
                sc = plr.GetAdjustedItemScale(plr.HeldItem);
                drawCen += plr.gfxOffY * Vector2.UnitY;
            }
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
        public virtual void OnHitEntity(Entity victim, int damageDone, object[] context)
        {
            if (OnHitTargetDelegate != null && OnHitTargetDelegate.Key != SequenceSystem.NoneDelegateKey)
            {
                SequenceSystem.elementDelegates[OnHitTargetDelegate.Key].Invoke(this);
            }
            if (Owner is Player player)
                damageDone /= MathHelper.Clamp(player.GetWeaponDamage(player.HeldItem), 1, int.MaxValue);
            float delta = Main.rand.NextFloat(0.85f, 1.15f) * MathF.Log(damageDone + 1);
            if (Main.LocalPlayer.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake < 4f)
                Main.LocalPlayer.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake += delta;//

            for (int n = 0; n < 30 * delta * (standardInfo.dustAmount + .2f); n++)
                OtherMethods.FastDust(victim.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), Main.rand.NextVector2Unit() * Main.rand.NextFloat(Main.rand.NextFloat(0, 8), 16), standardInfo.standardColor);


        }
        #endregion
        #region 吃闲饭的
        public Entity Owner { get; set; }
        public Projectile Projectile { get; set; }
        public StandardInfo standardInfo { get; set; }
        public int CurrentDamage => Owner is Player plr ? (int)(plr.GetWeaponDamage(plr.HeldItem) * ModifyData.actionOffsetDamage * offsetDamage) : Projectile.damage;
        public string LocalizationCategory => nameof(MeleeAction);
        public sealed override void Register()
        {
            ModTypeLookup<MeleeAction>.Register(this);
        }
        public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", () => GetType().Name);
        public abstract string Category { get; }
        #endregion
    }

}

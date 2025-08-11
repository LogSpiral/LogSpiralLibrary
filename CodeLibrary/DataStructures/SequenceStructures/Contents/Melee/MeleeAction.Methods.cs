using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

partial class MeleeAction
{
    public MeleeAction()
    {
        var type = GetType();
        var dummy = ModTypeLookup<MeleeAction>.dict.Values.FirstOrDefault(element => element.GetType() == type);
        if (dummy != null)
            Mod = dummy.Mod;
        //else if (SequenceManager<MeleeAction>._loaded)
        //    LogSpiralLibraryMod.Instance.Logger.Error($"Instance of the element:{type.FullName} not found.");
    }

    public void Update() 
    {
        if (Timer <= 0)//计时器小于等于0时
        {
            if (Counter < CounterMax || CounterMax == 0)//如果没执行完所有次数
            {
                // Owner = entity;
                // Projectile = projectile; // 外部赋值
                // standardInfo = standardInfo; // 外部赋值
                if (Counter == 0)//标志着刚切换上
                    OnActive();
                else OnEndSingle();
                OnStartSingle();
                var result = (int)(StandardInfo.standardTimer * ModifyData.actionOffsetTimeScaler / CounterMax);
                TimerMax = Timer = result;
                Counter++;
                if (Attacktive)
                    OnStartAttack();
            }
            //迁移至下方
            else
            {
                OnEndSingle();
                OnDeactive();
                OnEndAttack();
            }
        }

        //bool oldValue = Attacktive;
        //Attacktive = Attacktive;
        //if (!oldValue && Attacktive)
        //{
        //    OnStartAttack();//TODO Attack相关钩子合理化挂载位置
        //}
        //if (oldValue && !Attacktive)
        //{
        //    OnEndAttack();
        //}
        if (Attacktive) OnAttack();
        else OnCharge();
        Update(false);
    }

    public virtual void Update(bool triggered)
    {
        Timer--;
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

    public virtual void Initialize() {
        Counter = 0;
    }

    public virtual void OnActive()
    {
        if (OnActiveDelegate != null && OnActiveDelegate.Key != SequenceSystem.NoneDelegateKey)
        {
            SequenceSystem.elementDelegates[OnActiveDelegate.Key].Invoke(this);
        }
        _OnActive?.Invoke(this);
        Projectile.ownerHitCheck = OwnerHitCheek;
    }

    public virtual void OnDeactive()
    {
        if (OnDeactiveDelegate != null && OnDeactiveDelegate.Key != SequenceSystem.NoneDelegateKey)
        {
            SequenceSystem.elementDelegates[OnDeactiveDelegate.Key].Invoke(this);
        }
        _OnDeactive?.Invoke(this);
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

    public virtual void OnEndSingle()
    {

        if (OnEndSingleDelegate != null && OnEndSingleDelegate.Key != SequenceSystem.NoneDelegateKey)
        {
            SequenceSystem.elementDelegates[OnEndSingleDelegate.Key].Invoke(this);
        }
        _OnEndSingle?.Invoke(this);
    }

    public virtual void OnCharge()
    {

        if (OnChargeDelegate != null && OnChargeDelegate.Key != SequenceSystem.NoneDelegateKey)
        {
            SequenceSystem.elementDelegates[OnChargeDelegate.Key].Invoke(this);
        }
        _OnCharge?.Invoke(this);
    }

    public virtual void OnStartAttack()
    {

        if (OnStartAttackDelegate != null && OnStartAttackDelegate.Key != SequenceSystem.NoneDelegateKey)
        {
            SequenceSystem.elementDelegates[OnStartAttackDelegate.Key].Invoke(this);
        }
        _OnStartAttack?.Invoke(this);
    }

    public virtual void OnAttack()
    {
        if (OnAttackDelegate != null && OnAttackDelegate.Key != SequenceSystem.NoneDelegateKey)
        {
            SequenceSystem.elementDelegates[OnAttackDelegate.Key].Invoke(this);
        }
        _OnAttack?.Invoke(this);
    }

    public virtual void OnEndAttack()
    {

        if (OnEndAttackDelegate != null && OnEndAttackDelegate.Key != SequenceSystem.NoneDelegateKey)
        {
            SequenceSystem.elementDelegates[OnEndAttackDelegate.Key].Invoke(this);
        }
        _OnEndAttack?.Invoke(this);
    }

    public virtual bool Collide(Rectangle rectangle)
    {
        if (!Attacktive) return false;
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
            Vector2 finalOrigin = offsetOrigin + StandardInfo.standardOrigin;
            float finalRotation = offsetRotation + StandardInfo.standardRotation;
            Vector2 drawCen = offsetCenter + Owner.Center;

            float k = 1f;
            if (StandardInfo.VertexStandard.scaler > 0)
            {
                k = StandardInfo.VertexStandard.scaler / TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value.Size().Length();
            }
            CustomVertexInfo[] c = DrawingMethods.GetItemVertexes(finalOrigin, StandardInfo.standardRotation, offsetRotation, Rotation, TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value, KValue, offsetSize * ModifyData.actionOffsetSize * sc * k, drawCen, !Flip);

            float point = 0f;
            //Vector2 tar = c[4].Position - drawCen;
            if (Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), c[0].Position,
                c[4].Position, 48f, ref point))
            {
                fTimer = t;
                return true;
            }
        }
        fTimer = t;

        return false;
    }

    public virtual void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        Projectile.localNPCHitCooldown = Math.Clamp(TimerMax / 2, 1, 514);
        if (OnHitTargetDelegate != null && OnHitTargetDelegate.Key != SequenceSystem.NoneDelegateKey)
        {
            SequenceSystem.elementDelegates[OnHitTargetDelegate.Key].Invoke(this);
        }
        if (Owner is Player player)
            damageDone /= MathHelper.Clamp(player.GetWeaponDamage(player.HeldItem), 1, int.MaxValue);
        float delta = Main.rand.NextFloat(0.85f, 1.15f) * MathF.Log(damageDone + 1);
        if (Main.LocalPlayer.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake < 4f)
            Main.LocalPlayer.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake += delta;//

        for (int n = 0; n < 30 * delta * (StandardInfo.dustAmount + .2f); n++)
            MiscMethods.FastDust(victim.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), Main.rand.NextVector2Unit() * Main.rand.NextFloat(Main.rand.NextFloat(0, 8), 16), StandardInfo.standardColor);


    }

    /// <summary>
    /// 辅助用的量，指向末端
    /// </summary>
    public Vector2 targetedVector;
    public virtual CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        Vector2 finalOrigin = offsetOrigin + StandardInfo.standardOrigin;
        //float finalRotation = offsetRotation + standardInfo.standardRotation;
        Vector2 drawCen = offsetCenter + Owner.Center;
        float sc = 1;
        if (Owner is Player plr)
        {
            sc = plr.GetAdjustedItemScale(plr.HeldItem);
            drawCen += plr.gfxOffY * Vector2.UnitY;
        }
        return DrawingMethods.GetItemVertexes(finalOrigin, StandardInfo.standardRotation, offsetRotation, Rotation, texture, KValue, offsetSize * ModifyData.actionOffsetSize * sc, drawCen, Flip, alpha, StandardInfo.frame);
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
        if (StandardInfo.frame != null)
        {
            Rectangle frame = StandardInfo.frame.Value;
            Vector2 size = texture.Size();
            ItemEffect.Parameters["uItemFrame"].SetValue(new Vector4(frame.TopLeft() / size, frame.Width / size.X, frame.Height / size.Y));
        }
        else
            ItemEffect.Parameters["uItemFrame"].SetValue(new Vector4(0, 0, 1, 1));
        Main.graphics.GraphicsDevice.Textures[0] = texture;
        Main.graphics.GraphicsDevice.Textures[1] = LogSpiralLibraryMod.Misc[0].Value;
        Main.graphics.GraphicsDevice.Textures[2] = LogSpiralLibraryMod.BaseTex[15].Value;
        Main.graphics.GraphicsDevice.Textures[3] = StandardInfo.standardGlowTexture;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
        Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
        Main.graphics.GraphicsDevice.SamplerStates[3] = sampler;
        ItemEffect.CurrentTechnique.Passes[0].Apply();
        for (int n = 0; n < c.Length; n++) c[n].Color = StandardInfo.standardColor * StandardInfo.extraLight;
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
    // TODO: 修改默认判定逻辑，现在这样还是太暴力了


    public sealed override void Register()
    {
        ModTypeLookup<MeleeAction>.Register(this);
        Language.GetOrRegister(this.GetLocalizationKey("DisplayName"), () => GetType().Name);
        var type = GetType();
        SequenceGlobalManager.ElementTypeLookup[FullName] = type;
        foreach (var fld in type.GetFields())
        {
            if (type != fld.DeclaringType || fld.GetCustomAttribute<ElementCustomDataAttribute>() == null || fld.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() != null)
                continue;
            Language.GetOrRegister(this.GetLocalizationKey(fld.Name + ".Label").Replace(nameof(MeleeAction), "Configs"), () => fld.Name);

        }
        foreach (var property in type.GetProperties())
        {
            if (type != property.DeclaringType || property.GetCustomAttribute<ElementCustomDataAttribute>() == null || property.GetCustomAttribute<ElementCustomDataAbabdonedAttribute>() != null)
                continue;
            Language.GetOrRegister(this.GetLocalizationKey(property.Name + ".Label").Replace(nameof(MeleeAction), "Configs"), () => property.Name);
        }
        var categoryKey = $"Mods.{Mod.Name}.{LocalizationCategory}.Category.{Category}";
        //if (!Language.Exists(categoryKey))

        if (!string.IsNullOrEmpty(Category))
            Language.GetOrRegister(categoryKey, () => Category);
    }

}

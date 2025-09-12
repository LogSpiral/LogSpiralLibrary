using System.IO;

namespace LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;

public abstract class HeldProjectile : ModProjectile, IChannelProj
{
    public Player Player => Main.player[Projectile.owner];

    public virtual void OnCharging(bool left, bool right)
    { }

    public virtual void OnRelease(bool charged, bool left)
    { Projectile.Kill(); }

    public virtual bool UseLeft => true;
    public virtual bool UseRight => false;
    public virtual bool Charging => UseLeft && Player.controlUseItem || UseRight && Player.controlUseTile;
    public virtual bool Charged => true;
    public virtual (int X, int Y) FrameMax => (1, 1);

    public virtual Texture2D GlowEffect
    {
        get
        {
            if (ModContent.HasAsset(GlowTexture))
            {
                return ModContent.Request<Texture2D>(GlowTexture).Value;
            }
            return null;
        }
    }

    public virtual float Factor => 0;
    public virtual Color GlowColor => Color.White;

    /// <summary>
    /// 1为左键
    /// 2为右键
    /// 3自由发挥
    /// </summary>
    public byte controlState;

    public Texture2D projTex => TextureAssets.Projectile[Projectile.type].Value;

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(controlState);
        base.SendExtraAI(writer);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        controlState = reader.ReadByte();
        base.ReceiveExtraAI(reader);
    }
}

public abstract class RangedHeldProjectile : HeldProjectile
{
    //public byte controlState
    //{
    //    get => (byte)Projectile.ai[1];
    //    set => Projectile.ai[1] = value;
    //}
    public override void SetDefaults()
    {
        Projectile.width = 1;
        Projectile.height = 1;
        Projectile.alpha = 0;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = -1;
        Projectile.hide = true;
    }

    public override bool Charging => base.Charging && Projectile.frame == 0;
    //Texture2D glowEffect;
    //public override void Load()
    //{
    //    base.Load();
    //    if (Mod.HasAsset(GlowTexture.Replace("VirtualDream/", "")))
    //    {
    //        glowEffect = IllusionBoundMod.GetTexture(GlowTexture, false);
    //    }
    //}

    public override bool Charged => Factor == 1;
    public virtual Vector2 ShootCenter => HeldCenter;
    public virtual Vector2 HeldCenter => Player.Center;

    public virtual void UpdatePlayer()
    {
        Player.ChangeDir(Projectile.direction);
        Player.heldProj = Projectile.whoAmI;
        Player.itemTime = 2;
        Player.itemAnimation = 2;
        Player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);
        Player.SetCompositeArmFront(enabled: true, Player.CompositeArmStretchAmount.Full, Player.itemRotation - MathHelper.PiOver2 - (Player.direction == -1 ? MathHelper.Pi : 0));
        Projectile.Center = Player.Center;
    }

    public override void AI()
    {
        base.AI();

        #region 更新玩家

        UpdatePlayer();

        #endregion 更新玩家

        #region 更新弹幕

        if (Charging)
        {
            Projectile.timeLeft = 2;
            Projectile.ai[0]++;
            if (Projectile.owner == Main.myPlayer) 
            {
                Projectile.velocity = (Main.MouseWorld - HeldCenter).SafeNormalize(Vector2.One);
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            Projectile.ai[1] = Player.controlUseItem ? 1 : 0;
            if (Player.controlUseItem)
            {
                controlState = 1;
            }
            if (Player.controlUseTile)
            {
                controlState = 2;
            }
            OnCharging(Player.controlUseItem, Player.controlUseTile);
        }
        else
        {
            OnRelease(Charged, Projectile.ai[1] == 1);
            Projectile.frame = 1;
        }
        //Main.NewText(Charged);

        #endregion 更新弹幕
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Vector2 center = HeldCenter - Main.screenPosition + new Vector2(0, Player.gfxOffY);
        Rectangle? frame = null;
        float rotation = Projectile.rotation;
        float scale = 1f;
        SpriteEffects effect = Player.direction == -1 ? SpriteEffects.FlipVertically : 0;
        Vector2 texSize = texture.Size() / new Vector2(FrameMax.X, FrameMax.Y);
        Vector2 origin = texSize * new Vector2(0, 1);
        GetDrawInfos(ref texture, ref center, ref frame, ref lightColor, ref rotation, ref origin, ref scale, ref effect);
        FlipOrigin(ref origin, effect, texSize);
        Main.EntitySpriteDraw(texture, center, frame, lightColor, rotation, origin, scale, effect, 0);
        if (GlowEffect != null)
        {
            Main.EntitySpriteDraw(GlowEffect, center, frame, GlowColor, rotation, origin, scale, effect, 0);
        }

        return false;
    }

    public virtual void GetDrawInfos(ref Texture2D texture, ref Vector2 center, ref Rectangle? frame, ref Color color, ref float rotation, ref Vector2 origin, ref float scale, ref SpriteEffects spriteEffects)
    {
    }

    public virtual void FlipOrigin(ref Vector2 origin, SpriteEffects spriteEffects, Vector2 textureSize)
    {
        origin.Y = spriteEffects == SpriteEffects.FlipVertically ? textureSize.Y - origin.Y : origin.Y;
        origin.X = spriteEffects == SpriteEffects.FlipHorizontally ? textureSize.X - origin.X : origin.X;
    }
}
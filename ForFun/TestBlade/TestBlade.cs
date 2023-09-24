using LogSpiralLibrary.CodeLibrary.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.ForFun.TestBlade
{
    public class TestBlade : ModItem
    {
        public override void SetDefaults()
        {
            Item.height = Item.width = 69;
            Item.DamageType = DamageClass.Melee;
            Item.damage = 50;
            Item.knockBack = 5f;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<TestBladeProj>();
            Item.shootSpeed = 1f;
            base.SetDefaults();
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
    }
    public class TestBladeProj : ModProjectile
    {
        public override string Texture => base.Texture.Replace("Proj", "");
        public MeleeSequence meleeSequence = new MeleeSequence();
        public IMeleeAttackData currentData => meleeSequence.currentData;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 10;
            Projectile.width = Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            RapidlyStabInfo stabInfo = new RapidlyStabInfo();
            SwooshInfo swooshInfo = new SwooshInfo();

            MeleeSequence.MeleeGroup groupStab = new MeleeSequence.MeleeGroup();
            meleeSequence.Add(groupStab);
            MeleeSequence.MeleeGroup groupSlash = new MeleeSequence.MeleeGroup();
            meleeSequence.Add(groupStab);
            MeleeSequence.MeleeGroup groupShoot = new MeleeSequence.MeleeGroup();
            meleeSequence.Add(groupStab);
            base.SetDefaults();
        }
        Player player => Main.player[Projectile.owner];
        public override void AI()
        {
            Projectile.damage = player.GetWeaponDamage(player.HeldItem);
            Projectile.Center = player.Center + currentData.offsetCenter;
            meleeSequence.Update(player);
            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value,
                player.Center - Main.screenPosition + currentData.offsetCenter,
                null, Color.White, currentData.Rotation - MathHelper.PiOver4, 
                currentData.offsetOrigin, currentData.ModifyData.actionOffsetSize, 0, 0);
            return base.PreDraw(ref lightColor);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!currentData.Attacktive) return false;
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + currentData.Rotation.ToRotationVector2() * currentData.ModifyData.actionOffsetSize * TextureAssets.Projectile[Projectile.type].Size().Length(), 48f, ref point);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.Knockback *= meleeSequence.currentData.ModifyData.actionOffsetKnockBack;
            base.ModifyHitNPC(target, ref modifiers);
        }

    }
}

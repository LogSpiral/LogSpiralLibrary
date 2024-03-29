﻿using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;

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
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.rare = ItemRarityID.Red;
            base.SetDefaults();
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
    }
    public class TestBladeProj : MeleeSequenceProj
    {
        class TestBladeGrayStabInfo : RapidlyStabInfo
        {
            public override void OnEndAttack()
            {
                var u = UltraStab.NewUltraStab(Color.Gray, 30, 120, Owner.Center, null, flip, Rotation, KValue * 3);
                u.ResetAllRenderInfo();
                //                var u = UltraStab.NewUltraStab(Color.Purple, 30, 120, Owner.Center,
                //LogSpiralLibraryMod.HeatMap[1].Value, negativeDir, Rotation,
                //KValue, -3, 8, colorVec: new Vector3(0, 0, 1));
                //                var useDistort = new AirDistortEffectInfo(10);
                //                var useBloom = new BloomEffectInfo(0,1,3,2,true);
                //                useBloom.range = 4;
                //                var useMask = new MaskEffectInfo(LogSpiralLibraryMod.Misc[21].Value, LogSpiralLibraryMod.Misc[21].Size(), Color.Cyan, Color.White, 0.1f, 0.15f, default, true, false);
                //                useMask.offset = Owner.Center + new Vector2(0.707f) *
                //                    (float)LogSpiralLibraryMod.ModTime * 8;
                //                useBloom.ReDraw = !useMask.Active;
                //                u.autoUpdate = false;
                //                u.GetVertexDrawInfoInstance().blendState = null;
                //                u.GetVertexDrawInfoInstance().SetEffectValue = null;
                //                u.ModityAllRenderInfo(useDistort, useMask, useBloom);
                //u.ModityAllRenderInfo(new AirDistortEffectInfo(), new MaskEffectInfo(), new BloomEffectInfo());
                base.OnEndAttack();
            }
        }
        class TestBladeSwooshInfo : SwooshInfo
        {
            public override void Update()
            {
                base.Update();
            }
            public override void OnEndAttack()
            {
                var u = UltraSwoosh.NewUltraSwoosh(Color.Gray, 15, 100, Owner.Center, null, this.flip, Rotation, KValue, (.625f, -.75f));
                u.ResetAllRenderInfo();

                base.OnEndAttack();
            }
            public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
            {
                base.Draw(spriteBatch, texture);
            }
        }
        class TestBladeConvoluteInfo : ConvoluteInfo
        {
            public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
            {
                base.Draw(spriteBatch, texture);
            }
            public override void OnAttack()
            {
                base.OnAttack();
            }
        }
        public override string Texture => base.Texture.Replace("Proj", "");
        public override void SetUpSequence(MeleeSequence meleeSequence)
        {
            meleeSequence.SequenceName = $"测试剑[i:{ModContent.ItemType<TestBlade>()}]";
            SwooshInfo swooshInfo = new TestBladeSwooshInfo()
            {
                Cycle = 4,
                KValue = 3,
                ModifyData = new(1, 4f)
            };
            meleeSequence.Add(swooshInfo);
            RapidlyStabInfo stabInfo = new TestBladeGrayStabInfo()
            {
                Cycle = 5,
                CycleOffsetRange = (0, 1),
                ModifyData = new(1, 1)
            };
            stabInfo.KValue = 3;
            meleeSequence.Add(stabInfo);
            ConvoluteInfo convoluteInfo = new TestBladeConvoluteInfo() 
            { 
                Cycle = 4,
                ModifyData = new (1,2f)
            };
            meleeSequence.Add(convoluteInfo);

            meleeSequence.Add(stabInfo);
        }
    }
    //public class TestBladeProj : ModProjectile
    //{
    //    public override string Texture => base.Texture.Replace("Proj", "");
    //    public MeleeSequence meleeSequence = new MeleeSequence();
    //    public IMeleeAttackData currentData => meleeSequence.currentData;
    //    public override void SetDefaults()
    //    {
    //        Projectile.timeLeft = 10;
    //        Projectile.width = Projectile.height = 1;
    //        Projectile.friendly = true;
    //        Projectile.DamageType = DamageClass.Melee;
    //        Projectile.tileCollide = false;
    //        Projectile.penetrate = -1;
    //        Projectile.aiStyle = -1;
    //        Projectile.hide = true;
    //        Projectile.usesLocalNPCImmunity = true;
    //        Projectile.localNPCHitCooldown = 2;
    //        RapidlyStabInfo stabInfo = new RapidlyStabInfo();
    //        //SwooshInfo swooshInfo = new SwooshInfo();
    //        stabInfo.Cycle = 4;
    //        stabInfo.CycleOffsetRange = (-2, 2);
    //        stabInfo.kValue = 1;
    //        MeleeSequence.MeleeGroup groupStab = new MeleeSequence.MeleeGroup();
    //        groupStab.meleeAttackDatas.Add(stabInfo);
    //        meleeSequence.Add(groupStab);
    //        //MeleeSequence.MeleeGroup groupSlash = new MeleeSequence.MeleeGroup();
    //        //meleeSequence.Add(groupSlash);
    //        //MeleeSequence.MeleeGroup groupShoot = new MeleeSequence.MeleeGroup();
    //        //meleeSequence.Add(groupShoot);
    //        base.SetDefaults();
    //    }
    //    Player player => Main.player[Projectile.owner];
    //    public override void AI()
    //    {
    //        player.heldProj = Projectile.whoAmI;
    //        Projectile.damage = player.GetWeaponDamage(player.HeldItem);
    //        if (player.controlUseItem || currentData == null || meleeSequence.timer > 0)
    //        {
    //            meleeSequence.Update(player);
    //            Projectile.timeLeft = 2;
    //        }
    //        Projectile.Center = player.Center + currentData.offsetCenter;

    //        base.AI();
    //    }
    //    public override bool PreDraw(ref Color lightColor)
    //    {
    //        SpriteBatch spriteBatch = Main.spriteBatch;
    //        spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value,
    //            player.Center - Main.screenPosition + currentData.offsetCenter,
    //            null, Color.White, currentData.Rotation + MathHelper.PiOver4,
    //            currentData.offsetOrigin * TextureAssets.Projectile[Type].Size(), currentData.ModifyData.actionOffsetSize, 0, 0);
    //        return false;
    //    }
    //    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    //    {
    //        if (!currentData.Attacktive) return false;
    //        float point = 0f;
    //        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
    //            Projectile.Center + currentData.Rotation.ToRotationVector2() * currentData.ModifyData.actionOffsetSize * TextureAssets.Projectile[Projectile.type].Size().Length(), 48f, ref point);
    //    }
    //    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    //    {
    //        modifiers.Knockback *= meleeSequence.currentData.ModifyData.actionOffsetKnockBack;
    //        target.immune[player.whoAmI] = 0;
    //        base.ModifyHitNPC(target, ref modifiers);
    //    }

    //}
}

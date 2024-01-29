using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;

namespace LogSpiralLibrary.ForFun.TestBlade2
{
    public class TestBlade2 : ModItem
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
            Item.shoot = ModContent.ProjectileType<TestBlade2Proj>();
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
    public class TestBlade2Proj : MeleeSequenceProj
    {
        public override string Texture => base.Texture.Replace("Proj", "");
        public override StandardInfo StandardInfo => new StandardInfo(-MathHelper.PiOver4, new Vector2(0.1f, 0.9f), player.itemAnimationMax, Color.White * .2f, null);
        public override void SetUpSequence(MeleeSequence meleeSequence)
        {
            meleeSequence.SequenceName = $"测试剑2号[i:{ModContent.ItemType<TestBlade2>()}]";
            MeleeSequence sub;
            MeleeSequence.Group group;
            #region 第一组
            group = new MeleeSequence.Group();
            #region 晚上
            sub = new MeleeSequence();
            sub.Add(new BoardSwordInfo());
            sub.Add(new ShortSwordInfo());
            sub.Add(new BoomerangInfo());
            sub.Add(new FlailInfo());
            group.wrapers.Add(new MeleeSequence.Wraper(sub).SetCondition(Condition.TimeNight));
            #endregion

            #region 白天
            sub = new MeleeSequence();
            sub.Add(new SpearInfo());
            sub.Add(new FistInfo());
            sub.Add(new HammerInfo());
            sub.Add(new KnivesInfo());
            group.wrapers.Add(new MeleeSequence.Wraper(sub).SetCondition(Condition.TimeDay));
            #endregion
            meleeSequence.Add(group);
            #endregion
            #region 第二组
            group = new MeleeSequence.Group();
            #region 血月
            sub = new MeleeSequence();
            sub.Add(new YoyoInfo());
            sub.Add(new ArkhalisInfo());
            sub.Add(new EruptionInfo());
            sub.Add(new RotatingInfo());
            group.wrapers.Add(new MeleeSequence.Wraper(sub).SetCondition(Condition.BloodMoon));
            #endregion

            #region 雨天
            sub = new MeleeSequence();
            sub.Add(new LanceInfo());
            sub.Add(new StarlightInfo());
            sub.Add(new ZenithInfo());
            sub.Add(new TerraprismaInfo());
            group.wrapers.Add(new MeleeSequence.Wraper(sub).SetCondition(Condition.InRain));
            #endregion
            meleeSequence.Add(group);
            #endregion
            meleeSequence.Add(new StabInfo());
        }
    }
}

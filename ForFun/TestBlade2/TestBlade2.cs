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
        /// <summary>
        /// 发射弹幕后用发射的弹幕的参数来整活用的函数！！
        /// </summary>
        /// <param name="player">发射出这个弹幕的玩家</param>
        /// <param name="source">发射源，包含物品 弹药等信息</param>
        /// <param name="position">发射位置</param>
        /// <param name="velocity">发射速度</param>
        /// <param name="type">发射弹幕的ID</param>
        /// <param name="damage">发射出的弹幕的伤害</param>
        /// <param name="knockback">击退</param>
        /// <returns>是否发射这个弹幕</returns> //所以我刚刚记错了，这个应该是在发出弹幕前就执行的，用这个可以控制是否发射，但是CanShoot又是干嘛的，目前只能觉着应该是历史遗留问题，旧版是用这个控制修改和是否发射
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Item.damage = Main.rand.Next(214, 514);
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
    }
    public class TestBlade2Proj : MeleeSequenceProj
    {
        public override string Texture => base.Texture.Replace("Proj", "");
        public override StandardInfo StandardInfo => new StandardInfo(-MathHelper.Pi / 3, new Vector2(0.1f, 0.9f), player.itemAnimationMax, Color.White * .2f, null, ModContent.ItemType<TestBlade2>());

        public override void SetUpSequence(MeleeSequence meleeSequence)
        {

            base.SetUpSequence(meleeSequence);
            return;
            //meleeSequence.sequenceName = $"测试剑2号[i:{ModContent.ItemType<TestBlade2>()}]";

            //meleeSequence.Add(new StarlightInfo());
            //return;
            MeleeSequence sub;
            MeleeSequence.Group group;
            #region 第一组
            group = new MeleeSequence.Group();
            #region 晚上
            sub = new MeleeSequence();
            sub.Add(new BoardSwordInfo());
            sub.Add(new ShortSwordInfo());
            var bi = new BoomerangInfo();
            bi.ModifyData = new ActionModifyData(1, 3);
            sub.Add(bi);
            var fi = new FlailInfo();
            fi.ModifyData = new ActionModifyData(1f, 4f);
            sub.Add(fi);
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
            var yyi = new YoyoInfo();
            yyi.ModifyData = new ActionModifyData(1, 4);
            sub.Add(yyi);
            var ai = new ArkhalisInfo();
            ai.Cycle = 4;
            sub.Add(ai);
            var ei = new EruptionInfo();
            ei.ModifyData = new ActionModifyData(1, 1.5f);
            ei.Cycle = 2;
            sub.Add(ei);
            sub.Add(new RotatingInfo());
            group.wrapers.Add(new MeleeSequence.Wraper(sub).SetCondition(Condition.InJungle));
            #endregion

            #region 雨天
            sub = new MeleeSequence();
            sub.Add(new LanceInfo());
            sub.Add(new StarlightInfo());
            sub.Add(new ZenithInfo());
            var tpi = new TerraprismaInfo();
            tpi.ModifyData = new ActionModifyData(1, 10);
            sub.Add(tpi);
            group.wrapers.Add(new MeleeSequence.Wraper(sub).SetCondition(Condition.InRain));
            #endregion
            meleeSequence.Add(group);
            #endregion
            var si = new StabInfo();
            si.Cycle = 4;
            meleeSequence.Add(si);
        }
    }
}

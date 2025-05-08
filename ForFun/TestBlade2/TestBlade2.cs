using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;

namespace LogSpiralLibrary.ForFun.TestBlade2
{
    public class TestBlade2 : ModItem
    {
        //public override bool IsLoadingEnabled(Mod mod) => false;
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
        public override bool CanShoot(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;
    }
    public class TestBlade2Proj : MeleeSequenceProj
    {
        //public override bool IsLoadingEnabled(Mod mod) => false;
        public override string Texture => base.Texture.Replace("Proj", "");
        static readonly AirDistortEffect distortEffect = new(3, 1.5f);
        static readonly BloomEffect bloomEffect = new(0f, 1f, 1, 3, true, 0, true);

        const string CanvasName = nameof(LogSpiralLibrary) + ":" + nameof(TestBlade2Proj);

        public override void Load()
        {
            RenderCanvasSystem.RegisterCanvasFactory(CanvasName, () => new RenderingCanvas([[distortEffect],[bloomEffect]]));//, [bloomEffect]
            base.Load();
        }

        public override void InitializeStandardInfo(StandardInfo standardInfo, VertexDrawStandardInfo vertexStandard)
        {
            standardInfo.standardRotation = -MathHelper.Pi / 3;
            standardInfo.standardColor = Color.White * .2f;
            standardInfo.itemType = ModContent.ItemType<TestBlade2>();

            vertexStandard.timeLeft = 15;
            vertexStandard.scaler = 120;
            vertexStandard.alphaFactor = 2f;
            vertexStandard.canvasName = CanvasName;
            base.InitializeStandardInfo(standardInfo, vertexStandard);
        }

        // 历史遗留代码
        // 之前没有配置文件，也没有编辑界面
        // 只能用代码嗯搞
        /*
        public override void SetUpSequence(MeleeSequence sequence, string modName, string fileName)
        {
            base.SetUpSequence(meleeSequence, modName, fileName);
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
        */
    }
}


using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using Terraria.ModLoader.IO;

namespace LogSpiralLibrary.ForFun.TestBlade3
{
    public class TestBlade3 : ModItem
    {
        //public override bool IsLoadingEnabled(Mod mod) => false;
        public override void SetDefaults()
        {
            Item.height = 94;
            Item.width = 46;
            Item.DamageType = DamageClass.Melee;
            Item.damage = 50;
            Item.knockBack = 5f;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<TestBlade3Proj>();
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
    public class TestBlade3Proj : MeleeSequenceProj
    {
        //public override bool IsLoadingEnabled(Mod mod) => false;
        public override string Texture => base.Texture.Replace("Proj", "");
        public override StandardInfo StandardInfo => Main.netMode == NetmodeID.Server ? default : base.StandardInfo with 
        {
            standardColor = Color.Purple,
            //standardGlowTexture = DrawingMethods.GetTexture("LogSpiralLibrary/ForFun/TestBlade3/KluexStaffPH_Glow", false),
            standardRotation = -MathHelper.PiOver4,
            //standardOrigin = new Vector2(0.5f,0.8f),
            vertexStandard = new VertexDrawInfoStandardInfo()
            {
                active = true,
                
                scaler = 120,
                timeLeft = 15,
                renderInfos = [[new AirDistortEffectInfo(3)],[default(MaskEffectInfo),new BloomEffectInfo(0.05f,0.25f,1f,2,true)]],
                alphaFactor = 2f,

            },
            itemType = ModContent.ItemType<TestBlade3>()
        };
        public override bool LabeledAsCompleted => false;
        public override void SetUpSequence(MeleeSequence sequence, string modName, string fileName)
        {
            base.SetUpSequence(meleeSequence, modName, fileName);
            return;
            //meleeSequence.sequenceName = "TestBlade3Proj";
            //var subSequence1 = MeleeSequence.Load("C:\\Users\\32536\\Documents\\My Games\\Terraria\\tModLoader\\Mods\\LogSpiralLibrary_Sequence\\MeleeAction\\LogSpiralLibrary\\TestBladeProj.xml");
            //var subSequence2 = MeleeSequence.Load("C:\\Users\\32536\\Documents\\My Games\\Terraria\\tModLoader\\Mods\\LogSpiralLibrary_Sequence\\MeleeAction\\LogSpiralLibrary\\TestBlade2Proj.xml");
            //var group1 = new MeleeSequence.Group();
            //group1.wrapers.Add(new MeleeSequence.Wraper(subSequence1).SetCondition(Condition.EclipseOrBloodMoon));
            //group1.wrapers.Add(new MeleeSequence.Wraper(new SwooshInfo() { Cycle = 4,ModifyData = new(1,2f) }).SetCondition(Condition.TimeDay));
            //group1.wrapers.Add(new MeleeSequence.Wraper(subSequence2).SetCondition(Condition.TimeNight));
            //meleeSequence.Add(group1);
            //meleeSequence.Add(subSequence1);
        }
    }

    public class TestBlade3V2 : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => false;
        public override string Texture => base.Texture.Replace("V2", "");
        public override void SetDefaults()
        {
            Item.height = 94;
            Item.width = 46;
            Item.DamageType = DamageClass.Melee;
            Item.damage = 50;
            Item.knockBack = 5f;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<TestBlade3ProjV2>();
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
    public class TestBlade3ProjV2 : MeleeSequenceProj
    {
        public override bool IsLoadingEnabled(Mod mod) => false;
        public override string Texture => base.Texture.Replace("ProjV2", "");
        public override StandardInfo StandardInfo => Main.netMode == NetmodeID.Server ? default : base.StandardInfo with
        {
            standardColor = Color.Purple,
            standardRotation = -MathHelper.PiOver4,
            vertexStandard = new VertexDrawInfoStandardInfo() with
            {
                active = true,
                scaler = 120,
                timeLeft = 15,
                heatMap = LogSpiralLibraryMod.HeatMap[5].Value,
                colorVec = new Vector3(0,0.5f,0.5f),
                renderInfos = [[new AirDistortEffectInfo(3)], [new MaskEffectInfo(LogSpiralLibraryMod.Misc[25].Value,Color.Purple,0.15f,0.25f,default,true,false), new BloomEffectInfo(0.75f, 0.85f, 1f, 2, true)]],
                alphaFactor = 2f,

            },
            itemType = ModContent.ItemType<TestBlade3V2>()
        };
    }
}   

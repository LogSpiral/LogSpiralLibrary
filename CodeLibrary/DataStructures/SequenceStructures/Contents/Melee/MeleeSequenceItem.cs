namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

/// <summary>
/// 基剑必需品
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MeleeSequenceItem<T> : ModItem where T : MeleeSequenceProj
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.SkipsInitialUseSound[Type] = true;
        base.SetStaticDefaults();
    }

    public override bool AltFunctionUse(Player player) => EnableRightClick;

    public override void SetDefaults()
    {
        Item.width = 62;
        Item.height = 62;
        Item.useTime = 24;
        Item.useAnimation = 24;
        Item.rare = ItemRarityID.Red;
        Item.UseSound = SoundID.Item1;
        Item.knockBack = 4.95f;
        Item.damage = 514;

        //上面的是一些默认的属性，一般需要在重写那边另外赋值
        //下面是必要的，重写那边不用再写

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.DamageType = DamageClass.Melee;
        Item.shoot = ModContent.ProjectileType<T>();
        Item.shootSpeed = 1f;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.channel = true;
    }

    public override bool CanShoot(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<T>()] == 0;

    public virtual bool EnableRightClick => false;
}
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

namespace LogSpiralLibrary.ForFun.DoubleBladeTest;

public class DoubleBladeTest : ModItem
{
    public override bool IsLoadingEnabled(Mod mod) => false;
    public override string Texture => $"Terraria/Images/Item_{ItemID.Arkhalis}";
    public override void SetStaticDefaults()
    {
        ItemID.Sets.SkipsInitialUseSound[Type] = true;
        base.SetStaticDefaults();
    }
    public override bool AltFunctionUse(Player player) => true;
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
        Item.shootSpeed = 1f;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.channel = true;
    }
    public override bool CanShoot(Player player) => false;
    public override void HoldItem(Player player)
    {
        if (player.controlUseItem && Main.mouseLeft && ModContent.ProjectileType<DoubleBladeTestLeftProj>() is int typeLeft && player.ownedProjectileCounts[typeLeft] == 0)
        {
            var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.UnitX, typeLeft, player.GetWeaponDamage(Item), player.GetWeaponKnockback(Item), player.whoAmI);
            proj.hide = false;
        }

        if (player.controlUseTile && Main.mouseRight && ModContent.ProjectileType<DoubleBladeTestRightProj>() is int typeRight && player.ownedProjectileCounts[typeRight] == 0)
        {
            var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.UnitX, typeRight, player.GetWeaponDamage(Item), player.GetWeaponKnockback(Item), player.whoAmI);
            proj.hide = false;
        }

        base.HoldItem(player);
    }
    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        spriteBatch.Draw(TextureAssets.Item[ItemID.Terragrim].Value, position, frame, drawColor, MathHelper.Pi / 6, origin, scale, 0, 0);
        spriteBatch.Draw(TextureAssets.Item[ItemID.Arkhalis].Value, position, frame, drawColor, 0, origin, scale, 0, 0);
        return false;
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        spriteBatch.Draw(TextureAssets.Item[ItemID.Terragrim].Value, Item.position - Main.screenPosition, null, lightColor, MathHelper.Pi / 6 + rotation, new(0, 32), scale, 0, 0);
        spriteBatch.Draw(TextureAssets.Item[ItemID.Arkhalis].Value, Item.position - Main.screenPosition, null, lightColor, rotation, new(0, 32), scale, 0, 0);
        return false;
    }
}
public class DoubleBladeTestLeftProj : MeleeSequenceProj
{
    public override bool IsLoadingEnabled(Mod mod) => false;
    public override bool LabeledAsCompleted => true;
    public override string Texture => $"Terraria/Images/Item_{ItemID.Arkhalis}";

    public override void InitializeStandardInfo(StandardInfo standardInfo, VertexDrawStandardInfo vertexStandard)
    {

        standardInfo.standardColor = Color.Cyan * .1f;
        standardInfo.itemType = ItemID.Arkhalis;

        vertexStandard.timeLeft = 15;
        vertexStandard.scaler= 60;
        vertexStandard.colorVec = new(0, 1, 0);

    }
}
public class DoubleBladeTestRightProj : MeleeSequenceProj
{
    public override bool IsLoadingEnabled(Mod mod) => false;
    public override bool LabeledAsCompleted => true;
    public override string Texture => $"Terraria/Images/Item_{ItemID.Terragrim}";

    public override void InitializeStandardInfo(StandardInfo standardInfo, VertexDrawStandardInfo vertexStandard)
    {

        standardInfo.standardColor = Color.LimeGreen * .1f;
        standardInfo.itemType = ItemID.Arkhalis;

        vertexStandard.timeLeft = 15;
        vertexStandard.scaler = 60;
        vertexStandard.colorVec = new(0, 1, 0);

    }
}

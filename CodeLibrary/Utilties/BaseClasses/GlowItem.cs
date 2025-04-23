using ReLogic.Content;
namespace LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
public abstract class GlowItem : ModItem
{
    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        
        string glowPath = Texture + "_Glow";
        if (ModContent.HasAsset(glowPath) && ModContent.Request<Texture2D>(glowPath) is Asset<Texture2D> texture)
            spriteBatch.Draw(texture.Value, Item.Center - Main.screenPosition, null, Color.White, rotation, texture.Size() * .5f, scale, 0, 0);
    }
}
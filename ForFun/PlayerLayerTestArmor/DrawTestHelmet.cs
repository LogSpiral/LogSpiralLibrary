using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.ForFun.PlayerLayerTestArmor
{
    [AutoloadEquip(EquipType.Head)]
    public class DrawTestHelmet : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 14;
            Item.vanity = true;
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }
    }
    public class TestDrawLayer : PlayerDrawLayer
    {
        public override void Draw(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;
            if (player.armor[10].type == ModContent.ItemType<DrawTestHelmet>())
            {
                var data = new DrawData(TextureAssets.Extra[98].Value, 
                    player.Center + new Vector2(0, -16) - Main.screenPosition, null, 
                    Color.White with { A = 0 }, (float)LogSpiralLibraryMod.ModTime / 60, new Vector2(36), 1, 0, 0);
                drawInfo.DrawDataCache.Add(data);
                //Main.NewText("草");
            }
        }
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Head);
    }
}

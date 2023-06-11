global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria.ModLoader;
global using Terraria;
global using Terraria.ID;
global using Terraria.DataStructures;
global using Terraria.GameContent;
using ReLogic.Content;

namespace LogSpiralLibrary
{
    public class LogSpiralLibrary : Mod
    {
        public static Asset<Texture2D>[] BaseTex;
        public static Asset<Texture2D>[] AniTex;
        public static Asset<Texture2D>[] HeatMap;
        public static Asset<Texture2D>[] MagicZone;
        public static Asset<Texture2D>[] Misc;

        public override void Load()
        {
            LoadTextures(ref BaseTex, nameof(BaseTex));
            LoadTextures(ref AniTex, nameof(AniTex));
            LoadTextures(ref HeatMap, nameof(HeatMap));
            LoadTextures(ref MagicZone, nameof(MagicZone));
            LoadTextures(ref HeatMap, nameof(HeatMap));

            base.Load();
        }
        private static void LoadTextures(ref Asset<Texture2D>[] assets, string textureName)
        {
            int i = 0;
            string path = $"LogSpiralLibrary/Images/{textureName}/{textureName}_";
            while (true)
            {
                if (ModContent.HasAsset($"{path}{i}"))
                {
                    i++;
                }
                else
                    break;
            }
            assets = new Asset<Texture2D>[i];
            for (int n = 0; n < i; n++) 
            {
                assets[n] = ModContent.Request<Texture2D>($"{path}{i}");
            }
        }
    }



}
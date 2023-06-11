using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LogSpiralLibrary.ForFun.FractalSpawn
{
    public class FractalSpawnSystem : ModSystem
    {
        private static Effect fractal;
        public static Effect FractalEffect => fractal ??= ModContent.Request<Effect>("LogSpiralLibrary/Effects/Fractal").Value;
        public static RenderTarget2D render;
        public static RenderTarget2D renderShift;
        public override void Load()
        {
            var gd = Main.instance.GraphicsDevice;
            Color[] colors = new Color[1000000];
            Array.Fill(colors, new Color(0.5f, 0.5f, 0));
            Main.RunOnMainThread(() =>
            {
                render = new RenderTarget2D(gd, 1000, 1000);
                renderShift = new RenderTarget2D(gd, 1000, 1000);
                render.SetData(colors);
                renderShift.SetData(colors);
                //gd.SetRenderTarget(render);
                //gd.Clear(new Color(0.5f, 0.5f, 0));
                //gd.SetRenderTarget(renderShift);
                //gd.Clear(new Color(0.5f, 0.5f, 0));
                //gd.SetRenderTarget(Main.screenTarget);
            });

            base.Load();
        }

        public override void Unload()
        {
            render.Dispose();
            renderShift.Dispose();
            base.Unload();
        }
    }
    public class FractalSpawnItem : ModItem
    {
        public static Vector2 mouseScreen;
        public override string Texture => "Terraria/Images/Item_1";
        public override void SetDefaults()
        {
            Item.width = Item.height = 10;
            base.SetDefaults();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = Item.useAnimation = 24;
        }
        public override bool? UseItem(Player player)
        {
            if (player.itemAnimation == 1)
            {
                FileStream fileStream = new FileStream("C:/图片测试_LogSpiralLibrary/存下图图.png", FileMode.OpenOrCreate);
                FractalSpawnSystem.render.SaveAsPng(fileStream, 1000, 1000);
                fileStream.Dispose();
                Main.NewText("存图");
            }
            //Main.NewText(player.itemAnimation);
            return base.UseItem(player);
        }
        public override void UseAnimation(Player player)
        {
            base.UseAnimation(player);
        }
        public override bool CanUseItem(Player player)
        {
            return base.CanUseItem(player);
        }
        public static void ResetFractal()
        {
            var gd = Main.instance.GraphicsDevice;
            Color[] colors = new Color[1000000];
            Array.Fill(colors, new Color(0.5f, 0.5f, 0));
            //Main.RunOnMainThread(() =>
            //{
            //    FractalSpawnSystem.render.SetData(colors);
            //    FractalSpawnSystem.renderShift.SetData(colors);
            //});
            FractalSpawnSystem.render.SetData(colors);
            FractalSpawnSystem.renderShift.SetData(colors);
        }
        public static void UpdateFractal()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            var gd = Main.instance.GraphicsDevice;
            var effect = FractalSpawnSystem.FractalEffect;
            if (effect == null)
            {
                Main.NewText("螺线你在干什么");
                return;
            }
            var render = FractalSpawnSystem.render;
            var renderShift = FractalSpawnSystem.renderShift;
            gd.SetRenderTarget(render);//设置画布，将renderShift计算结果存在里面
            gd.Clear(Color.Transparent);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            effect.Parameters["uRange"].SetValue(new Vector4(-2, -2, 2, 2));
            effect.Parameters["uM"].SetValue();

            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(renderShift, new Vector2(), Color.White);
            spriteBatch.End();

            gd.SetRenderTarget(renderShift);
            gd.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(render, new Vector2(), Color.White);
            spriteBatch.End();

            gd.SetRenderTarget(Main.screenTarget);
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            var gd = Main.instance.GraphicsDevice;
            var effect = FractalSpawnSystem.FractalEffect;
            if (effect == null)
            {
                //Main.NewText("螺线你在干什么");
                return false;
            }
            var render = FractalSpawnSystem.render;
            var renderShift = FractalSpawnSystem.renderShift;
            bool drawOnly = Main.MouseScreen == mouseScreen;
            if (drawOnly)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                gd.Textures[1] = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/HeatMap/HeatMap_22").Value;
                effect.CurrentTechnique.Passes[1].Apply();
                gd.SamplerStates[1] = SamplerState.AnisotropicClamp;
                spriteBatch.Draw(renderShift, new Vector2(460, 0), Color.White);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                ResetFractal();
                UpdateFractal();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                gd.Textures[1] = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/HeatMap/HeatMap_22").Value;
                effect.CurrentTechnique.Passes[1].Apply();
                gd.SamplerStates[1] = SamplerState.AnisotropicClamp;
                spriteBatch.Draw(renderShift, new Vector2(460, 0), Color.White);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}

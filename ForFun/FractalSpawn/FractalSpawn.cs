using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using ReLogic.Graphics;

namespace LogSpiralLibrary.ForFun.FractalSpawn
{
    public class FractalSpawnSystem : ModSystem
    {
        public override bool IsLoadingEnabled(Mod mod) => false;
        public static Effect FractalEffect => ModAsset.Fractal.Value;
        public static RenderTarget2D render;
        public static RenderTarget2D renderShift;

        public override void Load()
        {
            if (Main.dedServ) return;
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
            if (Main.dedServ) return;

            Main.RunOnMainThread(() =>
            {
                render.Dispose();
                renderShift.Dispose();
            });

            base.Unload();
        }
    }

    public class FractalSpawnItem : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => false;

        public static Vector2 mouseScreen;
        public override string Texture => $"Terraria/Images/Item_{ItemID.WireKite}";

        public override void SetDefaults()
        {
            Item.width = Item.height = 10;
            base.SetDefaults();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = Item.useAnimation = 24;
        }
        public static void ResetFractal()
        {
            var gd = Main.instance.GraphicsDevice;
            Color[] colors = new Color[1000000];
            Array.Fill(colors, new Color(0.5f, 0.5f, 0));
            FractalSpawnSystem.render.SetData(colors);
            FractalSpawnSystem.renderShift.SetData(colors);
        }

        public static void UpdateFractal()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            var gd = Main.instance.GraphicsDevice;
            var effect = FractalSpawnSystem.FractalEffect;
            var render = FractalSpawnSystem.render;
            var renderShift = FractalSpawnSystem.renderShift;
            gd.SetRenderTarget(render);//设置画布，将renderShift计算结果存在里面
            gd.Clear(Color.Transparent);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            effect.Parameters["uRange"].SetValue(new Vector4(-2, -2, 2, 2));
            Vector2 t = VectorMethods.GetLerpValue(default, Main.ScreenSize.ToVector2(), mouseScreen, true);
            Main.NewText($"Screen Factor{t}");
            t = VectorMethods.Lerp(new Vector2(-2, 2), new Vector2(2, -2), t, false);
            Main.NewText($"Coefficient{t}");
            effect.Parameters["uM"].SetValue(t);

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
            if (!LogSpiralLibraryMod.CanUseRender)
            {
                spriteBatch.DrawString(FontAssets.MouseText.Value, "Color light mode required", Item.position - Main.screenPosition, Color.White);
                return true;
            }
            var gd = Main.instance.GraphicsDevice;
            var effect = FractalSpawnSystem.FractalEffect;

            var renderShift = FractalSpawnSystem.renderShift;
            bool drawOnly = Main.MouseScreen == mouseScreen;
            if (false)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                gd.Textures[1] = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/HeatMap/HeatMap_1").Value;
                effect.CurrentTechnique.Passes[1].Apply();
                gd.SamplerStates[1] = SamplerState.AnisotropicClamp;
                spriteBatch.Draw(renderShift, new Vector2(460, 0), Color.White);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                mouseScreen = Main.MouseScreen;
                ResetFractal();
                UpdateFractal();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                gd.Textures[1] = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/HeatMap/HeatMap_1").Value;
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
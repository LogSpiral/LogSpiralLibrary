//using LogSpiralLibrary.CodeLibrary;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace LogSpiralLibrary.ForFun.BezierCurveDrawingTest
//{
//    public class BCurveTestItem : ModItem
//    {
//        public override void SetDefaults()
//        {
//            Item.width = 32;
//            Item.height = 28;
//            base.SetDefaults();
//        }
//        public override string Texture => $"Terraria/Images/Item_{ItemID.WireKite}";
//        Vector2[] vecs;
//        BezierCurve<FloatVector2, Vector2> bezierCurve;
//        public BCurveTestItem()
//        {
//            IVector<FloatVector2, Vector2>[] inputs = new IVector<FloatVector2, Vector2>[13];
//            float[] iptX = new float[13];
//            float[] iptY = [0.032f, 0.150f, 0.260f, 0.367f, 0.418f, 0.477f, 0.518f, 0.514f, 0.426f, 0.342f, 0.249f, 0.132f, 0.005f];
//            vecs = new Vector2[13];
//            for (int n = 0; n < 13; n++)
//            {
//                iptX[n] = 10 * n / 12f;
//                iptY[n] *= 17;
//                vecs[n] = new Vector2(iptX[n], iptY[n]);
//                inputs[n] = new FloatVector2(vecs[n]);
//            }
//            bezierCurve = new(inputs);
//            bezierCurve.Recalculate(1300);
//        }
//        public override Color? GetAlpha(Color lightColor)
//        {
//            return Main.DiscoColor;
//        }
//        void DrawPoint(SpriteBatch spriteBatch, Vector2 position, Color color, float size)
//        {
//            spriteBatch.Draw(TextureAssets.MagicPixel.Value, position, new Rectangle(0, 0, 1, 1), color, 0, new Vector2(.5f), size, 0, 0);
//        }
//        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
//        {
//            var result = (from vec in bezierCurve.results select vec.Value).ToList();
//            for (int n = 0; n < result.Count(); n++)
//            {
//                DrawPoint(spriteBatch, result[n] * 60 + new Vector2(400), Color.Red, 4);
//            }
//            for (int n = 0; n < vecs.Length; n++)
//            {
//                DrawPoint(spriteBatch, vecs[n] * 60 + new Vector2(400), Color.White * .5f, 8);

//            }
//            base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
//        }
//        public override bool IsLoadingEnabled(Mod mod)
//        {
//            return true;
//        }
//    }
//}
using LogSpiralLibrary.CodeLibrary;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace LogSpiralLibrary.ForFun.HyperCube
{
    public class HyperCube : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.LunarBrick}";
        /// <summary>
        /// 获取立方体顶点坐标
        /// </summary>
        /// <param name="index"></param>
        /// <param name="scaler"></param>
        /// <returns></returns>
        private static Vector4 GetVector(int index, float scaler = 128)
        {
            var v = new Vector4(index / 8, index / 4 % 2, index / 2 % 2, index % 2);//单位立方体的顶点们
            v -= Vector4.One * .5f;//偏移一下中心
            v *= scaler;//放大放大再放大
            var t = (float)Main.time / 300 * MathHelper.TwoPi;//旋转角
            var matrix = Matrix.CreateRotationY(t) * Matrix.CreateRotationX(2 * t);//旋转矩阵
            return v.ApplyMatrix(matrix);
            //↓有笨蛋以前不知道有现成的创建旋转的函数

            //var c = (float)Math.Cos(t);
            //var s = (float)Math.Sin(t);
            //var matrix =
            //new Matrix
            //(
            //    c, 0, -s, 0,
            //    0, 1, 0, 0,
            //    s, 0, c, 0,
            //    0, 0, 0, 1
            //)
            //* new Matrix
            //(
            //    1, 0, 0, 0,
            //    0, c, -s, 0,
            //    0, s, c, 0,
            //    0, 0, 0, 1
            //);
            //return v.ApplyMatrix(matrix);
        }
        /// <summary>
        /// 选定立方体中心进行绘制
        /// </summary>
        /// <param name="center"></param>
        public void DrawHyperCube(Vector2 center)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            const float heightZ = 512;//投影中心的Z坐标
            const float heightW = 256;//W坐标
            const float width = 4;//线宽度
            var c = center - Main.screenPosition;//立方体中心XY坐标
            var pC = Main.screenPosition + Main.ScreenSize.ToVector2() * .5f - center;//投影中心XY坐标
            for (int n = 0; n < 15; n++)
            {
                var v = GetVector(n);
                if (n % 2 == 0)
                {
                    DrawLine(spriteBatch, v, GetVector(n + 1), Main.hslToRgb(0, 1, 0.75f), heightZ, heightW, width, false, c, pC);
                }
                if (n / 2 % 2 == 0)
                {
                    DrawLine(spriteBatch, v, GetVector(n + 2), Main.hslToRgb(0, 0.75f, 0.75f), heightZ, heightW, width, false, c, pC);
                }
                if (n / 4 % 2 == 0)
                {
                    DrawLine(spriteBatch, v, GetVector(n + 4), Main.hslToRgb(0, 0.75f, 0.5f), heightZ, heightW, width, false, c, pC);
                }
                if (n / 8 == 0)
                {
                    DrawLine(spriteBatch, v, GetVector(n + 8), Main.hslToRgb(0, 0.5f, 0.5f), heightZ, heightW, width, false, c, pC);
                }
                //我知道上面那一段看着有点迷惑
                //但其实意思挺简单，就是如果这个点还在单位立方体里面的时候某个坐标是0，那么就连接到它对应的和它相同坐标，不过这一个坐标为1的点上
                //比如(0,0,0,0)分别会和(1,0,0,0),(0,1,0,0),(0,0,1,0),(0,0,0,1)连接
                //因为是超立方体所以可以用一个四位2进制数给顶点编号，只有前15个数是存在某一位是0的
                //循环上限我就写的15了
                //你看看你能不能自己改个三维版本出来试试
                //其实这里有很多很明显的冗余计算，不过无所谓了
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            DrawHyperCube(Item.Center);
            return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            DrawHyperCube(Main.LocalPlayer.Center);
            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        /// <summary>
        /// 绘制四维线在二维投影
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="color">颜色</param>
        /// <param name="heightZ">投影中心z坐标</param>
        /// <param name="heightW">投影中心w坐标</param>
        /// <param name="width">线宽度</param>
        /// <param name="offset">是否采用偏移模式(如果采用，end就不是终点而是起点到终点的偏移量</param>
        /// <param name="drawOffset">绘制全局偏移，一般放-<see cref="Main.screenPosition"/></param>
        /// <param name="projCenter">投影中心xy坐标，欸为什么我不和前面两个合成一下，我之前怎么想的</param>
        public static void DrawLine(SpriteBatch spriteBatch, Vector4 start, Vector4 end, Color color, float heightZ, float heightW, float width = 4f, bool offset = false, Vector2 drawOffset = default, Vector2 projCenter = default)
        {
            if (offset)
            {
                end += start;
            }
            var s = Projectile(Projectile(start, heightW, new Vector3(projCenter, 0)), heightZ, projCenter);//其实明明可以一次投影到2维的，鬼知道我之前在想什么
            var e = Projectile(Projectile(end, heightW, new Vector3(projCenter, 0)), heightZ, projCenter);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, (s + e) * .5f + drawOffset,
                new Rectangle(0, 0, 1, 1), color, (e - s).ToRotation(),
                new Vector2(.5f, .5f), new Vector2((s - e).Length(), width), 0, 0);//单纯的直线绘制
        }
        /// <summary>
        /// 获取三维向量在平面上的投影坐标
        /// </summary>
        /// <param name="vector">投影向量本身</param>
        /// <param name="height">从多高的位置打个灯</param>
        /// <param name="center">投影中心xy</param>
        /// <returns></returns>
        public static Vector2 Projectile(Vector3 vector, float height, Vector2 center = default)
        {
            return (new Vector2(vector.X, vector.Y) - center) * height / (height - vector.Z) + center;
        }
        /// <summary>
        /// 获取四维向量在空间上的投影坐标
        /// </summary>
        /// <param name="vector">投影向量本身</param>
        /// <param name="height">从多高的位置打个灯</param>
        /// <param name="center">投影中心xyz</param>
        /// <returns></returns>
        public static Vector3 Projectile(Vector4 vector, float height, Vector3 center = default)
        {
            return (new Vector3(vector.X, vector.Y, vector.Z) - center) * height / (height - vector.W) + center;
        }
    }
}

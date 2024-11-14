using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing
{
    public class ComplexPanelInfo
    {
        public static void DrawComplexPanel_Bound(List<DrawDataBuffer> drawDatas, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation)
        {
            int count = (int)(length / 192f) + 1;
            Vector2 start = rotation.ToRotationVector2() * length * .5f;
            Vector2 end = center + start;
            start = end - 2 * start;
            float lengthScaler = length / 192f / count;
            for (int n = 0; n < count; n++)
            {
                drawDatas.Add(new(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(336, 0, 192, 40), Color.White, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0));
            }

        }
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation)
        {
            int count = (int)(length / 192f) + 1;
            Vector2 start = rotation.ToRotationVector2() * length * .5f;
            Vector2 end = center + start;
            start = end - 2 * start;
            float lengthScaler = length / 192f / count;
            for (int n = 0; n < count; n++)
            {
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(336, 0, 192, 40), Color.White, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
            }

        }
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation, Color glowLight)
        {
            int count = (int)(length / 192f) + 1;
            Vector2 start = rotation.ToRotationVector2() * length * .5f;
            Vector2 end = center + start;
            start = end - 2 * start;
            float lengthScaler = length / 192f / count;
            for (int n = 0; n < count; n++)
            {
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(336, 0, 192, 40), Color.White, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(530, 0, 192, 40), glowLight, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
            }
        }
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation, Color glowLight, float glowShakingStrength, float glowHueOffsetRange = .2f)
        {
            int count = (int)(length / 192f) + 1;
            Vector2 start = rotation.ToRotationVector2() * length * .5f;
            Vector2 end = center + start;
            start = end - 2 * start;
            float lengthScaler = length / 192f / count;
            for (int n = 0; n < count; n++)
            {
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(336, 0, 192, 40), Color.White, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                if (glowShakingStrength == 0)
                    spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(530, 0, 192, 40), glowLight, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                else
                    for (int k = 0; k < 4; k++)
                        spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, Main.rand.NextFloat(4f * glowShakingStrength)), new Rectangle(530, 0, 192, 40), ModifyHueByRandom(glowLight, glowHueOffsetRange), rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);

            }
        }
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation, Color glowLight, float glowShakingStrength, int count, float glowHueOffsetRange = .2f)
        {
            Vector2 start = rotation.ToRotationVector2() * length * .5f;
            Vector2 end = center + start;
            start = end - 2 * start;
            float lengthScaler = length / 192f / count;
            for (int n = 0; n < count; n++)
            {
                spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(336, 0, 192, 40), Color.White, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                if (glowShakingStrength == 0)
                    spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count), new Rectangle(530, 0, 192, 40), glowLight, rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);
                else
                    for (int k = 0; k < 4; k++)
                        spriteBatch.Draw(texture, Vector2.Lerp(start, end, (n + .5f) / count) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, Main.rand.NextFloat(4f * glowShakingStrength)), new Rectangle(530, 0, 192, 40), ModifyHueByRandom(glowLight, glowHueOffsetRange), rotation, new Vector2(96, 18), new Vector2(lengthScaler, widthScaler), 0, 0);

            }
        }
        public static void DrawComplexPanel_Bound(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, float length, float widthScaler, float rotation, Color glowLight, float glowShakingStrength, int? count, float glowHueOffsetRange = .2f)
        {
            if (count == null) DrawComplexPanel_Bound(spriteBatch, texture, center, length, widthScaler, rotation, glowLight, glowShakingStrength, glowHueOffsetRange);
            else DrawComplexPanel_Bound(spriteBatch, texture, center, length, widthScaler, rotation, glowLight, glowShakingStrength, count.Value, glowHueOffsetRange);
        }
        /// <summary>
        /// 指定背景图
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="frame"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawComplexPanel_BackGround(SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Rectangle frame, Vector2 size, Color color)
        {
            (float sizeX, float sizeY) = (size.X, size.Y);
            int countX = (int)(destination.Width / sizeX) + 1;
            int countY = (int)(destination.Height / sizeY) + 1;
            float width = frame.Width;
            for (int i = 0; i < countX; i++)
            {
                if (i == countX - 1) width = (destination.Width - i * sizeX) / sizeX * width;
                float height = frame.Height;
                for (int j = 0; j < countY; j++)
                {
                    if (j == countY - 1) height = (destination.Height - j * sizeY) / sizeY * height;
                    spriteBatch.Draw(texture, destination.TopLeft() + new Vector2(i * sizeX, j * sizeY), new Rectangle(frame.X, frame.Y, (int)width, (int)height), color, 0, default, new Vector2(sizeX, sizeY) / frame.Size() * 1.025f, 0, 0);
                }
            }
        }
        public static void DrawComplexPanel_BackGround(List<DrawDataBuffer> drawDatas, Texture2D texture, Rectangle destination, Rectangle frame, Vector2 size, Color color)
        {
            (float sizeX, float sizeY) = (size.X, size.Y);
            int countX = (int)(destination.Width / sizeX) + 1;
            int countY = (int)(destination.Height / sizeY) + 1;
            float width = frame.Width;
            for (int i = 0; i < countX; i++)
            {
                if (i == countX - 1) width = (destination.Width - i * sizeX) / sizeX * width;
                float height = frame.Height;
                for (int j = 0; j < countY; j++)
                {
                    if (j == countY - 1) height = (destination.Height - j * sizeY) / sizeY * height;
                    drawDatas.Add(new(texture, destination.TopLeft() + new Vector2(i * sizeX, j * sizeY), new Rectangle(frame.X, frame.Y, (int)width, (int)height), color, 0, default, new Vector2(sizeX, sizeY) / frame.Size() * 1.025f, 0, 0));
                }
            }
        }
        /// <summary>
        /// 使用config材质
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="texture"></param>
        /// <param name="rectangle"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        public static void DrawComplexPanel_BackGround(SpriteBatch spriteBatch, Texture2D texture, Rectangle rectangle, Vector2 size)
        {
            int countX = (int)(rectangle.Width / size.X) + 1;
            int countY = (int)(rectangle.Height / size.Y) + 1;
            float width = 40;
            for (int i = 0; i < countX; i++)
            {
                if (i == countX - 1) width = (rectangle.Width - i * size.X) / size.X * 40;
                float height = 40;
                for (int j = 0; j < countY; j++)
                {
                    if (j == countY - 1) height = (rectangle.Height - j * size.Y) / size.Y * 40;
                    spriteBatch.Draw(texture, rectangle.TopLeft() + new Vector2(i * size.X, j * size.Y), new Rectangle(210, 0, (int)width, (int)height), Color.White, 0, default, size / 40f * 1.025f, 0, 0);
                }
            }
        }

        public static void DrawComplexPanel_BackGround(List<DrawDataBuffer> drawDatas, Texture2D texture, Rectangle rectangle, Vector2 size)
        {
            int countX = (int)(rectangle.Width / size.X) + 1;
            int countY = (int)(rectangle.Height / size.Y) + 1;
            float width = 40;
            for (int i = 0; i < countX; i++)
            {
                if (i == countX - 1) width = (rectangle.Width - i * size.X) / size.X * 40;
                float height = 40;
                for (int j = 0; j < countY; j++)
                {
                    if (j == countY - 1) height = (rectangle.Height - j * size.Y) / size.Y * 40;
                    drawDatas.Add(new(texture, rectangle.TopLeft() + new Vector2(i * size.X, j * size.Y), new Rectangle(210, 0, (int)width, (int)height), Color.White, 0, default, size / 40f * 1.025f, 0, 0));
                }
            }
        }
        public static Color ModifyHueByRandom(Color color, float range)
        {
            var alpha = color.A;
            var vec = Main.rgbToHsl(color);
            vec.X += Main.rand.NextFloat(-range, range);
            while (vec.X < 0) vec.X++;
            vec.X %= 1;
            return Main.hslToRgb(vec) with { A = alpha };
        }
        #region 背景
        /// <summary>
        /// 指定背景贴图，为null的时候使用默认背景
        /// </summary>
        public Texture2D backgroundTexture;
        public virtual Texture2D StyleTexture { get; set; }
        /// <summary>
        /// 指定贴图背景的部分，和绘制那边一个用法
        /// </summary>
        public Rectangle? backgroundFrame;
        /// <summary>
        /// 单位大小，最后是进行平铺的
        /// </summary>
        public Vector2 backgroundUnitSize;
        /// <summary>
        /// 颜色，可以试试半透明的，很酷
        /// </summary>
        public Color backgroundColor;
        #endregion

        #region 边框
        /// <summary>
        /// 指定横向边界数
        /// </summary>
        public int? xBorderCount;
        /// <summary>
        /// 指定纵向边界数
        /// </summary>
        public int? yBorderCount;
        /// <summary>
        /// 外发光颜色
        /// </summary>
        public Color glowEffectColor;
        /// <summary>
        /// 外发光震动剧烈程度
        /// </summary>
        public float glowShakingStrength;
        /// <summary>
        /// 外发光色调偏移范围
        /// </summary>
        public float glowHueOffsetRange;
        #endregion

        #region 全局
        public Color mainColor;
        public Vector2 origin;
        public float scaler = 1f;
        public Vector2 offset;
        public Rectangle destination;
        #endregion

        public Rectangle ModifiedRectangle
        {
            get
            {
                Vector2 size = destination.Size() * scaler;
                //Vector2 topLeft = (origin - destination.TopLeft()) * scaler + offset;
                Vector2 topLeft = origin * (1 - scaler) + destination.TopLeft() + offset;
                return VectorsToRectangle(topLeft, size);
            }
        }
        public static Rectangle VectorsToRectangle(Vector2 topLeft, Vector2 size)
        {
            return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.X, (int)size.Y);
        }
        public ComplexPanelInfo()
        {
            mainColor = Color.White;
        }
        public virtual Rectangle DrawComplexPanel(SpriteBatch spriteBatch)
        {
            var rectangle = ModifiedRectangle;
            #region 参数准备
            //ConfigElement.DrawPanel2(spriteBatch, rectangle.TopLeft(), TextureAssets.SettingsPanel.Value, rectangle.Width, rectangle.Height, color);
            Vector2 center = rectangle.Center();
            Vector2 scalerVec = rectangle.Size() / new Vector2(64);
            var clampVec = Vector2.Clamp(scalerVec, default, Vector2.One);
            bool flagX = scalerVec.X == clampVec.X;
            bool flagY = scalerVec.Y == clampVec.Y;
            Texture2D texture = StyleTexture;
            float left = flagX ? center.X : rectangle.X + 32;
            float top = flagY ? center.Y : rectangle.Y + 32;
            float right = flagX ? center.X : rectangle.X + rectangle.Width - 32;
            float bottom = flagY ? center.Y : rectangle.Y + rectangle.Height - 32;
            #endregion
            #region 背景
            //spriteBatch.Draw(texture, rectangle, new Rectangle(210, 0, 40, 40), Color.White);
            if (backgroundTexture != null)
            {
                DrawComplexPanel_BackGround(spriteBatch, backgroundTexture, rectangle, backgroundFrame ?? new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height), backgroundUnitSize * scaler, backgroundColor);

            }
            else
            {
                DrawComplexPanel_BackGround(spriteBatch, texture, rectangle, new Vector2(40 * scaler));
            }
            #endregion
            #region 四个边框
            DrawComplexPanel_Bound(spriteBatch, texture, new Vector2(left - 28 * clampVec.X, center.Y), rectangle.Height - 24, clampVec.X, -MathHelper.PiOver2, glowEffectColor, glowShakingStrength, yBorderCount, glowHueOffsetRange);
            DrawComplexPanel_Bound(spriteBatch, texture, new Vector2(right + 28 * clampVec.X, center.Y), rectangle.Height - 24, clampVec.X, MathHelper.PiOver2, glowEffectColor, glowShakingStrength, yBorderCount, glowHueOffsetRange);
            DrawComplexPanel_Bound(spriteBatch, texture, new Vector2(center.X, top - 28 * clampVec.Y), rectangle.Width - 24, clampVec.Y, 0, glowEffectColor, glowShakingStrength, xBorderCount, glowHueOffsetRange);
            DrawComplexPanel_Bound(spriteBatch, texture, new Vector2(center.X, bottom + 28 * clampVec.Y), rectangle.Width - 24, clampVec.Y, MathHelper.Pi, glowEffectColor, glowShakingStrength, xBorderCount, glowHueOffsetRange);
            #endregion
            #region 四个角落
            spriteBatch.Draw(texture, new Vector2(left, top), new Rectangle(0, 0, 40, 40), Color.White, 0, new Vector2(40), clampVec, 0, 0);
            spriteBatch.Draw(texture, new Vector2(left, bottom), new Rectangle(42, 0, 40, 40), Color.White, 0, new Vector2(40, 0), clampVec, SpriteEffects.FlipVertically, 0);
            spriteBatch.Draw(texture, new Vector2(right, bottom), new Rectangle(42, 0, 40, 40), Color.White, MathHelper.Pi, new Vector2(40), clampVec, 0, 0);
            spriteBatch.Draw(texture, new Vector2(right, top), new Rectangle(42, 0, 40, 40), Color.White, 0, new Vector2(0, 40), clampVec, SpriteEffects.FlipHorizontally, 0);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(left, top), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 4f, 0, 0);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(960, 560), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 4f, 0, 0);

            #endregion
            return rectangle;
        }
        /*
        public virtual Rectangle DrawComplexPanel(List<DrawDataBuffer> drawDatas)
        {
            var rectangle = ModifiedRectangle;
            #region 参数准备
            //ConfigElement.DrawPanel2(spriteBatch, rectangle.TopLeft(), TextureAssets.SettingsPanel.Value, rectangle.Width, rectangle.Height, color);
            Vector2 center = rectangle.Center();
            Vector2 scalerVec = rectangle.Size() / new Vector2(64);
            var clampVec = Vector2.Clamp(scalerVec, default, Vector2.One);
            bool flagX = scalerVec.X == clampVec.X;
            bool flagY = scalerVec.Y == clampVec.Y;
            Texture2D texture = StyleTexture;
            float left = flagX ? center.X : rectangle.X + 32;
            float top = flagY ? center.Y : rectangle.Y + 32;
            float right = flagX ? center.X : rectangle.X + rectangle.Width - 32;
            float bottom = flagY ? center.Y : rectangle.Y + rectangle.Height - 32;
            #endregion
            #region 背景
            //spriteBatch.Draw(texture, rectangle, new Rectangle(210, 0, 40, 40), Color.White);
            if (backgroundTexture != null)
            {
                DrawComplexPanel_BackGround(drawDatas, backgroundTexture, rectangle, backgroundFrame ?? new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height), backgroundUnitSize * scaler, backgroundColor);

            }
            else
            {
                DrawComplexPanel_BackGround(drawDatas, texture, rectangle, new Vector2(40 * scaler));
            }
            #endregion
            #region 四个边框
            DrawComplexPanel_Bound(drawDatas, texture, new Vector2(left - 28 * clampVec.X, center.Y), rectangle.Height - 24, clampVec.X, -MathHelper.PiOver2, glowEffectColor, glowShakingStrength, yBorderCount, glowHueOffsetRange);
            DrawComplexPanel_Bound(drawDatas, texture, new Vector2(right + 28 * clampVec.X, center.Y), rectangle.Height - 24, clampVec.X, MathHelper.PiOver2, glowEffectColor, glowShakingStrength, yBorderCount, glowHueOffsetRange);
            DrawComplexPanel_Bound(drawDatas, texture, new Vector2(center.X, top - 28 * clampVec.Y), rectangle.Width - 24, clampVec.Y, 0, glowEffectColor, glowShakingStrength, xBorderCount, glowHueOffsetRange);
            DrawComplexPanel_Bound(drawDatas, texture, new Vector2(center.X, bottom + 28 * clampVec.Y), rectangle.Width - 24, clampVec.Y, MathHelper.Pi, glowEffectColor, glowShakingStrength, xBorderCount, glowHueOffsetRange);
            #endregion
            #region 四个角落
            drawDatas.Add(new(texture, new Vector2(left, top), new Rectangle(0, 0, 40, 40), Color.White, 0, new Vector2(40), clampVec, 0, 0));
            drawDatas.Add(new(texture, new Vector2(left, bottom), new Rectangle(42, 0, 40, 40), Color.White, 0, new Vector2(40, 0), clampVec, SpriteEffects.FlipVertically, 0));
            drawDatas.Add(new(texture, new Vector2(right, bottom), new Rectangle(42, 0, 40, 40), Color.White, MathHelper.Pi, new Vector2(40), clampVec, 0, 0));
            drawDatas.Add(new(texture, new Vector2(right, top), new Rectangle(42, 0, 40, 40), Color.White, 0, new Vector2(0, 40), clampVec, SpriteEffects.FlipHorizontally, 0));
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(left, top), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 4f, 0, 0);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(960, 560), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 4f, 0, 0);

            #endregion
            return rectangle;
        }
        */
    }
}

using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.ComplexPanel;

public partial class ComplexPanelInfo
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

    public static Rectangle VectorsToRectangle(Vector2 topLeft, Vector2 size)
    {
        return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.X, (int)size.Y);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.ComplexPanel;

public partial class ComplexPanelInfo
{
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
}

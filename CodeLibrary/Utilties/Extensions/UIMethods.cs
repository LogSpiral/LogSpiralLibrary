using Terraria.UI;
using static Terraria.Utils;
namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
public static class UIMethods
{
    public static bool BelongToMe(this UIElement element, UIElement target)
    {
        if (element.GetHashCode() == target.GetHashCode()) return true;
        if (element.Elements.Count == 0) return false;
        if (element.Elements.Contains(target))
            return true;
        foreach (var e in element.Elements)
        {
            if (e.BelongToMe(target))
                return true;
        }
        return false;
    }
    /// <summary>
    /// 绘制鼠标在某矩形下的悬浮字
    /// </summary>
    /// <param name="text">文本</param>
    /// <param name="color1">内色</param>
    /// <param name="color2">边框色</param>
    /// <param name="X">X坐标</param>
    /// <param name="Y">Y坐标</param>
    /// <param name="Width">矩形宽</param>
    /// <param name="Hegiht">矩形高</param>
    public static void DrawMouseTextOnRectangle(string text, Color color1, Color color2, int X, int Y, int Width, int Hegiht)
    {
        Vector2 mountedCenter = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);
        if (new Rectangle((int)mountedCenter.X, (int)mountedCenter.Y, 0, 0).Intersects(new Rectangle((int)(X + Main.screenPosition.X), (int)(Y + Main.screenPosition.Y), Width, Hegiht)))
        {
            string name = text;
            Vector2 worldPos = new(mountedCenter.X + 15, mountedCenter.Y + 15);
            Vector2 size = FontAssets.MouseText.Value.MeasureString(name);
            Vector2 texPos = worldPos + new Vector2(-size.X * 0.5f, name.Length) - Main.screenPosition;
            DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value, name, texPos.X, texPos.Y, color1, color2, Vector2.Zero);
        }

    }

    /// <summary>
    /// 缩放修复（这公式自己测的，没有游戏依据）
    /// 将屏幕坐标转换为UI坐标
    /// </summary>
    public static Vector2 TransformToUIPosition(Vector2 vector)
    {
        // 获取相对屏幕中心的向量(一定要在调节xy前获取)
        float oppositeX = (vector.X - Main.screenWidth / 2) / Main.UIScale;
        float oppositeY = (vector.Y - Main.screenHeight / 2) / Main.UIScale;
        vector.X = (int)(vector.X / Main.UIScale) + (int)(oppositeX * (Main.GameZoomTarget - 1f));
        vector.Y = (int)(vector.Y / Main.UIScale) + (int)(oppositeY * (Main.GameZoomTarget - 1f));
        return new(vector.X, vector.Y);
    }

    public static Vector2 MouseScreenUI => TransformToUIPosition(Main.MouseScreen);
    public static Vector2 GetSize(this UIElement uie) => uie.GetDimensions().ToRectangle().Size();
    public static UIElement SetSize(this UIElement uie, Vector2 size, float precentWidth = 0, float precentHeight = 0)
    {
        uie.SetSize(size.X, size.Y, precentWidth, precentHeight);
        return uie;
    }
    public static UIElement SetSize(this UIElement uie, float width, float height, float precentWidth = 0, float precentHeight = 0)
    {
        uie.Width.Set(width, precentWidth);
        uie.Height.Set(height, precentHeight);
        return uie;
    }
}
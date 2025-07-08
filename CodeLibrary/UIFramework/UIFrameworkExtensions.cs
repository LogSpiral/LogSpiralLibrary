using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using Terraria.UI;

namespace LogSpiralLibrary.CodeLibrary.UIFramework;

public static class UIFrameworkExtensions
{

    extension(in Size size)
    {
        public Vector2 ToVector2() => new(size.Width, size.Height);
    }

    extension(in Bounds bounds)
    {
        public CalculatedStyle ToDimension() => new(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
        public Rectangle ToRectangle() => new((int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height);
    }


    extension(UIView uiView)
    {
        public CalculatedStyle Dimension => uiView.GetBounds().ToDimension();
        public Vector2 Size => uiView.GetBounds().Size.ToVector2();
        public Vector2 Position => uiView.GetBounds().Position;

    }
}

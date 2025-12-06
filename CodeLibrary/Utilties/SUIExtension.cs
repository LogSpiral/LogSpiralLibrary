using SilkyUIFramework;
using SilkyUIFramework.Elements;

namespace LogSpiralLibrary.CodeLibrary.Utilties;

// [JITWhenModsEnabled("SilkyUIFramework")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
public static class SUIExtension
{
	extension(UIView view)
	{
		public Vector2 GetOuterBoundPercentedCoord(Vector2 percent) => view.OuterBounds.GetPercentedCoord(percent);

		public Vector2 GetInnerBoundPercentedCoord(Vector2 percent) => view.InnerBounds.GetPercentedCoord(percent);

		public Vector2 GetBoundPercentedCoord(Vector2 percent) => view.Bounds.GetPercentedCoord(percent);

		public Vector2 GetOuterBoundPercentedCoord(float percentX, float percentY) => view.OuterBounds.GetPercentedCoord(percentX, percentY);

		public Vector2 GetInnerBoundPercentedCoord(float percentX, float percentY) => view.InnerBounds.GetPercentedCoord(percentX, percentY);

		public Vector2 GetBoundPercentedCoord(float percentX, float percentY) => view.Bounds.GetPercentedCoord(percentX, percentY);
	}
	extension(UIElementGroup elementGroup)
	{
		/// <summary>
		/// 在给定子元素前插入子元素
		/// </summary>
		/// <param name="child"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public int AddBefore(UIView child, UIView target)
		{
			var idx = elementGroup.IndexOf(target);
			if (idx != -1)
				elementGroup.AddChild(child, idx);
			return idx;
		}
	}
	extension(Bounds bounds)
	{
		public Vector2 GetPercentedCoord(Vector2 percent) => bounds.Position + (Vector2)bounds.Size * percent;

		public Vector2 GetPercentedCoord(float percentX, float percentY) => bounds.Position + (Vector2)bounds.Size * new Vector2(percentX, percentY);

		public Vector2 LeftCenter => bounds.GetPercentedCoord(0, .5f);

		public Vector2 RightCenter => bounds.GetPercentedCoord(1, .5f);

		public Vector2 TopCenter => bounds.GetPercentedCoord(.5f, 0);

		public Vector2 BottomCenter => bounds.GetPercentedCoord(.5f, 1f);

		public float Top => bounds.Y;

		public float Left => bounds.X;

		public Vector2 LeftBottom => bounds.Position + new Vector2(0, bounds.Height);

		public Vector2 LeftTop => bounds.Position;

		public Vector2 RightBottom => bounds.Position + bounds.Size;

		public Vector2 RightTop => bounds.Position + new Vector2(bounds.Width, 0);
	}
}

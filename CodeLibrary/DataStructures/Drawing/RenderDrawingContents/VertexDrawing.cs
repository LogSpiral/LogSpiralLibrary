using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using static LogSpiralLibrary.LogSpiralLibraryMod;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;

/// <summary>
/// 非常抽象的顶点绘制用结构体
/// </summary>
public abstract class VertexDrawInfo : RenderDrawingContent
{
    public void DrawPrimitives(float distortScaler) => Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, DrawingMethods.CreateTriList(VertexInfos, center, distortScaler, true, !distortScaler.Equals(1.0f)), 0, VertexInfos.Length - 2);

    public override void Draw(SpriteBatch spriteBatch)
    {
#if false
        // 调试用，显示顶点框架，如果需要启用就把false改成true
        int m = VertexInfos.Length;
        for (int n = 0; n < m - 1; n++)
            spriteBatch.DrawLine(VertexInfos[n].Position, VertexInfos[n + 1].Position, Color.White);
#endif
        var targetTex = weaponTex ?? TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value;
        Main.graphics.GraphicsDevice.Textures[2] = targetTex;

        Main.graphics.GraphicsDevice.Textures[3] = heatMap;
        var swooshUL = ShaderSwooshUL;
        swooshUL.Parameters["uSize"].SetValue(targetTex.Size());
        if (frame != null)
        {
            Rectangle uframe = frame.Value;
            Vector2 size = (weaponTex ?? TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value).Size();
            swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(uframe.TopLeft() / size, uframe.Width / size.X, uframe.Height / size.Y));
        }
        else
            swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(0, 0, 1, 1));
        float dS = swooshUL.Parameters["distortScaler"].GetValueSingle();
        if (dS == 0)
        {
            dS = 1;
            swooshUL.Parameters["distortScaler"].SetValue(1);
        }
        // swooshUL.CurrentTechnique.Passes[dS.Equals(1.0f) ? 7 : 0].Apply();
        swooshUL.CurrentTechnique.Passes[7].Apply();
        DrawPrimitives(dS);
    }

    /// <summary>
    /// 中心偏移
    /// </summary>
    public Vector2 center;

    /// <summary>
    /// 热度/采样图
    /// </summary>
    public Texture2D heatMap;

    public Texture2D weaponTex;
    public Rectangle? frame;

    /// <summary>
    /// 缩放大小
    /// </summary>
    public float scaler;

    public int aniTexIndex;

    public int baseTexIndex;

    /// <summary>
    /// 顶点数据，存起来不每帧更新降低运算负担
    /// </summary>
    public abstract CustomVertexInfo[] VertexInfos { get; }
}
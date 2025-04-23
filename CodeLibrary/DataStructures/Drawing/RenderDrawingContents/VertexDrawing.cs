using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Main.graphics.GraphicsDevice.Textures[2] = weaponTex ?? TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value;
        Main.graphics.GraphicsDevice.Textures[3] = heatMap;
        var swooshUL = ShaderSwooshUL;
        swooshUL.Parameters["lightShift"].SetValue(factor - 1f);
        if (frame != null)
        {
            Rectangle uframe = frame.Value;
            Vector2 size = (weaponTex ?? TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value).Size();
            swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(uframe.TopLeft() / size, uframe.Width / size.X, uframe.Height / size.Y));
        }
        else
            swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(0, 0, 1, 1));
        float dS = swooshUL.Parameters["distortScaler"].GetValueSingle();
        swooshUL.CurrentTechnique.Passes[dS.Equals(1.0f) ? 7 : 0].Apply();//
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
    /// 颜色插值
    /// </summary>
    public Func<float, Color> color;

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




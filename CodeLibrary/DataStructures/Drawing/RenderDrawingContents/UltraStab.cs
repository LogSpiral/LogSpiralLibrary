using static LogSpiralLibrary.LogSpiralLibraryMod;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
public class UltraStab : MeleeVertexInfo
{
    const bool usePSShaderTransform = true;
    
    #region 参数和属性

    readonly CustomVertexInfo[] _vertexInfos = new CustomVertexInfo[usePSShaderTransform ? 4 : 90];
    public override CustomVertexInfo[] VertexInfos => _vertexInfos;

    #endregion

    #region 生成函数

    public static UltraStab NewUltraStab(string canvasName,int timeLeft,float scaler,Vector2 center) 
    {
        var content = new UltraStab();
        content.timeLeft = content.timeLeftMax = timeLeft;
        content.scaler = scaler;
        content.center = center;
        content.aniTexIndex = 9;
        content.baseTexIndex = 0;
        RenderCanvasSystem.AddRenderDrawingContent(canvasName, content);

        return content;
    }

    public static UltraStab NewUltraStabOnDefaultCanvas(int timeLeft, float scaler, Vector2 center)
        => NewUltraStab(RenderCanvasSystem.DEFAULTCANVASNAME, timeLeft, scaler, center);

    #endregion

    #region 绘制和更新，主体

    public override void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        base.PreDraw(spriteBatch, graphicsDevice);
        ShaderSwooshUL.Parameters["stab"].SetValue(usePSShaderTransform);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Main.graphics.GraphicsDevice.Textures[0] = BaseTex_Stab[baseTexIndex].Value;
        Main.graphics.GraphicsDevice.Textures[1] = AniTex_Stab[aniTexIndex].Value;
        base.Draw(spriteBatch);
    }

    public override void Update()
    {
        if (!autoUpdate)
        {
            autoUpdate = true;
            return;
        }
        if (usePSShaderTransform)
        {
            if (OnSpawn)
            {
                var realColor = Color.White;
                //Vector2 offsetVec = 20f * new Vector2(8, 3 / xScaler) * scaler;
                Vector2 offsetVec = new Vector2(1, 3 / xScaler / 8) * scaler;

                if (negativeDir) offsetVec.Y *= -1;
                VertexInfos[0] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 1, 1f));
                VertexInfos[2] = new CustomVertexInfo(center + (offsetVec with { X = 0 }).RotatedBy(rotation), realColor, new Vector3(1, 1, 1f));
                offsetVec.Y *= -1;
                VertexInfos[1] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 0, 1f));
                VertexInfos[3] = new CustomVertexInfo(center + (offsetVec with { X = 0 }).RotatedBy(rotation), realColor, new Vector3(1, 0, 1f));
            }
            for (int n = 0; n < 4; n++)
                VertexInfos[n].Position += rotation.ToRotationVector2();
        }
        else
        {
            center += rotation.ToRotationVector2();
            for (int i = 0; i < 45; i++)
            {
                var f = i / 44f;
                var num = 1 - Factor;
                var realColor = Color.White;//color.Invoke(f);
                //realColor.A = (byte)((1 - f).HillFactor2(1) * 255);
                float width;
                var t = 8 * f;
                if (t < 1) width = t;
                else if (t < 2) width = .5f + 1 / (3 - t);
                else if (t < 3.5f) width = 0.66f + 5f / (t - 1) / 6f;
                else if (t < 5.5f) width = 1;
                else width = 2.6f + 52 / (5 * t - 60);
                Vector2 offsetVec = scaler / 8 * new Vector2(t, width / xScaler * 2);
                if (negativeDir) offsetVec.Y *= -1;
                VertexInfos[2 * i] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(1 - f, 1, 1));
                offsetVec.Y *= -1;
                VertexInfos[2 * i + 1] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 0, 1));
            }
        }

        timeLeft--;
    }

    #endregion
}

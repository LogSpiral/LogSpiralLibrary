namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;

public abstract class MeleeVertexInfo : VertexDrawInfo
{
    public float rotation;
    public float xScaler = 1f;
    public bool negativeDir;
    public bool gather = true;

    /// <summary>
    /// 用来左乘颜色矩阵的系数向量
    /// <br>x:方向渐变</br>
    /// <br>y:武器对角线</br>
    /// <br>z:热度图</br>
    /// </summary>
    public Vector3 ColorVector;

    public bool normalize;
    public float alphaFactor = 2f;
    public float heatRotation;

    public override void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        base.PreDraw(spriteBatch, graphicsDevice);
        Effect effect = LogSpiralLibraryMod.ShaderSwooshUL;
        effect.Parameters["uTransform"].SetValue(RenderCanvasSystem.uTransform);
        effect.Parameters["uTime"].SetValue(-(float)GlobalTimeSystem.GlobalTime * 0.03f);
        effect.Parameters["checkAir"].SetValue(false);
        effect.Parameters["airFactor"].SetValue(2);
        effect.Parameters["heatRotation"].SetValue(Matrix.CreateRotationZ(heatRotation));
        effect.Parameters["lightShift"].SetValue(0f);
        // effect.Parameters["distortScaler"].SetValue(1f);
        effect.Parameters["alphaFactor"].SetValue(alphaFactor);
        effect.Parameters["heatMapAlpha"].SetValue(true);
        effect.Parameters["stab"].SetValue(false);
        effect.Parameters["alphaOffset"].SetValue(0f);
        //if (flag)
        //    effect.Parameters["AlphaVector"].SetValue(ConfigurationUltraTest.ConfigSwooshUltraInstance.AlphaVector);
        var sampler = SamplerState.AnisotropicWrap;
        Main.graphics.GraphicsDevice.SamplerStates[0] = sampler;
        Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
        Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
        Main.graphics.GraphicsDevice.SamplerStates[3] = SamplerState.AnisotropicWrap;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Effect effect = LogSpiralLibraryMod.ShaderSwooshUL;
        effect.Parameters["gather"].SetValue(gather);
        if (heatMap == null)
            ColorVector = new Vector3(0, 1, 0);
        effect.Parameters["AlphaVector"].SetValue(ColorVector);
        effect.Parameters["normalize"].SetValue(normalize);
        effect.Parameters["heatRotation"].SetValue(Matrix.CreateRotationZ(heatRotation));
        effect.Parameters["alphaFactor"].SetValue(alphaFactor);
        effect.Parameters["lightShift"].SetValue(Factor - 1f);
        base.Draw(spriteBatch);
    }
}
using System.Collections.Generic;
using System.Linq;
using Terraria.UI;
using static Terraria.Utils;
using static LogSpiralLibrary.LogSpiralLibraryMod;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
/// <summary>
/// <para>绘制辅助函数</para>
/// <para>包括但不限于</para>
/// <para>直接绘制内容的绘制函数</para>
/// <para>处理颜色或者获取相关属性的函数</para>
/// <para>还有一堆乱七八糟别的东西</para>
/// </summary>
public static class DrawingMethods
{
    //三个ReBegin来自更好的体验ImproveGame
    /// <summary>
    /// 仅改变SpriteSortMode和BlendState
    /// </summary>
    public static void ReBegin(this SpriteBatch sb, SpriteSortMode mode, BlendState blendState)
    {
        var matrix = sb.transformMatrix;
        var effect = sb.customEffect;

        sb.End();
        sb.Begin(mode, blendState, sb.GraphicsDevice.SamplerStates[0],
            sb.GraphicsDevice.DepthStencilState, sb.GraphicsDevice.RasterizerState, effect, matrix);
    }

    /// <summary>
    /// 仅改变SpriteSortMode
    /// </summary>
    public static void ReBegin(this SpriteBatch sb, SpriteSortMode mode)
    {
        var matrix = sb.transformMatrix;
        var effect = sb.customEffect;

        sb.End();
        sb.Begin(mode, sb.GraphicsDevice.BlendState, sb.GraphicsDevice.SamplerStates[0],
            sb.GraphicsDevice.DepthStencilState, sb.GraphicsDevice.RasterizerState, effect, matrix);
    }

    /// <summary>
    /// 不会改变原有的所有参数, 但是只能使用只有一个 PixelShader 的 Shader.
    /// </summary>
    public static void ReBegin(this SpriteBatch sb, Effect effect, Matrix matrix)
    {
        sb.End();
        // SpriteSortMode 精灵排序模式
        // BlendState 混合状态
        // SamplerState 采样器状态
        // DepthStencilState 深度模板状态
        // RasterizerState 光栅化状态 (目前知道的的作用是裁剪 Begin-End 所有贴图)
        // Effect Shader 特效
        // Matrix 矩阵 (用于控制整体的放大缩小)
        // 现在 Begin 的参数都不变, 因为不需要做出修改!
        sb.Begin(0, sb.GraphicsDevice.BlendState, sb.GraphicsDevice.SamplerStates[0],
            sb.GraphicsDevice.DepthStencilState, sb.GraphicsDevice.RasterizerState, effect, matrix);
    }
    public static void ApplyStdValueToVtxEffect(this MeleeVertexInfo info, StandardInfo std)
    {
        info.frame = std.frame;
        info.heatRotation = std.VertexStandard.heatRotation;
        info.alphaFactor = std.VertexStandard.alphaFactor;
        info.weaponTex = TextureAssets.Item[std.itemType].Value;
    }


    #region 直接用来绘制的b崽子们

    public static void DrawStarLight(this SpriteBatch spriteBatch, float Rotation, Vector2 center, Color color, float range, float scale = 1f, float alpha = .5f)
    {
        if (scale < 1) scale = 1;
        int max = (int)Math.Ceiling(3f * scale);
        for (int k = 0; k < max; k++)
        {
            float randValue = Main.rand.NextFloat();
            float amount = GetLerpValue(0f, 0.3f, randValue, true) * GetLerpValue(1f, 0.5f, randValue, true);//由此可以实现一个梯形的插值函数
            float size = MathHelper.Lerp(0.6f, 1f, amount);
            Texture2D starLight = TextureAssets.Projectile[927].Value;
            float num1 = (scale - 1f) / 2f;
            float num2 = Main.rand.NextFloat() * 2f * scale;
            num2 += num1;
            Vector2 scalerVector = new Vector2((2.8f + num2 * (1f + num1)) * range / 200f, 1f) * size;
            float angleRange = 0.03f - k * 0.012f;
            angleRange /= scale;
            float randLength = range / 4f + MathHelper.Lerp(0f, 50f * scale, randValue) + num2 * 16f;
            float rotation = Rotation + Main.rand.NextFloatDirection() * MathHelper.TwoPi * angleRange;
            center += Rotation.ToRotationVector2() * 2 + rotation.ToRotationVector2() * randLength + Main.rand.NextVector2Circular(20f, 20f) - Main.screenPosition;
            if (k > 0)
                center += Rotation.ToRotationVector2() * 56;
            Vector2 origin = starLight.Size() / 2f;
            Color mainColor = color with { A = 0 } * 4f * alpha;
            Color whiteLight = Color.White with { A = 0 } * amount * alpha;
            spriteBatch.Draw(starLight, center, null, mainColor, rotation, origin, scalerVector, 0, 0f);
            spriteBatch.Draw(starLight, center, null, whiteLight, rotation, origin, scalerVector * 0.6f, 0, 0f);
        }
    }
    public static void DrawHorizonBLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float scale = 1f, float width = 4f, int counts = 20)
    {
        Vector2[] vecs = new Vector2[counts];
        for (int n = 0; n < counts; n++)
        {

            //vecs[n] = Vector2.CatmullRom(new Vector2(MathHelper.Lerp(start.X,end.X,-scale), start.Y) , start, end, new Vector2(MathHelper.Lerp(end.X, start.X, -scale), end.Y) * new Vector2(scale, 1), n / (counts - 1f));
            vecs[n] = Vector2.Hermite(start, (end - start) * Vector2.UnitX * scale, end, (end - start) * Vector2.UnitX * scale, n / (counts - 1f));
        }

        for (int n = 0; n < counts - 1; n++)
            spriteBatch.DrawLine(vecs[n], vecs[n + 1], color, width);
    }
    public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color, float width = 4f)
    {
        spriteBatch.DrawLine(rectangle.TopLeft(), rectangle.TopRight(), color, width);
        spriteBatch.DrawLine(rectangle.TopLeft(), rectangle.BottomLeft(), color, width);
        spriteBatch.DrawLine(rectangle.BottomRight(), rectangle.TopRight(), color, width);
        spriteBatch.DrawLine(rectangle.BottomLeft(), rectangle.BottomRight(), color, width);
    }
    //不好用，用新的
    //    public static void DrawShaderTail(SpriteBatch spriteBatch, Projectile projectile, ShaderTailTexture shaderTail = ShaderTailTexture.Fire, ShaderTailStyle tailStyle = ShaderTailStyle.Dust, float Width = 30, ShaderTailMainStyle shaderTailMainStyle = ShaderTailMainStyle.MiddleLine, Vector2 Offset = default, float alpha = 1, bool additive = false)
    //    {
    //        //这里有几个我自己定义的枚举类型
    //        //ShaderTailTexture这个对应的是颜色
    //        //tailStyle这个是弹幕的动态亮度贴图（？
    //        //shaderTailMainStyle这个是弹幕的静态亮度贴图（？
    //        //它们分别对应uImage0 uImage2 uImage1
    //        List<CustomVertexInfo> bars = new List<CustomVertexInfo>();

    //        // 把所有的点都生成出来，按照顺序
    //        for (int i = 1; i < projectile.oldPos.Length; ++i)
    //        {
    //            if (projectile.oldPos[i] == Vector2.Zero)
    //            {
    //                break;
    //            }
    //            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, projectile.oldPos[i] - Main.screenPosition,
    //            //    new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f, 0.5f), 5f, SpriteEffects.None, 0f);

    //            //int width = 30;
    //            var normalDir = projectile.oldPos[i - 1] - projectile.oldPos[i];
    //            normalDir = Vector2.Normalize(new Vector2(-normalDir.Y, normalDir.X));

    //            var factor = i / (float)projectile.oldPos.Length;
    //            var color = Color.Lerp(Color.White, Color.Red, factor);//后来发现底下那些if似乎没用（
    //            if (shaderTail == ShaderTailTexture.Frozen)
    //            {
    //                color = Color.Lerp(Color.White, Color.Blue, factor);
    //            }
    //            if (shaderTail == ShaderTailTexture.Yellow)
    //            {
    //                color = Color.Lerp(Color.White, Color.Yellow, factor);
    //            }
    //            if (shaderTail == ShaderTailTexture.White)
    //            {
    //                color = Color.Lerp(Color.Black, Color.White, factor);
    //            }
    //            var w = 1 - factor;
    //            bars.Add(new CustomVertexInfo(projectile.oldPos[i] + Offset + normalDir * Width, color, new Vector3((float)Math.Sqrt(factor), 1, w * alpha)));//这里还是在截图画图吧
    //            bars.Add(new CustomVertexInfo(projectile.oldPos[i] + Offset + normalDir * -Width, color, new Vector3((float)Math.Sqrt(factor), 0, w * alpha)));
    //        }

    //        List<CustomVertexInfo> triangleList = new List<CustomVertexInfo>();//这里是三角形的顶点

    //        if (bars.Count > 2)
    //        {

    //            // 按照顺序连接三角形
    //            triangleList.Add(bars[0]);//等腰直角三角形的底角1的顶点
    //            var vertex = new CustomVertexInfo((bars[0].Position + bars[1].Position) * 0.5f + Vector2.Normalize(projectile.velocity) * 30, Color.White,
    //                new Vector3(0, 0.5f, alpha));
    //            triangleList.Add(bars[1]);//底角2的顶点
    //            triangleList.Add(vertex);//顶角顶点
    //            for (int i = 0; i < bars.Count - 2; i += 2)
    //            {
    //                triangleList.Add(bars[i]);
    //                triangleList.Add(bars[i + 2]);
    //                triangleList.Add(bars[i + 1]);

    //                triangleList.Add(bars[i + 1]);
    //                triangleList.Add(bars[i + 2]);
    //                triangleList.Add(bars[i + 3]);
    //            }//每次消耗两个点生成新三角形


    //            spriteBatch.End();
    //            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    //            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
    //            // 干掉注释掉就可以只显示三角形栅格
    //            //RasterizerState rasterizerState = new RasterizerState();
    //            //rasterizerState.CullMode = CullMode.None;
    //            //rasterizerState.FillMode = FillMode.WireFrame;
    //            //Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;

    //            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
    //            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));//这个矩阵没仔细看，应该是负责把图像丢到三角形栅格中

    //            // 把变换和所需信息丢给shader
    //            IllusionBoundMod.DefaultEffect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
    //            IllusionBoundMod.DefaultEffect.Parameters["uTime"].SetValue(-(float)IllusionBoundMod.ModTime * 0.03f);//会动的那个贴图的横向偏移量(就是这个才能让它动起来Main.time
    //            Main.graphics.GraphicsDevice.Textures[0] = IllusionBoundMod.HeatMap[(int)shaderTail];
    //            Main.graphics.GraphicsDevice.Textures[1] = IllusionBoundMod.BaseTexes[(int)shaderTailMainStyle];
    //            Main.graphics.GraphicsDevice.Textures[2] = IllusionBoundMod.AniTexes[(int)tailStyle];
    //            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
    //            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
    //            Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
    //            //Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
    //            //Main.graphics.GraphicsDevice.Textures[1] = TextureAssets.MagicPixel.Value;
    //            //Main.graphics.GraphicsDevice.Textures[2] = TextureAssets.MagicPixel.Value;
    //            /*if (isCyan)
    //{
    //	IllusionBoundMod.CleverEffect.CurrentTechnique.Passes["Clever"].Apply();
    //}*/
    //            IllusionBoundMod.DefaultEffect.CurrentTechnique.Passes[0].Apply();


    //            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList.ToArray(), 0, triangleList.Count / 3);//连接三角形顶点

    //            Main.graphics.GraphicsDevice.RasterizerState = originalState;
    //            spriteBatch.End();
    //            spriteBatch.Begin(SpriteSortMode.Immediate, additive ? BlendState.Additive : BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    //        }
    //    }
    public static void DrawProjShadow(this SpriteBatch spriteBatch, Projectile projectile, Color lightColor)
    {
        Texture2D projectileTexture = TextureAssets.Projectile[projectile.type].Value;
        int frameHeight = projectileTexture.Height / Main.projFrames[projectile.type];
        for (int k = 0; k < projectile.oldPos.Length; k++)
        {
            Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + new Vector2(projectile.width, projectile.height) / 2 + new Vector2(1f, projectile.gfxOffY);
            Color color = projectile.GetAlpha(lightColor * 0.5f) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
            spriteBatch.Draw(projectileTexture, drawPos, new Rectangle(0, frameHeight * projectile.frame, projectileTexture.Width, frameHeight), color, projectile.rotation, new Vector2(TextureAssets.Projectile[projectile.type].Value.Width * 0.5f, frameHeight * 0.5f), projectile.scale - 0.1f * k, SpriteEffects.None, 0f);
        }
    }
    public static void DrawStorm(Color c1, Projectile projectile, float MaxValue)
    {
        float num252 = 15f;
        float num253 = 15f;
        float num254 = projectile.ai[0];
        float scale8 = MathHelper.Clamp(num254 / 30f, 0f, 1f);
        if (num254 > MaxValue - 60f)
        {
            scale8 = MathHelper.Lerp(1f, 0f, (num254 - (MaxValue - 60f)) / 60f);
        }
        Point point6 = projectile.Center.ToTileCoordinates();
        Collision.ExpandVertically(point6.X, point6.Y, out int num255, out int num256, (int)num252, (int)num253);
        int num43 = num255;
        num255 = num43 + 1;
        num43 = num256;
        num256 = num43 - 1;
        float num257 = 0.2f;
        Vector2 vector50 = new Vector2(point6.X, num255) * 16f + new Vector2(8f);
        Vector2 vector51 = new Vector2(point6.X, num256) * 16f + new Vector2(8f);
        Vector2.Lerp(vector50, vector51, 0.5f);
        Vector2 vector52 = new(0f, vector51.Y - vector50.Y);
        vector52.X = vector52.Y * num257;
        _ = new Vector2(vector50.X - vector52.X / 2f, vector50.Y);
        Texture2D texture2D29 = TextureAssets.Projectile[projectile.type].Value;
        Rectangle rectangle14 = texture2D29.Frame(1, 1, 0, 0);
        Vector2 origin6 = rectangle14.Size() / 2f;
        float num258 = -0.06283186f * num254;
        Vector2 unitY3 = Vector2.UnitY;
        double radians7 = (double)(num254 * 0.1f);
        Vector2 center = default;
        Vector2 vector53 = unitY3.RotatedBy(radians7, center);
        float num259 = 0f;
        float num260 = 5.1f;
        float xValue = projectile.velocity.X > 0 ? -16 : 0;
        for (float num261 = (int)vector51.Y; num261 > (int)vector50.Y; num261 -= num260)
        {
            num259 += num260;
            float num262 = num259 / vector52.Y;
            float num263 = num259 * 6.28318548f / -20f;
            float num264 = num262 - 0.15f;
            Vector2 spinningpoint5 = vector53;
            double radians8 = (double)num263;
            center = default;
            Vector2 vector54 = spinningpoint5.RotatedBy(radians8, center);
            Vector2 vector55 = new(0f, num262 + 1f);
            vector55.X = vector55.Y * num257;
            Color color49 = Color.Lerp(Color.Transparent, c1, num262 * 2f);
            if (num262 > 0.5f)
            {
                color49 = Color.Lerp(Color.Transparent, c1, 2f - num262 * 2f);
            }
            color49.A = (byte)(color49.A * 0.5f);
            color49 *= scale8;
            vector54 *= vector55 * 100f;
            vector54.Y = 0f;
            vector54.X = 0f;
            vector54 += new Vector2(vector51.X, num261) - Main.screenPosition;
            Main.spriteBatch.Draw(texture2D29, vector54 + new Vector2(projectile.Center.X % 16 + xValue, projectile.Center.Y % 16), new Microsoft.Xna.Framework.Rectangle?(rectangle14), color49, num258 + num263, origin6, 1f + num264, SpriteEffects.None, 0f);
        }
    }
    public static void DrawWind(Color c1, Color c2, Projectile projectile, float MaxValue)
    {
        float num266 = projectile.ai[0];
        float scale9 = MathHelper.Clamp(num266 / 30f, 0f, 1f);
        if (num266 > MaxValue - 60f)
        {
            scale9 = MathHelper.Lerp(1f, 0f, (num266 - (MaxValue - 60f)) / 60f);
        }
        float num267 = 0.2f;
        Vector2 top = projectile.Top;
        Vector2 bottom = projectile.Bottom;
        Vector2.Lerp(top, bottom, 0.5f);
        Vector2 vector56 = new(0f, bottom.Y - top.Y);
        vector56.X = vector56.Y * num267;
        _ = new Vector2(top.X - vector56.X / 2f, top.Y);
        Texture2D texture2D30 = TextureAssets.Projectile[projectile.type].Value;
        Rectangle rectangle15 = texture2D30.Frame(1, 1, 0, 0);
        Vector2 origin7 = rectangle15.Size() / 2f;
        float num268 = -0.157079637f * num266 * (projectile.velocity.X > 0f ? -1 : 1);
        SpriteEffects effects2 = projectile.velocity.X > 0f ? SpriteEffects.FlipVertically : SpriteEffects.None;
        bool flag25 = projectile.velocity.X > 0f;
        Vector2 unitY4 = Vector2.UnitY;
        double radians9 = (double)(num266 * 0.14f);
        Vector2 center = default;
        Vector2 vector57 = unitY4.RotatedBy(radians9, center);
        float num269 = 0f;
        float num270 = 5.01f + num266 / 150f * -0.9f;
        float xValue = projectile.velocity.X > 0f ? -1 : 1;
        if (num270 < 4.11f)
        {
            num270 = 4.11f;
        }
        float num271 = num266 % 60f;
        if (num271 < 30f)
        {
            //c2 *= Terraria.Utils.InverseLerp(22f, 30f, num271, true);
        }
        else
        {
            //c2 *= Terraria.Utils.InverseLerp(38f, 30f, num271, true);
        }
        bool flag26 = c2 != Color.Transparent;
        for (float num272 = (int)bottom.Y; num272 > (int)top.Y; num272 -= num270)
        {
            num269 += num270;
            float num273 = num269 / vector56.Y;
            float num274 = num269 * 6.28318548f / -20f;
            if (flag25)
            {
                num274 *= -1f;
            }
            float num275 = num273 - 0.35f;
            Vector2 spinningpoint6 = vector57;
            double radians10 = (double)num274;
            center = default;
            Vector2 vector58 = spinningpoint6.RotatedBy(radians10, center);
            Vector2 vector59 = new(0f, num273 + 1f);
            vector59.X = vector59.Y * num267;
            Color color51 = Color.Lerp(Color.Transparent, c1, num273 * 2f);
            if (num273 > 0.5f)
            {
                color51 = Color.Lerp(Color.Transparent, c1, 2f - num273 * 2f);
            }
            color51.A = (byte)(color51.A * 0.5f);
            color51 *= scale9;
            vector58 *= vector59 * 100f;
            vector58.Y = 0f;
            vector58.X = 0f;
            vector58 += new Vector2(bottom.X, num272) - Main.screenPosition;
            if (flag26)
            {
                Color color52 = Color.Lerp(Color.Transparent, c2, num273 * 2f);
                if (num273 > 0.5f)
                {
                    color52 = Color.Lerp(Color.Transparent, c2, 2f - num273 * 2f);
                }
                color52.A = (byte)(color52.A * 0.5f);
                color52 *= scale9;
                Main.spriteBatch.Draw(texture2D30, vector58 + new Vector2(16 * xValue, 0), new Microsoft.Xna.Framework.Rectangle?(rectangle15), color52, num268 + num274, origin7, (1f + num275) * 0.8f, effects2, 0f);
            }
            Main.spriteBatch.Draw(texture2D30, vector58 + new Vector2(16 * xValue, 0), new Microsoft.Xna.Framework.Rectangle?(rectangle15), color51, num268 + num274, origin7, 1f + num275, effects2, 0f);
        }
    }
    /// <summary>
    /// 大猿人战那里星空背景的，不如用render，建议放弃掉这货
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="vectors"></param>
    /// <param name="light"></param>
    [Obsolete]
    public static void DrawOutSide(this SpriteBatch spriteBatch, Vector2[] vectors, float light = 1)
    {
        return;
        /*
        #region Outside
        //FileStream fileStream = new FileStream(@"D:\\TestTesseract.txt", FileMode.OpenOrCreate, FileAccess.Write);
        //BinaryWriter binaryWriter = new BinaryWriter(fileStream);
        float left = Main.screenPosition.X;
        float right = Main.screenPosition.X + 1920;
        float bottom = Main.screenPosition.Y;
        float top = Main.screenPosition.Y + 1120;

        //binaryWriter.Write("PreEdge");
        var edgePointsNative = vectors.CloneArray();//.EdgePoints()
        //for (int n = 0; n < edgePointsNative.Length; n++)
        //{
        //    //Main.NewText((edgePoints[n], n));
        //    spriteBatch.DrawString(Main.fontMouseText, (edgePointsNative[n], n).ToString(), new Vector2(800, 300 + 24 * n), Color.Red);
        //}
        //binaryWriter.Write("PostEdge");
        //var lep = new LoopArray<Vector2>(edgePointsNative);
        //for (int n = 0; n < lep.Length; n++)
        //{
        //    spriteBatch.DrawLine(lep[n], lep[n + 1], Color.Red, 32, false, -Main.screenPosition);
        //}

        //binaryWriter.Write("PreVertex");

        Vector2[] targetPoints = GetVertexPoints(ref edgePointsNative);//edgePoints.GetVertexPoints();
        //binaryWriter.Write("PostVertex");

        if (targetPoints == null)
        {
            return;
        }

        if (targetPoints[0].X - 200 < left)
        {
            left = targetPoints[0].X - 200;
        }

        if (targetPoints[1].Y + 200 > top)
        {
            top = targetPoints[1].Y + 200;
        }

        if (targetPoints[2].X + 200 > right)
        {
            right = targetPoints[2].X + 200;
        }

        if (targetPoints[3].Y - 200 < bottom)
        {
            bottom = targetPoints[3].Y - 200;
        }

        //CustomVertexInfo[] vertexs = new CustomVertexInfo[edgePoints.Length + 4];
        //LoopArray<CustomVertexInfo> vertexs = new LoopArray<CustomVertexInfo>(new CustomVertexInfo[edgePointsNative.Length + 4]);
        LoopArray<CustomVertexInfo> vertexs = new LoopArray<CustomVertexInfo>(new CustomVertexInfo[edgePointsNative.Length]);
        LoopArray<CustomVertexInfo> vertexs2 = new LoopArray<CustomVertexInfo>(new CustomVertexInfo[4]);

        List<CustomVertexInfo> vertexInfos = new List<CustomVertexInfo>();
        //binaryWriter.Write("PreTri");

        var l = edgePointsNative.Length;
        for (int n = 0; n < l; n++)
        {
            vertexs[n] = edgePointsNative[n].VertexInScreen(Color.Cyan, light);
        }
        vertexs2[0] = new Vector2(left, top).VertexInScreen(Color.Cyan, light);
        vertexs2[1] = new Vector2(right, top).VertexInScreen(Color.Cyan, light);
        vertexs2[2] = new Vector2(right, bottom).VertexInScreen(Color.Cyan, light);
        vertexs2[3] = new Vector2(left, bottom).VertexInScreen(Color.Cyan, light);
        var connecttingVertex = vertexs2[3];
        List<int> indexList = new List<int>();

        for (int n = 0; n < l; n++)
        {
            int index = -1;
            for (int i = 0; i < 4; i++)
            {
                if (targetPoints[i] == edgePointsNative[n] && !indexList.Contains(i))
                {
                    index = i;
                    indexList.Add(i);
                    break;
                }
            }
            if (index == -1)
            {
                vertexInfos.Add(vertexs[n - 1]);
                vertexInfos.Add(vertexs[n]);
                vertexInfos.Add(connecttingVertex);
            }
            else
            {
                //vertexInfos.Add(connecttingVertex);
                //vertexInfos.Add(vertexs[n]);
                //connecttingVertex = vertexs[l + index];
                //vertexInfos.Add(connecttingVertex);
                vertexInfos.Add(vertexs[n - 1]);
                vertexInfos.Add(vertexs[n]);
                vertexInfos.Add(connecttingVertex);

                vertexInfos.Add(connecttingVertex);
                vertexInfos.Add(vertexs[n]);
                connecttingVertex = vertexs2[index];
                vertexInfos.Add(connecttingVertex);
            }
        }
        //binaryWriter.Write("PostTri");

        //spriteBatch.End();
        //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone);
        Effect effect = IllusionBoundMod.TextureEffect;//IllusionBoundMod.ShaderSwoosh
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["uTime"].SetValue(0);
        Main.graphics.GraphicsDevice.Textures[0] = IllusionBoundMod.GetTexture("Backgrounds/StarSky_0");//IllusionBoundMod.MaskColor[4]
        //Main.graphics.GraphicsDevice.Textures[1] = IllusionBoundMod.MaskColor[4];
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        //Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        //Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
        effect.CurrentTechnique.Passes[0].Apply();
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexInfos.ToArray(), 0, vertexInfos.Count / 3);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        //spriteBatch.End();
        //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        //binaryWriter.Flush();
        //binaryWriter.Close();
        //fileStream.Close();
        #endregion
        */
    }
    public static void DrawWhip(this Projectile proj)
    {
        List<Vector2> list = [];
        Projectile.FillWhipControlPoints(proj, list);
        Texture2D value = TextureAssets.FishingLine.Value;
        Rectangle value2 = value.Frame();
        Vector2 origin = new(value2.Width / 2, 2f);
        Color originalColor = Color.White;
        Vector2 value3 = list[0];
        for (int i = 0; i < list.Count - 1; i++)
        {
            Vector2 vector = list[i];
            Vector2 vector2 = list[i + 1] - vector;
            float rotation = vector2.ToRotation() - (float)Math.PI / 2f;
            Color color = Lighting.GetColor(vector.ToTileCoordinates(), originalColor);
            Vector2 scale = new(1f, (vector2.Length() + 2f) / value2.Height);
            Main.spriteBatch.Draw(value, value3 - Main.screenPosition, value2, color, rotation, origin, scale, SpriteEffects.None, 0f);
            value3 += vector2;
        }
        DrawWhip_WhipBland(proj, list);
    }
    public static Vector2 DrawWhip_WhipBland(Projectile proj, List<Vector2> controlPoints, Texture2D otherTex = null)
    {
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (proj.spriteDirection == 1)
            spriteEffects ^= SpriteEffects.FlipHorizontally;

        Texture2D value = otherTex ?? TextureAssets.Projectile[proj.type].Value;
        Rectangle rectangle = value.Frame(1, 5);
        int height = rectangle.Height;
        rectangle.Height -= 2;
        Vector2 vector = rectangle.Size() / 2f;
        Vector2 vector2 = controlPoints[0];
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector2 origin = vector;
            float scale = 1f;
            bool flag;
            if (i == 0)
            {
                origin.Y -= 4f;
                flag = true;
            }
            else
            {
                flag = true;
                int num = 1;
                if (i > 10)
                    num = 2;

                if (i > 20)
                    num = 3;

                rectangle.Y = height * num;
            }

            if (i == controlPoints.Count - 2)
            {
                flag = true;
                rectangle.Y = height * 4;
                Projectile.GetWhipSettings(proj, out float timeToFlyOut, out _, out _);
                float t = proj.ai[0] / timeToFlyOut;
                float amount = GetLerpValue(0.1f, 0.7f, t, clamped: true) * GetLerpValue(0.9f, 0.7f, t, clamped: true);
                scale = MathHelper.Lerp(0.5f, 1.5f, amount);
            }

            Vector2 vector3 = controlPoints[i];
            Vector2 vector4 = controlPoints[i + 1] - vector3;
            if (flag)
            {
                float rotation = vector4.ToRotation() - (float)Math.PI / 2f;
                Color color = Lighting.GetColor(vector3.ToTileCoordinates());
                Main.spriteBatch.Draw(value, vector2 - Main.screenPosition, rectangle, color, rotation, origin, scale, spriteEffects, 0f);
            }

            vector2 += vector4;
        }

        return vector2;
    }
    public static void VertexDraw(CustomVertexInfo[] vertexs, Texture2D baseTex, Texture2D aniTex, Texture2D heatMap = null, Vector2 uTime = default, bool trailing = false, Matrix? matrix = null, string pass = null, bool autoStart = true, bool autoComplete = true)
    {
        Effect effect = LogSpiralLibraryMod.VertexDraw;
        if (effect == null) return;
        SpriteBatch spriteBatch = Main.spriteBatch;
        if (autoStart)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        if (trailing)
        {
            List<CustomVertexInfo> triangleList = [];
            for (int i = 0; i < vertexs.Length - 2; i += 2)
            {
                triangleList.Add(vertexs[i]);
                triangleList.Add(vertexs[i + 2]);
                triangleList.Add(vertexs[i + 1]);

                triangleList.Add(vertexs[i + 1]);
                triangleList.Add(vertexs[i + 2]);
                triangleList.Add(vertexs[i + 3]);
            }
            vertexs = [.. triangleList];
        }

        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue((matrix ?? Matrix.Identity) * model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["uTimeX"].SetValue(uTime.X);
        effect.Parameters["uTimeY"].SetValue(uTime.Y);
        Main.graphics.GraphicsDevice.Textures[0] = baseTex;
        Main.graphics.GraphicsDevice.Textures[1] = aniTex;
        if (heatMap != null)
            Main.graphics.GraphicsDevice.Textures[2] = heatMap;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.AnisotropicWrap;
        Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.AnisotropicClamp;
        if (pass != null)
            effect.CurrentTechnique.Passes[pass].Apply();
        else
            effect.CurrentTechnique.Passes[0].Apply();
        //Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList.ToArray(), 0, triangleList.Count / 3);
        if (vertexs.Length > 2)
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexs, 0, vertexs.Length / 3);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        if (autoComplete)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    /// <summary>
    /// 高阶版本的顶点绘制函数！！
    /// </summary>
    /// <param name="vertexs">顶点信息，现在支持齐次坐标！</param>
    /// <param name="baseTex">基本的静止贴图</param>
    /// <param name="aniTex">实现一些动态效果的图</param>
    /// <param name="heatMap">采样图，需要特殊的pass</param>
    /// <param name="uTime">时间偏移量</param>
    /// <param name="trailing">是否为拖尾串，自动帮你连好</param>
    /// <param name="matrix">变换矩阵，3d绘制那边会用到</param>
    /// <param name="pass">字面意思</param>
    /// <param name="autoStart">如果你连着一大串用这个函数，那第一个这个为true就行，其它都false</param>
    /// <param name="autoComplete">最后一个true，其它false</param>
    public static void VertexDrawEX(CustomVertexInfoEX[] vertexs, Texture2D baseTex, Texture2D aniTex, Texture2D heatMap = null, Vector2 uTime = default, bool trailing = false, Matrix? matrix = null, string pass = null, bool autoStart = true, bool autoComplete = true)
    {
        Effect effect = LogSpiralLibraryMod.VertexDrawEX;
        if (effect == null) return;
        SpriteBatch spriteBatch = Main.spriteBatch;
        if (autoStart)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        if (trailing)
        {
            List<CustomVertexInfoEX> triangleList = [];
            for (int i = 0; i < vertexs.Length - 2; i += 2)
            {
                triangleList.Add(vertexs[i]);
                triangleList.Add(vertexs[i + 2]);
                triangleList.Add(vertexs[i + 1]);

                triangleList.Add(vertexs[i + 1]);
                triangleList.Add(vertexs[i + 2]);
                triangleList.Add(vertexs[i + 3]);
            }
            vertexs = [.. triangleList];
        }

        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        var transform = (matrix ?? Matrix.Identity) * model * Main.GameViewMatrix.TransformationMatrix * projection;
        effect.Parameters["uTransform"].SetValue(transform);
        effect.Parameters["uTimeX"].SetValue(uTime.X);
        effect.Parameters["uTimeY"].SetValue(uTime.Y);
        Main.graphics.GraphicsDevice.Textures[0] = baseTex;
        Main.graphics.GraphicsDevice.Textures[1] = aniTex;
        if (heatMap != null)
            Main.graphics.GraphicsDevice.Textures[2] = heatMap;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.AnisotropicWrap;
        Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.AnisotropicClamp;
        if (pass != null)
            effect.CurrentTechnique.Passes[pass].Apply();
        else
            effect.CurrentTechnique.Passes[0].Apply();
        //Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList.ToArray(), 0, triangleList.Count / 3);
        if (vertexs.Length > 2)

            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexs, 0, vertexs.Length / 3);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        if (autoComplete)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

    }

    /// <summary>
    /// 方便的弹幕拖尾
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="projectile">弹幕</param>
    /// <param name="heatMap">采样图</param>
    /// <param name="aniTex">动态图</param>
    /// <param name="baseTex">静态图</param>
    /// <param name="Width">宽度</param>
    /// <param name="Offset">偏移量</param>
    /// <param name="alpha">不透明度</param>
    /// <param name="VeloTri">是否有那个小三角</param>
    /// <param name="additive">是否为加法模式</param>
    /// <param name="mainColor">主体颜色</param>
    public static void DrawShaderTail(this SpriteBatch spriteBatch, Projectile projectile, Texture2D heatMap, Texture2D aniTex, Texture2D baseTex, float Width = 30, Vector2 Offset = default, float alpha = 1, bool VeloTri = false, bool additive = false, Color? mainColor = null)
    {
        var triangleList = projectile.TailVertexFromProj(Offset, Width, alpha, VeloTri, mainColor);//顶点信息准备
        if (triangleList.Length < 3) return;
        spriteBatch.End();//调用End以结束先前的绘制，将内容画下   第一个参数是立即绘制不缓存信息 第二个是混合模式 第三个采样模式 第四个深度状态?不熟 第五个不熟 第六个Effect，这里传null因为我们自己搞
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        //RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        //var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        //var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        //IllusionBoundMod.DefaultEffect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        //IllusionBoundMod.DefaultEffect.Parameters["uTime"].SetValue(-(float)IllusionBoundMod.ModTime * 0.03f);
        //Main.graphics.GraphicsDevice.Textures[0] = heatMap;
        //Main.graphics.GraphicsDevice.Textures[1] = baseTex;
        //Main.graphics.GraphicsDevice.Textures[2] = aniTex;
        //Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        //Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        //Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
        //IllusionBoundMod.DefaultEffect.CurrentTechnique.Passes[0].Apply();
        //Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList, 0, triangleList.Length / 3);
        //Main.graphics.GraphicsDevice.RasterizerState = originalState;
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.gameMenu ? Main.instance.Window.ClientBounds.Width : Main.screenWidth, Main.gameMenu ? Main.instance.Window.ClientBounds.Height : Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        var swooshUL = ShaderSwooshUL;
        swooshUL.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);//传入坐标变换矩阵，把世界坐标转化到[0,1]单位区间内(屏幕左上角到右下角)
                                                                                                                  //这里是依次右乘这些矩阵
                                                                                                                  //先是model，它的作用和-Main.screenPosition一样
                                                                                                                  //然后是Main.GameViewMatrix.TransformationMatrix，包括画面缩放翻转之类(我记得有翻转
                                                                                                                  //最后是projection，这个就是最后进行压缩区间的
        swooshUL.Parameters["uTime"].SetValue(-(float)(float)ModTime * 0.03f);
        swooshUL.Parameters["uLighter"].SetValue(0);
        swooshUL.Parameters["checkAir"].SetValue(false);
        swooshUL.Parameters["airFactor"].SetValue(1);
        swooshUL.Parameters["gather"].SetValue(false);
        swooshUL.Parameters["lightShift"].SetValue(0);
        swooshUL.Parameters["distortScaler"].SetValue(0);
        swooshUL.Parameters["alphaFactor"].SetValue(3f);
        swooshUL.Parameters["heatMapAlpha"].SetValue(true);
        swooshUL.Parameters["AlphaVector"].SetValue(new Vector3(0, 0, 1));
        swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(0, 0, 1, 1));
        Main.graphics.GraphicsDevice.Textures[0] = baseTex;//传入各种辅助贴图，对应.fx那边的sampler(s[n])
        Main.graphics.GraphicsDevice.Textures[1] = aniTex;
        Main.graphics.GraphicsDevice.Textures[2] = BaseTex[8].Value;
        Main.graphics.GraphicsDevice.Textures[3] = heatMap;

        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;//设置采样模式，Wrap就是模1，Clamp是夹在[0,1]
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;//不想写跳过就行，上面Begin那里已经设置过了
        Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[3] = SamplerState.PointClamp;

        ShaderSwooshUL.CurrentTechnique.Passes[7].Apply();//启用shader
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList, 0, triangleList.Length / 3);//传入绘制信息
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, additive ? BlendState.Additive : BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }

    /// <summary>
    /// 画好看的星星闪光
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="dir"></param>
    /// <param name="drawpos"></param>
    /// <param name="drawColor"></param>
    /// <param name="shineColor"></param>
    public static void DrawPrettyStarSparkle(this Projectile projectile, SpriteBatch spriteBatch, SpriteEffects dir, Vector2 drawpos, Color drawColor, Color shineColor)
    {
        Texture2D value = Misc[13].Value;
        Color color = shineColor * projectile.Opacity * 0.5f;
        color.A = 0;
        Vector2 origin = value.Size() / 2f;
        Color color2 = drawColor * 0.5f;
        float num = GetLerpValue(15f, 30f, projectile.localAI[0], true) * GetLerpValue(45f, 30f, projectile.localAI[0], true);
        Vector2 vector = new Vector2(0.5f, 5f) * num;
        Vector2 vector2 = new Vector2(0.5f, 2f) * num;
        color *= num;
        color2 *= num;
        spriteBatch.Draw(value, drawpos, null, color, 1.57079637f, origin, vector, dir, 0);
        spriteBatch.Draw(value, drawpos, null, color, 0f, origin, vector2, dir, 0);
        spriteBatch.Draw(value, drawpos, null, color2, 1.57079637f, origin, vector * 0.6f, dir, 0);
        spriteBatch.Draw(value, drawpos, null, color2, 0f, origin, vector2 * 0.6f, dir, 0);
    }
    /// <summary>
    /// 画好看的星星拖尾
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="drawColor"></param>
    /// <param name="projectileColor"></param>
    public static void DrawProjWithStarryTrail(this Projectile projectile, SpriteBatch spriteBatch, float drawColor, Color projectileColor)
    {
        //GameTime gameTime = new GameTime();
        Color color = new(255, 255, 255, projectileColor.A - projectile.alpha);
        Vector2 vector = projectile.velocity;
        Color value = Color.Blue * 0.1f;
        Vector2 spinningpoint = new(0f, -4f);
        float t = vector.Length();
        float scale = GetLerpValue(3f, 5f, t, true);
        vector = projectile.position - projectile.oldPos[1];
        float num2 = vector.Length();
        if (num2 == 0f)
        {
            vector = Vector2.UnitY;
        }
        else
        {
            vector *= 5f / num2;
        }
        Vector2 origin = new(projectile.ai[0], projectile.ai[1]);
        Vector2 center = Main.player[projectile.owner].Center;
        float num3 = GetLerpValue(0f, 120f, Vector2.Distance(origin, center), true);
        float num4 = 60f;
        bool flag = false;
        float num5 = GetLerpValue(num4, num4 * 0.8333333f, projectile.localAI[0], true);
        float lerpValue = GetLerpValue(0f, 120f, Vector2.Distance(projectile.Center, center), true);
        num3 *= lerpValue;
        num5 *= GetLerpValue(0f, 15f, projectile.localAI[0], true);
        value = Color.HotPink * 0.15f * (num5 * num3);
        value = Main.hslToRgb(drawColor, 1f, 0.5f) * 0.15f * (num5 * num3);
        spinningpoint = new Vector2(0f, -2f);
        float num6 = GetLerpValue(num4, num4 * 0.6666667f, projectile.localAI[0], true);
        num6 *= GetLerpValue(0f, 20f, projectile.localAI[0], true);
        float num = -0.3f * (1f - num6);
        num += -1f * GetLerpValue(15f, 0f, projectile.localAI[0], true);
        num *= num3;
        scale = num5 * num3;
        Vector2 value2 = projectile.Center + vector;
        Texture2D value3 = TextureAssets.Projectile[projectile.type].Value;
        //new Microsoft.Xna.Framework.Rectangle(0, 0, value3.Width, value3.Height).Size() /= 2f;
        Texture2D value4 = Misc[14].Value;
        Rectangle rectangle = value4.Frame(1, 1, 0, 0, 0, 0);
        Vector2 origin2 = new(rectangle.Width / 2f, 10f);
        //Microsoft.Xna.Framework.Color.Cyan * 0.5f * scale;
        Vector2 value5 = new(0f, projectile.gfxOffY);
        float num7 = (float)Main.time / 60f;
        Vector2 value6 = value2 + vector * 0.5f;
        Color value7 = Color.White * 0.5f * scale;
        value7.A = 0;
        Color color2 = value * scale;
        color2.A = 0;
        Color color3 = value * scale;
        color3.A = 0;
        Color color4 = value * scale;
        color4.A = 0;
        float num8 = vector.ToRotation();
        spriteBatch.Draw(value4, value6 - Main.screenPosition + value5 + spinningpoint.RotatedBy((double)(6.28318548f * num7), default), new Microsoft.Xna.Framework.Rectangle?(rectangle), color2, projectile.velocity.ToRotation() + 1.57079637f, origin2, 1.5f + num, SpriteEffects.None, 0);
        spriteBatch.Draw(value4, value6 - Main.screenPosition + value5 + spinningpoint.RotatedBy((double)(6.28318548f * num7 + 2.09439516f), default), new Microsoft.Xna.Framework.Rectangle?(rectangle), color3, projectile.velocity.ToRotation() + 1.57079637f, origin2, 1.1f + num, SpriteEffects.None, 0);
        spriteBatch.Draw(value4, value6 - Main.screenPosition + value5 + spinningpoint.RotatedBy((double)(6.28318548f * num7 + 4.18879032f), default), new Microsoft.Xna.Framework.Rectangle?(rectangle), color4, projectile.velocity.ToRotation() + 1.57079637f, origin2, 1.3f + num, SpriteEffects.None, 0);
        Vector2 value8 = value2 - vector * 0.5f;
        for (float num9 = 0f; num9 < 1f; num9 += 0.5f)
        {
            float num10 = num7 % 0.5f / 0.5f;
            num10 = (num10 + num9) % 1f;
            float num11 = num10 * 2f;
            if (num11 > 1f)
            {
                num11 = 2f - num11;
            }
            spriteBatch.Draw(value4, value8 - Main.screenPosition + value5, new Microsoft.Xna.Framework.Rectangle?(rectangle), Color.White * num11 * .5f * projectile.Opacity, projectile.velocity.ToRotation() + 1.57079637f, origin2, 0.3f + num10 * 0.5f, SpriteEffects.None, 0);
        }
        if (flag)
        {
            float rotation = projectile.rotation + projectile.localAI[1];
            //float num12 = (float)Main.time / 240f;
            //float globalTimeWrappedHourly = (float)(gameTime.TotalGameTime.TotalSeconds % 3600.0);
            /*float num13 = (float)(gameTime.TotalGameTime.TotalSeconds % 3600.0);
            num13 %= 5f;
            num13 /= 2.5f;
            if (num13 >= 1f)
            {
                num13 = 2f - num13;
            }
            num13 = num13 * 0.5f + 0.5f;*/
            Vector2 position = projectile.Center - Main.screenPosition;
            //Main.instance.LoadItem(75);
            Texture2D value9 = Misc[15].Value;
            Rectangle rectangle2 = value9.Frame(1, 8, 0, 0, 0, 0);
            Vector2 origin3 = rectangle2.Size() / 2f;
            spriteBatch.Draw(value9, position, new Microsoft.Xna.Framework.Rectangle?(rectangle2), color, rotation, origin3, projectile.scale, SpriteEffects.None, 0);
        }
    }
    /// <summary>
    /// 画个高亮锤啊
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="hammerProj"></param>
    /// <param name="glowTex"></param>
    /// <param name="glowColor"></param>
    /// <param name="glowFrame"></param>
    public static void DrawHammer(this SpriteBatch spriteBatch, IHammerProj hammerProj, Texture2D glowTex, Color glowColor, Rectangle? glowFrame)
    {
        Vector2 origin = hammerProj.DrawOrigin;
        float rotation = hammerProj.Rotation;
        var flip = hammerProj.flip;
        if (hammerProj.Player.gravDir == -1)
        {
            rotation = MathHelper.PiOver2 - rotation;
            if (flip == 0) flip = SpriteEffects.FlipHorizontally;
            else if (flip == SpriteEffects.FlipHorizontally) flip = 0;
        }
        switch (hammerProj.flip)
        {
            case SpriteEffects.FlipHorizontally:
                origin.X = hammerProj.projTex.Size().X / hammerProj.FrameMax.X - origin.X;
                rotation += MathHelper.PiOver2;

                break;
            case SpriteEffects.FlipVertically:
                origin.Y = hammerProj.projTex.Size().Y / hammerProj.FrameMax.Y - origin.Y;
                break;
        }
        spriteBatch.Draw(hammerProj.projTex, hammerProj.projCenter - Main.screenPosition, hammerProj.frame, hammerProj.color, rotation, origin, hammerProj.scale, hammerProj.flip, 0);
        spriteBatch.Draw(glowTex, hammerProj.projCenter - Main.screenPosition, glowFrame ?? hammerProj.frame, glowColor, rotation, origin, hammerProj.scale, hammerProj.flip, 0);
    }

    /// <summary>
    /// 画个锤啊
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="hammerProj"></param>
    public static void DrawHammer(this SpriteBatch spriteBatch, IHammerProj hammerProj)
    {
        Vector2 origin = hammerProj.DrawOrigin;
        float rotation = hammerProj.Rotation;
        var flip = hammerProj.flip;
        if (hammerProj.Player.gravDir == -1)
        {
            rotation = MathHelper.PiOver2 - rotation;
            if (flip == 0) flip = SpriteEffects.FlipHorizontally;
            else if (flip == SpriteEffects.FlipHorizontally) flip = 0;
        }
        switch (flip)
        {
            case SpriteEffects.FlipHorizontally:
                origin.X = hammerProj.projTex.Size().X / hammerProj.FrameMax.X - origin.X;
                rotation += MathHelper.PiOver2;

                break;
            case SpriteEffects.FlipVertically:
                origin.Y = hammerProj.projTex.Size().Y / hammerProj.FrameMax.Y - origin.Y;
                break;
        }
        spriteBatch.Draw(hammerProj.projTex, hammerProj.projCenter - Main.screenPosition, hammerProj.frame, hammerProj.color, rotation, origin, hammerProj.scale, flip, 0);
    }

    #region 某阵子的阿汪实在太喜欢魔炮以至于要专门开一栏
    //哦等等，这货的.fx是不是丢了
    //找时间重做一个吧
    //TODO 魔炮shader
    public static void DrawQuadraticLaser_PassColorBar(this SpriteBatch spriteBatch, Vector2 start, Vector2 unit, Texture2D colorBar, Texture2D style, float length = 3200, float width = 512, float shakeRadMax = 0, float light = 4, bool timeOffset = false, float maxFactor = 0.5f, bool autoAdditive = true, (float x1, float y1, float x2, float y2) texcoord = default, float alpha = 1)
    {

        Effect effect = EightTrigramsFurnaceEffect; if (effect == null) return;
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        List<CustomVertexInfo> bars1 = [];
        if (shakeRadMax > 0)
        {
            unit = unit.RotatedBy(Main.rand.NextFloat(-shakeRadMax, shakeRadMax));
        }

        Vector2 unit2 = new(-unit.Y, unit.X);
        if (texcoord == default) texcoord = (0, 0, 1, 1);
        bars1.Add(new CustomVertexInfo(start + unit2 * width, alpha, new Vector3(texcoord.x1, texcoord.y1, light)));
        bars1.Add(new CustomVertexInfo(start - unit2 * width, alpha, new Vector3(texcoord.x1, texcoord.y2, light)));
        bars1.Add(new CustomVertexInfo(start + unit2 * width + length * unit, alpha, new Vector3(texcoord.x2, texcoord.y1, 0)));
        bars1.Add(new CustomVertexInfo(start - unit2 * width + length * unit, alpha, new Vector3(texcoord.x2, texcoord.y2, 0)));
        List<CustomVertexInfo> triangleList1 = [];
        if (bars1.Count > 2)
        {
            for (int i = 0; i < bars1.Count - 2; i += 2)
            {
                triangleList1.Add(bars1[i]);
                triangleList1.Add(bars1[i + 2]);
                triangleList1.Add(bars1[i + 1]);
                triangleList1.Add(bars1[i + 1]);
                triangleList1.Add(bars1[i + 2]);
                triangleList1.Add(bars1[i + 3]);
            }
            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
            effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
            effect.Parameters["maxFactor"].SetValue(maxFactor);
            effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
            Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
            Main.graphics.GraphicsDevice.Textures[1] = style;
            Main.graphics.GraphicsDevice.Textures[2] = colorBar;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
            if (timeOffset)
            {
                effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect_ColorBar_TimeOffset"].Apply();
            }
            else
            {
                effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect_ColorBar"].Apply();
            }

            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
            Main.graphics.GraphicsDevice.RasterizerState = originalState;
        }
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    public static void DrawQuadraticLaser_PassColorBar(this SpriteBatch spriteBatch, (Vector2 start, Vector2 unit)[] startAndUnits, Texture2D colorBar, Texture2D style, float length = 3200, float width = 512, float shakeRadMax = 0, float light = 4, bool timeOffset = false, float maxFactor = 0.5f, bool autoAdditive = true, float alpha = 1)
    {
        Effect effect = EightTrigramsFurnaceEffect; if (effect == null) return;
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["maxFactor"].SetValue(maxFactor);
        effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
        Main.graphics.GraphicsDevice.Textures[1] = style;
        Main.graphics.GraphicsDevice.Textures[2] = colorBar;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
        if (timeOffset)
        {
            effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect_ColorBar_TimeOffset"].Apply();
        }
        else
        {
            effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect_ColorBar"].Apply();
        }

        foreach (var (start, _unit) in startAndUnits)
        {
            List<CustomVertexInfo> bars1 = [];
            var unit = _unit;
            if (shakeRadMax > 0)
            {
                unit = unit.RotatedBy(Main.rand.NextFloat(-shakeRadMax, shakeRadMax));
            }

            Vector2 unit2 = new(-unit.Y, unit.X);
            bars1.Add(new CustomVertexInfo(start + unit2 * width, alpha, new Vector3(0, 0, light)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width, alpha, new Vector3(0, 1, light)));
            bars1.Add(new CustomVertexInfo(start + unit2 * width + length * unit, alpha, new Vector3(1, 0, 0)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width + length * unit, alpha, new Vector3(1, 1, 0)));
            List<CustomVertexInfo> triangleList1 = [];
            if (bars1.Count > 2)
            {
                for (int i = 0; i < bars1.Count - 2; i += 2)
                {
                    triangleList1.Add(bars1[i]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 3]);
                }
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }
        }
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    public static void DrawQuadraticLaser_PassHeatMap(this SpriteBatch spriteBatch, Vector2 start, Vector2 unit, Texture2D heatMap, Texture2D style, float length = 3200, float width = 512, float shakeRadMax = 0, float light = 4, bool timeOffset = false, float maxFactor = 0.5f, bool autoAdditive = true, (float x1, float y1, float x2, float y2) texcoord = default, float alpha = 1)
    {
        //start += Vector2.UnitX * 64;
        if (false)
        {
            Effect effect = ModAsset.EightTrigramsFurnaceEffectEX.Value;
            if (autoAdditive)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            List<CustomVertexInfo> bars1 = [];
            if (shakeRadMax > 0)
            {
                unit = unit.RotatedBy(Main.rand.NextFloat(-shakeRadMax, shakeRadMax));
            }

            Vector2 unit2 = new(-unit.Y, unit.X);
            if (texcoord == default) texcoord = (0, 0, 1, 1);
            bars1.Add(new CustomVertexInfo(start + unit2 * width, alpha, new Vector3(texcoord.x1, texcoord.y1, light)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width, alpha, new Vector3(texcoord.x1, texcoord.y2, light)));
            bars1.Add(new CustomVertexInfo(start + unit2 * width + length * unit, alpha, new Vector3(texcoord.x2, texcoord.y1, 0)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width + length * unit, alpha, new Vector3(texcoord.x2, texcoord.y2, 0)));
            List<CustomVertexInfo> triangleList1 = [];
            if (bars1.Count > 2)
            {
                for (int i = 0; i < bars1.Count - 2; i += 2)
                {
                    triangleList1.Add(bars1[i]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 3]);
                }
                RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
                var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
                var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
                effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
                effect.Parameters["uMaxFactor"].SetValue(maxFactor);
                effect.Parameters["uTime"].SetValue(new Vector4(-(float)ModTime * 0.03f, 0, 0, 0));
                effect.Parameters["uAlphaVector"].SetValue(new Vector3(0, 0, 1));
                Main.graphics.GraphicsDevice.Textures[0] = AniTex[1].Value;
                Main.graphics.GraphicsDevice.Textures[1] = AniTex[0].Value;
                Main.graphics.GraphicsDevice.Textures[2] = heatMap;
                Main.graphics.GraphicsDevice.Textures[3] = heatMap;
                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
                Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
                Main.graphics.GraphicsDevice.SamplerStates[3] = SamplerState.PointClamp;
                effect.CurrentTechnique.Passes[5].Apply();

                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }
            if (autoAdditive)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        else
        {
            Effect effect = EightTrigramsFurnaceEffect; if (effect == null) return;
            if (autoAdditive)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            List<CustomVertexInfo> bars1 = [];
            if (shakeRadMax > 0)
            {
                unit = unit.RotatedBy(Main.rand.NextFloat(-shakeRadMax, shakeRadMax));
            }

            Vector2 unit2 = new(-unit.Y, unit.X);
            if (texcoord == default) texcoord = (0, 0, 1, 1);
            bars1.Add(new CustomVertexInfo(start + unit2 * width, alpha, new Vector3(texcoord.x1, texcoord.y1, light)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width, alpha, new Vector3(texcoord.x1, texcoord.y2, light)));
            bars1.Add(new CustomVertexInfo(start + unit2 * width + length * unit, alpha, new Vector3(texcoord.x2, texcoord.y1, 0)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width + length * unit, alpha, new Vector3(texcoord.x2, texcoord.y2, 0)));
            List<CustomVertexInfo> triangleList1 = [];
            if (bars1.Count > 2)
            {
                for (int i = 0; i < bars1.Count - 2; i += 2)
                {
                    triangleList1.Add(bars1[i]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 3]);
                }
                RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
                var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
                var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
                effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
                effect.Parameters["maxFactor"].SetValue(maxFactor);
                effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
                Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
                Main.graphics.GraphicsDevice.Textures[1] = style;
                Main.graphics.GraphicsDevice.Textures[2] = heatMap;
                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
                Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
                if (timeOffset)
                {
                    effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect_HeatMap_TimeOffset"].Apply();
                }
                else
                {
                    effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect_HeatMap"].Apply();
                }

                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }
            if (autoAdditive)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
    }
    public static void DrawQuadraticLaser_PassHeatMap(this SpriteBatch spriteBatch, (Vector2 start, Vector2 unit)[] startAndUnits, Texture2D heatMap, Texture2D style, float length = 3200, float width = 512, float shakeRadMax = 0, float light = 4, bool timeOffset = false, float maxFactor = 0.5f, bool autoAdditive = true, float alpha = 1)
    {
        Effect effect = EightTrigramsFurnaceEffect; if (effect == null) return;
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["maxFactor"].SetValue(maxFactor);
        effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
        Main.graphics.GraphicsDevice.Textures[1] = style;
        Main.graphics.GraphicsDevice.Textures[2] = heatMap;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.PointWrap;
        if (timeOffset)
        {
            effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect_HeatMap_TimeOffset"].Apply();
        }
        else
        {
            effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect_HeatMap"].Apply();
        }

        foreach (var (start, _unit) in startAndUnits)
        {
            List<CustomVertexInfo> bars1 = [];
            var unit = _unit;
            if (shakeRadMax > 0)
            {
                unit = unit.RotatedBy(Main.rand.NextFloat(-shakeRadMax, shakeRadMax));
            }

            Vector2 unit2 = new(-unit.Y, unit.X);
            bars1.Add(new CustomVertexInfo(start + unit2 * width, alpha, new Vector3(0, 0, light)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width, alpha, new Vector3(0, 1, light)));
            bars1.Add(new CustomVertexInfo(start + unit2 * width + length * unit, alpha, new Vector3(1, 0, 0)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width + length * unit, alpha, new Vector3(1, 1, 0)));
            List<CustomVertexInfo> triangleList1 = [];
            if (bars1.Count > 2)
            {
                for (int i = 0; i < bars1.Count - 2; i += 2)
                {
                    triangleList1.Add(bars1[i]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 3]);
                }
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }
        }
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    public static void DrawQuadraticLaser_PassNormal(this SpriteBatch spriteBatch, Vector2 start, Vector2 unit, Color color, Texture2D style, float length = 3200, float width = 512, float shakeRadMax = 0, float light = 4, float maxFactor = 0.5f, bool autoAdditive = true)
    {
        Effect effect = EightTrigramsFurnaceEffect; if (effect == null) return;
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        List<CustomVertexInfo> bars1 = [];
        if (shakeRadMax > 0)
        {
            unit = unit.RotatedBy(Main.rand.NextFloat(-shakeRadMax, shakeRadMax));
        }

        Vector2 unit2 = new(-unit.Y, unit.X);
        bars1.Add(new CustomVertexInfo(start + unit2 * width, color, new Vector3(0, 0, light)));
        bars1.Add(new CustomVertexInfo(start - unit2 * width, color, new Vector3(0, 1, light)));
        bars1.Add(new CustomVertexInfo(start + unit2 * width + length * unit, color, new Vector3(1, 0, 0)));
        bars1.Add(new CustomVertexInfo(start - unit2 * width + length * unit, color, new Vector3(1, 1, 0)));
        List<CustomVertexInfo> triangleList1 = [];
        if (bars1.Count > 2)
        {
            for (int i = 0; i < bars1.Count - 2; i += 2)
            {
                triangleList1.Add(bars1[i]);
                triangleList1.Add(bars1[i + 2]);
                triangleList1.Add(bars1[i + 1]);
                triangleList1.Add(bars1[i + 1]);
                triangleList1.Add(bars1[i + 2]);
                triangleList1.Add(bars1[i + 3]);
            }
            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.gameMenu ? Main.instance.Window.ClientBounds.Width : Main.screenWidth, Main.gameMenu ? Main.instance.Window.ClientBounds.Height : Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
            effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
            effect.Parameters["maxFactor"].SetValue(maxFactor);
            effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
            Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
            Main.graphics.GraphicsDevice.Textures[1] = style;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect"].Apply();
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
            Main.graphics.GraphicsDevice.RasterizerState = originalState;
        }
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    public static void DrawQuadraticLaser_PassNormal(this SpriteBatch spriteBatch, (Vector2 start, Vector2 unit)[] startAndUnits, Color color, Texture2D style, float length = 3200, float width = 512, float shakeRadMax = 0, float light = 4, float maxFactor = 0.5f, bool autoAdditive = true)
    {
        Effect effect = EightTrigramsFurnaceEffect; if (effect == null) return;
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["maxFactor"].SetValue(maxFactor);
        effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
        Main.graphics.GraphicsDevice.Textures[1] = style;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        effect.CurrentTechnique.Passes["EightTrigramsFurnaceEffect"].Apply();
        foreach (var (start, _unit) in startAndUnits)
        {
            List<CustomVertexInfo> bars1 = [];
            var unit = _unit;
            if (shakeRadMax > 0)
            {
                unit = unit.RotatedBy(Main.rand.NextFloat(-shakeRadMax, shakeRadMax));
            }

            Vector2 unit2 = new(-unit.Y, unit.X);
            bars1.Add(new CustomVertexInfo(start + unit2 * width, color, new Vector3(0, 0, light)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width, color, new Vector3(0, 1, light)));
            bars1.Add(new CustomVertexInfo(start + unit2 * width + length * unit, color, new Vector3(1, 0, 0)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width + length * unit, color, new Vector3(1, 1, 0)));
            List<CustomVertexInfo> triangleList1 = [];
            if (bars1.Count > 2)
            {
                for (int i = 0; i < bars1.Count - 2; i += 2)
                {
                    triangleList1.Add(bars1[i]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 3]);
                }
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }
        }
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    #endregion

    #region 什么，他还喜欢画特效线
    public static void DrawEffectLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 _unit, Color color, Texture2D style, float startLight = 1, float endLight = 0, float length = 3200, float width = 512, bool autoAdditive = true)
    {
        try
        {
            Effect effect = ShaderSwooshEffect;
            if (effect == null)
            {
                return;
            }

            if (autoAdditive)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.gameMenu ? Main.instance.Window.ClientBounds.Width : Main.screenWidth, Main.gameMenu ? Main.instance.Window.ClientBounds.Height : Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
            effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
            effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
            Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
            Main.graphics.GraphicsDevice.Textures[1] = style;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            effect.CurrentTechnique.Passes[0].Apply();
            List<CustomVertexInfo> bars1 = [];
            var unit = _unit;
            Vector2 unit2 = new(-unit.Y, unit.X);
            bars1.Add(new CustomVertexInfo(start + unit2 * width, color, new Vector3(0, 0, startLight)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width, color, new Vector3(0, 1, startLight)));
            bars1.Add(new CustomVertexInfo(start + unit2 * width + length * unit, color, new Vector3(1, 0, endLight)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width + length * unit, color, new Vector3(1, 1, endLight)));
            List<CustomVertexInfo> triangleList1 = [];
            if (bars1.Count > 2)
            {
                for (int i = 0; i < bars1.Count - 2; i += 2)
                {
                    triangleList1.Add(bars1[i]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 3]);
                }
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }
            if (autoAdditive)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        catch (Exception e)
        {
            Main.NewText(e);
        }
    }
    public static void DrawEffectLine(this SpriteBatch spriteBatch, (Vector2 start, Vector2 unit)[] startAndUnits, Color color, Texture2D style, float startLight = 1, float endLight = 0, float length = 3200, float width = 512, bool autoAdditive = true)
    {
        Effect effect = ShaderSwooshEffect;
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        //effect.Parameters["maxFactor"].SetValue(maxFactor);
        effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
        Main.graphics.GraphicsDevice.Textures[1] = style;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        effect.CurrentTechnique.Passes[0].Apply();
        foreach (var (start, _unit) in startAndUnits)
        {
            List<CustomVertexInfo> bars1 = [];
            var unit = _unit;
            Vector2 unit2 = new(-unit.Y, unit.X);
            bars1.Add(new CustomVertexInfo(start + unit2 * width, color, new Vector3(0, 0, startLight)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width, color, new Vector3(0, 1, startLight)));
            bars1.Add(new CustomVertexInfo(start + unit2 * width + length * unit, color, new Vector3(1, 0, endLight)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width + length * unit, color, new Vector3(1, 1, endLight)));
            List<CustomVertexInfo> triangleList1 = [];
            if (bars1.Count > 2)
            {
                for (int i = 0; i < bars1.Count - 2; i += 2)
                {
                    triangleList1.Add(bars1[i]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 3]);
                }
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }
        }
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    public static void DrawEffectLine_StartAndEnd(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, Texture2D style, float startLight = 1, float endLight = 0, float width = 512, bool autoAdditive = true)
    {
        Effect effect = ShaderSwooshEffect;
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
        Main.graphics.GraphicsDevice.Textures[1] = style;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        effect.CurrentTechnique.Passes[1].Apply();
        List<CustomVertexInfo> bars1 = [];
        var unit = Vector2.Normalize(end - start);
        //unit.Normalize();
        Vector2 unit2 = new(-unit.Y, unit.X);
        bars1.Add(new CustomVertexInfo(start + unit2 * width, color, new Vector3(0, 0, startLight)));
        bars1.Add(new CustomVertexInfo(start - unit2 * width, color, new Vector3(0, 1, startLight)));
        bars1.Add(new CustomVertexInfo(end + unit2 * width, color, new Vector3(1, 0, endLight)));
        bars1.Add(new CustomVertexInfo(end - unit2 * width, color, new Vector3(1, 1, endLight)));
        List<CustomVertexInfo> triangleList1 = [];
        if (bars1.Count > 2)
        {
            for (int i = 0; i < bars1.Count - 2; i += 2)
            {
                triangleList1.Add(bars1[i]);
                triangleList1.Add(bars1[i + 2]);
                triangleList1.Add(bars1[i + 1]);
                triangleList1.Add(bars1[i + 1]);
                triangleList1.Add(bars1[i + 2]);
                triangleList1.Add(bars1[i + 3]);
            }
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
            Main.graphics.GraphicsDevice.RasterizerState = originalState;
        }
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    public static void DrawEffectLine_StartAndEnd(this SpriteBatch spriteBatch, (Vector2 start, Vector2 end)[] startAndEnds, Color color, Texture2D style, float startLight = 1, float endLight = 0, float width = 512, bool autoAdditive = true)
    {
        Effect effect = ShaderSwooshEffect;
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = BaseTex[8].Value;
        Main.graphics.GraphicsDevice.Textures[1] = style;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        effect.CurrentTechnique.Passes[1].Apply();
        foreach (var (start, end) in startAndEnds)
        {
            List<CustomVertexInfo> bars1 = [];
            var unit = Vector2.Normalize(end - start);
            //unit.Normalize();
            Vector2 unit2 = new(-unit.Y, unit.X);
            bars1.Add(new CustomVertexInfo(start + unit2 * width, color, new Vector3(0, 0, startLight)));
            bars1.Add(new CustomVertexInfo(start - unit2 * width, color, new Vector3(0, 1, startLight)));
            bars1.Add(new CustomVertexInfo(end + unit2 * width, color, new Vector3(1, 0, endLight)));
            bars1.Add(new CustomVertexInfo(end - unit2 * width, color, new Vector3(1, 1, endLight)));
            List<CustomVertexInfo> triangleList1 = [];
            if (bars1.Count > 2)
            {
                for (int i = 0; i < bars1.Count - 2; i += 2)
                {
                    triangleList1.Add(bars1[i]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 1]);
                    triangleList1.Add(bars1[i + 2]);
                    triangleList1.Add(bars1[i + 3]);
                }
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList1.ToArray(), 0, triangleList1.Count / 3);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }
        }
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    #endregion

    #region 总算，这次不是特效线了
    public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float width = 4f, bool offset = false, Vector2 drawOffset = default)
    {
        if (offset)
        {
            end += start;
        }

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, (start + end) * .5f + drawOffset, new Rectangle(0, 0, 1, 1), color, (end - start).ToRotation(), new Vector2(.5f, .5f), new Vector2((start - end).Length(), width), 0, 0);
    }
    public static void DrawLine(this SpriteBatch spriteBatch, Vector3 start, Vector3 end, Color color, float height, float width = 4f, bool offset = false, Vector2 drawOffset = default, Vector2 projCenter = default)
    {
        if (offset)
        {
            end += start;
        }

        var s = start.Projectile(height, projCenter);
        var e = end.Projectile(height, projCenter);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, (s + e) * .5f + drawOffset, new Rectangle(0, 0, 1, 1), color, (e - s).ToRotation(), new Vector2(.5f, .5f), new Vector2((s - e).Length(), width), 0, 0);
    }
    public static void DrawLine(this SpriteBatch spriteBatch, Vector4 start, Vector4 end, Color color, float heightZ, float heightW, float width = 4f, bool offset = false, Vector2 drawOffset = default, Vector2 projCenter = default)
    {
        if (offset)
        {
            end += start;
        }

        var s = start.Projectile(heightW, new Vector3(projCenter, 0)).Projectile(heightZ, projCenter);
        var e = end.Projectile(heightW, new Vector3(projCenter, 0)).Projectile(heightZ, projCenter);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, (s + e) * .5f + drawOffset,
            new Rectangle(0, 0, 1, 1), color, (e - s).ToRotation(),
            new Vector2(.5f, .5f), new Vector2((s - e).Length(), width), 0, 0);
    }
    public static void DrawLine(this SpriteBatch spriteBatch, Vector3 start, Vector3 end, Color color, float height, out Vector2 s, out Vector2 e, float width = 4f, bool offset = false, Vector2 drawOffset = default, Vector2 projCenter = default)
    {
        if (offset)
        {
            end += start;
        }

        s = start.Projectile(height, projCenter) + drawOffset;
        e = end.Projectile(height, projCenter) + drawOffset;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, (s + e) * .5f, new Rectangle(0, 0, 1, 1), color, (e - s).ToRotation(), new Vector2(.5f, .5f), new Vector2((s - e).Length(), width), 0, 0);
    }
    public static void DrawLine(this SpriteBatch spriteBatch, Vector4 start, Vector4 end, Color color, float heightZ, float heightW, out Vector2 s, out Vector2 e, float width = 4f, bool offset = false, Vector2 drawOffset = default, Vector2 projCenter = default)
    {
        if (offset)
        {
            end += start;
        }

        s = start.Projectile(heightW, new Vector3(projCenter, 0)).Projectile(heightZ, projCenter) + drawOffset;
        e = end.Projectile(heightW, new Vector3(projCenter, 0)).Projectile(heightZ, projCenter) + drawOffset;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, (s + e) * .5f, new Rectangle(0, 0, 1, 1), color, (e - s).ToRotation(), new Vector2(.5f, .5f), new Vector2((s - e).Length(), width), 0, 0);
    }
    #endregion

    #region 绘制一个路径，你不会想用第二次
    public static void DrawPath(this SpriteBatch spriteBatch, Vector2[] vectorFunc, Func<float, Color> colorFunc, Effect effect, Texture2D baseTex, Texture2D aniTex, Vector2 offest = default, float width = 16, float kOfX = 1, bool looped = false, Func<float, float> factorFunc = null, Func<float, float> widthFunc = null, Func<float, float> lightFunc = null, Func<float> timeFunc = null, string pass = default, Action<Vector2, int> doSth = null, bool alwaysDoSth = false)
    {
        if (vectorFunc == null || colorFunc == null || effect == null || vectorFunc.Length < 3)
        {
            return;
        }
        var counts = vectorFunc.Length;
        var bars = new CustomVertexInfo[counts * 2];
        for (int n = 0; n < counts; n++)
        {
            vectorFunc[n] += offest;
            if ((!Main.gamePaused || alwaysDoSth) && doSth != null)
            {
                doSth.Invoke(vectorFunc[n], n);
            }
        }
        for (int i = 0; i < counts; ++i)
        {
            var normalDir = i == 0 ? looped ? vectorFunc[0] - vectorFunc[counts - 1] : vectorFunc[1] - vectorFunc[0] : vectorFunc[i] - vectorFunc[i - 1];
            normalDir = Vector2.Normalize(new Vector2(-normalDir.Y, normalDir.X));
            var factor = 1 - i / (counts - 1f);
            if (factorFunc != null)
            {
                factor = factorFunc.Invoke(factor);
            }
            var color = colorFunc.Invoke(factor);
            var w = widthFunc == null ? width : widthFunc.Invoke(factor);
            var l = lightFunc == null ? factor : lightFunc.Invoke(factor);
            bars[2 * i] = new CustomVertexInfo(vectorFunc[i] + normalDir * w, color, new Vector3(factor * kOfX, 1, l));
            bars[2 * i + 1] = new CustomVertexInfo(vectorFunc[i] + normalDir * -w, color, new Vector3(factor * kOfX, 0, l));
        }
        List<CustomVertexInfo> triangleList = [];
        for (int i = 0; i < bars.Length - 2; i += 2)
        {
            triangleList.Add(bars[i]);
            triangleList.Add(bars[i + 2]);
            triangleList.Add(bars[i + 1]);
            triangleList.Add(bars[i + 1]);
            triangleList.Add(bars[i + 2]);
            triangleList.Add(bars[i + 3]);
        }
        if (triangleList.Count > 2)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
            //RasterizerState rasterizerState = new RasterizerState();
            //rasterizerState.CullMode = CullMode.None;
            //rasterizerState.FillMode = FillMode.WireFrame;
            //Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;
            RasterizerState rasterizerState = new()
            {
                CullMode = CullMode.None
            };
            Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
            effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
            effect.Parameters["uTime"].SetValue(timeFunc == null ? -(float)Main.time * 0.03f : timeFunc.Invoke());
            Main.graphics.GraphicsDevice.Textures[0] = baseTex;
            Main.graphics.GraphicsDevice.Textures[1] = aniTex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            if (pass != null) { effect.CurrentTechnique.Passes[pass].Apply(); } else { effect.CurrentTechnique.Passes[0].Apply(); }
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList.ToArray(), 0, triangleList.Count / 3);
            Main.graphics.GraphicsDevice.RasterizerState = originalState;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

    }
    public static void DrawPath(this SpriteBatch spriteBatch, Func<float, Vector2> vectorFunc, Func<float, Color> colorFunc, Effect effect, Texture2D baseTex, Texture2D aniTex, Vector2 offest = default, int counts = 25, float min = 0, float max = 1, float width = 16, float kOfX = 1, bool looped = false, Func<float, float> factorFunc = null, Func<float, float> widthFunc = null, Func<float, float> lightFunc = null, Func<float> timeFunc = null, string pass = default, Action<Vector2, float> doSth = null, bool alwaysDoSth = false, bool autoAdditive = true, int[] skipPoint = null)
    {
        if (vectorFunc == null || colorFunc == null || effect == null || counts < 3)
        {
            return;
        }
        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        var positions = new Vector2[counts];
        var bars = new CustomVertexInfo[counts * 2];
        for (int n = 0; n < counts; n++)
        {
            var factor = (float)n / (counts - 1);
            if (factorFunc != null)
            {
                factor = factorFunc.Invoke(factor);
            }
            var lerp = factor * min + (1 - factor) * max;
            var position = vectorFunc.Invoke(lerp) + offest;
            positions[n] = position;
            if ((!Main.gamePaused || alwaysDoSth) && doSth != null)
            {
                doSth.Invoke(position, factor);
            }
        }
        for (int i = 0; i < counts; ++i)
        {
            if (positions[i] == Vector2.Zero)
            {
                break;
            }

            var normalDir = i == 0 ? looped ? positions[0] - positions[counts - 1] : positions[1] - positions[0] : positions[i] - positions[i - 1];
            normalDir = Vector2.Normalize(new Vector2(-normalDir.Y, normalDir.X));
            var factor = i / (counts - 1f);
            if (factorFunc != null)
            {
                factor = factorFunc.Invoke(factor);
            }
            var color = colorFunc.Invoke(factor);
            var w = widthFunc == null ? width : widthFunc.Invoke(factor);
            var l = lightFunc == null ? factor : lightFunc.Invoke(factor);
            bars[2 * i] = new CustomVertexInfo(positions[i] + normalDir * w, color, new Vector3(factor * kOfX, 1, l));
            bars[2 * i + 1] = new CustomVertexInfo(positions[i] + normalDir * -w, color, new Vector3(factor * kOfX, 0, l));
        }
        List<CustomVertexInfo> triangleList = [];
        for (int i = 0; i < bars.Length - 2; i += 2)
        {
            if (skipPoint != null && skipPoint.Contains(i)) i += 2;
            triangleList.Add(bars[i]);
            triangleList.Add(bars[i + 2]);
            triangleList.Add(bars[i + 1]);
            triangleList.Add(bars[i + 1]);
            triangleList.Add(bars[i + 2]);
            triangleList.Add(bars[i + 3]);
        }
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        //RasterizerState rasterizerState = new RasterizerState();
        //rasterizerState.CullMode = CullMode.None;
        //rasterizerState.FillMode = FillMode.WireFrame;
        //Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;
        RasterizerState rasterizerState = new()
        {
            CullMode = CullMode.None
        };
        Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;
        if (triangleList.Count > 2)
        {
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
            effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
            effect.Parameters["uTime"].SetValue(timeFunc == null ? -(float)Main.time * 0.03f : timeFunc.Invoke());
            Main.graphics.GraphicsDevice.Textures[0] = baseTex;
            Main.graphics.GraphicsDevice.Textures[1] = aniTex;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
            if (pass != null) { effect.CurrentTechnique.Passes[pass].Apply(); } else { effect.CurrentTechnique.Passes[0].Apply(); }
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList.ToArray(), 0, triangleList.Count / 3);
        }

        Main.graphics.GraphicsDevice.RasterizerState = originalState;

        if (autoAdditive)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

    }
    #endregion

    #region 绘制空间平面，不好用，该换
    public static void Draw3DPlane(this SpriteBatch spriteBatch, Effect effect, Texture2D baseTex, Texture2D aniTex, VertexTriangle3List loti, string pass = default)
    {
        if (loti.tris == null)
        {
            return;
        }

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        RasterizerState rasterizerState = new()
        {
            CullMode = CullMode.None
        };
        Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["uTime"].SetValue(-(float)Main.time * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = baseTex;
        Main.graphics.GraphicsDevice.Textures[1] = aniTex;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        if (pass != null) { effect.CurrentTechnique.Passes[pass].Apply(); } else { effect.CurrentTechnique.Passes[0].Apply(); }
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, loti.ToVertexInfo(), 0, loti.Length);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void Draw3DPlane(this SpriteBatch spriteBatch, Effect effect, Texture2D baseTex, Texture2D aniTex, string pass = default, params VertexTriangle3[] tris)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        RasterizerState rasterizerState = new()
        {
            CullMode = CullMode.None
        };
        Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["uTime"].SetValue(-(float)Main.time * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = baseTex;
        Main.graphics.GraphicsDevice.Textures[1] = aniTex;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        if (pass != null) { effect.CurrentTechnique.Passes[pass].Apply(); } else { effect.CurrentTechnique.Passes[0].Apply(); }
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VertexTriangle3.ToVertexInfo(tris), 0, tris.Length);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void DrawPlane(this SpriteBatch spriteBatch, Effect effect, Texture2D baseTex, Texture2D aniTex, VertexTriangleList vttl, string pass = default)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        RasterizerState rasterizerState = new()
        {
            CullMode = CullMode.None
        };
        Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["uTime"].SetValue(-(float)Main.time * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = baseTex;
        Main.graphics.GraphicsDevice.Textures[1] = aniTex;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        if (pass != null) { effect.CurrentTechnique.Passes[pass].Apply(); } else { effect.CurrentTechnique.Passes[0].Apply(); }
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vttl.ToVertexInfo(), 0, vttl.Length);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public static void DrawPlane(this SpriteBatch spriteBatch, Effect effect, Texture2D baseTex, Texture2D aniTex, string pass = default, params VertexTriangle[] tris)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        RasterizerState rasterizerState = new()
        {
            CullMode = CullMode.None
        };
        Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        effect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        effect.Parameters["uTime"].SetValue(-(float)Main.time * 0.03f);
        Main.graphics.GraphicsDevice.Textures[0] = baseTex;
        Main.graphics.GraphicsDevice.Textures[1] = aniTex;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        if (pass != null) { effect.CurrentTechnique.Passes[pass].Apply(); } else { effect.CurrentTechnique.Passes[0].Apply(); }
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VertexTriangle.ToVertexInfo(tris), 0, tris.Length);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    #endregion

    /// <summary>
    /// 给物品上fumo光泽，但是这次是在世界
    /// </summary>
    /// <param name="item"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="effectTex"></param>
    /// <param name="c"></param>
    /// <param name="rotation"></param>
    /// <param name="light"></param>
    public static void ShaderItemEffectInWorld(this Item item, SpriteBatch spriteBatch, Texture2D effectTex, Color c, float rotation, float light = 2)
    {
        if (ItemEffect == null) return;

        var samplerState = spriteBatch.GraphicsDevice.SamplerStates[0];
        var depthStencilState = spriteBatch.GraphicsDevice.DepthStencilState;
        var rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;

        var matrix = spriteBatch.transformMatrix;
        var effect = spriteBatch.customEffect;


        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null);
        CustomVertexInfo[] triangleArry = new CustomVertexInfo[6];
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        //Color c = Main.hslToRgb((float)LogSpiralLibrary.ModTime / 60 % 1, 1f, 0.75f);
        var texture = TextureAssets.Item[item.type].Value;
        Vector2 scale = texture.Size();
        //triangleArry[0] = new CustomVertexInfo(item.position, c, new Vector3(0, 0, light));
        //triangleArry[1] = new CustomVertexInfo(item.position + new Vector2(scale.X, 0), c, new Vector3(1, 0, light));
        //triangleArry[2] = new CustomVertexInfo(item.position + scale, c, new Vector3(1, 1, light));
        //triangleArry[3] = triangleArry[2];
        //triangleArry[4] = new CustomVertexInfo(item.position + new Vector2(0, scale.Y), c, new Vector3(0, 1, light));
        //triangleArry[5] = triangleArry[0];
        var ani = Main.itemAnimations[item.type];
        var texCoordStart = new Vector2(0);
        var texCoordEnd = new Vector2(1);
        if (ani != null)
        {
            var frame = ani.GetFrame(texture);
            texCoordStart = frame.TopLeft() / scale;
            texCoordEnd = frame.BottomRight() / scale;
            scale = frame.Size();
        }
        scale *= 0.5f;
        Vector2 vector2 = new(item.width / 2 - scale.X, item.height - scale.Y * 2.0f);

        var center = item.position + scale + vector2;//item.position + scale
        triangleArry[0] = new CustomVertexInfo(center - scale.RotatedBy(rotation), c, new Vector3(texCoordStart, light));
        triangleArry[1] = new CustomVertexInfo(center + new Vector2(scale.X, -scale.Y).RotatedBy(rotation), c, new Vector3(new Vector2(texCoordEnd.X, texCoordStart.Y), light));
        triangleArry[2] = new CustomVertexInfo(center + scale.RotatedBy(rotation), c, new Vector3(texCoordEnd, light));
        triangleArry[3] = triangleArry[2];
        triangleArry[4] = new CustomVertexInfo(center - new Vector2(scale.X, -scale.Y).RotatedBy(rotation), c, new Vector3(new Vector2(texCoordStart.X, texCoordEnd.Y), light));
        triangleArry[5] = triangleArry[0];

        var projection = Matrix.CreateOrthographicOffCenter(0, Main.gameMenu ? Main.instance.Window.ClientBounds.Width : Main.screenWidth, Main.gameMenu ? Main.instance.Window.ClientBounds.Height : Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        ItemEffect.Parameters["uTransform"].SetValue(model * Main.GameViewMatrix.TransformationMatrix * projection);
        ItemEffect.Parameters["uTime"].SetValue((float)ModTime / 60f);//(float)LogSpiralLibrary.ModTime / 60
        Main.graphics.GraphicsDevice.Textures[0] = texture;
        Main.graphics.GraphicsDevice.Textures[1] = effectTex;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        ItemEffect.CurrentTechnique.Passes[0].Apply();
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleArry, 0, 2);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState, depthStencilState, rasterizerState, effect, matrix);
    }
    /// <summary>
    /// 给物品上fumo光泽，但是这次是在包包
    /// </summary>
    /// <param name="item"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="position"></param>
    /// <param name="origin"></param>
    /// <param name="effectTex"></param>
    /// <param name="c"></param>
    /// <param name="Scale"></param>
    /// <param name="light"></param>
    public static void ShaderItemEffectInventory(this Item item, SpriteBatch spriteBatch, Vector2 position, Vector2 origin, Texture2D effectTex, Color c, float Scale, float light = 2)
    {
        if (ItemEffect == null) return;

        var samplerState = spriteBatch.GraphicsDevice.SamplerStates[0];
        var depthStencilState = spriteBatch.GraphicsDevice.DepthStencilState;
        var rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;

        var matrix = spriteBatch.transformMatrix;
        var effect = spriteBatch.customEffect;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, UIElement.OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);
        var ani = Main.itemAnimations[item.type];
        CustomVertexInfo[] triangleArry = new CustomVertexInfo[6];
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        Texture2D texture = TextureAssets.Item[item.type].Value;
        Vector2 scale = texture.Size();
        //float offsetX = texture.Width * Scale;
        //float offsetY = texture.Height * Scale;
        //Color c = Main.hslToRgb(0f, 1f, 0.75f);
        //triangleArry[0] = new CustomVertexInfo(position + Main.screenPosition - new Vector2(offsetX, offsetY) + origin, c, new Vector3(0, 0, light));
        //triangleArry[1] = new CustomVertexInfo(position + Main.screenPosition + new Vector2(offsetX, -offsetY) + origin, c, new Vector3(1, 0, light));
        //triangleArry[2] = new CustomVertexInfo(position + Main.screenPosition + new Vector2(offsetX, offsetY) + origin, c, new Vector3(1, 1, light));
        //triangleArry[3] = triangleArry[2];
        //triangleArry[4] = new CustomVertexInfo(position + Main.screenPosition - new Vector2(offsetX, -offsetY) + origin, c, new Vector3(0, 1, light));
        //triangleArry[5] = triangleArry[0];
        //Vector2 offset = ItemID.Sets.ItemIconPulse[item.type] ? default : new Vector2(-2, -2);
        //var texCoordYstart = 0f;
        //var texCoordYend = 1f;
        //if (ani != null)
        //{
        //    offsetY /= ani.FrameCount;
        //    texCoordYend = 1f / ani.FrameCount;
        //    texCoordYstart = texCoordYend * ani.Frame;
        //    texCoordYend += texCoordYstart;
        //}
        var texCoordStart = new Vector2(0);
        var texCoordEnd = new Vector2(1);

        if (ani != null)
        {
            var frame = ani.GetFrame(texture);
            texCoordStart = frame.TopLeft() / scale;
            texCoordEnd = frame.BottomRight() / scale;
            scale = frame.Size();
        }
        scale *= Scale;
        position -= origin * Scale;
        triangleArry[0] = new CustomVertexInfo(position + Main.screenPosition, c, new Vector3(texCoordStart, light));
        triangleArry[1] = new CustomVertexInfo(position + Main.screenPosition + new Vector2(scale.X, 0), c, new Vector3(new Vector2(texCoordEnd.X, texCoordStart.Y), light));
        triangleArry[2] = new CustomVertexInfo(position + Main.screenPosition + scale, c, new Vector3(texCoordEnd, light));
        triangleArry[3] = triangleArry[2];
        triangleArry[4] = new CustomVertexInfo(position + Main.screenPosition + new Vector2(0, scale.Y), c, new Vector3(new Vector2(texCoordStart.X, texCoordEnd.Y), light));
        triangleArry[5] = triangleArry[0];
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        ItemEffect.Parameters["uTransform"].SetValue(model * projection);
        ItemEffect.Parameters["uTime"].SetValue((float)ModTime / 60f % 1);
        Main.graphics.GraphicsDevice.Textures[0] = texture;
        Main.graphics.GraphicsDevice.Textures[1] = effectTex;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
        ItemEffect.CurrentTechnique.Passes[0].Apply();
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleArry, 0, 2);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, depthStencilState, rasterizerState, effect, matrix);
    }
    #endregion

    #region 处理颜色的奇怪函数
    public static Color ToColor(this Vector3 vector) => new(vector.X, vector.Y, vector.Z);
    /// <summary>
    /// 获取颜色的亮度
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static float Luminance(this Color color) => (Math.Max(Math.Max(color.R, color.G), color.B) + Math.Min(Math.Min(color.R, color.G), color.B)) / 2;
    /// <summary>
    /// 对比两个颜色的相似程度，实现得挺迫真的
    /// 0是直接向量距离了
    /// 1是转hsl然后用一些比较复杂的手段进行处理
    /// </summary>
    /// <param name="mainColor"></param>
    /// <param name="target"></param>
    /// <param name="style">模式</param>
    /// <returns></returns>
    public static float DistanceColor(this Color mainColor, Color target, int style) => style switch { 0 => (mainColor.ToVector3() - target.ToVector3()).Length(), 1 => mainColor.DistanceColor(target), _ => 0 };
    /// <summary>
    /// 获取颜色的距离，这里是hsl
    /// </summary>
    /// <param name="mainColor"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float DistanceColor(this Vector3 mainColor, Vector3 target)
    {
        float hueDistance;
        float saturationDistance = Math.Max(mainColor.Y, target.Y) - Math.Min(mainColor.Y, target.Y);
        float luminosityDistance = Math.Max(mainColor.Z, target.Z) - Math.Min(mainColor.Z, target.Z);
        #region 色调处理
        {
            hueDistance = Math.Max(mainColor.X, target.X);
            float helper = Math.Min(mainColor.X, target.X);
            hueDistance = Math.Min(hueDistance - helper, helper + 1 - hueDistance);
        }
        #endregion
        hueDistance *= mainColor.Y * 2 * MathF.Sqrt(mainColor.Z * (1 - mainColor.Z));
        return hueDistance * 8 + saturationDistance + luminosityDistance;
    }
    /// <summary>
    /// 获取颜色的距离，这里是rgb
    /// </summary>
    /// <param name="mainColor"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float DistanceColor(this Color mainColor, Color target) => Main.rgbToHsl(mainColor).DistanceColor(Main.rgbToHsl(target));
    #endregion

    #region 其它
    public static CustomVertexInfo[] GetItemVertexes(Vector2 origin, float rotationStandard, float rotationOffset, float rotationDirection, Texture2D texture, float KValue, float size, Vector2 drawCen, bool flip, float alpha = 1f, Rectangle? frame = null)
    {
        Rectangle realFrame = frame ?? new Rectangle(0, 0, texture.Width, texture.Height);
        //对数据进行矩阵变换吧！
        Matrix matrix =
        Matrix.CreateTranslation(-origin.X, origin.Y - 1, 0) *          //把变换中心平移到传入的origin上，这里我应该是为了方便改成数学常用的坐标系下的origin了(?)
            Matrix.CreateScale(realFrame.Width, realFrame.Height, 1) *      //缩放到图片原本的正常比例
            Matrix.CreateRotationZ(rotationStandard) *                          //先进行一个旋转操作
            Matrix.CreateScale(1, flip ? 1 : -1, 1) *
            Matrix.CreateRotationZ(rotationOffset) *
            Matrix.CreateScale(1, 1 / KValue, 1) *                      //压扁来有一种横批的感觉(??)
            Matrix.CreateRotationZ(rotationDirection) *                       //朝向旋转量，我用这个的时候是这个固定，上面那个从小变大，形成一种纸片挥砍的动态感(x
            Matrix.CreateScale(size);                                   //单纯大小缩放
        Vector2[] vecs = new Vector2[4];
        for (int i = 0; i < 4; i++)
            vecs[i] = Vector2.Transform(new Vector2(i % 2, i / 2), matrix);//生成单位正方形四个顶点
        CustomVertexInfo[] c = new CustomVertexInfo[6];//两个三角形，六个顶点
        Vector2 startCoord = realFrame.TopLeft() / texture.Size();
        Vector2 endCoord = realFrame.BottomRight() / texture.Size();
        c[0] = new CustomVertexInfo(vecs[0] + drawCen, new Vector3(startCoord.X, endCoord.Y, alpha));
        c[1] = new CustomVertexInfo(vecs[1] + drawCen, new Vector3(endCoord, alpha));
        c[2] = new CustomVertexInfo(vecs[2] + drawCen, new Vector3(startCoord, alpha));
        c[3] = c[1];
        c[4] = new CustomVertexInfo(vecs[3] + drawCen, new Vector3(endCoord.X, startCoord.Y, alpha));
        //c[4] = new CustomVertexInfo(vecs[3] + drawCen, new Vector3(startCoord.Y,endCoord.X, alpha));

        c[5] = c[2];
        return c;
    }

    public static Color Vec2NormalColor(Vector2 vector)
    {
        vector = vector.SafeNormalize(default);
        return new Color(0, vector.X / 2 + .5f, vector.Y / 2 + .5f);
    }
    /// <summary>
    /// 生成三角形列
    /// </summary>
    /// <param name="source">至少四个，要求元素个数为偶数，带状的两侧顶点</param>
    /// <param name="center">缩放中心</param>
    /// <param name="scaler">缩放倍率</param>
    /// <param name="addedCenter">有没有加过中心</param>
    /// <param name="createNormalGraph">是否构建法线贴图</param>
    /// <returns></returns>
    public static CustomVertexInfo[] CreateTriList(CustomVertexInfo[] source, Vector2 center, float scaler, bool addedCenter = false, bool createNormalGraph = false)
    {
        var length = source.Length;
        CustomVertexInfo[] triangleList = new CustomVertexInfo[3 * length - 6];
        for (int i = 0; i < length - 2; i += 2)
        {
            triangleList[3 * i] = source[i];
            triangleList[3 * i + 1] = source[i + 2];
            triangleList[3 * i + 2] = source[i + 1];


            triangleList[3 * i + 3] = source[i + 1];
            triangleList[3 * i + 4] = source[i + 2];
            triangleList[3 * i + 5] = source[i + 3];

            if (createNormalGraph)
            {
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 3; k++)
                        triangleList[3 * i + k + 3 * j].Color = Vec2NormalColor(source[i + 2 + j].Position - source[i + j].Position);
            }
        }
        for (int n = 0; n < triangleList.Length; n++)
        {
            var vertex = triangleList[n];
            if (addedCenter)
            {
                if (scaler != 1) vertex.Position = (vertex.Position - center) * scaler + center;
            }
            else
            {
                if (scaler != 1) vertex.Position *= scaler;
                vertex.Position += center;
            }
            triangleList[n] = vertex;
        }
        return triangleList;
    }
    public static CustomVertexInfo VertexInScreen(this Vector2 vec, Color color, float light = 1)
    {
        return new CustomVertexInfo(vec, color, new Vector3((vec.X - Main.screenPosition.X) / 1920f, (vec.Y - Main.screenPosition.Y) / 1120f, light));
    }
    //[Obsolete]
    //public static Vector2[] GetVertexPoints(ref Vector2[] points)
    //{

    //    //Vector2[] result = new Vector2[4];
    //    LoopArray<Vector2> result = new LoopArray<Vector2>(new Vector2[4]);
    //    float left = float.MaxValue, bottom = float.MaxValue;
    //    float right = float.MinValue, top = float.MinValue;
    //    Stopwatch sw = new Stopwatch();
    //    sw.Start();
    //    //foreach (var vec in points)
    //    //{
    //    //    if (vec.X < left)
    //    //    {
    //    //        left = vec.X;
    //    //        result[0] = vec;
    //    //    }
    //    //    if (vec.Y > top)
    //    //    {
    //    //        top = vec.Y;
    //    //        result[1] = vec;
    //    //    }
    //    //    if (vec.X > right)
    //    //    {
    //    //        right = vec.X;
    //    //        result[2] = vec;
    //    //    }
    //    //    if (vec.Y < bottom)
    //    //    {
    //    //        bottom = vec.Y;
    //    //        result[3] = vec;
    //    //    }
    //    //}

    //    for (int n = 0; n < points.Length; n++)
    //    {
    //        if (sw.ElapsedTicks >= 10000)
    //        {
    //            return null;
    //        }

    //        var vec = points[n];
    //        //if (!result.array.Contains(vec)) 
    //        //{
    //        //    if (vec.X < left)
    //        //    {
    //        //        left = vec.X;
    //        //        result[0] = vec;
    //        //    }
    //        //    if (vec.Y > top)
    //        //    {
    //        //        top = vec.Y;
    //        //        result[1] = vec;
    //        //    }
    //        //    if (vec.X > right)
    //        //    {
    //        //        right = vec.X;
    //        //        result[2] = vec;
    //        //    }
    //        //    if (vec.Y < bottom)
    //        //    {
    //        //        bottom = vec.Y;
    //        //        result[3] = vec;
    //        //    }
    //        //}
    //        if (vec.X < left)
    //        {
    //            left = vec.X;
    //            result[0] = vec;
    //        }
    //        if (vec.Y > top)
    //        {
    //            top = vec.Y;
    //            result[1] = vec;
    //        }
    //        if (vec.X > right)
    //        {
    //            right = vec.X;
    //            result[2] = vec;
    //        }
    //        if (vec.Y < bottom)
    //        {
    //            bottom = vec.Y;
    //            result[3] = vec;
    //        }
    //    }
    //    //if ((result[0] == result[1] && result[2] == result[3]) || (result[2] == result[1] && result[0] == result[3])) 
    //    //{
    //    //    return null;
    //    //}
    //    List<Vector2> newPoints = points.ToList();

    //    for (int n = 0; n < 4; n++)
    //    {
    //        if (sw.ElapsedTicks >= 10000)
    //        {
    //            return null;
    //        }

    //        if (result[n] == result[n + 1])
    //        {
    //            for (int i = 0; i < newPoints.Count; i++)
    //            {
    //                if (result[n] == newPoints[i])
    //                {
    //                    newPoints.Insert(i, result[n]);
    //                    break;
    //                }

    //            }
    //            //var points2 = new Vector2[points.Length + 1];
    //            //int offset = 0;
    //            //for (int i = 0; i < points.Length; i++)
    //            //{
    //            //    points2[i + offset] = points[i];
    //            //    if (points[i] == result[n])
    //            //    {
    //            //        offset++;
    //            //        points2[i + offset] = points[i];
    //            //        //break;
    //            //    }
    //            //}
    //            //points = points2;

    //            //for (int k = 0; k < points.Length; k++)
    //            //{
    //            //    Main.NewText(points[k]);
    //            //}
    //        }
    //    }
    //    points = newPoints.ToArray();
    //    //for (int k = 0; k < points.Length; k++)
    //    //{
    //    //    Main.NewText(points[k]);
    //    //}
    //    return result;
    //}
    //public static Vector2[] GetVertexPoints(this Vector2[] points)
    //{
    //    Vector2[] result = new Vector2[4];
    //    float left = float.MaxValue, bottom = float.MaxValue;
    //    float right = float.MinValue, top = float.MinValue;
    //    foreach (var vec in points)
    //    {
    //        if (vec.X < left)
    //        {
    //            left = vec.X;
    //            result[0] = vec;
    //        }
    //        if (vec.Y > top)
    //        {
    //            top = vec.Y;
    //            result[1] = vec;
    //        }
    //        if (vec.X > right)
    //        {
    //            right = vec.X;
    //            result[2] = vec;
    //        }
    //        if (vec.Y < bottom)
    //        {
    //            bottom = vec.Y;
    //            result[3] = vec;
    //        }
    //    }
    //    return result;
    //}
    public static Color GetColorFromTex(this Texture2D texture, Vector2 texcoord)
    {
        var w = texture.Width;
        var h = texture.Height;
        var cs = new Color[w * h];
        texture.GetData(cs);
        return cs[(int)(texcoord.X * w) + (int)(texcoord.Y * h) * w];
    }
    public static Color GetColor(this Player drawPlayer, Color color)
    {
        return Lighting.GetColor((drawPlayer.Center / 16).ToPoint().X, (drawPlayer.Center / 16).ToPoint().Y, color);
    }
    public static Color GetColor(this Player drawPlayer)
    {
        return Lighting.GetColor((drawPlayer.Center / 16).ToPoint().X, (drawPlayer.Center / 16).ToPoint().Y);
    }
    /// <summary>
    /// 弹幕生成顶点数组
    /// </summary>
    /// <param name="projectile">弹幕</param>
    /// <param name="Offset">偏移量</param>
    /// <param name="Width">宽度</param>
    /// <param name="alpha">不透明度</param>
    /// <param name="VeloTri">速度三角</param>
    /// <param name="mainColor">主色</param>
    /// <returns></returns>
    public static CustomVertexInfo[] TailVertexFromProj(this Projectile projectile, Vector2 Offset = default, float Width = 30, float alpha = 1, bool VeloTri = false, Color? mainColor = null)
    {
        List<CustomVertexInfo> bars = [];
        int indexMax = -1;
        for (int n = 0; n < projectile.oldPos.Length; n++) if (projectile.oldPos[n] == Vector2.Zero) { indexMax = n; break; }
        //if(!Main.gamePaused)
        //Main.NewText(projectile.oldPos[0]);
        if (indexMax == -1) indexMax = projectile.oldPos.Length;
        Offset += projectile.velocity;
        var _mainColor = mainColor ?? Color.Purple;
        for (int i = 1; i < indexMax; ++i)
        {
            if (projectile.oldPos[i] == Vector2.Zero)
            {
                break;
            }
            var normalDir = projectile.oldPos[i - 1] - projectile.oldPos[i];
            normalDir = Vector2.Normalize(new Vector2(-normalDir.Y, normalDir.X));
            var factor = i / (float)indexMax;
            var w = 1 - factor;
            bars.Add(new CustomVertexInfo(projectile.oldPos[i] + Offset + normalDir * Width, _mainColor * w, new Vector3((float)Math.Sqrt(factor), 1, alpha * .6f)));//w * 
            bars.Add(new CustomVertexInfo(projectile.oldPos[i] + Offset + normalDir * -Width, _mainColor * w, new Vector3((float)Math.Sqrt(factor), 0, alpha * .6f)));//w * 
        }
        List<CustomVertexInfo> triangleList = [];
        if (bars.Count > 2)
        {
            if (VeloTri)
            {
                triangleList.Add(bars[0]);
                var vertex = new CustomVertexInfo((bars[0].Position + bars[1].Position) * 0.5f + Vector2.Normalize(projectile.velocity) * 30, _mainColor,
                    new Vector3(0, 0.5f, alpha * .8f));
                triangleList.Add(bars[1]);
                triangleList.Add(vertex);
            }

            for (int i = 0; i < bars.Count - 2; i += 2)
            {
                triangleList.Add(bars[i]);
                triangleList.Add(bars[i + 2]);
                triangleList.Add(bars[i + 1]);

                triangleList.Add(bars[i + 1]);
                triangleList.Add(bars[i + 2]);
                triangleList.Add(bars[i + 3]);
            }
        }
        return [.. triangleList];
    }

    /// <summary>
    /// 弹幕生成顶点数组
    /// </summary>
    /// <param name="projectile">弹幕</param>
    /// <param name="Offset">偏移量</param>
    /// <param name="widthFunc">宽度函数</param>
    /// <param name="colorFunc">颜色函数</param>
    /// <param name="alpha">不透明度</param>
    /// <returns></returns>
    public static CustomVertexInfo[] TailVertexFromProj(this Projectile projectile, Vector2 Offset = default, Func<float, float> widthFunc = null, Func<float, Color> colorFunc = null, float alpha = 1)
    {
        List<CustomVertexInfo> bars = [];
        int indexMax = -1;
        for (int n = 0; n < projectile.oldPos.Length; n++) if (projectile.oldPos[n] == Vector2.Zero) { indexMax = n; break; }
        //if(!Main.gamePaused)
        //Main.NewText(projectile.oldPos[0]);
        if (indexMax == -1) indexMax = projectile.oldPos.Length;
        //Offset += projectile.velocity * 15f;
        for (int i = 0; i < indexMax; ++i)
        {
            if (projectile.oldPos[i] == Vector2.Zero)
            {
                break;
            }
            var normalDir = i == 0 ? projectile.oldPos[0] - projectile.oldPos[1] : projectile.oldPos[i - 1] - projectile.oldPos[i];
            normalDir = Vector2.Normalize(new Vector2(-normalDir.Y, normalDir.X));
            var factor = i / (float)indexMax;
            var w = 1 - factor;
            var Width = widthFunc?.Invoke(factor) ?? 30f;
            var _mainColor = colorFunc?.Invoke(factor) ?? Color.White;
            bars.Add(new CustomVertexInfo(projectile.oldPos[i] + Offset + normalDir * Width, _mainColor * w, new Vector3((float)Math.Sqrt(factor), 1, alpha)));//w * 
            bars.Add(new CustomVertexInfo(projectile.oldPos[i] + Offset + normalDir * -Width, _mainColor * w, new Vector3((float)Math.Sqrt(factor), 0, alpha)));//w * 
        }
        List<CustomVertexInfo> triangleList = [];
        if (bars.Count > 2)
        {
            //if (VeloTri)
            //{
            //    triangleList.Add(bars[0]);
            //    var vertex = new CustomVertexInfo((bars[0].Position + bars[1].Position) * 0.5f + Vector2.Normalize(projectile.velocity) * 30, _mainColor,
            //        new Vector3(0, 0.5f, alpha * .8f));
            //    triangleList.Add(bars[1]);
            //    triangleList.Add(vertex);
            //}

            for (int i = 0; i < bars.Count - 2; i += 2)
            {
                triangleList.Add(bars[i]);
                triangleList.Add(bars[i + 2]);
                triangleList.Add(bars[i + 1]);

                triangleList.Add(bars[i + 1]);
                triangleList.Add(bars[i + 2]);
                triangleList.Add(bars[i + 3]);
            }
        }
        return [.. triangleList];
    }


    /// <summary>
    /// 临时获取贴图
    /// </summary>
    /// <param name="path"></param>
    /// <param name="autoPath"></param>
    /// <returns></returns>
    public static Texture2D GetTexture(string path, bool autoPath = true) => ModContent.Request<Texture2D>((autoPath ? "LogSpiralLibrary/Images/" : "") + path).Value;

    #endregion
}
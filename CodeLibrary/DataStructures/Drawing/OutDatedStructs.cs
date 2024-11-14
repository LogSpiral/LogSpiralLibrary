using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing
{
    //这里的基本都没用，留着只是单纯因为不想删
    public interface IVertexTriangle
    {
        //这个不好用（明明是阿汪你不会用
        CustomVertexInfo A { get; }
        CustomVertexInfo B { get; }
        CustomVertexInfo C { get; }
        CustomVertexInfo this[int index] { get; }
        //CustomVertexInfo[] ToVertexInfo(IVertexTriangle[] tris);
    }
    public struct VertexTriangle3List
    {
        //ListOfTriangleIn3DSpace
        public int Length => tris.Length;
        public float height;
        public Vector2 offset;
        public VertexTriangle3[] tris;
        public VertexTriangle3 this[int i] => tris[i];

        public VertexTriangle3List(float _height, Vector2 _offset, params VertexTriangle3[] _tris)
        {
            height = _height;
            offset = _offset;
            tris = _tris;
        }
        public Vector2 Projectile(Vector3 vector) => height / (height - vector.Z) * (new Vector2(vector.X, vector.Y) + offset - Main.screenPosition - new Vector2(960, 560)) + Main.screenPosition + new Vector2(960, 560);
        public CustomVertexInfo[] ToVertexInfo()
        {
            var vis = new CustomVertexInfo[tris.Length * 3];
            for (int i = 0; i < tris.Length; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    var t = tris[i];
                    vis[i * 3 + n] = new CustomVertexInfo(Projectile(t.positions[n]), t.colors[n], t.vertexs[n]);
                }
            }
            return vis;
        }
    }
    public struct VertexTriangle3 : IVertexTriangle
    {
        //TriangleIn3DSpace
        public VertexTriangle3(Vector3 vA, Vector3 vB, Vector3 vC, Color cA, Color cB, Color cC, Vector3 pA = default, Vector3 pB = default, Vector3 pC = default)
        {
            positions = new Vector3[3];
            vertexs = new Vector3[3];
            colors = new Color[3];
            vertexs[0] = vA;
            vertexs[1] = vB;
            vertexs[2] = vC;
            colors[0] = cA;
            colors[1] = cB;
            colors[2] = cC;
            positions[0] = pA;
            positions[1] = pB;
            positions[2] = pC;
        }
        public static float height = 100;
        public readonly Vector3[] positions;
        public readonly Vector3[] vertexs;
        public readonly Color[] colors;
        public static Vector2 offset = default;
        public static Vector2 Projectile(Vector3 vector) => height / (height - vector.Z) * (new Vector2(vector.X, vector.Y) + offset - Main.screenPosition - new Vector2(960, 560)) + Main.screenPosition + new Vector2(960, 560);
        public CustomVertexInfo this[int index] => new CustomVertexInfo(Projectile(positions[index]), colors[index], vertexs[index]);
        public CustomVertexInfo A => new CustomVertexInfo(Projectile(positions[0]), colors[0], vertexs[0]);
        public CustomVertexInfo B => new CustomVertexInfo(Projectile(positions[1]), colors[1], vertexs[1]);
        public CustomVertexInfo C => new CustomVertexInfo(Projectile(positions[2]), colors[2], vertexs[2]);
        public static CustomVertexInfo[] ToVertexInfo(VertexTriangle3[] tris)
        {
            var vis = new CustomVertexInfo[tris.Length * 3];
            for (int i = 0; i < tris.Length; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    var t = tris[i];
                    vis[i * 3 + n] = new CustomVertexInfo(Projectile(t.positions[n]), t.colors[n], t.vertexs[n]);
                }
            }
            return vis;
        }
    }
    public struct VertexTriangleList
    {
        public int Length => tris.Length;
        public Vector2 offset;
        public VertexTriangle this[int i] => tris[i];

        public VertexTriangle[] tris;
        public Vector2 GetRealPosition(Vector2 vector) => vector + offset;
        public VertexTriangleList(Vector2 _offset, params VertexTriangle[] _tris)
        {
            offset = _offset;
            tris = _tris;
        }
        public CustomVertexInfo[] ToVertexInfo()
        {
            var vis = new CustomVertexInfo[tris.Length * 3];
            for (int i = 0; i < tris.Length; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    var t = tris[i];
                    vis[i * 3 + n] = new CustomVertexInfo(GetRealPosition(t.positions[n]), t.colors[n], t.vertexs[n]);
                }
            }
            return vis;
        }
    }
    public struct VertexTriangle : IVertexTriangle
    {
        public VertexTriangle(Vector3 vA, Vector3 vB, Vector3 vC, Color cA, Color cB, Color cC, Vector2 pA = default, Vector2 pB = default, Vector2 pC = default)
        {
            positions = new Vector2[3];
            vertexs = new Vector3[3];
            colors = new Color[3];
            vertexs[0] = vA;
            vertexs[1] = vB;
            vertexs[2] = vC;
            colors[0] = cA;
            colors[1] = cB;
            colors[2] = cC;
            positions[0] = pA;
            positions[1] = pB;
            positions[2] = pC;
        }
        public readonly Vector2[] positions;
        public readonly Vector3[] vertexs;
        public readonly Color[] colors;
        public static Vector2 offset = default;
        public static Vector2 GetRealPosition(Vector2 vector) => vector + offset;
        public CustomVertexInfo this[int index] => new CustomVertexInfo(GetRealPosition(positions[index]), colors[index], vertexs[index]);
        public CustomVertexInfo A => new CustomVertexInfo(GetRealPosition(positions[0]), colors[0], vertexs[0]);
        public CustomVertexInfo B => new CustomVertexInfo(GetRealPosition(positions[1]), colors[1], vertexs[1]);
        public CustomVertexInfo C => new CustomVertexInfo(GetRealPosition(positions[2]), colors[2], vertexs[2]);
        public static CustomVertexInfo[] ToVertexInfo(VertexTriangle[] tris)
        {
            var vis = new CustomVertexInfo[tris.Length * 3];
            for (int i = 0; i < tris.Length; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    var t = tris[i];
                    vis[i * 3 + n] = t[n];
                }
            }
            return vis;
        }

    }

    /// <summary>
    /// 参考<see cref="DrawData"/>
    /// <br>在原有基础上加入了深度和字符绘制的兼容</br>
    /// <br>优化了构造函数</br>
    /// <br>不太好用以至于被放弃了</br>
    /// </summary>
    public struct DrawDataBuffer
    {
        public Texture2D texture;

        public Vector2 position;

        public Rectangle destinationRectangle;

        public Rectangle? sourceRect;

        public Color color;

        public float rotation;

        public Vector2 origin;

        public Vector2 scale;

        public SpriteEffects effect;

        public readonly bool useDestinationRectangle;

        public static Rectangle? nullRectangle;

        public float depth;

        public string text;

        DynamicSpriteFont spriteFont;

        public bool IsText;
        #region 纹理部分
        public DrawDataBuffer(Texture2D texture, Vector2 position, Color color, float layerDepth = 0f)
            : this(texture, position, nullRectangle, color, layerDepth)
        {
        }

        public DrawDataBuffer(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float layerDepth = 0f)
            : this(texture, position, sourceRect, color, 0, default, 1, 0, layerDepth)
        {
        }

        public DrawDataBuffer(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float layerDepth = 0f)
            : this(texture, position, sourceRect, color, rotation, origin, scale * Vector2.One, effect, layerDepth)
        {
        }

        public DrawDataBuffer(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float layerDepth = 0f)
        {
            this.texture = texture;
            this.position = position;
            this.sourceRect = sourceRect;
            this.color = color;
            this.rotation = rotation;
            this.origin = origin;
            this.scale = scale;
            this.effect = effect;
            depth = layerDepth;
            destinationRectangle = default(Rectangle);
            useDestinationRectangle = false;

            IsText = false;
            text = null;
            spriteFont = null;
        }

        public DrawDataBuffer(Texture2D texture, Rectangle destinationRectangle, Color color, float layerDepth = 0f)
            : this(texture, destinationRectangle, nullRectangle, color, layerDepth)
        {
        }

        public DrawDataBuffer(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRect, Color color, float layerDepth = 0f)
            : this(texture, destinationRectangle, sourceRect, color, 0, default, 0, layerDepth)
        {
        }

        public DrawDataBuffer(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, SpriteEffects effect, float layerDepth = 0f)
        {
            this.texture = texture;
            this.destinationRectangle = destinationRectangle;
            this.sourceRect = sourceRect;
            this.color = color;
            this.rotation = rotation;
            this.origin = origin;
            this.effect = effect;
            position = Vector2.Zero;
            scale = Vector2.One;
            useDestinationRectangle = true;
            depth = layerDepth;
            IsText = false;
            text = null;
            spriteFont = null;
        }
        #endregion


        #region 字体部分
        public DrawDataBuffer(DynamicSpriteFont spriteFont, string text, Vector2 position, Color color, float layerDepth)
            : this(spriteFont, text, position, color, 0, default, 1f, 0, layerDepth)
        {
        }

        public DrawDataBuffer(DynamicSpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float layerDepth)
            : this(spriteFont, text.ToString(), position, color, layerDepth)
        {
        }

        public DrawDataBuffer(DynamicSpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
             : this(spriteFont, text, position, color, rotation, origin, scale * Vector2.One, effects, layerDepth)
        {
        }

        public DrawDataBuffer(DynamicSpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
            : this(spriteFont, text.ToString(), position, color, rotation, origin, scale * Vector2.One, effects, layerDepth)
        {
        }

        public DrawDataBuffer(DynamicSpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
            : this(spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, layerDepth)
        {
        }
        public DrawDataBuffer(DynamicSpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            this.spriteFont = spriteFont;
            this.text = text;
            this.position = position;
            this.color = color;
            this.rotation = rotation;
            this.origin = origin;
            this.scale = scale;
            this.effect = effects;
            this.depth = layerDepth;
        }


        #endregion


        public void Draw(SpriteBatch sb)
        {
            if (IsText)
                sb.DrawString(spriteFont, text, position, color, rotation, origin, scale, effect, depth);
            else if (useDestinationRectangle)
                sb.Draw(texture, destinationRectangle, sourceRect, color, rotation, origin, effect, depth);
            else
                sb.Draw(texture, position, sourceRect, color, rotation, origin, scale, effect, depth);
        }

        public void Draw(SpriteDrawBuffer sb)
        {
            if (useDestinationRectangle)
            {
                sb.Draw(texture, destinationRectangle, sourceRect, color, rotation, origin, effect);
            }
            else
            {
                sb.Draw(texture, position, sourceRect, color, rotation, origin, scale, effect);
            }
        }
    }
}

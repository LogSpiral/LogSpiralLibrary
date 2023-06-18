using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary
{
    /// <summary>
    /// 满足一般顶点绘制需求
    /// </summary>
    public struct CustomVertexInfo : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
        {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        });
        public Vector2 Position;
        public Color Color;
        public Vector3 TexCoord;

        public CustomVertexInfo(Vector2 position, Color color, Vector3 texCoord)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
        }
        public CustomVertexInfo(Vector2 position, float alpha, Vector3 texCoord)
        {
            Position = position;
            Color = Color.White with { A = (byte)(MathHelper.Clamp(255 * alpha, 0, 255)) };
            TexCoord = texCoord;
        }
        public CustomVertexInfo(Vector2 position, Vector3 texCoord)
        {
            Position = position;
            Color = Color.White;
            TexCoord = texCoord;
        }

        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return _vertexDeclaration;
            }
        }
    }
    /// <summary>
    /// 支持空间顶点，非常酷齐次坐标
    /// </summary>
    public struct CustomVertexInfoEX : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
        {
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        });
        /// <summary>
        /// 使用齐次坐标！！
        /// </summary>
        public Vector4 Position;
        public Color Color;
        public Vector3 TexCoord;

        public CustomVertexInfoEX(Vector4 position, Color color, Vector3 texCoord)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
        }
        public CustomVertexInfoEX(Vector4 position, float alpha, Vector3 texCoord) : this(position, Color.White with { A = (byte)MathHelper.Clamp(255 * alpha, 0, 255) }, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector4 position, Vector3 texCoord) : this(position, Color.White, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector3 position, Color color, Vector3 texCoord) : this(new Vector4(position, 1), color, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector3 position, float alpha, Vector3 texCoord) : this(new Vector4(position, 1), alpha, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector3 position, Vector3 texCoord) : this(new Vector4(position, 1), texCoord)
        {
        }
        public CustomVertexInfoEX(Vector2 position, Color color, Vector3 texCoord) : this(new Vector4(position, 0, 1), color, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector2 position, float alpha, Vector3 texCoord) : this(new Vector4(position, 0, 1), alpha, texCoord)
        {
        }
        public CustomVertexInfoEX(Vector2 position, Vector3 texCoord) : this(new Vector4(position, 0, 1), texCoord)
        {
        }
        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return _vertexDeclaration;
            }
        }
    }
    /// <summary>
    /// 滞留剑气类
    /// </summary>
    public class UltraSwoosh
    {
        public float rotation;
        public byte timeLeft;
        public float xScaler;
        public Vector2 center;
        public bool negativeDir;
        public Texture2D heatMap;
        public Color color;
        public Vector3 hsl;
        public int type;
        public float checkAirFactor;
        public float rotationVelocity;
        public readonly CustomVertexInfo[] vertexInfos = new CustomVertexInfo[60];
        public bool Active => timeLeft > 0;
        public float scaler;
        public byte timeLeftMax;
        public (float from, float to) angleRange;
        public bool updateWithData;
    }
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
}

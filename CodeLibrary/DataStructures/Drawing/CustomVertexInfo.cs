using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.Drawing
{
    /// <summary>
    /// 满足一般顶点绘制需求
    /// </summary>
    public struct CustomVertexInfo : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new(
        [
                new (0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new (8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new (12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        ]);
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
            Color = Color.White with { A = (byte)MathHelper.Clamp(255 * alpha, 0, 255) };
            TexCoord = texCoord;
        }
        public CustomVertexInfo(Vector2 position, Vector3 texCoord)
        {
            Position = position;
            Color = Color.White;
            TexCoord = texCoord;
        }

        public VertexDeclaration VertexDeclaration => _vertexDeclaration;
    }
    /// <summary>
    /// 支持空间顶点，非常酷齐次坐标
    /// </summary>
    public struct CustomVertexInfoEX : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new(
        [
                new (0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new (16, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new (20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        ]);
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
        public VertexDeclaration VertexDeclaration => _vertexDeclaration;
    }
}

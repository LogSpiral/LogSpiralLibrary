using ReLogic.Graphics;
using System.Text;

namespace LogSpiralLibrary.CodeLibrary.DataStructures
{
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

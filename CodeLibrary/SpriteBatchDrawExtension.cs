namespace LogSpiralLibrary.CodeLibrary;

public static class SpriteBatchDrawExtension
{
    public unsafe static void PushSprite(this SpriteBatch spriteBatch,
Texture2D texture,
float sourceX,
float sourceY,
float sourceW,
float sourceH,
float destinationX,
float destinationY,
float destinationW,
float destinationH,
Color color0,
Color color1,
Color color2,
Color color3,
float originX,
float originY,
float rotationSin,
float rotationCos,
float depth,
byte effects
)
    {
        if (spriteBatch.numSprites >= spriteBatch.vertexInfo.Length)
        {
            if (spriteBatch.vertexInfo.Length >= SpriteBatch.MAX_ARRAYSIZE)
            {
                /* FIXME: We're doing this for safety but it's possible that
					 * XNA just keeps expanding and crashes with OutOfMemory.
					 * Since GraphicsProfile has a buffer cap, we use that for safety.
					 * This might change if someone depends on running out of memory(?!).
					 */
                spriteBatch.FlushBatch();
            }
            else
            {
                int newMax = Math.Min(spriteBatch.vertexInfo.Length * 2, SpriteBatch.MAX_ARRAYSIZE);
                Array.Resize(ref spriteBatch.vertexInfo, newMax);
                Array.Resize(ref spriteBatch.textureInfo, newMax);
                Array.Resize(ref spriteBatch.spriteInfos, newMax);
                Array.Resize(ref spriteBatch.sortedSpriteInfos, newMax);
            }
        }

        if (spriteBatch.sortMode == SpriteSortMode.Immediate)
        {
            int offset;
            fixed (SpriteBatch.VertexPositionColorTexture4* sprite = &spriteBatch.vertexInfo[0])
            {
                GenerateVertexInfo(
                    sprite,
                    sourceX,
                    sourceY,
                    sourceW,
                    sourceH,
                    destinationX,
                    destinationY,
                    destinationW,
                    destinationH,
                    color0,
                    color1,
                    color2,
                    color3,
                    originX,
                    originY,
                    rotationSin,
                    rotationCos,
                    depth,
                    effects
                );

                if (spriteBatch.supportsNoOverwrite)
                {
                    offset = spriteBatch.UpdateVertexBuffer(0, 1);
                }
                else
                {
                    /* We do NOT use Discard here because
						 * it would be stupid to reallocate the
						 * whole buffer just for one sprite.
						 *
						 * Unless you're using this to blit a
						 * target, stop using Immediate ya donut
						 * -flibit
						 */
                    offset = 0;
                    spriteBatch.vertexBuffer.SetDataPointerEXT(
                        0,
                        (IntPtr)sprite,
                        SpriteBatch.VertexPositionColorTexture4.RealStride,
                        SetDataOptions.None
                    );
                }
            }
            spriteBatch.DrawPrimitives(texture, offset, 1);
        }
        else if (spriteBatch.sortMode == SpriteSortMode.Deferred)
        {
            fixed (SpriteBatch.VertexPositionColorTexture4* sprite = &spriteBatch.vertexInfo[spriteBatch.numSprites])
            {
                GenerateVertexInfo(
                    sprite,
                    sourceX,
                    sourceY,
                    sourceW,
                    sourceH,
                    destinationX,
                    destinationY,
                    destinationW,
                    destinationH,
                    color0,
                    color1,
                    color2,
                    color3,
                    originX,
                    originY,
                    rotationSin,
                    rotationCos,
                    depth,
                    effects
                );
            }

            spriteBatch.textureInfo[spriteBatch.numSprites] = texture;
            spriteBatch.numSprites++;
        }
    }
    private static unsafe void GenerateVertexInfo(
 SpriteBatch.VertexPositionColorTexture4* sprite,
 float sourceX,
 float sourceY,
 float sourceW,
 float sourceH,
 float destinationX,
 float destinationY,
 float destinationW,
 float destinationH,
 Color color0,
 Color color1,
 Color color2,
 Color color3,
 float originX,
 float originY,
 float rotationSin,
 float rotationCos,
 float depth,
 byte effects
)
    {
        float cornerX = -originX * destinationW;
        float cornerY = -originY * destinationH;
        sprite->Position0.X = (
            (-rotationSin * cornerY) +
            (rotationCos * cornerX) +
            destinationX
        );
        sprite->Position0.Y = (
            (rotationCos * cornerY) +
            (rotationSin * cornerX) +
            destinationY
        );
        cornerX = (1.0f - originX) * destinationW;
        cornerY = -originY * destinationH;
        sprite->Position1.X = (
            (-rotationSin * cornerY) +
            (rotationCos * cornerX) +
            destinationX
        );
        sprite->Position1.Y = (
            (rotationCos * cornerY) +
            (rotationSin * cornerX) +
            destinationY
        );
        cornerX = -originX * destinationW;
        cornerY = (1.0f - originY) * destinationH;
        sprite->Position2.X = (
            (-rotationSin * cornerY) +
            (rotationCos * cornerX) +
            destinationX
        );
        sprite->Position2.Y = (
            (rotationCos * cornerY) +
            (rotationSin * cornerX) +
            destinationY
        );
        cornerX = (1.0f - originX) * destinationW;
        cornerY = (1.0f - originY) * destinationH;
        sprite->Position3.X = (
            (-rotationSin * cornerY) +
            (rotationCos * cornerX) +
            destinationX
        );
        sprite->Position3.Y = (
            (rotationCos * cornerY) +
            (rotationSin * cornerX) +
            destinationY
        );
        fixed (float* flipX = &SpriteBatch.CornerOffsetX[0])
        {
            fixed (float* flipY = &SpriteBatch.CornerOffsetY[0])
            {
                sprite->TextureCoordinate0.X = (flipX[0 ^ effects] * sourceW) + sourceX;
                sprite->TextureCoordinate0.Y = (flipY[0 ^ effects] * sourceH) + sourceY;
                sprite->TextureCoordinate1.X = (flipX[1 ^ effects] * sourceW) + sourceX;
                sprite->TextureCoordinate1.Y = (flipY[1 ^ effects] * sourceH) + sourceY;
                sprite->TextureCoordinate2.X = (flipX[2 ^ effects] * sourceW) + sourceX;
                sprite->TextureCoordinate2.Y = (flipY[2 ^ effects] * sourceH) + sourceY;
                sprite->TextureCoordinate3.X = (flipX[3 ^ effects] * sourceW) + sourceX;
                sprite->TextureCoordinate3.Y = (flipY[3 ^ effects] * sourceH) + sourceY;
            }
        }
        sprite->Position0.Z = depth;
        sprite->Position1.Z = depth;
        sprite->Position2.Z = depth;
        sprite->Position3.Z = depth;
        sprite->Color0 = color0;
        sprite->Color1 = color1;
        sprite->Color2 = color2;
        sprite->Color3 = color3;
    }

    public static void Draw4C(this SpriteBatch spriteBatch,
Texture2D texture,
Vector2 position,
 Color color0,
 Color color1,
 Color color2,
 Color color3
)
    {
        spriteBatch.CheckBegin("Draw");
        spriteBatch.PushSprite(
            texture,
            0.0f,
            0.0f,
            1.0f,
            1.0f,
            position.X,
            position.Y,
            texture.Width,
            texture.Height,
            color0,
            color1,
            color2,
            color3,
            0.0f,
            0.0f,
            0.0f,
            1.0f,
            0.0f,
            0
        );
    }

    public static void Draw4C(this SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
 Color color0,
 Color color1,
 Color color2,
 Color color3
    )
    {
        float sourceX, sourceY, sourceW, sourceH;
        float destW, destH;
        if (sourceRectangle.HasValue)
        {
            sourceX = sourceRectangle.Value.X / (float)texture.Width;
            sourceY = sourceRectangle.Value.Y / (float)texture.Height;
            sourceW = sourceRectangle.Value.Width / (float)texture.Width;
            sourceH = sourceRectangle.Value.Height / (float)texture.Height;
            destW = sourceRectangle.Value.Width;
            destH = sourceRectangle.Value.Height;
        }
        else
        {
            sourceX = 0.0f;
            sourceY = 0.0f;
            sourceW = 1.0f;
            sourceH = 1.0f;
            destW = texture.Width;
            destH = texture.Height;
        }
        spriteBatch.CheckBegin("Draw");
        spriteBatch.PushSprite(
            texture,
            sourceX,
            sourceY,
            sourceW,
            sourceH,
            position.X,
            position.Y,
            destW,
            destH,
            color0,
            color1,
            color2,
            color3,
            0.0f,
            0.0f,
            0.0f,
            1.0f,
            0.0f,
            0
        );
    }

    public static void Draw4C(this SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
 Color color0,
 Color color1,
 Color color2,
 Color color3,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth
    )
    {
        spriteBatch.CheckBegin("Draw");
        float sourceX, sourceY, sourceW, sourceH;
        float destW = scale;
        float destH = scale;
        if (sourceRectangle.HasValue)
        {
            sourceX = sourceRectangle.Value.X / (float)texture.Width;
            sourceY = sourceRectangle.Value.Y / (float)texture.Height;
            sourceW = Math.Sign(sourceRectangle.Value.Width) * Math.Max(
                Math.Abs(sourceRectangle.Value.Width),
                MathHelper.MachineEpsilonFloat
            ) / (float)texture.Width;
            sourceH = Math.Sign(sourceRectangle.Value.Height) * Math.Max(
                Math.Abs(sourceRectangle.Value.Height),
                MathHelper.MachineEpsilonFloat
            ) / (float)texture.Height;
            destW *= sourceRectangle.Value.Width;
            destH *= sourceRectangle.Value.Height;
        }
        else
        {
            sourceX = 0.0f;
            sourceY = 0.0f;
            sourceW = 1.0f;
            sourceH = 1.0f;
            destW *= texture.Width;
            destH *= texture.Height;
        }
        spriteBatch.PushSprite(
            texture,
            sourceX,
            sourceY,
            sourceW,
            sourceH,
            position.X,
            position.Y,
            destW,
            destH,
            color0,
            color1,
            color2,
            color3,
            origin.X / sourceW / (float)texture.Width,
            origin.Y / sourceH / (float)texture.Height,
            (float)Math.Sin(rotation),
            (float)Math.Cos(rotation),
            layerDepth,
            (byte)(effects & (SpriteEffects)0x03)
        );
    }

    public static void Draw4C(this SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
 Color color0,
 Color color1,
 Color color2,
 Color color3,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth
    )
    {
        spriteBatch.CheckBegin("Draw");
        float sourceX, sourceY, sourceW, sourceH;
        if (sourceRectangle.HasValue)
        {
            sourceX = sourceRectangle.Value.X / (float)texture.Width;
            sourceY = sourceRectangle.Value.Y / (float)texture.Height;
            sourceW = Math.Sign(sourceRectangle.Value.Width) * Math.Max(
                Math.Abs(sourceRectangle.Value.Width),
                MathHelper.MachineEpsilonFloat
            ) / (float)texture.Width;
            sourceH = Math.Sign(sourceRectangle.Value.Height) * Math.Max(
                Math.Abs(sourceRectangle.Value.Height),
                MathHelper.MachineEpsilonFloat
            ) / (float)texture.Height;
            scale.X *= sourceRectangle.Value.Width;
            scale.Y *= sourceRectangle.Value.Height;
        }
        else
        {
            sourceX = 0.0f;
            sourceY = 0.0f;
            sourceW = 1.0f;
            sourceH = 1.0f;
            scale.X *= texture.Width;
            scale.Y *= texture.Height;
        }
        spriteBatch.PushSprite(
            texture,
            sourceX,
            sourceY,
            sourceW,
            sourceH,
            position.X,
            position.Y,
            scale.X,
            scale.Y,
            color0,
            color1,
            color2,
            color3,
            origin.X / sourceW / (float)texture.Width,
            origin.Y / sourceH / (float)texture.Height,
            (float)Math.Sin(rotation),
            (float)Math.Cos(rotation),
            layerDepth,
            (byte)(effects & (SpriteEffects)0x03)
        );
    }

    public static void Draw4C(this SpriteBatch spriteBatch,
        Texture2D texture,
        Rectangle destinationRectangle,
 Color color0,
 Color color1,
 Color color2,
 Color color3
    )
    {
        spriteBatch.CheckBegin("Draw");
        spriteBatch.PushSprite(
            texture,
            0.0f,
            0.0f,
            1.0f,
            1.0f,
            destinationRectangle.X,
            destinationRectangle.Y,
            destinationRectangle.Width,
            destinationRectangle.Height,
            color0,
            color1,
            color2,
            color3,
            0.0f,
            0.0f,
            0.0f,
            1.0f,
            0.0f,
            0
        );
    }

    public static void Draw4C(this SpriteBatch spriteBatch,
        Texture2D texture,
        Rectangle destinationRectangle,
        Rectangle? sourceRectangle,
 Color color0,
 Color color1,
 Color color2,
 Color color3
    )
    {
        spriteBatch.CheckBegin("Draw");
        float sourceX, sourceY, sourceW, sourceH;
        if (sourceRectangle.HasValue)
        {
            sourceX = sourceRectangle.Value.X / (float)texture.Width;
            sourceY = sourceRectangle.Value.Y / (float)texture.Height;
            sourceW = sourceRectangle.Value.Width / (float)texture.Width;
            sourceH = sourceRectangle.Value.Height / (float)texture.Height;
        }
        else
        {
            sourceX = 0.0f;
            sourceY = 0.0f;
            sourceW = 1.0f;
            sourceH = 1.0f;
        }
        spriteBatch.PushSprite(
            texture,
            sourceX,
            sourceY,
            sourceW,
            sourceH,
            destinationRectangle.X,
            destinationRectangle.Y,
            destinationRectangle.Width,
            destinationRectangle.Height,
            color0,
            color1,
            color2,
            color3,
            0.0f,
            0.0f,
            0.0f,
            1.0f,
            0.0f,
            0
        );
    }

    public static void Draw4C(this SpriteBatch spriteBatch,
        Texture2D texture,
        Rectangle destinationRectangle,
        Rectangle? sourceRectangle,
 Color color0,
 Color color1,
 Color color2,
 Color color3,
        float rotation,
        Vector2 origin,
        SpriteEffects effects,
        float layerDepth
    )
    {
        spriteBatch.CheckBegin("Draw");
        float sourceX, sourceY, sourceW, sourceH;
        if (sourceRectangle.HasValue)
        {
            sourceX = sourceRectangle.Value.X / (float)texture.Width;
            sourceY = sourceRectangle.Value.Y / (float)texture.Height;
            sourceW = Math.Sign(sourceRectangle.Value.Width) * Math.Max(
                Math.Abs(sourceRectangle.Value.Width),
                MathHelper.MachineEpsilonFloat
            ) / (float)texture.Width;
            sourceH = Math.Sign(sourceRectangle.Value.Height) * Math.Max(
                Math.Abs(sourceRectangle.Value.Height),
                MathHelper.MachineEpsilonFloat
            ) / (float)texture.Height;
        }
        else
        {
            sourceX = 0.0f;
            sourceY = 0.0f;
            sourceW = 1.0f;
            sourceH = 1.0f;
        }
        spriteBatch.PushSprite(
            texture,
            sourceX,
            sourceY,
            sourceW,
            sourceH,
            destinationRectangle.X,
            destinationRectangle.Y,
            destinationRectangle.Width,
            destinationRectangle.Height,
            color0,
            color1,
            color2,
            color3,
            origin.X / sourceW / (float)texture.Width,
            origin.Y / sourceH / (float)texture.Height,
            (float)Math.Sin(rotation),
            (float)Math.Cos(rotation),
            layerDepth,
            (byte)(effects & (SpriteEffects)0x03)
        );
    }
}

//    public static class SpriteBatchDrawExtension
//    {
//        private struct VertexPositionColorTexture4
//        {
//            public const int RealStride = 96;

//            public Vector3 Position0;
//            public Color Color0;
//            public Vector2 TextureCoordinate0;
//            public Vector3 Position1;
//            public Color Color1;
//            public Vector2 TextureCoordinate1;
//            public Vector3 Position2;
//            public Color Color2;
//            public Vector2 TextureCoordinate2;
//            public Vector3 Position3;
//            public Color Color3;
//            public Vector2 TextureCoordinate3;
//        }
//        private static readonly float[] CornerOffsetX = [0, 1, 0, 1];
//        private static readonly float[] CornerOffsetY = [0, 0, 1, 1];
//        private const int MAX_ARRAYSIZE = 2048;
//        static FieldInfo numSpritesInfo;
//        static FieldInfo vertexInfoInfo;
//        static FieldInfo sortModeInfo;
//        static FieldInfo supportsNoOverwriteInfo;
//        static FieldInfo vertexBufferInfo;
//        static FieldInfo textureInfoInfo;
//        static MethodInfo FlushBatchInfo;
//        static MethodInfo UpdateVertexBufferInfo;
//        static MethodInfo DrawPrimitivesInfo;

//        static SpriteBatchDrawExtension()
//        {
//            var type = typeof(SpriteBatch);
//            var flag = BindingFlags.Instance | BindingFlags.NonPublic;
//            numSpritesInfo = type.GetField("numSprites", flag);
//            vertexInfoInfo = type.GetField("vertexInfo", flag);
//            sortModeInfo = type.GetField("sortMode", flag);
//            supportsNoOverwriteInfo = type.GetField("supportsNoOverwrite", flag);
//            vertexBufferInfo = type.GetField("vertexBuffer", flag);
//            textureInfoInfo = type.GetField("textureInfo", flag);

//            FlushBatchInfo = type.GetMethod("FlushBatch", flag);
//            UpdateVertexBufferInfo = type.GetMethod("UpdateVertexBuffer", flag);
//            DrawPrimitivesInfo = type.GetMethod("DrawPrimitives", flag);
//        }
//        private static int get_numSprites(this SpriteBatch spriteBatch) => (int)numSpritesInfo.GetValue(spriteBatch);
//        private static void set_numSprites(this SpriteBatch spriteBatch, int value) => numSpritesInfo.SetValue(spriteBatch, value);
//        private static object[] get_vertexInfo(this SpriteBatch spriteBatch) 
//        {
//            Array array = (Array)vertexInfoInfo.GetValue(spriteBatch);
//            object[] result = new object[array.Length];
//            Array.Copy(array, result, array.Length);
//            return result;
//        }
//        private static SpriteSortMode get_SortMode(this SpriteBatch spriteBatch) => (SpriteSortMode)sortModeInfo.GetValue(spriteBatch);
//        private static bool get_supportsNoOverwrite(this SpriteBatch spriteBatch) => (bool)supportsNoOverwriteInfo.GetValue(spriteBatch);
//        private static DynamicVertexBuffer get_vertexBuffer(this SpriteBatch spriteBatch) => (DynamicVertexBuffer)vertexBufferInfo.GetValue(spriteBatch);
//        private static Texture2D[] get_textureInfo(this SpriteBatch spriteBatch) => (Texture2D[])textureInfoInfo.GetValue(spriteBatch);
//        private static void call_FlushBatchInfo(this SpriteBatch spriteBatch) => FlushBatchInfo.Invoke(spriteBatch, []);
//        private static void call_DrawPrimitives(this SpriteBatch spriteBatch, Texture2D texture, int baseSprite, int batchSize) => DrawPrimitivesInfo.Invoke(spriteBatch, [texture, baseSprite, batchSize]);
//        private static int call_UpdateVertexBuffer(this SpriteBatch spriteBatch, int start, int count) => (int)UpdateVertexBufferInfo.Invoke(spriteBatch, [start, count]);
//        public unsafe static void PushSprite(this SpriteBatch spriteBatch,
//    Texture2D texture,
//    float sourceX,
//    float sourceY,
//    float sourceW,
//    float sourceH,
//    float destinationX,
//    float destinationY,
//    float destinationW,
//    float destinationH,
//    Color color0,
//    Color color1,
//    Color color2,
//    Color color3,
//    float originX,
//    float originY,
//    float rotationSin,
//    float rotationCos,
//    float depth,
//    byte effects
//)
//        {
//            if (spriteBatch.get_numSprites() >= spriteBatch.get_vertexInfo().Length)
//            {
//                if (spriteBatch.get_vertexInfo().Length >= MAX_ARRAYSIZE)
//                {
//                    /* FIXME: We're doing this for safety but it's possible that
//					 * XNA just keeps expanding and crashes with OutOfMemory.
//					 * Since GraphicsProfile has a buffer cap, we use that for safety.
//					 * This might change if someone depends on running out of memory(?!).
//					 */
//                    spriteBatch.call_FlushBatchInfo();
//                }
//                else
//                {
//                    //我放弃乐，不知道会不会炸((
//                    /*int newMax = Math.Min(spriteBatch.get_vertexInfo().Length * 2, MAX_ARRAYSIZE);
//                    Array.Resize(ref spriteBatch.get_vertexInfo(), newMax);
//                    Array.Resize(ref spriteBatch.textureInfo, newMax);
//                    Array.Resize(ref spriteBatch.spriteInfos, newMax);
//                    Array.Resize(ref spriteBatch.sortedSpriteInfos, newMax);*/
//                }
//            }

//            if (spriteBatch.get_SortMode() == SpriteSortMode.Immediate)
//            {
//                int offset;
//                fixed (void* _sprite = &spriteBatch.get_vertexInfo()[0])
//                {
//                    VertexPositionColorTexture4* sprite = (VertexPositionColorTexture4*)_sprite;
//                    GenerateVertexInfo(
//                        sprite,
//                        sourceX,
//                        sourceY,
//                        sourceW,
//                        sourceH,
//                        destinationX,
//                        destinationY,
//                        destinationW,
//                        destinationH,
//                        color0,
//                        color1,
//                        color2,
//                        color3,
//                        originX,
//                        originY,
//                        rotationSin,
//                        rotationCos,
//                        depth,
//                        effects
//                    );

//                    if (spriteBatch.get_supportsNoOverwrite())
//                    {
//                        offset = spriteBatch.call_UpdateVertexBuffer(0, 1);
//                    }
//                    else
//                    {
//                        /* We do NOT use Discard here because
//						 * it would be stupid to reallocate the
//						 * whole buffer just for one sprite.
//						 *
//						 * Unless you're using this to blit a
//						 * target, stop using Immediate ya donut
//						 * -flibit
//						 */
//                        offset = 0;
//                        spriteBatch.get_vertexBuffer().SetDataPointerEXT(
//                            0,
//                            (IntPtr)sprite,
//                            VertexPositionColorTexture4.RealStride,
//                            SetDataOptions.None
//                        );
//                    }
//                }
//                spriteBatch.call_DrawPrimitives(texture, offset, 1);
//            }
//            else if (spriteBatch.get_SortMode() == SpriteSortMode.Deferred)
//            {
//                fixed (void* _sprite = &spriteBatch.get_vertexInfo()[spriteBatch.get_numSprites()])
//                {
//                    var sprite = (VertexPositionColorTexture4*)_sprite;
//                    GenerateVertexInfo(
//                        sprite,
//                        sourceX,
//                        sourceY,
//                        sourceW,
//                        sourceH,
//                        destinationX,
//                        destinationY,
//                        destinationW,
//                        destinationH,
//                        color0,
//                        color1,
//                        color2,
//                        color3,
//                        originX,
//                        originY,
//                        rotationSin,
//                        rotationCos,
//                        depth,
//                        effects
//                    );
//                }

//                spriteBatch.get_textureInfo()[spriteBatch.get_numSprites()] = texture;
//                spriteBatch.set_numSprites(spriteBatch.get_numSprites() + 1);
//            }
//        }
//        private static unsafe void GenerateVertexInfo(
//     VertexPositionColorTexture4* sprite,
//     float sourceX,
//     float sourceY,
//     float sourceW,
//     float sourceH,
//     float destinationX,
//     float destinationY,
//     float destinationW,
//     float destinationH,
//     Color color0,
//     Color color1,
//     Color color2,
//     Color color3,
//     float originX,
//     float originY,
//     float rotationSin,
//     float rotationCos,
//     float depth,
//     byte effects
// )
//        {
//            float cornerX = -originX * destinationW;
//            float cornerY = -originY * destinationH;
//            sprite->Position0.X = (
//                (-rotationSin * cornerY) +
//                (rotationCos * cornerX) +
//                destinationX
//            );
//            sprite->Position0.Y = (
//                (rotationCos * cornerY) +
//                (rotationSin * cornerX) +
//                destinationY
//            );
//            cornerX = (1.0f - originX) * destinationW;
//            cornerY = -originY * destinationH;
//            sprite->Position1.X = (
//                (-rotationSin * cornerY) +
//                (rotationCos * cornerX) +
//                destinationX
//            );
//            sprite->Position1.Y = (
//                (rotationCos * cornerY) +
//                (rotationSin * cornerX) +
//                destinationY
//            );
//            cornerX = -originX * destinationW;
//            cornerY = (1.0f - originY) * destinationH;
//            sprite->Position2.X = (
//                (-rotationSin * cornerY) +
//                (rotationCos * cornerX) +
//                destinationX
//            );
//            sprite->Position2.Y = (
//                (rotationCos * cornerY) +
//                (rotationSin * cornerX) +
//                destinationY
//            );
//            cornerX = (1.0f - originX) * destinationW;
//            cornerY = (1.0f - originY) * destinationH;
//            sprite->Position3.X = (
//                (-rotationSin * cornerY) +
//                (rotationCos * cornerX) +
//                destinationX
//            );
//            sprite->Position3.Y = (
//                (rotationCos * cornerY) +
//                (rotationSin * cornerX) +
//                destinationY
//            );
//            fixed (float* flipX = &CornerOffsetX[0])
//            {
//                fixed (float* flipY = &CornerOffsetY[0])
//                {
//                    sprite->TextureCoordinate0.X = (flipX[0 ^ effects] * sourceW) + sourceX;
//                    sprite->TextureCoordinate0.Y = (flipY[0 ^ effects] * sourceH) + sourceY;
//                    sprite->TextureCoordinate1.X = (flipX[1 ^ effects] * sourceW) + sourceX;
//                    sprite->TextureCoordinate1.Y = (flipY[1 ^ effects] * sourceH) + sourceY;
//                    sprite->TextureCoordinate2.X = (flipX[2 ^ effects] * sourceW) + sourceX;
//                    sprite->TextureCoordinate2.Y = (flipY[2 ^ effects] * sourceH) + sourceY;
//                    sprite->TextureCoordinate3.X = (flipX[3 ^ effects] * sourceW) + sourceX;
//                    sprite->TextureCoordinate3.Y = (flipY[3 ^ effects] * sourceH) + sourceY;
//                }
//            }
//            sprite->Position0.Z = depth;
//            sprite->Position1.Z = depth;
//            sprite->Position2.Z = depth;
//            sprite->Position3.Z = depth;
//            sprite->Color0 = color0;
//            sprite->Color1 = color1;
//            sprite->Color2 = color2;
//            sprite->Color3 = color3;
//        }
//    }

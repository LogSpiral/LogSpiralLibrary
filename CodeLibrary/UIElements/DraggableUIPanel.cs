using Humanizer;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.ComplexPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LogSpiralLibrary.CodeLibrary.UIElements
{

    public class DraggableUIPanel : UIPanel
    {
        public bool Dragging = false;
        public bool fixedSize;
        public Vector2 Offset;
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (evt.Target == this)
            {
                Dragging = true;
                var dimension = GetDimensions();
                if (Vector2.Dot(evt.MousePosition - dimension.Position(), Vector2.One / new Vector2(dimension.Width, dimension.Height)) <= 1)
                {
                    fixedSize = true;
                    Offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);

                }
                else
                {
                    fixedSize = false;
                    Offset = new Vector2(evt.MousePosition.X - Width.Pixels, evt.MousePosition.Y - Height.Pixels);
                }
            }
            base.LeftMouseDown(evt);
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            Dragging = false;
            base.LeftMouseUp(evt);
        }
        public override void Update(GameTime gameTime)
        {
            if (Dragging)
            {
                if (fixedSize)
                {
                    Left.Set(Main.mouseX - Offset.X, 0f);
                    Top.Set(Main.mouseY - Offset.Y, 0f);
                }
                else
                {
                    Width.Set(Main.mouseX - Offset.X, 0f);
                    Height.Set(Main.mouseY - Offset.Y, 0f);
                }
                Recalculate();
            }
            base.Update(gameTime);
        }
        public float glowFactor;
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dimensions = GetDimensions();
            glowFactor = MathHelper.Lerp(glowFactor, IsMouseHovering ? 1f : 0f, 0.05f);
            ComplexPanelInfo panel = new()
            {
                destination = new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width + 1, (int)dimensions.Height),
                StyleTexture = ModContent.Request<Texture2D>($"LogSpiralLibrary/Images/ComplexPanel/panel_0").Value,
                glowEffectColor = Color.Lerp(default, Color.MediumPurple with { A = 0 } * .125f, glowFactor),
                glowShakingStrength = 1f,
                glowHueOffsetRange = 0.2f,
                backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value,
                backgroundFrame = new Rectangle(4, 4, 28, 28),
                backgroundUnitSize = new Vector2(28, 28) * 2f,
                backgroundColor = BackgroundColor with { A = 51 }
            };
            panel.DrawComplexPanel(spriteBatch);
            //base.DrawSelf(spriteBatch);
        }
    }
}

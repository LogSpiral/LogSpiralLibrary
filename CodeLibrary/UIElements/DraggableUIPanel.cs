using Humanizer;
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
    }
}

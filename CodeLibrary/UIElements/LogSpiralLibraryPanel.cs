using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;

namespace LogSpiralLibrary.CodeLibrary.UIElements
{
    public class LogSpiralLibraryPanel : UIPanel
    {
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var color = BackgroundColor;
            BackgroundColor *= .5f;
            base.DrawSelf(spriteBatch);
            BackgroundColor = color;
        }
    }
}

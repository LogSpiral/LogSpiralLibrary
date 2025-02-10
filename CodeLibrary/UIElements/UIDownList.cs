using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LogSpiralLibrary.CodeLibrary.UIElements
{
    public class UIDownList : UIState
    {
        public bool expand;
        public bool pendingExpand;
        public UIList contentList;
        public LogSpiralLibraryPanel TitlePanel;
        public string title;
        public override void OnInitialize()
        {
            TitlePanel = new LogSpiralLibraryPanel()
            {
                Width = new(0, 0.75f),
                Height = new(40, 0)
            };
            TitlePanel.OnLeftClick += (evt, elem) =>
            {
                expand = !expand;
                pendingExpand = true;
                SoundEngine.PlaySound(expand ? SoundID.MenuOpen : SoundID.MenuClose);
            };
            Append(TitlePanel);
            contentList = new UIList()
            {
                Left = new(16,0),
                Width = new(0, 1),
                ListPadding = 16
            };
            //Append(contentList);
            base.OnInitialize();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!pendingExpand) return;
            contentList.Remove();
            if (expand)
                Append(contentList);
            (Parent ?? this).Recalculate();
        }
        public void Add(UIElement element)
        {
            contentList.Add(element);
        }
        public override void Recalculate()
        {
            if (contentList == null || TitlePanel == null) return;
            contentList.Top.Set(TitlePanel.Height.Pixels + 10, 0);
            contentList.Height.Set(contentList.GetTotalHeight(), 0);
            Height.Set((expand ? contentList.Height.Pixels : 0) + TitlePanel.Height.Pixels + 10, 0);
            base.Recalculate();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            var titleDimension = TitlePanel.GetDimensions();
            var font = FontAssets.MouseText.Value;
            //spriteBatch.DrawString(font, title, titleDimension.ToRectangle().Left(), Color.White, 0, font.MeasureString(title) * .5f * Vector2.UnitY, 1f, 0, 0);
            spriteBatch.DrawString(font, title, titleDimension.Center(), Color.White, 0, font.MeasureString(title) * .5f, 1f, 0, 0);

        }
        public UIDownList(string titleName)
        {
            title = titleName;
        }
    }
}

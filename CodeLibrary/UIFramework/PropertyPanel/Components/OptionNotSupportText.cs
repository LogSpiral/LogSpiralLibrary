using SilkyUIFramework.Extensions;
using SilkyUIFramework;
using SilkyUIFramework.BasicElements;
using System.Linq;
using Terraria.Localization;

namespace LogSpiralLibrary.CodeLibrary.UIFramework.PropertyPanel.Components
{
    /// <summary>
    /// 单纯显示一行文字
    /// 嗯？你问我为什么不用SUIText 因为设置和文本混杂在一起的时候用那个会被刷掉，动_allOptions感觉会出更多很麻烦的东西
    /// </summary>
    internal class OptionNotSupportText : OptionBase
    {
        protected override void OnBind()
        {
            CheckExternalModify = false;
            var variable = VariableInfo;
            //TODO 本地化+对多级嵌套的正确支持

            //ConfigHelper.GetLabel(Config, variable.Name)
            var text = new UITextView
            {
                Text  =  Language.GetTextValue("Mods.LogSpiralLibrary.PropertyPanel.NotSupportText", [Handler.GetLabel().Value, variable.Name, variable.Type]),
                TextAlign = new Vector2(0f),
                WordWrap = true,
                TextScale = 1.1f
            };
            text.SetWidth(0, 1);
            text.Join(this);
            text.OnTextChanged += delegate
            {
                text.SetHeight(text.TextSize.Y + 60, 0);
            };
        }
        public override void OnLeftMouseDown(UIMouseEvent evt)
        {
            base.OnLeftMouseDown(evt);
            Handler.HandleNotSupport();
        }
    }
}

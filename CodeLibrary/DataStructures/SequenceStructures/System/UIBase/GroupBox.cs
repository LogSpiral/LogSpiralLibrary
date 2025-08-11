//using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.OldSources;
//using LogSpiralLibrary.CodeLibrary.UIElements;
//using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
//using System.Collections.Generic;
//using Terraria.GameContent.UI.Elements;
//using Terraria.UI;

//namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UIBase;
//public class GroupBox : UIElement
//{
//    public GroupBox(Sequence.GroupBase groupBase)
//    {
//        group = groupBase;
//        foreach (var w in group.Wrapers)
//        {
//            var wbox = new WraperBox(w);
//            wraperBoxes.Add(wbox);

//        }
//    }
//    public UIPanel panel;
//    public Sequence.GroupBase group;
//    public List<WraperBox> wraperBoxes = [];
//    public bool CacheRefresh;
//    //public void Add(WraperBox wraperBox)
//    //{
//    //    wraperBoxes.Add(wraperBox);
//    //    Elements.Clear();
//    //    group.Wrapers.Add(wraperBox.wraper);
//    //    OnInitialize();
//    //}
//    public override void OnInitialize()
//    {
//        //this.IgnoresMouseInteraction = true;
//        Vector2 size = this.GetSize();
//        panel = new LogSpiralLibraryPanel();
//        panel.SetSize(size);
//        Elements.Clear();
//        float offset = SequenceConfig.Instance.Step.Y * .5f;
//        foreach (var w in wraperBoxes)
//        {
//            w.Top.Set(offset, 0f);
//            Append(w);
//            var dimension = w.GetDimensions();
//            offset += dimension.Height + SequenceConfig.Instance.Step.Y;
//            w.Left.Set((size.X - dimension.Width) * .5f, 0f);
//            w.OnInitialize();
//        }
//        base.OnInitialize();
//    }
//    public override void DrawSelf(SpriteBatch spriteBatch)
//    {
//        if (SequenceConfig.Instance.ShowGroupBox)
//            spriteBatch.DrawRectangle(GetDimensions().ToRectangle(), Color.Cyan * .75f, 8);
//        var dimension = GetDimensions();
//        var scale = 1 + ((float)LogSpiralLibraryMod.ModTime / 180).CosFactor();

//        foreach (var w in wraperBoxes)
//        {
//            var wD = w.GetDimensions();
//            var offY = w.wraper.Condition.Description.Key != SequenceSystem.AlwaysConditionKey ? -FontAssets.MouseText.Value.MeasureString("→" + w.wraper.Condition.Description.Value).Y * .5f : 0;
//            //谜题之我也不知道这一个像素的偏移是哪里来的
//            spriteBatch.DrawHorizonBLine(dimension.Position() + new Vector2(-SequenceConfig.Instance.Step.X * .25f, dimension.Height * .5f) + new Vector2(-1, 0), wD.Position() + new Vector2(0, wD.Height * .5f + offY), Color.White, scale);
//            spriteBatch.DrawHorizonBLine(dimension.Position() + new Vector2(SequenceConfig.Instance.Step.X * .25f + dimension.Width, dimension.Height * .5f) + new Vector2(-1, 0), wD.Position() + new Vector2(wD.Width, wD.Height * .5f + offY), Color.White, scale);

//            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, wD.Position() + new Vector2(0, wD.Height * .5f), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(0.5f), 16f, 0, 0);
//        }
//        base.DrawSelf(spriteBatch);
//    }
//    public override void Update(GameTime gameTime)
//    {
//        MaxWidth = MaxHeight = new StyleDimension(223214514, 0);

//        base.Update(gameTime);
//    }
//}

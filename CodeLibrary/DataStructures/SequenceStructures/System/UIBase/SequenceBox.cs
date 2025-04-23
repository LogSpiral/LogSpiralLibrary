using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.UIElements;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UIBase;

public class SequenceBox : UIElement
{
    bool expand = true;
    public bool Expand
    {
        get => expand;
        set
        {
            bool flag = expand ^ value;
            if (!flag) return;
            expand = value;
            if (expand)
            {
                CacheRefresh = true;
                OnInitialize();
            }
            else
            {
                Elements.Clear();
                CacheRefresh = true;
                this.SequenceSize(true);
            }
        }
    }
    public UIPanel panel;
    Sequence seq;
    public Sequence sequenceBase
    {
        get => seq;
        set
        {
            string name = seq?.DisplayName;
            seq = value;
        }
    }
    public List<GroupBox> groupBoxes = [];
    public bool CacheRefresh;
    public float offY;
    //public bool startSequence;
    public SequenceBox(Sequence sequence)
    {
        sequenceBase = sequence;
        foreach (var g in sequence.GroupBases)
        {
            var gbox = new GroupBox(g);

            groupBoxes.Add(gbox);

        }
    }
    /// <summary>
    /// 检测鼠标是否在空隙中(也就是非打包器区域
    /// </summary>
    /// <returns></returns>
    public bool MouseCheckInEmptySpace(Vector2 position)
    {
        foreach (var g in groupBoxes)
        {
            foreach (var w in g.wraperBoxes)
            {
                if (w.GetDimensions().ToRectangle().Contains(position.ToPoint()))
                    return false;
            }
        }
        return true;
    }
    public void InsertWraper(WraperBox wraper, Vector2 center)
    {
        if (!ContainsPoint(center))
            return;
        int gIndex = 0;
        bool inGroup = false;
        foreach (var g in groupBoxes)
        {
            if (center.X > g.GetDimensions().ToRectangle().Right)
                gIndex++;
            else if (center.X > g.GetDimensions().ToRectangle().Left)
                inGroup = true;
        }
        if (inGroup)
        {
            int wIndex = 0;
            bool inWraper = false;
            GroupBox groupBox = groupBoxes[gIndex];
            foreach (var w in groupBox.wraperBoxes)
            {
                if (center.Y > w.GetDimensions().ToRectangle().Bottom)
                    wIndex++;
                else if (center.Y > w.GetDimensions().ToRectangle().Top)
                    inWraper = true;
            }
            if (inWraper)
            {
                WraperBox wraperBox = groupBox.wraperBoxes[wIndex];
                if (wraperBox.wraper.IsSequence)
                {
                    wraperBox.sequenceBox.InsertWraper(wraper, center);
                    wraperBox.SetSize(wraperBox.sequenceBox.GetSize());
                    groupBox.CacheRefresh = true;
                    groupBox.GroupSize();
                }
                else
                {
                    //出口三，和先前的元素组成新的序列
                    //Main.NewText("这里本来有个出口三，但是我还没做完((");

                    bool flag = center.X < wraperBox.GetDimensions().Center().X;

                    var seq = (Sequence)Activator.CreateInstance(sequenceBase.GetType());
                    var nW = (Sequence.WraperBase)Activator.CreateInstance(wraper.wraper.GetType(), seq);
                    for (int n = 0; n < 2; n++)
                    {
                        seq.Add(flag ? wraper.wraper : wraperBox.wraper, out _);
                        flag = !flag;
                    }
                    groupBox.group.Replace(wIndex, nW);
                    groupBox.wraperBoxes[wIndex] = new WraperBox(nW);
                    groupBox.CacheRefresh = true;
                }
                wraperBox.ParentCacheRefreshSet();
            }
            else
            {
                //出口二，作为独立单元并联
                sequenceBase.GroupBases[gIndex].Insert(wIndex, wraper.wraper);
                groupBoxes[gIndex].wraperBoxes.Insert(wIndex, wraper);
                groupBoxes[gIndex].CacheRefresh = true;
            }
        }
        else
        {
            //出口一，作为独立单元串联
            sequenceBase.Insert(gIndex, wraper.wraper, out var nG);
            groupBoxes.Insert(gIndex, new GroupBox(nG));
            if (wraper.wraper.IsSequence)
                groupBoxes[gIndex].wraperBoxes[0].sequenceBox.expand = wraper.sequenceBox.expand;

        }
        var box = this;
        box.Elements.Clear();
        box.CacheRefresh = true;
        box.OnInitialize();
        box.Recalculate();
    }
    public override void Update(GameTime gameTime)
    {
        if (CacheRefresh)
            Recalculate();
        MaxWidth = MaxHeight = new StyleDimension(223214514, 0);
        base.Update(gameTime);
    }
    public override void OnInitialize()
    {
        //this.IgnoresMouseInteraction = true;
        Vector2 size = this.SequenceSize(true);
        panel = new LogSpiralLibraryPanel();
        panel.SetSize(size);
        float offset = SequenceConfig.Instance.Step.X * .5f + 16;
        //if (!startSequence)
        //{
        //    offset += 16;
        //}
        //startSequence = false;
        foreach (var g in groupBoxes)
        {
            g.Left.Set(offset, 0f);
            Append(g);
            var dimension = g.GetDimensions();
            offset += dimension.Width + SequenceConfig.Instance.Step.X;
            g.Top.Set((size.Y - dimension.Height) * .5f + offY, 0f);
            g.OnInitialize();
        }
        base.OnInitialize();
    }
    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dimension = GetDimensions();
        Vector2 pos = dimension.Position() + new Vector2(0, dimension.Height * .5f + offY);
        Vector2 startP = pos + new Vector2(SequenceConfig.Instance.Step.X * .25f + 16 - 1, 0);//(startSequence ? 0 : 16)
        spriteBatch.DrawLine(pos - Vector2.UnitX, startP, Color.White);
        int counter = 0;
        foreach (var g in groupBoxes)
        {
            counter++;
            startP += new Vector2(SequenceConfig.Instance.Step.X * .5f + g.GetDimensions().Width, 0);
            Vector2 endP = startP + new Vector2(SequenceConfig.Instance.Step.X * 0.5f, 0);
            if (counter == groupBoxes.Count)
                endP = pos + new Vector2(dimension.Width - 1, 0);
            spriteBatch.DrawLine(startP, endP, Color.White);
            startP = endP;
        }
        if (SequenceConfig.Instance.ShowSequenceBox)
            spriteBatch.DrawRectangle(dimension.ToRectangle(), Color.Red * .5f);
        base.DrawSelf(spriteBatch);
    }
}

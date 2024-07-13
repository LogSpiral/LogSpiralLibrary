using Newtonsoft.Json;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures
{
    public class SequenceConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [DefaultValue(false)]
        public bool ShowWrapBox = false;
        [DefaultValue(false)]
        public bool ShowGroupBox = false;
        [DefaultValue(false)]
        public bool ShowSequenceBox = false;
        [DefaultValue(typeof(Vector2), "32, 16")]
        [Range(0f, 64f)]
        public Vector2 Step = new Vector2(32, 16);
        public static SequenceConfig Instance => ModContent.GetInstance<SequenceConfig>();
        public override void OnChanged()
        {
            SequenceSystem.instance?.sequenceUI?.SetupConfigList();
            base.OnChanged();
        }
    }
    public class SequencePlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (SequenceSystem.ShowSequenceKeybind.JustPressed)
            {
                if (SequenceUI.Visible)
                    SequenceSystem.instance.sequenceUI.Close();
                else
                    SequenceSystem.instance.sequenceUI.Open();
            }
            base.ProcessTriggers(triggersSet);
        }
    }
    public class SequenceSystem : ModSystem
    {
        //TODO 可以给其它类型的序列用
        public static Dictionary<string, SequenceBase> sequenceBases => instance._sequenceBases;
        Dictionary<string, SequenceBase> _sequenceBases = new Dictionary<string, SequenceBase>();
        public SequenceUI sequenceUI;
        UserInterface userInterfaceSequence;
        public static ModKeybind ShowSequenceKeybind { get; private set; }
        public static SequenceSystem instance;
        public static Dictionary<string, Condition> conditions = new Dictionary<string, Condition>();
        public override void Load()
        {
            instance = this;
            sequenceUI = new SequenceUI();
            userInterfaceSequence = new UserInterface();
            sequenceUI.Activate();
            userInterfaceSequence.SetState(sequenceUI);
            ShowSequenceKeybind = KeybindLoader.RegisterKeybind(Mod, "展示序列列表", "Y");
            #region conditions的赋值
            var fieldInfos = typeof(Condition).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var fieldInfo in fieldInfos)
            {
                Condition condition = (Condition)fieldInfo.GetValue(null);
                string key = condition.Description.Key.Split('.')[^1];
                if (!conditions.ContainsKey(key))
                    conditions.Add(key, condition);
            }//录入原版条件
            var mouseLeftCondition = new Condition(Language.GetOrRegister("Mods.LogSpiralLibrary.Condition.MouseLeft"), () => Main.LocalPlayer.controlUseItem);
            conditions.Add("MouseLeft", mouseLeftCondition);
            var mouseRightCondition = new Condition(Language.GetOrRegister("Mods.LogSpiralLibrary.Condition.MouseRight"), () => Main.LocalPlayer.controlUseTile);
            conditions.Add("MouseRight", mouseRightCondition);
            var surroundThreatCondition = new Condition(Language.GetOrRegister("Mods.LogSpiralLibrary.Condition.SurroundThreat"), () =>
            {
                SurroundStatePlayer ssp = Main.LocalPlayer.GetModPlayer<SurroundStatePlayer>();
                return ssp.state == SurroundState.SurroundThreat;
            });
            conditions.Add("SurroundThreat", surroundThreatCondition);
            var frontThreatCondition = new Condition(Language.GetOrRegister("Mods.LogSpiralLibrary.Condition.FrontThreat"), () =>
            {
                SurroundStatePlayer ssp = Main.LocalPlayer.GetModPlayer<SurroundStatePlayer>();
                return ssp.state == SurroundState.FrontThreat;
            });
            conditions.Add("FrontThreat", frontThreatCondition);
            #endregion

        }
        public override void Unload()
        {
            instance = null;
            base.Unload();
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (SequenceUI.Visible)
                userInterfaceSequence?.Update(gameTime);
            base.UpdateUI(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            //寻找一个名字为Vanilla: Mouse Text的绘制层，也就是绘制鼠标字体的那一层，并且返回那一层的索引
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            //寻找到索引时
            if (MouseTextIndex != -1)
            {
                //往绘制层集合插入一个成员，第一个参数是插入的地方的索引，第二个参数是绘制层
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
                   //这里是绘制层的名字
                   "LogSpiralLibrary:SequenceUI",
                   //这里是匿名方法
                   delegate
                   {
                       //当Visible开启时（当UI开启时）
                       if (SequenceUI.Visible)
                           //绘制UI（运行exampleUI的Draw方法）
                           sequenceUI.Draw(Main.spriteBatch);
                       return true;
                   },
                   //这里是绘制层的类型
                   InterfaceScaleType.UI)
               );
            }
            base.ModifyInterfaceLayers(layers);
        }
    }
    public class SequenceUI : UIState
    {
        public static bool Visible = false;
        public override void OnInitialize()
        {
            UIPanel panel = new UIPanel();
            panel.SetSize(new Vector2(240, 300));
            panel.Top.Set(80, 0);
            panel.Left.Set(100, 0);
            Append(panel);
            UIList = new UIList();
            UIList.ListPadding = 24f;
            UIList.SetSize(200, 400);

            panel.Append(UIList);
            SequenceDrawer = new SequenceDrawer();
            SequenceDrawer.Top.Set(240, 0);
            SequenceDrawer.Left.Set(0, 1.25f);
            panel.Append(SequenceDrawer);
            base.OnInitialize();
        }
        public void Open()
        {
            Visible = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            //SequenceDrawer.Top.Set(240, 0);
            //SequenceDrawer.Left.Set(0, 1.25f);

            if (SequenceDrawer.box != null)
            {
                SequenceDrawer.box.SequenceSize();
            }

            SetupConfigList();
        }
        public void Close()
        {
            Visible = false;
            Main.blockInput = false;
            SequenceDrawer.box = null;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
        public UIList UIList;
        public SequenceDrawer SequenceDrawer;
        public void SetupConfigList()
        {
            UIList.Clear();//清空
            foreach (SequenceBase sequence in SequenceSystem.sequenceBases.Values)
            {

                UIList.Add(SequenceToPanel(sequence));
            }
            Recalculate();
        }
        public UIPanel SequenceToPanel(SequenceBase sequence)
        {
            UIPanel panel = new UIPanel();
            panel.SetSize(200, 40);
            UIText uIText = new UIText(sequence.SequenceNameBase);
            SequenceBox box = new SequenceBox(sequence);
            box.SequenceSize();
            panel.Append(uIText);
            //panel.Append(box);
            if (SequenceDrawer.box != null && SequenceDrawer.box.sequenceBase.SequenceNameBase == sequence.SequenceNameBase) SequenceDrawer.box = box;
            panel.OnLeftDoubleClick += (evt, elem) => { SequenceDrawer.box = box; SoundEngine.PlaySound(SoundID.Unlock); };
            return panel;
        }
    }
    public class SequencePanel : UIElement
    {
        public SequenceBase sequenceBase;
        public UIPanel panel;
    }
    public class SequenceDrawer : UIElement
    {
        public SequenceBox box;
        public override void OnInitialize()
        {
            //Append(box);
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (box != null)
                DrawSequence(box, this.GetDimensions().Position(), default, true, true);
            base.DrawSelf(spriteBatch);
        }
        public void DrawWraper(WraperBox wraperBox, Vector2 position, float offset, bool active)
        {
            var pos = position;
            //position += SequenceConfig.Instance.Step * new Vector2(0, .5f);
            var spriteBatch = Main.spriteBatch;
            var desc = wraperBox.wraper.condition.Description.ToString();
            bool flag = desc != "Always";
            if (wraperBox.wraper.IsSequence)
            {
                ComplexPanelInfo panel = new ComplexPanelInfo();
                var boxSize = wraperBox.GetSize();
                //if (flag)
                //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f + new Vector2(0, 16), boxSize + new Vector2(32, 64));
                //else
                //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f, boxSize + new Vector2(32, 32));
                float offY = flag ? FontAssets.MouseText.Value.MeasureString(desc).Y : 0;
                panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X * .5f, 0), boxSize);
                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
                panel.glowEffectColor = Color.MediumPurple with { A = 0 };
                panel.glowShakingStrength = .1f;
                panel.glowHueOffsetRange = 0.1f;
                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
                panel.DrawComplexPanel(spriteBatch);
                DrawSequence(wraperBox.sequenceBox, position - offY * .5f * Vector2.UnitY, offY * .5f * Vector2.UnitY, active, false);
                if (flag)
                {
                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, position + boxSize * Vector2.UnitY * .5f - offY * 1.5f * Vector2.UnitY + new Vector2(16, 0), wraperBox.wraper.condition.IsMet() ? Color.MediumPurple : Color.Gray);
                }
            }
            else
            {
                //position.Y += wraperBox.GetSize().Y * .5f;
                var font = FontAssets.MouseText.Value;
                var name = wraperBox.wraper.Name;
                Vector2 textSize = font.MeasureString(name);
                Vector2 boxSize = wraperBox.GetSize();
                #region 框框
                ComplexPanelInfo panel = new ComplexPanelInfo();
                panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X, 0) * .5f, boxSize);
                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
                panel.glowEffectColor = Color.Cyan with { A = 0 };
                panel.glowShakingStrength = .05f;
                panel.glowHueOffsetRange = 0.05f;
                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
                panel.DrawComplexPanel(spriteBatch);
                #endregion
                spriteBatch.DrawString(FontAssets.MouseText.Value, name, position + new Vector2(16) - boxSize * Vector2.UnitY * .5f, active ? Color.Cyan : Color.Gray, 0, default, 1f, 0, 0);//|| wraperBox.wraper.Active
                if (flag)
                {
                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, position + new Vector2(16) + textSize.Y * Vector2.UnitY - boxSize * Vector2.UnitY * .5f, wraperBox.wraper.condition.IsMet() ? Color.MediumPurple : Color.Gray);
                }
                //spriteBatch.DrawRectangle(panel.destination, Color.MediumPurple);

            }
            if (SequenceConfig.Instance.ShowWrapBox)
                Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + wraperBox.GetSize() * Vector2.UnitX * .5f, wraperBox.GetSize()), Color.Purple, 8);

            //锚点
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.MediumPurple * .5f, 0, new Vector2(.5f), 16, 0, 0);


        }
        public void DrawGroup(GroupBox groupBox, Vector2 position, bool active)
        {
            var pos = position;
            var size = groupBox.GetSize();
            position.Y -= size.Y * .5f;
            var tarCen1 = pos + new Vector2(-SequenceConfig.Instance.Step.X * .25f, 0);
            var tarCen2 = pos + new Vector2(SequenceConfig.Instance.Step.X * .25f + size.X, 0);
            int c = 0;
            var font = FontAssets.MouseText.Value;
            foreach (var w in groupBox.wraperBoxes)
            {
                string desc = w.wraper.condition.Description.ToString();
                float offY = desc != "Always" ? font.MeasureString("→" + desc).Y : 0;
                Vector2 wsize = w.GetSize();
                position.Y += (wsize.Y + SequenceConfig.Instance.Step.Y - offY) * .5f;
                var offset = size.X - wsize.X;
                var scale = 1 + ((float)LogSpiralLibraryMod.ModTime / 180).CosFactor();
                //scale = 1;
                position.X = pos.X + offset * .5f;

                var tarCen3 = position;
                //var tarCen4 = w.wraper.IsSequence ? position + wsize * new Vector2(1, 0) + new Vector2(offset * 1.5f, 0) * .5f : position + new Vector2(offset * .5f + wsize.X, 0);
                var tarCen4 = tarCen3 + wsize.X * Vector2.UnitX;
                Main.spriteBatch.DrawHorizonBLine(tarCen3, tarCen1, Color.White, scale);
                Main.spriteBatch.DrawHorizonBLine(tarCen4, tarCen2, Color.White, scale);
                DrawWraper(w, position + new Vector2(0, offY * .5f), offset, active && groupBox.group.Index == c);//
                c++;
                position.Y += (wsize.Y + SequenceConfig.Instance.Step.Y + offY) * .5f;


            }
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.DarkCyan, 0, new Vector2(.5f), 16, 0, 0);
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.Cyan, 0, new Vector2(.5f), 12, 0, 0);
            if (SequenceConfig.Instance.ShowGroupBox)
                Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + groupBox.GetSize() * Vector2.UnitX * .5f, groupBox.GetSize()), Color.Cyan, 6);

            //锚点
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.Cyan * .5f, 0, new Vector2(.5f), 12, 0, 0);


        }
        public void DrawSequence(SequenceBox sequenceBox, Vector2 position, Vector2 offsetFrame, bool active, bool start)
        {
            var pos = position;
            position.X += 16;
            int counter = 0;
            Main.spriteBatch.DrawLine(pos, position, Color.White);
            position.X += SequenceConfig.Instance.Step.X * .25f;//16
            foreach (var g in sequenceBox.groupBoxes)
            {
                //绘制组之间的连接线
                if (counter < sequenceBox.groupBoxes.Count - 1)
                {
                    var p = position + (g.GetSize().X + SequenceConfig.Instance.Step.X * .25f) * Vector2.UnitX;// + 16
                    //if(LogSpiralLibraryMod.ModTime % 60 < 30)
                    Main.spriteBatch.DrawLine(p, p + (SequenceConfig.Instance.Step.X * .5f) * Vector2.UnitX, Color.White);//* 1f - 32
                }
                //绘制组，添加位置偏移
                DrawGroup(g, position, active && counter == sequenceBox.sequenceBase.Counter % sequenceBox.sequenceBase.GroupBases.Count);
                if (counter < sequenceBox.groupBoxes.Count - 1)
                    position.X += g.GetSize().X + SequenceConfig.Instance.Step.X;
                else
                    position.X += g.GetSize().X + SequenceConfig.Instance.Step.X;

                //position.X += g.GetSize().X + offset + SequenceConfig.Instance.Step.X;


                //计数器自增
                counter++;

            }
            Main.spriteBatch.DrawLine(pos + new Vector2(sequenceBox.GetSize().X + (start ? SequenceConfig.Instance.Step.X * .5f : 0), 0), position, Color.White);//32


            //锚点
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.Red * .5f, 0, new Vector2(.5f), 8, 0, 0);
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos + new Vector2(sequenceBox.GetSize().X + (start ? 32 : 0), 0), new Rectangle(0, 0, 1, 1), Color.Red * .5f, 0, new Vector2(.5f), 8, 0, 0);

            if (SequenceConfig.Instance.ShowSequenceBox)
                Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + sequenceBox.GetSize() * Vector2.UnitX * .5f + offsetFrame + (start ? new Vector2(12,0):default), sequenceBox.GetSize()), Color.Red);//以pos为左侧中心绘制矩形框框
        }
    }
    public class WraperBox : UIElement
    {
        public UIPanel panel;
        public SequenceBase.WraperBase wraper;
        public SequenceBox sequenceBox;
        public bool CacheRefresh;
        public WraperBox(SequenceBase.WraperBase wraperBase)
        {
            wraper = wraperBase;
            if (wraper.IsSequence)
                sequenceBox = new SequenceBox(wraper.SequenceInfo);

        }
        public override void OnInitialize()
        {
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {

            base.DrawSelf(spriteBatch);
        }
    }
    public class GroupBox : UIElement
    {
        public GroupBox(SequenceBase.GroupBase groupBase)
        {
            group = groupBase;
            foreach (var w in group.Wrapers)
            {
                var wbox = new WraperBox(w);
                wraperBoxes.Add(wbox);

            }
        }
        public UIPanel panel;
        public SequenceBase.GroupBase group;
        public List<WraperBox> wraperBoxes = new List<WraperBox>();
        public bool CacheRefresh;

        public override void OnInitialize()
        {
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
        }
    }
    public class SequenceBox : UIElement
    {
        public UIPanel panel;
        public SequenceBase sequenceBase;
        public List<GroupBox> groupBoxes = new List<GroupBox>();
        public bool CacheRefresh;
        public SequenceBox(SequenceBase sequence)
        {
            sequenceBase = sequence;
            foreach (var g in sequence.GroupBases)
            {
                var gbox = new GroupBox(g);

                groupBoxes.Add(gbox);

            }
        }
        public override void OnInitialize()
        {
            Vector2 size = this.SequenceSize();
            panel = new UIPanel();
            panel.SetSize(size);
            foreach (var g in groupBoxes)
                Append(g);
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {

            base.DrawSelf(spriteBatch);
        }
    }

    public static class SequenceDrawHelper
    {
        public static Vector2 WrapperSize(this WraperBox wrapBox)
        {
            Vector2 curr = wrapBox.GetSize();
            if (curr == default || wrapBox.CacheRefresh)
            {
                Vector2 delta;
                var wraper = wrapBox.wraper;
                var desc = wraper.condition.Description.Value;
                if (wraper.IsSequence)
                {
                    delta = SequenceSize(wrapBox.sequenceBox);
                    if (desc != "Always")
                    {
                        var textSize = FontAssets.MouseText.Value.MeasureString("→" + desc);
                        delta.Y += textSize.Y;
                        delta.X = Math.Max(textSize.X, delta.X);
                    }
                    delta += new Vector2(32, 32);
                    wrapBox.sequenceBox.SetSize(delta);
                    wrapBox.sequenceBox.Recalculate();
                }
                else
                {
                    var font = FontAssets.MouseText.Value;
                    var name = wraper.Name;
                    //if (name == "挥砍" && desc != "Always") 
                    //{
                    //    Main.NewText((font.MeasureString(name), font.MeasureString(desc)));
                    //}
                    Vector2 textSize = font.MeasureString(name);
                    Vector2 boxSize = textSize;
                    if (desc != "Always")
                    {
                        Vector2 descSize = font.MeasureString("→" + desc);
                        boxSize.Y += descSize.Y;
                        boxSize.X = Math.Max(textSize.X, descSize.X);
                    }
                    boxSize += Vector2.One * 32;
                    delta = boxSize;
                }
                wrapBox.SetSize(delta);
                wrapBox.Recalculate();

                return delta;
            }
            return curr;
        }
        public static Vector2 GroupSize(this GroupBox groupBox)
        {
            Vector2 curr = groupBox.GetSize();
            if (curr == default || groupBox.CacheRefresh)
            {
                Vector2 result = default;
                foreach (var wrapper in groupBox.wraperBoxes)
                {
                    if (!wrapper.wraper.Available) continue;
                    Vector2 delta = WrapperSize(wrapper);
                    result.Y += delta.Y + SequenceConfig.Instance.Step.Y;
                    if (delta.X > result.X) result.X = delta.X;
                }
                groupBox.SetSize(result);
                groupBox.Recalculate();
                return result;
            }
            return curr;
        }
        public static Vector2 SequenceSize(this SequenceBox sequencebox)
        {
            Vector2 curr = sequencebox.GetSize();
            if (curr == default || sequencebox.CacheRefresh)
            {
                Vector2 result = default;
                foreach (var group in sequencebox.groupBoxes)
                {
                    Vector2 delta = GroupSize(group);
                    result.X += delta.X + SequenceConfig.Instance.Step.X;
                    if (delta.Y > result.Y) result.Y = delta.Y;
                }

                sequencebox.SetSize(result);
                sequencebox.Recalculate();
                //if (Main.chatMonitor is RemadeChatMonitor remade)
                //{
                //    remade._showCount = 40;
                //}
                return result;
            }
            return curr;
        }
        //static void DrawWrapper(this SpriteBatch spriteBatch, MeleeSequence.MeleeSAWraper meleeSAWraper, Vector2 position)
        //{
        //    /*
        //    Vector2 size = MeleeWrapperSize(meleeSAWraper);
        //    if (meleeSAWraper.IsSequence)
        //    {

        //    }
        //    else 
        //    {
        //        var font = FontAssets.MouseText.Value;
        //        var name = wrapper.attackInfo.GetType().Name;
        //        Vector2 textSize = font.MeasureString(name);
        //        Vector2 boxSize = textSize;
        //        if (desc != "Always")
        //        {
        //            Vector2 descSize = font.MeasureString(desc);
        //            boxSize.Y += descSize.Y;
        //            boxSize.X = Math.Max(textSize.X, descSize.X);
        //        }
        //        boxSize += Vector2.One * 32;
        //        #region 框框
        //        ComplexPanelInfo panel = new ComplexPanelInfo();
        //        panel.destination = Utils.CenteredRectangle(positionHere + boxSize * .5f - new Vector2(16), boxSize);
        //        panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_0").Value;
        //        panel.glowEffectColor = Color.White with { A = 0 };
        //        panel.glowShakingStrength = 0;
        //        panel.glowHueOffsetRange = 0;
        //        panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
        //        panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
        //        panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
        //        panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
        //        panel.DrawComplexPanel(spriteBatch);
        //        #endregion
        //        spriteBatch.DrawString(FontAssets.MouseText.Value, name, positionHere, wrapper.attackInfo.timerMax > 0 ? Color.Cyan : Color.Gray, 0, default, 1f, 0, 0);
        //        if (desc != "Always")
        //        {
        //            spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, positionHere + textSize.Y * Vector2.UnitY, wrapper.condition.IsMet() ? Color.MediumPurple : Color.Gray);
        //        }
        //    }*/
        //}
        //public static void DrawMeleeSequence(this SpriteBatch spriteBatch, MeleeSequence meleeSequence, Vector2 position)
        //{
        //    int counterX = 0;
        //    foreach (var group in meleeSequence.MeleeGroups)
        //    {
        //        int counterY = 0;
        //        foreach (var wrapper in group.wrapers)
        //        {
        //            Vector2 positionHere = new Vector2(counterX, counterY) * new Vector2(128, 64) * 2 + position;
        //            var desc = wrapper.condition.Description.Value;

        //            if (wrapper.IsElement)
        //            {
        //                //if (wrapper.attackInfo.SkipCheck()) break;
        //                var font = FontAssets.MouseText.Value;
        //                var name = wrapper.attackInfo.GetType().Name;
        //                Vector2 textSize = font.MeasureString(name);
        //                Vector2 boxSize = textSize;
        //                if (desc != "Always")
        //                {
        //                    Vector2 descSize = font.MeasureString(desc);
        //                    boxSize.Y += descSize.Y;
        //                    boxSize.X = Math.Max(textSize.X, descSize.X);
        //                }
        //                boxSize += Vector2.One * 32;
        //                #region 框框
        //                ComplexPanelInfo panel = new ComplexPanelInfo();
        //                panel.destination = Utils.CenteredRectangle(positionHere + boxSize * .5f - new Vector2(16), boxSize);
        //                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_0").Value;
        //                panel.glowEffectColor = Color.White with { A = 0 };
        //                panel.glowShakingStrength = 0;
        //                panel.glowHueOffsetRange = 0;
        //                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
        //                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
        //                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
        //                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
        //                panel.DrawComplexPanel(spriteBatch);
        //                #endregion
        //                spriteBatch.DrawString(FontAssets.MouseText.Value, name, positionHere, wrapper.attackInfo.timerMax > 0 ? Color.Cyan : Color.Gray, 0, default, 1f, 0, 0);
        //                if (desc != "Always")
        //                {
        //                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, positionHere + textSize.Y * Vector2.UnitY, wrapper.condition.IsMet() ? Color.MediumPurple : Color.Gray);

        //                }

        //            }
        //            if (wrapper.IsSequence)
        //            {

        //                spriteBatch.DrawMeleeSequence(wrapper.sequenceInfo, positionHere/*, depth + 1, out Vector2 _finalSize*/);
        //                if (desc != "Always")
        //                {
        //                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, positionHere + new Vector2(0, 64), wrapper.condition.IsMet() ? Color.MediumPurple : Color.Gray);
        //                }
        //            }
        //            counterY++;
        //        }
        //        counterX++;
        //    }
        //}
    }

}

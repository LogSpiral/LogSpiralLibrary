using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using System.Collections;
using Terraria.ModLoader.UI;
using static Terraria.ModLoader.Config.UI.Vector2Element;

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
            UIPanel panel = UIPanel = new UIPanel();
            panel.SetSize(new Vector2(240, 300));
            panel.Top.Set(80, 0);
            panel.Left.Set(20, 0);
            Append(panel);

            UIPanel container = UIConfigSetterContainer = new UIPanel();
            container.SetSize(new Vector2(540, 400));
            container.Top.Set(400, 0);
            container.Left.Set(20, 0);
            //Append(container);
            ConfigElemList = new UIList();
            ConfigElemList.SetSize(540, 360);
            container.Append(ConfigElemList);

            UIList = new UIList();
            UIList.ListPadding = 24f;
            UIList.SetSize(200, 400);

            panel.Append(UIList);
            SequenceDrawer = new SequenceDrawer();
            SequenceDrawer.Top.Set(0, 0.25f);
            SequenceDrawer.Left.Set(0, 0.25f);
            Append(SequenceDrawer);
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
            UIConfigSetterContainer.Top.Set(400, 0);
            SetupConfigList();
        }
        public void Close()
        {
            Visible = false;
            Main.blockInput = false;
            SequenceDrawer.box = null;
            currentWraper = null;
            RemoveChild(UIConfigSetterContainer);
            RemoveChild(currentBox);
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
        public UIPanel UIPanel;
        public UIList UIList;
        public UIPanel UIConfigSetterContainer;
        public UIList ConfigElemList;
        public SequenceDrawer SequenceDrawer;
        public SequenceBox currentBox;
        public WraperBox currentWraper;


        public override void Update(GameTime gameTime)
        {
            //if (ConfigElemList != null)
            //    foreach (var u in from uiS in ConfigElemList._innerList.Children select uiS.Children)
            //        foreach(var v in u)
            //        Main.NewText(v.GetType());
            base.Update(gameTime);
        }
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

            panel.OnLeftClick += (evt, elem) =>
            {
                SequenceDrawer.box = box;
                SoundEngine.PlaySound(SoundID.Unlock);
            };
            panel.OnRightClick += (evt, elem) =>
            {
                if (currentBox != null)
                    this.RemoveChild(currentBox);
                currentBox = box;
                this.Append(box);
                box.startSequence = true;
                box.OnInitialize();
                box.Top.Set(0, 0.5f);
                box.Left.Set(0, 0.25f);
                box.Recalculate();

                SoundEngine.PlaySound(SoundID.Unlock);

            };
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
            {
                DrawSequence(box, this.GetDimensions().Position(), default, box.sequenceBase.Active, true);
                box.sequenceBase.Active = false;
            }
            base.DrawSelf(spriteBatch);
        }
        public static void DrawWraper(WraperBox wraperBox, Vector2 position, float offset, bool active)
        {
            var pos = position;
            //position += SequenceConfig.Instance.Step * new Vector2(0, .5f);
            var spriteBatch = Main.spriteBatch;
            var desc = wraperBox.wraper.condition.Description.ToString();
            bool flag = desc != "Always";
            if (wraperBox.wraper.IsSequence && wraperBox.sequenceBox.Expand)
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
                    var cen = position + boxSize * Vector2.UnitY * .5f - offY * 1.5f * Vector2.UnitY + new Vector2(16, 0);
                    if (wraperBox.wraper.condition.IsMet())
                    {
                        for (int n = 0; n < 3; n++)
                            spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f), new Color(0.8f, 0.2f, 1f, 0f), 0, default, 1f, 0, 0);
                        spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen, Color.White, 0, default, 1f, 0, 0);
                    }
                    else
                        spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen, Color.Gray);
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
                var cen = position + new Vector2(16) - boxSize * Vector2.UnitY * .5f;
                if (active)
                {
                    //var fontOff = font.MeasureString(name) * .5f;
                    //spriteBatch.DrawString(font, name, cen + fontOff, Color.Cyan, 0, fontOff, 1.1f, 0, 0);
                    //spriteBatch.DrawString(font, name, cen + fontOff, Color.Cyan, 0, fontOff, 0.9f, 0, 0);
                    for (int n = 0; n < 3; n++)
                        spriteBatch.DrawString(font, name, cen + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f), new Color(0.2f, 0.8f, 1f, 0f), 0, default, 1f, 0, 0);
                    spriteBatch.DrawString(font, name, cen, Color.White, 0, default, 1f, 0, 0);

                }
                else
                    spriteBatch.DrawString(font, name, cen, Color.Gray, 0, default, 1f, 0, 0);
                cen += textSize * Vector2.UnitY;
                if (flag)
                {
                    if (wraperBox.wraper.condition.IsMet())
                    {

                        for (int n = 0; n < 3; n++)
                            spriteBatch.DrawString(font, "→" + desc, cen + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f), new Color(0.8f, 0.2f, 1f, 0f), 0, default, 1f, 0, 0);
                        spriteBatch.DrawString(font, "→" + desc, cen, Color.White, 0, default, 1f, 0, 0);

                    }
                    else
                        spriteBatch.DrawString(font, "→" + desc, cen, Color.Gray);
                }
                //spriteBatch.DrawRectangle(panel.destination, Color.MediumPurple);

            }
            if (SequenceConfig.Instance.ShowWrapBox)
                Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + wraperBox.GetSize() * Vector2.UnitX * .5f, wraperBox.GetSize()), Color.MediumPurple * .5f, 8);

            //锚点
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.MediumPurple * .5f, 0, new Vector2(.5f), 16, 0, 0);


        }
        public static void DrawGroup(GroupBox groupBox, Vector2 position, bool active)
        {
            Color GroupColor = Color.Cyan;
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
                Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + groupBox.GetSize() * Vector2.UnitX * .5f, groupBox.GetSize()), GroupColor * .5f, 6);

            //锚点
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), Color.Cyan * .5f, 0, new Vector2(.5f), 12, 0, 0);


        }
        public static void DrawSequence(SequenceBox sequenceBox, Vector2 position, Vector2 offsetFrame, bool active, bool start)
        {
            Color SequenceColor = Color.Red;
            var pos = position;
            if (!start)
                position.X += 16;
            int counter = 0;
            position.X += SequenceConfig.Instance.Step.X * .5f;//16
            Main.spriteBatch.DrawLine(pos, position - SequenceConfig.Instance.Step.X * .25f * Vector2.UnitX, Color.White);
            foreach (var g in sequenceBox.groupBoxes)
            {
                //绘制组之间的连接线
                if (counter < sequenceBox.groupBoxes.Count - 1)
                {
                    var p = position + (g.GetSize().X + SequenceConfig.Instance.Step.X * .25f) * Vector2.UnitX;// + 16
                                                                                                               //if (LogSpiralLibraryMod.ModTime % 60 < 30)
                    Main.spriteBatch.DrawLine(p, p + (SequenceConfig.Instance.Step.X * .5f) * Vector2.UnitX, Color.White);// 1f - 32
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
            Main.spriteBatch.DrawLine(pos + new Vector2(sequenceBox.GetSize().X, 0), position + new Vector2(-SequenceConfig.Instance.Step.X * .75f, 0), Color.White);//32


            //锚点
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), SequenceColor * .5f, 0, new Vector2(.5f), 8, 0, 0);
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos + new Vector2(sequenceBox.GetSize().X + (start ? 32 : 0), 0), new Rectangle(0, 0, 1, 1), Color.Red * .5f, 0, new Vector2(.5f), 8, 0, 0);

            if (SequenceConfig.Instance.ShowSequenceBox)
                Main.spriteBatch.DrawRectangle(Utils.CenteredRectangle(pos + sequenceBox.GetSize() * Vector2.UnitX * .5f + offsetFrame, sequenceBox.GetSize()), SequenceColor * .5f);//以pos为左侧中心绘制矩形框框
        }
    }
    public abstract class DragableBox : UIElement
    {
        public bool IsClone;
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            Main.NewText("按下左键");
            base.LeftMouseDown(evt);
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            Main.NewText("松开左键");
            base.LeftMouseUp(evt);
        }

    }
    public class ActionElementDragableBox : UIElement
    {

    }
    public class SequenceElementDragableBox : UIElement
    {

    }
    public class WraperBox : UIElement
    {
        public UIPanel panel;
        public SequenceBase.WraperBase wraper;
        public SequenceBox sequenceBox;
        public bool CacheRefresh;
        public bool chosen;
        public WraperBox(SequenceBase.WraperBase wraperBase)
        {
            wraper = wraperBase;
            if (wraper.IsSequence)
                sequenceBox = new SequenceBox(wraper.SequenceInfo);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">外层</param>
        /// <param name="top">距离顶部的高度？</param>
        /// <param name="memberInfo">成员信息</param>
        /// <param name="item"></param>
        /// <param name="order">下标？</param>
        /// <param name="list">元素所属的链表？</param>
        /// <param name="arrayType">链表中值类型</param>
        /// <param name="index">链表元素的下标？</param>
        /// <returns></returns>
        public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1)
        {
            int elementHeight;
            Type type = memberInfo.Type;

            if (arrayType != null)
            {
                type = arrayType;
            }

            UIElement e;

            // TODO: Other common structs? -- Rectangle, Point
            var customUI = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomModConfigItemAttribute>(memberInfo, null, null);//是否对该成员有另外实现的UI支持

            if (customUI != null)
            {
                Type customUIType = customUI.Type;

                if (typeof(ConfigElement).IsAssignableFrom(customUIType))
                {
                    ConstructorInfo ctor = customUIType.GetConstructor(Array.Empty<Type>());

                    if (ctor != null)
                    {
                        object instance = ctor.Invoke(new object[0]);//执行相应UI的构造函数
                        e = instance as UIElement;
                    }
                    else
                    {
                        e = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not have an empty constructor.");
                    }
                }
                else
                {
                    e = new UIText($"{customUIType.Name} specified via CustomModConfigItem for {memberInfo.Name} does not inherit from ConfigElement.");
                }
            }
            else if (item.GetType() == typeof(HeaderAttribute))
            {
                e = new HeaderElement((string)memberInfo.GetValue(item));
            }
            //基于成员类型添加上默认的编辑UI
            else if (type == typeof(ItemDefinition))
            {
                e = new ItemDefinitionElement();
            }
            else if (type == typeof(ProjectileDefinition))
            {
                e = new ProjectileDefinitionElement();
            }
            else if (type == typeof(NPCDefinition))
            {
                e = new NPCDefinitionElement();
            }
            else if (type == typeof(PrefixDefinition))
            {
                e = new PrefixDefinitionElement();
            }
            else if (type == typeof(BuffDefinition))
            {
                e = new BuffDefinitionElement();
            }
            else if (type == typeof(TileDefinition))
            {
                e = new TileDefinitionElement();
            }
            else if (type == typeof(Color))
            {
                e = new ColorElement();
            }
            else if (type == typeof(Vector2))
            {
                e = new Vector2Element();
            }
            else if (type == typeof(bool)) // isassignedfrom?
            {
                e = new BooleanElement();
            }
            else if (type == typeof(float))
            {
                e = new FloatElement();
            }
            else if (type == typeof(byte))
            {
                e = new ByteElement();
            }
            else if (type == typeof(uint))
            {
                e = new UIntElement();
            }
            else if (type == typeof(int))
            {
                SliderAttribute sliderAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SliderAttribute>(memberInfo, item, list);

                if (sliderAttribute != null)
                    e = new IntRangeElement();
                else
                    e = new IntInputElement();

                Main.NewText("我是整型!!");
            }
            else if (type == typeof(string))
            {
                OptionStringsAttribute ost = ConfigManager.GetCustomAttributeFromMemberThenMemberType<OptionStringsAttribute>(memberInfo, item, list);
                if (ost != null)
                    e = new StringOptionElement();
                else
                    e = new StringInputElement();
            }
            else if (type.IsEnum)
            {
                if (list != null)
                    e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}).");
                else
                    e = new EnumElement();
            }
            else if (type.IsArray)
            {
                e = new ArrayElement();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                e = new ListElement();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                e = new SetElement();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                e = new DictionaryElement();
            }
            else if (type.IsClass)
            {
                e = new ObjectElement(/*, ignoreSeparatePage: ignoreSeparatePage*/);
            }
            else if (type.IsValueType && !type.IsPrimitive)
            {
                e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}) Structs need special UI.");
                //e.Top.Pixels += 6;
                e.Height.Pixels += 6;
                e.Left.Pixels += 4;

                //object subitem = memberInfo.GetValue(item);
            }
            else
            {
                e = new UIText($"{memberInfo.Name} not handled yet ({type.Name})");
                e.Top.Pixels += 6;
                e.Left.Pixels += 4;
            }

            if (e != null)
            {
                if (e is ConfigElement configElement)
                {
                    configElement.Bind(memberInfo, item, (IList)list, index);//将UI控件与成员信息及实例绑定
                    configElement.OnBind();
                }

                e.Recalculate();

                elementHeight = (int)e.GetOuterDimensions().Height;

                var container = UIModConfig.GetContainer(e, index == -1 ? order : index);
                container.Height.Pixels = elementHeight;

                if (parent is UIList uiList)
                {
                    uiList.Add(container);
                    uiList.GetTotalHeight();
                }
                else
                {
                    // Only Vector2 and Color use this I think, but modders can use the non-UIList approach for custom UI and layout.
                    container.Top.Pixels = top;
                    container.Width.Pixels = -20;
                    container.Left.Pixels = 20;
                    top += elementHeight + 4;
                    parent.Append(container);
                    parent.Height.Set(top, 0);
                }

                var tuple = new Tuple<UIElement, UIElement>(container, e);

                if (parent == Interface.modConfig.mainConfigList)
                {
                    Interface.modConfig.mainConfigItems.Add(tuple);
                }

                return tuple;
            }
            else
            {
                Main.NewText("怎么null乐");
            }
            return null;
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            if (evt.Target != this && !(this.BelongToMe(evt.Target) && evt.Target is not WraperBox)) return;
            //Main.NewText((GetHashCode(), evt.Target.GetHashCode()));
            if (wraper.IsSequence)
            {
                //sequenceBox.Expand = !sequenceBox.Expand;
                Main.NewText("芝士序列");
            }
            else
            {
                Main.NewText("芝士元素");

                var ui = SequenceSystem.instance.sequenceUI;
                ui.Append(ui.UIConfigSetterContainer);
                if (ui.currentWraper != null)
                    ui.currentWraper.chosen = false;
                ui.currentWraper = this;
                chosen = true;
                var list = ui.ConfigElemList;
                list.Clear();
                //wraper.SetConfigPanel(list);
                SoundEngine.PlaySound(SoundID.MenuOpen);


                int top = 0;
                int order = 0;

                foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(wraper.Element))
                {
                    if (!Attribute.IsDefined(variable.MemberInfo, typeof(ElementCustomDataAttribute)) || Attribute.IsDefined(variable.MemberInfo, typeof(ElementCustomDataAbabdonedAttribute)))
                        continue;
                    var (container, elem) = UIModConfig.WrapIt(list, ref top, variable, wraper.Element, order++);
                    //elem.OnLeftClick += (_evt, uielem) => { };
                }
            }
            base.LeftClick(evt);
        }
        public override void LeftDoubleClick(UIMouseEvent evt)
        {
            if (evt.Target == this || (this.BelongToMe(evt.Target) && evt.Target is not WraperBox))
                Main.NewText(wraper.Name);
            base.LeftDoubleClick(evt);
        }

        public override void OnInitialize()
        {
            Vector2 size = this.GetSize();
            panel = new UIPanel();
            panel.SetSize(size);
            if (wraper.IsSequence)
            {
                var desc = wraper.condition.Description.ToString();
                if (desc != "Always")
                {
                    sequenceBox.offY = FontAssets.MouseText.Value.MeasureString("→" + desc).Y * -.5f;
                }
                //sequenceBox.Left.Set(SequenceConfig.Instance.Step.X * .5f, 0);
                sequenceBox.OnInitialize();
                Append(sequenceBox);
            }
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            //spriteBatch.DrawRectangle(this.GetDimensions().ToRectangle(), Color.MediumPurple, 12);
            var desc = wraper.condition.Description.ToString();
            bool flag = desc != "Always";
            var wraperBox = this;
            var position = this.GetDimensions().Position() + new Vector2(0, this.GetDimensions().Height * .5f);
            if (wraperBox.wraper.IsSequence && wraperBox.sequenceBox.Expand)
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
                //DrawSequence(wraperBox.sequenceBox, position - offY * .5f * Vector2.UnitY, offY * .5f * Vector2.UnitY, active, false);
                if (flag)
                {
                    var cen = position + boxSize * Vector2.UnitY * .5f - offY * 1.5f * Vector2.UnitY + new Vector2(16, 0);
                    spriteBatch.DrawString(FontAssets.MouseText.Value, "→" + desc, cen, Color.White);
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
                panel.glowEffectColor = (chosen ? Color.Red : Color.Cyan) with { A = 0 };
                panel.glowShakingStrength = .05f;
                panel.glowHueOffsetRange = 0.05f;
                panel.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/HotbarRadial_1").Value;
                panel.backgroundFrame = new Rectangle(4, 4, 28, 28);
                panel.backgroundUnitSize = new Vector2(28, 28) * 2f;
                panel.backgroundColor = Color.Lerp(Color.Purple, Color.Pink, MathF.Sin(Main.GlobalTimeWrappedHourly) * .5f + .5f) * .5f;
                panel.DrawComplexPanel(spriteBatch);
                #endregion
                var cen = position + new Vector2(16) - boxSize * Vector2.UnitY * .5f;
                spriteBatch.DrawString(font, name, cen, Color.White, 0, default, 1f, 0, 0);
                cen += textSize * Vector2.UnitY;
                if (flag)
                {
                    spriteBatch.DrawString(font, "→" + desc, cen, Color.White);
                }
                //spriteBatch.DrawRectangle(panel.destination, Color.MediumPurple);
                if (chosen)
                {
                    var tarVec = SequenceSystem.instance.sequenceUI.UIConfigSetterContainer.GetDimensions().ToRectangle().BottomRight();
                    spriteBatch.DrawHorizonBLine(tarVec, position, Color.White);
                    spriteBatch.DrawHorizonBLine(tarVec + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4), position + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4), Main.DiscoColor with { A = 0 }, 1, 6);

                }
            }

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
            //this.IgnoresMouseInteraction = true;
            Vector2 size = this.GetSize();
            panel = new UIPanel();
            panel.SetSize(size);
            float offset = SequenceConfig.Instance.Step.Y * .5f;
            foreach (var w in wraperBoxes)
            {
                w.Top.Set(offset, 0f);
                Append(w);
                var dimension = w.GetDimensions();
                offset += dimension.Height + SequenceConfig.Instance.Step.Y;
                w.Left.Set((size.X - dimension.Width) * .5f, 0f);
                w.OnInitialize();
            }
            base.OnInitialize();
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            //spriteBatch.DrawRectangle(this.GetDimensions().ToRectangle(), Color.Cyan * .75f, 8);
            var dimension = GetDimensions();
            var scale = 1 + ((float)LogSpiralLibraryMod.ModTime / 180).CosFactor();

            foreach (var w in wraperBoxes)
            {
                var wD = w.GetDimensions();
                var offY = w.wraper.condition.Description.Value != "Always" ? -FontAssets.MouseText.Value.MeasureString("→" + w.wraper.condition.Description).Y * .5f : 0;
                spriteBatch.DrawHorizonBLine(dimension.Position() + new Vector2(-SequenceConfig.Instance.Step.X * .25f, dimension.Height * .5f), wD.Position() + new Vector2(0, wD.Height * .5f + offY), Color.White, scale);
                spriteBatch.DrawHorizonBLine(dimension.Position() + new Vector2(SequenceConfig.Instance.Step.X * .25f + dimension.Width, dimension.Height * .5f), wD.Position() + new Vector2(wD.Width, wD.Height * .5f + offY), Color.White, scale);

                //spriteBatch.Draw(TextureAssets.MagicPixel.Value, wD.Position() + new Vector2(0, wD.Height * .5f), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(0.5f), 16f, 0, 0);
            }
            base.DrawSelf(spriteBatch);
        }
    }
    public class SequenceBox : UIElement
    {
        public bool Expand = true;
        public UIPanel panel;
        public SequenceBase sequenceBase;
        public List<GroupBox> groupBoxes = new List<GroupBox>();
        public bool CacheRefresh;
        public float offY;
        public bool startSequence;
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
            //this.IgnoresMouseInteraction = true;
            Vector2 size = this.SequenceSize();
            panel = new UIPanel();
            panel.SetSize(size);
            float offset = SequenceConfig.Instance.Step.X * .5f;
            if (!startSequence)
            {
                offset += 16;
            }
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
            Vector2 startP = pos + new Vector2(SequenceConfig.Instance.Step.X * .25f + (startSequence ? 0 : 16), 0);
            spriteBatch.DrawLine(pos, startP, Color.White);
            int counter = 0;
            foreach (var g in groupBoxes)
            {
                counter++;
                startP += new Vector2(SequenceConfig.Instance.Step.X * .5f + g.GetDimensions().Width, 0);
                Vector2 endP = startP + new Vector2(SequenceConfig.Instance.Step.X * (counter == groupBoxes.Count ? 0.25f : 0.5f), 0);
                spriteBatch.DrawLine(startP, endP, Color.White);
                startP = endP;
            }
            //spriteBatch.DrawRectangle(dimension.ToRectangle(), Color.Red * .5f);
            base.DrawSelf(spriteBatch);
        }
    }
    public class ActionModifyDataElement : ConfigElement
    {
        class DataObject
        {
            private readonly PropertyFieldWrapper memberInfo;
            private readonly object item;
            private readonly IList<ActionModifyData> array;
            private readonly int index;

            private ActionModifyData current;

            //[LabelKey("缩放系数")]
            public float actionOffsetSize
            {
                get => current.actionOffsetSize;
                set
                {
                    current.actionOffsetSize = value;
                    Update();
                }
            }

            //[LabelKey("时长系数")]
            public float actionOffsetTimeScaler
            {
                get => current.actionOffsetTimeScaler;
                set
                {
                    current.actionOffsetTimeScaler = value;
                    Update();
                }
            }

            //[LabelKey("击退系数")]
            public float actionOffsetKnockBack
            {
                get => current.actionOffsetKnockBack;
                set
                {
                    current.actionOffsetKnockBack = value;
                    Update();
                }
            }
            //[LabelKey("伤害系数")]
            public float actionOffsetDamage
            {
                get => current.actionOffsetDamage;
                set
                {
                    current.actionOffsetDamage = value;
                    Update();
                }
            }
            //[LabelKey("暴击率增益")]
            public int actionOffsetCritAdder
            {
                get => current.actionOffsetCritAdder;
                set
                {
                    current.actionOffsetCritAdder = value;
                    Update();
                }
            }
            //[LabelKey("暴击率系数")]
            public float actionOffsetCritMultiplyer
            {
                get => current.actionOffsetCritMultiplyer;
                set
                {
                    current.actionOffsetCritMultiplyer = value;
                    Update();
                }
            }

            private void Update()
            {
                if (array == null)
                    memberInfo.SetValue(item, current);
                else
                    array[index] = current;

                Interface.modConfig.SetPendingChanges();
            }
            public DataObject(PropertyFieldWrapper memberInfo, object item)
            {
                this.item = item;
                this.memberInfo = memberInfo;
                current = (ActionModifyData)memberInfo.GetValue(item);
            }

            public DataObject(IList<ActionModifyData> array, int index)
            {
                current = array[index];
                this.array = array;
                this.index = index;
            }
        }
        private int height;
        private DataObject c;
        private float min = 0;
        private float max = 10;
        private float increment = 0.01f;

        public IList<ActionModifyData> DataList { get; set; }

        public override void OnBind()
        {
            base.OnBind();

            DataList = (IList<ActionModifyData>)List;

            if (DataList != null)
            {
                DrawLabel = false;
                height = 30;
                c = new DataObject(DataList, Index);
            }
            else
            {
                height = 30;
                c = new DataObject(MemberInfo, Item);
            }

            if (RangeAttribute != null && RangeAttribute.Min is float && RangeAttribute.Max is float)
            {
                max = (float)RangeAttribute.Max;
                min = (float)RangeAttribute.Min;
            }

            if (IncrementAttribute != null && IncrementAttribute.Increment is float)
            {
                increment = (float)IncrementAttribute.Increment;
            }

            int order = 0;
            foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(c))
            {
                var wrapped = UIModConfig.WrapIt(this, ref height, variable, c, order++);

                // Can X and Y inherit range and increment automatically? Pass in "fake PropertyFieldWrapper" to achieve? Real one desired for label.
                if (wrapped.Item2 is FloatElement floatElement)
                {
                    floatElement.Min = min;
                    floatElement.Max = max;
                    floatElement.Increment = increment;
                    floatElement.DrawTicks = Attribute.IsDefined(MemberInfo.MemberInfo, typeof(DrawTicksAttribute));
                }

                if (DataList != null)
                {
                    wrapped.Item1.Left.Pixels -= 20;
                    wrapped.Item1.Width.Pixels += 20;
                }
            }
        }

        // Draw axis? ticks?
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //CalculatedStyle dimensions = base.GetInnerDimensions();
            //Rectangle rectangle = dimensions.ToRectangle();
            //rectangle = new Rectangle(rectangle.Right - 30, rectangle.Y, 30, 30);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.AliceBlue);

            //float x = (c.X - min) / (max - min);
            //float y = (c.Y - min) / (max - min);

            //var position = rectangle.TopLeft();
            //position.X += x * rectangle.Width;
            //position.Y += y * rectangle.Height;
            //var blipRectangle = new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4);

            //if (x >= 0 && x <= 1 && y >= 0 && y <= 1)
            //    spriteBatch.Draw(TextureAssets.MagicPixel.Value, blipRectangle, Color.Black);

            //if (IsMouseHovering && rectangle.Contains((Main.MouseScreen).ToPoint()) && Main.mouseLeft)
            //{
            //    float newPerc = (Main.mouseX - rectangle.X) / (float)rectangle.Width;
            //    newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
            //    c.X = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;

            //    newPerc = (Main.mouseY - rectangle.Y) / (float)rectangle.Height;
            //    newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
            //    c.Y = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;
            //}
        }

        internal float GetHeight()
        {
            return height;
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
        //    }
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
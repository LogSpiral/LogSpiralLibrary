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
using LogSpiralLibrary.CodeLibrary.UIElements;
using System.Xml;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework.Input;
using Terraria.UI.Chat;
using Terraria;
using rail;

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
            //SequenceSystem.instance?.sequenceUI?.SetupConfigList();
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
    public static class SequenceCollectionManager<T> where T : ISequenceElement
    {
        public static Dictionary<string, SequenceBase<T>> sequences = new Dictionary<string, SequenceBase<T>>();
        public static void Load()
        {
            //foreach (var type in AvailableElementBaseTypes)
            //{
            //    var list = GetAllFilesByDir($"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{type.Name}");
            //    foreach (FileInfo file in list)
            //    {
            //        var seqType = typeof(SequenceBase<>);
            //        seqType = seqType.MakeGenericType(type);
            //        var loadMethod = seqType.GetMethod("Load", BindingFlags.Public | BindingFlags.Static, [typeof(string)]);
            //        sequenceBases.Add(file.Name, (SequenceBase)loadMethod.Invoke(null, [file.FullName]));
            //    }
            //}
            var path = $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence/{typeof(T).Name}";
            var list = SequenceSystem.GetAllFilesByDir(path);
            foreach (FileInfo file in list)
            {
                var modName = file.FullName.Split('\\', '/')[^2];
                bool flag = ModLoader.TryGetMod(modName, out _);
                if (flag)
                {
                    string key = $"{modName}/{Path.GetFileNameWithoutExtension(file.Name)}";
                    SequenceBase<T> instance = SequenceBase<T>.Load(file.FullName);
                    sequences[key] = instance;
                }
            }
        }
    }
    public class SequenceSystem : ModSystem
    {
        public static void SetSequenceUIPending(bool flag = true)
        {
            instance.sequenceUI.PendingModify = flag;
        }
        //TODO 可以给其它类型的序列用
        public static Dictionary<Type, Dictionary<string, SequenceBase>> sequenceBases = new();
        public static Dictionary<string, SequenceBasicInfo> sequenceInfos = new Dictionary<string, SequenceBasicInfo>();
        public SequenceUI sequenceUI;
        public UserInterface userInterfaceSequence;
        public static ModKeybind ShowSequenceKeybind { get; private set; }
        public static SequenceSystem instance;
        public static Dictionary<string, Condition> conditions = new Dictionary<string, Condition>();
        public static List<Type> AvailableElementBaseTypes = new List<Type>();
        public override void Load()
        {
            instance = this;
            sequenceUI = new SequenceUI();
            userInterfaceSequence = new UserInterface();
            sequenceUI.Activate();
            userInterfaceSequence.SetState(sequenceUI);
            ShowSequenceKeybind = KeybindLoader.RegisterKeybind(Mod, "ShowSequenceModifier", "Y");
            //On_UIElement.DrawSelf += On_UIElement_DrawSelf;
            #region conditions的赋值
            var noneCondition = new Condition(Language.GetOrRegister("Mods.LogSpiralLibrary.Condition.None"), () => true);
            conditions.Add("None", noneCondition);
            var alwaysCondition = new Condition(Language.GetOrRegister("Mods.LogSpiralLibrary.Condition.Always"), () => true);
            conditions.Add("Always", alwaysCondition);
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
            var fieldInfos = typeof(Condition).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var fieldInfo in fieldInfos)
            {
                Condition condition = (Condition)fieldInfo.GetValue(null);
                string key = condition.Description.Key.Split('.')[^1];
                if (!conditions.ContainsKey(key))
                    conditions.Add(key, condition);
            }//录入原版条件

            #endregion



        }
        public override void PostSetupContent()
        {
            AvailableElementBaseTypes.Add(typeof(MeleeAction));
            foreach (var type in AvailableElementBaseTypes)
                LoadSequenceWithType(type);
            base.PostSetupContent();
        }
        private void On_UIElement_DrawSelf(On_UIElement.orig_DrawSelf orig, UIElement self, SpriteBatch spriteBatch)
        {
            orig(self, spriteBatch);
            spriteBatch.DrawString(FontAssets.MouseText.Value, self.GetHashCode().ToString(), self.GetDimensions().Position(), Color.White);
        }
        /// <summary>
        /// 获得指定目录下的所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesByDir(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            //找到该目录下的文件
            FileInfo[] fi = di.GetFiles();

            //把FileInfo[]数组转换为List
            List<FileInfo> list = fi.ToList<FileInfo>();
            return list;
        }
        /// <summary>
        /// 获得指定目录及其子目录的所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<FileInfo> GetAllFilesByDir(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            //找到该目录下的文件
            FileInfo[] fi = dir.GetFiles();

            //把FileInfo[]数组转换为List
            List<FileInfo> list = fi.ToList();

            //找到该目录下的所有目录里的文件
            DirectoryInfo[] subDir = dir.GetDirectories();
            foreach (DirectoryInfo d in subDir)
            {
                List<FileInfo> subList = GetFilesByDir(d.FullName);
                foreach (FileInfo subFile in subList)
                {
                    list.Add(subFile);
                }
            }
            return list;
        }
        public static void LoadSequences<T>() where T : ISequenceElement
        {
            SequenceCollectionManager<MeleeAction>.Load();
            var seq = SequenceCollectionManager<MeleeAction>.sequences;
            sequenceBases[typeof(T)] = (from s in seq select new KeyValuePair<string, SequenceBase>(s.Key, s.Value)).ToDictionary();
        }
        public static void LoadSequenceWithType(Type type) => typeof(SequenceSystem).GetMethod("LoadSequences", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type).Invoke(null, []);

        public override void Unload()
        {
            AvailableElementBaseTypes?.Clear();
            instance = null;
            base.Unload();
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (SequenceUI.Visible)
            {
                userInterfaceSequence?.Update(gameTime);
            }
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
        public UIList actionLib;
        public UIList sequenceLib;
        public UIListByRow pageList;
        public UIPanel WorkingPlacePanel;
        public UIPanel OuterWorkingPanel;
        public WraperBox currentWraper;
        public UIList propList;
        public UIList infoList;
        public UIButton<string> saveButton;
        public UIButton<string> saveAsButton;
        public UIButton<string> revertButton;
        public UIPanel BasicInfoPanel;
        public Type CurrentSelectedType;
        public Dictionary<string, SequenceBase> currentSequences => SequenceSystem.sequenceBases[CurrentSelectedType];
        public bool Draggable;
        public bool Dragging;
        bool pendingModify;
        public bool PendingModify
        {
            get => pendingModify;
            set
            {
                pendingModifyChange = value | pendingModify;
                pendingModify = value;
            }
        }
        bool pendingModifyChange;

        public Vector2 Offset;
        public void ReloadLib()
        {
            actionLib.Clear();
            sequenceLib.Clear();
            Type elemBaseType = CurrentSelectedType;
            Type type = typeof(ModTypeLookup<>);
            type = type.MakeGenericType(elemBaseType);
            //Main.NewText(type.GetField("dict",BindingFlags.Static|BindingFlags.NonPublic) == null);
            IDictionary dict = (IDictionary)type.GetField("dict", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            type = typeof(SequenceBase<>.Wraper);
            type = type.MakeGenericType(elemBaseType);
            foreach (var v in dict.Values)
            {
                var e = (ISequenceElement)Activator.CreateInstance(v.GetType());
                WraperBox wraperBox = new WraperBox((SequenceBase.WraperBase)Activator.CreateInstance(type, [e]));
                wraperBox.WrapperSize();
                //wraperBox.HAlign = 0.5f;
                wraperBox.IsClone = true;
                actionLib.Add(wraperBox);

            }
            foreach (var s in currentSequences.Values)
            {
                WraperBox wraperBox = new WraperBox((SequenceBase.WraperBase)Activator.CreateInstance(type, [s]));
                wraperBox.sequenceBox.Expand = false;
                wraperBox.WrapperSize();
                wraperBox.IsClone = true;
                sequenceLib.Add(wraperBox);
            }
        }
        public void ResetPage()
        {
            pageList.Clear();
            UIButton<string> defPage = new UIButton<string>("默认页面");
            defPage.SetSize(new Vector2(128, 0), 0, 1);
            pageList.Add(defPage);
            defPage.OnLeftClick += (e, evt) => { SwitchToDefaultPage(); };
            //UIText defText = new UIText();
            //defText.IgnoresMouseInteraction = true;
            //defText.SetSize(default, 1, 1);
            //defPage.Append(defText);
            UIButton<string> newPage = new UIButton<string>("+");
            newPage.SetSize(new Vector2(32, 0), 0, 1);
            var seq = new MeleeSequence();
            seq.Add(new SwooshInfo());
            newPage.OnLeftClick += (e, evt) => { OpenBasicSetter(new SequenceBasicInfo() { createDate = DateTime.Now, lastModifyDate = DateTime.Now }, seq); };
            pageList.Add(newPage);
            //UIText newText = new UIText("+");
            //newText.IgnoresMouseInteraction = true;
            //newText.SetSize(default, 1, 1);
            //newPage.Append(newText);
            SwitchToDefaultPage();
        }
        public void SwitchToDefaultPage()
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Draggable = false;
            Offset = default;
            WorkingPlacePanel.Elements.Clear();
            if (currentWraper != null)
                currentWraper.chosen = false;
            currentWraper = null;
            propList.Clear();
            infoList.Clear();
            UIText info = new UIText(
                "你正处于默认页面，可以在这里选择一个序列以开始编辑。\n" +
                "成品序列指适于直接给武器使用的完成序列\n" +
                "库存序列指适于用来辅助组成成品序列可以反复调用的序列\n" +
                "二者没有硬性区别，根据自己感觉进行标记即可。");
            info.IsWrapped = true;
            info.SetSize(new Vector2(-40, 1), 1, 1);
            infoList.Add(info);
            UIPanel finishedSequencePanel = new UIPanel();
            finishedSequencePanel.SetSize(default, 0.5f, 1f);
            UIPanel libSequencePanel = new UIPanel();
            libSequencePanel.SetSize(default, 0.5f, 1f);
            libSequencePanel.Left.Set(0, 0.5f);
            WorkingPlacePanel.Append(libSequencePanel);
            WorkingPlacePanel.Append(finishedSequencePanel);
            UIText fTitle = new UIText("成品序列");
            fTitle.SetSize(new Vector2(0, 20), 1, 0);
            UIText lTitle = new UIText("库存序列");
            lTitle.SetSize(new Vector2(0, 20), 1, 0);
            finishedSequencePanel.Append(fTitle);
            libSequencePanel.Append(lTitle);
            UIList fList = new UIList();
            fList.SetSize(0, -20, 1, 1);
            fList.Top.Set(20, 0);
            UIScrollbar fScrollbar = new UIScrollbar();
            fScrollbar.Height.Set(0f, 1f);
            fScrollbar.HAlign = 1f;
            fScrollbar.SetView(100, 1000);
            fList.SetScrollbar(fScrollbar);
            finishedSequencePanel.Append(fScrollbar);
            UIList lList = new UIList();
            lList.SetSize(0, -20, 1, 1);
            lList.Top.Set(20, 0);
            UIScrollbar lScrollbar = new UIScrollbar();
            lScrollbar.Height.Set(0f, 1f);
            lScrollbar.HAlign = 1f;
            lScrollbar.SetView(100, 1000);
            lList.SetScrollbar(lScrollbar);
            libSequencePanel.Append(lScrollbar);
            finishedSequencePanel.Append(fList);
            libSequencePanel.Append(lList);
            foreach (var s in currentSequences.Values)
            {
                (SequenceSystem.sequenceInfos[s.KeyName].Finished ? fList : lList).Add(SequenceToButton(s.Clone()));

            }
        }
        public void SwitchToSequencePage(SequenceBox box)
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Draggable = true;
            Offset = default;
            WorkingPlacePanel.Elements.Clear();
            //WorkingPlacePanel.BackgroundColor = Color.White with { A = 0} * .25f;
            if (currentWraper != null)
                currentWraper.chosen = false;
            currentWraper = null;
            propList.Clear();
            infoList.Clear();
            int top = 0;
            int order = 0;
            SequenceBasicInfo info = SequenceSystem.sequenceInfos[box.sequenceBase.KeyName].Clone();
            foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(info))
            {
                if (variable.Name == "passWord" || Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                    continue;
                var (container, elem) = UIModConfig.WrapIt(infoList, ref top, variable, info, order++);
            }
            WorkingPlacePanel.OverflowHidden = true;
            box.SequenceSize(true);
            box.OnInitialize();
            WorkingPlacePanel.Append(box);

            saveButton = new UIButton<string>("保存");
            saveButton.SetSize(64, 32);
            saveButton.Left.Set(32, 0);
            saveButton.Top.Set(-48, 1);
            saveButton.OnLeftClick += (evt, elem) =>
            {
                var sequence = box.sequenceBase;
                sequence.Save();
                Type type = sequence.GetType();
                var method = type.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, [typeof(string), type]);

                Type mgrType = typeof(SequenceCollectionManager<>).MakeGenericType(CurrentSelectedType);


                method.Invoke(null, [sequence.LocalPath, (mgrType.GetField("sequences", BindingFlags.Public | BindingFlags.Static).GetValue(null) as IDictionary)[sequence.KeyName]]);
                currentSequences[sequence.KeyName] = sequence;
                info.lastModifyDate = DateTime.Now;
                SequenceSystem.sequenceInfos[box.sequenceBase.KeyName] = info;
                PendingModify = false;
                SoundEngine.PlaySound(SoundID.MenuOpen);
            };


            saveAsButton = new UIButton<string>("另存为...");
            saveAsButton.SetSize(128, 32);
            saveAsButton.Left.Set(112, 0);
            saveAsButton.Top.Set(-48, 1);
            saveAsButton.OnLeftClick += (evt, elem) =>
            {
                var sequence = box.sequenceBase.Clone();
                var info = SequenceSystem.sequenceInfos[sequence.KeyName].Clone();
                info.lastModifyDate = DateTime.Now;
                OpenBasicSetter(info, sequence);

            };

            revertButton = new UIButton<string>("撤销修改");
            revertButton.SetSize(128, 32);
            revertButton.Left.Set(256, 0);
            revertButton.Top.Set(-48, 1);
            revertButton.OnLeftClick += (evt, elem) =>
            {
                propList.Clear();
                box.Elements.Clear();
                box.groupBoxes.Clear();
                var sequence = box.sequenceBase;
                box.sequenceBase = SequenceSystem.instance.sequenceUI.currentSequences[sequence.KeyName].Clone();
                sequence = box.sequenceBase;
                foreach (var g in sequence.GroupBases)
                {
                    var gbox = new GroupBox(g);

                    box.groupBoxes.Add(gbox);

                }
                box.CacheRefresh = true;
                box.OnInitialize();
                box.Recalculate();
                PendingModify = false;
                SoundEngine.PlaySound(SoundID.MenuClose);

                infoList.Clear();
                info = SequenceSystem.sequenceInfos[box.sequenceBase.KeyName].Clone();
                foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(info))
                {
                    if (variable.Name == "passWord" || Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                        continue;
                    var (container, _elem) = UIModConfig.WrapIt(infoList, ref top, variable, info, order++);
                }
            };
            pendingModify = pendingModifyChange = false;
        }
        public void SequenceToPage(SequenceBox box)
        {
            SoundEngine.PlaySound(SoundID.Unlock);
            UIButton<string> seqPage = new UIButton<string>(box.sequenceBase.DisplayName);
            seqPage.SetSize(new Vector2(128, 0), 0, 1);
            pageList.Insert(pageList.Count - 1, seqPage);
            seqPage.OnLeftClick += (_evt, _elem) => { if (!pendingModify) SwitchToSequencePage(box); else Main.NewText("请先保存或撤销修改"); };
            seqPage.OnRightClick += (_evt, _elem) =>
            {
                if (!pendingModify)
                {
                    SwitchToDefaultPage();
                    pageList.Remove(_elem);
                }
                else Main.NewText("请先保存或撤销修改");
            };
            //UIText seqText = new UIText(box.sequenceBase.SequenceNameBase);
            //seqText.IgnoresMouseInteraction = true;
            //seqText.SetSize(default, 1, 1);
            //seqPage.Append(seqText);
        }
        public UIButton<string> SequenceToButton(SequenceBase sequence)
        {
            UIButton<string> panel = new UIButton<string>(sequence.DisplayName);
            panel.SetSize(-20, 40, 1, 0);
            //UIText uIText = new UIText(sequence.SequenceNameBase);
            SequenceBox box = new SequenceBox(sequence);
            box.SequenceSize(true);
            //panel.Append(uIText);
            //panel.Append(box);
            //if (SequenceDrawer.box != null && SequenceDrawer.box.sequenceBase.SequenceNameBase == sequence.SequenceNameBase) SequenceDrawer.box = box;

            panel.OnLeftClick += (evt, elem) =>
            {
                SequenceToPage(box);
            };
            panel.OnRightClick += (evt, elem) =>
            {
                SequenceToPage(box);
                SwitchToSequencePage(box);
            };
            panel.OnMouseOut += (evt, elem) =>
            {
                (elem as UIPanel).BackgroundColor = UICommon.DefaultUIBlueMouseOver;
            };
            panel.OnMouseOver += (evt, elem) =>
            {
                (elem as UIPanel).BackgroundColor = UICommon.DefaultUIBlue;
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            return panel;
        }
        public override void OnInitialize()
        {
            //CurrentSelectedType = SequenceSystem.AvailableElementBaseTypes[0];
            UIPanel BasePanel = new UIPanel();
            BasePanel.SetSize(default, 0.8f, 0.75f);
            BasePanel.Top.Set(0, 0.2f);
            BasePanel.Left.Set(0, 0.05f);
            Append(BasePanel);
            UIPanel BottonPanel = new UIPanel();
            BottonPanel.SetSize(default, 1.0f, 0.05f);
            BasePanel.Append(BottonPanel);
            UIPanel PagePanel = new UIPanel();
            PagePanel.SetSize(default, 0.85f, 0.05f);
            PagePanel.Top.Set(0, 0.05f);
            PagePanel.Left.Set(0, 0.15f);
            pageList = new UIListByRow();
            pageList.SetSize(default, 1, 1);
            PagePanel.Append(pageList);
            //UIScrollbarByRow pageScrollbar = new UIScrollbarByRow();
            //pageScrollbar.SetView(100f, 1000f);
            //pageScrollbar.Width.Set(0f, 1f);
            //pageScrollbar.VAlign = 1f;
            //PagePanel.Append(pageScrollbar);
            //pageList.SetScrollbar(pageScrollbar);
            BasePanel.Append(PagePanel);
            UIPanel InfoPanel = new UIPanel();
            InfoPanel.SetSize(default, 0.15f, 0.35f);
            InfoPanel.Top.Set(0, 0.05f);
            infoList = new UIList();
            infoList.SetSize(-20, -20, 1f, 1f);
            infoList.Top.Set(20, 0);
            UIScrollbar infoScrollBar = new UIScrollbar();
            infoScrollBar.SetView(100f, 1000f);
            infoScrollBar.Height.Set(0f, 1f);
            infoScrollBar.HAlign = 1f;
            InfoPanel.Append(infoList);
            InfoPanel.Append(infoScrollBar);
            infoList.SetScrollbar(infoScrollBar);
            BasePanel.Append(InfoPanel);
            UIText infoTitle = new UIText("当前页面序列信息");
            infoTitle.SetSize(0, 20, 1, 0);
            InfoPanel.Append(infoTitle);
            UIPanel PropertyPanel = new UIPanel();
            PropertyPanel.SetSize(default, 0.15f, 0.6f);
            PropertyPanel.Top.Set(0, 0.4f);
            BasePanel.Append(PropertyPanel);
            UIText propTitle = new UIText("选中组件信息");
            propTitle.SetSize(0, 20, 1, 0);
            PropertyPanel.Append(propTitle);
            propList = new UIList();
            propList.SetSize(-20, -20, 1, 1);
            propList.Top.Set(20, 0);
            PropertyPanel.Append(propList);
            var propScrollbar = new UIScrollbar();
            propScrollbar.SetView(100f, 1000f);
            propScrollbar.Height.Set(0f, 1f);
            propScrollbar.HAlign = 1f;
            PropertyPanel.Append(propScrollbar);
            propList.SetScrollbar(propScrollbar);
            OuterWorkingPanel = new UIPanel();
            OuterWorkingPanel.SetSize(default, 0.85f, 0.9f);
            OuterWorkingPanel.Top.Set(0, 0.1f);
            OuterWorkingPanel.Left.Set(0, 0.15f);
            OuterWorkingPanel.OverflowHidden = true;
            BasePanel.Append(OuterWorkingPanel);
            WorkingPlacePanel = new UIPanel();
            WorkingPlacePanel.SetSize(default, 0.65f / 0.85f, 1f);
            WorkingPlacePanel.OverflowHidden = true;
            WorkingPlacePanel.OnLeftMouseDown +=
                (evt, elem) =>
                {
                    if (Draggable && evt.Target == elem)
                    {
                        var e = elem.Elements.FirstOrDefault();
                        Offset = new Vector2(evt.MousePosition.X - e.Left.Pixels, evt.MousePosition.Y - e.Top.Pixels);
                        Dragging = true;
                    }
                };
            WorkingPlacePanel.OnLeftMouseUp += (evt, elem) => { Dragging = false; };
            WorkingPlacePanel.OnUpdate +=
                (elem) =>
                {
                    if (Dragging)
                    {
                        var e = elem.Elements.FirstOrDefault();
                        e.Left.Set(Main.mouseX - Offset.X, 0f);
                        e.Top.Set(Main.mouseY - Offset.Y, 0f);
                        e.Recalculate();
                    }
                };

            OuterWorkingPanel.Append(WorkingPlacePanel);
            UIPanel ActionLibraryPanel = new UIPanel();
            ActionLibraryPanel.SetSize(default, 0.2f / 0.85f, 0.4f);
            ActionLibraryPanel.Left.Set(0, 0.65f / 0.85f);
            OuterWorkingPanel.Append(ActionLibraryPanel);
            actionLib = new UIList();
            actionLib.SetSize(new Vector2(0, -20), 1, 1);
            actionLib.Top.Set(20, 0);
            actionLib.ListPadding = 16;
            ActionLibraryPanel.Append(actionLib);
            var actionScrollbar = new UIScrollbar();
            actionScrollbar.SetView(100f, 1000f);
            actionScrollbar.Height.Set(0f, 1f);
            actionScrollbar.HAlign = 1f;
            ActionLibraryPanel.Append(actionScrollbar);
            UIText actionTitle = new UIText("元素库");
            actionTitle.SetSize(0, 20, 1, 0);
            ActionLibraryPanel.Append(actionTitle);
            actionLib.SetScrollbar(actionScrollbar);
            UIPanel SequenceLibraryPanel = new UIPanel();
            SequenceLibraryPanel.SetSize(default, 0.2f / 0.85f, 0.6f);
            SequenceLibraryPanel.Left.Set(0, 0.65f / 0.85f);
            SequenceLibraryPanel.Top.Set(0, 0.4f);
            sequenceLib = new UIList();
            sequenceLib.SetSize(new Vector2(0, -20), 1, 1);
            sequenceLib.ListPadding = 16;
            SequenceLibraryPanel.Append(sequenceLib);
            var sequenceScrollbar = new UIScrollbar();
            sequenceScrollbar.SetView(100f, 1000f);
            sequenceScrollbar.Height.Set(0f, 1f);
            sequenceScrollbar.HAlign = 1f;
            SequenceLibraryPanel.Append(sequenceScrollbar);
            sequenceLib.SetScrollbar(sequenceScrollbar);
            sequenceLib.Top.Set(20, 0);
            UIText sequenceTitle = new UIText("序列库");
            sequenceTitle.SetSize(0, 20, 1, 0);
            SequenceLibraryPanel.Append(sequenceTitle);
            OuterWorkingPanel.Append(SequenceLibraryPanel);


            SetUpBasicInfoPanel();
            //ResetPage();

            /*
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
            Append(SequenceDrawer);*/
            base.OnInitialize();
        }
        public void SetUpBasicInfoPanel()
        {
            BasicInfoPanel = new UIPanel();
            BasicInfoPanel.SetSize(default, 0.5f, 0.5f);
            BasicInfoPanel.HAlign = BasicInfoPanel.VAlign = 0.5f;
            UIList infoList = new UIList();
            infoList.Top.Set(32, 0);
            infoList.SetSize(new Vector2(-40, -64), 1, 1);

            BasicInfoPanel.Append(infoList);

            //UIButton<string> yesButton = new UIButton<string>("确定");
            UIButton<string> noButton = new UIButton<string>("取消");
            noButton.BackgroundColor = Color.Red * .7f;
            //yesButton.SetSize(64, 32, 0, 0);
            noButton.SetSize(64, 32, 0, 0);
            //yesButton.Top.Set(-32, 1);
            noButton.Top.Set(-32, 1);
            //yesButton.HAlign = 0.125f;
            noButton.HAlign = 0.785f;
            //BasicInfoPanel.Append(yesButton);
            BasicInfoPanel.Append(noButton);
            noButton.OnLeftClick += (evt, elem) =>
            {
                BasicInfoPanel.Remove();
            };

        }
        public void OpenBasicSetter(SequenceBasicInfo info, SequenceBase sequence)//string modName,
        {
            var list = (BasicInfoPanel.Elements[0] as UIList);
            list.Clear();
            //wraper.SetConfigPanel(list);
            SoundEngine.PlaySound(SoundID.MenuOpen);
            int top = 0;
            int order = 0;
            WorkingPlacePanel.Append(BasicInfoPanel);
            foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(info))
            {
                if (variable.Type == typeof(DateTime) || Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                    continue;
                var (container, elem) = UIModConfig.WrapIt(list, ref top, variable, info, order++);
                //elem.OnLeftClick += (_evt, uielem) => { };
            }
            UIScrollbar uIScrollbar = new UIScrollbar();
            uIScrollbar.SetView(100f, 1000f);
            uIScrollbar.Height.Set(0f, 1f);
            uIScrollbar.HAlign = 1f;
            list.SetScrollbar(uIScrollbar);
            BasicInfoPanel.Append(uIScrollbar);
            UIButton<string> yesButton = new UIButton<string>("确定");
            yesButton.SetSize(64, 32, 0, 0);
            yesButton.Top.Set(-32, 1);
            yesButton.HAlign = 0.125f;
            BasicInfoPanel.Append(yesButton);
            yesButton.OnLeftClick += (evt, elem) =>
            {
                if (info.FileName == "")
                {
                    Main.NewText("文件名不可为空", Color.Red);
                    return;
                }
                if (info.DisplayName == "")
                {
                    Main.NewText("显示名不可为空", Color.Red);
                    return;
                }
                if (info.FileName == SequenceBase.SequenceDefaultName || currentSequences.ContainsKey(info.KeyName))
                {
                    Main.NewText("文件名不可用", Color.Red);
                    return;
                }
                if (info.ModName == null || !ModLoader.TryGetMod(info.ModName, out _))
                {
                    Main.NewText("不存在的模组", Color.Red);
                    return;
                }
                pendingModify = false;
                BasicInfoPanel.Remove();
                Main.NewText("保存成功", Color.LimeGreen);
                var dnames = from i in SequenceSystem.sequenceInfos.Values select i.DisplayName;
                if (dnames.Contains(info.DisplayName))
                {
                    Main.NewText("但是建议避免显示名的重复", Color.Orange);
                }
                //SequenceSystem.instance.sequenceUI.currentSequences[info.KeyName] = sequence;
                (typeof(SequenceCollectionManager<>).MakeGenericType(CurrentSelectedType).GetField("sequences", BindingFlags.Static | BindingFlags.Public).GetValue(null) as IDictionary)[info.KeyName] = sequence;
                SequenceSystem.sequenceInfos[info.KeyName] = info;
                var box = new SequenceBox(sequence);
                sequence.SyncInfo(info);
                sequence.Save();
                SequenceToPage(box);
                SwitchToSequencePage(box);
            };
        }


        public void Open()
        {
            Visible = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            CurrentSelectedType = SequenceSystem.AvailableElementBaseTypes[0];
            //UIPanel BasePanel = Elements[0] as UIPanel;
            //BasePanel.SetSize(default, 0.8f, 0.75f);
            //BasePanel.Top.Set(0, 0.2f);
            //BasePanel.Left.Set(0, 0.05f);
            ////Elements[0].SetSize(2560, 1600 * 0.8f);
            ///

            Elements.Clear();
            OnInitialize();


            ResetPage();
            ReloadLib();
            Recalculate();
            //SequenceDrawer.Top.Set(240, 0);
            //SequenceDrawer.Left.Set(0, 1.25f);

            //if (SequenceDrawer.box != null)
            //{
            //    SequenceDrawer.box.SequenceSize();
            //}
            //UIConfigSetterContainer.Top.Set(400, 0);
            //SetupConfigList();
        }
        public void Close()
        {
            Visible = false;
            Main.blockInput = false;
            //SequenceDrawer.box = null;
            //currentWraper = null;
            //RemoveChild(UIConfigSetterContainer);
            //RemoveChild(currentBox);
            saveButton?.Remove();
            PendingModify = false;
            pendingModifyChange = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
        public override void Update(GameTime gameTime)
        {
            //if (ConfigElemList != null)
            //    foreach (var u in from uiS in ConfigElemList._innerList.Children select uiS.Children)
            //        foreach(var v in u)
            //        Main.NewText(v.GetType());
            if (Elements[0].ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
                PlayerInput.LockVanillaMouseScroll("LogSpiralLibrary:SequenceUI");
            }
            if (pendingModifyChange)
            {
                if (pendingModify)
                {
                    WorkingPlacePanel.Append(saveButton);
                    WorkingPlacePanel.Append(saveAsButton);
                    WorkingPlacePanel.Append(revertButton);

                }
                else
                {
                    saveButton?.Remove();
                    saveAsButton?.Remove();
                    revertButton?.Remove();
                }
                pendingModifyChange = false;
            }
            base.Update(gameTime);
        }
    }
    public class SequenceBasicInfo
    {
        [JsonIgnore]
        public string KeyName => $"{ModName}/{FileName}";
        [CustomModConfigItem(typeof(SeqStringInputElement))]
        public string AuthorName;
        [CustomModConfigItem(typeof(SeqStringInputElement))]
        public string Description;
        [CustomModConfigItem(typeof(SeqStringInputElement))]
        public string FileName;
        [CustomModConfigItem(typeof(SeqStringInputElement))]
        public string DisplayName;
        //[CustomModConfigItem(typeof(SeqStringInputElement))]
        //public string ModName;
        [CustomModConfigItem(typeof(ModDefinitionElement))]
        public ModDefinition ModDefinition = new ModDefinition("LogSpiralLibrary");
        [JsonIgnore]
        public string ModName => ModDefinition?.Name ?? "UnknownMod";
        [CustomModConfigItem(typeof(DateTimeElement))]
        public DateTime createDate;
        [CustomModConfigItem(typeof(DateTimeElement))]
        public DateTime lastModifyDate;
        //TODO RSA加密
        [CustomModConfigItem(typeof(SeqStringInputElement))]
        public string passWord;
        public bool Finished;
        //public enum SequenceMode
        //{
        //    balanced = 0,
        //    expansion = 1,
        //    developer = 2,
        //    lunatic = 3
        //}
        public void Save(XmlWriter writer)
        {
            writer.WriteAttributeString("AuthorName", AuthorName);
            writer.WriteAttributeString("Description", Description);
            writer.WriteAttributeString("FileName", FileName);
            writer.WriteAttributeString("DisplayName", DisplayName);
            writer.WriteAttributeString("ModName", ModName);
            writer.WriteAttributeString("createTime", createDate.Ticks.ToString());
            writer.WriteAttributeString("lastModifyDate", lastModifyDate.Ticks.ToString());
            writer.WriteAttributeString("passWord", passWord);
            writer.WriteAttributeString("Finished", Finished.ToString());
        }
        public SequenceBasicInfo Load(XmlReader reader)
        {
            AuthorName = reader["AuthorName"] ?? "不认识的孩子呢";
            Description = reader["Description"] ?? "绝赞的描述";
            FileName = reader["FileName"] ?? "Error";
            DisplayName = reader["DisplayName"] ?? "出错的序列";
            ModDefinition = new ModDefinition(reader["ModName"]);
            //ModName = reader["ModName"] ?? "Unknown";//"LogSpiralLibrary";
            createDate = new DateTime(long.Parse(reader["createTime"] ?? "0"));
            lastModifyDate = new DateTime(long.Parse(reader["lastModifyDate"] ?? "0"));
            passWord = reader["passWord"] ?? "";
            Finished = bool.Parse(reader["Finished"] ?? "false");
            return this;
        }
        public SequenceBasicInfo Clone() => MemberwiseClone() as SequenceBasicInfo;
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
            var desc = wraperBox.wraper.Condition.Description.ToString();
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
                    if (wraperBox.wraper.Condition.IsMet())
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
                    if (wraperBox.wraper.Condition.IsMet())
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
                string desc = w.wraper.Condition.Description.ToString();
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
    public class WraperBox : UIElement
    {
        public UIPanel panel;
        public SequenceBase.WraperBase wraper;
        public SequenceBox sequenceBox;
        public bool CacheRefresh;
        public bool chosen;
        public bool Dragging;
        public bool IsClone;
        public WraperBox Clone()
        {
            return new WraperBox(wraper.Clone());
        }
        public WraperBox(SequenceBase.WraperBase wraperBase)
        {
            wraper = wraperBase;
            if (wraper.IsSequence)
                sequenceBox = new SequenceBox(wraper.SequenceInfo);

        }
        public override void RightClick(UIMouseEvent evt)
        {
            if (IsClone)
            {
                Main.NewText("此处无法编辑参数，请将其拖入序列中再编辑");
                return;
            }
            if (evt.Target != this && !(this.BelongToMe(evt.Target) && evt.Target is not WraperBox)) return;
            //Main.NewText((GetHashCode(), evt.Target.GetHashCode()));
            var ui = SequenceSystem.instance.sequenceUI;
            var list = ui.propList;
            list.Clear();
            if (ui.currentWraper != null)
                ui.currentWraper.chosen = false;
            ui.currentWraper = this;
            chosen = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            int top = 0;
            int order = 0;
            {
                var fieldInfo = wraper.GetType().GetField("conditionDefinition", BindingFlags.Instance | BindingFlags.Public);
                if (fieldInfo != null)
                    UIModConfig.WrapIt(list, ref top, new PropertyFieldWrapper(fieldInfo), wraper, order);
            }
            if (wraper.IsSequence)
            {
                if (!sequenceBox.Expand || sequenceBox.sequenceBase.FileName != SequenceBase.SequenceDefaultName)
                {
                    UIButton<string> TurnToButton = new UIButton<string>("SwtichToSequencePage");
                    TurnToButton.SetSize(new Vector2(0, 32), 0.8f, 0f);
                    TurnToButton.HAlign = 0.5f;
                    list.Add(TurnToButton);
                    TurnToButton.OnLeftClick += (evt, elem) =>
                    {
                        ui.SequenceToPage(sequenceBox);
                        ui.SwitchToSequencePage(sequenceBox);
                    };
                }
            }
            else
            {
                //wraper.SetConfigPanel(list);
                foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(wraper.Element))
                {
                    if (!Attribute.IsDefined(variable.MemberInfo, typeof(ElementCustomDataAttribute)) || Attribute.IsDefined(variable.MemberInfo, typeof(ElementCustomDataAbabdonedAttribute)))
                        continue;
                    var (container, elem) = UIModConfig.WrapIt(list, ref top, variable, wraper.Element, order++);
                }
            }
            base.RightClick(evt);
        }
        public override void MouseOver(UIMouseEvent evt)
        {
            //if (Dragging)
            //{
            //    Main.NewText((evt.Target.GetHashCode(),this.GetHashCode(),this.BelongToMe(evt.Target)));
            //}
            base.MouseOver(evt);
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (SequenceSystem.instance.sequenceUI.WorkingPlacePanel.Elements[0] is not SequenceBox box)
                return;
            SoundEngine.PlaySound(SoundID.CoinPickup);
            if (IsClone)
            {
                WraperBox copy = Clone();
                if (wraper.IsSequence)
                    copy.sequenceBox.Expand = false;
                copy.Dragging = true;
                copy.OnInitialize();
                SequenceSystem.instance.sequenceUI.OuterWorkingPanel.Append(copy);
                var vec = Main.MouseScreen - copy.Parent.GetDimensions().Position() - copy.GetSize() * .5f;
                copy.Top.Set(vec.X, 0);
                copy.Left.Set(vec.X, 0);
                copy.Recalculate();
                SequenceSystem.instance.userInterfaceSequence.LeftMouse.LastDown = copy;
            }
            else
            {
                if (this.Parent.Parent.GetHashCode() == box.GetHashCode() && box.groupBoxes.Count == 1 && box.groupBoxes.First().wraperBoxes.Count == 1)
                {
                    Main.NewText("只剩这个元素了，无法移动或者移除");
                    return;

                }
                Dragging = true;

                if (Parent is GroupBox group)
                {
                    group.wraperBoxes.Remove(this);
                    this.Remove();
                    group.CacheRefresh = true;

                    SequenceBox sBox = (group.Parent as SequenceBox);
                    sBox.sequenceBase.Remove(wraper, group.group);
                    if (group.wraperBoxes.Count == 0)
                    {
                        sBox.groupBoxes.Remove(group);
                        group.Remove();


                    }

                    SequenceBox mainBox = SequenceSystem.instance.sequenceUI.WorkingPlacePanel.Elements[0] as SequenceBox;
                    mainBox.Elements.Clear();
                    mainBox.CacheRefresh = true;
                    mainBox.OnInitialize();
                    mainBox.Recalculate();
                }
                Remove();
                SequenceSystem.instance.sequenceUI.OuterWorkingPanel.Append(this);
            }
            base.LeftMouseDown(evt);
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if (!Dragging) return;
            if (!this.BelongToMe(evt.Target))
                return;
            SequenceUI sequenceUI = SequenceSystem.instance.sequenceUI;
            if (sequenceUI.WorkingPlacePanel.Elements[0] is not SequenceBox box)
            {
                return;
            }
            sequenceUI.OuterWorkingPanel.RemoveChild(this);
            box.InsertWraper(this, evt.MousePosition);
            sequenceUI.PendingModify = true;
            box.Elements.Clear();
            box.CacheRefresh = true;
            box.OnInitialize();
            box.Recalculate();
            //box = new SequenceBox(box.sequenceBase);
            //box.SequenceSize();
            //box.OnInitialize();
            Dragging = false;
            base.LeftMouseUp(evt);
        }
        public override void Update(GameTime gameTime)
        {
            if (Dragging)
            {
                var vec = Main.MouseScreen - Parent.GetDimensions().Position() - this.GetSize() * .5f;
                Left.Set(vec.X, 0f);
                Top.Set(vec.Y, 0f);
                Recalculate();
            }
            MaxWidth = MaxHeight = new StyleDimension(223214514, 0);
            base.Update(gameTime);
        }
        public override void OnInitialize()
        {
            Vector2 size = this.GetSize();
            panel = new UIPanel();
            panel.SetSize(size);
            if (wraper.IsSequence && (sequenceBox.Expand && wraper.SequenceInfo.FileName == SequenceBase.SequenceDefaultName))
            {
                var desc = wraper.Condition.Description.ToString();
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
            //Vector4 vec = new Vector4(0,0, 0, 1.5f + MathF.Cos((float)LogSpiralLibrarySystem.ModTime / 60) * .5f);// MathF.Cos((float)LogSpiralLibrarySystem.ModTime / 60) * 60
            //object m = spriteBatch.GetType().GetField("transformMatrix", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(spriteBatch);
            //spriteBatch.GetType().GetField("transformMatrix", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(spriteBatch, Main.UIScaleMatrix with { /*M14 = vec.X,*/ M24 = vec.Y, M34 = vec.Z, M44 = vec.W });
            if (SequenceConfig.Instance.ShowWrapBox)
                spriteBatch.DrawRectangle(this.GetDimensions().ToRectangle(), Color.MediumPurple, 12);
            var desc = wraper.Condition.Description.ToString();
            bool flag = desc != "Always";
            var wraperBox = this;
            var position = this.GetDimensions().Position() + new Vector2(0, this.GetDimensions().Height * .5f);
            if (wraperBox.wraper.IsSequence && wraperBox.sequenceBox.Expand && wraperBox.wraper.SequenceInfo.FileName == SequenceBase.SequenceDefaultName)
            {
                ComplexPanelInfo panel = new ComplexPanelInfo();
                var boxSize = wraperBox.WrapperSize();
                //if (flag)
                //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f + new Vector2(0, 16), boxSize + new Vector2(32, 64));
                //else
                //    panel.destination = Utils.CenteredRectangle(position + boxSize * .5f, boxSize + new Vector2(32, 32));
                float offY = flag ? FontAssets.MouseText.Value.MeasureString(desc).Y : 0;
                panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X * .5f, 0), boxSize);
                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
                panel.glowEffectColor = chosen ? Main.DiscoColor with { A = 0 } : Color.MediumPurple with { A = 102 };
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
                if (wraper.IsSequence && SequenceSystem.sequenceInfos.TryGetValue(wraper.SequenceInfo.KeyName, out var value))
                    name = value.DisplayName;
                Vector2 textSize = font.MeasureString(name);
                Vector2 boxSize = wraperBox.WrapperSize();
                #region 框框
                ComplexPanelInfo panel = new ComplexPanelInfo();
                panel.destination = Utils.CenteredRectangle(position + new Vector2(boxSize.X, 0) * .5f, boxSize);
                panel.StyleTexture = ModContent.Request<Texture2D>("LogSpiralLibrary/Images/ComplexPanel/panel_2").Value;
                if (wraper.IsSequence)
                    panel.glowEffectColor = chosen ? Color.Lerp(Color.Blue, Color.Purple, 0.5f) with { A = 0 } : Color.MediumPurple with { A = 102 };
                else
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
                    var tarVec = SequenceSystem.instance.sequenceUI.propList.GetDimensions().ToRectangle().TopRight();
                    spriteBatch.DrawHorizonBLine(tarVec, position, Color.White with { A = 0 } * .125f);
                    spriteBatch.DrawHorizonBLine(tarVec + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4), position + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4), Main.DiscoColor with { A = 0 } * .125f, 1, 6);

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
        //public void Add(WraperBox wraperBox)
        //{
        //    wraperBoxes.Add(wraperBox);
        //    Elements.Clear();
        //    group.Wrapers.Add(wraperBox.wraper);
        //    OnInitialize();
        //}
        public override void OnInitialize()
        {
            //this.IgnoresMouseInteraction = true;
            Vector2 size = this.GetSize();
            panel = new UIPanel();
            panel.SetSize(size);
            Elements.Clear();
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
            if (SequenceConfig.Instance.ShowGroupBox)
                spriteBatch.DrawRectangle(this.GetDimensions().ToRectangle(), Color.Cyan * .75f, 8);
            var dimension = GetDimensions();
            var scale = 1 + ((float)LogSpiralLibraryMod.ModTime / 180).CosFactor();

            foreach (var w in wraperBoxes)
            {
                var wD = w.GetDimensions();
                var offY = w.wraper.Condition.Description.Value != "Always" ? -FontAssets.MouseText.Value.MeasureString("→" + w.wraper.Condition.Description).Y * .5f : 0;
                //谜题之我也不知道这一个像素的偏移是哪里来的
                spriteBatch.DrawHorizonBLine(dimension.Position() + new Vector2(-SequenceConfig.Instance.Step.X * .25f, dimension.Height * .5f) + new Vector2(-1, 0), wD.Position() + new Vector2(0, wD.Height * .5f + offY), Color.White, scale);
                spriteBatch.DrawHorizonBLine(dimension.Position() + new Vector2(SequenceConfig.Instance.Step.X * .25f + dimension.Width, dimension.Height * .5f) + new Vector2(-1, 0), wD.Position() + new Vector2(wD.Width, wD.Height * .5f + offY), Color.White, scale);

                //spriteBatch.Draw(TextureAssets.MagicPixel.Value, wD.Position() + new Vector2(0, wD.Height * .5f), new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(0.5f), 16f, 0, 0);
            }
            base.DrawSelf(spriteBatch);
        }
        public override void Update(GameTime gameTime)
        {
            MaxWidth = MaxHeight = new StyleDimension(223214514, 0);

            base.Update(gameTime);
        }
    }
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
                    this.CacheRefresh = true;
                    this.SequenceSize(true);
                }
            }
        }
        public UIPanel panel;
        public SequenceBase sequenceBase;
        public List<GroupBox> groupBoxes = new List<GroupBox>();
        public bool CacheRefresh;
        public float offY;
        //public bool startSequence;
        public SequenceBox(SequenceBase sequence)
        {
            sequenceBase = sequence;
            foreach (var g in sequence.GroupBases)
            {
                var gbox = new GroupBox(g);

                groupBoxes.Add(gbox);

            }
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
                    else if (center.X > w.GetDimensions().ToRectangle().Top)
                        inWraper = true;
                }
                if (inWraper)
                {
                    WraperBox wraperBox = groupBox.wraperBoxes[wIndex];
                    if (wraperBox.wraper.IsSequence)
                    {
                        wraperBox.sequenceBox.InsertWraper(wraper, center);//递归入口
                        Main.NewText("递归");
                    }
                    else
                    {
                        //出口三，和先前的元素组成新的序列
                        Main.NewText("这里本来有个出口三，但是我还没做完((");
                    }
                }
                else
                {
                    //出口二，作为独立单元并联
                    this.sequenceBase.GroupBases[gIndex].Insert(wIndex, wraper.wraper);
                    this.groupBoxes[gIndex].wraperBoxes.Insert(wIndex, wraper);
                    Main.NewText("出口二");
                }
            }
            else
            {
                //出口一，作为独立单元串联
                this.sequenceBase.Insert(gIndex, wraper.wraper, out var nG);
                groupBoxes.Insert(gIndex, new GroupBox(nG));
                if (wraper.wraper.IsSequence)
                    groupBoxes[gIndex].wraperBoxes[0].sequenceBox.expand = wraper.sequenceBox.expand;
                Main.NewText("出口一");

            }
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
            panel = new UIPanel();
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
    public static class SequenceDrawHelper
    {
        public static Vector2 WrapperSize(this WraperBox wrapBox)
        {
            Vector2 curr = wrapBox.GetSize();
            if (curr == default || wrapBox.CacheRefresh)
            {
                Vector2 delta;
                var wraper = wrapBox.wraper;
                var desc = wraper.Condition.Description.Value;
                if (wraper.IsSequence && wrapBox.sequenceBox.Expand && wraper.SequenceInfo.FileName == SequenceBase.SequenceDefaultName)
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
                    if (wraper.IsSequence && SequenceSystem.sequenceInfos.TryGetValue(wraper.SequenceInfo.KeyName, out var value))
                        name = value.DisplayName;
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
        public static Vector2 SequenceSize(this SequenceBox sequencebox, bool start = false)
        {

            Vector2 curr = sequencebox.GetSize();
            if (curr == default || sequencebox.CacheRefresh)
            {
                sequencebox.CacheRefresh = false;
                Vector2 result = new Vector2(start ? 32 : 0, 0);//sequencebox.startSequence // SequenceConfig.Instance.Step.X * .5f
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
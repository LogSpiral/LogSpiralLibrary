using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UIBase;
using LogSpiralLibrary.CodeLibrary.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.UI;
using System.IO;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;
using System.Text.Json.Serialization;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System.UI;

public class SequenceUI : UIState
{
    public const string localizationPath = "Mods.LogSpiralLibrary.SequenceUI";
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
    public Dictionary<string, Sequence> currentSequences => (from pair in SequenceManager<MeleeAction>.sequences select new KeyValuePair<string, Sequence>(pair.Key, pair.Value)).ToDictionary();//SequenceSystem.sequenceBases[CurrentSelectedType];//(Dictionary<string, SequenceBase>)typeof(SequenceCollectionManager<>).MakeGenericType(CurrentSelectedType).GetField("sequences",BindingFlags.Static | BindingFlags.Public).GetValue(null);
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
    string hintText;
    SequenceBasicInfo currentInfo;
    public void ReloadLib()
    {
        actionLib.Clear();
        sequenceLib.Clear();
        Type elemBaseType = CurrentSelectedType;
        Type type = typeof(ModTypeLookup<>);
        type = type.MakeGenericType(elemBaseType);
        //Main.NewText(type.GetField("dict",BindingFlags.Static|BindingFlags.NonPublic) == null);
        IDictionary dict = (IDictionary)type.GetField("dict", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        type = typeof(Sequence<>.Wraper);
        type = type.MakeGenericType(elemBaseType);

        Dictionary<string, (UIDownList list, Dictionary<string, UIDownList> dict)> Categories = [];
        List<WraperBox> NoCategoryWrapers = [];
        foreach (var v in dict.Values)
        {
            var e = (ISequenceElement)Activator.CreateInstance(v.GetType());
            WraperBox wraperBox = new((Sequence.WraperBase)Activator.CreateInstance(type, [e]));
            wraperBox.WrapperSize();
            //wraperBox.HAlign = 0.5f;
            wraperBox.IsClone = true;

            string modName = e.Mod.DisplayName;
            string categoryName = e.Category;

            if (!Categories.TryGetValue(modName, out var singleModCategory)) //如果当前mod没添加过
            {
                UIDownList singleModList = new(modName)
                {
                    Width = new(0, 1)
                };
                actionLib.Add(singleModList);
                singleModList.OnInitialize();
                singleModCategory = Categories[modName] = (singleModList, []);
            }
            if (categoryName != "")
            {
                if (!singleModCategory.dict.TryGetValue(categoryName, out var categoriedList)) //如果当前分类没添加过
                {
                    string titleName = categoryName;
                    string key = $"Mods.{e.Mod.Name}.{CurrentSelectedType.Name}.Category.{categoryName}";
                    if (Language.Exists(key))
                        titleName = Language.GetTextValue(key);
                    UIDownList list = new(titleName)
                    {
                        Width = new(0, 1)
                    };
                    singleModCategory.list.Add(list);
                    list.OnInitialize();
                    categoriedList = singleModCategory.dict[categoryName] = list;
                }
                categoriedList.Add(wraperBox);
            }
            else
                NoCategoryWrapers.Add(wraperBox);
        }
        foreach (var box in NoCategoryWrapers)
            Categories[box.wraper.Element.Mod.DisplayName].list.Add(box);
        Categories = [];
        foreach (var s in currentSequences.Values)
        {
            WraperBox wraperBox = new((Sequence.WraperBase)Activator.CreateInstance(type, [s]));
            wraperBox.sequenceBox.Expand = false;
            wraperBox.WrapperSize();
            wraperBox.IsClone = true;


            string modName = s.Mod.DisplayName;
            string categoryName = SequenceSystem.sequenceInfos[s.KeyName].Finished ? "Finished" : "Library";

            if (!Categories.TryGetValue(modName, out var singleModCategory)) //如果当前mod没添加过
            {
                UIDownList singleModList = new(modName)
                {
                    Width = new(0, 1)
                };
                sequenceLib.Add(singleModList);
                singleModList.OnInitialize();
                singleModCategory = Categories[modName] = (singleModList, []);
            }
            if (!singleModCategory.dict.TryGetValue(categoryName, out var categoriedList)) //如果当前分类没添加过
            {

                UIDownList list = new(Language.GetOrRegister(localizationPath + (categoryName == "Finished" ? ".FinishedSequences" : ".LibrarySequences")).Value)
                {
                    Width = new(0, 1)
                };
                singleModCategory.list.Add(list);
                list.OnInitialize();
                categoriedList = singleModCategory.dict[categoryName] = list;
            }
            categoriedList.Add(wraperBox);
            //sequenceLib.Add(wraperBox);
        }
    }
    public void ResetPage()
    {
        pageList.Clear();
        UIButton<string> defPage = new(Language.GetOrRegister(localizationPath + ".DefaultPage").Value);//"默认页面"
        defPage.SetSize(new Vector2(128, 0), 0, 1);
        pageList.Add(defPage);
        defPage.OnLeftClick += (e, evt) => { SwitchToDefaultPage(); };
        //UIText defText = new UIText();
        //defText.IgnoresMouseInteraction = true;
        //defText.SetSize(default, 1, 1);
        //defPage.Append(defText);
        UIButton<string> newPage = new("+");
        newPage.SetSize(new Vector2(32, 0), 0, 1);

        newPage.OnLeftClick += (e, evt) =>
        {
            var seq = new MeleeSequence();
            seq.Add(new SwooshInfo());
            OpenBasicSetter(new SequenceBasicInfo() { createDate = DateTime.Now, lastModifyDate = DateTime.Now }, seq);
        };
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
        currentInfo = null;
        UIText info = new(Language.GetOrRegister(localizationPath + ".DefaultPageHint").Value);
        //            "你正处于默认页面，可以在这里选择一个序列以开始编辑。\n" +
        //"成品序列指适于直接给武器使用的完成序列\n" +
        //"库存序列指适于用来辅助组成成品序列可以反复调用的序列\n" +
        //"二者没有硬性区别，根据自己感觉进行标记即可。"
        var d = info.GetDimensions().Height;
        info.SetSize(new Vector2(-10, 1), 1, 0);
        infoList.Add(info);
        info.IsWrapped = true;
        UIPanel finishedSequencePanel = new LogSpiralLibraryPanel();
        finishedSequencePanel.SetSize(default, 0.5f, 1f);
        UIPanel libSequencePanel = new LogSpiralLibraryPanel();
        libSequencePanel.SetSize(default, 0.5f, 1f);
        libSequencePanel.Left.Set(0, 0.5f);
        WorkingPlacePanel.Append(libSequencePanel);
        WorkingPlacePanel.Append(finishedSequencePanel);
        UIText fTitle = new(Language.GetOrRegister(localizationPath + ".FinishedSequences"));
        fTitle.SetSize(new Vector2(0, 20), 1, 0);
        UIText lTitle = new(Language.GetOrRegister(localizationPath + ".LibrarySequences"));
        lTitle.SetSize(new Vector2(0, 20), 1, 0);
        finishedSequencePanel.Append(fTitle);
        libSequencePanel.Append(lTitle);
        UIList fList = [];
        fList.SetSize(0, -20, 1, 1);
        fList.Top.Set(20, 0);
        UIScrollbar fScrollbar = new();
        fScrollbar.Height.Set(0f, 1f);
        fScrollbar.HAlign = 1f;
        fScrollbar.SetView(100, 1000);
        fList.SetScrollbar(fScrollbar);
        finishedSequencePanel.Append(fScrollbar);
        UIList lList = [];
        lList.SetSize(0, -20, 1, 1);
        lList.Top.Set(20, 0);
        UIScrollbar lScrollbar = new();
        lScrollbar.Height.Set(0f, 1f);
        lScrollbar.HAlign = 1f;
        lScrollbar.SetView(100, 1000);
        lList.SetScrollbar(lScrollbar);
        libSequencePanel.Append(lScrollbar);
        finishedSequencePanel.Append(fList);
        libSequencePanel.Append(lList);
        foreach (var s in currentSequences.Values)
        {
            //var dic = SequenceSystem.sequenceInfos.Values;
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
        SequenceBasicInfo info = currentInfo = SequenceSystem.sequenceInfos[box.sequenceBase.KeyName].Clone();
        foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(info))
        {
            if (variable.Name == "passWord" || Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                continue;
            var (container, elem) = SequenceSystem.WrapIt(infoList, ref top, variable, info, order++);
        }
        WorkingPlacePanel.OverflowHidden = true;
        box.SequenceSize(true);
        box.OnInitialize();
        WorkingPlacePanel.Append(box);

        saveButton = new UIButton<string>(Language.GetOrRegister(localizationPath + ".Save").Value);
        saveButton.SetSize(64, 32);
        saveButton.Left.Set(32, 0);
        saveButton.Top.Set(-48, 1);
        saveButton.OnLeftClick += (evt, elem) =>
        {
            var sequence = box.sequenceBase;
            if (currentInfo != null)
                SequenceSystem.sequenceInfos[sequence.KeyName] = currentInfo;
            var info = SequenceSystem.sequenceInfos[sequence.KeyName];
            info.FileName = sequence.FileName;
            info.lastModifyDate = DateTime.Now;
            sequence.Save();
            Type type = sequence.GetType();
            var method = type.GetMethod("Load", BindingFlags.Static | BindingFlags.Public, [typeof(string), type]);

            Type mgrType = typeof(SequenceManager<>).MakeGenericType(CurrentSelectedType);

            var target = (mgrType.GetField("sequences", BindingFlags.Public | BindingFlags.Static).GetValue(null) as IDictionary)[sequence.KeyName];

            method.Invoke(null, [sequence.LocalPath, target]);
            currentSequences[sequence.KeyName] = sequence;
            SequenceSystem.sequenceInfos[box.sequenceBase.KeyName] = info;
            PendingModify = false;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                SyncSingleSequence.Get(Main.myPlayer, sequence, CurrentSelectedType).Send();
            }
        };


        saveAsButton = new UIButton<string>(Language.GetOrRegister(localizationPath + ".SaveAs").Value);
        saveAsButton.SetSize(128, 32);
        saveAsButton.Left.Set(112, 0);
        saveAsButton.Top.Set(-48, 1);
        void revertSeqData()
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
            currentInfo = info = SequenceSystem.sequenceInfos[box.sequenceBase.KeyName].Clone();
            foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(info))
            {
                if (variable.Name == "passWord" || Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                    continue;
                var (container, _elem) = SequenceSystem.WrapIt(infoList, ref top, variable, info, order++);
            }
        }
        ;
        saveAsButton.OnLeftClick += (evt, elem) =>
        {
            var sequence = box.sequenceBase.Clone();
            var info = SequenceSystem.sequenceInfos[sequence.KeyName].Clone();
            info.lastModifyDate = DateTime.Now;
            OpenBasicSetter(info, sequence);

            revertSeqData();
        };

        revertButton = new UIButton<string>(Language.GetOrRegister(localizationPath + ".Revert").Value);
        revertButton.SetSize(128, 32);
        revertButton.Left.Set(256, 0);
        revertButton.Top.Set(-48, 1);
        revertButton.OnLeftClick += (evt, elem) =>
        {
            revertSeqData();
        };
        pendingModify = pendingModifyChange = false;
    }
    public void SequenceToPage(SequenceBox box)
    {
        SoundEngine.PlaySound(SoundID.Unlock);
        UIButton<string> seqPage = new(box.sequenceBase.DisplayName);
        seqPage.SetSize(new Vector2(128, 0), 0, 1);
        pageList.Insert(pageList.Count - 1, seqPage);
        string hint = Language.GetOrRegister(localizationPath + ".SaveOrRevertPlz").Value;
        seqPage.OnLeftClick += (_evt, _elem) => { if (!pendingModify) SwitchToSequencePage(box); else Main.NewText(hint); };
        seqPage.OnRightClick += (_evt, _elem) =>
        {
            if (!pendingModify)
            {
                SwitchToDefaultPage();
                pageList.Remove(_elem);
            }
            else Main.NewText(hint);
        };
        //UIText seqText = new UIText(box.sequenceBase.SequenceNameBase);
        //seqText.IgnoresMouseInteraction = true;
        //seqText.SetSize(default, 1, 1);
        //seqPage.Append(seqText);
    }
    public UIButton<string> SequenceToButton(Sequence sequence)
    {
        UIButton<string> panel = new(sequence.DisplayName);
        panel.SetSize(-20, 40, 1, 0);
        //UIText uIText = new UIText(sequence.SequenceNameBase);
        SequenceBox box = new(sequence);
        box.SequenceSize(true);
        //panel.Append(uIText);
        //panel.Append(box);
        //if (SequenceDrawer.box != null && SequenceDrawer.box.sequenceBase.SequenceNameBase == sequence.SequenceNameBase) SequenceDrawer.box = box;
        string filePath = $"PresetSequences/{sequence.ElementTypeName}/{sequence.FileName}.xml";
        bool resetable = sequence.Mod.GetFileNames().Contains(filePath);
        UIImageButton delete = new(resetable ? Main.Assets.Request<Texture2D>("Images/UI/CharCreation/HairStyle_Arrow") : Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete"));
        delete.OnLeftClick += (evt, elem) =>
        {

            if (resetable)
            {
                sequence.Reset();
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    SyncSingleSequence.Get(Main.myPlayer, sequence, CurrentSelectedType).Send();
                }
                SwitchToDefaultPage();
                //var list = panel.Parent.Parent as UIList;
                //int index = list._items.IndexOf(panel);
                //list.Remove(panel);

                //var nPanel = SequenceToButton(currentSequences[sequence.KeyName]);

                //list._items.Insert(index,nPanel);
                //list._innerList.Append(nPanel);
                //list.UpdateOrder();
                //list._innerList.Recalculate();
            }
            else
            {
                //bool finished = SequenceSystem.sequenceInfos[sequence.KeyName].Finished;
                //(finished ? )
                var list = panel.Parent.Parent as UIList;
                list.Remove(panel);
            }
            ;
        };
        //delete.SetSize(20, 20, 0, 0);
        delete.Left.Set(-20, 1);
        //delete.VAlign = 0.5f;
        delete.Top.Set(-12, 0.5f);
        delete.OnUpdate += e =>
        {
            if (e.IsMouseHovering)
                hintText = Language.GetOrRegister(localizationPath + (resetable ? ".resetSequence" : ".deleteSequence")).Value;
            //Main.instance.MouseText(Language.GetOrRegister(localizationPath + (resetable ? ".resetSequence" : ".deleteSequence")).Value);
        };
        if (resetable)//TODO 给其它序列添加游戏内删除功能
            panel.Append(delete);
        panel.OnLeftClick += (evt, elem) =>
        {
            if (evt.Target == elem)
                SequenceToPage(box);
        };
        panel.OnRightClick += (evt, elem) =>
        {
            if (evt.Target == elem)
            {
                SequenceToPage(box);
                SwitchToSequencePage(box);
            }
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
        UIPanel BasePanel = new DraggableUIPanel();
        //BasePanel.SetSize(default, 0.8f, 0.75f);
        //BasePanel.Top.Set(0, 0.2f);
        //BasePanel.Left.Set(0, 0.05f);
        BasePanel.SetSize(Main.ScreenSize.ToVector2() * new Vector2(0.8f, 0.75f));
        BasePanel.Top.Set(Main.screenHeight * (Main.playerInventory ? .2f : .05f), 0);
        BasePanel.Left.Set(Main.screenWidth * .05f, 0);

        Append(BasePanel);
        UIPanel BottonPanel = new LogSpiralLibraryPanel();
        BottonPanel.SetSize(default, 1.0f, 0.05f);
        UIImageButton roseButton = new(ModAsset.Rose);
        roseButton.OnLeftClick += (evt, elem) =>
        {
            Utils.OpenToURL("https://space.bilibili.com/259264134");
            SoundEngine.PlaySound(SoundID.MenuOpen);
        };
        roseButton.OnUpdate += e =>
        {
            if (e.IsMouseHovering)
                hintText = Language.GetOrRegister(localizationPath + ".OpenHomePage").Value;

            //Main.instance.MouseText(Language.GetOrRegister(localizationPath + ".OpenHomePage").Value);
        };
        BottonPanel.Append(roseButton);
        UIImageButton openFolderButton = new(ModAsset.Folder);
        openFolderButton.OnLeftClick += (evt, elem) =>
        {
            string path = $"{Main.SavePath}/Mods/LogSpiralLibrary_Sequence";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Utils.OpenFolder(path);
        };
        openFolderButton.OnUpdate += e =>
        {
            if (e.IsMouseHovering)
                hintText = Language.GetOrRegister(localizationPath + ".OpenSquenceFolder").Value;

            //Main.instance.MouseText(Language.GetOrRegister(localizationPath + ".OpenSquenceFolder").Value);
        };
        //openFolderButton.SetSize(new Vector2(32));
        openFolderButton.Left.Set(48, 0);
        BottonPanel.Append(openFolderButton);
        BasePanel.Append(BottonPanel);


        UIPanel PagePanel = new LogSpiralLibraryPanel();
        PagePanel.SetSize(default, 0.85f, 0.05f);
        PagePanel.Top.Set(0, 0.05f);
        PagePanel.Left.Set(0, 0.15f);
        pageList = [];
        pageList.SetSize(default, 1, 1);
        PagePanel.Append(pageList);
        PagePanel.PaddingTop = PagePanel.PaddingBottom = PagePanel.PaddingLeft = PagePanel.PaddingRight = 6;
        BottonPanel.MinHeight = PagePanel.MinHeight = new StyleDimension(40, 0);
        //UIScrollbarByRow pageScrollbar = new UIScrollbarByRow();
        //pageScrollbar.SetView(100f, 1000f);
        //pageScrollbar.Width.Set(0f, 1f);
        //pageScrollbar.VAlign = 1f;
        //PagePanel.Append(pageScrollbar);
        //pageList.SetScrollbar(pageScrollbar);
        BasePanel.Append(PagePanel);
        UIPanel InfoPanel = new LogSpiralLibraryPanel();
        InfoPanel.SetSize(default, 0.15f, 0.35f);
        InfoPanel.Top.Set(0, 0.05f);
        infoList = [];
        infoList.SetSize(-20, -20, 1f, 1f);
        infoList.Top.Set(20, 0);
        UIScrollbar infoScrollBar = new();
        infoScrollBar.SetView(100f, 1000f);
        infoScrollBar.Height.Set(0f, 1f);
        infoScrollBar.HAlign = 1f;
        InfoPanel.Append(infoList);
        InfoPanel.Append(infoScrollBar);
        infoList.SetScrollbar(infoScrollBar);
        BasePanel.Append(InfoPanel);
        UIText infoTitle = new(Language.GetOrRegister(localizationPath + ".InfoTitle").Value);
        infoTitle.SetSize(0, 20, 1, 0);
        InfoPanel.Append(infoTitle);
        UIPanel PropertyPanel = new LogSpiralLibraryPanel();
        PropertyPanel.SetSize(default, 0.15f, 0.6f);
        PropertyPanel.Top.Set(0, 0.4f);
        BasePanel.Append(PropertyPanel);
        UIText propTitle = new(Language.GetOrRegister(localizationPath + ".PropTitle").Value);
        propTitle.SetSize(0, 20, 1, 0);
        PropertyPanel.Append(propTitle);
        propList = [];
        propList.SetSize(-20, -20, 1, 1);
        propList.Top.Set(20, 0);
        PropertyPanel.Append(propList);
        var propScrollbar = new UIScrollbar();
        propScrollbar.SetView(100f, 1000f);
        propScrollbar.Height.Set(0f, 1f);
        propScrollbar.HAlign = 1f;
        PropertyPanel.Append(propScrollbar);

        InfoPanel.MinWidth = PropertyPanel.MinWidth = new StyleDimension(300, 0);
        propList.SetScrollbar(propScrollbar);
        OuterWorkingPanel = new LogSpiralLibraryPanel();
        OuterWorkingPanel.SetSize(default, 0.85f, 0.9f);
        OuterWorkingPanel.Top.Set(0, 0.1f);
        OuterWorkingPanel.Left.Set(0, 0.15f);
        OuterWorkingPanel.OverflowHidden = true;
        BasePanel.Append(OuterWorkingPanel);
        WorkingPlacePanel = new LogSpiralLibraryPanel();
        WorkingPlacePanel.SetSize(default, 0.65f / 0.85f, 1f);
        WorkingPlacePanel.OverflowHidden = true;
        WorkingPlacePanel.OnLeftMouseDown +=
            (evt, elem) =>
            {
                if (Draggable && (evt.Target == elem || WorkingPlacePanel.Elements[0] is SequenceBox sb && sb.MouseCheckInEmptySpace(evt.MousePosition)))
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
        UIPanel ActionLibraryPanel = new LogSpiralLibraryPanel();
        ActionLibraryPanel.SetSize(default, 0.2f / 0.85f, 0.4f);
        ActionLibraryPanel.Left.Set(0, 0.65f / 0.85f);
        OuterWorkingPanel.Append(ActionLibraryPanel);
        actionLib = [];
        actionLib.SetSize(new Vector2(0, -20), 1, 1);
        actionLib.Top.Set(20, 0);
        actionLib.ListPadding = 16;
        ActionLibraryPanel.Append(actionLib);
        var actionScrollbar = new UIScrollbar();
        actionScrollbar.SetView(100f, 1000f);
        actionScrollbar.Height.Set(0f, 1f);
        actionScrollbar.HAlign = 1f;
        ActionLibraryPanel.Append(actionScrollbar);
        UIText actionTitle = new(Language.GetOrRegister(localizationPath + ".ActionLibrary").Value);
        actionTitle.SetSize(0, 20, 1, 0);
        ActionLibraryPanel.Append(actionTitle);
        actionLib.SetScrollbar(actionScrollbar);
        UIPanel SequenceLibraryPanel = new LogSpiralLibraryPanel();
        SequenceLibraryPanel.SetSize(default, 0.2f / 0.85f, 0.6f);
        SequenceLibraryPanel.Left.Set(0, 0.65f / 0.85f);
        SequenceLibraryPanel.Top.Set(0, 0.4f);
        sequenceLib = [];
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
        UIText sequenceTitle = new(Language.GetOrRegister(localizationPath + ".SequenceLibrary").Value);
        sequenceTitle.SetSize(0, 20, 1, 0);
        SequenceLibraryPanel.Append(sequenceTitle);
        SequenceLibraryPanel.MinWidth = ActionLibraryPanel.MinWidth = new StyleDimension(400, 0);
        OuterWorkingPanel.Append(SequenceLibraryPanel);


        SetUpBasicInfoPanel();
        //ResetPage();

        /*
        UIPanel panel = UIPanel = new LogSpiralLibraryPanel();
        panel.SetSize(new Vector2(240, 300));
        panel.Top.Set(80, 0);
        panel.Left.Set(20, 0);
        Append(panel);

        UIPanel container = UIConfigSetterContainer = new LogSpiralLibraryPanel();
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
        BasicInfoPanel = new LogSpiralLibraryPanel();
        BasicInfoPanel.SetSize(default, 0.5f, 0.5f);
        BasicInfoPanel.HAlign = BasicInfoPanel.VAlign = 0.5f;
        UIList infoList = [];
        infoList.Top.Set(32, 0);
        infoList.SetSize(new Vector2(-40, -64), 1, 1);

        BasicInfoPanel.Append(infoList);

        //UIButton<string> yesButton = new UIButton<string>("确定");
        UIButton<string> noButton = new(Language.GetOrRegister(localizationPath + ".Cancel").Value);
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
    public void OpenBasicSetter(SequenceBasicInfo info, Sequence sequence)//string modName,
    {
        var list = BasicInfoPanel.Elements[0] as UIList;
        list.Clear();
        //wraper.SetConfigPanel(list);
        SoundEngine.PlaySound(SoundID.MenuOpen);
        int top = 0;
        int order = 0;
        WorkingPlacePanel.Append(BasicInfoPanel);
        foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(info))
        {
            if (variable.Type == typeof(DateTime) || variable.Name == "passWord" || Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
                continue;
            var (container, elem) = SequenceSystem.WrapIt(list, ref top, variable, info, order++);
            //elem.OnLeftClick += (_evt, uielem) => { };
        }
        UIScrollbar uIScrollbar = new();
        uIScrollbar.SetView(100f, 1000f);
        uIScrollbar.Height.Set(0f, 1f);
        uIScrollbar.HAlign = 1f;
        list.SetScrollbar(uIScrollbar);
        if (BasicInfoPanel.Elements.Count > 2 && BasicInfoPanel.Elements[2] is UIScrollbar oldBar)
            oldBar.Remove();
        BasicInfoPanel.Append(uIScrollbar);
        UIButton<string> yesButton = new(Language.GetOrRegister(localizationPath + ".OK").Value);
        yesButton.SetSize(64, 32, 0, 0);
        yesButton.Top.Set(-32, 1);
        yesButton.HAlign = 0.125f;
        if (BasicInfoPanel.Elements.Count > 3)

            BasicInfoPanel.Elements[2].Remove();

        BasicInfoPanel.Append(yesButton);
        yesButton.OnLeftClick += (evt, elem) =>
        {
            if (info.FileName == "")
            {
                Main.NewText(Language.GetOrRegister(localizationPath + ".EmptyFileNameException").Value, Color.Red);
                return;
            }
            if (info.DisplayName == "")
            {
                Main.NewText(Language.GetOrRegister(localizationPath + ".EmptyDisplayNameException").Value, Color.Red);
                return;
            }
            if (info.FileName == Sequence.SequenceDefaultName || currentSequences.ContainsKey(info.KeyName))
            {
                Main.NewText(Language.GetOrRegister(localizationPath + ".InvalidFileNameException").Value, Color.Red);
                return;
            }
            if (info.ModName == null || !ModLoader.TryGetMod(info.ModName, out _))
            {
                Main.NewText(Language.GetOrRegister(localizationPath + ".UnknownModException").Value, Color.Red);
                return;
            }
            pendingModify = false;
            yesButton.Remove();
            BasicInfoPanel.Remove();
            Main.NewText(Language.GetOrRegister(localizationPath + ".SaveSucceed").Value, Color.LimeGreen);
            var dnames = from i in SequenceSystem.sequenceInfos.Values select i.DisplayName;
            if (dnames.Contains(info.DisplayName))
            {
                Main.NewText(Language.GetOrRegister(localizationPath + ".RepeatedNameHint").Value, Color.Orange);
            }
            //SequenceSystem.instance.sequenceUI.currentSequences[info.KeyName] = sequence;
            var dict = typeof(SequenceManager<>).MakeGenericType(CurrentSelectedType).GetField("sequences", BindingFlags.Static | BindingFlags.Public).GetValue(null) as IDictionary;
            dict[info.KeyName] = sequence;
            SequenceSystem.sequenceInfos[info.KeyName] = info;
            var box = new SequenceBox(sequence);
            sequence.SyncInfo(info);
            sequence.Save();



            SequenceToPage(box);
            SwitchToSequencePage(box);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                SyncSingleSequence.Get(Main.myPlayer, sequence, CurrentSelectedType).Send();
            }
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
        hintText = "";
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
                if (saveButton != null)
                    WorkingPlacePanel.Append(saveButton);
                if (saveAsButton != null)

                    WorkingPlacePanel.Append(saveAsButton);
                if (revertButton != null)
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
    public override void DrawSelf(SpriteBatch spriteBatch)
    {

        base.DrawSelf(spriteBatch);
        if (hintText != string.Empty)
            UICommon.TooltipMouseText(hintText);
    }
}

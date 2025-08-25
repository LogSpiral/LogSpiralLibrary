using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.UIBase;
using LogSpiralLibrary.UIBase.InsertablePanel;
using LogSpiralLibrary.UIBase.SequenceEditUI;
using LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;
using Microsoft.Xna.Framework.Input;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Panel.Fillers;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.BasicComponents;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Graphics2D;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.Localization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using LogSpiralLibrary.CodeLibrary.Utilties;

namespace LogSpiralLibrary.UI.SequenceEditUI;

[RegisterUI("Vanilla: Radial Hotbars", $"{nameof(LogSpiralLibrary)}: {nameof(SequenceEditUI)}")]
public partial class SequenceEditUI : BasicBody
{
    #region 属性

    public static SequenceEditUI Instance { get; set; }
    public static bool Active { get; set; }
    public AnimationTimer SwitchTimer { get; init; } = new(3);

    public override bool Enabled
    {
        get => Active || !SwitchTimer.IsReverseCompleted;
        set => Active = value;
    }

    #endregion 属性

    #region 辅助字段

    private bool _pendingUpdateMenu = true;
    private bool _pendingUpdateElementLib = true;
    private bool _pendingUpdateSequenceLib = true;
#nullable enable
    private string? _currentPageFullName;

    private Dictionary<string, PageView> OpenedPages = [];
#nullable disable

    #endregion 辅助字段

    #region 初始化 开启关闭

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Instance = this;
        Border = 0;
        BorderColor = default;
        BackgroundColor = default;
    }

    private void InitializeComponentExtra()
    {
        #region 主容器颜色等设置

        MainContainer.RectangleRender.ShadowColor = Color.Black * .1f;
        MainContainer.RectangleRender.ShadowSize = 12f;
        MainContainer.BackgroundColor = CurrentCategory?.ThemeColor ?? SUIColor.Background * .25f;
        MainContainer.BorderColor = SUIColor.Border;

        #endregion 主容器颜色等设置

        #region 压缩条目标设置

        DataLine.TargetView = DataPanel;
        LibraryLine.TargetView = LibraryPanel;

        #endregion 压缩条目标设置

        #region 文本标题

        static string GetText(string suffix) => Language.GetTextValue($"Mods.{nameof(LogSpiralLibrary)}.SequenceUI.{suffix}");
        PanelDataText.Text = GetText("InfoTitle");
        PanelConfigText.Text = GetText("PropTitle");
        ElementLibraryText.Text = GetText("ActionLibrary");
        SequenceLibraryText.Text = GetText("SequenceLibrary");

        #endregion 文本标题

        #region 图标按钮

        SequenceTypeIcon.Texture2D = CurrentCategory?.Icon ?? TextureAssets.Item[ItemID.WireKite]; // TODO 注册序列类型，添加更多图标
        HomePageIcon.Texture2D = ModAsset.Rose;
        OpenFolderIcon.Texture2D = ModAsset.Folder;
        HelperIcon.Texture2D = ModAsset.Helper;
        SequenceTypeIcon.OnUpdateStatus += delegate
        {
            SequenceTypeIcon.ImageColor = Color.White * SequenceTypeIcon.HoverTimer.Lerp(0.5f, 1f);
        };
        HomePageIcon.OnUpdateStatus += delegate
        {
            HomePageIcon.ImageColor = Color.White * HomePageIcon.HoverTimer.Lerp(0.5f, 1f);
        };
        OpenFolderIcon.OnUpdateStatus += delegate
        {
            OpenFolderIcon.ImageColor = Color.White * OpenFolderIcon.HoverTimer.Lerp(0.5f, 1f);
        };
        HelperIcon.OnUpdateStatus += delegate
        {
            HelperIcon.ImageColor = Color.White * HelperIcon.HoverTimer.Lerp(0.5f, 1f);
        };

        SequenceTypeIcon.LeftMouseClick += delegate
        {
            CurrentCategory = null;
        };
        HomePageIcon.LeftMouseClick += delegate
        {
            Utils.OpenToURL("https://space.bilibili.com/259264134");
            SoundEngine.PlaySound(SoundID.MenuOpen);
        };

        OpenFolderIcon.LeftMouseClick += delegate
        {
            string path = SequenceSystem.SequenceSavePath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Utils.OpenFolder(path);
        };
        HelperIcon.LeftMouseClick += delegate
        {
            if (SequenceEditHelperUI.Active)
                SequenceEditHelperUI.Close();
            else
                SequenceEditHelperUI.Open();
        };

        #endregion 图标按钮

        #region 页面栏

        PagePanel.ControlTarget = this;
        CreateNewButton.Texture2D = ModAsset.Plus;
        MainPageButton.Texture2D = ModAsset.Menu;
        MainPageButton.LeftMouseClick += delegate { SwitchToMenu(); };
        MainPageButton.OnUpdateStatus += delegate { HoverCommon(MainPageButton); };
        PagePanel.OnUpdateStatus += delegate
        {
            var list = PagePanel.PendingChildren;
            int count = list.Count;
            for (int n = 0; n < count; n += 2)
            {
                var curr = list[n];
                if (curr.HoverTimer.IsCompleted || (curr is PageView cPage && cPage.NameIndex == _currentPageFullName)) continue;
                float factor = 1 - curr.HoverTimer.Schedule;
                if (n > 0)
                {
                    if (list[n - 2] is not PageView page || page.NameIndex != _currentPageFullName)
                        list[n - 1].BackgroundColor = Color.Black * (.25f * factor);
                }
                if (n < count - 1)
                {
                    if (list[n + 2] is not PageView page || page.NameIndex != _currentPageFullName)
                        list[n + 1].BackgroundColor = Color.Black * (.25f * factor);
                }
            }
        };
        /*
        TestPage1.OnUpdateStatus += delegate { HoverCommon(TestPage1); };
        TestPage1.LeftMouseClick += delegate { SwitchToEdit(); };

        TestPage2.OnUpdateStatus += delegate { HoverCommon(TestPage2); };*/
        CreateNewButton.OnUpdateStatus += delegate { HoverCommon(CreateNewButton); };

        #endregion 页面栏

        #region 元素库 序列库设置
        ElementLibrary.ScrollBar.SetHeight(-16, 1);
        SequenceLibrary.ScrollBar.SetHeight(-16, 1);
        #endregion

        SwitchToMenu();
    }



    private void SwitchToMenu()
    {
        if (_currentPageFullName != null && OpenedPages.TryGetValue(_currentPageFullName, out var previous))
        {
            previous.BackgroundColor = default;
            var prevIndex = PagePanel.GetInnerChildIndex(previous);
            PagePanel.PendingChildren[prevIndex - 1].BackgroundColor = Color.Black * .25f;
            PagePanel.PendingChildren[prevIndex + 1].BackgroundColor = Color.Black * .25f;
        }
        _currentPageFullName = null;
        MenuPanel ??= new();
        EditPanel.Remove();
        MainContainer.Add(MenuPanel, 1);
        SetupMenu();
        // 防止预览过程中退出导致死锁
        InsertablePanel.ForceEnablePV();
    }

    private void SetupMenu()
    {
        if (!_pendingUpdateMenu) return;

        MenuPanel.RecentList.Container.RemoveAllChildren();
        MenuPanel.FavoriteList.Container.RemoveAllChildren();
        MenuPanel.FinishedList.Container.RemoveAllChildren();
        MenuPanel.LibraryList.Container.RemoveAllChildren();

        // _pendingUpdateMenu = false;


        KeyValuePair<UIView, IReadOnlyList<KeyValuePair<string, string>>> SpawnContentFromPair(KeyValuePair<string, Sequence> pair)
        {
            var sequence = pair.Value;
            var name = pair.Key;
            var pathArray = name.Split("/");
            var modName = pathArray[0];
            var mask = SequenceEditUIHelper.NewDownlistMask();
            var textView = new UITextView();
            textView.Text = sequence.Data?.DisplayName ?? name;
            textView.IgnoreMouseInteraction = true;
            textView.SetTop(0, 0, .5f);
            textView.TextAlign = new(0.5f, 10.5f);
            textView.Join(mask);
            void AppendPage(bool immediateSwitch)
            {
                var fullName = name;
                if (!OpenedPages.TryGetValue(name, out var page))
                {
                    page = new()
                    {
                        TitleText = textView.Text,
                        NameIndex = name
                    };
                    var vr = new VerticalRule() { Height = new(-16, 0.8f), Top = new(-8f, 0, 1), BackgroundColor = Color.Black * .25f };
                    PagePanel.AddBefore(page, CreateNewButton);
                    PagePanel.AddBefore(vr, CreateNewButton);

                    page.CloseButton.LeftMouseClick += delegate
                    {
                        page.Remove();
                        vr.Remove();
                        OpenedPages.Remove(name);
                        if (_currentPageFullName == name)
                            SwitchToMenu();
                    };
                    page.LeftMouseClick += (source, evt) =>
                    {
                        if (evt.Source != source) return;
                        if (_currentPageFullName != null && OpenedPages.TryGetValue(_currentPageFullName, out var previous))
                        {
                            previous.BackgroundColor = default;
                            var prevIndex = PagePanel.GetInnerChildIndex(previous);
                            PagePanel.PendingChildren[prevIndex - 1].BackgroundColor = Color.Black * .25f;
                            PagePanel.PendingChildren[prevIndex + 1].BackgroundColor = Color.Black * .25f;
                        }
                        _currentPageFullName = name;
                        SwitchToEdit();
                        page.BackgroundColor = Color.Black * .135f;
                        var index = PagePanel.GetInnerChildIndex(page);
                        PagePanel.PendingChildren[index - 1].BackgroundColor = default;
                        PagePanel.PendingChildren[index + 1].BackgroundColor = default;
                        // TODO 填充页面内容
                    };
                    page.OnUpdateStatus += delegate
                    {
                        if (_currentPageFullName != name)
                            SequenceEditUIHelper.HoverCommon(page);
                    };
                    OpenedPages[name] = page;
                    if (immediateSwitch)
                    {
                        _currentPageFullName = name;
                        SwitchToEdit();
                        page.BackgroundColor = Color.Black * .135f;
                        var index = PagePanel.GetInnerChildIndex(page);
                        PagePanel.PendingChildren[index - 1].BackgroundColor = default;
                        PagePanel.PendingChildren[index + 1].BackgroundColor = default;
                    }
                }
                else
                {
                    _currentPageFullName = name;
                    SwitchToEdit();
                    page.BackgroundColor = Color.Black * .135f;
                    var index = PagePanel.GetInnerChildIndex(page);
                    PagePanel.PendingChildren[index - 1].BackgroundColor = default;
                    PagePanel.PendingChildren[index + 1].BackgroundColor = default;
                }
            }
            mask.LeftMouseClick += delegate
            {
                AppendPage(false);
            };
            mask.RightMouseClick += delegate
            {
                AppendPage(true);
            };

            List<KeyValuePair<string, string>> path = [new(modName, ModLoader.GetMod(modName).DisplayName)];
            int c = pathArray.Length;
            for (int n = 1; n < c - 1; n++)
                path.Add(new(pathArray[n], pathArray[n]));
            return new(mask, path);

        }
        SUIFolder.BuildFoldersToTarget(MenuPanel.FinishedList.Container, from pair in CurrentCategory.Maganger.Sequences where pair.Value.Data.Finished select SpawnContentFromPair(pair));
        SUIFolder.BuildFoldersToTarget(MenuPanel.LibraryList.Container, from pair in CurrentCategory.Maganger.Sequences where !pair.Value.Data.Finished select SpawnContentFromPair(pair));
    }

    public void SetupElementLib()
    {
        if (!_pendingUpdateElementLib) return;

        ElementLibrary.Container.RemoveAllChildren();

        KeyValuePair<UIView, IReadOnlyList<KeyValuePair<string, string>>> SpawnContentFromPair(KeyValuePair<string, Type> pair)
        {
            var name = pair.Key;
            var type = pair.Value;
            var strs = name.Split("/");
            var modName = strs[0];
            var dummy = ModContent.Find<MeleeAction>(name);
            InsertablePanelFactory factory = new()
            {
                PanelFactory = delegate
                {
                    var panel = InsertablePanelUtils.ElementTypeToPanel(type);
                    panel.BaseView = BasePanel;
                    panel.Mask = Mask;
                    return panel;
                }
            };
            var mask = SequenceEditUIHelper.NewDownlistMask();
            mask.Padding = new(0);
            factory.Join(mask);
            string categoryTitleName = dummy.Category;
            string key = $"Mods.{dummy.Mod.Name}.{CurrentCategory.ElementName}.Category.{dummy.Category}";
            if (Language.Exists(key))
                categoryTitleName = Language.GetTextValue(key);
            List<KeyValuePair<string, string>> path;
            if (!string.IsNullOrEmpty(dummy.Category))
                path =
                   [
                        new(modName,ModLoader.GetMod(modName).DisplayName),
                        new(dummy.Category,categoryTitleName)
                   ];
            else
                path =
                    [
                        new(modName,ModLoader.GetMod(modName).DisplayName)
                    ];

            return new(mask, path);
        }
        SUIFolder.BuildFoldersToTarget(ElementLibrary.Container, from pair in CurrentCategory.Maganger.ElementTypeLookup select SpawnContentFromPair(pair));
    }

    private void SetupSequenceLib()
    {
        if (!_pendingUpdateElementLib) return;

        SequenceLibrary.Container.RemoveAllChildren();

        // _pendingUpdateElementLib = false;
        KeyValuePair<UIView, IReadOnlyList<KeyValuePair<string, string>>> SpawnContentFromPair(KeyValuePair<string, Sequence> pair)
        {
            var name = pair.Key;
            var type = pair.Value;
            var strs = name.Split("/");
            var modName = strs[0];
            InsertablePanelFactory factory = new()
            {
                PanelFactory = delegate
                {
                    var panel = InsertablePanelUtils.SequenceRefKeyToPanel(name);
                    panel.BaseView = BasePanel;
                    panel.Mask = Mask;
                    return panel;
                }
            };
            var mask = SequenceEditUIHelper.NewDownlistMask();
            mask.Padding = new(0);
            factory.Join(mask);
            List<KeyValuePair<string, string>> path = [new(modName, ModLoader.GetMod(modName).DisplayName)];
            var length = strs.Length;
            for (int i = 1; i < length - 1; i++)
            {
                var folderName = strs[i];
                path.Add(new(folderName, folderName));
            }
            return new(mask, path);
        }
        SUIFolder.BuildFoldersToTarget(SequenceLibrary.Container, from pair in CurrentCategory.Maganger.Sequences select SpawnContentFromPair(pair));
    }

    private void SetupRootElement()
    {
        var sequenceName = _currentPageFullName;
        var sequence = SequenceGlobalManager.SequenceLookup[sequenceName];
        //PropertyPanelData.Filler = new DesignatedMemberFiller(
        //    [(sequence.Data,
        //    [nameof(SequenceData.FileName),
        //    nameof(SequenceData.DisplayName),
        //    nameof(SequenceData.AuthorName),
        //    nameof(SequenceData.Description),
        //    nameof(SequenceData.ModDefinition),
        //    nameof(SequenceData.Finished)]
        //    )]);
        PropertyPanelData.Filler = new ObjectMetaDataFiller(sequence.Data);
        var rootPanel = InsertablePanelUtils.SequenceToInsertablePanel(sequence);
        rootPanel.BaseView = BasePanel;
        rootPanel.Mask = Mask;
        BasePanel.RootElement = rootPanel;
        rootPanel.Join(BasePanel);
        rootPanel.SetLeft(BasePanel.Bounds.Width * .25f);
        rootPanel.SetTop(BasePanel.Bounds.Height * .25f);
        PropertyPanelConfig.Filler = new NoneFiller();
    }

    private void SwitchToEdit()
    {
        MenuPanel.Remove();
        MainContainer.Add(EditPanel, 1);
        BasePanel.RemoveAllChildren();
        // 防止预览过程中退出导致死锁
        InsertablePanel.ForceEnablePV();
        SetupElementLib();
        SetupSequenceLib();
        SetupRootElement();
    }

    private SequenceMenuPanel MenuPanel { get; set; }

    private void ReloadContent()
    {
        RemoveAllChildren();
        _contentLoaded = false;
        InitializeComponent();
        InitializeComponentExtra();
    }

    public static void Open()
    {
        // 为了方便测试用
        Instance?.ReloadContent();
        if (!Active)
            SoundEngine.PlaySound(SoundID.MenuOpen);
        Active = true;
    }

    public static void Close(bool silent = false)
    {
        if (Active && !silent)
            SoundEngine.PlaySound(SoundID.MenuClose);
        Active = false;
        Instance.OpenedPages.Clear();
    }

    #endregion 初始化 开启关闭

    #region 开启UI的淡入淡出等

    protected override void UpdateStatus(GameTime gameTime)
    {
        if (Active) SwitchTimer.StartUpdate();
        else SwitchTimer.StartReverseUpdate();

        SwitchTimer.Update(gameTime);

        UseRenderTarget = SwitchTimer.IsUpdating;
        Opacity = SwitchTimer.Lerp(0f, 1f);

        var center = Bounds.Center * Main.UIScale;
        RenderTargetMatrix =
            Matrix.CreateTranslation(-center.X, -center.Y, 0) *
            Matrix.CreateScale(SwitchTimer.Lerp(0.95f, 1f), SwitchTimer.Lerp(0.95f, 1f), 1) *
            Matrix.CreateTranslation(center.X, center.Y, 0);
        base.UpdateStatus(gameTime);
    }

    #endregion 开启UI的淡入淡出等

    #region 毛玻璃效果

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (BlurMakeSystem.BlurAvailable)
        {
            if (BlurMakeSystem.SingleBlur)
            {
                var batch = Main.spriteBatch;
                batch.End();
                BlurMakeSystem.KawaseBlur();
                batch.Begin();
            }

            SDFRectangle.SampleVersion(BlurMakeSystem.BlurRenderTarget,
                MainContainer.Bounds.Position * Main.UIScale, MainContainer.Bounds.Size * Main.UIScale, MainContainer.BorderRadius * Main.UIScale, Matrix.Identity);
        }

        base.Draw(gameTime, spriteBatch);
    }

    #endregion 毛玻璃效果

    #region 拖动效果

    private Vector2 _dragOffset;
    private bool _fixSize;
    private bool _dragging;

    public override void OnLeftMouseDown(UIMouseEvent evt)
    {
        if (evt.Source == this)
        {
            var bounds = Bounds;
            var coord = (Main.MouseScreen - bounds.Position) / (Vector2)bounds.Size;
            _fixSize = Vector2.Dot(coord, Vector2.One) < 1;
            _dragging = true;
            _dragOffset = Main.MouseScreen - (_fixSize ? new Vector2(Left.Pixels, Top.Pixels) : new Vector2(Width.Pixels, Height.Pixels));
        }
        base.OnLeftMouseDown(evt);
    }

    public override void OnLeftMouseUp(UIMouseEvent evt)
    {
        _dragging = false;
        base.OnLeftMouseUp(evt);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (_dragging)
        {
            var vec = Main.MouseScreen - _dragOffset;
            if (_fixSize)
            {
                SetLeft(vec.X);
                SetTop(vec.Y);
            }
            else
            {
                SetSize(
                    MathF.Max(vec.X, MinWidth.Pixels - Main.screenWidth),
                    MathF.Max(vec.Y, MinHeight.Pixels - Main.screenHeight)
                    );
            }
        }
    }

    #endregion 拖动效果
}

public class SequenceUIPlayer : ModPlayer
{
    private static ModKeybind OpenSequenceEditor { get; set; }

    public override void Load()
    {
        OpenSequenceEditor = KeybindLoader.RegisterKeybind(Mod, nameof(OpenSequenceEditor), Keys.Y);
        base.Load();
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (OpenSequenceEditor.JustPressed)
        {
            if (SequenceEditUI.CurrentCategory == null)
            {
                if (SequenceTypeSelectUI.Active)
                    SequenceTypeSelectUI.Close();
                else
                    SequenceTypeSelectUI.Open();
            }
            else
            {
                if (SequenceEditUI.Active)
                    SequenceEditUI.Close();
                else
                    SequenceEditUI.Open();
            }
        }
        base.ProcessTriggers(triggersSet);
    }
}
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Helpers;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;
using LogSpiralLibrary.UIBase.SequenceEditUI;
using LogSpiralLibrary.UIBase.SequenceEditUI.InsertablePanelSupport;
using SilkyUIFramework;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using System.IO;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core.Definition;
using Terraria.Audio;

namespace LogSpiralLibrary.UI.SequenceEditUI;

public partial class SequenceEditUI
{
    void InitializeBody()
    {
        Instance = this;
        Border = 0;
        BorderColor = default;
        BackgroundColor = default;
        BlackMask = new UIView
        {
            Width = new Dimension(0, 1),
            Height = new Dimension(0, 1),
            Positioning = Positioning.Absolute
        };

        if (InvalidPathChars.Count == 0)
            foreach (var c in Path.GetInvalidPathChars())
                InvalidPathChars.Add(c);
    }
    protected override void OnInitialize()
    {
        base.OnInitialize();

        InitializeBody();
        Instance.OpenedPanels.Clear();
        Instance.PendingPanels.Clear();
        Instance.OpenedSequences.Clear();
        Instance.PendingSequences.Clear();
    }

    private void InitializeMainContainer()
    {
        MainContainer.RectangleRender.ShadowColor = Color.Black * .1f;
        MainContainer.RectangleRender.ShadowSize = 12f;
        MainContainer.BackgroundColor = CurrentCategory?.ThemeColor ?? SUIColor.Background * .25f;
        MainContainer.BorderColor = SUIColor.Border;
    }

    private void InitializeCompressLine()
    {
        DataLine.TargetView = DataPanel;
        LibraryLine.TargetView = LibraryPanel;

        DataLine.OnStartCompress += delegate
        {
            BasePanel.FixedTarget = BasePanel.RootElement;
            BasePanel.RecoverAfterFixing = false;
        };
        DataLine.OnEndCompress += delegate
        {
            BasePanel.FixedTarget = null;
        };
    }

    private void InitializeTitle()
    {
        PanelDataText.Text = SequenceEditUIHelper.GetText("InfoTitle");
        PanelConfigText.Text = SequenceEditUIHelper.GetText("PropTitle");
        ElementLibraryText.Text = SequenceEditUIHelper.GetText("ActionLibrary");
        SequenceLibraryText.Text = SequenceEditUIHelper.GetText("SequenceLibrary");
    }

    private void InitializeIconVisuals()
    {
        SequenceTypeIcon.Texture2D = CurrentCategory?.Icon ?? TextureAssets.Item[ItemID.WireKite]; // TODO 注册序列类型，添加更多图标
        HomePageIcon.Texture2D = ModAsset.Rose;
        OpenFolderIcon.Texture2D = ModAsset.Folder;
        HelperIcon.Texture2D = ModAsset.Helper;
        ReloadIcon.Texture2D = ModAsset.Reload;
        SequenceTypeIcon.OnUpdateStatus += delegate
        {
            SequenceTypeIcon.ImageColor = Color.White * SequenceTypeIcon.HoverTimer.Lerp(.5f, 1f);
        };
        HomePageIcon.OnUpdateStatus += delegate
        {
            HomePageIcon.ImageColor = Color.White * HomePageIcon.HoverTimer.Lerp(.5f, 1f);
        };
        OpenFolderIcon.OnUpdateStatus += delegate
        {
            OpenFolderIcon.ImageColor = Color.White * OpenFolderIcon.HoverTimer.Lerp(.5f, 1f);
        };
        HelperIcon.OnUpdateStatus += delegate
        {
            HelperIcon.ImageColor = Color.White * HelperIcon.HoverTimer.Lerp(.5f, 1f);
        };
        ReloadIcon.OnUpdateStatus += delegate
        {
            ReloadIcon.ImageColor = Color.White * ReloadIcon.HoverTimer.Lerp(.5f, 1f);
        };
    }

    private void InitializeIconFunction()
    {
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
        ReloadIcon.LeftMouseClick += delegate
        {
            if (OpenedPages.Count > 0)
            {
                Main.NewText(SequenceEditUIHelper.GetText("ReloadInMenuOnly"), Color.Red);
                return;
            }
            CurrentCategory.Maganger.ReloadSequences();
            SwitchToMenu();
            Main.NewText(SequenceEditUIHelper.GetText("ReloadSucceed"), Color.Green);
        };
    }

    private void InitializePagePanel()
    {
        PagePanel.ControlTarget = this;
        CreateNewButton.Texture2D = ModAsset.Plus;
        MainPageButton.Texture2D = ModAsset.Menu;
        MainPageButton.LeftMouseClick += delegate { SwitchToMenu(); };
        MainPageButton.OnUpdateStatus += delegate { SequenceEditUIHelper.HoverColor(MainPageButton, default, Color.White * .1f); };
        PagePanel.OnUpdateStatus += delegate
        {
            var list = PagePanel.Children;
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
        CreateNewButton.LeftMouseClick += delegate
        {
            SequenceCreateNewUI.SequenceData =
            new CodeLibrary.DataStructures.SequenceStructures.Core.SequenceData()
            {
                ModDefinition = new ModDefinition(nameof(LogSpiralLibrary)),
                CreateTime = DateTime.Now,
                ModifyTime = DateTime.Now
            };
            SequenceCreateNewUI.IsFromSaveAs = false;
            SequenceCreateNewUI.Open();
        };
        CreateNewButton.OnUpdateStatus += delegate { SequenceEditUIHelper.HoverColor(CreateNewButton, default, Color.White * .1f); };
    }

    private void InitializeLibrary()
    {
        ElementLibrary.ScrollBar.SetHeight(-16, 1);
        SequenceLibrary.ScrollBar.SetHeight(-16, 1);
    }

    private void InitializeEditButtons()
    {
        SaveButton = SequenceEditUIHelper.FastIconTextButton(ModAsset.Save_Transparent_Premultiplied, "Save");
        RevertButton = SequenceEditUIHelper.FastIconTextButton(ModAsset.Revert_Transparent_Premultiplied, "Revert");
        SaveAsButton = SequenceEditUIHelper.FastIconTextButton(ModAsset.SaveAs_Transparent_Premultiplied, "SaveAs");
        EditButtonContainer = new UIElementGroup()
        {
            FitHeight = true,
            FitWidth = true,
            Gap = new Size(16)
        };
        EditButtonMask = new UIElementGroup()
        {
            Left = new Anchor(48),
            Top = new Anchor(-48, 0, 1),
            Positioning = Positioning.Absolute,
            OverflowHidden = true
        };
        EditButtonContainer.Join(EditButtonMask);
        SaveButton.Join(EditButtonContainer);
        RevertButton.Join(EditButtonContainer);
        SaveAsButton.Join(EditButtonContainer);

        SaveButton.LeftMouseClick += delegate
        {
            if (_currentPageFullName == null || CurrentPage is not { } page) return;
            if (PendingSequences.TryGetValue(_currentPageFullName, out var sequence) && PendingPanels.TryGetValue(_currentPageFullName, out var panel))
            {
                var loadingSequence = CurrentCategory.Maganger.Sequences[_currentPageFullName];
                bool renamed = loadingSequence.Data.GetSequenceKeyName(CurrentCategory.ElementName) != sequence.Data.GetSequenceKeyName(CurrentCategory.ElementName);

                var msg = SequenceEditUIHelper.GetText("SaveSucceed");
                if (renamed)
                {
                    // 发生重命名或者更换所属模组时删除原来的
                    if (!SequenceEditUIHelper.SequenceDataSaveCheck(sequence.Data, out msg))
                    {
                        Main.NewText(msg, Color.Red);
                        return;
                    }

                    var origPath = Path.Combine(SequenceSystem.SequenceSavePath, CurrentCategory.ElementName, loadingSequence.Data.ModDefinition.Name, $"{loadingSequence.Data.FileName}.xml");
                    if (File.Exists(origPath))
                        File.Delete(origPath);
                }
                loadingSequence.Data = sequence.Data;

                InsertPanelToSequenceUtils.RefillSequenceViaInsertablePanel(panel, loadingSequence);

                SequenceSaveHelper.SaveSequence(loadingSequence, CurrentCategory.ElementName);

                Main.NewText(msg, Color.Green);

                PendingSequences.Remove(_currentPageFullName);
                PendingPanels.Remove(_currentPageFullName);
            }
            page.PendingModified = false;
            OpenedSequences.Remove(_currentPageFullName);
            OpenedPanels.Remove(_currentPageFullName);
            SwitchToEdit();
        };

        RevertButton.LeftMouseClick += delegate
        {
            if (_currentPageFullName == null || CurrentPage is not { } page) return;
            page.PendingModified = false;
            PendingSequences.Remove(_currentPageFullName);
            OpenedSequences.Remove(_currentPageFullName);
            OpenedPanels.Remove(_currentPageFullName);
            PendingPanels.Remove(_currentPageFullName);
            SwitchToEdit();
        };

        SaveAsButton.LeftMouseClick += delegate
        {
            if (_currentPageFullName == null || CurrentPage is not { } page) return;
            SequenceCreateNewUI.SequenceData = OpenedSequences[_currentPageFullName].Data.Clone();
            SequenceCreateNewUI.SequenceData.CreateTime = DateTime.Now;
            SequenceCreateNewUI.SequenceData.ModifyTime = DateTime.Now;
            SequenceCreateNewUI.IsFromSaveAs = true;
            SequenceCreateNewUI.Open();
        };

        Mask.OnUpdateStatus += (gameTime) =>
        {
            if (EditButtonMask.Parent is null && CurrentPage is { PendingModified: true })
            {
                EditButtonMask.Join(Mask);
                _buttonContainerTimer.StartUpdate();
                return;
            }
            if (EditButtonMask.Parent != null && (CurrentPage == null || !CurrentPage.PendingModified))
            {
                _buttonContainerTimer.StartReverseUpdate();
                if (_buttonContainerTimer.IsReverseCompleted)
                {
                    EditButtonMask.Remove();
                    return;
                }
            }
            _buttonContainerTimer.Update(gameTime);
            var bounds = EditButtonContainer.Bounds;
            var factor = _buttonContainerTimer.Schedule;
            EditButtonMask.SetWidth(bounds.Width * factor);
            EditButtonMask.SetHeight(bounds.Height);
        };
    }

    private void InitializeComponentExtra()
    {
        InitializeMainContainer();

        InitializeCompressLine();

        InitializeTitle();

        InitializeIconVisuals();

        InitializeIconFunction();

        InitializePagePanel();

        InitializeLibrary();

        InitializeEditButtons();

        SwitchToMenu();
    }
}
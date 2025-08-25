using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;
using LogSpiralLibrary.UIBase.SequenceEditUI;
using SilkyUIFramework;
using System.IO;
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
    }
    protected override void OnInitialize()
    {
        base.OnInitialize();
        InitializeBody();
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
    }

    private void InitializePagePanel() 
    {
        PagePanel.ControlTarget = this;
        CreateNewButton.Texture2D = ModAsset.Plus;
        MainPageButton.Texture2D = ModAsset.Menu;
        MainPageButton.LeftMouseClick += delegate { SwitchToMenu(); };
        MainPageButton.OnUpdateStatus += delegate { SequenceEditUIHelper.HoverColor(MainPageButton,default,Color.White * .1f); };
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
        CreateNewButton.OnUpdateStatus += delegate { SequenceEditUIHelper.HoverColor(CreateNewButton, default, Color.White * .1f); };
    }

    private void InitializeLibrary() 
    {
        ElementLibrary.ScrollBar.SetHeight(-16, 1);
        SequenceLibrary.ScrollBar.SetHeight(-16, 1);
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

        SwitchToMenu();
    }

}

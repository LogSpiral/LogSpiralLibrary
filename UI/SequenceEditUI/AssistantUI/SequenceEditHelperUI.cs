using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Graphics2D;
using Terraria.Audio;
using Terraria.Localization;

namespace LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;

[RegisterUI("Vanilla: Mouse Text", $"{nameof(LogSpiralLibrary)}: {nameof(SequenceEditHelperUI)}")]
[JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
public partial class SequenceEditHelperUI : BasicBody
{
    #region 属性

    public static SequenceEditHelperUI Instance { get; set; }
    public static bool Active { get; set; }
    public AnimationTimer SwitchTimer { get; init; } = new(3);

    public override bool Enabled
    {
        get => Active || !SwitchTimer.IsReverseCompleted;
        set => Active = value;
    }

    #endregion 属性

    public static void SetHelpHintKey(string key) => Instance?.HelpHintKey = key;
    public string HelpHintKey
    {
        get;
        set
        {
            if (value != field && HintTextTitle != null)
            {
                var key = value ?? "HelpPanel";
                HintTextTitle.Text = Language.GetTextValue($"Mods.LogSpiralLibrary.SequenceUI.Help.{key}.DisplayName");
                HintTextContent.Text = Language.GetTextValue($"Mods.LogSpiralLibrary.SequenceUI.Help.{key}.Tooltip");
            }
            field = value;
        }
    }

    #region 初始化 开启关闭

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Instance = this;
    }

    private void InitializeComponentExtra()
    {
        RectangleRender.ShadowColor = Color.Black * .1f;
        RectangleRender.ShadowSize = 12f;
        BackgroundColor = SUIColor.Background * .25f;
        BorderColor = SUIColor.Border;
        CloseButton.CrossBorderColor = SUIColor.Border * 0.75f;
        CloseButton.CrossBackgroundColor = SUIColor.Warn * 0.75f;
        CloseButton.CrossBorderHoverColor = SUIColor.Highlight;
        CloseButton.CrossBackgroundHoverColor = SUIColor.Warn;
        TitlePanel.ControlTarget = this;
        Title.Text = Language.GetTextValue("Mods.LogSpiralLibrary.SequenceUI.Help.Help");
        Title.UseDeathText();
        CloseButton.LeftMouseClick += delegate
        {
            Close();
        };
        HintTextTitle.Text = Language.GetTextValue("Mods.LogSpiralLibrary.SequenceUI.Help.HelpPanel.DisplayName");
        HintTextContent.Text = Language.GetTextValue("Mods.LogSpiralLibrary.SequenceUI.Help.HelpPanel.Tooltip");
    }

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

    public static void Close()
    {
        if (Active)
            SoundEngine.PlaySound(SoundID.MenuClose);
        Active = false;
    }

    #endregion 初始化 开启关闭

    #region 开启UI的淡入淡出等

    protected override void UpdateStatus(GameTime gameTime)
    {
        HandleTextManually();


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
                Bounds.Position * Main.UIScale, Bounds.Size * Main.UIScale, BorderRadius * Main.UIScale, Matrix.Identity);
        }

        base.Draw(gameTime, spriteBatch);
    }

    #endregion 毛玻璃效果


    #region 手动显示提示文本
    static void HandleTextManually()
    {
        if (!SequenceEditUI.Active) return;
        Vector2 mousePosition = Main.MouseScreen;
        var instance = SequenceEditUI.Instance;
        if (instance.PagePanel.ContainsPoint(mousePosition))
        {
            if (instance.MainPageButton.ContainsPoint(mousePosition))
                SetHelpHintKey("MenuButton");
            else if (instance.CreateNewButton.ContainsPoint(mousePosition))
                SetHelpHintKey("CreateNewButton");
            else
                SetHelpHintKey("PagePanel");
        }
        else if (instance.ButtonPanel.ContainsPoint(mousePosition))
        {
            if (instance.SequenceTypeIcon.ContainsPoint(mousePosition))
                SetHelpHintKey("TypeSelect");
            else if (instance.OpenFolderIcon.ContainsPoint(mousePosition))
                SetHelpHintKey("OpenFolder");
            else if (instance.HomePageIcon.ContainsPoint(mousePosition))
                SetHelpHintKey("LSHomePage");
            else if (instance.HelperIcon.ContainsPoint(mousePosition))
                SetHelpHintKey("HelpPanelOpener");
            else if (instance.ReloadIcon.ContainsPoint(mousePosition))
                SetHelpHintKey("ReloadSequences");
            else
                SetHelpHintKey("ButtonPanel");
        }
        else if (instance.MenuPanel.ContainsPoint(mousePosition) && instance.CurrentPage is null)
        {
            if (instance.MenuPanel.RecentList.ContainsPoint(mousePosition))
                SetHelpHintKey("MenuRecent");
            else if (instance.MenuPanel.FavoriteList.ContainsPoint(mousePosition))
                SetHelpHintKey("MenuFavorite");
            else if (instance.MenuPanel.FinishedList.ContainsPoint(mousePosition))
                SetHelpHintKey("MenuFinished");
            else if (instance.MenuPanel.LibraryList.ContainsPoint(mousePosition))
                SetHelpHintKey("MenuLibrary");
            else
                SetHelpHintKey("Menu");
        }
        else if (instance.PropertyPanelData.ContainsPoint(mousePosition))
            SetHelpHintKey("PropertyPanelData");
        else if (instance.PropertyPanelConfig.ContainsPoint(mousePosition))
            SetHelpHintKey("PropertyPanelConfig");
        else if (instance.ElementLibrary.ContainsPoint(mousePosition))
            SetHelpHintKey("ElementLibrary");
        else if (instance.SequenceLibrary.ContainsPoint(mousePosition))
            SetHelpHintKey("SequenceLibrary");
        else if (instance.BasePanel.ContainsPoint(mousePosition))
            SetHelpHintKey("EditPanel");
        else
            SetHelpHintKey(null);

    }
    #endregion
}
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Graphics2D;
using Terraria.Audio;
using Terraria.Localization;

namespace LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
[RegisterUI("Vanilla: Mouse Text", $"{nameof(LogSpiralLibrary)}: {nameof(SequenceEditHelperUI)}")]
public partial class SequenceEditHelperUI : BaseBody
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
        static bool ContainsCheck(UIView view) => view.Parent != null && view.ContainsPoint(Main.MouseScreen);
        var instance = SequenceEditUI.Instance;
        if (ContainsCheck(instance.PagePanel))
        {
            if (ContainsCheck(instance.MainPageButton))
                SetHelpHintKey("MenuButton");
            else if (ContainsCheck(instance.CreateNewButton))
                SetHelpHintKey("CreateNewButton");
            else
                SetHelpHintKey("PagePanel");
        }
        else if (ContainsCheck(instance.ButtonPanel))
        {
            if (ContainsCheck(instance.SequenceTypeIcon))
                SetHelpHintKey("TypeSelect");
            else if (ContainsCheck(instance.OpenFolderIcon))
                SetHelpHintKey("OpenFolder");
            else if (ContainsCheck(instance.HomePageIcon))
                SetHelpHintKey("LSHomePage");
            else if (ContainsCheck(instance.HelperIcon))
                SetHelpHintKey("HelpPanelOpener");
            else if (ContainsCheck(instance.ReloadIcon))
                SetHelpHintKey("ReloadSequences");
            else
                SetHelpHintKey("ButtonPanel");
        }
        else if (ContainsCheck(instance.MenuPanel) && instance.CurrentPage is null)
        {
            if (ContainsCheck(instance.MenuPanel.RecentList))
                SetHelpHintKey("MenuRecent");
            else if (ContainsCheck(instance.MenuPanel.FavoriteList))
                SetHelpHintKey("MenuFavorite");
            else if (ContainsCheck(instance.MenuPanel.FinishedList))
                SetHelpHintKey("MenuFinished");
            else if (ContainsCheck(instance.MenuPanel.LibraryList))
                SetHelpHintKey("MenuLibrary");
            else
                SetHelpHintKey("Menu");
        }
        else if (ContainsCheck(instance.PropertyPanelData))
            SetHelpHintKey("PropertyPanelData");
        else if (ContainsCheck(instance.PropertyPanelConfig))
            SetHelpHintKey("PropertyPanelConfig");
        else if (ContainsCheck(instance.ElementLibrary))
            SetHelpHintKey("ElementLibrary");
        else if (ContainsCheck(instance.SequenceLibrary))
            SetHelpHintKey("SequenceLibrary");
        else if (ContainsCheck(instance.BasePanel))
            SetHelpHintKey("EditPanel");
        else
            SetHelpHintKey(null);

    }
    #endregion
}
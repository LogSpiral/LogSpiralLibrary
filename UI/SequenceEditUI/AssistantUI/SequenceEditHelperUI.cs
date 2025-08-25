using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Graphics2D;
using Terraria.Audio;

namespace LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;

[RegisterUI("Vanilla: Radial Hotbars", $"{nameof(LogSpiralLibrary)}: {nameof(SequenceEditHelperUI)}")]
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
        Title.Text = "帮助";
        Title.UseDeathText();
        CloseButton.LeftMouseClick += delegate
        {
            Close();
        };
        HintTextTitle.Text = "这是一个非常好的元素！";
        HintTextContent.Text = "这个元素能帮你干非常爽的编辑功能\n什么，你说我不知道说的哪个元素？\n我也不知道哦(\n因为这个槽氮的悬浮提示功能还没实装";
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
}
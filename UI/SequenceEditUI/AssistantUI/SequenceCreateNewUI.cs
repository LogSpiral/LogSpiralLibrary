using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using PropertyPanelLibrary.PropertyPanelComponents.BuiltInProcessors.Panel.Fillers;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Graphics2D;
using System.Linq;
using Terraria.Audio;
using Terraria.Localization;

namespace LogSpiralLibrary.UI.SequenceEditUI.AssistantUI;

// [JITWhenModsEnabled("SilkyUIFramework", "PropertyPanelLibrary")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
[RegisterUI("Vanilla: Mouse Text", $"{nameof(LogSpiralLibrary)}: {nameof(SequenceCreateNewUI)}")]
public partial class SequenceCreateNewUI : BasicBody
{
    #region 属性

    public static SequenceCreateNewUI Instance { get; set; }
    public static bool Active { get; set; }
    public AnimationTimer SwitchTimer { get; init; } = new(3);

    public override bool Enabled
    {
        get => Active || !SwitchTimer.IsReverseCompleted;
        set => Active = value;
    }

    #endregion 属性

    public UIElementGroup CreateNewButton { get; set; }
    public UIElementGroup CancelButton { get; set; }
    public static SequenceData SequenceData { get; set; }
    public static bool IsFromSaveAs { get; set; }

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
        TitlePanel.ControlTarget = this;
        Title.UseDeathText();

        if (IsFromSaveAs)
        {
            Title.Text = Language.GetTextValue("Mods.LogSpiralLibrary.SequenceUI.SaveAs");
            CreateNewButton = SequenceEditUI.SequenceEditUIHelper.FastIconTextButton(ModAsset.SaveAs_Transparent_Premultiplied, "SaveAs");
        }
        else
        {
            Title.Text = Language.GetTextValue("Mods.LogSpiralLibrary.SequenceUI.CreateNew");
            CreateNewButton = SequenceEditUI.SequenceEditUIHelper.FastIconTextButton(ModAsset.CreateNew_Transparent_Premultiplied, "CreateNew");
        }
        CancelButton = SequenceEditUI.SequenceEditUIHelper.FastIconTextButton(ModAsset.CancelCreate_Transparent_Premultiplied, "Cancel");

        CreateNewButton.LeftMouseClick += delegate
        {
            if (!SequenceEditUI.SequenceEditUIHelper.SequenceDataSaveCheck(SequenceData, out var msg))
            {
                Main.NewText(msg, Color.Red);
                return;
            }
            Main.NewText(msg, Color.Green);
            SequenceEditUI.Instance.AppendNewPageFromData(SequenceData, IsFromSaveAs);
            Close();
            // TODO 数据转发至主编辑界面
        };
        CancelButton.LeftMouseClick += delegate
        {
            Close();
        };
        CreateNewButton.Join(ButtonPanel);
        CancelButton.Join(ButtonPanel);
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

        var instance = SequenceEditUI.Instance;
        instance.BlackMaskTimer.StartUpdate();
        if (instance.BlackMask.Parent == null)
            instance.BlackMask.Join(instance);

        if (SequenceData != null)
            Instance.DataPanel.Filler = new ObjectMetaDataFiller(SequenceData);
    }

    public static void Close()
    {
        if (Active)
            SoundEngine.PlaySound(SoundID.MenuClose);
        Active = false;
        SequenceEditUI.Instance.BlackMaskTimer.StartReverseUpdate();
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
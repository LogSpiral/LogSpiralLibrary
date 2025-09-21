using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Graphics2D;
using Terraria.Audio;

namespace LogSpiralLibrary.UI.MIDIScoreSelector;

// [JITWhenModsEnabled("SilkyUIFramework")]
// [ExtendsFromMod(nameof(SilkyUIFramework))]
[RegisterUI("Vanilla: Radial Hotbars", $"{nameof(LogSpiralLibrary)}: {nameof(MIDIScoreSelectorUI)}")]
public partial class MIDIScoreSelectorUI : BaseBody
{
    #region 属性

    public static MIDIScoreSelectorUI Instance { get; set; }
    public static bool Active { get; set; }
    public AnimationTimer SwitchTimer { get; init; } = new(3);

    public override bool Enabled
    {
        get => Active || !SwitchTimer.IsReverseCompleted;
        set => Active = value;
    }

    public MIDIPlayer EditTarget { get; private set; }

    #endregion 属性

    #region 初始化 开启关闭

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Instance = this;
    }

    private void InitializeComponentExtra()
    {
        BackgroundColor = SUIColor.Background * .25f;
        BorderColor = SUIColor.Border;

        foreach (var pair in SequenceManager<NoteElement>.Instance.Sequences)
        {
            UIElementGroup container = new()
            {
                FitWidth = true,
                FitHeight = true,
                BorderRadius = new Vector4(8),
                Border = 1,
                BackgroundColor = Color.Black * .1f
            };
            UITextView title = new()
            {
                TextAlign = new Vector2(.5f),
                Text = pair.Value.Data.DisplayName,
                IgnoreMouseInteraction = true
            };
            title.Join(container);
            container.Join(ScoreList.Container);
            container.LeftMouseClick += delegate
            {
                EditTarget?.CurrentScore = pair.Value;
                Close();
            };
        }
    }

    private void ReloadContent()
    {
        RemoveAllChildren();
        _contentLoaded = false;
        InitializeComponent();
        InitializeComponentExtra();
    }

    public static void Open(MIDIPlayer editTarget)
    {
        Instance.EditTarget = editTarget;
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
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Extensions;
using LogSpiralLibrary.CodeLibrary.Utilties;

namespace LogSpiralLibrary.UIBase.InsertablePanel;

[XmlElementMapping(nameof(InsertablePanel))]
public class InsertablePanel : UIElementGroup
{
    #region Dragging

    private bool _isDragging = false;
    private Vector2 _draggingOffset;

    /// <summary>
    /// 遮罩面板，拖动时会临时进入，单纯用于过场
    /// </summary>
    public virtual UIElementGroup Mask { get; set; }

    #endregion Dragging

    #region CoreProps

    public InsertPanelDecoratorManager DecoratorManager
    {
        get;
        set
        {
            _pendingUpdateDecorate = true;
            field = value;
        }
    } = new();

    /// <summary>
    /// 主面板，包含根元素，用以查找目标
    /// </summary>
    public virtual InsertBasePanel BaseView { get; set; }

    /// <summary>
    /// 准备加入的目标面板
    /// </summary>
    public InsertablePanel TargetParent { get; private set; }

    /// <summary>
    /// 挂起的面板, 可能要加入
    /// </summary>
    public InsertablePanel PendingPanel
    {
        get;
        set
        {
            field = value;
            if (value != null)
            {
                var oldIsPv = _isPreviewing;
                _isPreviewing = true;
                _pvTimer.StartUpdate();
                if (!oldIsPv)
                {
                    _pvSize = value.Bounds.Size; // new(value.Width.Pixels, value.Height.Pixels);
                    _pvMargin = value.Margin;
                    RecordCachedData();
                }

                BaseView.FixedPoint = Bounds.Position;
                BaseView.FixedTarget = this;
            }
            else
                _pvTimer.StartReverseUpdate();
        }
    }

    /// <summary>
    /// 监听预览操作，对移除元素进行保护
    /// </summary>
    public static bool PreviewProtect { get; set; }

    #endregion CoreProps

    #region fields

    private bool _pendingUpdateDecorate;

    /// <summary>
    /// 外部预览容器，仅上下左右四个方向插入的时候会有
    /// </summary>
    protected readonly UIElementGroup _pvContainer;

    /// <summary>
    /// 内部预览
    /// </summary>
    protected readonly UIView _pvView;

    protected bool _isPreviewing;
    protected AnimationTimer _pvTimer = new(3);

    /// <summary>
    /// 缓存挂起面板大小
    /// </summary>
    protected Vector2 _pvSize;

    /// <summary>
    /// 缓存挂起面板外边距
    /// </summary>
    protected Margin _pvMargin;

    /// <summary>
    /// 预览状态
    /// <br>右左下上对应0123</br>
    /// <br>Group和Sequence中更大的值用作下标</br>
    /// </summary>
    protected int _pvState;

    public static bool IsPVExists { get; private set; }

    #endregion fields

    #region ctor

    public InsertablePanel()
    {
        _pvContainer = new()
        {
            FitHeight = true,
            FitWidth = true,
            BackgroundColor = Color.White * .1f,
            BorderColor = Color.White,
            BorderRadius = new(8f),
            Padding = new(8f),
            Margin = new(8f),
            Border = 1f,
            Gap = new(16),
            MainAlignment = MainAlignment.Center,
            CrossAlignment = CrossAlignment.Center
        };
        _pvView = new()
        {
            BackgroundColor = Color.White * .1f,
            BorderColor = Color.White,
            BorderRadius = new(8f),
            Border = 1f,
            Margin = new(8f),
            Padding = new(8f),
        };
        BackgroundColor = Color.Black * .1f;
        BorderColor = Color.Black;
        BorderRadius = new(8f);
        Margin = new(8f);
        Border = 1f;
    }

    #endregion ctor

    #region Editing

    public void StartDragging()
    {
        _isDragging = true;
        this.Join(Mask);
        var position = Bounds.Position;
        var origin = Mask.Bounds.Position;
        var pixelPosition = position - origin;
        _draggingOffset = Main.MouseScreen - pixelPosition + new Vector2(Margin.Left, Margin.Top);
        Positioning = Positioning.Absolute;
        SetLeft(pixelPosition.X, 0, 0);
        SetTop(pixelPosition.Y, 0, 0);
    }

    public override void OnLeftMouseDown(UIMouseEvent evt)
    {
        // 需要有父元素才可拖出

        if (Parent != null && Parent != BaseView && !IgnoreMouseInteraction)
        {
            _isDragging = true;
            Remove();
            StartDragging();
        }

        base.OnLeftMouseDown(evt);
    }

    public override void OnLeftMouseUp(UIMouseEvent evt)
    {
        if (_isDragging)
        {
            _isDragging = false;
            Remove();
            if (TargetParent != null)
            {
                BaseView.PendingRecoverPositionRejected = true;
                BaseView.FixPointOffset = default;
                TargetParent.Inserting(evt);
                TargetParent = null;
            }
        }
        base.OnLeftMouseUp(evt);
    }

    #endregion Editing

    #region Updating

    protected override void UpdateStatus(GameTime gameTime)
    {
        UpdateDecorate();
        base.UpdateStatus(gameTime);
        UpdateDragging();
        UpdatePreview(gameTime);
    }
    private void UpdateDecorate() 
    {
        if (!_pendingUpdateDecorate) return;
        _pendingUpdateDecorate = false;
        // TODO 也许要卸之前的装饰？但是目前应该没问题，先不管了
        DecoratorManager.Apply(this);
    }

    private void UpdateDragging()
    {
        if (!_isDragging) return;
        var pixelPosition = Main.MouseScreen - _draggingOffset;
        SetLeft(pixelPosition.X, 0, 0);
        SetTop(pixelPosition.Y, 0, 0);
        FindTargetParent();
    }

    private void FindTargetParent()
    {
        // 获取鼠标处可插入面板
        var cursoredElem = BaseView?.GetInsertablePanelAt(Main.MouseScreen);

        if (cursoredElem == TargetParent) return;
        if (cursoredElem != null && IsPVExists) return;

        TargetParent?.PendingPanel = null;

        TargetParent = cursoredElem;
        TargetParent?.PendingPanel = this;
    }

    private void UpdatePreview(GameTime gameTime)
    {
        if (!_isPreviewing) return;

        IsPVExists |= true;

        bool isJustStart = _pvTimer.Timer == 0;
        _pvTimer.Update(gameTime);

        // 如果结束预览就复原
        if (_pvTimer.IsReverseCompleted)
        {
            _isPreviewing = false;

            var parent = _pvContainer.Parent;
            PreviewProtect = true;
            parent?.AddBefore(this, _pvContainer);
            PreviewProtect = false;
            _pvContainer.RemoveAllChildren();
            _pvContainer.Remove();
            _pvView.Remove();
            BaseView.PVRoot = null;
            BaseView.FixedTarget = null;
            IsPVExists = false;
            return;
        }

        // 预览插值
        var factor = _pvTimer.Schedule;

        // 更新容器颜色
        var color = Color.White * factor;
        _pvContainer.BorderColor = color;
        _pvContainer.BackgroundColor = color * .1f;

        // 更新预览颜色
        _pvView.BorderColor = color;
        _pvView.BackgroundColor = color * (.1f + .05f * MathF.Cos(Main.GlobalTimeWrappedHourly * 6.28f));
        _pvView.Margin = _pvContainer.Margin;
        _pvView.Padding = _pvContainer.Padding;

        // 更新容器边距
        var margin = new Margin(8 * factor);
        _pvContainer.Margin = margin;
        _pvContainer.Padding = margin;

        // 更新预览边距
        _pvView.Margin = margin;
        _pvView.Padding = margin;

        _pvContainer.Border = factor;
        _pvContainer.Gap = new(16 * factor);
        _pvView.Border = factor;

        var oldState = _pvState;
        var state = UpdateInsertPreviewState();
        if (state != -1 && isJustStart)
            _pvState = state;
        Vector2 deltaSize = _pvSize - Bounds.Size;
        if (deltaSize.X < 0) deltaSize.X = 0;
        if (deltaSize.Y < 0) deltaSize.Y = 0;
        if (_pvState < 4)
        {
            bool initPV = Parent != _pvContainer;
            if (initPV)
            {
                var parent = Parent;
                parent.AddBefore(_pvContainer, this);
                SetLeft(0);
                SetTop(0);
            }

            if (oldState != _pvState || initPV)
            {
                _pvContainer.RemoveAllChildren();
                // 开启预览保护，防止自身因为编辑预览而从内部面板列表中移除
                PreviewProtect = true;
                if (_pvState % 2 == 0)
                {
                    this.Join(_pvContainer);
                    _pvView.Join(_pvContainer);
                }
                else
                {
                    _pvView.Join(_pvContainer);
                    this.Join(_pvContainer);
                }
                PreviewProtect = false;
                _pvContainer.FlexDirection = _pvState < 2 ? FlexDirection.Row : FlexDirection.Column;
            }

            if (BaseView.RootElement == this)
                BaseView.PVRoot = _pvContainer;
        }

        if (_pvState < 2)
        {
            _pvView.SetWidth(_pvSize.X * factor);
            _pvView.SetHeight(_pvSize.Y - deltaSize.Y * (1 - factor));
        }
        else if (_pvState < 4)
        {
            _pvView.SetWidth(_pvSize.X - deltaSize.X * (1 - factor));
            _pvView.SetHeight(_pvSize.Y * factor);
        }
        else
            HandlePreview();
    }

    #endregion Updating

    #region Virtuals
    protected virtual void RecordCachedData()
    {

    }
    protected virtual int UpdateInsertPreviewState()
    {
        var result = -1;

        var bounds = OuterBounds;
        var pos = Main.MouseScreen - bounds.Position;

        bool left = pos.X < Margin.Left;
        bool right = pos.X > bounds.Width - Margin.Right;
        bool up = pos.Y < Margin.Top;
        bool down = pos.Y > bounds.Height - Margin.Bottom;

        bool Xdir = left || right;
        bool Ydir = up || down;
        if (!Ydir)
        {
            if (right)
                result = 0;
            if (left)
                result = 1;
        }
        if (!Xdir)
        {
            if (down)
                result = 2;
            if (up)
                result = 3;
        }
        return result;

        //int state = 0;
        //var bounds = Bounds;
        //var coord = (Main.MouseScreen - bounds.Position) / (Vector2)bounds.Size - new Vector2(.5f);
        //if (coord.X > MathF.Abs(coord.Y))
        //    state = 0;
        //else if (coord.X <= -MathF.Abs(coord.Y))
        //    state = 1;
        //else if (coord.Y > MathF.Abs(coord.X))
        //    state = 2;
        //else if (coord.Y <= -MathF.Abs(coord.X))
        //    state = 3;
        //return state;
    }

    protected virtual void HandlePreview()
    {
    }

    public event Action<InsertablePanel, GroupPanel> OnAppendingToGroup;
    public event Action<InsertablePanel,SequencePanel> OnAppendingToSequence;

    protected virtual void Inserting(UIMouseEvent evt)
    {
        _pvTimer.ImmediateReverseCompleted();
        _pvView.Remove();

        if (_pvContainer.Parent is UIElementGroup parent && _pvState < 4)
        {
            MultiPanel multiPanel = _pvState < 2 ? new SequencePanel()
            {
                BaseView = BaseView,
                Mask = Mask
            } : new GroupPanel()
            {
                BaseView = BaseView,
                Mask = Mask
            };

            if (_pvState < 2)
                OnAppendingToSequence?.Invoke(this, multiPanel as SequencePanel);
            else
                OnAppendingToGroup?.Invoke(this, multiPanel as GroupPanel);

            if (parent.Parent is MultiPanel mPanel)
                mPanel.RemoveFromInnerListManually(this);
            if (_pvState % 2 == 0)
            {
                multiPanel.InsertContainerPanel.Add(this);
                multiPanel.InsertContainerPanel.Add(PendingPanel);
            }
            else
            {
                multiPanel.InsertContainerPanel.Add(PendingPanel);
                multiPanel.InsertContainerPanel.Add(this);
            }
            multiPanel.Left = _pvContainer.Left;
            multiPanel.Top = _pvContainer.Top;
            parent.AddBefore(multiPanel, _pvContainer);
            _pvContainer.Remove();

            if (BaseView.RootElement == this)
            {
                BaseView.RootElement = multiPanel;
                SetLeft(0);
                SetTop(0);
            }
        }
        PendingPanel.Positioning = Positioning.Relative;
        PendingPanel.SetLeft(0);
        PendingPanel.SetTop(0);
        PendingPanel = null;
    }

    public virtual InsertablePanel GetInnerInsertablePanelAt(Vector2 mousePosition)
    {
        if (Invalid || IgnoreMouseInteraction) return null;

        if (OuterBounds.Contains(mousePosition) && UpdateInsertPreviewState() != -1) return this;

        return null;
    }

    #endregion Virtuals

    public static void ForceEnablePV() => IsPVExists = false;
}

//  public class ElementPanel : InsertablePanel
//  {
//  }
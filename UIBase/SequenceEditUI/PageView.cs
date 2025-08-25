using LogSpiralLibrary.CodeLibrary.Utilties;
using ReLogic.Content;
using SilkyUIFramework;
using SilkyUIFramework.Attributes;

namespace LogSpiralLibrary.UIBase.SequenceEditUI;

[XmlElementMapping(nameof(PageView))]
public partial class PageView : UIElementGroup
{
    private static Asset<Effect> ColorPanelEffect { get; } = ModAsset.PageCorner;

    private struct PageCornerVertex(Vector2 pos, Vector2 coord) : IVertexType
    {
        private static readonly VertexDeclaration _vertexDeclaration = new(
        [
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        ]);

        public Vector2 Pos = pos;
        public Vector2 Coord = coord;
        public readonly VertexDeclaration VertexDeclaration => _vertexDeclaration;
    }

    private const float Radius = 12;
    private const float root2Over2 = 1.414213562373f / 2f;

    public PageView()
    {
        BorderRadius = new(Radius, Radius, 0, 0);
        InitializeComponent();
        CloseButton.Texture2D = ModAsset.Close;
        CloseButton.OnUpdateStatus += delegate
        {
            if (CloseButton.HoverTimer.IsCompleted) return;
            CloseButton.BackgroundColor = Color.Black * (CloseButton.HoverTimer.Schedule * .1f);
        };
    }

    public string TitleText
    {
        private get;
        set
        {
            Title.Text = value;
            field = value;
        }
    }

    public string NameIndex { get; set; }

    public bool PendingModified
    {
        get;
        set
        {
            field = value;
            if (value)
                Title.Text = $"*{TitleText}";
            else
                Title.Text = TitleText;
        }
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (BackgroundColor == default) return;
        base.Draw(gameTime, spriteBatch);

        DrawPageCorner(Bounds.RightBottom - Vector2.UnitY * Radius, BackgroundColor, Radius, 1);
        DrawPageCorner(Bounds.LeftBottom - Vector2.One * Radius, BackgroundColor, Radius, 0);
    }

    private static void DrawPageCorner(Vector2 position, Color color, float r, int pass)
    {
        PageCornerVertex[] vertexs = new PageCornerVertex[4];
        for (int n = 0; n < 4; n++)
        {
            Vector2 coord = new Vector2(n / 2, n % 2);
            vertexs[n] = new PageCornerVertex(position + new Vector2(r) * coord, coord);
        }
        Effect colorPanel = ColorPanelEffect.Value;
        colorPanel.Parameters["uColor"].SetValue(color.ToVector4());
        colorPanel.Parameters["uTransition"].SetValue(new Vector2(-root2Over2, root2Over2) / Main.UIScale / Radius);
        colorPanel.Parameters["uTransform"].SetValue(SDFGraphics.GetMatrix(true));
        colorPanel.CurrentTechnique.Passes[pass].Apply();
        Main.instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, [vertexs[0], vertexs[2], vertexs[1], vertexs[1], vertexs[2], vertexs[3]], 0, 2);
        Main.spriteBatch.spriteEffectPass.Apply();
    }
}
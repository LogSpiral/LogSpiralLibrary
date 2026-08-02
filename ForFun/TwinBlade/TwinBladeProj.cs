using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.Core;

namespace LogSpiralLibrary.ForFun.TwinBlade;

public partial class TwinBladeProj : MeleeSequenceProj
{
    public override bool LabeledAsCompleted => true;

    private static readonly AirDistortEffect distortEffect = new(3, 1.5f);

    private static readonly BloomEffect bloomEffect = new(0.05f, 0.25f, 1f, 2, true, 1, true);

    private const string CanvasName = nameof(LogSpiralLibrary) + ":" + nameof(TwinBladeProj);

    public override void Load()
    {
        RenderCanvasSystem.RegisterCanvasFactory(CanvasName, () => new RenderingCanvas([[distortEffect], [bloomEffect]]));
        base.Load();
    }
    public override void InitializeStandardInfo(StandardInfo standardInfo, VertexDrawStandardInfo vertexStandard)
    {
        standardInfo.standardColor = Color.White * .25f;
        standardInfo.frame = new(0, 0, 32, 36);
        vertexStandard.canvasName = CanvasName;
    }
}

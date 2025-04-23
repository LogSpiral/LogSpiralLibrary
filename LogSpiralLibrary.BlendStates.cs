using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary;

partial class LogSpiralLibraryMod
{
    public static BlendState AllOne;
    public static BlendState InverseColor;
    public static BlendState SoftAdditive;//from yiyang233
    static void InitializeBlendStates() 
    {
        AllOne = new BlendState();
        AllOne.Name = "BlendState.AllOne";
        AllOne.ColorDestinationBlend = AllOne.AlphaDestinationBlend = AllOne.ColorSourceBlend = AllOne.AlphaSourceBlend = Blend.One;

        InverseColor = new BlendState()
        {
            Name = "BlendState.InverseColor",
            ColorDestinationBlend = Blend.InverseSourceColor,
            ColorSourceBlend = Blend.InverseDestinationColor,
            AlphaDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.Zero
        };
        SoftAdditive = new BlendState()
        {
            Name = "BlendState.SoftAdditve",
            ColorDestinationBlend = Blend.One,
            ColorSourceBlend = Blend.InverseDestinationColor,
            AlphaDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.SourceAlpha
        };
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria.DataStructures;

namespace LogSpiralLibrary.ForFun.TwinBlade;

public class TwinBladePlayerVisual : ModPlayer
{
    public bool IsTwinBladeStorming { get; set; }
    public float Rotation { get; set; }
    public float HeadRotation { get; set; }
    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        drawInfo.rotationOrigin = new Microsoft.Xna.Framework.Vector2(10, 28);
        drawInfo.drawPlayer.headRotation = HeadRotation;
        if (IsTwinBladeStorming)
            drawInfo.rotation = Rotation;
    }
}

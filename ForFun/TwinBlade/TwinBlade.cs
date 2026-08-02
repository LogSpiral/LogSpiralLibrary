using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.Core;

namespace LogSpiralLibrary.ForFun.TwinBlade;

public class TwinBlade : MeleeSequenceItem<TwinBladeProj>
{
    public override void Load()
    {
        if (Main.dedServ)
            return;
        Main.instance.LoadItem(ItemID.Arkhalis);
        Main.instance.LoadItem(ItemID.Terragrim);
    }
    public override bool EnableRightClick => true;
}
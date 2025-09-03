namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.Core;

public class MIDIPlayerProjectile : ModProjectile
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.SparkleGuitar}";

    public override bool IsLoadingEnabled(Mod mod) => false;
}
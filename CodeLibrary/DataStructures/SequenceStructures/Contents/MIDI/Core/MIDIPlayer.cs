using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.UI.MIDIScoreSelector;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.MIDI.Core;

public class MIDIPlayer : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.SparkleGuitar}";

    public Sequence CurrentScore { get; set; }
    public SequenceModel SequenceModel { get; set; }
    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Purple;
        Item.width = Item.height = 55;
        Item.useTime = 4;
        Item.useAnimation = 4;
        Item.useStyle = ItemUseStyleID.Guitar;
        base.SetDefaults();
    }

    public override bool? UseItem(Player player)
    {
        if (CurrentScore == null || player.altFunctionUse == 2) return base.UseItem(player);

        if (player.itemAnimation == player.itemAnimationMax)
            SequenceModel = new(CurrentScore);


        if (SequenceModel == null) return base.UseItem(player);

        SequenceModel.Update();

        if (player.itemAnimation == 2 && !SequenceModel.IsCompleted && player.controlUseItem)
            player.itemAnimation++;

        return base.UseItem(player);
    }
    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 2)
        {
            if (MIDIScoreSelectorUI.Active)
                MIDIScoreSelectorUI.Close();
            else
                MIDIScoreSelectorUI.Open(this);
        }
        return base.CanUseItem(player);
    }
    public override bool AltFunctionUse(Player player) => true;
}
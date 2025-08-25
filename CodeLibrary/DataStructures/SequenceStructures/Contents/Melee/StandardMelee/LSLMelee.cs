namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;

public abstract class LSLMelee : MeleeAction
{
    public override string Category => "LsLibrary";

    protected static void ShootProjCall(Player plr, int dmg) => plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, dmg);
}
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;

public abstract class LSLMelee : MeleeAction
{
    public override string Category => "LsLibrary";

    protected static void ShootProjCall(Player plr, int dmg)
    {
        int type = plr.HeldItem.shoot;
        int origCount = plr.ownedProjectileCounts[type];
        plr.ownedProjectileCounts[type]++;
        if (origCount != 0 || ItemLoader.CanShoot(plr.HeldItem, plr))
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, dmg);
        plr.ownedProjectileCounts[type] = origCount;
    }
}
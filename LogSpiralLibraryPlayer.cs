namespace LogSpiralLibrary;
public class LogSpiralLibraryPlayer : ModPlayer
{
    public bool ultraFallEnable;
    public Vector2 targetedMousePosition;
    public float strengthOfShake;
    public override void ResetEffects()
    {
        ultraFallEnable = false;
        base.ResetEffects();
    }
    public override void PreUpdate()
    {
        if (ultraFallEnable)
            Player.maxFallSpeed = 214514;

        if (Main.myPlayer == Player.whoAmI)
        {
            targetedMousePosition = Main.MouseWorld;
            if ((int)Main.time % 10 == 0)
                SyncMousePosition.Get(Player.whoAmI, targetedMousePosition).Send();
        }
        base.PreUpdate();
    }
    public override void ModifyScreenPosition()
    {
        var set = LogSpiralLibraryMiscConfig.Instance.screenShakingSetting;
        if (set.Available)
        {
            strengthOfShake *= 0.6f;
            if (strengthOfShake < 0.025f) strengthOfShake = 0;
            Main.screenPosition += Main.rand.NextVector2Unit() * strengthOfShake * 48 * set.strength;
        }
    }
}
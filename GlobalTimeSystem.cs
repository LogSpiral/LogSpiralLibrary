namespace LogSpiralLibrary;
public class GlobalTimeSystem : ModSystem
{
    public static double GlobalTime { get; set; }
    public static double GlobalTimePaused { get; set; }
    public override void UpdateUI(GameTime gameTime)
    {
        GlobalTime++;
        if (!Main.gamePaused)
            GlobalTimePaused++;
    }
}

using NetSimplified.Syncing;
using NetSimplified;
namespace LogSpiralLibrary;
[AutoSync]
public class SyncMousePosition : NetModule
{
    int whoAmI;
    Vector2 pos;
    public static SyncMousePosition Get(int whoAmI, Vector2 position)
    {
        var result = NetModuleLoader.Get<SyncMousePosition>();
        result.pos = position;
        result.whoAmI = whoAmI;
        return result;
    }
    public override void Receive()
    {
        Main.player[whoAmI].GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition = pos;
        if (Main.dedServ)
        {
            Get(whoAmI, pos).Send(-1, whoAmI);
        }
    }
}

[AutoSync]
public class SyncPlayerPosition : NetModule
{
    int whoAmI;
    Vector2 pos;
    public static SyncPlayerPosition Get(int whoAmI, Vector2 position)
    {
        var result = NetModuleLoader.Get<SyncPlayerPosition>();
        result.pos = position;
        result.whoAmI = whoAmI;
        return result;
    }
    public override void Receive()
    {
        Main.player[whoAmI].position = pos;
        if (Main.dedServ)
        {
            Get(whoAmI, pos).Send(-1, whoAmI);
        }
    }
}

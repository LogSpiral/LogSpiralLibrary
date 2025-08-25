using NetSimplified;
using NetSimplified.Syncing;
using System.IO;

namespace LogSpiralLibrary;

[AutoSync]
public class SyncMousePosition : NetModule
{
    private int whoAmI;
    private Vector2 pos;

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
    private byte whoAmI;
    private Vector2 pos;

    public static SyncPlayerPosition Get(byte whoAmI, Vector2 position)
    {
        var result = NetModuleLoader.Get<SyncPlayerPosition>();
        result.pos = position;
        result.whoAmI = whoAmI;
        return result;
    }

    public static SyncPlayerPosition Get(int whoAmI, Vector2 position) => Get((byte)whoAmI, position);

    public override void Receive()
    {
        Main.player[whoAmI].position = pos;
        if (Main.dedServ)
        {
            Get(whoAmI, pos).Send(-1, whoAmI);
        }
    }
}

public class SyncPlayerVelocity : NetModule
{
    private byte whoAmI;
    private Vector2 velocity;

    public static SyncPlayerVelocity Get(byte whoAmI, Vector2 velocity)
    {
        var result = NetModuleLoader.Get<SyncPlayerVelocity>();
        result.velocity = velocity;
        result.whoAmI = whoAmI;
        return result;
    }

    public static SyncPlayerVelocity Get(int whoAmI, Vector2 velocity) => Get((byte)whoAmI, velocity);

    public override void Receive()
    {
        Main.player[whoAmI].velocity = velocity;
        if (Main.dedServ)
        {
            Get(whoAmI, velocity).Send(-1, whoAmI);
        }
    }

    public override void Send(ModPacket p)
    {
        p.Write(whoAmI);
        p.WriteVector2(velocity);
    }

    public override void Read(BinaryReader r)
    {
        whoAmI = r.ReadByte();
        velocity = r.ReadVector2();
    }
}
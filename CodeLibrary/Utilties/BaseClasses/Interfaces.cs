namespace LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;

public interface IChannelProj
{
    bool Charging { get; }
    bool Charged { get; }

    void OnCharging(bool left, bool right);

    void OnRelease(bool charged, bool left);
}

public interface IHammerProj
{
    //string HammerName { get; }
    Vector2 CollidingSize { get; }

    Vector2 CollidingCenter { get; }
    Vector2 DrawOrigin { get; }
    Texture2D projTex { get; }
    Vector2 projCenter { get; }
    Rectangle? frame { get; }
    Color color { get; }
    float Rotation { get; }
    Vector2 scale { get; }
    SpriteEffects flip { get; }
    (int X, int Y) FrameMax { get; }
    Player Player { get; }
}
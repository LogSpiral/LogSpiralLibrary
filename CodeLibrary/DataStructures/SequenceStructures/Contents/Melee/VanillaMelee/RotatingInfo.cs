using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 其实是天龙之怒
/// </summary>
public class RotatingInfo : VanillaMelee
{
    #region 重写属性

    public override float OffsetRotation => (float)LogSpiralLibraryMod.ModTime2 * 0.45f * (Flip ? -1 : 1);
    public override Vector2 OffsetOrigin => base.OffsetOrigin;
    public override bool Attacktive => true;

    #endregion 重写属性

    #region 重写函数

    public override void OnStartSingle()
    {
        Flip = Owner.direction != 1;
        ShootExtraProjectile();


        SoundEngine.PlaySound(StandardInfo.soundStyle);
        base.OnStartSingle();
    }

    public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        float origf = (float)GlobalTimeSystem.GlobalTimePaused;
        IEnumerable<CustomVertexInfo> result = [];
        GlobalTimeSystem.GlobalTimePaused -= 2f;
        for (int i = 9; i >= 0; i--)
        {
            GlobalTimeSystem.GlobalTimePaused += .2f;
            result = result.Concat(base.GetWeaponVertex(texture, (1f - i / 10f) * (i == 0 ? 1f : .5f)));
        }
        GlobalTimeSystem.GlobalTimePaused = origf;
        return [.. result];
    }

    #endregion 重写函数
}
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
    public override float offsetRotation => (float)LogSpiralLibraryMod.ModTime2 * 0.45f * (flip ? -1 : 1);
    public override Vector2 offsetOrigin => base.offsetOrigin;
    public override bool Attacktive => true;
    #endregion

    #region 重写函数
    public override void OnStartSingle()
    {
        flip = Owner.direction != 1;
        if (Owner is Player plr)
        {
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
        }
        SoundEngine.PlaySound(standardInfo.soundStyle);
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
    #endregion
}

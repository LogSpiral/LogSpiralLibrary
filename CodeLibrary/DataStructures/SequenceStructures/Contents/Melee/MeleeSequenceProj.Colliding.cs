using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

partial class MeleeSequenceProj
{
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (currentData == null) return;
        //target.life = target.lifeMax;
        var data = currentData.ModifyData;
        modifiers.SourceDamage *= data.actionOffsetDamage * currentData.offsetDamage;
        modifiers.Knockback *= data.actionOffsetKnockBack;
        var _crit = player.GetWeaponCrit(player.HeldItem);
        _crit += data.actionOffsetCritAdder;
        _crit = (int)(_crit * data.actionOffsetCritMultiplyer);
        if (Main.rand.Next(100) < _crit)
        {
            modifiers.SetCrit();
        }
        else
        {
            modifiers.DisableCrit();
        }
        target.immune[player.whoAmI] = 0;
        base.ModifyHitNPC(target, ref modifiers);
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        if (currentData == null) return;
        var data = currentData.ModifyData;
        modifiers.SourceDamage *= data.actionOffsetDamage * currentData.offsetDamage;
        modifiers.Knockback *= data.actionOffsetKnockBack;
        base.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        currentData?.OnHitEntity(target, hit.Damage, [hit, damageDone]);

        base.OnHitNPC(target, hit, damageDone);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        currentData?.OnHitEntity(target, info.Damage, [info]);
        //player.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake = Main.rand.NextFloat(0.85f, 1.15f);

        base.OnHitPlayer(target, info);
    }
}

using Terraria.Enums;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;

public partial class MeleeSequenceProj
{
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (CurrentElement == null) return;
        //target.life = target.lifeMax;
        var data = CurrentElement.ModifyData;
        modifiers.SourceDamage *= data.Damage * CurrentElement.OffsetDamage;
        modifiers.Knockback *= data.KnockBack;
        var _crit = Player.GetWeaponCrit(Player.HeldItem);
        _crit += data.CritAdder;
        _crit = (int)(_crit * data.CritMultiplyer);
        if (Main.rand.Next(100) < _crit)
        {
            modifiers.SetCrit();
        }
        else
        {
            modifiers.DisableCrit();
        }
        target.immune[Player.whoAmI] = 0;
        base.ModifyHitNPC(target, ref modifiers);
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        if (CurrentElement == null) return;
        var data = CurrentElement.ModifyData;
        modifiers.SourceDamage *= data.Damage * CurrentElement.OffsetDamage;
        modifiers.Knockback *= data.KnockBack;
        base.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        CurrentElement?.OnHitEntity(target, hit.Damage, [hit, damageDone]);

        base.OnHitNPC(target, hit, damageDone);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        CurrentElement?.OnHitEntity(target, info.Damage, [info]);
        //player.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake = Main.rand.NextFloat(0.85f, 1.15f);

        base.OnHitPlayer(target, info);
    }

    public override void CutTiles()
    {
        if (CurrentElement is null) return;
        DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
        Utils.TileActionAttempt cut = new Utils.TileActionAttempt(DelegateMethods.CutTiles);
        Vector2 beamStartPos = Projectile.Center;
        Vector2 beamEndPos = Projectile.Center + CurrentElement.targetedVector;
        Utils.PlotTileLine(beamStartPos, beamEndPos, 16, cut);
        base.CutTiles();
    }
}
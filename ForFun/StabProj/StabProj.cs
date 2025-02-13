using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.ForFun.StabProj
{
    public class StabProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.timeLeft = 15;
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            base.SetDefaults();
        }
        public override void AI()
        {
            base.AI();
        }
        public override string Texture => "Terraria/Images/Extra_98";
        public override bool PreDraw(ref Color lightColor)
        {
            float t = Projectile.timeLeft / 15f;
            float fac = (1 - MathF.Cos(MathHelper.TwoPi * MathF.Sqrt(t))) * .5f;
            Vector2 unit = Projectile.ai[0].ToRotationVector2();
            Color mainColor = Main.hslToRgb(new(Projectile.ai[1], 1, 0.5f));
            Main.EntitySpriteDraw(TextureAssets.Extra[98].Value, Projectile.Center - Main.screenPosition, null, mainColor with { A = 0 } * fac,  Projectile.ai[0] + MathHelper.PiOver2, new(36), new Vector2(1, 4) * fac, 0, 0);
            Main.EntitySpriteDraw(TextureAssets.Extra[98].Value, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * fac, Projectile.ai[0] + MathHelper.PiOver2, new(36), new Vector2(1, 4) * fac * .75f, 0, 0);
            //Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.HotPink, 0, new Vector2(.5f), 16, 0, 0);
            return false;
        }
    }
}

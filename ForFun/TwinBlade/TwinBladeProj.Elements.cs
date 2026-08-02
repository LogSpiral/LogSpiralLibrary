using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using LogSpiralLibrary.ForFun.TwinBlade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace LogSpiralLibrary.ForFun.TwinBlade;

public partial class TwinBladeProj
{
    public class TwinBladeNormalAttack : MeleeAction
    {
        public override void UpdateStatus(bool triggered)
        {
            Timer--;
            switch (Owner)
            {
                case Player player:
                    {
                        player.itemTime = 2;
                        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, CompositeArmRotation);

                        break;
                    }
            }
        }
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            StandardInfo.frame = new Rectangle(0, 0, 32, 36);
            var array1 = base.GetWeaponVertex(texture, alpha);

            return array1;
        }
    }

    public class TwinBladeCharge : MeleeAction
    {
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {

            if (Owner is not Player plr) return [];
            bool flip = Factor < 0.25f;
            // StandardInfo.frame = new(0, 0, 32, 36);
            Vector2 finalOrigin = new Vector2(0.2f, 0.7f);
            Vector2 drawCen = OffsetCenter + plr.Center;
            float sc = plr.GetAdjustedItemScale(plr.HeldItem);
            drawCen += plr.gfxOffY * Vector2.UnitY;

            float sign = flip ? -1 : 1;

            float factor = 1 - (fTimer - 1f) / (TimerMax - 1f);
            float beta = (factor - 0.9f) * factor * 2f * plr.direction;
            float randomScaler = MathHelper.Lerp(1, Main.rand.NextFloat(0.9f, 1.1f), factor);
            beta *= randomScaler;
            float gamma = beta;
            beta *= MathF.Exp(factor * 2);
            gamma *= MathF.Exp(factor * 2.3f);
            if (!flip)
            {
                beta += MathHelper.Pi;
                gamma += MathHelper.Pi;
            }
            float off = -(1 - MathF.Cos(MathF.Tau * MathF.Pow(1 - factor, MathF.Log10(2)))) * 0.75f;
            CustomVertexInfo[] result;
            if (Projectile.drawLayer == ProjectileDrawLayerID.HeldProj)
            {
                drawCen += (beta - MathHelper.PiOver2).ToRotationVector2() * (12 * sign);
                drawCen += beta.ToRotationVector2() * (2.667f * (1.5f + off * 4) * sign * plr.direction);
                drawCen += new Vector2(-2 * plr.direction, -2);
                beta += off * plr.direction;
                result = DrawingMethods.GetItemVertexes(finalOrigin, MathHelper.PiOver4, beta, Rotation, texture, KValue, OffsetSize * ModifyData.Size * sc, drawCen, Flip ^ flip, alpha, new Rectangle(0, 0, 32, 36));
            }
            else
            {
                drawCen += (gamma - MathHelper.PiOver2).ToRotationVector2() * (12 * sign);
                drawCen += gamma.ToRotationVector2() * (2.667f * (1.5f + off * 4) * sign * plr.direction);
                drawCen += new Vector2(8 * plr.direction, -2);
                gamma += off * plr.direction * 0.75f;
                result = DrawingMethods.GetItemVertexes(finalOrigin, MathHelper.PiOver4, gamma, Rotation, texture, KValue, OffsetSize * ModifyData.Size * sc, drawCen, Flip ^ flip, alpha, new Rectangle(32, 0, 32, 36));
            }
            if (Projectile.drawLayer == ProjectileDrawLayerID.None)
            {
                Projectile.drawLayer = ProjectileDrawLayerID.HeldProj;
            }
            else
            {
                Projectile.drawLayer = ProjectileDrawLayerID.None;
            }
            return result;
        }
        public override bool Attacktive => false;
        private bool _giveUp;
        public override void OnActive()
        {
            base.OnActive();
            _giveUp = false;
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            Rotation = Owner.direction > 0 ? 0 : MathHelper.Pi;
            Flip = Owner.direction < 0;
        }
        public override void UpdateStatus(bool triggered)
        {
            if (!triggered || Timer != 1 || _giveUp)
                Timer--;
            if (!triggered && Timer > 2)
                _giveUp = true;
            if (_giveUp)
                Timer--;
            if (Timer < 0)
                Timer = 0;
            if (Owner is not Player player)
                return;
            if (Timer == 2 && !_giveUp)
            {
                SoundEngine.PlaySound(SoundID.Item84);
                ParticleOrchestrator.Spawn_TrueExcalibur(new() { PositionInWorld = Owner.Center });
                float dir = Main.rand.NextFloat(0, MathHelper.Pi);
                for (int n = 0; n < 20; n++)
                {
                    float angle = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                    float a = angle < 0 ? angle + MathHelper.Pi : angle;
                    var dust = Dust.NewDustPerfect(Owner.Center, 278, angle.ToRotationVector2() * (9 - MathF.Pow(-a + dir, 2f)) * Main.rand.NextFloat(1f, 2f), 0, Color.White);
                    dust.noGravity = true;
                }
            }
            float factor = 1 - Factor;
            {
                var visualPlayer = player.GetModPlayer<TwinBladePlayerVisual>();
                float fac = (factor - 0.9f) * factor * 10 * MathF.Exp((factor - 1) * 2);
                fac *= player.direction;
                visualPlayer.Rotation = 0.1f * fac;
                visualPlayer.HeadRotation = -0.1f * fac;
                player.headPosition = new Vector2(3, 1) * fac;
                player.bodyPosition = new Vector2(2, 0) * fac;
                player.bodyRotation = 0.2f * fac;
                player.legRotation = 0.1f * fac;
                if (MathF.Abs(player.velocity.X) < 1f)
                    player.legFrame = new Microsoft.Xna.Framework.Rectangle(0, 56 * 8, 40, 56);
            }
            player.direction = Flip ? -1 : 1;
            float alpha = (factor - 0.9f) * factor * 2f * player.direction;
            float randomScaler = MathHelper.Lerp(1, Main.rand.NextFloat(0.9f, 1.1f), factor);
            alpha *= randomScaler;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, alpha * MathF.Exp(factor * 2));
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, alpha * MathF.Exp(factor * 2.3f));

        }
        public override void OnDeactive()
        {
            base.OnDeactive();
            if (Owner is not Player player)
                return;
            player.headPosition = Vector2.Zero;
            player.bodyPosition = Vector2.Zero;
            player.bodyRotation = 0;
            player.headRotation = 0;
            player.legRotation = 0;
            player.GetModPlayer<TwinBladePlayerVisual>().HeadRotation = 0;
            if (_giveUp)
                Projectile.Kill();
            else
                player.GetModPlayer<SequencePlayer>().PendingForcedNext = true;

        }
    }
    public class TwinBladeDash : MeleeAction
    {
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            if (Owner is not Player plr) return [];
            bool flip = true;
            // StandardInfo.frame = new(0, 0, 32, 36);
            Vector2 finalOrigin = new Vector2(0.2f, 0.7f);
            Vector2 drawCen = OffsetCenter + plr.Center;
            float sc = plr.GetAdjustedItemScale(plr.HeldItem);
            drawCen += plr.gfxOffY * Vector2.UnitY;

            float sign = flip ? -1 : 1;

            float factor = 1;
            float beta = 0.2f * plr.direction;
            float randomScaler = MathHelper.Lerp(1, Main.rand.NextFloat(0.9f, 1.1f), factor);
            beta *= randomScaler;
            float gamma = beta;
            beta *= MathF.Exp(2);
            gamma *= MathF.Exp(2.3f);
            if (!flip)
            {
                beta += MathHelper.Pi;
                gamma += MathHelper.Pi;
            }
            CustomVertexInfo[] result;
            if (Projectile.drawLayer == ProjectileDrawLayerID.HeldProj)
            {
                drawCen += (beta - MathHelper.PiOver2).ToRotationVector2() * (12 * sign);
                drawCen += beta.ToRotationVector2() * (4 * sign * plr.direction);
                drawCen += new Vector2(-2 * plr.direction, -2);
                result = DrawingMethods.GetItemVertexes(finalOrigin, MathHelper.PiOver4, beta, Rotation, texture, KValue, OffsetSize * ModifyData.Size * sc, drawCen, Flip ^ flip, alpha, new Rectangle(0, 0, 32, 36));
            }
            else
            {
                drawCen += (gamma - MathHelper.PiOver2).ToRotationVector2() * (12 * sign);
                drawCen += gamma.ToRotationVector2() * (4 * sign * plr.direction);
                drawCen += new Vector2(8 * plr.direction, -2);
                result = DrawingMethods.GetItemVertexes(finalOrigin, MathHelper.PiOver4, gamma, Rotation, texture, KValue, OffsetSize * ModifyData.Size * sc, drawCen, Flip ^ flip, alpha, new Rectangle(32, 0, 32, 36));
            }
            if (Projectile.drawLayer == ProjectileDrawLayerID.None)
            {
                Projectile.drawLayer = ProjectileDrawLayerID.HeldProj;
            }
            else
            {
                Projectile.drawLayer = ProjectileDrawLayerID.None;
            }
            return result;
        }
        public override bool Attacktive => false;
        public override void UpdateStatus(bool triggered)
        {
            Timer--;
            if (Owner is not Player player)
                return;
            {
                var visualPlayer = player.GetModPlayer<TwinBladePlayerVisual>();
                float fac = player.direction;
                visualPlayer.Rotation = 0.1f * fac;
                visualPlayer.HeadRotation = -0.1f * fac;
                player.headPosition = new Vector2(3, 1) * fac;
                player.bodyPosition = new Vector2(2, 0) * fac;
                player.bodyRotation = 0.2f * fac;
                player.legRotation = 0.1f * fac;
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathF.Exp(2) * player.direction);
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathF.Exp(2.3f) * player.direction);
            }
            player.direction = Flip ? -1 : 1;
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            Owner.velocity += new Vector2(32 * Owner.direction, 0);
            Flip = Owner.direction < 0;
            SoundEngine.PlaySound(SoundID.Item92, Owner.Center);
            for (int n = 0; n < 30; n++)
            {
                float max = Main.rand.NextFloat(Main.rand.NextFloat(MathHelper.Pi));
                var dust = Dust.NewDustPerfect(Owner.Center, DustID.FireworksRGB, -Main.rand.NextFloat(-max, max).ToRotationVector2() * Main.rand.NextFloat(32) * Owner.direction + new Vector2(-Owner.direction * 24, 0), 0, Color.White, 1);
                dust.noGravity = true;
            }
        }
        public override void OnDeactive()
        {
            base.OnDeactive();
            Owner.velocity *= 0.25f;
            if (Owner is not Player player)
                return;
            player.GetModPlayer<SequencePlayer>().PendingForcedNext = true;
            {
                var visualPlayer = player.GetModPlayer<TwinBladePlayerVisual>();
                float fac = player.direction;
                visualPlayer.Rotation = 0;
                visualPlayer.HeadRotation = 0;
                player.headPosition = Vector2.Zero;
                player.bodyPosition = Vector2.Zero;
                player.bodyRotation = 0;
                player.legRotation = 0;
            }
        }
    }
    public class TwinBladeStorm : MeleeAction
    {
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            if (Owner is not Player plr) return [];
            var vPlayer = plr.GetModPlayer<TwinBladePlayerVisual>();
            Vector2 finalOrigin = new Vector2(0.2f, 0.7f);
            Vector2 drawCen = OffsetCenter + plr.Center;
            float sc = plr.GetAdjustedItemScale(plr.HeldItem);
            drawCen += plr.gfxOffY * Vector2.UnitY;
            CustomVertexInfo[] result;
            float beta = MathF.Tau * Factor * 10;
            float gamma = beta + MathHelper.Pi;
            bool flag = (int)(Factor * 10) % 2 == 0;
            float x = MathHelper.Lerp(4f, 1.5f, MathF.Pow(Factor - 0.5f, 2) * 4);
            if (Projectile.drawLayer == ProjectileDrawLayerID.HeldProj)
            {
                drawCen += (beta - MathHelper.PiOver2).ToRotationVector2() * 12 * (Flip ? -1 : 1);
                drawCen += beta.ToRotationVector2() * (4 * plr.direction);
                drawCen += new Vector2(-2 * plr.direction, -2);
                drawCen -= plr.position + new Vector2(10, 28);
                drawCen = drawCen.RotatedBy(vPlayer.Rotation);
                drawCen += plr.position + new Vector2(10, 28);
                result = DrawingMethods.GetItemVertexes(finalOrigin, MathHelper.PiOver4, beta, Rotation, texture, x, OffsetSize * ModifyData.Size * sc, drawCen, Flip, alpha, new Rectangle(flag ? 32 : 0, 0, 32, 36));



            }
            else
            {
                drawCen += (gamma - MathHelper.PiOver2).ToRotationVector2() * 12 * (Flip ? -1 : 1);
                drawCen += gamma.ToRotationVector2() * (4 * plr.direction);
                drawCen += new Vector2(8 * plr.direction, -2);
                drawCen -= plr.position + new Vector2(10, 28);
                drawCen = drawCen.RotatedBy(vPlayer.Rotation);
                drawCen += plr.position + new Vector2(10, 28);
                result = DrawingMethods.GetItemVertexes(finalOrigin, MathHelper.PiOver4, gamma, Rotation, texture, x, OffsetSize * ModifyData.Size * sc, drawCen, Flip, alpha, new Rectangle(flag ? 0 : 32, 0, 32, 36));



            }
            if (Projectile.drawLayer == ProjectileDrawLayerID.None)
            {
                Projectile.drawLayer = ProjectileDrawLayerID.HeldProj;
            }
            else
            {
                Projectile.drawLayer = ProjectileDrawLayerID.None;
            }
            return result;
        }
        public override bool Attacktive => true;
        public override void UpdateStatus(bool triggered)
        {
            Timer--;
            if (Owner is Player player)
            {
                var vPlayer = player.GetModPlayer<TwinBladePlayerVisual>();
                player.direction = Flip ^ ((int)(Factor * 10) % 2 == 0) ? -1 : 1;
                vPlayer.Rotation = -MathF.Tau * Factor * (Flip ? -1 : 1);
                Rotation = vPlayer.Rotation;
                vPlayer.IsTwinBladeStorming = true;
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathF.Tau * Factor * 10);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathF.Tau * Factor * 10 + MathHelper.Pi);

                bool flag = (int)(Factor * 10) % 2 == 0;
                UltraSwoosh front = flag ? _swooshGreen : _swooshBlue;
                UltraSwoosh back = flag ? _swooshBlue : _swooshGreen;
                var drawCen = player.Center;
                drawCen -= player.position + new Vector2(10, 28);
                drawCen = drawCen.RotatedBy(vPlayer.Rotation);
                drawCen += player.position + new Vector2(10, 28);
                float x = MathHelper.Lerp(4f, 1.5f, MathF.Pow(Factor - 0.5f, 2) * 4);
                if (front != null)
                {
                    front.center = drawCen;
                    front.angleRange = (20 * Factor - 0.5f, 20 * Factor - 1f - 0.5f);
                    front.rotation = Rotation;
                    front.timeLeft = 30;
                    front.xScaler = x;
                }
                if (back != null)
                {
                    back.center = drawCen;
                    back.angleRange = (20 * Factor + 1f - 0.5f, 20 * Factor - 0.5f);
                    back.rotation = Rotation;
                    back.timeLeft = 30;
                    back.xScaler = x;
                }
                if (_swooshGreen != null && Timer % 2 == 0)
                {
                    var (from, to) = _swooshGreen.angleRange;
                    var u = UltraSwoosh.NewUltraSwoosh(StandardInfo.VertexStandard.canvasName,60, 90, drawCen, (from + 0.25f, to - 0.25f));
                    u.ColorVector = new(0, 1, 0);
                    u.xScaler = _swooshGreen.xScaler;
                    u.rotation = _swooshGreen.rotation;
                    u.ApplyStdValueToVtxEffect(StandardInfo);
                    u.weaponTex = TextureAssets.Item[ItemID.Terragrim].Value;
                }
                if (_swooshBlue != null && Timer % 2 == 1)
                {
                    var (from, to) = _swooshBlue.angleRange;
                    var u = UltraSwoosh.NewUltraSwoosh(StandardInfo.VertexStandard.canvasName, 60, 90, drawCen, (from + 0.25f, to - 0.25f));
                    u.ColorVector = new(0, 1, 0);
                    u.xScaler = _swooshBlue.xScaler;
                    u.rotation = _swooshBlue.rotation;
                    u.ApplyStdValueToVtxEffect(StandardInfo);
                    u.weaponTex = TextureAssets.Item[ItemID.Arkhalis].Value;
                }


            }
            if(Timer % 4 == 0)
                SoundEngine.PlaySound(SoundID.Item71 with { MaxInstances = -1}, Owner.Center);

        }

        UltraSwoosh _swooshGreen;
        UltraSwoosh _swooshBlue;
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            Owner.velocity += new Vector2(32 * Owner.direction, -8);
            Flip = Owner.direction < 0;
            if (!Main.dedServ)
            {
                _swooshGreen = UltraSwoosh.NewUltraSwoosh(StandardInfo.VertexStandard.canvasName, 30, 75, Owner.Center, (0, 0));
                _swooshGreen.ColorVector = new(0, 1, 0);
                _swooshGreen.xScaler = 1.5f;
                _swooshGreen.ApplyStdValueToVtxEffect(StandardInfo);
                _swooshGreen.weaponTex = TextureAssets.Item[ItemID.Terragrim].Value;
                _swooshBlue = UltraSwoosh.NewUltraSwoosh(StandardInfo.VertexStandard.canvasName, 30, 75, Owner.Center, (0, 0));
                _swooshBlue.ColorVector = new(0, 1, 0);
                _swooshBlue.xScaler = 1.5f;
                _swooshBlue.ApplyStdValueToVtxEffect(StandardInfo);
                _swooshBlue.weaponTex = TextureAssets.Item[ItemID.Arkhalis].Value;
            }
        }

        public override void OnEndSingle()
        {
            base.OnEndSingle();
            if (Owner is Player player)
            {
                var vPlayer = player.GetModPlayer<TwinBladePlayerVisual>();
                vPlayer.Rotation = 0;
                vPlayer.IsTwinBladeStorming = false;
                player.velocity *= 0.125f;
            }
        }
        public override bool Collide(Rectangle rectangle)
        {
            return base.Collide(rectangle);
        }
    }
}

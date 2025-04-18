﻿//using CoolerItemVisualEffect;
using LogSpiralLibrary.CodeLibrary.DataStructures;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;
using LogSpiralLibrary.CodeLibrary.UIGenericConfig;
using LogSpiralLibrary.ForFun.TestBlade3;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using ReLogic.Content;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent.NetModules;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
//using static CoolerItemVisualEffect.CoolerItemVisualEffect;
using static LogSpiralLibrary.LogSpiralLibraryMod;
namespace LogSpiralLibrary.CodeLibrary
{
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
    public abstract class HeldProjectile : ModProjectile, IChannelProj
    {
        public Player Player => Main.player[Projectile.owner];

        public virtual void OnCharging(bool left, bool right) { }
        public virtual void OnRelease(bool charged, bool left) { Projectile.Kill(); }
        public virtual bool UseLeft => true;
        public virtual bool UseRight => false;
        public virtual bool Charging => (UseLeft && Player.controlUseItem) || (UseRight && Player.controlUseTile);
        public virtual bool Charged => true;
        public virtual (int X, int Y) FrameMax => (1, 1);
        public virtual Texture2D GlowEffect
        {
            get
            {
                if (ModContent.HasAsset(GlowTexture))
                {
                    return ModContent.Request<Texture2D>(GlowTexture).Value;
                }
                return null;
            }
        }
        public virtual float Factor => 0;
        public virtual Color GlowColor => Color.White;
        /// <summary>
        /// 1为左键
        /// 2为右键
        /// 3自由发挥
        /// </summary>
        public byte controlState;
        public Texture2D projTex => TextureAssets.Projectile[Projectile.type].Value;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(controlState);
            base.SendExtraAI(writer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            controlState = reader.ReadByte();
            base.ReceiveExtraAI(reader);
        }
    }
    public abstract class RangedHeldProjectile : HeldProjectile
    {
        //public byte controlState
        //{
        //    get => (byte)Projectile.ai[1];
        //    set => Projectile.ai[1] = value;
        //}
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.alpha = 0;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }
        public override bool Charging => base.Charging && Projectile.frame == 0;
        //Texture2D glowEffect;
        //public override void Load()
        //{
        //    base.Load();
        //    if (Mod.HasAsset(GlowTexture.Replace("VirtualDream/", "")))
        //    {
        //        glowEffect = IllusionBoundMod.GetTexture(GlowTexture, false);
        //    }
        //}

        public override bool Charged => Factor == 1;
        public virtual Vector2 ShootCenter => HeldCenter;
        public virtual Vector2 HeldCenter => Player.Center;
        public virtual void UpdatePlayer()
        {
            Player.ChangeDir(Projectile.direction);
            Player.heldProj = Projectile.whoAmI;
            Player.itemTime = 2;
            Player.itemAnimation = 2;
            Player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);
            Player.SetCompositeArmFront(enabled: true, Player.CompositeArmStretchAmount.Full, Player.itemRotation - MathHelper.PiOver2 - (Player.direction == -1 ? MathHelper.Pi : 0));
            Projectile.Center = Player.Center;
        }
        public override void AI()
        {
            base.AI();
            #region 更新玩家
            UpdatePlayer();
            #endregion
            #region 更新弹幕
            if (Charging)
            {
                Projectile.timeLeft = 2;
                Projectile.ai[0]++;
                Projectile.velocity = Utils.SafeNormalize(Player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition - HeldCenter, Vector2.One);
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.ai[1] = Player.controlUseItem ? 1 : 0;
                if (Player.controlUseItem)
                {
                    controlState = 1;
                }
                if (Player.controlUseTile)
                {
                    controlState = 2;
                }
                OnCharging(Player.controlUseItem, Player.controlUseTile);
            }
            else
            {
                OnRelease(Charged, Projectile.ai[1] == 1);
                Projectile.frame = 1;
            }
            //Main.NewText(Charged);
            #endregion
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 center = HeldCenter - Main.screenPosition + new Vector2(0, Player.gfxOffY);
            Rectangle? frame = null;
            float rotation = Projectile.rotation;
            float scale = 1f;
            SpriteEffects effect = Player.direction == -1 ? SpriteEffects.FlipVertically : 0;
            Vector2 texSize = texture.Size() / new Vector2(FrameMax.X, FrameMax.Y);
            Vector2 origin = texSize * new Vector2(0, 1);
            GetDrawInfos(ref texture, ref center, ref frame, ref lightColor, ref rotation, ref origin, ref scale, ref effect);
            FlipOrigin(ref origin, effect, texSize);
            Main.EntitySpriteDraw(texture, center, frame, lightColor, rotation, origin, scale, effect, 0);
            if (GlowEffect != null)
            {
                Main.EntitySpriteDraw(GlowEffect, center, frame, GlowColor, rotation, origin, scale, effect, 0);
            }

            return false;
        }
        public virtual void GetDrawInfos(ref Texture2D texture, ref Vector2 center, ref Rectangle? frame, ref Color color, ref float rotation, ref Vector2 origin, ref float scale, ref SpriteEffects spriteEffects)
        {

        }
        public virtual void FlipOrigin(ref Vector2 origin, SpriteEffects spriteEffects, Vector2 textureSize)
        {
            origin.Y = spriteEffects == SpriteEffects.FlipVertically ? textureSize.Y - origin.Y : origin.Y;
            origin.X = spriteEffects == SpriteEffects.FlipHorizontally ? textureSize.X - origin.X : origin.X;
        }
    }
    public abstract class HammerProj : HeldProjectile, IHammerProj
    {
        public virtual Vector2 scale => new(1);
        public virtual Rectangle? frame => null;
        public virtual Vector2 projCenter => Player.Center + new Vector2(0, Player.gfxOffY);
        public Projectile projectile => Projectile;
        public override bool Charged => Factor > 0.75f;
        public virtual SpriteEffects flip => Player.direction == -1 ? SpriteEffects.FlipHorizontally : 0;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault(HammerName);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.immune[projectile.owner] = 5;
        }
        public override void SetDefaults()
        {
            projectile.width = 1;
            projectile.height = 1;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
            projectile.scale = 1f;
            //projectile.alpha = 255;
            projectile.hide = true;
            projectile.ownerHitCheck = true;
            projectile.DamageType = DamageClass.Melee;
            projectile.tileCollide = false;
            projectile.friendly = true;
        }
        public override void OnRelease(bool charged, bool left)
        {
            if (Charged)
            {
                if ((int)projectile.ai[1] == 1)
                {
                    OnChargedShoot();
                }
            }
            if ((int)projectile.ai[1] == 0)
            {
                projectile.damage = 0;
                if (Charged)
                {
                    projectile.damage = (int)(Player.GetWeaponDamage(Player.HeldItem) * (3 * Factor * Factor));
                    SoundEngine.PlaySound(SoundID.Item71);
                }
            }
            projectile.ai[1]++;
            if (projectile.ai[1] > (Charged ? (MaxTimeLeft * Factor) : timeCount))
            {
                projectile.Kill();
            }
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if ((int)projectile.ai[1] == 0)
            {
                return false;
            }
            float point = 0f;
            return targetHitbox.Intersects(Utils.CenteredRectangle((CollidingCenter - DrawOrigin).RotatedBy(Rotation) + projCenter, CollidingSize)) || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projCenter, (CollidingCenter - DrawOrigin).RotatedBy(Rotation) + projCenter, 8, ref point);
            //float point = 0f;
            //Vector2 vec = Pos - player.Center;
            //vec.Normalize();
            //bool flag2 = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), player.Center - vec * (30 - Distance - NegativeDistance), player.Center + vec * (66 + Distance + NegativeDistance), 18, ref point);
            //return flag2;
        }
        //public float Rotation => projectile.ai[1] > 0 ? ((int)factor == 1 ? (projectile.ai[1] / 5).Lerp(-MathHelper.PiOver2, MathHelper.Pi / 8 * 3) : ((timeCount - projectile.ai[1]) / MaxTime).Lerp(MathHelper.Pi / 8 * 3, -MathHelper.PiOver2)) : ((float)Math.Pow(factor,2)).Lerp(MathHelper.Pi / 8 * 3, -MathHelper.PiOver2 - MathHelper.Pi / 8);//MathHelper.Pi / 8 * 3 - factor * (MathHelper.Pi / 8 * 7)
        public virtual float Rotation
        {
            get
            {
                //Main.NewText(timeCount);
                var theta = ((float)Math.Pow(Factor, 2)).Lerp(MathHelper.Pi / 8 * 3, -MathHelper.PiOver2 - MathHelper.Pi / 8);
                if (projectile.ai[1] > 0)
                {
                    if (Charged)
                    {
                        //Main.NewText(projectile.ai[1] / MaxTimeLeft / factor);
                        theta = (projectile.ai[1] / MaxTimeLeft / Factor).Lerp(theta, MathHelper.Pi / 8 * 3);
                        //return player.direction == -1 ? MathHelper.Pi * 1.5f - theta : theta;
                    }
                    else
                    {
                        theta = ((timeCount - projectile.ai[1]) / MaxTime).Lerp(MathHelper.Pi / 8 * 3, -MathHelper.PiOver2);
                        //return player.direction == -1 ? MathHelper.Pi * 1.5f - theta : theta;
                    }
                }
                return Player.direction == -1 ? MathHelper.Pi * 1.5f - theta : theta;

            }
        }

        public virtual float timeCount
        {
            get => projectile.ai[0];
            set
            {
                projectile.ai[0] = MathHelper.Clamp(value, 0, MaxTime);
            }
        }
        public virtual string HammerName => "做个锤子";
        public virtual float MaxTime => 15;
        public override float Factor => timeCount / MaxTime;
        public virtual Vector2 CollidingSize => new(32);
        public virtual Vector2 CollidingCenter => new(size.X / FrameMax.X - 16, 16);
        public virtual Vector2 DrawOrigin => new(16, size.Y / FrameMax.Y - 16);
        public Vector2 size;
        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.Server)
                size = projTex.Size();

            base.OnSpawn(source);
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                writer.WriteVector2(size);
            base.SendExtraAI(writer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            if (Main.netMode == NetmodeID.Server)
                size = reader.ReadVector2();
            base.ReceiveExtraAI(reader);
        }
        public virtual Color color => /*projectile.GetAlpha(Color.White);*/Lighting.GetColor((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16, Color.White);
        public virtual float MaxTimeLeft => 5;
        public override bool Charging => base.Charging && projectile.ai[1] == 0;
        public override void AI()
        {
            //Projectiles.KluexEnergyCrystal.KluexEnergyZone
            Player.lastVisualizedSelectedItem = new();
            if (Player.dead) projectile.Kill();
            if (Charging && projectile.ai[1] == 0)
            {
                OnCharging(Player.controlUseItem, Player.controlUseTile);
                timeCount++;
                if (Player.controlUseItem)
                {
                    controlState = 1;
                }
                if (Player.controlUseTile)
                {
                    controlState = 2;
                }
            }
            else
            {
                OnRelease(Charged, controlState == 1);
            }
            projectile.timeLeft = 2;
            Player.heldProj = projectile.whoAmI;
            Player.RotatedRelativePoint(Player.MountedCenter, true);
            Player.itemTime = 2;
            Player.itemAnimation = 2;
            Player.ChangeDir(Math.Sign((Player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition - projCenter).X));
            Player.SetCompositeArmFront(enabled: true, Player.CompositeArmStretchAmount.Full, Rotation - (Player.direction == -1 ? MathHelper.Pi : MathHelper.PiOver2));// -MathHelper.PiOver2

            projectile.Center = Player.Center + new Vector2(0, Player.gfxOffY);

        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (size == default)
                size = projTex.Size();
            if (GlowEffect != null)
            {
                Main.spriteBatch.DrawHammer(this, GlowEffect, GlowColor, frame);
            }
            else Main.spriteBatch.DrawHammer(this);
            //Main.spriteBatch.DrawHammer(this);
            return false;
        }
        public virtual void OnChargedShoot()
        {
        }
    }
    public abstract class VertexHammerProj : HammerProj
    {
        BloomEffectInfo useBloom = default;
        AirDistortEffectInfo useDistort = default;
        MaskEffectInfo useMask = default;
        public override float Rotation => base.Rotation;
        public virtual CustomVertexInfo[] CreateVertexs(Vector2 drawCen, float scaler, float startAngle, float endAngle, float alphaLight, ref int[] whenSkip)
        {
            var bars = new CustomVertexInfo[90];
            for (int i = 0; i < 45; i++)
            {
                var f = i / 44f;
                //var newVec = (endAngle.AngleLerp(startAngle, f) - MathHelper.PiOver4).ToRotationVector2() * scaler;
                var newVec = (f.Lerp(endAngle + (Player.direction == -1 ? MathHelper.TwoPi : 0), startAngle + (Player.direction == -1 && Player.gravDir == -1 ? MathHelper.TwoPi * 2 : 0)) - MathHelper.PiOver4).ToRotationVector2() * scaler;// + (Player.direction == -1 ? MathHelper.TwoPi : 0)
                //Main.spriteBatch.DrawLine(drawCen, drawCen + newVec, Color.Red, 1, drawOffset: -Main.screenPosition);
                var _f = 6 * f / (3 * f + 1);
                _f = MathHelper.Clamp(_f, 0, 1);
                var realColor = VertexColor(f);
                realColor.A = (byte)(_f * 255);
                bars[2 * i] = new CustomVertexInfo(drawCen + newVec, realColor, new Vector3(1 - f, 1, alphaLight));
                realColor.A = 0;
                bars[2 * i + 1] = new CustomVertexInfo(drawCen, realColor, new Vector3(0, 0, alphaLight));
            }
            return bars;
        }
        public virtual Color VertexColor(float time) => Color.White;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="additive"></param>
        /// <param name="indexOfGreyTex"></param>
        /// <param name="endAngle"></param>
        /// <param name="useHeatMap"></param>
        /// <param name="passCount">已被弃用</param>
        public virtual void VertexInfomation(ref bool additive, ref int indexOfGreyTex, ref float endAngle, ref bool useHeatMap, ref int passCount) { }
        public virtual void RenderInfomation(ref BloomEffectInfo useBloom, ref AirDistortEffectInfo useDistort, ref MaskEffectInfo useMask) { }
        public virtual bool RedrawSelf => false;
        public virtual bool WhenVertexDraw => !Charging && Charged;
        /// <summary>
        /// 默认使用的热度图，允许被外部修改，除非子类那边重写了属性
        /// </summary>
        public Texture2D heatMap;
        public virtual Texture2D HeatMap
        {
            get
            {
                //dynamic modplr = null;
                //if (LogSpiralLibrary.CIVELoaded && ModContent.TryFind("CoolerItemVisualEffect", "CoolerItemVisualEffectPlayer", out ModPlayer modPlayer))
                //{
                //    modplr = modPlayer;
                //    return modplr.colorInfo.tex ?? heatMap;
                //}
                return heatMap;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            bool predraw = false;
            if (!RedrawSelf)
                predraw = base.PreDraw(ref lightColor);
            var swooshUL = ShaderSwooshUL;

            if (!WhenVertexDraw || swooshUL == null || Main.GameViewMatrix == null || RenderEffect == null) goto mylabel; //
            var itemTex = TextureAssets.Item[Player.HeldItem.type].Value;

            var trans = Main.GameViewMatrix != null ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity;
            var _center = projCenter;
            var drawCen = _center;
            float xScaler = 1f;
            float scaler = (projTex.Size() / new Vector2(FrameMax.X, FrameMax.Y)).Length() * Player.GetAdjustedItemScale(Player.HeldItem) / xScaler - (new Vector2(0, projTex.Size().Y / FrameMax.Y) - DrawOrigin).Length();
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
            bool additive = false;
            int indexOfGreyTex = 7;
            float endAngle = Player.direction == -1 ? MathHelper.Pi / 8 : (-MathHelper.PiOver2 - MathHelper.Pi / 8);
            bool useHeatMap = HeatMap != null;

            RenderInfomation(ref useBloom, ref useDistort, ref useMask);
            var passCount = 3;
            VertexInfomation(ref additive, ref indexOfGreyTex, ref endAngle, ref useHeatMap, ref passCount);
            if (useHeatMap) passCount = 2;
            int[] whenSkip = [];
            endAngle = Player.gravDir == -1 ? MathHelper.PiOver2 - endAngle : endAngle;
            CustomVertexInfo[] bars = CreateVertexs(drawCen, scaler, Player.gravDir == -1 ? MathHelper.PiOver2 - Rotation : Rotation, endAngle, additive ? 0.5f : Lighting.GetColor((projCenter / 16).ToPoint().X, (projCenter / 16).ToPoint().Y).R / 255f * .5f, ref whenSkip);
            if (bars.Length < 2) goto mylabel;
            SamplerState sampler = SamplerState.LinearWrap;
            CustomVertexInfo[] triangleList = new CustomVertexInfo[(bars.Length - 2) * 3];//
            for (int i = 0; i < bars.Length - 2; i += 2)
            {
                if (whenSkip.Contains(i)) continue;
                var k = i / 2;
                if (6 * k < triangleList.Length)
                {
                    triangleList[6 * k] = bars[i];
                    triangleList[6 * k + 1] = bars[i + 2];
                    triangleList[6 * k + 2] = bars[i + 1];
                }
                if (6 * k + 3 < triangleList.Length)
                {
                    triangleList[6 * k + 3] = bars[i + 1];
                    triangleList[6 * k + 4] = bars[i + 2];
                    triangleList[6 * k + 5] = bars[i + 3];
                }
            }
            var sb = Main.spriteBatch;
            if ((useBloom.Active || useDistort.Active || useMask.Active) && LogSpiralLibraryMod.CanUseRender)
            {
                // 如果任一特效存在，就走这边的流程
                // 现在已经被VertexMeleeEffect全面取代了
                GraphicsDevice gd = Main.instance.GraphicsDevice;
                RenderTarget2D render = Instance.Render;
                sb.End(); // 之前的绘制内容存到Main.screenTarget上
                gd.SetRenderTarget(render);// 新的画到自己的render上
                gd.Clear(Color.Transparent);
                sb.Begin(SpriteSortMode.Immediate, additive ? BlendState.Additive : BlendState.NonPremultiplied, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);//Main.DefaultSamplerState//Main.GameViewMatrix.TransformationMatrix
                swooshUL.Parameters["uTransform"].SetValue(model * trans * projection);
                swooshUL.Parameters["uLighter"].SetValue(0);
                swooshUL.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);
                swooshUL.Parameters["checkAir"].SetValue(false);
                swooshUL.Parameters["airFactor"].SetValue(3);
                swooshUL.Parameters["gather"].SetValue(true);
                swooshUL.Parameters["lightShift"].SetValue(0);
                swooshUL.Parameters["distortScaler"].SetValue(1);
                if (frame != null)
                {
                    Rectangle uframe = frame.Value;
                    Vector2 size = itemTex.Size();
                    swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(uframe.TopLeft() / size, uframe.Width / size.X, uframe.Height / size.Y));
                }
                else
                    swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(0, 0, 1, 1));
                swooshUL.Parameters["heatRotation"].SetValue(Matrix.Identity);
                swooshUL.Parameters["heatMapAlpha"].SetValue(true);
                swooshUL.Parameters["AlphaVector"].SetValue(HeatMap != null ? new Vector3(0, 0, 1) : new Vector3(0, 1, 0));


                Main.graphics.GraphicsDevice.Textures[0] = BaseTex[indexOfGreyTex].Value;
                Main.graphics.GraphicsDevice.Textures[1] = AniTex[3].Value;
                Main.graphics.GraphicsDevice.Textures[2] = itemTex;
                if (HeatMap != null && useHeatMap)
                {
                    Main.graphics.GraphicsDevice.Textures[3] = HeatMap;
                }

                Main.graphics.GraphicsDevice.SamplerStates[0] = sampler;
                Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
                Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
                Main.graphics.GraphicsDevice.SamplerStates[3] = SamplerState.LinearClamp;

                swooshUL.CurrentTechnique.Passes[7].Apply();
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList, 0, bars.Length - 2);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;

                if (useDistort.Active)
                {
                    sb.End();
                    gd.SetRenderTarget(Instance.Render_Swap);
                    gd.Clear(Color.Transparent);
                    sb.Begin(SpriteSortMode.Immediate, additive ? BlendState.Additive : BlendState.NonPremultiplied, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);//Main.DefaultSamplerState//Main.GameViewMatrix.TransformationMatrix
                                                                                                                                                                                                //CoolerItemVisualEffect.ShaderSwooshEX.Parameters["lightShift"].SetValue(0);
                    swooshUL.Parameters["distortScaler"].SetValue(useDistort.distortScaler);
                    //sb.Draw(AniTex[8].Value, new Vector2(200, 200), Color.White);
                    Main.graphics.GraphicsDevice.Textures[0] = BaseTex[indexOfGreyTex].Value;
                    Main.graphics.GraphicsDevice.Textures[1] = AniTex[3].Value;
                    Main.graphics.GraphicsDevice.Textures[2] = itemTex;
                    if (HeatMap != null && useHeatMap)
                        Main.graphics.GraphicsDevice.Textures[3] = HeatMap;

                    Main.graphics.GraphicsDevice.SamplerStates[0] = sampler;
                    Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
                    Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
                    Main.graphics.GraphicsDevice.SamplerStates[3] = SamplerState.LinearClamp;

                    swooshUL.CurrentTechnique.Passes[7].Apply();
                    for (int n = 0; n < triangleList.Length; n++)
                    {
                        triangleList[n].Position = (triangleList[n].Position - Player.Center) * useDistort.distortScaler + Player.Center;
                    }
                    //TODO 对color魔改
                    Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList, 0, bars.Length - 2);
                    Main.graphics.GraphicsDevice.RasterizerState = originalState;
                }
                sb.End();
                //然后在随便一个render里绘制屏幕，并把上面那个带弹幕的render传进shader里对屏幕进行处理
                //原版自带的screenTargetSwap就是一个可以使用的render，（原版用来连续上滤镜）
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                if (useDistort.Active)
                {
                    gd.SetRenderTarget(Main.screenTargetSwap);//将画布设置为这个 
                    gd.Clear(Color.Transparent);//清空
                    Main.instance.GraphicsDevice.Textures[2] = Misc[18].Value;
                    AirDistortEffect.Parameters["uScreenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                    AirDistortEffect.Parameters["strength"].SetValue(5f);
                    AirDistortEffect.Parameters["rotation"].SetValue(Matrix.Identity);//MathHelper.Pi * Main.GlobalTimeWrappedHourly
                    AirDistortEffect.Parameters["tex0"].SetValue(Instance.Render_Swap);
                    AirDistortEffect.Parameters["colorOffset"].SetValue(0f);
                    AirDistortEffect.CurrentTechnique.Passes[0].Apply();//ApplyPass 
                    //0    1     2
                    //.001 .0035 .005
                    sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);//绘制原先屏幕内容
                    gd.SetRenderTarget(Main.screenTarget);
                    gd.Clear(Color.Transparent);
                    sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                }
                if (useMask.Active)
                {

                    gd.SetRenderTarget(Main.screenTargetSwap);
                    gd.Clear(Color.Transparent);
                    sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                    gd.SetRenderTarget(LogSpiralLibraryMod.Instance.Render_Swap);
                    gd.Clear(Color.Transparent);
                    Main.graphics.GraphicsDevice.Textures[1] = useMask.fillTex;
                    RenderEffect.Parameters["tex0"].SetValue(render);
                    RenderEffect.Parameters["invAlpha"].SetValue(useMask.tier1);
                    RenderEffect.Parameters["lightAsAlpha"].SetValue(useMask.lightAsAlpha);
                    RenderEffect.Parameters["tier2"].SetValue(useMask.tier2);
                    RenderEffect.Parameters["position"].SetValue(useMask.offset);
                    RenderEffect.Parameters["maskGlowColor"].SetValue(useMask.glowColor.ToVector4());
                    RenderEffect.Parameters["ImageSize"].SetValue(useMask.TexSize);
                    RenderEffect.Parameters["inverse"].SetValue(useMask.inverse);
                    RenderEffect.CurrentTechnique.Passes[1].Apply();
                    sb.Draw(render, Vector2.Zero, Color.White);

                    sb.End();
                    sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                    gd.SetRenderTarget(render);
                    gd.Clear(Color.Transparent);
                    sb.Draw(Instance.Render_Swap, Vector2.Zero, Color.White);

                    gd.SetRenderTarget(Main.screenTarget);
                    gd.Clear(Color.Transparent);
                    sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                    sb.Draw(Instance.Render_Swap, Vector2.Zero, Color.White);

                    sb.End();
                    sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                }
                if (useBloom.Active)
                {
                    gd.SetRenderTarget(Main.screenTargetSwap);
                    gd.Clear(Color.Transparent);
                    sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                    RenderEffect.Parameters["screenScale"].SetValue(Main.ScreenSize.ToVector2());
                    RenderEffect.Parameters["threshold"].SetValue(useBloom.threshold);
                    RenderEffect.Parameters["range"].SetValue(useBloom.range);
                    RenderEffect.Parameters["intensity"].SetValue(useBloom.intensity * 2.5f);
                    RenderEffect.Parameters["uBloomAdditive"].SetValue(useBloom.additive);
                    for (int n = 0; n < useBloom.times - 1; n++)
                    {
                        gd.SetRenderTarget(Instance.Render_Swap);
                        //RenderEffect.Parameters["tex0"].SetValue(render);
                        gd.Clear(Color.Transparent);
                        RenderEffect.CurrentTechnique.Passes[4].Apply();
                        sb.Draw(n == 0 ? render : Instance.Render_Swap2, Vector2.Zero, Color.White);



                        gd.SetRenderTarget(Instance.Render_Swap2);
                        //RenderEffect.Parameters["tex0"].SetValue(Instance.Render_Swap);
                        gd.Clear(Color.Transparent);
                        RenderEffect.CurrentTechnique.Passes[4].Apply();
                        sb.Draw(Instance.Render_Swap, Vector2.Zero, Color.White);
                    }
                    gd.SetRenderTarget(Instance.Render_Swap);
                    gd.Clear(Color.Transparent);
                    //RenderEffect.Parameters["tex0"].SetValue(render);
                    RenderEffect.CurrentTechnique.Passes[4].Apply();
                    sb.Draw(useBloom.times == 1 ? render : Instance.Render_Swap2, Vector2.Zero, Color.White);
                    gd.SetRenderTarget(useBloom.times == 1 ? render : Instance.Render_Swap2);
                    gd.Clear(Color.Transparent);
                    RenderEffect.CurrentTechnique.Passes[4].Apply();
                    sb.Draw(Instance.Render_Swap, Vector2.Zero, Color.White);
                    sb.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                    gd.SetRenderTarget(Main.screenTarget);
                    gd.Clear(Color.Transparent);
                    sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                }
                else
                {
                    gd.SetRenderTarget(Main.screenTarget);
                    gd.Clear(Color.Transparent);
                    sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                }
                Main.instance.GraphicsDevice.BlendState = AllOne;
                sb.Draw(render, Vector2.Zero, Color.White);// + Main.rand.NextVector2Unit() * 16

                if (useBloom.Active) 
                {
                    Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
                    sb.Draw(useBloom.times == 1 ? render : Instance.Render_Swap2, Vector2.Zero, Color.White);
                }
                Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            }
            else
            {
                // 否则使用普通绘制

                sb.End();
                sb.Begin(SpriteSortMode.Immediate, additive ? BlendState.Additive : BlendState.NonPremultiplied, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);//Main.DefaultSamplerState//Main.GameViewMatrix.TransformationMatrix

                swooshUL.Parameters["uTransform"].SetValue(VertexDrawInfo.uTransform);
                swooshUL.Parameters["uLighter"].SetValue(0);
                swooshUL.Parameters["uTime"].SetValue(-(float)ModTime * 0.03f);//-(float)Main.time * 0.06f
                swooshUL.Parameters["checkAir"].SetValue(false);
                swooshUL.Parameters["airFactor"].SetValue(1);
                swooshUL.Parameters["gather"].SetValue(true);
                swooshUL.Parameters["lightShift"].SetValue(0);
                swooshUL.Parameters["heatRotation"].SetValue(Matrix.Identity);
                swooshUL.Parameters["distortScaler"].SetValue(0);
                swooshUL.Parameters["alphaFactor"].SetValue(1.5f);
                swooshUL.Parameters["heatMapAlpha"].SetValue(true);
                swooshUL.Parameters["AlphaVector"].SetValue(HeatMap != null ? new Vector3(0, 0, 1) : new Vector3(0, 1, 0));
                if (frame != null)
                {
                    Rectangle uframe = frame.Value;
                    Vector2 size = itemTex.Size();
                    swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(uframe.TopLeft() / size, uframe.Width / size.X, uframe.Height / size.Y));
                }
                else
                    swooshUL.Parameters["uItemFrame"].SetValue(new Vector4(0, 0, 1, 1));

                Main.graphics.GraphicsDevice.Textures[0] = BaseTex[indexOfGreyTex].Value;
                Main.graphics.GraphicsDevice.Textures[1] = AniTex[3].Value;
                Main.graphics.GraphicsDevice.Textures[2] = itemTex;
                if (HeatMap != null && useHeatMap)
                    Main.graphics.GraphicsDevice.Textures[3] = HeatMap;

                Main.graphics.GraphicsDevice.SamplerStates[0] = sampler;
                Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
                Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
                Main.graphics.GraphicsDevice.SamplerStates[3] = SamplerState.LinearClamp;

                swooshUL.CurrentTechnique.Passes[7].Apply();
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList, 0, bars.Length - 2);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);//Main.DefaultSamplerState//Main.GameViewMatrix.TransformationMatrix
            }
        mylabel:
            return predraw;
        }
        public override void PostAI()
        {
            base.PostAI();
        }
        public override void OnKill(int timeLeft)
        {
            if (Charged && Main.netMode != NetmodeID.Server)
            {
                var length = ((projTex.Size() / new Vector2(FrameMax.X, FrameMax.Y)).Length() * Player.GetAdjustedItemScale(Player.HeldItem) - (new Vector2(0, projTex.Size().Y / FrameMax.Y) - DrawOrigin).Length());//
                var u = UltraSwoosh.NewUltraSwoosh(VertexColor, 15, length, Player.Center, HeatMap, false, 0, 1, angleRange: (Player.direction == 1 ? -1.125f : 2.125f, Player.direction == 1 ? 3f / 8 : 0.625f));//HeatMap
                u.ModityAllRenderInfo([useDistort], [useMask, useBloom]);
                u.weaponTex = TextureAssets.Item[Player.HeldItem.type].Value;
            }
        }
    }
    /// <summary>
    /// 武器手持弹幕对应的基类
    /// 以下是需要经常重写的属性
    /// Charged
    /// FrameMax
    /// Factor
    /// </summary>
    public abstract class HandMeleeProj : ModProjectile, IHammerProj
    {
        public virtual Vector2 scale => new(1);
        public virtual Rectangle? frame => null;
        public virtual Vector2 projCenter => Player.Center + new Vector2(0, Player.gfxOffY) + new Vector2(-8 * Player.direction, -3) + (Rotation - (Player.direction == -1 ? MathHelper.PiOver2 : 0)).ToRotationVector2() * 16;// 
        public Projectile projectile => Projectile;
        public virtual bool Charged => factor > 0.75f && controlState == 2;
        public virtual SpriteEffects flip => Player.direction == -1 ? SpriteEffects.FlipHorizontally : 0;
        public virtual (int X, int Y) FrameMax => (1, 1);
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault(ProjName);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.immune[projectile.owner] = 5;
        }
        public override void SetDefaults()
        {
            projectile.width = 1;
            projectile.height = 1;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
            projectile.scale = 1f;
            projectile.hide = true;
            projectile.ownerHitCheck = true;
            projectile.DamageType = DamageClass.Melee;
            projectile.tileCollide = false;
            projectile.friendly = true;
        }
        public virtual void OnEndAttack()
        {
            projectile.ai[0] = controlState == 2 ? MaxTime : 0;
            if (controlState == 1)
                controlTier++;
        }
        public virtual void OnCharging(bool left, bool right)
        {
            if (left)
            {
                projectile.ai[0]++;
                if (projectile.ai[0] >= MaxTime)
                {
                    OnEndAttack();
                }
            }
            else
            {
                projectile.ai[0] += projectile.ai[0] < MaxTime ? 1 : 0;
            }
            if ((int)projectile.ai[0] == MaxTime / 4 && left)
            {
                SoundEngine.PlaySound(SoundID.Item71);
            }
        }
        public virtual void OnRelease(bool charged, bool left)
        {
            if (left)
            {
                projectile.ai[0]++;
                if (projectile.ai[0] > MaxTime)
                {
                    OnEndAttack();
                    projectile.Kill();

                }
                if ((int)projectile.ai[0] == MaxTime / 4 && left)
                {
                    SoundEngine.PlaySound(SoundID.Item71);
                }
            }
            else
            {
                if (Charged)
                {
                    if ((int)projectile.ai[1] == 1)
                    {
                        OnChargedShoot();
                    }
                }
                if ((int)projectile.ai[1] == 0)
                {
                    projectile.damage = 0;
                    if (Charged)
                    {
                        SoundEngine.PlaySound(SoundID.Item71);

                        projectile.damage = (int)(Player.GetWeaponDamage(Player.HeldItem) * (3 * factor * factor));
                    }
                }
                projectile.ai[1]++;
                if (projectile.ai[1] > MaxTimeLeft)
                {
                    if (charged)
                        OnEndAttack();
                    projectile.Kill();
                }
            }

        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            //if ((int)projectile.ai[1] == 0)
            //{
            //    return false;
            //}

            if (controlState == 2 && Player.controlUseTile) return false;
            float point = 0f;
            var _rotation = Rotation;
            return targetHitbox.Intersects(Utils.CenteredRectangle((CollidingCenter - DrawOrigin).RotatedBy(_rotation) + projCenter, CollidingSize)) || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projCenter, (CollidingCenter - DrawOrigin).RotatedBy(_rotation) + projCenter, 8, ref point);
        }
        /// <summary>
        /// 面向右边时实际角度与x正半轴夹角
        /// </summary>
        public virtual float RealRotation
        {
            get
            {
                if (controlState == 1)
                {
                    //int tier = (int)(projectile.ai[0] / MaxTime);
                    float _factor = factor;
                    //if (tier % 2 == 1) _factor = 1 - _factor;
                    return MathHelper.SmoothStep(MathHelper.Pi * 7 / 8, -MathHelper.Pi * 3 / 8, _factor);//
                }
                else if (controlState == 2)
                {
                    float _factor = factor;
                    var theta = ((float)Math.Pow(_factor, 2)).Lerp(-MathHelper.Pi * 3 / 8, MathHelper.Pi * 7 / 8);
                    if (projectile.ai[1] > 0)
                    {
                        if (Charged)
                        {
                            //Main.NewText(projectile.ai[1] / MaxTimeLeft / factor);
                            theta = (projectile.ai[1] / MaxTimeLeft / _factor).Lerp(theta, -MathHelper.Pi * 3 / 8);
                            //return player.direction == -1 ? MathHelper.Pi * 1.5f - theta : theta;
                        }
                        else
                        {
                            theta = ((timeCount - projectile.ai[1]) / MaxTime).Lerp(-MathHelper.Pi * 3 / 8, theta);
                            //return player.direction == -1 ? MathHelper.Pi * 1.5f - theta : theta;
                        }
                    }
                    return theta;
                }
                return 0;

            }
        }
        /// <summary>
        /// 绘制用Rotation属性
        /// </summary>
        public virtual float Rotation
        {
            get
            {
                var rotation = -RealRotation + MathHelper.PiOver4;
                return Player.direction == -1 ? MathHelper.PiOver2 * 3 - rotation : rotation;
            }
        }
        public Player Player => Main.player[projectile.owner];

        public virtual float timeCount
        {
            get => projectile.ai[0];//controlState == 2 ? MathHelper.Clamp(projectile.ai[0], 0, MaxTime) : projectile.ai[0] % MaxTime
            set => projectile.ai[0] = value;
        }
        public Texture2D projTex => TextureAssets.Projectile[projectile.type].Value;
        public virtual string ProjName => "做个弹幕";
        public virtual float MaxTime => 15;
        public virtual float factor => timeCount / MaxTime;
        public virtual Vector2 CollidingSize => new(32);
        public virtual Vector2 CollidingCenter => new(size.X / FrameMax.X - 16, 16);
        public virtual Vector2 DrawOrigin => new(16, size.Y / FrameMax.Y - 16);
        public Vector2 size;
        public override void SendExtraAI(BinaryWriter writer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                writer.WriteVector2(size);
            base.SendExtraAI(writer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            if (Main.netMode == NetmodeID.Server)
                size = reader.ReadVector2();
            base.ReceiveExtraAI(reader);
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.Server)
                size = projTex.Size();
            base.OnSpawn(source);
        }
        public virtual Color color => /*projectile.GetAlpha(Color.White);*/Lighting.GetColor((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16, Color.White);
        public virtual float MaxTimeLeft => 5;
        public virtual bool UseLeft => true;
        public virtual bool UseRight => true;
        public virtual bool Charging => (UseLeft && Player.controlUseItem) || (UseRight && Player.controlUseTile) && projectile.ai[1] == 0;
        public override void AI()
        {

            //Projectiles.KluexEnergyCrystal.KluexEnergyZone
            if (Player.dead) projectile.Kill();
            if (Charging && projectile.ai[1] == 0)
            {
                OnCharging(Player.controlUseItem, Player.controlUseTile);
                if (Player.controlUseItem)
                {
                    controlState = 1;
                }
                if (Player.controlUseTile)
                {
                    controlState = 2;
                }
            }
            else
            {
                OnRelease(Charged, controlState == 1);
            }
            projectile.timeLeft = 2;
            Player.heldProj = projectile.whoAmI;
            Player.RotatedRelativePoint(Player.MountedCenter, true);
            Player.itemTime = 2;
            Player.itemAnimation = 2;
            Player.ChangeDir(Math.Sign((Player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition - projCenter).X));
            Player.SetCompositeArmFront(enabled: true, Player.CompositeArmStretchAmount.Full, Rotation - (Player.direction == -1 ? MathHelper.Pi : MathHelper.PiOver2));// -MathHelper.PiOver2
            projectile.velocity = (Player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition - projCenter).SafeNormalize(default);
            projectile.Center = Player.Center + new Vector2(0, Player.gfxOffY);

        }
        public byte controlState;
        public byte controlTier;
        public override bool PreDraw(ref Color lightColor)
        {
            if (size == default)
                size = projTex.Size();
            Main.spriteBatch.DrawHammer(this);
            //Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, projCenter - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Red, 0, new Vector2(.5f), 4f, 0, 0);
            //Main.spriteBatch.DrawLine(Player.Center, MathHelper.PiOver4.ToRotationVector2() * 32, Color.Purple, 4, true, -Main.screenPosition);
            //Main.spriteBatch.DrawLine(Player.Center, (MathHelper.PiOver4 * 3).ToRotationVector2() * 32, Color.Purple, 4, true, -Main.screenPosition);
            //Main.spriteBatch.DrawLine(Player.Center, RealRotation.ToRotationVector2() * 32, Color.Cyan, 4, true, -Main.screenPosition);
            //Main.spriteBatch.DrawLine(Player.Center, Rotation.ToRotationVector2() * 32, Color.Yellow, 4, true, -Main.screenPosition);
            //Main.spriteBatch.DrawLine(Player.Center + new Vector2(-8 * Player.direction, -3), (Rotation - (Player.direction == -1 ? MathHelper.PiOver2 : 0)).ToRotationVector2() * 32, Color.Green, 4, true, -Main.screenPosition);

            return false;
        }
        public virtual void OnChargedShoot()
        {
        }
    }
    /*public abstract class MeleeSequenceProj : ModProjectile, IHammerProj ,IChannelProj
    {

        public MeleeSequence MeleeSequenceData 
        {
            get => meleeSequence;
            init => SetUpSequence(meleeSequence);
        }
        public IMeleeAttackData currentData => MeleeSequenceData.currentData;

        public virtual Vector2 CollidingSize => throw new NotImplementedException();

        public virtual Vector2 CollidingCenter => throw new NotImplementedException();

        public virtual Vector2 DrawOrigin => currentData.offsetOrigin * projTex.Size();

        public virtual Texture2D projTex => TextureAssets.Projectile[Type].Value;
        public virtual Texture2D GlowEffect
        {
            get
            {
                if (ModContent.HasAsset(GlowTexture))
                {
                    return ModContent.Request<Texture2D>(GlowTexture).Value;
                }
                return null;
            }
        }
        public virtual Vector2 projCenter => currentData.offsetCenter + Player.Center + new Vector2(0, Player.gfxOffY);

        public virtual Rectangle? frame => null;

        public virtual Color color => Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, Color.White);

        public virtual float Rotation => currentData.Rotation;

        public virtual Vector2 scale => Vector2.One;

        public virtual SpriteEffects flip => default;
        public virtual Color GlowColor => Color.White;


        public (int X, int Y) FrameMax => (1,1);

        public Player Player => Main.player[Projectile.owner];

        public virtual bool Charging => true;

        public virtual bool Charged => throw new NotImplementedException();

        public abstract void SetUpSequence(MeleeSequence meleeSequence);
        MeleeSequence meleeSequence = new MeleeSequence();
        
        public sealed override void SetDefaults()
        {
            Projectile.timeLeft = 10;
            Projectile.width = Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 2;
            base.SetDefaults();
        }
        public override void AI()
        {
            Player.heldProj = Projectile.whoAmI;
            Projectile.damage = Player.GetWeaponDamage(Player.HeldItem);
            if (Player.controlUseItem || currentData == null || meleeSequence.timer > 0)
            {
                meleeSequence.Update(Player);
                Projectile.timeLeft = 2;
            }
            Projectile.Center = Player.Center + currentData.offsetCenter;

            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (GlowEffect != null)
            {
                Main.spriteBatch.DrawHammer(this, GlowEffect, GlowColor, frame);
            }
            else Main.spriteBatch.DrawHammer(this);

            /*SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value,
                player.Center - Main.screenPosition + currentData.offsetCenter,
                null, Color.White, currentData.Rotation + MathHelper.PiOver4,
                currentData.offsetOrigin * TextureAssets.Projectile[Type].Size(), currentData.ModifyData.actionOffsetSize, 0, 0);
            //
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!currentData.Attacktive) return false;
            return this.HammerCollide(targetHitbox);


            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + currentData.Rotation.ToRotationVector2() * currentData.ModifyData.actionOffsetSize * TextureAssets.Projectile[Projectile.type].Size().Length(), 48f, ref point);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.Knockback *= meleeSequence.currentData.ModifyData.actionOffsetKnockBack;
            target.immune[Player.whoAmI] = 0;
            base.ModifyHitNPC(target, ref modifiers);
        }

        public void OnCharging(bool left, bool right)
        {
            throw new NotImplementedException();
        }

        public void OnRelease(bool charged, bool left)
        {
            throw new NotImplementedException();
        }
    }*/

    public abstract class GlowItem : ModItem
    {
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            string glowPath = Texture + "_Glow";
            if (ModContent.HasAsset(glowPath) && ModContent.Request<Texture2D>(glowPath) is Asset<Texture2D> texture)
                spriteBatch.Draw(texture.Value, Item.Center - Main.screenPosition, null, Color.White, rotation, texture.Size() * .5f, scale, 0, 0);
        }
    }

    /// <summary>
    /// 简化使用的<see cref = "ModTileEntity"/>
    /// </summary>
    /// <typeparam name="T">对应绑定物块的类型</typeparam>
    public abstract class LModTileEntity<T> : ModTileEntity where T : ModTile
    {
        /// <summary>
        /// 必须和tileObjectData那边一样
        /// </summary>
        public abstract Point16 Origin { get; }
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<T>();
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Sync the entire multitile's area.  Modify "width" and "height" to the size of your multitile in tiles
                int width = 2;
                int height = 2;
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);

                // Sync the placement of the tile entity with other clients
                // The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
            }

            // ModTileEntity.Place() handles checking if the entity can be placed, then places it for you
            // Set "tileOrigin" to the same value you set TileObjectData.newTile.Origin to in the ModTile
            Point16 tileOrigin = Origin;
            int placedEntity = Place(i - tileOrigin.X, j - tileOrigin.Y);
            return placedEntity;
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
            }
        }
    }
    //[CustomModConfigItem(typeof(AvailableConfigElement))]
    /// <summary>
    /// 因为一些讨厌的原因，必须用上<see cref="CustomModConfigItemAttribute"/>在相应的字段/属性处
    /// </summary>
    public interface IAvailabilityChangableConfig
    {
        public bool Available { get; set; }
    }
    /// <summary>
    /// 请不要把这玩意和<see cref="SeparatePageAttribute"/>一起用，目前会出冲突(x
    /// </summary>
    public class AvailableConfigElement : ConfigElement<IAvailabilityChangableConfig>
    {
        protected Func<string> AbridgedTextDisplayFunction { get; set; }

        private readonly bool ignoreSeparatePage;
        //private SeparatePageAttribute separatePageAttribute;
        //private object data;
        private bool separatePage;
        private bool pendingChanges;
        private bool expanded = true;
        private NestedUIList dataList;
        private UIModConfigHoverImage initializeButton;
        private UIModConfigHoverImage deleteButton;
        private UIModConfigHoverImage expandButton;
        private UIPanel separatePagePanel;
        private UITextPanel<FuncStringWrapper> separatePageButton;

        // Label:
        //  Members
        //  Members
        public AvailableConfigElement()
        {
        }

        public override void OnBind()
        {
            base.OnBind();

            if (List != null)
            {
                // TODO: only do this if ToString is overriden.

                var listType = MemberInfo.Type.GetGenericArguments()[0];

                System.Reflection.MethodInfo methodInfo = listType.GetMethod("ToString", Array.Empty<Type>());
                bool hasToString = methodInfo != null && methodInfo.DeclaringType != typeof(object);

                if (hasToString)
                {
                    TextDisplayFunction = () => Index + 1 + ": " + (List[Index]?.ToString() ?? "null");
                    AbridgedTextDisplayFunction = () => (List[Index]?.ToString() ?? "null");
                }
                else
                {
                    TextDisplayFunction = () => Index + 1 + ": ";
                }
            }
            else
            {
                bool hasToString = MemberInfo.Type.GetMethod("ToString", Array.Empty<Type>()).DeclaringType != typeof(object);

                if (hasToString)
                {
                    TextDisplayFunction = () => Label + (Value == null ? "" : ": " + Value.ToString());
                    AbridgedTextDisplayFunction = () => Value?.ToString() ?? "";
                }
            }

            // Null values without AllowNullAttribute aren't allowed, but could happen with modder mistakes, so not automatically populating will hint to modder the issue.
            if (Value == null && List != null)
            {
                // This should never actually happen, but I guess a bad Json file could.
                object data = Activator.CreateInstance(MemberInfo.Type, true);
                string json = JsonDefaultValueAttribute?.Json ?? "{}";

                JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

                Value = (IAvailabilityChangableConfig)data;
            }

            separatePage = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SeparatePageAttribute>(MemberInfo, Item, List) != null;

            //separatePage = separatePage && !ignoreSeparatePage;
            //separatePage = (SeparatePageAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(SeparatePageAttribute)) != null;

            if (separatePage && !ignoreSeparatePage)
            {
                // TODO: UITextPanel doesn't update...
                separatePageButton = new UITextPanel<FuncStringWrapper>(new FuncStringWrapper(TextDisplayFunction));
                separatePageButton.HAlign = 0.5f;
                //e.Recalculate();
                //elementHeight = (int)e.GetOuterDimensions().Height;
                separatePageButton.OnLeftClick += (a, c) =>
                {
                    UIModConfig.SwitchToSubConfig(this.separatePagePanel);
                    /*	Interface.modConfig.uIElement.RemoveChild(Interface.modConfig.configPanelStack.Peek());
                        Interface.modConfig.uIElement.Append(separateListPanel);
                        Interface.modConfig.configPanelStack.Push(separateListPanel);*/
                    //separateListPanel.SetScrollbar(Interface.modConfig.uIScrollbar);

                    //UIPanel panel = new UIPanel();
                    //panel.Width.Set(200, 0);
                    //panel.Height.Set(200, 0);
                    //panel.Left.Set(200, 0);
                    //panel.Top.Set(200, 0);
                    //Interface.modConfig.Append(panel);

                    //Interface.modConfig.subMenu.Enqueue(subitem);
                    //Interface.modConfig.DoMenuModeState();
                };
                //e = new UIText($"{memberInfo.Name} click for more ({type.Name}).");
                //e.OnLeftClick += (a, b) => { };
            }

            //data = _GetValue();// memberInfo.GetValue(this.item);
            //drawLabel = false;

            if (List == null)
            {
                // Member > Class
                var expandAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ExpandAttribute>(MemberInfo, Item, List);
                if (expandAttribute != null)
                    expanded = expandAttribute.Expand;
            }
            else
            {
                // ListMember's ExpandListElements > Class
                var listType = MemberInfo.Type.GetGenericArguments()[0];
                var expandAttribute = (ExpandAttribute)Attribute.GetCustomAttribute(listType, typeof(ExpandAttribute), true);
                if (expandAttribute != null)
                    expanded = expandAttribute.Expand;
                expandAttribute = (ExpandAttribute)Attribute.GetCustomAttribute(MemberInfo.MemberInfo, typeof(ExpandAttribute), true);
                if (expandAttribute != null && expandAttribute.ExpandListElements.HasValue)
                    expanded = expandAttribute.ExpandListElements.Value;
            }

            dataList = [];
            dataList.Width.Set(-14, 1f);
            dataList.Left.Set(14, 0f);
            dataList.Height.Set(-30, 1f);
            dataList.Top.Set(30, 0);
            dataList.ListPadding = 5f;

            if (expanded)
                Append(dataList);

            //string name = memberInfo.Name;
            //if (labelAttribute != null) {
            //	name = labelAttribute.Label;
            //}
            if (List == null)
            {
                // drawLabel = false; TODO uncomment
            }

            initializeButton = new UIModConfigHoverImage(PlayTexture, "Initialize");
            initializeButton.Top.Pixels += 4;
            initializeButton.Left.Pixels -= 3;
            initializeButton.HAlign = 1f;
            initializeButton.OnLeftClick += (a, b) =>
            {
                SoundEngine.PlaySound(SoundID.Tink);

                object data = Activator.CreateInstance(MemberInfo.Type, true);
                string json = JsonDefaultValueAttribute?.Json ?? "{}";

                JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

                Value = (IAvailabilityChangableConfig)data;

                //SeparatePageAttribute here?

                pendingChanges = true;
                //RemoveChild(initializeButton);
                //Append(deleteButton);
                //Append(expandButton);

                SetupList();
                Interface.modConfig.RecalculateChildren();
                Interface.modConfig.SetPendingChanges();
            };
            expandButton = new UIModConfigHoverImage(expanded ? ExpandedTexture : CollapsedTexture, expanded ? "Collapse" : "Expand");
            expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
            expandButton.Left.Set(-52, 1f);
            expandButton.OnLeftClick += (a, b) =>
            {
                expanded = !expanded;
                pendingChanges = true;
            };

            deleteButton = new UIModConfigHoverImage(DeleteTexture, "Clear");
            deleteButton.Top.Set(4, 0f);
            deleteButton.Left.Set(-25, 1f);
            deleteButton.OnLeftClick += (a, b) =>
            {
                Value = null;
                pendingChanges = true;

                SetupList();
                //Interface.modConfig.RecalculateChildren();
                Interface.modConfig.SetPendingChanges();
            };

            if (Value != null)
            {
                //Append(expandButton);
                //Append(deleteButton);
                SetupList();
            }
            else
            {
                Append(initializeButton);
                //sortedContainer.Append(initializeButton);
            }

            pendingChanges = true;
            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!pendingChanges)
                return;
            pendingChanges = false;
            DrawLabel = !separatePage || ignoreSeparatePage;

            RemoveChild(deleteButton);
            RemoveChild(expandButton);
            RemoveChild(initializeButton);
            RemoveChild(dataList);
            if (separatePage && !ignoreSeparatePage)
                RemoveChild(separatePageButton);
            if (Value == null)
            {
                Append(initializeButton);
                DrawLabel = true;
            }
            else
            {
                if (List == null && !(separatePage && ignoreSeparatePage) && NullAllowed)
                    Append(deleteButton);

                if (!separatePage || ignoreSeparatePage)
                {
                    if (!ignoreSeparatePage)
                        Append(expandButton);
                    if (expanded)
                    {
                        Append(dataList);
                        expandButton.HoverText = "Collapse";
                        expandButton.SetImage(ExpandedTexture);
                    }
                    else
                    {
                        RemoveChild(dataList);
                        expandButton.HoverText = "Expand";
                        expandButton.SetImage(CollapsedTexture);
                    }
                }
                else
                {
                    Append(separatePageButton);
                }
            }
        }

        private void SetupList()
        {
            dataList.Clear();

            object data = Value;

            if (data != null)
            {
                if (separatePage && !ignoreSeparatePage)
                {
                    separatePagePanel = UIModConfig.MakeSeparateListPanel(Item, data, MemberInfo, List, Index, AbridgedTextDisplayFunction);
                }
                else
                {
                    var variables = ConfigManager.GetFieldsAndProperties(data);
                    int order = 0;
                    {
                        PropertyInfo propInfo = data.GetType().GetProperty(nameof(IAvailabilityChangableConfig.Available), BindingFlags.Public | BindingFlags.Instance);
                        PropertyFieldWrapper availableProp = new(propInfo);
                        int top = 0;
                        UIModConfig.HandleHeader(dataList, ref top, ref order, availableProp);
                        var wrapped = UIModConfig.WrapIt(dataList, ref top, availableProp, data, order++);
                        wrapped.Item2.OnLeftClick += (e, element) =>
                        {
                            pendingChanges = true;
                            SetupList();
                        };
                    }
                    if (Value.Available)
                        foreach (PropertyFieldWrapper variable in variables)
                        {
                            if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) || variable.Name == nameof(IAvailabilityChangableConfig.Available))
                                continue;


                            int top = 0;

                            UIModConfig.HandleHeader(dataList, ref top, ref order, variable);

                            var wrapped = UIModConfig.WrapIt(dataList, ref top, variable, data, order++);
                            if (List != null)
                            {
                                //wrapped.Item1.Left.Pixels -= 20;
                                wrapped.Item1.Width.Pixels += 20;
                            }
                            else
                            {
                                //wrapped.Item1.Left.Pixels += 20;
                                //wrapped.Item1.Width.Pixels -= 20;
                            }
                        }
                }
            }
        }

        public override void Recalculate()
        {
            base.Recalculate();

            float defaultHeight = separatePage ? 40 : 30;
            float h = dataList.Parent != null ? dataList.GetTotalHeight() + defaultHeight : defaultHeight;

            Height.Set(h, 0f);

            if (Parent != null && Parent is UISortableElement)
            {
                Parent.Height.Set(h, 0f);
            }
        }
    }

    /// <summary>
    /// 请不要把这玩意和<see cref="SeparatePageAttribute"/>一起用，目前会出冲突(x
    /// </summary>
    public class GenericAvailableConfigElement : GenericConfigElement<IAvailabilityChangableConfig>
    {
        protected Func<string> AbridgedTextDisplayFunction { get; set; }

        private readonly bool ignoreSeparatePage;
        //private SeparatePageAttribute separatePageAttribute;
        //private object data;
        private bool separatePage;
        private bool pendingChanges;
        private bool expanded = true;
        private NestedUIList dataList;
        private UIModConfigHoverImage initializeButton;
        private UIModConfigHoverImage deleteButton;
        private UIModConfigHoverImage expandButton;
        private UIPanel separatePagePanel;
        private UITextPanel<FuncStringWrapper> separatePageButton;

        // Label:
        //  Members
        //  Members
        public GenericAvailableConfigElement()
        {
        }

        public override void OnBind()
        {
            base.OnBind();

            if (List != null)
            {
                // TODO: only do this if ToString is overriden.

                var listType = MemberInfo.Type.GetGenericArguments()[0];

                System.Reflection.MethodInfo methodInfo = listType.GetMethod("ToString", Array.Empty<Type>());
                bool hasToString = methodInfo != null && methodInfo.DeclaringType != typeof(object);

                if (hasToString)
                {
                    TextDisplayFunction = () => Index + 1 + ": " + (List[Index]?.ToString() ?? "null");
                    AbridgedTextDisplayFunction = () => (List[Index]?.ToString() ?? "null");
                }
                else
                {
                    TextDisplayFunction = () => Index + 1 + ": ";
                }
            }
            else
            {
                bool hasToString = MemberInfo.Type.GetMethod("ToString", Array.Empty<Type>()).DeclaringType != typeof(object);

                if (hasToString)
                {
                    TextDisplayFunction = () => Label + (Value == null ? "" : ": " + Value.ToString());
                    AbridgedTextDisplayFunction = () => Value?.ToString() ?? "";
                }
            }

            // Null values without AllowNullAttribute aren't allowed, but could happen with modder mistakes, so not automatically populating will hint to modder the issue.
            if (Value == null && List != null)
            {
                // This should never actually happen, but I guess a bad Json file could.
                object data = Activator.CreateInstance(MemberInfo.Type, true);
                string json = JsonDefaultValueAttribute?.Json ?? "{}";

                JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

                Value = (IAvailabilityChangableConfig)data;
            }

            separatePage = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SeparatePageAttribute>(MemberInfo, Item, List) != null;

            //separatePage = separatePage && !ignoreSeparatePage;
            //separatePage = (SeparatePageAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(SeparatePageAttribute)) != null;

            if (separatePage && !ignoreSeparatePage)
            {
                // TODO: UITextPanel doesn't update...
                separatePageButton = new UITextPanel<FuncStringWrapper>(new FuncStringWrapper(TextDisplayFunction));
                separatePageButton.HAlign = 0.5f;
                //e.Recalculate();
                //elementHeight = (int)e.GetOuterDimensions().Height;
                separatePageButton.OnLeftClick += (a, c) =>
                {
                    UIModConfig.SwitchToSubConfig(this.separatePagePanel);
                    /*	Interface.modConfig.uIElement.RemoveChild(Interface.modConfig.configPanelStack.Peek());
                        Interface.modConfig.uIElement.Append(separateListPanel);
                        Interface.modConfig.configPanelStack.Push(separateListPanel);*/
                    //separateListPanel.SetScrollbar(Interface.modConfig.uIScrollbar);

                    //UIPanel panel = new UIPanel();
                    //panel.Width.Set(200, 0);
                    //panel.Height.Set(200, 0);
                    //panel.Left.Set(200, 0);
                    //panel.Top.Set(200, 0);
                    //Interface.modConfig.Append(panel);

                    //Interface.modConfig.subMenu.Enqueue(subitem);
                    //Interface.modConfig.DoMenuModeState();
                };
                //e = new UIText($"{memberInfo.Name} click for more ({type.Name}).");
                //e.OnLeftClick += (a, b) => { };
            }

            //data = _GetValue();// memberInfo.GetValue(this.item);
            //drawLabel = false;

            if (List == null)
            {
                // Member > Class
                var expandAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ExpandAttribute>(MemberInfo, Item, List);
                if (expandAttribute != null)
                    expanded = expandAttribute.Expand;
            }
            else
            {
                // ListMember's ExpandListElements > Class
                var listType = MemberInfo.Type.GetGenericArguments()[0];
                var expandAttribute = (ExpandAttribute)Attribute.GetCustomAttribute(listType, typeof(ExpandAttribute), true);
                if (expandAttribute != null)
                    expanded = expandAttribute.Expand;
                expandAttribute = (ExpandAttribute)Attribute.GetCustomAttribute(MemberInfo.MemberInfo, typeof(ExpandAttribute), true);
                if (expandAttribute != null && expandAttribute.ExpandListElements.HasValue)
                    expanded = expandAttribute.ExpandListElements.Value;
            }

            dataList = [];
            dataList.Width.Set(-14, 1f);
            dataList.Left.Set(14, 0f);
            dataList.Height.Set(-30, 1f);
            dataList.Top.Set(30, 0);
            dataList.ListPadding = 5f;

            if (expanded)
                Append(dataList);

            //string name = memberInfo.Name;
            //if (labelAttribute != null) {
            //	name = labelAttribute.Label;
            //}
            if (List == null)
            {
                // drawLabel = false; TODO uncomment
            }

            initializeButton = new UIModConfigHoverImage(PlayTexture, "Initialize");
            initializeButton.Top.Pixels += 4;
            initializeButton.Left.Pixels -= 3;
            initializeButton.HAlign = 1f;
            initializeButton.OnLeftClick += (a, b) =>
            {
                SoundEngine.PlaySound(SoundID.Tink);

                object data = Activator.CreateInstance(MemberInfo.Type, true);
                string json = JsonDefaultValueAttribute?.Json ?? "{}";

                JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

                Value = (IAvailabilityChangableConfig)data;

                //SeparatePageAttribute here?

                pendingChanges = true;
                //RemoveChild(initializeButton);
                //Append(deleteButton);
                //Append(expandButton);

                SetupList();
                //Interface.modConfig.RecalculateChildren();
                InternalOnSetObject();
                //Interface.modConfig.SetPendingChanges();
            };
            expandButton = new UIModConfigHoverImage(expanded ? ExpandedTexture : CollapsedTexture, expanded ? "Collapse" : "Expand");
            expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
            expandButton.Left.Set(-52, 1f);
            expandButton.OnLeftClick += (a, b) =>
            {
                expanded = !expanded;
                pendingChanges = true;
            };

            deleteButton = new UIModConfigHoverImage(DeleteTexture, "Clear");
            deleteButton.Top.Set(4, 0f);
            deleteButton.Left.Set(-25, 1f);
            deleteButton.OnLeftClick += (a, b) =>
            {
                Value = null;
                pendingChanges = true;

                SetupList();
                //Interface.modConfig.RecalculateChildren();
                //Interface.modConfig.SetPendingChanges();
                InternalOnSetObject();
            };

            if (Value != null)
            {
                //Append(expandButton);
                //Append(deleteButton);
                SetupList();
            }
            else
            {
                Append(initializeButton);
                //sortedContainer.Append(initializeButton);
            }

            pendingChanges = true;
            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!pendingChanges)
                return;
            pendingChanges = false;
            DrawLabel = !separatePage || ignoreSeparatePage;

            RemoveChild(deleteButton);
            RemoveChild(expandButton);
            RemoveChild(initializeButton);
            RemoveChild(dataList);
            if (separatePage && !ignoreSeparatePage)
                RemoveChild(separatePageButton);
            if (Value == null)
            {
                Append(initializeButton);
                DrawLabel = true;
            }
            else
            {
                if (List == null && !(separatePage && ignoreSeparatePage) && NullAllowed)
                    Append(deleteButton);

                if (!separatePage || ignoreSeparatePage)
                {
                    if (!ignoreSeparatePage)
                        Append(expandButton);
                    if (expanded)
                    {
                        Append(dataList);
                        expandButton.HoverText = "Collapse";
                        expandButton.SetImage(ExpandedTexture);
                    }
                    else
                    {
                        RemoveChild(dataList);
                        expandButton.HoverText = "Expand";
                        expandButton.SetImage(CollapsedTexture);
                    }
                }
                else
                {
                    Append(separatePageButton);
                }
            }
        }

        private void SetupList()
        {
            dataList.Clear();

            object data = Value;

            if (data != null)
            {
                if (separatePage && !ignoreSeparatePage)
                {
                    separatePagePanel = UIModConfig.MakeSeparateListPanel(Item, data, MemberInfo, List, Index, AbridgedTextDisplayFunction);
                }
                else
                {
                    var variables = ConfigManager.GetFieldsAndProperties(data);
                    int order = 0;
                    {
                        PropertyInfo propInfo = data.GetType().GetProperty(nameof(IAvailabilityChangableConfig.Available), BindingFlags.Public | BindingFlags.Instance);
                        PropertyFieldWrapper availableProp = new(propInfo);
                        int top = 0;
                        UIModConfig.HandleHeader(dataList, ref top, ref order, availableProp);
                        var wrapped = GenericConfigElement.WrapIt(dataList, ref top, availableProp, data, order++, onSetObj: OnSetObjectDelegate, owner: Owner);
                        wrapped.Item2.OnLeftClick += (e, element) =>
                        {
                            pendingChanges = true;
                            SetupList();
                        };
                    }
                    if (Value.Available)
                        foreach (PropertyFieldWrapper variable in variables)
                        {
                            if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) || variable.Name == nameof(IAvailabilityChangableConfig.Available))
                                continue;


                            int top = 0;

                            UIModConfig.HandleHeader(dataList, ref top, ref order, variable);

                            var wrapped = GenericConfigElement.WrapIt(dataList, ref top, variable, data, order++, onSetObj: OnSetObjectDelegate, owner: Owner);
                            if (List != null)
                            {
                                //wrapped.Item1.Left.Pixels -= 20;
                                wrapped.Item1.Width.Pixels += 20;
                            }
                            else
                            {
                                //wrapped.Item1.Left.Pixels += 20;
                                //wrapped.Item1.Width.Pixels -= 20;
                            }
                        }
                }
            }
        }

        public override void Recalculate()
        {
            base.Recalculate();

            float defaultHeight = separatePage ? 40 : 30;
            float h = dataList.Parent != null ? dataList.GetTotalHeight() + defaultHeight : defaultHeight;

            Height.Set(h, 0f);

            if (Parent != null && Parent is UISortableElement)
            {
                Parent.Height.Set(h, 0f);
            }
        }
    }
}

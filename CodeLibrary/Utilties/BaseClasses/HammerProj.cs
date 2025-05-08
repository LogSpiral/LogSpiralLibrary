using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingEffects;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using static LogSpiralLibrary.LogSpiralLibraryMod;

namespace LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
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
        if (projectile.ai[1] > (Charged ? MaxTimeLeft * Factor : timeCount))
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
    string CanvasName => FullName.Replace("/", ":");
    public override void Load()
    {
        RenderCanvasSystem.RegisterCanvasFactory(CanvasName, () => new RenderingCanvas([[UseDistort], [UseMask, UseBloom]]));
        base.Load();
    }
    BloomEffect UseBloom => field ??= new BloomEffect();
    AirDistortEffect UseDistort => field ??= new AirDistortEffect();
    MaskEffect UseMask => field ??= new MaskEffect();
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
    public virtual void RenderInfomation(BloomEffect useBloom, AirDistortEffect useDistort, MaskEffect useMask) { }
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
        float endAngle = Player.direction == -1 ? MathHelper.Pi / 8 : -MathHelper.PiOver2 - MathHelper.Pi / 8;
        bool useHeatMap = HeatMap != null;

        RenderInfomation(UseBloom, UseDistort, UseMask);
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
        if ((UseBloom.Active || UseDistort.Active || UseMask.Active) && CanUseRender)
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

            if (UseDistort.Active)
            {
                sb.End();
                gd.SetRenderTarget(Instance.Render_Swap);
                gd.Clear(Color.Transparent);
                sb.Begin(SpriteSortMode.Immediate, additive ? BlendState.Additive : BlendState.NonPremultiplied, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);//Main.DefaultSamplerState//Main.GameViewMatrix.TransformationMatrix
                                                                                                                                                                                            //CoolerItemVisualEffect.ShaderSwooshEX.Parameters["lightShift"].SetValue(0);
                swooshUL.Parameters["distortScaler"].SetValue(UseDistort.Intensity);
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
                    triangleList[n].Position = (triangleList[n].Position - Player.Center) * UseDistort.Intensity + Player.Center;
                }
                //TODO 对color魔改
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList, 0, bars.Length - 2);
                Main.graphics.GraphicsDevice.RasterizerState = originalState;
            }
            sb.End();
            //然后在随便一个render里绘制屏幕，并把上面那个带弹幕的render传进shader里对屏幕进行处理
            //原版自带的screenTargetSwap就是一个可以使用的render，（原版用来连续上滤镜）
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            if (UseDistort.Active)
            {
                gd.SetRenderTarget(Main.screenTargetSwap);//将画布设置为这个 
                gd.Clear(Color.Transparent);//清空
                Main.instance.GraphicsDevice.Textures[2] = Misc[18].Value;
                LogSpiralLibraryMod.AirDistortEffect.Parameters["uScreenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                LogSpiralLibraryMod.AirDistortEffect.Parameters["strength"].SetValue(5f);
                LogSpiralLibraryMod.AirDistortEffect.Parameters["rotation"].SetValue(Matrix.Identity);//MathHelper.Pi * Main.GlobalTimeWrappedHourly
                LogSpiralLibraryMod.AirDistortEffect.Parameters["tex0"].SetValue(Instance.Render_Swap);
                LogSpiralLibraryMod.AirDistortEffect.Parameters["colorOffset"].SetValue(0f);
                LogSpiralLibraryMod.AirDistortEffect.CurrentTechnique.Passes[0].Apply();//ApplyPass 
                                                                                        //0    1     2
                                                                                        //.001 .0035 .005
                sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);//绘制原先屏幕内容
                gd.SetRenderTarget(Main.screenTarget);
                gd.Clear(Color.Transparent);
                sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            }
            if (UseMask.Active)
            {

                gd.SetRenderTarget(Main.screenTargetSwap);
                gd.Clear(Color.Transparent);
                sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                gd.SetRenderTarget(Instance.Render_Swap);
                gd.Clear(Color.Transparent);
                Main.graphics.GraphicsDevice.Textures[1] = UseMask.FillTex;
                //RenderEffect.Parameters["tex0"].SetValue(render);
                RenderEffect.Parameters["invAlpha"].SetValue(UseMask.Tier1);
                RenderEffect.Parameters["lightAsAlpha"].SetValue(UseMask.LightAsAlpha);
                RenderEffect.Parameters["tier2"].SetValue(UseMask.Tier2);
                RenderEffect.Parameters["position"].SetValue(UseMask.Offset);
                RenderEffect.Parameters["maskGlowColor"].SetValue(UseMask.GlowColor.ToVector4());
                RenderEffect.Parameters["ImageSize"].SetValue(UseMask.FillTex.Size());
                RenderEffect.Parameters["inverse"].SetValue(UseMask.Inverse);
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
            if (UseBloom.Active)
            {
                gd.SetRenderTarget(Main.screenTargetSwap);
                gd.Clear(Color.Transparent);
                sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                RenderEffect.Parameters["screenScale"].SetValue(Main.ScreenSize.ToVector2());
                RenderEffect.Parameters["threshold"].SetValue(UseBloom.Threshold);
                RenderEffect.Parameters["range"].SetValue(UseBloom.Range);
                RenderEffect.Parameters["intensity"].SetValue(UseBloom.Intensity * 2.5f);
                RenderEffect.Parameters["uBloomAdditive"].SetValue(UseBloom.Additive);
                for (int n = 0; n < UseBloom.Count - 1; n++)
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
                sb.Draw(UseBloom.Count == 1 ? render : Instance.Render_Swap2, Vector2.Zero, Color.White);
                gd.SetRenderTarget(UseBloom.Count == 1 ? render : Instance.Render_Swap2);
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

            if (UseBloom.Active)
            {
                Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
                sb.Draw(UseBloom.Count == 1 ? render : Instance.Render_Swap2, Vector2.Zero, Color.White);
            }
            Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
        else
        {
            // 否则使用普通绘制

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, additive ? BlendState.Additive : BlendState.NonPremultiplied, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);//Main.DefaultSamplerState//Main.GameViewMatrix.TransformationMatrix

            swooshUL.Parameters["uTransform"].SetValue(RenderCanvasSystem.uTransform);
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
            var length = (projTex.Size() / new Vector2(FrameMax.X, FrameMax.Y)).Length() * Player.GetAdjustedItemScale(Player.HeldItem) - (new Vector2(0, projTex.Size().Y / FrameMax.Y) - DrawOrigin).Length();//

            var u = UltraSwoosh.NewUltraSwoosh(CanvasName, 15, length, Player.Center, (Player.direction == 1 ? -1.125f : 2.125f, Player.direction == 1 ? 3f / 8 : 0.625f));
            u.heatMap = heatMap;
            u.xScaler = 1f;
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
    public virtual bool Charging => UseLeft && Player.controlUseItem || UseRight && Player.controlUseTile && projectile.ai[1] == 0;
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


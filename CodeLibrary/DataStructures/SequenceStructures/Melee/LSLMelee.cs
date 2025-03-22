using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Terraria.Audio;
using Terraria.ModLoader.Config;
using Terraria.ID;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Melee
{
    public abstract class LSLMelee : MeleeAction
    {
        public override string Category => "LsLibrary";
    }
    public class SwooshInfo : LSLMelee
    {
        int hitCounter;
        public enum SwooshMode
        {
            Chop,
            Slash,
            //Storm
        }
        public override void NetReceive(BinaryReader reader)
        {
            Rotation = reader.ReadSingle();
            KValue = reader.ReadSingle();
            base.NetReceive(reader);
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Rotation);
            writer.Write(KValue);
            base.NetSend(writer);
        }
        public override float Factor => base.Factor;

        //public virtual bool useTransition => false;
        float TimeToAngle(float t)
        {
            float max = timerMax;
            var fac = t / max;
            if (max > cutTime * 1.5f)
            {
                float tier2 = (max - cutTime) * k;
                float tier1 = tier2 + cutTime;
                if (t > tier1)
                    fac = MathHelper.SmoothStep(mode == SwooshMode.Chop ? 160 / 99f : 1, 1.125f, Utils.GetLerpValue(max, tier1, t, true));
                else if (t < tier2)
                    fac = 0;
                else
                    fac = MathHelper.SmoothStep(0, 1.125f, Utils.GetLerpValue(tier2, tier1, t, true));

            }
            else
                fac = MathHelper.SmoothStep(-.125f, 1.25f, fac);

            fac = flip ? 1 - fac : fac;
            float start = -.75f;
            float end = .625f;
            return MathHelper.Lerp(end, start, fac) * MathHelper.Pi;
        }

        [ElementCustomData]
        //[CustomSeqConfigItem(typeof(SeqEnumElement))]
        [DrawTicks]
        public SwooshMode mode;
        int cutTime => 8;
        float k => 0.25f;
        public override float offsetRotation => TimeToAngle(fTimer);
        public override float offsetSize => base.offsetSize;
        public override float offsetDamage => MathF.Pow(.75f, hitCounter);
        public override bool Attacktive
        {
            get
            {
                float t = (timerMax - cutTime) * k;
                return fTimer > t && fTimer < t + cutTime;
            }
        }

        static void ShootProjCall(Player plr, int dmg)
        {
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, dmg);
        }
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound((standardInfo.soundStyle ?? MySoundID.Scythe) with { MaxInstances = -1 }, Owner?.Center);
            if (Owner is Player plr)
            {
                SequencePlayer seqPlayer = plr.GetModPlayer<SequencePlayer>();

                int dmg = CurrentDamage;
                if (standardInfo.standardShotCooldown > 0)
                {
                    float delta = standardInfo.standardTimer * ModifyData.actionOffsetTimeScaler / Cycle;
                    bool canShoot = plr.HeldItem.shoot > ProjectileID.None;

                    float m = Math.Max(standardInfo.standardShotCooldown, delta);
                    if (canShoot || seqPlayer.cachedTime < m)
                        seqPlayer.cachedTime += delta + 1;
                    if (seqPlayer.cachedTime > m)
                        seqPlayer.cachedTime = m;
                    int count = (int)(seqPlayer.cachedTime / standardInfo.standardShotCooldown);
                    if (canShoot)
                    {
                        seqPlayer.cachedTime -= standardInfo.standardShotCooldown * count;
                        if (count > 0)
                        {
                            count--;
                            ShootProjCall(plr, dmg);
                        }
                    }

                    Vector2 orig = Main.MouseWorld;
                    Vector2 unit = (orig - plr.Center);//.SafeNormalize(default) * 32f;
                    float angleMax = MathHelper.Pi / 6;
                    if (count % 2 == 1)
                    {
                        count--;
                        Vector2 target = plr.Center + unit.RotatedBy(angleMax * Main.rand.NextFloat(-.5f, .5f));
                        Main.mouseX = (int)(target.X - Main.screenPosition.X);
                        Main.mouseY = (int)(target.Y - Main.screenPosition.Y);
                        ShootProjCall(plr, dmg);

                    }
                    count /= 2;
                    for (int i = 0; i < count; i++)
                    {
                        float angle = angleMax * MathF.Pow((i + 1f) / count, 2);

                        Vector2 target = plr.Center + unit.RotatedBy(angle);
                        Main.mouseX = (int)(target.X - Main.screenPosition.X);
                        Main.mouseY = (int)(target.Y - Main.screenPosition.Y);
                        ShootProjCall(plr, dmg);


                        target = plr.Center + unit.RotatedBy(-angle);
                        Main.mouseX = (int)(target.X - Main.screenPosition.X);
                        Main.mouseY = (int)(target.Y - Main.screenPosition.Y);
                        ShootProjCall(plr, dmg);

                    }
                    Main.mouseX = (int)(orig.X - Main.screenPosition.X);
                    Main.mouseY = (int)(orig.Y - Main.screenPosition.Y);
                }
                else
                    ShootProjCall(plr, dmg);


            }
            base.OnStartAttack();
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();

            hitCounter = 0;
            Rotation += Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6);
            KValue = Main.rand.NextFloat(1, 2);
            //if (Projectile.owner == Main.myPlayer)
            //{
            //    Rotation += Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6);
            //    KValue = Main.rand.NextFloat(1, 2);
            //    Projectile.netImportant = true;
            //}
            if (mode == SwooshMode.Slash)
                flip ^= true;
            NewSwoosh();
        }
        public override void OnEndSingle()
        {
            hitCounter = 0;
            base.OnEndSingle();
        }
        public override void OnHitEntity(Entity victim, int damageDone, object[] context)
        {
            hitCounter++;
            base.OnHitEntity(victim, damageDone, context);
        }
        public override void OnDeactive()
        {
            if (mode == SwooshMode.Slash)
                flip ^= true;

            base.OnDeactive();
        }
        public override void OnActive()
        {
            flip = Owner.direction == -1;
            base.OnActive();
        }
        UltraSwoosh swoosh;
        UltraSwoosh subSwoosh;
        public void NewSwoosh()
        {
            var verS = standardInfo.vertexStandard;
            if (verS.active)
            {
                UltraSwoosh u = null;
                swoosh = subSwoosh = null;
                var range = mode switch
                {
                    SwooshMode.Chop => (.875f, -1f),
                    SwooshMode.Slash => (.625f, -.75f),
                    //SwooshMode.Storm or _ => (.625f, -.75f)
                };
                bool f = mode switch
                {
                    SwooshMode.Chop => !flip,
                    _ => flip
                };
                float size = verS.scaler * ModifyData.actionOffsetSize * offsetSize;
                var pair = standardInfo.vertexStandard.swooshTexIndex;
                if (standardInfo.itemType == ItemID.TrueExcalibur)
                {
                    var eVec = verS.colorVec with { Y = 0 };
                    if (eVec.X == 0 && eVec.Z == 0)
                        eVec = new(.5f, 0, .5f);
                    u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size * .67f, Owner.Center, verS.heatMap, f, Rotation, KValue, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                    subSwoosh = UltraSwoosh.NewUltraSwoosh(Color.Pink, (int)(verS.timeLeft * 1.2f), size, Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, f, Rotation, KValue, (range.Item1 + 0.125f, range.Item2 - 0.125f), pair?.Item1 ?? 3, pair?.Item2 ?? 7, eVec);
                    subSwoosh.ApplyStdValueToVtxEffect(standardInfo);
                    subSwoosh.heatRotation = 0;
                    subSwoosh.weaponTex = null;
                }
                else
                {
                    u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size, Owner.Center, verS.heatMap, f, Rotation, KValue, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                }
                if (verS.renderInfos == null)
                    u.ResetAllRenderInfo();
                else
                {
                    u.ModityAllRenderInfo(verS.renderInfos);
                }
                swoosh = u;
                u.ApplyStdValueToVtxEffect(standardInfo);
                u.weaponTex = null;
            }
            //return null;
        }
        public override void OnEndAttack()
        {
            //NewSwoosh();
            base.OnEndAttack();
        }
        void UpdateSwoosh(UltraSwoosh swoosh, (float, float) range)
        {
            if (swoosh == null)
                return;
            //swoosh.timeLeft = standardInfo.vertexStandard.timeLeft;
            //var fac = 0f;
            //switch (mode)
            //{
            //    case SwooshMode.Chop:
            //        {
            //            fac = Utils.GetLerpValue(MathHelper.Lerp(cutTime, timerMax, 1 - k), MathHelper.Lerp(cutTime, timerMax, k), timer, true);
            //            break;
            //        }
            //    default:
            //        {
            //            fac = timer > MathHelper.Lerp(cutTime, timerMax, k) ? 0 : 1;
            //            break;
            //        }
            //}

            //var fac = cutTime * 1.5f >= timerMax ? 1 : Utils.GetLerpValue(MathHelper.Lerp(cutTime, timerMax, 1 - k), MathHelper.Lerp(cutTime, timerMax, k), timer, true);
            ////Main.NewText((cutTime * 2>= timerMax, MathHelper.SmoothStep((1 - Factor) * 4f / 3f, 0, 1), fac));
            //if(mode == SwooshMode.Slash)
            //    fac = MathHelper.SmoothStep(0, 1, (1 - Factor) * 4 / 3f);
            //swoosh.timeLeft = (int)MathHelper.Lerp(1, standardInfo.vertexStandard.timeLeft, MathHelper.SmoothStep(0, 1, fac)) + 1;// 
            //swoosh.timeLeft = swoosh.timeLeftMax + 1;
            swoosh.center = Owner.Center;
            swoosh.rotation = Rotation;
            swoosh.negativeDir = flip;
            swoosh.angleRange = range;
            if (flip)
                swoosh.angleRange = (swoosh.angleRange.from, -swoosh.angleRange.to);
            swoosh.timeLeft = (int)(MathHelper.Clamp(MathF.Abs(swoosh.angleRange.Item1 - swoosh.angleRange.Item2), 0, 1) * swoosh.timeLeftMax) + 1;
        }
        public override void Update(bool triggered)
        {

            if (timer > (timerMax - cutTime) * k)
            {
                //UpdateSwoosh(swoosh, (offsetRotation / MathF.PI, mode == SwooshMode.Chop ? 0.625f : -0.75f));
                //UpdateSwoosh(subSwoosh, (offsetRotation / MathF.PI, mode == SwooshMode.Chop ? 0.75f : -0.875f));
                timer--;
                UpdateSwoosh(swoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : (-1.25f + .5f * Factor), offsetRotation / MathF.PI));
                UpdateSwoosh(subSwoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : (-1.25f + .5f * Factor), offsetRotation / MathF.PI));
                timer++;
            }
            else
            {
                swoosh = null;
                subSwoosh = null;

            }
            base.Update(triggered);
        }
        public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            base.Draw(spriteBatch, texture);
        }
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            return base.GetWeaponVertex(texture, alpha);
            //float origf = fTimer;
            //IEnumerable<CustomVertexInfo> result = [];
            //fTimer += 2.0f;
            //for (int i = 9; i >= 0; i--)
            //{
            //    fTimer -= .2f;
            //    result = result.Concat(base.GetWeaponVertex(texture, (1f - i / 10f) * (i == 0 ? 1f : .5f)));
            //}
            //fTimer = origf;
            //return result.ToArray();
        }
        public override void OnAttack()
        {
            for (int n = 0; n < 30 * (1 - Factor) * standardInfo.dustAmount; n++)
            {
                var Center = Owner.Center + offsetCenter + targetedVector * Main.rand.NextFloat(0.5f, 1f);//
                var velocity = -Owner.velocity * 2 + targetedVector.RotatedBy(MathHelper.PiOver2 * (flip ? -1 : 1) + Main.rand.NextFloat(-MathHelper.Pi / 12, MathHelper.Pi / 12)) * Main.rand.NextFloat(.125f, .25f);
                OtherMethods.FastDust(Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), velocity * .25f, standardInfo.standardColor);
            }
            base.OnAttack();
        }

    }
    public class StabInfo : LSLMelee
    {
        int hitCounter;
        public override void NetReceive(BinaryReader reader)
        {
            Rotation = reader.ReadSingle();
            KValue = reader.ReadSingle();
            base.NetReceive(reader);
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Rotation);
            writer.Write(KValue);
            base.NetSend(writer);
        }
        public override float offsetDamage => MathF.Pow(.75f, hitCounter);
        public override Vector2 offsetCenter => default;//new Vector2(64 * Factor, 0).RotatedBy(Rotation);
        public override Vector2 offsetOrigin => new Vector2(Factor * .4f, 0).RotatedBy(standardInfo.standardRotation);
        public override bool Attacktive => timer <= MathF.Sqrt(timerMax);
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.SwooshNormal_1, Owner?.Center);
            if (Owner is Player plr)
            {
                SequencePlayer seqPlr = plr.GetModPlayer<SequencePlayer>();
                int dmg = CurrentDamage;
                if (standardInfo.standardShotCooldown > 0)
                {
                    float delta = standardInfo.standardTimer * ModifyData.actionOffsetTimeScaler / Cycle;
                    seqPlr.cachedTime += delta + 1;
                    int count = (int)(seqPlr.cachedTime / standardInfo.standardShotCooldown);
                    seqPlr.cachedTime -= count * standardInfo.standardShotCooldown;
                    if (count > 0)
                    {
                        count--;
                        plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, dmg);
                    }
                    Vector2 orig = plr.Center;
                    Vector2 unit = (Main.MouseWorld - orig).SafeNormalize(default) * 64;
                    for (int i = 0; i < count; i++)
                    {
                        plr.Center += unit.RotatedBy(MathHelper.Pi / count * (i - (count - 1) * .5f));
                        plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, dmg);
                        plr.Center = orig;

                    }
                    if (count > 0)
                        if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
                        }
                }
                else
                    plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, dmg);
                for (int n = 0; n < 30; n++)
                {
                    if (Main.rand.NextFloat(0, 1) < standardInfo.dustAmount)
                        for (int k = 0; k < 2; k++)
                        {
                            var flag = k == 0;
                            var unit = ((MathHelper.TwoPi / 30 * n).ToRotationVector2() * new Vector2(1, .75f)).RotatedBy(Rotation) * (flag ? 2 : 1) * .5f;
                            var Center = Owner.Center + offsetCenter + targetedVector * .75f;
                            var velocity = -Owner.velocity * 2 + unit - targetedVector * .125f;
                            velocity *= 2;
                            OtherMethods.FastDust(Center, velocity, standardInfo.standardColor);

                        }
                }
            }
            base.OnStartAttack();
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            hitCounter = 0;
            KValue = Main.rand.NextFloat(1f, 2.4f);
            Rotation += Main.rand.NextFloat(0, Main.rand.NextFloat(0, MathHelper.Pi / 6)) * Main.rand.Next(new int[] { -1, 1 });
            //if (Projectile.owner == Main.myPlayer)
            //{
            //    KValue = Main.rand.NextFloat(1f, 2.4f);
            //    Rotation += Main.rand.NextFloat(0, Main.rand.NextFloat(0, MathHelper.Pi / 6)) * Main.rand.Next(new int[] { -1, 1 });
            //    Projectile.netImportant = true;
            //}

            flip ^= true;
        }
        public override void OnEndSingle()
        {
            hitCounter = 0;
            base.OnEndSingle();
        }
        public override void OnHitEntity(Entity victim, int damageDone, object[] context)
        {
            hitCounter++;
            base.OnHitEntity(victim, damageDone, context);
        }
        public virtual UltraStab NewStab()
        {
            var verS = standardInfo.vertexStandard;
            if (verS.active)
            {
                UltraStab u = null;
                var pair = standardInfo.vertexStandard.stabTexIndex;
                if (standardInfo.itemType == ItemID.TrueExcalibur)
                {
                    float size = verS.scaler * ModifyData.actionOffsetSize * offsetSize * 1.25f;
                    u = UltraStab.NewUltraStab(standardInfo.standardColor, (int)(verS.timeLeft * 1.2f), size,
                    Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, flip, Rotation, 2, pair?.Item1 ?? 9, pair?.Item2 ?? 0, colorVec: verS.colorVec);
                    var su = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, size * .67f,
                    Owner.Center + Rotation.ToRotationVector2() * size * .2f, verS.heatMap, !flip, Rotation, 2, pair?.Item1 ?? 9, pair?.Item2 ?? 0, colorVec: verS.colorVec);
                    su.ApplyStdValueToVtxEffect(standardInfo);
                }
                else
                {
                    u = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize * 1.25f,
                    Owner.Center, verS.heatMap, flip, Rotation, 2, pair?.Item1 ?? 9, pair?.Item2 ?? 0, colorVec: verS.colorVec);
                }
                //Main.NewText(Owner.Center);
                //var u = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize, Owner.Center, verS.heatMap, this.flip, Rotation, KValue, (.625f, -.75f), colorVec: verS.colorVec);
                if (verS.renderInfos == null)
                    u.ResetAllRenderInfo();
                else
                {
                    u.ModityAllRenderInfo(verS.renderInfos);
                }
                u.ApplyStdValueToVtxEffect(standardInfo);
                return u;
            }
            return null;
        }
        public override void OnEndAttack()
        {
            NewStab();
            //Projectile.NewProjectile(Owner.GetSource_FromThis(), Owner.Center, Rotation.ToRotationVector2() * 16, ProjectileID.TerraBeam, 100, 1, Owner.whoAmI);
            base.OnEndAttack();
        }
        public override float Factor
        {
            get
            {
                float k = MathF.Sqrt(timerMax);
                float max = timerMax;
                float t = timer;
                if (t >= k)
                {
                    return MathHelper.SmoothStep(1, 1.125f, Utils.GetLerpValue(max, k, t, true));
                }
                else
                {
                    //return MathHelper.SmoothStep(0, 1.125f, t / k);
                    return MathHelper.Hermite(0, -5, 1.125f, 0, t / k);

                }
                float fac = base.Factor;
                fac = 1 - fac;
                fac *= fac;
                return fac.CosFactor();
            }
        }
        public override void Update(bool triggered)
        {
            base.Update(triggered);
        }
    }
    public class RapidlyStabInfo : StabInfo
    {
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((byte)realCycle);
            base.NetSend(writer);
        }
        public override void NetReceive(BinaryReader reader)
        {
            realCycle = reader.ReadByte();
            base.NetReceive(reader);
        }
        public RapidlyStabInfo()
        {

        }
        public (int min, int max) CycleOffsetRange
        {
            get => (rangeOffsetMin, rangeOffsetMax);
            set
            {
                var v = value;
                v.min = Math.Clamp(v.min, 1 - givenCycle, v.max);
                v.max = Math.Clamp(v.max, v.min, int.MaxValue);
                (rangeOffsetMin, rangeOffsetMax) = v;
                ResetCycle();
            }
        }
        [ElementCustomData]
        [DefaultValue(-2)]
        [Range(-5, 5)]
        //[CustomSeqConfigItem(typeof(SeqIntInputElement))]
        [Slider]
        public int rangeOffsetMin = -2;
        [ElementCustomData]
        [DefaultValue(1)]
        [Range(-5, 5)]
        [Slider]
        //[CustomSeqConfigItem(typeof(SeqIntInputElement))]
        public int rangeOffsetMax = 1;
        [ElementCustomDataAbabdoned]
        public override int Cycle { get => realCycle; set => givenCycle = value; }
        public int realCycle;
        [ElementCustomData]
        //[CustomSeqConfigItem(typeof(SeqIntInputElement))]
        [Range(1, 10)]
        [DefaultValue(4)]
        [Slider]
        public int givenCycle = 4;
        void ResetCycle()
        {
            realCycle = Math.Clamp(rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);

            //if (Projectile.owner == Main.myPlayer)
            //{
            //    realCycle = rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : Math.Clamp(givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);
            //    Projectile.netImportant = true;
            //}

        }
        public override void OnActive()
        {
            ResetCycle();
            base.OnActive();
        }
    }
    public class ConvoluteInfo : LSLMelee
    {
        public override Vector2 offsetCenter => unit * Factor.CosFactor() * 512;
        public Vector2 unit;
        public override bool Attacktive => Factor >= .25f;
        public override float CompositeArmRotation => Owner.direction;
        public override float offsetRotation => Factor * MathHelper.TwoPi * 2 + (float)LogSpiralLibraryMod.ModTime2 * .025f;
        public override void OnStartSingle()
        {
            base.OnStartSingle();
            KValue = 1.5f;
            unit = Rotation.ToRotationVector2();
        }
        public override void OnAttack()
        {
            if ((int)LogSpiralLibraryMod.ModTime2 % 6 == 0)
                SoundEngine.PlaySound(MySoundID.BoomerangRotating, Owner?.Center);

            base.OnAttack();
        }
        public override void OnStartAttack()
        {
            if (Owner is Player plr)
            {
                plr.Center += offsetCenter;
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
                plr.Center -= offsetCenter;
            }
            base.OnStartAttack();
        }
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            var origf = fTimer;
            IEnumerable<CustomVertexInfo> result = base.GetWeaponVertex(texture, 1f);
            for (int i = 1; i < 10; i++)
            {
                fTimer += .2f;
                result = result.Concat(base.GetWeaponVertex(texture, 1f - i / 10f));
            }
            fTimer = origf;
            return result.ToArray();
        }
    }

    public class ChargingInfo : LSLMelee
    {
        public override float offsetRotation => Main.rand.NextFloat(-1, 1) * Main.rand.NextFloat(0, 1) * Factor * .5f + MathHelper.Lerp(StartRotation, ChargingRotation, MathHelper.SmoothStep(1, 0, MathF.Pow(Factor, 3))) * Owner.direction;
        public override bool Attacktive => timer == 1;
        public override float offsetSize => base.offsetSize;
        public override float CompositeArmRotation => base.CompositeArmRotation;
        [ElementCustomData]
        [Range(-MathF.PI, MathF.PI)]
        [Increment(0.1f)]
        public float ChargingRotation = 0;

        [ElementCustomData]
        [Range(-MathF.PI, MathF.PI)]
        [Increment(0.1f)]
        public float StartRotation = 0;
        [ElementCustomData]
        public bool AutoNext = false;
        public override void OnCharge()
        {
            base.OnCharge();
        }
        public override void Update(bool triggered)
        {
            standardInfo = standardInfo with { extraLight = 3 * MathF.Pow(1 - Factor, 4f) };
            flip = Owner.direction == 1;
            switch (Owner)
            {
                case Player player:
                    {
                        //SoundEngine.PlaySound(SoundID.Item71);
                        var tarpos = player.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition;
                        player.direction = Math.Sign(tarpos.X - player.Center.X);
                        Rotation = (tarpos - Owner.Center).ToRotation();
                        break;
                    }

            }
            base.Update(triggered);
            if (timer > 0)
                for (int n = 0; n < 4; n++)
                {
                    Vector2 unit = (MathHelper.PiOver2 * n + 4 * Factor).ToRotationVector2();
                    OtherMethods.FastDust(Owner.Center + unit * (MathF.Exp(Factor) - 1) * 128, default, standardInfo.standardColor, 2f);

                    OtherMethods.FastDust(Owner.Center + new Vector2(unit.X + unit.Y, -unit.X + unit.Y) * (MathF.Exp(Factor) - 1) * 128, default, standardInfo.standardColor, 1.5f);

                }
            if (timer == 1 && counter == Cycle)
            {
                timer = 0;
                SoundEngine.PlaySound(SoundID.Item84);
                for (int n = 0; n < 40; n++)
                {
                    OtherMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 32), standardInfo.standardColor, Main.rand.NextFloat(1, 4));
                    OtherMethods.FastDust(Owner.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4) + (Rotation + offsetRotation).ToRotationVector2() * Main.rand.NextFloat(0, 64), standardInfo.standardColor, Main.rand.NextFloat(1, 2));

                }
            }
            if (!AutoNext && timer < 2 && counter == Cycle)
            {

                if (triggered)
                    timer++;
                else
                    switch (Owner)
                    {
                        case Player plr:
                            SequencePlayer mplr = plr.GetModPlayer<SequencePlayer>();
                            mplr.PendingForcedNext = true;
                            timer = 0;
                            break;
                    }
            }
            if (!triggered && timer != 0)
            {
                timer = 0;
                SoundEngine.PlaySound(MySoundID.MagicStaff);
            }
        }

        public override void OnEndSingle()
        {
            if (!AutoNext)
                switch (Owner)
                {
                    case Player plr:
                        SequencePlayer mplr = plr.GetModPlayer<SequencePlayer>();
                        mplr.PendingForcedNext = true;
                        break;
                }
            base.OnEndSingle();
        }

        public override bool Collide(Rectangle rectangle) => false;
    }
    /*public class ShockingDashInfo : MeleeAction
    {
    }*/
}

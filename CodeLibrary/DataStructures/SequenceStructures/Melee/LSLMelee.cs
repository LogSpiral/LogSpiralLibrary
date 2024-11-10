using LogSpiralLibrary.CodeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Terraria.Audio;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Melee
{
    public class SwooshInfo : MeleeAction
    {

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
        public override void LoadAttribute(XmlReader xmlReader)
        {
            mode = (SwooshMode)int.Parse(xmlReader["mode"] ?? "0");
            base.LoadAttribute(xmlReader);
        }
        public override void SaveAttribute(XmlWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("mode", ((int)mode).ToString());
            base.SaveAttribute(xmlWriter);
        }

        public override float Factor => base.Factor;

        //public virtual bool useTransition => false;
        float TimeToAngle(float t)
        {
            float max = timerMax;
            var fac = t / max;
            if (max > cutTime)
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
            {
                fac = MathHelper.SmoothStep(0, 1.125f, fac);
            }
            fac = flip ? 1 - fac : fac;
            float start = -.75f;
            float end = .625f;
            return MathHelper.Lerp(end, start, fac) * MathHelper.Pi;
        }

        [ElementCustomData]
        [CustomSeqConfigItem(typeof(SeqEnumElement))]
        public SwooshMode mode;
        int cutTime => 8;
        float k => 0.25f;
        public override float offsetRotation => TimeToAngle(fTimer);
        public override float offsetSize => base.offsetSize;

        public override bool Attacktive
        {
            get
            {
                float t = (timerMax - cutTime) * k;
                return fTimer > t && fTimer < t + cutTime;
            }
        }
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
            if (Owner is Player plr)
            {
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));

            }
            base.OnStartAttack();
        }
        public override void OnStartSingle()
        {
            base.OnStartSingle();
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
        public override void OnDeactive()
        {
            if (mode == SwooshMode.Slash)
                flip ^= true;
            base.OnDeactive();
        }
        public override void OnActive()
        {
            //flip = Main.rand.NextBool();
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
                    u = UltraSwoosh.NewUltraSwoosh(Color.Pink, (int)(verS.timeLeft * 1.2f), size, Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, f, Rotation, KValue, (range.Item1 + 0.125f, range.Item2 - 0.125f), pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                    subSwoosh = UltraSwoosh.NewUltraSwoosh(standardInfo.standardColor, verS.timeLeft, size * .67f, Owner.Center, verS.heatMap, f, Rotation, KValue, range, pair?.Item1 ?? 3, pair?.Item2 ?? 7, verS.colorVec);
                    subSwoosh.ApplyStdValueToVtxEffect(standardInfo);
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
            var fac = cutTime > timerMax ? 1 : Utils.GetLerpValue(MathHelper.Lerp(cutTime, timerMax, 1 - k), MathHelper.Lerp(cutTime, timerMax, k), timer, true);
            swoosh.timeLeft = (int)MathHelper.Lerp(1, standardInfo.vertexStandard.timeLeft, MathHelper.SmoothStep(0, 1, fac));
            swoosh.center = Owner.Center;
            swoosh.rotation = Rotation;
            swoosh.negativeDir = flip;
            swoosh.angleRange = range;
            if (flip)
                swoosh.angleRange = (swoosh.angleRange.from, -swoosh.angleRange.to);
        }
        public override void Update(bool triggered)
        {
            if (timer > (timerMax - cutTime) * k)
            {
                //UpdateSwoosh(swoosh, (offsetRotation / MathF.PI, mode == SwooshMode.Chop ? 0.625f : -0.75f));
                //UpdateSwoosh(subSwoosh, (offsetRotation / MathF.PI, mode == SwooshMode.Chop ? 0.75f : -0.875f));
                timer--;
                UpdateSwoosh(swoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : -0.75f, offsetRotation / MathF.PI));
                UpdateSwoosh(subSwoosh, (mode == SwooshMode.Chop ? 0.625f - 2 : -0.75f, offsetRotation / MathF.PI));
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
    public class StabInfo : MeleeAction
    {
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
        public override Vector2 offsetCenter => default;//new Vector2(64 * Factor, 0).RotatedBy(Rotation);
        public override Vector2 offsetOrigin => new Vector2(Factor * .4f, 0).RotatedBy(standardInfo.standardRotation);
        public override bool Attacktive => timer <= MathF.Sqrt(timerMax);
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(standardInfo.soundStyle ?? MySoundID.SwooshNormal_1, Owner?.Center);
            if (Owner is Player plr)
            {
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
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
                    Owner.Center, LogSpiralLibraryMod.HeatMap[5].Value, flip, Rotation, 2, pair?.Item1 ?? -3, pair?.Item2 ?? 8, colorVec: verS.colorVec);
                    var su = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, size * .67f,
                    Owner.Center + Rotation.ToRotationVector2() * size * .2f, verS.heatMap, !flip, Rotation, 2, pair?.Item1 ?? -3, pair?.Item2 ?? 8, colorVec: verS.colorVec);
                    su.weaponTex = TextureAssets.Item[standardInfo.itemType].Value;
                    su.ApplyStdValueToVtxEffect(standardInfo);
                }
                else
                {
                    u = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize * 1.25f,
                    Owner.Center, verS.heatMap, flip, Rotation, 2, pair?.Item1 ?? -3, pair?.Item2 ?? 8, colorVec: verS.colorVec);
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
        [CustomSeqConfigItem(typeof(SeqIntInputElement))]
        public int rangeOffsetMin;
        [ElementCustomData]
        [CustomSeqConfigItem(typeof(SeqIntInputElement))]
        public int rangeOffsetMax;
        public override void LoadAttribute(XmlReader xmlReader)
        {
            givenCycle = int.Parse(xmlReader["givenCycle"]);
            ModifyData = ActionModifyData.LoadFromString(xmlReader["ModifyData"]);
            rangeOffsetMin = int.Parse(xmlReader["rangeOffsetMin"]);
            rangeOffsetMax = int.Parse(xmlReader["rangeOffsetMax"]);
        }
        public override void SaveAttribute(XmlWriter xmlWriter)
        {
            xmlWriter.WriteAttributeString("givenCycle", givenCycle.ToString());
            xmlWriter.WriteAttributeString("ModifyData", ModifyData.ToString());
            xmlWriter.WriteAttributeString("rangeOffsetMin", rangeOffsetMin.ToString());
            xmlWriter.WriteAttributeString("rangeOffsetMax", rangeOffsetMax.ToString());

        }
        [ElementCustomDataAbabdoned]
        public override int Cycle { get => realCycle; set => givenCycle = value; }
        public int realCycle;
        [ElementCustomData]
        [CustomSeqConfigItem(typeof(SeqIntInputElement))]
        [Range(1, 10)]
        public int givenCycle;
        void ResetCycle()
        {
            realCycle = rangeOffsetMin == rangeOffsetMax ? givenCycle + rangeOffsetMin : Math.Clamp(givenCycle + Main.rand.Next(rangeOffsetMin, rangeOffsetMax), 1, int.MaxValue);

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
    public class ConvoluteInfo : MeleeAction
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
                plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, (int)(ModifyData.actionOffsetDamage * plr.GetWeaponDamage(plr.HeldItem)));
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
    /*public class ShockingDashInfo : MeleeAction
    {
    }*/
}

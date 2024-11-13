using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace LogSpiralLibrary.CodeLibrary.DataStructures
{
    public class AirDistortConfigs : IAvailabilityChangableConfig
    {
        public bool Available { get; set; } = true;
        [Range(0, 10f)]
        [DefaultValue(6f)]
        public float intensity = 6f;
        [Range(0, MathHelper.TwoPi)]
        public float rotation;
        [Range(0, 1)]
        [DefaultValue(0.5f)]
        public float colorOffset = .5f;
        //[Range(0, 2)]
        //public int tier = 1;
        [JsonIgnore]
        public AirDistortEffectInfo effectInfo => !Available ? default : new AirDistortEffectInfo(intensity, rotation, colorOffset);
    }
    public class BloomConfigs : IAvailabilityChangableConfig
    {
        public bool Available { get; set; } = true;
        [Range(0, 1f)]
        public float threshold = 0f;
        [Range(0, 1f)]
        public float intensity = 1f;
        [Range(1f, 12f)]
        public float range = 1;
        [Range(1, 5)]
        public int times = 3;
        [JsonIgnore]
        public bool additive = true;

        [DefaultValue(true)]
        public bool useDownSample = true;

        [DefaultValue(true)]
        public bool useModeMK = true;

        [JsonIgnore]
        public BloomEffectInfo effectInfo => !Available ? default : new BloomEffectInfo(threshold, intensity, range, times, additive) with { useDownSample = useDownSample, useModeMK = useModeMK };// - 4 + 8 * Main.GlobalTimeWrappedHourly.CosFactor()
    }
    public class MaskConfigs : IAvailabilityChangableConfig
    {
        public bool Available { get; set; } = false;
        [Range(0, 5)]
        public int SkyStyle = 1;
        public Color glowColor = new Color(152, 74, 255);//166,17,240//255,55,225//255,153,240
        [Range(0, 1f)]
        public float tier1 = 0.2f;
        [Range(0, 1f)]
        public float tier2 = 0.25f;
        //public bool lightAsAlpha = true;
        //public bool inverse;
        [JsonIgnore]
        public MaskEffectInfo maskEffectInfo => !Available ? default :
            new MaskEffectInfo(LogSpiralLibraryMod.Misc[20 + SkyStyle].Value, glowColor, tier1, tier2, default, true, false);

    }
}

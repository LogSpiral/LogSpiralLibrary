using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSpiralLibrary.ForFun.AVictim
{
    public class AVictim:ModNPC
    {

        public override void SetStaticDefaults()
        {
            // Total count animation frames
            Main.npcFrameCount[NPC.type] = 11;
        }
        public override string Texture => $"Terraria/Images/NPC_{NPCID.TargetDummy}";
        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.TargetDummy);

            base.SetDefaults();
        }
        public override void AI()
        {
            NPC.life = NPC.lifeMax;
            base.AI();
        }
        public override bool PreKill()
        {
            return false;
        }
    }
}

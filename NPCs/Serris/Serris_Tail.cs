using Terraria;
using Terraria.ID;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace MetroidMod.NPCs.Serris
{
    public class Serris_Tail : Serris_Body
    {
		private int tailType
		{
			get { return (int)npc.ai[2]; }
		}

		public override bool Autoload(ref string name)
		{
			return (true);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Serris");
			Main.npcFrameCount[npc.type] = 15;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			npc.width = 32;
			npc.height = 32;
		}
		public override bool PreAI()
		{
			if(tailType > 0)
			{
				npc.width = 20;
				npc.height = 20;
			}
			return true;
		}
		public override void HitEffect(int hitDirection, double damage)
		{
			if (Main.netMode != 2)
			{
				if (npc.life <= 0)
				{
					int gore = Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/SerrisGore3"), 1f);
					Main.gore[gore].velocity *= 0.4f;
					Main.gore[gore].timeLeft = 60;
				}
			}
		}

		public override bool PreDraw(SpriteBatch sb, Color drawColor)
		{
			Texture2D texTail = mod.GetTexture("NPCs/Serris/Serris_Tail");
			Serris_Head serris_head = (Serris_Head)head.modNPC;

			float bRot = npc.rotation - 1.57f;
			int tailHeight = texTail.Height / 15;
			Vector2 tailOrig = new Vector2(28, 29);
			Color bodyColor = npc.GetAlpha(Lighting.GetColor((int)npc.Center.X / 16, (int)npc.Center.Y / 16));

			SpriteEffects effects = SpriteEffects.None;
			if (head.spriteDirection == -1)
			{
				effects = SpriteEffects.FlipVertically;
				tailOrig.Y = tailHeight - tailOrig.Y;
			}
			int frame = serris_head.state - 1;
			if (serris_head.state == 4)
				frame = serris_head.sbFrame + 3;

			int yFrame = frame * (tailHeight * 3) + (tailHeight * tailType);
			sb.Draw(texTail, npc.Center - Main.screenPosition, new Rectangle?(new Rectangle(0, yFrame, texTail.Width, tailHeight)),
			bodyColor, bRot, tailOrig, 1f, effects, 0f);
			return (false);
		}
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria.ModLoader;
using Terraria;

namespace MetroidMod.Buffs
{
    public class GravityDebuff : ModBuff
    {
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Amplified Gravity");
			Description.SetDefault("Your local gravity is amplified, causing you to move more slowly and heavily");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}
		public override void Update(Player player,ref int buffIndex)
		{
			player.maxFallSpeed += 10f;
			player.gravity += Player.defaultGravity;
			Player.jumpHeight -= (int)(Player.jumpHeight*0.5f);
			Player.jumpSpeed -= Player.jumpSpeed*0.5f;
			
			player.maxRunSpeed *= 0.5f;
			player.runAcceleration *= 0.5f;
			player.accRunSpeed *= 0.5f;
		}
    }
}

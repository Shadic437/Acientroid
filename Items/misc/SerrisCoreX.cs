using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace MetroidMod.Items.misc
{
    public class SerrisCoreX : ModItem
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Serris Core-X");
			Tooltip.SetDefault("Soft and squishy\n" + 
			"Seems to react to kinetic energy, and amplifies it");
			ItemID.Sets.ItemNoGravity[item.type] = true;
			Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(5, 8));
		}
		public override void SetDefaults()
		{
			item.maxStack = 99;
			item.width = 64;
			item.height = 64;
			item.value = 10000;
			item.rare = 5;
		}
	}
}
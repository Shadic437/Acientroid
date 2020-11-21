using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace MetroidMod.Items.misc
{
    public class GravityGel : ModItem
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gravity Gel");
			Tooltip.SetDefault("'Totally breaking Newton's laws.'");
			ItemID.Sets.ItemNoGravity[item.type] = true;
		}
		public override void SetDefaults()
		{
			item.maxStack = 999;
			item.width = 16;
			item.height = 16;
			item.value = 10000;
			item.rare = 5;
			
		}
    
	}
}
﻿using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace MetroidMod.Items.equipables
{
    [AutoloadEquip(EquipType.Body)]
    class ChoziteBreastplate : ModItem
    {
        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Chozite Breastplate");
		}
        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.rare = 1;
            item.value = 5000;
            item.defense = 6;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return (head.type == mod.ItemType("ChoziteHelmet") && body.type == mod.ItemType("ChoziteBreastplate") && legs.type == mod.ItemType("ChoziteGreaves"));
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "+2 defense" + "\r\n"
                + "Allows you to slide down walls";
            player.statDefense += 2;
            player.spikedBoots += 1;
            MPlayer mp = player.GetModPlayer<MPlayer>();
            mp.visorGlow = true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "ChoziteBar", 30);
            recipe.AddIngredient(ItemID.Topaz, 2);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}

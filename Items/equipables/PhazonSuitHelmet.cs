﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MetroidMod.Items.equipables
{
    [AutoloadEquip(EquipType.Head)]
    public class PhazonSuitHelmet : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phazon Suit Helmet");
            Tooltip.SetDefault("5% increased ranged damage\n" +
            "+25 overheat capacity\n" +
            "Improved night vision\n" +
            "Increased jump height");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.rare = 7;
            item.value = 36000;
            item.defense = 15;
        }

        public override void UpdateEquip(Player player)
        {
            player.rangedDamage += 0.05f;
            player.nightVision = true;
            player.jumpBoost = true;
            MPlayer mp = player.GetModPlayer<MPlayer>();
            mp.maxOverheat += 25;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "GravitySuitHelmet");
            recipe.AddIngredient(ItemID.SpectreBar, 15);
            recipe.AddIngredient(null, "PurePhazon", 10);
            recipe.AddTile(null, "NovaWorkTableTile");
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}

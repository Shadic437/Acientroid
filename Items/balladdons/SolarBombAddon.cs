using Terraria.ID;
using Terraria.ModLoader;

namespace MetroidMod.Items.balladdons
{
	public class SolarBombAddon : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Solar Bomb");
			Tooltip.SetDefault("Morph Ball Addon\n" +
			"Slot Type: Special\n" +
			"-Press the Power Bomb Key to set off a Solar Bomb (20 second cooldown)\n" +
			"-Solar Bombs create massive explosions which burn enemies and vacuum in items afterwards\n" +
			"-Solar Bombs ignore 50% of enemy defense and can deal ~7400 damage total");
		}
		public override void SetDefaults()
		{
			item.width = 32;
			item.height = 32;
			item.maxStack = 1;
			item.value = 30000;
			item.rare = 4;
			/*item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.createTile = mod.TileType("SolarBombTile");*/
			MGlobalItem mItem = item.GetGlobalItem<MGlobalItem>();
			mItem.ballSlotType = 2;
			mItem.powerBombType = mod.ProjectileType("SolarBomb");
		}

		public override void AddRecipes()
		{
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.AdamantiteBar, 3);
            recipe.AddIngredient(ItemID.Dynamite, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 20);
            recipe.SetResult(this);
            recipe.AddRecipe();

            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.TitaniumBar, 3);
            recipe.AddIngredient(ItemID.Dynamite, 15);
            recipe.AddIngredient(ItemID.SoulofFright, 20);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
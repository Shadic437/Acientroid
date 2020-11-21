using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MetroidMod.Items.tiles
{
	public class MissileStation : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Missile Station");
			Tooltip.SetDefault("Right click the station while standing next to it to recharge your missiles");
		}
		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 30;
			item.maxStack = 999;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.createTile = mod.TileType("MissileStation");
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "MissileExpansion");
			recipe.AddRecipeGroup("IronBar", 5);
            recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}

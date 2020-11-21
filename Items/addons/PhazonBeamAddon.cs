using Terraria.ID;
using Terraria.ModLoader;

namespace MetroidMod.Items.addons
{
	public class PhazonBeamAddon : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Phazon Beam");
			Tooltip.SetDefault("Power Beam Addon\n" +
			"Slot Type: Charge\n" +
			"'It's made of pure Phazon energy!'\n" + 
			"Shots inflict a Phazon DoT debuff on enemies\n" + 
			"Only activates when the Secondary, Utility, and Primary A and B slots are in use\n" +
			"Now with out restictions!");
		}
		public override void SetDefaults()
		{
			item.width = 10;
			item.height = 14;
			item.maxStack = 1;
			item.value = 2500;
			item.rare = 4;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.createTile = mod.TileType("PhazonBeamTile");
			MGlobalItem mItem = item.GetGlobalItem<MGlobalItem>();
			mItem.addonSlotType = 0;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.ShroomiteBar, 6);
            		recipe.AddIngredient(null, "PurePhazon", 12);
            		recipe.AddIngredient(ItemID.SoulofNight, 5);
			recipe.AddIngredient(ItemID.SoulofSight, 10);
			recipe.AddIngredient(ItemID.SoulofMight, 10);
			recipe.AddIngredient(ItemID.SoulofFright, 10);
            		recipe.AddTile(null, "NovaWorkTableTile");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}

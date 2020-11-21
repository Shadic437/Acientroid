using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;

namespace MetroidMod.Items.tools
{
    public class TorizoSummon : ModItem
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Inactive Torizo");
			//Tooltip.SetDefault("Places an inactive Torizo if none are in the world");
			Tooltip.SetDefault("Summons the Torizo");
		}
        public override void SetDefaults()
		{
			item.maxStack = 20;
			item.consumable = true;
			item.width = 48;
			item.height = 48;
			item.useTime = 45;//25;
			item.useAnimation = 45;//25;
			item.useStyle = 4;//1;
			item.noMelee = true;
			item.value = 1000;
			item.rare = 2;
		}

        public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "EnergyTank");
			recipe.AddIngredient(null, "ChozoStatueArm");
			recipe.AddIngredient(null, "ChozoStatue");
			recipe.AddTile(TileID.DemonAltar);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

        public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(mod.NPCType("Torizo"));// && !NPC.AnyNPCs(mod.NPCType("TorizoIdle"));
		}
        public override bool UseItem(Player player)
        {
            //Vector2 pos = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);
            //NPC.NewNPC((int)pos.X, (int)pos.Y, mod.NPCType("TorizoIdle"));
			
			NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("Torizo"));
			Main.PlaySound(15,(int)player.position.X,(int)player.position.Y,0);
            return true;
        }
    }
}

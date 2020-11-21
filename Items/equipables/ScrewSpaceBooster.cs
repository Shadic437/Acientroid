using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace MetroidMod.Items.equipables
{
    public class ScrewSpaceBooster : ModItem
    {
        bool screwAttack = false;
        int screwAttackSpeed = 0;
        int screwSpeedDelay = 0;
        int proj = -1;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Space Boosted Screw Attack");
            Tooltip.SetDefault("Allows the user to run insanely fast\n" +
            "Allows somersaulting\n" +
            "Damage enemies while running or somersaulting\n" +
            "Damage increases with enemy's damage\n" +
            "Allows the user to jump up to 10 times in a row\n" +
            "Jumps recharge mid-air\n" +
            "Holding left/right while jumping midair gives a boost");
        }
        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 38;
            item.maxStack = 1;
            item.value = 80000;
            item.rare = 8;
            item.accessory = true;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = 1;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "ScrewAttack");
            recipe.AddIngredient(null, "SpaceBooster");
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            MPlayer mp = player.GetModPlayer<MPlayer>();
            mp.speedBooster = true;
            mp.spaceJump = true;
            mp.screwAttack = 1;
        }
        public override bool CanEquipAccessory(Player player, int slot)
        {
            for (int k = 3; k < 8 + player.extraAccessorySlots; k++)
            {
                if(k != slot && (player.armor[k].type == mod.ItemType("SpeedBooster") || player.armor[k].type == mod.ItemType("SpaceJump") || player.armor[k].type == mod.ItemType("SpaceBooster") || player.armor[k].type == mod.ItemType("ScrewAttack") || player.armor[k].type == mod.ItemType("TerraBooster")))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
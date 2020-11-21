using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

using MetroidMod.Items;
using MetroidMod.Projectiles;
using MetroidMod.Projectiles.chargelead;

namespace MetroidMod.Items.weapons
{
	public class MissileLauncher : ModItem
	{
		// Failsaves.
		private Item[] _missileMods;
		public Item[] missileMods
		{
			get
			{
				if (_missileMods == null)
				{
					_missileMods = new Item[MetroidMod.missileSlotAmount];
					for (int i = 0; i < _missileMods.Length; ++i)
					{
						_missileMods[i] = new Item();
						_missileMods[i].TurnToAir();
					}
				}

				return _missileMods;
			}
			set { _missileMods = value; }
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Missile Launcher");
			Tooltip.SetDefault("Select this item in your hotbar and open your inventory to open the Missile Addon UI");
		}
		public override void SetDefaults()
		{
			item.damage = 30;
			item.ranged = true;
			item.width = 24;
			item.height = 16;
			item.scale = 0.8f;
			item.useTime = 9;
			item.useAnimation = 9;
			item.useStyle = 5;
			item.noMelee = true;
			item.knockBack = 5.5f;
			item.value = 20000;
			item.rare = 2;
			item.UseSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/MissileSound");
			item.autoReuse = false;
			item.shoot = mod.ProjectileType("MissileShot");
			item.shootSpeed = 8f;
			item.crit = 3;
			
			MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
			mi.statMissiles = 5;
			mi.maxMissiles = 5;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ChoziteBar", 10);
			recipe.AddIngredient(null, "EnergyTank", 1);
			recipe.AddIngredient(ItemID.Musket, 1);
			recipe.SetResult(this);
			recipe.AddRecipe();
			
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ChoziteBar", 10);
			recipe.AddIngredient(null, "EnergyTank", 1);
			recipe.AddIngredient(ItemID.TheUndertaker, 1);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
		
		public override void UseStyle(Player P)
		{
			P.itemLocation.X = P.MountedCenter.X - (float)Main.itemTexture[item.type].Width * 0.5f;
			P.itemLocation.Y = P.MountedCenter.Y - (float)Main.itemTexture[item.type].Height * 0.5f;
		}
		
		public override bool CanUseItem(Player player)
		{
			MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
			if(player.whoAmI == Main.myPlayer && item.type == Main.mouseItem.type)
			{
				return false;
			}
			return (player.whoAmI == Main.myPlayer && mi.statMissiles > 0);
		}
		
		int finalDmg = 0;
		
		int useTime = 9;
		
		string shot = "MissileShot";
		string chargeShot = "DiffusionMissileShot";
		string shotSound = "MissileSound";
		string chargeShotSound = "SuperMissileSound";
		string chargeUpSound = "ChargeStartup_Power";
		string chargeTex = "ChargeLead_PlasmaRed";
		int dustType = 6;
		Color dustColor = default(Color);
		Color lightColor = MetroidMod.plaRedColor;
		
		float comboKnockBack = 5.5f;

		bool isCharge = false;
		bool isSeeker = false;
		int isHeldCombo = 0;
		int chargeCost = 5;
		int comboSound = 0;
		bool noSomersault = false;
		bool useFlameSounds = false;
		bool useVortexSounds = false;
		
		bool isShotgun = false;
		int shotgunAmt = 5;
		
		bool isMiniGun = false;
		int miniRateIncr = 2;
		int miniGunCostReduct = 2;
		int miniGunAmt = 1;
		
		int comboUseTime = 4;
		int comboCostUseTime = 12;
		int comboShotAmt = 1;
		float chargeMult = 1f;
		
		float leadAimSpeed = 0f;

		public override void UpdateInventory(Player P)
		{
			MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
			MPlayer mp = P.GetModPlayer<MPlayer>();

			int ic = mod.ItemType("IceMissileAddon");
			int sm = mod.ItemType("SuperMissileAddon");
			int icSm = mod.ItemType("IceSuperMissileAddon");
			int st = mod.ItemType("StardustMissileAddon");
			int ne = mod.ItemType("NebulaMissileAddon");
			
			int se = mod.ItemType("SeekerMissileAddon");
			
			Item slot1 = missileMods[0];
			Item slot2 = missileMods[1];
			Item exp = missileMods[2];
			
			int damage = 30;
			useTime = 9;
			shot = "MissileShot";
			chargeShot = "";
			shotSound = "MissileSound";
			chargeShotSound = "SuperMissileSound";
			chargeUpSound = "";
			chargeTex = "";
			dustType = 0;
			dustColor = default(Color);
			lightColor = Color.White;
			
			comboKnockBack = item.knockBack;
			
			isSeeker = (slot1.type == se);
			isCharge = (!slot1.IsAir && !isSeeker);
			isHeldCombo = 0;
			chargeCost = 5;
			comboSound = 0;
			noSomersault = false;
			useFlameSounds = false;
			useVortexSounds = false;
			
			isShotgun = false;
			shotgunAmt = 5;
			
			isMiniGun = false;
			miniRateIncr = 2;
			miniGunCostReduct = 2;
			miniGunAmt = 1;
			
			comboUseTime = 4;
			comboCostUseTime = 12;
			comboShotAmt = 1;
			
			chargeMult = 1f;
			
			leadAimSpeed = 0f;
			
			mi.maxMissiles = 5 + (5*exp.stack);
			if(mi.statMissiles > mi.maxMissiles)
			{
				mi.statMissiles = mi.maxMissiles;
			}

			// Default Combos
			
			if(slot2.type == sm)
			{
				damage = 90;
				useTime = 18;
				shot = "SuperMissileShot";
			}
			else if(slot2.type == ic)
			{
				damage = 45;
				shot = "IceMissileShot";
			}
			else if(slot2.type == icSm)
			{
				damage = 105;
				useTime = 18;
				shot = "IceSuperMissileShot";
			}
			else if(slot2.type == st)
			{
				damage = 150;
				useTime = 18;
				shot = "StardustMissileShot";
			}
			else if(slot2.type == ne)
			{
				damage = 150;
				useTime = 18;
				shot = "NebulaMissileShot";
			}
			
			int wb = mod.ItemType("WavebusterAddon");
			int icSp = mod.ItemType("IceSpreaderAddon");
			int sp = mod.ItemType("SpazerComboAddon");
			int ft = mod.ItemType("FlamethrowerAddon");
			int pl = mod.ItemType("PlasmaMachinegunAddon");
			int nv = mod.ItemType("NovaComboAddon");
			
			int di = mod.ItemType("DiffusionMissileAddon");
			
			// Charge Combos
			if(slot1.type == wb)
			{
				isHeldCombo = 1;
				chargeCost = 0;
				comboSound = 1;
				noSomersault = true;
				chargeShot = "WavebusterShot";
				chargeUpSound = "ChargeStartup_Wave";
				chargeTex = "ChargeLead_WaveV2";
				dustType = 62;
				lightColor = MetroidMod.waveColor2;
				comboKnockBack = 0f;
			}
			if(slot1.type == icSp)
			{
				chargeCost = 10;
				chargeShot = "IceSpreaderShot";
				chargeShotSound = "IceSpreaderSound";
				chargeUpSound = "ChargeStartup_Ice";
				chargeTex = "ChargeLead_Ice";
				dustType = 59;
				lightColor = MetroidMod.iceColor;
			}
			if(slot1.type == sp)
			{
				isShotgun = true;
				chargeCost = 5;
				chargeShot = shot;
				chargeUpSound = "ChargeStartup_Power";
				chargeTex = "ChargeLead_Spazer";
				dustType = 64;
				lightColor = MetroidMod.powColor;
			}
			if(slot1.type == ft)
			{
				isHeldCombo = 2;
				chargeCost = 0;
				comboSound = 1;
				noSomersault = true;
				useFlameSounds = true;
				chargeShot = "FlamethrowerShot";
				chargeUpSound = "ChargeStartup_PlasmaRed";
				chargeTex = "ChargeLead_PlasmaRed";
				dustType = 6;
				lightColor = MetroidMod.plaRedColor;
			}
			if(slot1.type == pl)
			{
				isHeldCombo = 2;
				chargeCost = 0;
				comboCostUseTime = 0;
				comboSound = 2;
				noSomersault = true;
				isMiniGun = true;
				//chargeShot = "PlasmaMachinegunLead";
				chargeShot = "PlasmaMachinegunShot";
				chargeShotSound = "PlasmaMachinegunSound";
				chargeUpSound = "ChargeStartup_Power";
				chargeTex = "ChargeLead_PlasmaGreen";
				dustType = 61;
				lightColor = MetroidMod.plaGreenColor;
			}
			if(slot1.type == nv)
			{
				isHeldCombo = 1;
				chargeCost = 0;
				comboSound = 1;
				noSomersault = true;
				leadAimSpeed = 0.85f;
				chargeShot = "NovaLaserShot";
				chargeUpSound = "ChargeStartup_Nova";
				chargeTex = "ChargeLead_Nova";
				dustType = 75;
				lightColor = MetroidMod.novColor;
			}
			
			if(slot1.type == di)
			{
				chargeShot = "DiffusionMissileShot";
				chargeUpSound = "ChargeStartup_Power";
				chargeTex = "ChargeLead_PlasmaRed";
				dustType = 6;
				lightColor = MetroidMod.plaRedColor;
				
				if(slot2.type == ic || slot2.type == icSm)
				{
					chargeShot = "IceDiffusionMissileShot";
					chargeUpSound = "ChargeStartup_Ice";
					chargeTex = "ChargeLead_Ice";
					dustType = 135;
					lightColor = MetroidMod.iceColor;
				}
				if(slot2.type == st)
				{
					chargeShot = "StardustDiffusionMissileShot";
					chargeUpSound = "ChargeStartup_Ice";
					chargeTex = "ChargeLead_Stardust";
					dustType = 87;
					lightColor = MetroidMod.iceColor;
				}
				if(slot2.type == ne)
				{
					chargeShot = "NebulaDiffusionMissileShot";
					chargeUpSound = "ChargeStartup_Wave";
					chargeTex = "ChargeLead_Nebula";
					dustType = 255;
					lightColor = MetroidMod.waveColor;
				}
			}
			
			int sd = mod.ItemType("StardustComboAddon");
			int nb = mod.ItemType("NebulaComboAddon");
			int vt = mod.ItemType("VortexComboAddon");
			int sl = mod.ItemType("SolarComboAddon");
			
			if(slot1.type == sd)
			{
				chargeCost = 10;
				chargeShot = "StardustComboShot";
				chargeShotSound = "IceSpreaderSound";
				chargeUpSound = "ChargeStartup_Ice";
				chargeTex = "ChargeLead_Stardust";
				dustType = 87;
				lightColor = MetroidMod.iceColor;
			}
			if(slot1.type == nb)
			{
				isHeldCombo = 1;
				chargeCost = 0;
				comboSound = 1;
				noSomersault = true;
				chargeShot = "NebulaComboShot";
				chargeUpSound = "ChargeStartup_Wave";
				chargeTex = "ChargeLead_Nebula";
				dustType = 255;
				lightColor = MetroidMod.waveColor;
			}
			if(slot1.type == vt)
			{
				isHeldCombo = 2;
				chargeCost = 0;
				comboSound = 2;
				noSomersault = true;
				
				comboUseTime = 10;
				comboShotAmt = 3;
				
				useVortexSounds = true;
				
				chargeShot = "VortexComboShot";
				chargeShotSound = "PlasmaMachinegunSound";
				chargeUpSound = "ChargeStartup_Power";
				chargeTex = "ChargeLead_Vortex";
				dustType = 229;
				lightColor = MetroidMod.lumColor;
			}
			if(slot1.type == sl)
			{
				isHeldCombo = 1;
				chargeCost = 0;
				comboSound = 1;
				noSomersault = true;
				leadAimSpeed = 0.9f;
				chargeShot = "SolarLaserShot";
				chargeUpSound = "ChargeStartup_PlasmaRed";
				chargeTex = "ChargeLead_SolarCombo";
				dustType = 6;
				lightColor = MetroidMod.plaRedColor;
			}
			
			
			isCharge &= (mi.statMissiles >= chargeCost);
			
			finalDmg = damage;
			
			item.damage = finalDmg;
			item.useTime = useTime;
			item.useAnimation = useTime;
			item.shoot = mod.ProjectileType(shot);
			item.UseSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/"+shotSound);
			if(isCharge || isSeeker)
			{
				item.UseSound = null;
			}
			
			item.autoReuse = (isCharge || isSeeker);

			item.shootSpeed = 8f;
			item.reuseDelay = 0;
			item.mana = 0;
			item.knockBack = 5.5f;
			item.scale = 0.8f;
			item.crit = 3;
			item.value = 20000;
			
			item.rare = 2;
			
			item.Prefix(item.prefix);
		}
		
		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if(item == Main.HoverItem)
			{
				item.modItem.UpdateInventory(Main.player[Main.myPlayer]);
			}

			for (int k = 0; k < tooltips.Count; k++)
			{
				if(tooltips[k].Name == "PrefixDamage")
				{
					double num19 = (double)((float)item.damage - (float)finalDmg);
					num19 = num19 / (double)((float)finalDmg) * 100.0;
					num19 = Math.Round(num19);
					if (num19 > 0.0)
					{
						tooltips[k].text = "+" + num19 + Lang.tip[39].Value;
					}
					else
					{
						tooltips[k].text = num19 + Lang.tip[39].Value;
					}
				}
				if(tooltips[k].Name == "PrefixSpeed")
				{
					double num20 = (double)((float)item.useAnimation - (float)useTime);
					num20 = num20 / (double)((float)useTime) * 100.0;
					num20 = Math.Round(num20);
					num20 *= -1.0;
					if (num20 > 0.0)
					{
						tooltips[k].text = "+" + num20 + Lang.tip[40].Value;
					}
					else
					{
						tooltips[k].text = num20 + Lang.tip[40].Value;
					}
				}
			}
		}
		
		public override ModItem Clone(Item item)
		{
			ModItem clone = this.NewInstance(item);
			MissileLauncher missileClone = (MissileLauncher)clone;
			missileClone.missileMods = new Item[MetroidMod.missileSlotAmount];
			for (int i = 0; i < MetroidMod.missileSlotAmount; ++i)
			{
				missileClone.missileMods[i] = this.missileMods[i];
			}

			return clone;
		}
		
		int chargeLead = -1;
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
			if(isCharge)
			{
				int ch = Projectile.NewProjectile(position.X,position.Y,speedX,speedY,mod.ProjectileType("ChargeLead"),damage,knockBack,player.whoAmI);
				ChargeLead cl = (ChargeLead)Main.projectile[ch].modProjectile;
				cl.ChargeUpSound = chargeUpSound;
				cl.ChargeTex = chargeTex;
				cl.DustType = dustType;
				cl.DustColor = dustColor;
				cl.LightColor = lightColor;
				cl.ShotSound = shotSound;
				cl.ChargeShotSound = chargeShotSound;
				cl.projectile.netUpdate = true;
				cl.missile = true;
				cl.comboSound = comboSound;
				cl.noSomersault = noSomersault;
				cl.aimSpeed = leadAimSpeed;

				chargeLead = ch;
				return false;
			}
			else if(isSeeker)
			{
				int ch = Projectile.NewProjectile(position.X,position.Y,speedX,speedY,mod.ProjectileType("SeekerMissileLead"),damage,knockBack,player.whoAmI);
				chargeLead = ch;
				return false;
			}
			else
			{
				mi.statMissiles -= 1;
			}
			return true;
		}
		
		bool initialShot = false;
		int comboTime = 0;
		int comboCostTime = 0;
		int useTimeMax = 20;
		float scalePlus = 0f;
		int targetingDelay = 0;
		int targetNum = 0;
		public override void HoldItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				MPlayer mp = player.GetModPlayer<MPlayer>();
				MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();

				if (isCharge)
				{
					if (!mp.ballstate && !mp.shineActive && !player.dead && !player.noItems)
					{
						Vector2 oPos = player.RotatedRelativePoint(player.MountedCenter, true);

						float MY = Main.mouseY + Main.screenPosition.Y;
						float MX = Main.mouseX + Main.screenPosition.X;
						if (player.gravDir == -1f)
							MY = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY;

						float targetrotation = (float)Math.Atan2((MY - oPos.Y), (MX - oPos.X));

						Vector2 velocity = targetrotation.ToRotationVector2() * item.shootSpeed;

						float dmgMult = chargeMult;
						int damage = player.GetWeaponDamage(item);
						
						if (player.controlUseItem && chargeLead != -1 && Main.projectile[chargeLead].active && Main.projectile[chargeLead].owner == player.whoAmI && Main.projectile[chargeLead].type == mod.ProjectileType("ChargeLead"))
						{
							if (mp.statCharge < MPlayer.maxCharge)
							{
								mp.statCharge = Math.Min(mp.statCharge + 1, MPlayer.maxCharge);
							}
							if(isHeldCombo > 0)
							{
								if(mi.statMissiles > 0)
								{
									if(mp.statCharge >= MPlayer.maxCharge)
									{
										if(isMiniGun)
										{
											this.MiniGunShoot(player, item, Main.projectile[chargeLead], mod.ProjectileType(chargeShot), (int)((float)damage * dmgMult), comboKnockBack, chargeShotSound);
										}
										else
										{
											if(!initialShot)
											{
												if(useFlameSounds || useVortexSounds)
												{
													int type = mod.ProjectileType("FlamethrowerLead");
													if(useVortexSounds)
													{
														type = mod.ProjectileType("VortexComboLead");
													}
													int proj = Projectile.NewProjectile(oPos.X, oPos.Y, velocity.X, velocity.Y, type, 0, 0, player.whoAmI);
													Main.projectile[proj].ai[0] = chargeLead;
												}
												initialShot = true;
											}
											if(comboTime <= 0)
											{
												for(int i = 0; i < comboShotAmt; i++)
												{
													int proj = Projectile.NewProjectile(oPos.X, oPos.Y, velocity.X, velocity.Y, mod.ProjectileType(chargeShot), (int)((float)damage * dmgMult), comboKnockBack, player.whoAmI);
													Main.projectile[proj].ai[0] = chargeLead;
												}
												comboTime = comboUseTime;
											}
											
											if(comboCostUseTime > 0)
											{
												if(comboCostTime <= 0)
												{
													mi.statMissiles -= 1;
													comboCostTime = comboCostUseTime;
												}
												else
												{
													comboCostTime--;
												}
											}
											
											if(isHeldCombo == 2 && comboTime > 0)
											{
												comboTime--;
											}
										}
									}
								}
								else
								{
									Main.projectile[chargeLead].Kill();
								}
							}
						}
						else
						{
							if(isHeldCombo <= 0 || mp.statCharge < MPlayer.maxCharge)
							{
								if (mp.statCharge >= MPlayer.maxCharge && mi.statMissiles >= chargeCost)
								{
									if(isShotgun)
									{
										for(int i = 0; i < shotgunAmt; i++)
										{
											int k = i - (shotgunAmt/2);
											Vector2 shotGunVel = Vector2.Normalize(velocity) * (item.shootSpeed + 4f);
											double rot = Angle.ConvertToRadians(4.0*k);
											shotGunVel = shotGunVel.RotatedBy(rot, default(Vector2));
											if (float.IsNaN(shotGunVel.X) || float.IsNaN(shotGunVel.Y))
											{
												shotGunVel = -Vector2.UnitY;
											}
											int chargeProj = Projectile.NewProjectile(oPos.X, oPos.Y, shotGunVel.X, shotGunVel.Y, mod.ProjectileType(chargeShot), (int)((float)damage * dmgMult), item.knockBack, player.whoAmI);
										}
									}
									else
									{
										int chargeProj = Projectile.NewProjectile(oPos.X, oPos.Y, velocity.X, velocity.Y, mod.ProjectileType(chargeShot), (int)((float)damage * dmgMult), item.knockBack, player.whoAmI);
									}
									mi.statMissiles -= chargeCost;
								}
								else if (mp.statCharge > 0)
								{
									int shotProj = Projectile.NewProjectile(oPos.X, oPos.Y, velocity.X, velocity.Y, mod.ProjectileType(shot), damage, item.knockBack, player.whoAmI);
									mi.statMissiles -= 1;
								}
							}

							if (chargeLead == -1 || !Main.projectile[chargeLead].active || Main.projectile[chargeLead].owner != player.whoAmI || Main.projectile[chargeLead].type != mod.ProjectileType("ChargeLead"))
							{
								mp.statCharge = 0;
							}
							
							comboTime = 0;
							comboCostTime = 0;
							useTimeMax = 20;
							miniCostNum = 0;
							scalePlus = 0f;
							initialShot = false;
						}
					}
					else if (!mp.ballstate) { 
						mp.statCharge = 0;
						comboTime = 0;
						comboCostTime = 0;
						useTimeMax = 20;
						miniCostNum = 0;
						scalePlus = 0f;
						initialShot = false;
					}
				}
				else { 
					mp.statCharge = 0;
					comboTime = 0;
					comboCostTime = 0;
					useTimeMax = 20;
					miniCostNum = 0;
					scalePlus = 0f;
					initialShot = false;
				}

				if (targetingDelay > 0)
					targetingDelay--;

				if (isSeeker && !mp.ballstate && !mp.shineActive && !player.dead && !player.noItems)
				{
					Vector2 oPos = player.RotatedRelativePoint(player.MountedCenter, true);
					float MY = Main.mouseY + Main.screenPosition.Y;
					float MX = Main.mouseX + Main.screenPosition.X;
					Rectangle mouse = new Rectangle((int)MX - 1, (int)MY - 1, 2, 2);
					if (player.gravDir == -1f)
					{
						MY = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY;
					}
					float targetrotation = (float)Math.Atan2((MY - oPos.Y), (MX - oPos.X));
					Vector2 velocity = targetrotation.ToRotationVector2() * item.shootSpeed;
					int damage = player.GetWeaponDamage(item);
					if (player.controlUseItem && chargeLead != -1 && Main.projectile[chargeLead].active && Main.projectile[chargeLead].owner == player.whoAmI && Main.projectile[chargeLead].type == mod.ProjectileType("SeekerMissileLead"))
					{
						if (mi.seekerCharge < MGlobalItem.seekerMaxCharge)
						{
							mi.seekerCharge = Math.Min(mi.seekerCharge + 1, MGlobalItem.seekerMaxCharge);
						}
						else
						{
							for (int i = 0; i < Main.maxNPCs; i++)
							{
								NPC npc = Main.npc[i];
								if (npc.active && npc.chaseable && !npc.dontTakeDamage && !npc.friendly)// && !npc.immortal)
								{
									Rectangle npcRect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
									bool flag = false;
									for (int j = 0; j < mi.seekerTarget.Length; j++)
									{
										if(mi.seekerTarget[j] == npc.whoAmI)
										{
											flag = true;
										}
									}
									if (mouse.Intersects(npcRect) && mi.seekerTarget[targetNum] <= -1 && (targetingDelay <= 0 || !flag /*prevTarget != npc.whoAmI*/) && mi.statMissiles > mi.numSeekerTargets)
									{
										mi.seekerTarget[targetNum] = npc.whoAmI;
										targetNum++;
										if (targetNum > 4)
										{
											targetNum = 0;
										}
										targetingDelay = 40;
										Main.PlaySound(SoundLoader.customSoundType, (int)oPos.X, (int)oPos.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/SeekerLockSound"));
									}
								}
							}

							int num = 10;
							while (mi.seekerTarget[targetNum] > -1 && num > 0)
							{
								targetNum++;
								if (targetNum > 4)
								{
									targetNum = 0;
								}
								num--;
							}

							mi.numSeekerTargets = 0;
							for (int i = 0; i < mi.seekerTarget.Length; i++)
							{
								if (mi.seekerTarget[i] > -1)
								{
									mi.numSeekerTargets++;

									if (!Main.npc[mi.seekerTarget[i]].active)
									{
										mi.seekerTarget[i] = -1;
									}
								}
							}
						}
					}
					else
					{
						if (mi.seekerCharge >= MGlobalItem.seekerMaxCharge && mi.numSeekerTargets > 0)
						{
							for (int i = 0; i < mi.seekerTarget.Length; i++)
							{
								if (mi.seekerTarget[i] > -1)
								{
									int shotProj = Projectile.NewProjectile(oPos.X, oPos.Y, velocity.X, velocity.Y, mod.ProjectileType(shot), damage, item.knockBack, player.whoAmI);
									MProjectile mProj = (MProjectile)Main.projectile[shotProj].modProjectile;
									mProj.seekTarget = mi.seekerTarget[i];
									mProj.seeking = true;
									mProj.projectile.netUpdate2 = true;
									mi.statMissiles = Math.Max(mi.statMissiles - 1, 0);
								}
							}

							Main.PlaySound(SoundLoader.customSoundType, (int)oPos.X, (int)oPos.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/SeekerMissileSound"));
						}
						else if (mi.seekerCharge > 0)
						{
							int shotProj = Projectile.NewProjectile(oPos.X, oPos.Y, velocity.X, velocity.Y, mod.ProjectileType(shot), damage, item.knockBack, player.whoAmI);
							Main.PlaySound(SoundLoader.customSoundType, (int)oPos.X, (int)oPos.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/" + shotSound));

							mi.statMissiles -= 1;
						}
						if (chargeLead == -1 || !Main.projectile[chargeLead].active || Main.projectile[chargeLead].owner != player.whoAmI || Main.projectile[chargeLead].type != mod.ProjectileType("SeekerMissileLead"))
						{
							mi.seekerCharge = 0;
						}
						mi.numSeekerTargets = 0;
						for (int k = 0; k < mi.seekerTarget.Length; k++)
						{
							mi.seekerTarget[k] = -1;
						}
						targetNum = 0;
						targetingDelay = 0;
					}
				}
				else
				{
					mi.seekerCharge = 0;
					mi.numSeekerTargets = 0;
					for (int k = 0; k < mi.seekerTarget.Length; k++)
					{
						mi.seekerTarget[k] = -1;
					}
					targetNum = 0;
					targetingDelay = 0;
				}
			}
		}
		int waveDir = 1;
		int miniCostNum = 0;
		SoundEffectInstance soundInstance;
		public void MiniGunShoot(Player player, Item item, Projectile Lead, int projType, int damage, float knockBack, string sound)
		{
			if(comboTime <= 0)
			{
				soundInstance = Main.PlaySound(SoundLoader.customSoundType, (int)player.Center.X, (int)player.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/" + sound));
				if(soundInstance != null)
				{
					soundInstance.Volume *= 1f - 0.25f*(scalePlus / 20f);
				}
				
				float spray = 1f * (scalePlus / 20f);
				
				float scaleFactor2 = 14f;
				for(int i = 0; i < miniGunAmt; i++)
				{
					float rot = Lead.velocity.ToRotation() + (float)Angle.ConvertToRadians(Main.rand.Next(18)*10) - (float)Math.PI/2f;
					Vector2 vector3 = Lead.Center + rot.ToRotationVector2() * 7f * spray;
					Vector2 vector5 = Vector2.Normalize(Lead.velocity) * scaleFactor2;
					vector5 = vector5.RotatedBy((Main.rand.NextDouble() * 0.12 - 0.06)*spray, default(Vector2));
					if (float.IsNaN(vector5.X) || float.IsNaN(vector5.Y))
					{
						vector5 = -Vector2.UnitY;
					}
					int proj = Projectile.NewProjectile(vector3.X, vector3.Y, vector5.X, vector5.Y, projType, damage, knockBack, player.whoAmI, 0f, 0f);
					Main.projectile[proj].ai[0] = Lead.whoAmI;
					MProjectile mProj = (MProjectile)Main.projectile[proj].modProjectile;
					mProj.waveDir = waveDir;
				}
				
				waveDir *= -1;
				
				comboTime = useTimeMax;
				useTimeMax = Math.Max(useTimeMax - miniRateIncr, comboUseTime);
				
				MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
				if(miniCostNum == 0)
				{
					mi.statMissiles -= 1;
				}
				
				miniCostNum++;
				if(miniCostNum > miniGunCostReduct)
				{
					miniCostNum = 0;
				}
			}
			else
			{
				comboTime--;
			}
			scalePlus = Math.Min(scalePlus + (2f / useTimeMax), 20f);
			ChargeLead chLead = (ChargeLead)Lead.modProjectile;
			chLead.extraScale = 0.3f * (scalePlus / 20f);
		}
		
		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound();
			for (int i = 0; i < missileMods.Length; ++i)
				tag.Add("missileItem" + i, ItemIO.Save(missileMods[i]));

			MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
			tag.Add("statMissiles", mi.statMissiles);
			tag.Add("maxMissiles", mi.maxMissiles);

			return tag;
		}
		public override void Load(TagCompound tag)
		{
			try
			{
				missileMods = new Item[MetroidMod.missileSlotAmount];
				for (int i = 0; i < missileMods.Length; i++)
				{
					Item item = tag.Get<Item>("missileItem" + i);
					missileMods[i] = item;
				}
				
				MGlobalItem mi = this.item.GetGlobalItem<MGlobalItem>();
				mi.statMissiles = tag.GetInt("statMissiles");
				mi.maxMissiles = tag.GetInt("maxMissiles");
			}
			catch{}
		}

		public override void NetSend(BinaryWriter writer)
		{
			for (int i = 0; i < missileMods.Length; ++i)
			{
				writer.WriteItem(missileMods[i]);
			}
			writer.Write(chargeLead);
		}
		public override void NetRecieve(BinaryReader reader)
		{
			for (int i = 0; i < missileMods.Length; ++i)
			{
				missileMods[i] = reader.ReadItem();
			}
			chargeLead = reader.ReadInt32();
		}
	}
}

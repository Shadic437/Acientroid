using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.ModLoader;
using ReLogic;
using ReLogic.Graphics;

using MetroidMod.Items;
using MetroidMod.NewUI;
using MetroidMod.Items.weapons;

namespace MetroidMod
{
	public enum MetroidMessageType : byte
	{
		SyncStartPlayerStats,
		SyncPlayerStats,
		PlaySyncedSound
	}

	public class MetroidMod : Mod
	{
		internal const int ballSlotAmount = 5;
		internal const int beamSlotAmount = 5;
		internal const int missileSlotAmount = 3;

		public static Color powColor = new Color(248, 248, 110);
		public static Color iceColor = new Color(0, 255, 255);
		public static Color waveColor = new Color(215, 0, 215);
		public static Color waveColor2 = new Color(239, 153, 239);
		public static Color plaRedColor = new Color(253, 221, 3);
		public static Color plaGreenColor = new Color(0, 248, 112);
		public static Color plaGreenColor2 = new Color(61, 248, 154);
		public static Color novColor = new Color(50, 255, 1);
		public static Color wideColor = new Color(255, 210, 255);
		public static Color lumColor = new Color(209, 255, 250);

		internal static ModHotKey SpiderBallKey;
		internal static ModHotKey BoostBallKey;
		internal static ModHotKey PowerBombKey;
		internal static ModHotKey SenseMoveKey;
		public const string TorizoHead = "MetroidMod/NPCs/Torizo/Torizo_Head_Boss";
		public const string SerrisHead = "MetroidMod/NPCs/Serris/Serris_Head_Head_Boss_";
		public const string KraidHead = "MetroidMod/NPCs/Kraid/Kraid_Head_Head_Boss_";
		public const string PhantoonHead = "MetroidMod/NPCs/Phantoon/Phantoon_Head_Boss";
		public const string NightmareHead = "MetroidMod/NPCs/Nightmare/Nightmare_Head_Boss";
		public const string OmegaPirateHead = "MetroidMod/NPCs/OmegaPirate/OmegaPirate_Head_Boss";
		public static Mod Instance;

		internal UserInterface pbUserInterface;
		internal PowerBeamUI powerBeamUI;

		internal UserInterface mlUserInterface;
		internal MissileLauncherUI missileLauncherUI;

		internal UserInterface mbUserInterface;
		internal MorphBallUI morphBallUI;

		public int selectedItem = 0;
		public int oldSelectedItem = 0;

		public int[] FrozenStandOnNPCs;

		public MetroidMod()	{ }

		public override void Load()
		{
			Instance = this;

			FrozenStandOnNPCs = new int[] { this.NPCType("Ripper") };

			SpiderBallKey = RegisterHotKey("Spider Ball", "X");
			BoostBallKey = RegisterHotKey("Boost Ball", "F");
			PowerBombKey = RegisterHotKey("Power Bomb", "R");
			SenseMoveKey = RegisterHotKey("Use Sense Move", "F");

			if (!Main.dedServ)
			{
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Serris"), ItemType("SerrisMusicBox"), TileType("SerrisMusicBox"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Kraid"), ItemType("KraidPhantoonMusicBox"), TileType("KraidPhantoonMusicBox"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Ridley"), ItemType("RidleyMusicBox"), TileType("RidleyMusicBox"));
			}
			for (int s = 1; s <= 7; s++)
				AddBossHeadTexture(SerrisHead + s);
			for (int k = 0; k <= 3; k++)
				AddBossHeadTexture(KraidHead + k);

			AddBossHeadTexture(TorizoHead);
			AddBossHeadTexture(PhantoonHead);
			AddBossHeadTexture(NightmareHead);
			AddBossHeadTexture(OmegaPirateHead);

			SetupUI();
		}

		private void SetupUI()
		{
			if (Main.dedServ) return;

			powerBeamUI = new PowerBeamUI();
			powerBeamUI.Activate();
			pbUserInterface = new UserInterface();
			pbUserInterface.SetState(powerBeamUI);

			missileLauncherUI = new MissileLauncherUI();
			missileLauncherUI.Activate();
			mlUserInterface = new UserInterface();
			mlUserInterface.SetState(missileLauncherUI);

			morphBallUI = new MorphBallUI();
			morphBallUI.Activate();
			mbUserInterface = new UserInterface();
			mbUserInterface.SetState(morphBallUI);
		}

		public override void UpdateUI(GameTime gameTime)
		{
			Player player = Main.LocalPlayer;
			if (player.selectedItem < 10)
			{
				oldSelectedItem = selectedItem;
				selectedItem = player.selectedItem;
			}

			if (pbUserInterface != null && PowerBeamUI.visible)
				pbUserInterface.Update(gameTime);
			if (mlUserInterface != null && MissileLauncherUI.visible)
				mlUserInterface.Update(gameTime);
			if (mbUserInterface != null && MorphBallUI.visible)
				mbUserInterface.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			Player P = Main.player[Main.myPlayer];
			
			int TargetIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Entity Health Bars"));
			if(TargetIndex != -1)
			{
				layers.Insert(TargetIndex + 1, new LegacyGameInterfaceLayer(
					"MetroidMod: Seeker Targets",
					delegate
					{
						DrawSeekerTargets(Main.spriteBatch);
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
			
			int MapIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Map / Minimap"));
			if(MapIndex != -1)
			{
				layers.Insert(MapIndex + 1, new LegacyGameInterfaceLayer(
					"MetroidMod: Charge Meter",
					delegate
					{
						if(!Main.playerInventory && Main.npcChatText == "" && P.sign < 0 && !Main.ingameOptionsWindow)
						{
							DrawChargeBar(Main.spriteBatch);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
				layers.Insert(MapIndex + 1, new LegacyGameInterfaceLayer(
					"MetroidMod: Space Jump Meter",
					delegate
					{
						if(!Main.playerInventory && Main.npcChatText == "" && P.sign < 0 && !Main.ingameOptionsWindow)
						{
							DrawSpaceJumpBar(Main.spriteBatch);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
			
			int ResourceIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if(ResourceIndex != -1)
			{
				layers.Insert(ResourceIndex + 1, new LegacyGameInterfaceLayer(
					"MetroidMod: Reserve Tanks",
					delegate
					{
						DrawReserveHearts(Main.spriteBatch);
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
			
			int InventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (InventoryIndex != -1)
			{
				layers.Insert(InventoryIndex + 1, new LegacyGameInterfaceLayer(
					"MetroidMod: Power Beam UI",
					delegate
					{
						if(PowerBeamUI.visible)
							pbUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
				layers.Insert(InventoryIndex + 1, new LegacyGameInterfaceLayer(
					"MetroidMod: Missile Launcher UI",
					delegate
					{
						if (MissileLauncherUI.visible)
							mlUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
				layers.Insert(InventoryIndex + 1, new LegacyGameInterfaceLayer(
					"MetroidMod: Morph Ball UI",
					delegate
					{
						if (MorphBallUI.visible)
							mbUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		static int z = 0;
		public override void PostDrawInterface(SpriteBatch sb)
		{
			Mod mod = ModLoader.GetMod(UIParameters.MODNAME);
			Player P = Main.player[Main.myPlayer];
			MPlayer mp = P.GetModPlayer<MPlayer>();
			Item item = P.inventory[P.selectedItem];

			if (P.buffType[0] > 0)
			{
				if(P.buffType[11] > 0)
				{
					z = 100;
				}
				else
				{
					z = 50;
				}
			}
			else
			{
				z = 0;
			}
		}
		float tRot = 0f;
		public void DrawSeekerTargets(SpriteBatch sb)
		{
			Mod mod = ModLoader.GetMod(UIParameters.MODNAME);
			Player P = Main.player[Main.myPlayer];
			MPlayer mp = P.GetModPlayer<MPlayer>();
			Item item = P.inventory[P.selectedItem];
			
			if(item.type == mod.ItemType("MissileLauncher"))
			{
				MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
				if(mi.numSeekerTargets > 0)
				{
					tRot += 0.05f;
					for(int i = 0; i < mi.seekerTarget.Length; i++)
					{
						if(mi.seekerTarget[i] > -1)
						{
							int frame = 0;
							bool flag = true;
							for(int j = 0; j < mi.seekerTarget.Length; j++)
							{
								if(i != j)
								{
									if(mi.seekerTarget[i] == mi.seekerTarget[j])
									{
										flag = false;
										frame += 1;
									}
								}
								else
								{
									flag = true;
								}
							}
							if(flag)
							{
								NPC npc = Main.npc[mi.seekerTarget[i]];
								Texture2D tTex = mod.GetTexture("Gore/Targeting_retical");
								Color color = new Color(255, 255, 255, 10);
								int height = tTex.Height / 5;
								int yFrame = height*frame;
								sb.Draw(tTex, npc.Center - Main.screenPosition, new Rectangle?(new Rectangle(0, yFrame, tTex.Width, height)), color, tRot, new Vector2((float)tTex.Width/2f, (float)height/2f), npc.scale*1.5f, SpriteEffects.None, 0f);
							}
						}
					}
				}
			}
		}
		public static int chStyle;
		public static int chR = 255;
		public static int chG = 0;
		public static int chB = 0;
		public void DrawChargeBar(SpriteBatch sb)
		{
			Mod mod = ModLoader.GetMod(UIParameters.MODNAME);
			Player P = Main.player[Main.myPlayer];
			MPlayer mp = P.GetModPlayer<MPlayer>();
			Item item = P.inventory[P.selectedItem];
			if (P.whoAmI == Main.myPlayer && P.active && !P.dead && !P.ghost)
			{
				Texture2D texBar = mod.GetTexture("Gore/ChargeBar"),
					texBarBorder = mod.GetTexture("Gore/ChargeBarBorder"),
					texBarBorder2 = mod.GetTexture("Gore/ChargeBarBorder2");
				if(item.type == mod.ItemType("PowerBeam") || item.type == mod.ItemType("MissileLauncher") || mp.ballstate)
				{
					int ch = (int)mp.statCharge, chMax = (int)MPlayer.maxCharge;
					int pb = (int)mp.statPBCh, pbMax = (int)MPlayer.maxPBCh;
					float x = 22, y = 78+z;
					int times = (int)Math.Ceiling(texBar.Height/2f);
					float chpercent = chMax == 0 ? 0f : 1f*ch/chMax;
					float pbpercent = pbMax == 0 ? 0f : 1f*pb/pbMax;
					int w = (int)(Math.Floor(texBar.Width/2f*chpercent)*2);
					int w2 = (int)(Math.Floor(texBar.Width/2f*pbpercent)*2);
					Color c = chpercent < 1f ? new Color(chR,chG,chB) : Color.Gold;
					Color p = pbpercent < 1f ? Color.Crimson : Color.Gray;
					chStyle = chpercent <= 0f ? 0 : (chpercent <= .5f ? 1 : (chpercent <= .75f ? 2 : (chpercent <= .99f ? 3 : 0)));
					float offsetX = 2, offsetY = 2;
					sb.Draw(texBarBorder2,new Vector2(x,y),new Rectangle(0,0,texBarBorder2.Width,texBarBorder2.Height),Color.White);
					if(pb > 0)
					{
						for (int i = 0; i < times; i++)
						{
							int ww = w2-(i*2);
							if (ww > 0)
							{
								sb.Draw(texBar,new Vector2(x+offsetX,y+offsetY+i*2),new Rectangle(0,i*2,ww,2),p);
							}
						}
					}
					if(ch > 9)
					{
						for (int i = 0; i < times; i++)
						{
							int ww = w-(i*2);
							if (ww > 0)
							{
								sb.Draw(texBar,new Vector2(x+offsetX,y+offsetY+i*2),new Rectangle(0,i*2,ww,2),c);
							}
						}
					}
					if(mp.hyperColors > 0)
					{
						sb.Draw(texBar,new Vector2(x+offsetX,y+offsetY),new Rectangle(0,0,texBar.Width,texBar.Height),new Color(mp.r,mp.g,mp.b));
					}
					sb.Draw(texBarBorder,new Vector2(x,y),new Rectangle(0,0,texBarBorder.Width,texBarBorder.Height),Color.White);

					if(item.type == mod.ItemType("MissileLauncher"))
					{
						MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
						int num = Math.Min(mi.statMissiles,mi.maxMissiles);
						string text = num.ToString("000");
						Vector2 vect = Main.fontMouseText.MeasureString(text);
						Color color = new Color((int)((byte)((float)Main.mouseTextColor)), (int)((byte)((float)Main.mouseTextColor)), (int)((byte)((float)Main.mouseTextColor)), (int)((byte)((float)Main.mouseTextColor)));
						sb.DrawString(Main.fontMouseText, text, new Vector2(x+38-(vect.X/2), y), color, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
					}
				}
				if(item.type == mod.ItemType("PowerBeam") || mp.shineDirection != 0 || mp.shineActive)
				{
					Texture2D overheatBar = mod.GetTexture("Gore/OverheatBar"),
					overheatBorder = mod.GetTexture("Gore/OverheatBorder");
					int ovh = (int)Math.Min(mp.statOverheat,mp.maxOverheat), ovhMax = (int)mp.maxOverheat;
					float x2 = 22, y2 = 120+z;
					int times2 = (int)Math.Ceiling(overheatBar.Height/2f);
					float ovhpercent = ovhMax == 0 ? 0f : 1f*ovh/ovhMax;
					int wo = (int)(Math.Floor(overheatBar.Width*ovhpercent));
					Color colorheat = new Color((int)((byte)((float)Main.mouseTextColor)),(int)((byte)((float)Main.mouseTextColor*0.25f)),(int)((byte)((float)Main.mouseTextColor*0.1f)),(int)((byte)((float)Main.mouseTextColor)));
					Color o = ovhpercent < 1f ? Color.Gold : colorheat;
					sb.Draw(overheatBorder,new Vector2(x2,y2),new Rectangle(0,0,overheatBorder.Width,overheatBorder.Height),Color.White);
					if(ovh > 0)
					{
						for (int i = 0; i < times2; i++)
						{
							int ww = wo-(i*2);
							if (ww > 0 && ovh <= ovhMax)
							{
								sb.Draw(overheatBar,new Vector2(x2+6,y2+2+i*2),new Rectangle(0,i*2,ww,2),o);
							}
						}
					}
					string text = (int)Math.Round((double)mp.statOverheat)+"/"+ovhMax;
					Vector2 vect = Main.fontMouseText.MeasureString(text);
					Color color = new Color((int)((byte)((float)Main.mouseTextColor)), (int)((byte)((float)Main.mouseTextColor)), (int)((byte)((float)Main.mouseTextColor)), (int)((byte)((float)Main.mouseTextColor)));
					sb.DrawString(Main.fontMouseText, text, new Vector2(x2+2, y2+overheatBorder.Height+2), color, 0f, default(Vector2), 0.75f, SpriteEffects.None, 0f);
				}
				int num4 = (int)((float)30 % 255);
				if (chStyle == 1)
				{
					chG += num4;
					if (chG >= 255)
					{
						chG = 255;
						chStyle++;
					}
					chR -= num4;
					if (chR <= 0)
					{
						chR = 0;
					}
				}
				else if (chStyle == 2)
				{
					chB += num4;
					if (chB >= 255)
					{
						chB = 255;
						chStyle++;
					}
					chG -= num4;
					if (chG <= 196)
					{
						chG = 196;
					}
				}
				else if (chStyle == 3)
				{
					chR += num4;
					if (chR >= 255)
					{
						chR = 255;
						chStyle = 0;
					}
					chB -= num4;
					if (chB <= 0)
					{
						chB = 0;
					}
					if(chB <= 196)
					{
						chG -= num4;
						if (chG <= 0)
						{
							chG = 0;
						}
					}
				}
				else if (chStyle == 0 || mp.statCharge <= 0)
				{
					chR = 255;
					chG = 0;
					chB = 0;
				}
			}
		}
		public void DrawSpaceJumpBar(SpriteBatch sb)
		{
			Mod mod = ModLoader.GetMod(UIParameters.MODNAME);
			Player P = Main.player[Main.myPlayer];
			MPlayer mp = P.GetModPlayer<MPlayer>();
			if(mp.shineDirection == 0 && mp.spaceJump && mp.spaceJumped && P.velocity.Y != 0 && !mp.ballstate)
			{
				Texture2D texBar = mod.GetTexture("Gore/SpaceJumpBar"), texBarBorder = mod.GetTexture("Gore/SpaceJumpBarBorder");
				if (P.whoAmI == Main.myPlayer && P.active && !P.dead && !P.ghost)
				{
					int sj = (int)mp.statSpaceJumps, sjMax = (int)MPlayer.maxSpaceJumps;
					float x = 160, y = 98+z;
					int times = (int)Math.Ceiling(texBar.Height/2f);
					float sjpercent = sjMax == 0 ? 0f : 1f*sj/sjMax;
					int w = (int)(Math.Floor(texBar.Width/2f*sjpercent)*2);
					Color s = sjpercent < 1f ? Color.Cyan : Color.SkyBlue;
					sb.Draw(texBarBorder,new Vector2(x,y),new Rectangle(0,0,texBarBorder.Width,texBarBorder.Height),Color.White);
					sb.Draw(texBar,new Vector2(x+2,y+2),new Rectangle(0,0,w,texBar.Height),s);
				}
			}
		}
		public void DrawReserveHearts(SpriteBatch sb)
		{
			Mod mod = ModLoader.GetMod(UIParameters.MODNAME);
			Player P = Main.player[Main.myPlayer];
			MPlayer mp = P.GetModPlayer<MPlayer>();
			if (mp.reserveTanks > 0)
			{
				Texture2D texHeart = mod.GetTexture("Gore/ReserveHeart");
				if (P.whoAmI == Main.myPlayer && P.active && !P.dead && !P.ghost)
				{
					float lifePerHeart = 20f;
					int num = Main.player[Main.myPlayer].statLifeMax / 20;
					int num2 = (Main.player[Main.myPlayer].statLifeMax - 400) / 5;
					if (num2 < 0)
					{
						num2 = 0;
					}
					if (num2 > 0)
					{
						num = Main.player[Main.myPlayer].statLifeMax / (20 + num2 / 4);
						lifePerHeart = (float)Main.player[Main.myPlayer].statLifeMax / 20f;
					}
					int num3 = Main.player[Main.myPlayer].statLifeMax2 - Main.player[Main.myPlayer].statLifeMax;
					lifePerHeart += (float)(num3 / num);
					int num4 = (int)((float)Main.player[Main.myPlayer].statLifeMax2 / lifePerHeart);
					if (num4 >= 10)
					{
						num4 = 10;
					}
					for (int i = 1; i < mp.reserveHearts + 1; i++)
					{
						float num5 = 1f;
						bool flag = false;
						int num6;
						if ((float)Main.player[Main.myPlayer].statLife >= (float)i * lifePerHeart)
						{
							num6 = 255;
							if ((float)Main.player[Main.myPlayer].statLife == (float)i * lifePerHeart)
							{
								flag = true;
							}
						}
						else
						{
							float num7 = ((float)Main.player[Main.myPlayer].statLife - (float)(i - 1) * lifePerHeart) / lifePerHeart;
							num6 = (int)(30f + 225f * num7);
							if (num6 < 30)
							{
								num6 = 30;
							}
							num5 = num7 / 4f + 0.75f;
							if ((double)num5 < 0.75)
							{
								num5 = 0.75f;
							}
							if (num7 > 0f)
							{
								flag = true;
							}
						}
						if (flag)
						{
							num5 += Main.cursorScale - 1f;
						}
						int num8 = 0;
						int num9 = 0;
						if (i > 10)
						{
							num8 -= 260;
							num9 += 26;
						}
						int a = (int)((double)((float)num6) * 0.9);
						if (mp.reserveHeartsValue >= 25)
						{
							texHeart = mod.GetTexture("Gore/ReserveHeart2");
						}
						Main.spriteBatch.Draw(texHeart, new Vector2((float)(500 + 26 * (i - 1) + num8 + (Main.screenWidth - 800) + Main.heartTexture.Width / 2), 32f + ((float)Main.heartTexture.Height - (float)Main.heartTexture.Height * num5) / 2f + (float)num9 + (float)(Main.heartTexture.Height / 2)), new Rectangle?(new Rectangle(0, 0, texHeart.Width, texHeart.Height)), new Color(num6, num6, num6, a), 0f, new Vector2((float)(texHeart.Width / 2), (float)(texHeart.Height / 2)), num5, SpriteEffects.None, 0f);
					}
				}
			}
		}

		/* NETWORK SYNICNG <<<<< WIP >>>>> */

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MetroidMessageType msgType = (MetroidMessageType)reader.ReadByte();
			switch(msgType)
			{
				case MetroidMessageType.SyncPlayerStats:
				case MetroidMessageType.SyncStartPlayerStats:
					byte playerID = reader.ReadByte();
					MPlayer targetPlayer = Main.player[playerID].GetModPlayer<MPlayer>();
					double statCharge = reader.ReadDouble();
					bool spiderBall = reader.ReadBoolean();
					int boostEffect = reader.ReadInt32();
					int boostCharge = reader.ReadInt32();

					targetPlayer.statCharge = (float)statCharge;
					targetPlayer.spiderball = spiderBall;
					targetPlayer.boostEffect = boostEffect;
					targetPlayer.boostCharge = boostCharge;

					if (msgType == MetroidMessageType.SyncPlayerStats && Main.netMode == NetmodeID.Server)
					{
						var packet = GetPacket();
						packet.Write((byte)MetroidMessageType.SyncPlayerStats);
						packet.Write(playerID);
						packet.Write(statCharge);
						packet.Write(spiderBall);
						packet.Write(boostEffect);
						packet.Write(boostCharge);
						packet.Send(-1, playerID);
					}
					break;

				case MetroidMessageType.PlaySyncedSound:
					byte playerID2 = reader.ReadByte();
					Player targetPlayer2 = Main.player[playerID2];
					string sound = reader.ReadString();

					Main.PlaySound(GetLegacySoundSlot(SoundType.Custom, "Sounds/" + sound), targetPlayer2.position);

					if (Main.netMode == 2)
					{
						ModPacket packet = GetPacket();
						packet.Write((byte)MetroidMessageType.PlaySyncedSound);
						packet.Write(playerID2);
						packet.Write(sound);
						packet.Send(-1, whoAmI);
					}
					break;
			}
		}
	}
}

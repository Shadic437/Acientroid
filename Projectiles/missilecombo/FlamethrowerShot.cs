using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MetroidMod.Projectiles.missilecombo
{
	public class FlamethrowerShot : MProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flamethrower Shot");
			Main.projFrames[projectile.type] = 9;
		}
		int maxTimeLeft = 60;
		static int width = 24;
		static int height = 36;
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = width;
			projectile.height = height;
			projectile.scale = 0.5f;
			projectile.timeLeft = maxTimeLeft;
			projectile.penetrate = 40;//-1;
			//projectile.usesLocalNPCImmunity = true;
       	 	//projectile.localNPCHitCooldown = 20;
			projectile.extraUpdates = 1;
		}
		
		bool collideFlag = false;
		bool initialize = false;
		public override void AI()
		{
			Projectile P = projectile;
			P.rotation = 0f;
			
			if(!initialize)
			{
				P.frame = Main.rand.Next(3);
				P.position.Y -= 2f*P.scale;
				initialize = true;
			}
			
			Color color = MetroidMod.plaRedColor;
			Lighting.AddLight(P.Center, color.R/255f,color.G/255f,color.B/255f);
			
			P.ai[0] += 1f;
			if(P.ai[0] > 3f)
			{
				float num297 = 0.7f + 0.3f * (P.scale - 1f);
				int num3;
				for(int num299 = 0; num299 < 1; num299 = num3 + 1)
				{
					int num300 = Dust.NewDust(new Vector2(P.position.X, P.position.Y), P.width, P.height, 6, P.velocity.X * 0.2f, P.velocity.Y * 0.2f, 100, default(Color), 1f);
					Dust dust3;
					if(Main.rand.Next(3) != 0)
					{
						Main.dust[num300].noGravity = true;
						dust3 = Main.dust[num300];
						dust3.scale *= 3f;
						Dust dust52 = Main.dust[num300];
						dust52.velocity.X = dust52.velocity.X * 2f;
						Dust dust53 = Main.dust[num300];
						dust53.velocity.Y = dust53.velocity.Y * 2f;
					}
					dust3 = Main.dust[num300];
					dust3.scale *= 1.5f;
					Dust dust54 = Main.dust[num300];
					dust54.velocity.X = dust54.velocity.X * 1.2f;
					Dust dust55 = Main.dust[num300];
					dust55.velocity.Y = dust55.velocity.Y * 1.2f;
					dust3 = Main.dust[num300];
					dust3.scale *= num297;
					num3 = num299;
				}
			}
			
			if(P.ai[0] <= maxTimeLeft)
			{
				P.scale += 2f / maxTimeLeft;
			}
			else
			{
				P.scale -= 2f / maxTimeLeft;
			}
			if(P.scale < 0.5f)
			{
				P.scale = 0.5f;
			}
			
			P.position.X += (float)P.width/2f;
			P.position.Y += (float)P.height;
			P.width = (int)((float)width * P.scale);
			P.height = (int)((float)height * P.scale);
			P.position.X -= (float)P.width/2f;
			P.position.Y -= (float)P.height;
			
			if(P.numUpdates <= 0)
			{
				P.frame++;
				if(P.frame >= 3)
				{
					P.frame = 0;
				}
			}
			
			Rectangle projRect = new Rectangle((int)P.position.X,(int)P.position.Y,P.width,P.height);
			for(int i = 0; i < Main.maxNPCs; i++)
			{
				if(Main.npc[i].active && Main.npc[i].lifeMax > 5 && !Main.npc[i].dontTakeDamage && !Main.npc[i].friendly)
				{
					NPC npc = Main.npc[i];
					Rectangle npcRect = new Rectangle((int)npc.position.X,(int)npc.position.Y,npc.width,npc.height);
					if(projRect.Intersects(npcRect))
					{
						P.velocity *= 0f;
						if(!collideFlag)
						{
							P.timeLeft += maxTimeLeft;
							collideFlag = true;
						}
					}
				}
			}
		}
		
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			width = projectile.width;
			height = projectile.width;
			return true;
		}
		
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (projectile.velocity != oldVelocity)
			{
				projectile.velocity *= 0f;
				if(!collideFlag)
				{
					projectile.timeLeft += maxTimeLeft;
					collideFlag = true;
				}
			}
			return false;
		}
		
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.immune[projectile.owner] = 4;
			target.AddBuff(24,600,true);
		}

		public override void Kill(int timeLeft)
		{
			Projectile P = projectile;
			P.position -= P.velocity;
			for (int num70 = 0; num70 < 10; num70++)
			{
				int num71 = Dust.NewDust(P.position, P.width, P.height, 6, 0f, 0f, 100, default(Color), 3f);
				Main.dust[num71].velocity *= 1.4f;
				Main.dust[num71].noGravity = true;
			}
		}
		
		public override Color? GetAlpha(Color lightColor)
		{
			return new Color((int)lightColor.R, (int)lightColor.G, (int)lightColor.B, 100);
		}
		
		public override bool PreDraw(SpriteBatch sb, Color lightColor)
		{
			Projectile P = projectile;
			if(P.ai[0] > 3f)
			{
				SpriteEffects effects = SpriteEffects.None;
				if (P.spriteDirection == -1)
				{
					effects = SpriteEffects.FlipHorizontally;
				}
				Texture2D tex = Main.projectileTexture[P.type];
				int num108 = tex.Height / Main.projFrames[P.type];
				int frame = P.frame;
				float scale = P.scale;
				if(P.scale >= 1.75f)
				{
					scale -= 1f;
					frame += 6;
				}
				else if(P.scale >= 1.25f)
				{
					scale -= 0.5f;
					frame += 3;
				}
				int y4 = num108 * frame;
				
				sb.Draw(tex, new Vector2((float)((int)(P.Center.X - Main.screenPosition.X)), (float)((int)(P.position.Y + P.height - Main.screenPosition.Y))), 
				new Rectangle?(new Rectangle(0, y4, tex.Width, num108)), 
				P.GetAlpha(Color.White), 0f, 
				new Vector2((float)tex.Width/2f, (float)num108-2), 
				scale, effects, 0f);
			}
			return false;
		}
	}
}
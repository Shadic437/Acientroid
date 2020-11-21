using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MetroidMod.Projectiles
{
	public class PhazonBeamShot : MProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Phazon Beam Shot");
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 10;
			projectile.height = 10;
			projectile.scale = 1.25f;
			projectile.tileCollide = false;
			projectile.penetrate = -1;
			
			
			mProjectile.amplitude = 8f*projectile.scale;
			mProjectile.wavesPerSecond = 0.9f;
			
		}

		public override void AI()
		{
			int dustType = 62;
			
			projectile.rotation = 0;
			
			
			mProjectile.WaveBehavior(projectile);
			
			if(projectile.numUpdates == 0)
			{
				int dust = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType, 0, 0, 100, default(Color), projectile.scale);
				Main.dust[dust].noGravity = true;
			}
		}
		
		
		public override bool PreDraw(SpriteBatch sb, Color lightColor)
		{
			mProjectile.DrawCentered(projectile, sb);
			return false;
		}
	}
}
using Terraria;
using Terraria.Localization;

namespace AdvancedWorldGen.BetterVanillaWorldGen;

public class GenerateIceBiome : ControlledWorldGenPass
{
	public GenerateIceBiome() : base("Generate Ice Biome", 25.8571f)
	{
	}

	protected override void ApplyPass()
	{
		Progress.Message = Language.GetTextValue("LegacyWorldGen.56");
		VanillaInterface.SnowTop.Value = (int)Main.worldSurface;
		int num840 = WorldGen.lavaLine - WorldGen.genRand.Next(160, 200);
		int snowLeft = VanillaInterface.SnowOriginLeft.Value;
		int snowRight = VanillaInterface.SnowOriginRight.Value;
		int num843 = 10;
		bool dungeonRight = VanillaInterface.DungeonSide.Value > 0;

		int[] snowMinX = VanillaInterface.SnowMinX.Value;
		int[] snowMaxX = VanillaInterface.SnowMaxX.Value;
		for (int num844 = 0; num844 <= WorldGen.lavaLine - 140; num844++)
		{
			Progress.Set(num844 / (float)(WorldGen.lavaLine - 140));
			snowLeft += WorldGen.genRand.Next(-4, 4);
			snowRight += WorldGen.genRand.Next(-3, 5);
			if (num844 > 0)
			{
				snowLeft = (snowLeft + snowMinX[num844 - 1]) / 2;
				snowRight = (snowRight + snowMaxX[num844 - 1]) / 2;
			}

			if (dungeonRight)
			{
				if (WorldGen.genRand.NextBool(4))
				{
					snowLeft++;
					snowRight++;
				}
			}
			else if (WorldGen.genRand.NextBool(4))
			{
				snowLeft--;
				snowRight--;
			}

			if (snowLeft < 10)
				snowLeft = 10;
			else if (snowLeft >= Main.maxTilesX - 10)
				snowLeft = Main.maxTilesX - 11;
			if (snowRight < 0)
				snowRight = 0;
			else if (snowRight >= Main.maxTilesX - 10)
				snowRight = Main.maxTilesX - 11;
			if (snowLeft > snowRight) (snowLeft, snowRight) = (snowRight, snowLeft);

			snowMinX[num844] = snowLeft;
			snowMaxX[num844] = snowRight;
			for (int num845 = snowLeft; num845 < snowRight; num845++)
				if (num844 < num840)
				{
					if (Main.tile[num845, num844].WallType == 2)
						Main.tile[num845, num844].WallType = 40;

					switch (Main.tile[num845, num844].TileType)
					{
						case 0:
						case 2:
						case 23:
						case 40:
						case 53:
							Main.tile[num845, num844].TileType = 147;
							break;
						case 1:
							Main.tile[num845, num844].TileType = 161;
							break;
					}
				}
				else
				{
					num843 += WorldGen.genRand.Next(-3, 4);
					if (WorldGen.genRand.NextBool(3))
					{
						num843 += WorldGen.genRand.Next(-4, 5);
						if (WorldGen.genRand.NextBool(3))
							num843 += WorldGen.genRand.Next(-6, 7);
					}

					if (num843 < 0)
						num843 = WorldGen.genRand.Next(3);
					else if (num843 > 50)
						num843 = 50 - WorldGen.genRand.Next(3);

					for (int num846 = num844; num846 < num844 + num843; num846++)
					{
						if (Main.tile[num845, num846].WallType == 2)
							Main.tile[num845, num846].WallType = 40;

						switch (Main.tile[num845, num846].TileType)
						{
							case 0:
							case 2:
							case 23:
							case 40:
							case 53:
								Main.tile[num845, num846].TileType = 147;
								break;
							case 1:
								Main.tile[num845, num846].TileType = 161;
								break;
						}
					}
				}

			if (VanillaInterface.SnowBottom.Value < num844)
				VanillaInterface.SnowBottom.Value = num844;
		}
	}
}
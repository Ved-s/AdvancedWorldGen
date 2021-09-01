using System;
using System.Collections.Generic;
using AdvancedWorldGen.Base;
using AdvancedWorldGen.Helper;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace AdvancedWorldGen.SpecialOptions
{
	public class SpecialCase
	{
		public Func<int, int, Tile, bool> Condition;
		public int Type;

		public SpecialCase(int type, Func<int, int, Tile, bool> condition = null)
		{
			Type = type;
			Condition = condition;
		}

		public bool IsValid(int x, int y, Tile tile)
		{
			return Condition == null || Condition.Invoke(x, y, tile);
		}
	}

	public class TileReplacer
	{
		public const int None = -1;
		public const int Water = -2;
		public const int Lava = -3;
		public const int Honey = -4;

		public static List<int> NotReplaced;

		public Dictionary<int, int> Dictionary;
		public Dictionary<int, SpecialCase> SpecialCases;

		public TileReplacer(Dictionary<int, int> dictionary, Dictionary<int, SpecialCase> specialCases)
		{
			Dictionary = dictionary;
			SpecialCases = specialCases;
		}

		public static TileReplacer Snow
		{
			get
			{
				Dictionary<int, int> dictionary = new();
				UpdateDictionary(dictionary, TileID.SnowBlock, TileID.Dirt, TileID.Grass, TileID.CorruptGrass,
					TileID.ClayBlock, TileID.CrimsonGrass);
				UpdateDictionary(dictionary, TileID.IceBlock, TileID.Stone, TileID.GreenMoss, TileID.BrownMoss,
					TileID.RedMoss, TileID.BlueMoss, TileID.PurpleMoss, TileID.LavaMoss, TileID.KryptonMoss,
					TileID.XenonMoss, TileID.ArgonMoss);
				UpdateDictionary(dictionary, TileID.CorruptIce, TileID.Ebonstone);
				UpdateDictionary(dictionary, TileID.FleshIce, TileID.Crimstone);
				UpdateDictionary(dictionary, TileID.BorealWood, TileID.WoodBlock);
				UpdateDictionary(dictionary, TileID.BreakableIce, Water);
				UpdateDictionary(dictionary, TileID.Slush, TileID.Silt);
				UpdateDictionary(dictionary, TileID.Trees, TileID.VanityTreeSakura, TileID.VanityTreeYellowWillow);
				UpdateDictionary(dictionary, None, TileID.Plants, TileID.CorruptPlants, TileID.Sunflower, TileID.Vines,
					TileID.Plants2, TileID.CrimsonPlants, TileID.CrimsonVines, TileID.VineFlowers, TileID.CorruptThorns,
					TileID.CrimsonThorns);
				Dictionary<int, SpecialCase> specialCases = new()
				{
					{
						TileID.ImmatureHerbs, new SpecialCase(None,
							(x, y, tile) => tile.frameX == 0 || tile.frameX == 32 || tile.frameX == 32 * 2 ||
							                tile.frameX == 32 * 3 || tile.frameX == 32 * 6)
					},
					{
						TileID.MatureHerbs, new SpecialCase(None,
							(x, y, tile) => tile.frameX == 0 || tile.frameX == 32 || tile.frameX == 32 * 2 ||
							                tile.frameX == 32 * 3 || tile.frameX == 32 * 6)
					},
					{
						TileID.BloomingHerbs, new SpecialCase(None,
							(x, y, tile) => tile.frameX == 0 || tile.frameX == 32 || tile.frameX == 32 * 2 ||
							                tile.frameX == 32 * 3 || tile.frameX == 32 * 6)
					},
					{
						TileID.Cattail, new SpecialCase(None,
							(x, y, tile) => tile.frameY == 0 || tile.frameY == 32 * 3 || tile.frameY == 32 * 4)
					},
					{
						TileID.LilyPad, new SpecialCase(None,
							(x, y, tile) => tile.frameY == 0 || tile.frameY == 32 * 3 || tile.frameY == 32 * 4)
					},
					{
						TileID.DyePlants, new SpecialCase(None,
							(x, y, tile) => tile.frameX == 32 * 3 || tile.frameX == 32 * 4 || tile.frameX == 32 * 7)
					}
				};

				return new TileReplacer(dictionary, specialCases);
			}
		}

		public static void Initialize()
		{
			NotReplaced = new List<int>
			{
				TileID.ClosedDoor,
				TileID.MagicalIceBlock,
				TileID.Traps,
				TileID.Boulder,
				TileID.Teleporter,
				TileID.MetalBars,
				TileID.PlanterBox,
				TileID.TrapdoorClosed,
				TileID.TallGateClosed
			};
			IEnumerable<ModTile> modTiles = ModLoader.GetMod("ModLoader").GetContent<ModTile>();

			foreach (ModTile modTile in modTiles) NotReplaced.Add(modTile.Type);
		}

		public static void Unload()
		{
			NotReplaced = null;
		}

		public static void UpdateDictionary(Dictionary<int, int> dictionary, int to,
			params int[] from)
		{
			foreach (int tile in from) dictionary.Add(tile, to);
		}

		public void ReplaceTiles(GenerationProgress progress, string s)
		{
			progress.Message = Language.GetTextValue("Mods.AdvancedWorldGen.WorldGenMessage." + s);
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				GenPassHelper.SetProgress(progress, x, Main.maxTilesX);
				for (int y = 0; y < Main.maxTilesY; y++)
				{
					Tile tile = Main.tile[x, y];
					if (tile == null) continue;
					if (tile.IsActive) HandleReplacement(tile.type, x, y, tile);

					if (tile.LiquidAmount > 0)
						HandleReplacement(-tile.LiquidType - 2, x, y, tile);
				}
			}
		}

		public void HandleReplacement(int tileType, int x, int y, Tile tile)
		{
			if (!Dictionary.TryGetValue(tileType, out int type))
			{
				if (!SpecialCases.TryGetValue(tileType, out SpecialCase specialCase) ||
				    !specialCase.IsValid(x, y, tile))
					return;
				type = specialCase.Type;
			}

			if (tileType < -1)
			{
				tile.LiquidAmount = 0;
				if (!NotReplaced.Contains(tile.type))
					tile.IsActive = false;
			}
			else
				tile.IsActive = false;

			switch (type)
			{
				case > -1:
					if (!tile.IsActive)
					{
						tile.IsActive = true;
						tile.type = (ushort) type;
						WorldGen.DiamondTileFrame(x, y);
					}

					break;
				case < -1:
					tile.LiquidAmount = byte.MaxValue;
					tile.LiquidType = -type + 2;
					break;
			}
		}

		public static void RandomizeWorld(GenerationProgress progress, OptionHelper optionHelper)
		{
			Dictionary<ushort, ushort> tileRandom = new();
			tileRandom.Add(189, 53);
			Dictionary<ushort, ushort> wallRandom = new();
			Dictionary<ushort, byte> paintRandom = new();
			Dictionary<ushort, byte> paintWallRandom = new();

			progress.Message = Language.GetTextValue("Mods.AdvancedWorldGen.WorldGenMessage.Random");
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				GenPassHelper.SetProgress(progress, x, Main.maxTilesX, 0.5f);
				for (int y = 0; y < Main.maxTilesY; y++)
				{
					Tile tile = Main.tile[x, y];
					if (tile != null)
					{
						if (tile.IsActive) RandomizeTile(optionHelper, tile, tileRandom, paintRandom);

						if (tile.wall != 0) RandomizeWall(optionHelper, wallRandom, tile, paintWallRandom);
					}
				}
			}

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				GenPassHelper.SetProgress(progress, x, Main.maxTilesX, 0.5f, 0.5f);
				int previousBlock = 0;
				for (int y = Main.maxTilesY - 1; y >= 1; y--)
				{
					Tile tile = Main.tile[x, y];
					if (!tile.IsActive)
						continue;
					if (tile.type == TileID.Cactus)
					{
						WorldGen.CheckCactus(x, y);
						continue;
					}

					if (previousBlock != 0 && y != previousBlock - 1 &&
					    TileID.Sets.Falling[tile.type])
					{
						Tile tileAbove = Main.tile[x, y - 1];
						if (tileAbove.IsActive && !Main.tileSolid[tileAbove.type])
						{
							WorldGen.KillTile(x, y - 1);
							if (!tileAbove.IsActive)
							{
								previousBlock = y;
								continue;
							}
						}

						Tile newPos = Main.tile[x, previousBlock - 1];
						newPos.IsActive = true;
						newPos.type = tile.type;
						newPos.IsHalfBlock = tile.IsHalfBlock;
						newPos.Slope = tile.Slope;
						tile.IsActive = false;
						previousBlock--;
					}
					else
						previousBlock = y;
				}
			}
		}

		public static void RandomizeTile(OptionHelper optionHelper, Tile tile, Dictionary<ushort, ushort> tileRandom,
			Dictionary<ushort, byte> paintRandom)
		{
			if (optionHelper.OptionsContains("Random"))
				if (Main.tileSolid[tile.type])
				{
					if (tileRandom.TryGetValue(tile.type, out ushort type))
					{
						tile.type = type;
					}
					else if (Main.tileSolid[tile.type] && !TileID.Sets.Platforms[tile.type] &&
					         !NotReplaced.Contains(tile.type))
					{
						do
						{
							type = (ushort) WorldGen._genRand.Next(TileLoader.TileCount);
						} while (!Main.tileSolid[type] || TileID.Sets.Platforms[type] ||
						         NotReplaced.Contains(type) ||
						         tileRandom.ContainsValue(type));

						tileRandom[tile.type] = type;
						tile.type = type;
					}
				}

			if (optionHelper.OptionsContains("Painted"))
			{
				if (!paintRandom.TryGetValue(tile.type, out byte paint))
				{
					paint = (byte) WorldGen._genRand.Next(PaintID.IlluminantPaint + 1);
					paintRandom[tile.type] = paint;
				}

				tile.Color = paint;
			}
		}

		public static void RandomizeWall(OptionHelper optionHelper, Dictionary<ushort, ushort> wallRandom, Tile tile,
			Dictionary<ushort, byte> paintWallRandom)
		{
			if (optionHelper.OptionsContains("Random"))
			{
				if (wallRandom.TryGetValue(tile.wall, out ushort type))
				{
					tile.wall = type;
				}
				else
				{
					do
					{
						type = (ushort) WorldGen._genRand.Next(1, WallLoader.WallCount);
					} while (wallRandom.ContainsValue(type));

					wallRandom[tile.wall] = type;
					tile.wall = type;
				}
			}

			if (optionHelper.OptionsContains("Painted"))
			{
				if (!paintWallRandom.TryGetValue(tile.wall, out byte paint))
				{
					paint = (byte) WorldGen._genRand.Next(PaintID.IlluminantPaint + 1);
					paintWallRandom[tile.wall] = paint;
				}

				tile.WallColor = paint;
			}
		}
	}
}
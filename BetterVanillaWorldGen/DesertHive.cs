using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Biomes.Desert;
using Terraria.ID;
using Terraria.Utilities;

namespace AdvancedWorldGen.BetterVanillaWorldGen
{
	public static class DesertHive
	{
		public static void Place(DesertDescription description)
		{
			ClusterGroup clusters = ClusterGroup.FromDescription(description);
			PlaceClusters(clusters);
			AddTileVariance(description);
		}

		public static void PlaceClusters(ClusterGroup clusters)
		{
			Dictionary<(int, int), List<int>> hive = RegisterInterestingTiles(clusters);
			PlaceClustersArea(clusters, hive);
			foreach (((int x, int y), List<int> _) in hive) Tile.SmoothSlope(x, y, false);
		}

		public static Dictionary<(int, int), List<int>> RegisterInterestingTiles(ClusterGroup clusters)
		{
			Dictionary<(int, int), List<int>> registerInterestingTiles = new();
			int spreadX = (int) (10 * clusters.SpreadX);
			int spreadY = (int) (10 * clusters.SpreadY);
			for (int index = 0; index < clusters.Count; index++)
			{
				Cluster cluster = clusters[index];
				for (int x = cluster[0].x - spreadX; x <= cluster[0].x + spreadX; x++)
				for (int y = cluster[0].y - spreadY; y <= cluster[0].y + spreadY; y++)
					if (WorldGen.InWorld(x, y, 1))
					{
						if (!registerInterestingTiles.TryGetValue((x, y), out List<int> clusterList))
						{
							clusterList = new List<int>();
							registerInterestingTiles[(x, y)] = clusterList;
						}

						clusterList.Add(index);
					}
			}

			return registerInterestingTiles;
		}

		public static void PlaceClustersArea(ClusterGroup clusterGroup, Dictionary<(int, int), List<int>> hive)
		{
			FastRandom fastRandom = new FastRandom(Main.ActiveWorldFileData.Seed).WithModifier(57005uL);
			foreach (((int x, int y), List<int> interestingClusters) in hive)
			{
				float distanceToClosestCenter = 0f;
				int closestCluster = -1;
				float distanceToSecondClosestCenter = 0f;
				ushort type = 53;
				if (fastRandom.Next(3) == 0)
					type = 397;

				foreach (int k in interestingClusters)
				{
					Cluster cluster = clusterGroup[k];
					if (Math.Abs(cluster[0].x - x) > 10f * clusterGroup.SpreadX ||
					    Math.Abs(cluster[0].y - y) > 10f * clusterGroup.SpreadY)
						continue;

					float distanceScore = cluster.Sum(item =>
						1f / Vector2.DistanceSquared(
							new Vector2(item.x / clusterGroup.SpreadX, item.y / clusterGroup.SpreadY),
							new Vector2(x / clusterGroup.SpreadX, y / clusterGroup.SpreadY)));

					if (distanceScore > distanceToClosestCenter)
					{
						distanceToSecondClosestCenter = distanceToClosestCenter;

						distanceToClosestCenter = distanceScore;
						closestCluster = k;
					}
					else if (distanceScore > distanceToSecondClosestCenter)
					{
						distanceToSecondClosestCenter = distanceScore;
					}
				}

				float score = distanceToClosestCenter + distanceToSecondClosestCenter;
				Tile tile = Main.tile[x, y];
				if (score > 3.5f)
				{
					tile.ClearEverything();
					tile.wall = 187;
					if (closestCluster % 15 == 2)
						tile.ResetToType(404);
				}
				else if (score > 1.8f)
				{
					tile.wall = 187;
					if (y < Main.worldSurface)
						tile.LiquidAmount = 0;
					else
						tile.LiquidType = LiquidID.Lava;

					if (tile.IsActive) tile.ResetToType(396);
				}
				else if (score > 0.7f)
				{
					tile.wall = 216;
					tile.LiquidAmount = 0;
					if (tile.IsActive) tile.ResetToType(type);
				}
				else if (score > 0.25f)
				{
					FastRandom fastRandom2 = fastRandom.WithModifier(x, y);
					float num8 = (score - 0.25f) / 0.45f;
					if (fastRandom2.NextFloat() < num8)
					{
						tile.wall = 187;
						if (y < Main.worldSurface)
							tile.LiquidAmount = 0;
						else
							tile.LiquidType = LiquidID.Lava;

						if (tile.IsActive) tile.ResetToType(type);
					}
				}
			}
		}

		public static void AddTileVariance(DesertDescription description)
		{
			for (int i = -20; i < description.Hive.Width + 20; i++)
			for (int j = -20; j < description.Hive.Height + 20; j++)
			{
				int num = i + description.Hive.X;
				int num2 = j + description.Hive.Y;
				if (WorldGen.InWorld(num, num2, 1))
				{
					Tile tile = Main.tile[num, num2];
					Tile testTile = Main.tile[num, num2 + 1];
					Tile testTile2 = Main.tile[num, num2 + 2];
					if (tile.type == 53 && (!WorldGen.SolidTile(testTile) || !WorldGen.SolidTile(testTile2)))
						tile.type = 397;
				}
			}

			for (int k = -20; k < description.Hive.Width + 20; k++)
			for (int l = -20; l < description.Hive.Height + 20; l++)
			{
				int num3 = k + description.Hive.X;
				int num4 = l + description.Hive.Y;
				if (!WorldGen.InWorld(num3, num4, 1))
					continue;

				Tile tile2 = Main.tile[num3, num4];
				if (!tile2.IsActive || tile2.type != 396)
					continue;

				bool flag = true;
				for (int num5 = -1; num5 >= -3; num5--)
					if (Main.tile[num3, num4 + num5].IsActive)
					{
						flag = false;
						break;
					}

				bool flag2 = true;
				for (int m = 1; m <= 3; m++)
					if (Main.tile[num3, num4 + m].IsActive)
					{
						flag2 = false;
						break;
					}

				if (flag && WorldGen.genRand.Next(20) == 0)
				{
					WorldGen.PlaceTile(num3, num4 - 1, 485, true, true, -1, WorldGen.genRand.Next(4));
				}
				else if (flag && WorldGen.genRand.Next(5) == 0)
				{
					WorldGen.PlaceTile(num3, num4 - 1, 484, true, true);
				}
				else
				{
					if (flag ^ flag2 && WorldGen.genRand.Next(5) == 0)
						WorldGen.PlaceTile(num3, num4 + (!flag ? 1 : -1), 165, true, true);
					else if (flag && WorldGen.genRand.Next(5) == 0)
						WorldGen.PlaceTile(num3, num4 - 1, 187, true, true, -1, 29 + WorldGen.genRand.Next(6));
				}
			}
		}

		public class Cluster : List<(int x, int y)>
		{
		}

		public class ClusterGroup : List<Cluster>
		{
			public readonly int Height;
			public readonly float SpreadX;
			public readonly float SpreadY;
			public readonly int Width;

			public ClusterGroup(int width, int height, DesertDescription description)
			{
				Width = width;
				Height = height;
				SpreadX = description.Hive.Width / (float) Width;
				SpreadY = description.Hive.Height / (float) Height;
				Generate(description);
			}

			public static ClusterGroup FromDescription(DesertDescription description)
			{
				return new(description.BlockColumnCount, description.BlockRowCount, description);
			}

			public static void SearchForCluster(bool[,] blockMap, List<Point> pointCluster, int x, int y,
				int level = 2)
			{
				pointCluster.Add(new Point(x, y));
				blockMap[x, y] = false;
				level--;
				if (level != -1)
				{
					if (x > 0 && blockMap[x - 1, y])
						SearchForCluster(blockMap, pointCluster, x - 1, y, level);

					if (x < blockMap.GetLength(0) - 1 && blockMap[x + 1, y])
						SearchForCluster(blockMap, pointCluster, x + 1, y, level);

					if (y > 0 && blockMap[x, y - 1])
						SearchForCluster(blockMap, pointCluster, x, y - 1, level);

					if (y < blockMap.GetLength(1) - 1 && blockMap[x, y + 1])
						SearchForCluster(blockMap, pointCluster, x, y + 1, level);
				}
			}

			public static void AttemptClaim(int x, int y, int[,] clusterIndexMap, List<List<Point>> pointClusters,
				int index)
			{
				int num = clusterIndexMap[x, y];
				if (num == -1 || num == index)
					return;

				int num2 = WorldGen.genRand.Next(2) == 0 ? -1 : index;
				foreach (Point item in pointClusters[num]) clusterIndexMap[item.X, item.Y] = num2;
			}

			public void Generate(DesertDescription description)
			{
				Clear();
				bool[,] array = new bool[Width, Height];
				int num = Width / 2 - 1;
				int num2 = Height / 2 - 1;
				int num3 = (num + 1) * (num + 1);
				Point point = new(num, num2);
				for (int i = point.Y - num2; i <= point.Y + num2; i++)
				{
					float num4 = num / (float) num2 * (i - point.Y);
					int num5 = Math.Min(num, (int) Math.Sqrt(num3 - num4 * num4));
					for (int j = point.X - num5; j <= point.X + num5; j++) array[j, i] = WorldGen.genRand.Next(2) == 0;
				}

				List<List<Point>> list = new();
				for (int k = 0; k < array.GetLength(0); k++)
				for (int l = 0; l < array.GetLength(1); l++)
					if (array[k, l] && WorldGen.genRand.Next(2) == 0)
					{
						List<Point> list2 = new();
						SearchForCluster(array, list2, k, l);
						if (list2.Count > 2)
							list.Add(list2);
					}

				int[,] array2 = new int[array.GetLength(0), array.GetLength(1)];
				for (int m = 0; m < array2.GetLength(0); m++)
				for (int n = 0; n < array2.GetLength(1); n++)
					array2[m, n] = -1;

				for (int num6 = 0; num6 < list.Count; num6++)
					foreach (Point item in list[num6])
						array2[item.X, item.Y] = num6;

				for (int num7 = 0; num7 < list.Count; num7++)
					foreach (Point item2 in list[num7])
					{
						int x = item2.X;
						int y = item2.Y;
						if (array2[x, y] == -1)
							break;

						int index = array2[x, y];
						if (x > 0)
							AttemptClaim(x - 1, y, array2, list, index);

						if (x < array2.GetLength(0) - 1)
							AttemptClaim(x + 1, y, array2, list, index);

						if (y > 0)
							AttemptClaim(x, y - 1, array2, list, index);

						if (y < array2.GetLength(1) - 1)
							AttemptClaim(x, y + 1, array2, list, index);
					}

				foreach (List<Point> item3 in list) item3.Clear();

				for (int num8 = 0; num8 < array2.GetLength(0); num8++)
				for (int num9 = 0; num9 < array2.GetLength(1); num9++)
					if (array2[num8, num9] != -1)
						list[array2[num8, num9]].Add(new Point(num8, num9));

				foreach (List<Point> item5 in list)
				{
					Cluster cluster = new();
					if (item5.Count < 4)
						continue;

					cluster.AddRange(item5.Select(item6 => (
						(int) (item6.X * SpreadX) + description.Hive.X,
						(int) (item6.Y * SpreadY) + description.Hive.Y)));

					Add(cluster);
				}
			}
		}
	}
}
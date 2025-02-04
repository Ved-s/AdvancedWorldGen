using System.Collections.Generic;
using AdvancedWorldGen.UI.InputUI.List;

namespace AdvancedWorldGen.CustomSized;

public class Params
{
	public Dictionary<string, object> Data = new();

	public Params()
	{
		Initialize();
	}

	public int SizeX
	{
		get => (int)Data[nameof(SizeX)];
		set => Data[nameof(SizeX)] = value;
	}

	public int SizeY
	{
		get => (int)Data[nameof(SizeY)];
		set => Data[nameof(SizeY)] = value;
	}

	public float TempleMultiplier
	{
		get => (float)Data[nameof(TempleMultiplier)];
		set => Data[nameof(TempleMultiplier)] = value;
	}

	public float DungeonMultiplier
	{
		get => (float)Data[nameof(DungeonMultiplier)];
		set => Data[nameof(DungeonMultiplier)] = value;
	}

	public float BeachMultiplier
	{
		get => (float)Data[nameof(BeachMultiplier)];
		set => Data[nameof(BeachMultiplier)] = value;
	}
	
	public int Copper
	{
		get => (int)Data[nameof(Copper)];
		set => Data[nameof(Copper)] = value;
	}
	
	public int Iron
	{
		get => (int)Data[nameof(Iron)];
		set => Data[nameof(Iron)] = value;
	}
	
	public int Silver
	{
		get => (int)Data[nameof(Silver)];
		set => Data[nameof(Silver)] = value;
	}
	
	public int Gold
	{
		get => (int)Data[nameof(Gold)];
		set => Data[nameof(Gold)] = value;
	}

	public void Wipe()
	{
		Data.Clear();
		Initialize();
	}

	public void Initialize()
	{
		SizeX = -1;
		SizeY = -1;
		TempleMultiplier = 1;
		DungeonMultiplier = 1;
		BeachMultiplier = 1;
		
		Copper = TileExpandableList.Random;
		Iron = TileExpandableList.Random;
		Silver = TileExpandableList.Random;
		Gold = TileExpandableList.Random;
	}
}
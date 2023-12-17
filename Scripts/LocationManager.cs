using Godot;
using System;
using System.Collections.Generic;

public partial class LocationManager : Node
{
	public static LocationManager Instance;

	//[Export] private LocationData[] locations;
	[Export] private Node3D[] locationNodes;

	//private Dictionary<string, LocationData> locationDict;

	public override void _EnterTree()
	{
		base._EnterTree();

		Instance = this;
	}

	public override void _Ready()
	{
		base._Ready();

		// locationDict = new Dictionary<string, LocationData>(locations.Length);
		//
		// foreach (LocationData loc in locations)
		// {
		// 	locationDict.Add(loc.locationName, loc);
		// }
	}

	public FareDropoff GetLocationByName(string nameToCheck)
	{
		Node3D node = locationNodes[0];
		
		if (node is FareDropoff data)
		{
			return data;
		}

		return null;
	}

	public FareDropoff GetLocationByIndex(int index)
	{
		if (index >= locationNodes.Length) return null;
		
		Node3D node = locationNodes[index];
		
		if (node is FareDropoff data)
		{
			return data;
		}

		return null;
	}
}

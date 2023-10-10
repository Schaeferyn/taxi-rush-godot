using Godot;
using System;
using System.Collections.Generic;

public partial class LocationManager : Node
{
	public static LocationManager Instance;

	[Export] private LocationData[] locations;

	private Dictionary<string, LocationData> locationDict;

	public override void _EnterTree()
	{
		base._EnterTree();

		Instance = this;
	}

	public override void _Ready()
	{
		base._Ready();

		locationDict = new Dictionary<string, LocationData>(locations.Length);

		foreach (LocationData loc in locations)
		{
			locationDict.Add(loc.locationName, loc);
		}
	}
}

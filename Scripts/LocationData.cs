using Godot;
using System;

public partial class LocationData : Resource
{
	[Export] public string locationName;
	[Export] public Texture2D locationTexture;
	[Export] public Vector3 locationPosition;
}

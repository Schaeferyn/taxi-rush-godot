using Godot;
using System;

public partial class TaxiSessionManager : Node
{
	public static TaxiSessionManager Instance;

	public Action<FareLocationData> OnFarePickup;
    
	public override void _EnterTree()
	{
		base._EnterTree();

		Instance = this;
	}

	public void FarePickedUp(TaxiFare pickup, FareDropoff dropoff)
	{
		OnFarePickup?.Invoke(dropoff.LocationData);
	}
}

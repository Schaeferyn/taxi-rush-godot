using Godot;
using System;

public partial class FareDropoff : Node3D
{
	private Area3D dropoffArea;
	private CollisionShape3D collision;

	private Node3D nodeScrolling;
	private Node3D nodeSolid;
	
	private bool isActive;
	private bool isTaxiNear;
	private PlayerTaxi nearbyTaxi;
	private bool isDroppingOff;
	
	public override void _Ready()
	{
		dropoffArea = GetNode<Area3D>("DropoffArea");
		dropoffArea.BodyEntered += OnBodyEntered;
		dropoffArea.BodyExited += OnBodyExited;
		
		collision = GetNode<CollisionShape3D>("DropoffArea/CollisionShape3D");
		
		nodeScrolling = GetNode<Node3D>("DropoffArea/scrolling");
		nodeSolid = GetNode<Node3D>("DropoffArea/solids");
		
		SetActive(false);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		dropoffArea.BodyEntered -= OnBodyEntered;
		dropoffArea.BodyExited -= OnBodyExited;
	}

	public override void _Process(double delta)
	{
		if (!isActive || !isTaxiNear || isDroppingOff) return;
		
		if (nearbyTaxi.LinearVelocity.Length() < 0.5f)
		{
			if (!isDroppingOff)
			{
				InitiateDropoff();
			}
				
			// //GlobalTranslate(-Transform.Basis.Z * (float)delta * walkSpeed);
			//
			// float finalDist = GlobalPosition.DistanceTo(taxiDoor.GlobalPosition);
			//
			// if (finalDist <= pickupFinalizeDistance)
			// {
			// 	//LocationData data = LocationManager.Instance.GetLocationByName(targetLocation);
			// 	FareDropoff dropoff = LocationManager.Instance.GetLocationByName(targetLocation);
			//
			// 	nearbyTaxi.FinalizeFarePickup(dropoff);
			// 	PlayerTaxiCamera.Instance.EndOverride();
			// 	
			// 	QueueFree();
			// }
		}
	}

	void InitiateDropoff()
	{
		isDroppingOff = true;
		SetActive(false);
	}

	public void SetActive(bool active)
	{
		GD.Print("OBJ: " + Name + " -- " + active);
		
		isActive = active;

		dropoffArea.Monitorable = active;
		dropoffArea.Monitoring = active;
		collision.Disabled = !active;

		nodeScrolling.Visible = active;
		nodeSolid.Visible = active;
	}

	void OnBodyEntered(Node3D body)
	{
		PlayerTaxi taxi = null;
		if (body is PlayerTaxi playerTaxi)
		{
			taxi = playerTaxi;
		}
		
		if (taxi == null) return;

		nearbyTaxi = taxi;
		isTaxiNear = true;
	}

	void OnBodyExited(Node3D body)
	{
		if (isDroppingOff) return;
		
		PlayerTaxi taxi = null;
		if (body is PlayerTaxi playerTaxi)
		{
			taxi = playerTaxi;
		}
		
		if (taxi == null) return;
		
		nearbyTaxi = null;
		isTaxiNear = false;
	}
}

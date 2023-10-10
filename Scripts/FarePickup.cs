using Godot;
using System;

public partial class FarePickup : Node3D
{
	private Area3D pickupArea;
	private CollisionShape3D pickupCollider;

	//[Export] private FareData myData;
	[Export] public string name;
	[Export] public int age;
	[Export] public string targetLocation;

	private bool isTaxiNear;
	private PlayerTaxi nearbyTaxi;
	private AnimationPlayer anim;

	private Node3D coinNode;
	[Export] private float coinSpeed = 4.0f;

	[Export] private float walkSpeed = 2.0f;

	private Node3D taxiDoor;
	private bool isBeingPickedUp;
	[Export] private float pickupFinalizeDistance = 0.1f;
	
	public override void _Ready()
	{
		pickupArea = GetNode<Area3D>("Area3D");
		pickupArea.BodyEntered += OnBodyEnter;
		pickupArea.BodyExited += OnBodyExit;

		pickupCollider = GetNode<CollisionShape3D>("Area3D/CollisionShape3D");

		anim = GetNode<AnimationPlayer>("character/AnimationPlayer");
		coinNode = GetNode<Node3D>("coin");
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		coinNode.RotateY(coinSpeed * (float)delta);
		
		if (isTaxiNear)
		{
			UpdateTaxiNear(delta);
		}
	}

	void UpdateTaxiNear(double delta)
	{
		GlobalTransform = GlobalTransform.LookingAt(taxiDoor.GlobalPosition, Vector3.Up);
		
		if (nearbyTaxi.LinearVelocity.Length() < 0.5f)
		{
			if (!isBeingPickedUp)
			{
				InitiatePickup();
			}
				
			GlobalTranslate(-Transform.Basis.Z * (float)delta * walkSpeed);
			
			float finalDist = GlobalPosition.DistanceTo(taxiDoor.GlobalPosition);

			if (finalDist <= pickupFinalizeDistance)
			{
				nearbyTaxi.FinalizeFarePickup();
				PlayerTaxiCamera.Instance.EndOverride();
				QueueFree();
			}
		}
	}

	void InitiatePickup()
	{
		isBeingPickedUp = true;
			
		nearbyTaxi.PickupFare(this);
		anim.Play("runLib/Run", 1.0);
				
		pickupArea.Monitoring = false;
		pickupCollider.Disabled = true;

		Vector3 camPosCurrent =
			PlayerTaxiCamera.Instance.GlobalPosition + (PlayerTaxiCamera.Instance.GlobalTransform.Basis.Z * 3);
		Vector3 camPosTarget = camPosCurrent.Lerp(GlobalPosition, 0.5f);
		camPosTarget.Y = 1.0f;
		PlayerTaxiCamera.Instance.ApplyOverride(camPosTarget, this, true);
	}

	void OnBodyEnter(Node3D body)
	{
		PlayerTaxi taxi = null;
		if (body is PlayerTaxi playerTaxi)
		{
			taxi = playerTaxi;
		}
		
		if (taxi == null) return;

		nearbyTaxi = taxi;
		isTaxiNear = true;
		
		if (GlobalPosition.DistanceTo(nearbyTaxi.DoorLeft.GlobalPosition) >
		    GlobalPosition.DistanceTo(nearbyTaxi.DoorRight.GlobalPosition))
		{
			taxiDoor = nearbyTaxi.DoorRight;
		}
		else
		{
			taxiDoor = nearbyTaxi.DoorLeft;
		}
	}

	void OnBodyExit(Node3D body)
	{
		if (isBeingPickedUp) return;
		
		PlayerTaxi taxi = null;
		if (body is PlayerTaxi playerTaxi)
		{
			taxi = playerTaxi;
		}
		
		if (taxi == null) return;
		
		//GD.Print("TAXI HAS EXITED THE FARE AREA");
		
		nearbyTaxi = null;
		taxiDoor = null;
		isTaxiNear = false;
	}
}

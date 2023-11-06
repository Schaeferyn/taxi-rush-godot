using Godot;
using System;

public partial class PlayerTaxi : VehicleBody3D
{
	private double steer;
	[Export] private float max_torque = 500;
	[Export] private float max_rpm = 500;
	[Export] private float brake_strength = 5.0f;
	[Export] private float steerChangeForce = 5.0f;
	[Export] private float steerPowerDegrees = 60.0f;

	private VehicleWheel3D wheel_frontLeft;
	private VehicleWheel3D wheel_frontRight;
	
	private VehicleWheel3D wheel_rearLeft;
	private VehicleWheel3D wheel_rearRight;

	private Node3D fareDoorLeft;
	private Node3D fareDoorRight;

	private Node3D arrow;
	private FareLocationData targetFareLocation;
	private bool hasTargetLocation;

	[Export] private MeshInstance3D taxiMesh;

	public Node3D DoorLeft => fareDoorLeft;
	public Node3D DoorRight => fareDoorRight;
	
	public bool CanDrive { get; private set; }
	public TaxiFare CurrentFare { get; private set; }
	
	public override void _Ready()
	{
		base._Ready();

		Engine.MaxFps = 60;

		wheel_frontLeft = GetNode<VehicleWheel3D>("wheel_front_left");
		wheel_frontRight = GetNode<VehicleWheel3D>("wheel_front_right");
		
		wheel_rearLeft = GetNode<VehicleWheel3D>("wheel_rear_left");
		wheel_rearRight = GetNode<VehicleWheel3D>("wheel_rear_right");

		fareDoorLeft = GetNode<Node3D>("fare_door_left");
		fareDoorRight = GetNode<Node3D>("fare_door_right");

		arrow = GetNode<Node3D>("ArrowParent");

		CanDrive = true;
		TaxiSessionManager.Instance.OnFarePickup += FarePickedUp;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		TaxiSessionManager.Instance.OnFarePickup -= FarePickedUp;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (hasTargetLocation)
		{
			arrow.LookAt(targetFareLocation.locationPosition);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!CanDrive) return;

		float steerAxis = Input.GetAxis("right", "left");
		steer = Mathf.Lerp(steer, steerAxis, steerChangeForce * delta);
		

		float abs = Mathf.Abs(steerAxis);
		//float steerPowerMult = abs > 0.5f ? 0.5f : 1.0f;

		Steering = (float)steer * Mathf.DegToRad(steerPowerDegrees);

		//float acceleration = Input.GetAxis("brake", "gas");
		float acceleration = Input.GetActionStrength("gas");
		float braking = Input.GetActionStrength("brake");
		
		float rpmL = Mathf.Abs(wheel_rearLeft.GetRpm());
		float rpmR = Mathf.Abs(wheel_rearLeft.GetRpm());
		wheel_rearLeft.EngineForce = acceleration * max_torque * (1 - (rpmL / max_rpm));
		wheel_rearRight.EngineForce = acceleration * max_torque * (1 - (rpmR / max_rpm));

		Brake = braking * brake_strength * (float)delta;

		Vector3 forward = GlobalTransform.Basis.Z;
		float angle = LinearVelocity.AngleTo(forward);
		LinearVelocity += GlobalTransform.Basis.X * (angle * 0.1f);

		//PlayerTaxiCamera.Instance.Fov = Mathf.Lerp(40, 75, rpmL / max_rpm);
	}

	public void PickupStarted(TaxiFare fare)
	{
		CanDrive = false;
		CurrentFare = fare;
		//(taxiMesh.GetActiveMaterial(0) as StandardMaterial3D).AlbedoColor = Colors.Black;
	}

	void FarePickedUp(FareLocationData locData)
	{
		targetFareLocation = locData;
		hasTargetLocation = true;

		arrow.Visible = true;
		CanDrive = false;
		//(taxiMesh.GetActiveMaterial(0) as StandardMaterial3D).AlbedoColor = Colors.Black;
	}

	public void FinalizeFarePickup(FareDropoff locData)
	{
		CanDrive = true;
		//(taxiMesh.GetActiveMaterial(0) as StandardMaterial3D).AlbedoColor = Colors.White;
		
		if (locData != null)
		{
			//GD.Print("Activating " + locData.locationName);
			//locData.dropoffPoint.SetActive(true);
			locData.SetActive(true);
		}
		else
		{
			GD.Print("NULL LOCATION");
		}
	}

	public void InitiateDropoff(Vector3 dropPosition)
	{
		arrow.Visible = false;
		CanDrive = false;
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
		//(taxiMesh.GetActiveMaterial(0) as StandardMaterial3D).AlbedoColor = Colors.Black;

		Vector3 appearPos = DoorLeft.GlobalPosition;
		if (dropPosition.DistanceTo(DoorLeft.GlobalPosition) >
		    dropPosition.DistanceTo(DoorRight.GlobalPosition))
		{
			appearPos = DoorRight.GlobalPosition;
		}

		CurrentFare.GlobalPosition = appearPos;
		CurrentFare.BeginDropoff();
	}

	public void FinalizeDropoff()
	{
		(taxiMesh.GetActiveMaterial(0) as StandardMaterial3D).AlbedoColor = Colors.White;
		
		CurrentFare.EndDropoff();
		CanDrive = true;
		CurrentFare = null;
	}
}

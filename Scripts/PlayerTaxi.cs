using Godot;
using System;

public partial class PlayerTaxi : VehicleBody3D
{
	private double steer;
	[Export] private float max_torque = 500;
	private float currentMaxTorque;
	[Export] private float max_speed = 13;
	private float currentMaxSpeed;
	[Export] private float brake_strength = 5.0f;
	[Export] private float steerChangeForce = 5.0f;
	[Export] private float steerPowerDegrees = 60.0f;

	private VehicleWheel3D wheel_frontLeft;
	private VehicleWheel3D wheel_frontRight;
	private float wheelFrictionFront;
	
	private VehicleWheel3D wheel_rearLeft;
	private VehicleWheel3D wheel_rearRight;
	private float wheelFrictionRear;

	private Node3D fareDoorLeft;
	private Node3D fareDoorRight;

	private Node3D arrow;
	private FareLocationData targetFareLocation;
	private bool hasTargetLocation;

	[Export] private MeshInstance3D taxiMesh;
	[Export] private Vector2 driftThresholds;
	[Export] private float extraDriftTorque = 100;
	[Export] private float extraDriftSpeed = 5.0f;
	[Export] private float extraDriftSlippage = 0.25f;
	[Export] private float handbrakeSlippage = 0.5f;
	private float addedDriftTorque;
	private float addedDriftSpeed;
	private float addedDriftSlippage;

	private bool isDrifting;
	private float driftStrength;

	private Label3D debugLabel;
	private Label3D debugLabel2;
	private Label3D debugLabelFL;
	private Label3D debugLabelFR;
	private Label3D debugLabelRL;
	private Label3D debugLabelRR;

	private bool isHandbraking;

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
		wheelFrictionFront = wheel_frontLeft.WheelFrictionSlip;
		
		wheel_rearLeft = GetNode<VehicleWheel3D>("wheel_rear_left");
		wheel_rearRight = GetNode<VehicleWheel3D>("wheel_rear_right");
		wheelFrictionRear = wheel_rearLeft.WheelFrictionSlip;

		fareDoorLeft = GetNode<Node3D>("fare_door_left");
		fareDoorRight = GetNode<Node3D>("fare_door_right");

		arrow = GetNode<Node3D>("ArrowParent");

		debugLabel = GetNode<Label3D>("Debug1");
		debugLabel2 = GetNode<Label3D>("Debug2");
		debugLabelFL = GetNode<Label3D>("DebugFL");
		debugLabelFR = GetNode<Label3D>("DebugFR");
		debugLabelRL = GetNode<Label3D>("DebugRL");
		debugLabelRR = GetNode<Label3D>("DebugRR");

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
		steer = steerAxis;

		Steering = (float)steer * Mathf.DegToRad(steerPowerDegrees);
		
		float acceleration = Input.GetActionStrength("gas");
		float braking = Input.GetActionStrength("brake");
		isHandbraking = Input.GetActionStrength("handbrake") > 0.5f;

		currentMaxTorque = max_torque + (isDrifting ? addedDriftTorque : 0);
		currentMaxSpeed = max_speed + (isDrifting ? addedDriftSpeed : 0);

		// wheel_frontLeft.WheelFrictionSlip = wheelFrictionFront - (isDrifting ? addedDriftSlippage : 0);
		// wheel_frontRight.WheelFrictionSlip = wheelFrictionFront - (isDrifting ? addedDriftSlippage : 0);
		wheel_frontLeft.WheelFrictionSlip = wheelFrictionFront;
		wheel_frontRight.WheelFrictionSlip = wheelFrictionFront;
		wheel_rearLeft.WheelFrictionSlip = wheelFrictionRear - (isHandbraking ? handbrakeSlippage : 0);
		wheel_rearRight.WheelFrictionSlip = wheelFrictionRear - (isHandbraking ? handbrakeSlippage : 0);
		
		debugLabelFL.Text = wheel_frontLeft.WheelFrictionSlip.ToString("0.00");
		debugLabelFR.Text = wheel_frontRight.WheelFrictionSlip.ToString("0.00");
		debugLabelRL.Text = wheel_rearLeft.WheelFrictionSlip.ToString("0.00");
		debugLabelRR.Text = wheel_rearRight.WheelFrictionSlip.ToString("0.00");
		
		// float rpmL = Mathf.Abs(wheel_rearLeft.GetRpm());
		// float rpmR = Mathf.Abs(wheel_rearLeft.GetRpm());
		// wheel_rearLeft.EngineForce = acceleration * currentMaxTorque * (1 - (rpmL / currentMaxRPM));
		// wheel_rearRight.EngineForce = acceleration * currentMaxTorque * (1 - (rpmR / currentMaxRPM));
		wheel_rearLeft.EngineForce = acceleration * currentMaxTorque;
		wheel_rearRight.EngineForce = acceleration * currentMaxTorque;

		if (isHandbraking)
		{
			Brake = 0.75f * brake_strength * (float)delta;
		}
		else
		{
			Brake = braking * brake_strength * (float)delta;
		}
		
		debugLabel2.Text = LinearVelocity.Length().ToString("0.00");

		if (LinearVelocity.Length() < 0.5f) return;
			
		float ang = Mathf.RadToDeg(Mathf.Atan2(LinearVelocity.X, LinearVelocity.Z));
		Vector3 rota = GlobalRotationDegrees;
		float angDiff = Mathf.Abs(rota.Y - ang);
		
		//GD.Print("A: " + angDiff);
		debugLabel.Text = angDiff.ToString("0.00");
		debugLabel.Modulate = isDrifting ? Colors.Fuchsia : Colors.White;
		debugLabel2.Modulate = isHandbraking ? Colors.Blue : Colors.White;

		isDrifting = angDiff > driftThresholds.X;
		
		if (!isDrifting && !isHandbraking) return;
		
		driftStrength = (angDiff - driftThresholds.X) / (driftThresholds.Y - driftThresholds.X);
		driftStrength = Mathf.Clamp(driftStrength, 0.0f, 1.0f);
		
		addedDriftTorque = extraDriftTorque * driftStrength;
		addedDriftSpeed = extraDriftSpeed * driftStrength;
		addedDriftSlippage = extraDriftSlippage;
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

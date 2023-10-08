using Godot;
using System;

public partial class PlayerTaxi : VehicleBody3D
{
	private double steer;
	private float max_torque = 500;
	private float max_rpm = 500;

	private VehicleWheel3D wheel_rearLeft;
	private VehicleWheel3D wheel_rearRight;
	
	public override void _Ready()
	{
		base._Ready();

		Engine.MaxFps = 60;

		wheel_rearLeft = GetNode<VehicleWheel3D>("wheel_rear_left");
		wheel_rearRight = GetNode<VehicleWheel3D>("wheel_rear_right");
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		float steerAxis = Input.GetAxis("right", "left");
		steer = Mathf.Lerp(steer, steerAxis * 0.4f, 5.0d * delta);
		

		float abs = Mathf.Abs(steerAxis);
		//float steerPowerMult = abs > 0.5f ? 0.5f : 1.0f;
		float steerPowerMult = 1.0f;
		
		Steering = (float)steer * steerPowerMult;

		float acceleration = Input.GetAxis("brake", "gas");
		
		float rpmL = Mathf.Abs(wheel_rearLeft.GetRpm());
		float rpmR = Mathf.Abs(wheel_rearLeft.GetRpm());
		wheel_rearLeft.EngineForce = acceleration * max_torque * (1 - (rpmL / max_rpm));
		wheel_rearRight.EngineForce = acceleration * max_torque * (1 - (rpmR / max_rpm));

		PlayerTaxiCamera.Instance.Fov = Mathf.Lerp(40, 75, rpmL / max_rpm);
	}
}
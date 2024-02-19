using Godot;
using System;

public partial class PlayerTaxiCamera : Camera3D
{
	public static PlayerTaxiCamera Instance;
    
	private Node3D camTarget;
	private Transform3D t_prev;
	private Transform3D t_current;
	
	private float followLerp = 5.0f;

	private bool shouldUpdateTransforms;

	private bool hasTargetOverride;
	private bool snapToOverride;
	private Vector3 positionOverride;
	private Node3D lookTargetOveride;

	public Vector3 CameraGlobalPosition => GlobalPosition;

	public override void _EnterTree()
	{
		base._EnterTree();

		Instance = this;
	}

	public override void _Ready()
	{
		TopLevel = true;
		
		camTarget = GetNode<Node3D>("/root/world/taxi/camera_target");
	}
	
	public override void _Process(double delta)
	{
		if (hasTargetOverride)
		{
			ProcessOverrideCamera(delta);
		}
		else
		{
			ProcessStandardCamera(delta);
		}
	}

	void ProcessOverrideCamera(double delta)
	{
		GlobalPosition = positionOverride;
		GlobalTransform = GlobalTransform.LookingAt(lookTargetOveride.GlobalPosition, Vector3.Up);
	}

	void ProcessStandardCamera(double delta)
	{
		GlobalTransform = GlobalTransform.InterpolateWith(camTarget.GlobalTransform, (float)delta * followLerp);

		Vector3 rot = GlobalRotation;
		rot.Z = 0;
		GlobalRotation = rot;
	}

	// void UpdateTransforms()
	// {
	// 	t_prev = t_current;
	// 	t_current = camTarget.GlobalTransform;
	// 	shouldUpdateTransforms = false;
	// }
	//
	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	//
	// 	shouldUpdateTransforms = true;
	// }

	public void ApplyOverride(Vector3 pos, Node3D lookTarget, bool snap)
	{
		hasTargetOverride = true;

		positionOverride = pos;
		lookTargetOveride = lookTarget;
		snapToOverride = snap;
	}

	public void EndOverride()
	{
		hasTargetOverride = false;
		positionOverride = Vector3.Zero;
		lookTargetOveride = null;
	}
}

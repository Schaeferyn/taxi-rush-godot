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
		GlobalTransform = GlobalTransform.InterpolateWith(camTarget.GlobalTransform, (float)delta * followLerp);

		Vector3 rot = GlobalRotation;
		rot.Z = 0;
		GlobalRotation = rot;
	}

	void UpdateTransforms()
	{
		t_prev = t_current;
		t_current = camTarget.GlobalTransform;
		shouldUpdateTransforms = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		shouldUpdateTransforms = true;
	}
}

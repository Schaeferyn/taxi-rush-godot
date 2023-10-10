using Godot;
using System;

public partial class TaxiSessionManager : Node
{
	public static TaxiSessionManager Instance;
    
	public override void _EnterTree()
	{
		base._EnterTree();

		Instance = this;
	}
}

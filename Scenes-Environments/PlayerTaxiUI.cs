using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerTaxiUI : ControlTimedActions
{
	public static PlayerTaxiUI Instance;

	[Export] private TextureRect locationImage;
	[Export] private Label locationName;
	[Export] private CanvasItem locationParent;

	public override void _EnterTree()
	{
		base._EnterTree();

		Instance = this;
	}

	public override void _Ready()
	{
		base._Ready();
		
		SetTargetLocationVisibility(false);

		TaxiSessionManager.Instance.OnFarePickup += ShowTargetLocation;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		TaxiSessionManager.Instance.OnFarePickup -= ShowTargetLocation;
	}

	void ShowTargetLocation(FareLocationData locData)
	{
		locationImage.Texture = locData.locationTexture;
		locationName.Text = locData.locationName;

		SetTargetLocationVisibility(true);
		
		BeginTimerEvent(5.0f, HideTargetLocation);
	}

	void HideTargetLocation()
	{
		SetTargetLocationVisibility(false);
	}

	void SetTargetLocationVisibility(bool isVisible)
	{
		locationParent.Visible = isVisible;
	}
}


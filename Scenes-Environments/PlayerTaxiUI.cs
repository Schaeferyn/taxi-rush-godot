using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerTaxiUI : ControlTimedActions
{
	public static PlayerTaxiUI Instance;

	[Export] private TextureRect locationImage;
	[Export] private Label locationName;
	[Export] private CanvasItem locationParent;
	[Export] private PlayerTaxi taxi;

	private ColorRect gearUIDrive;
	private ColorRect gearUIReverse;

	public override void _EnterTree()
	{
		base._EnterTree();

		Instance = this;
	}

	public override void _Ready()
	{
		base._Ready();

		gearUIDrive = GetNode<ColorRect>("GearUIBG/GearUIDrive");
		gearUIReverse = GetNode<ColorRect>("GearUIBG/GearUIReverse");
		
		SetTargetLocationVisibility(false);

		TaxiSessionManager.Instance.OnFarePickup += ShowTargetLocation;
		taxi.OnReverseToggled += ReverseToggled;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		TaxiSessionManager.Instance.OnFarePickup -= ShowTargetLocation;
		taxi.OnReverseToggled -= ReverseToggled;
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

	void ReverseToggled(bool isReversing)
	{
		gearUIDrive.Visible = !isReversing;
		gearUIReverse.Visible = isReversing;
	}
}


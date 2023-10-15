using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerTaxiUI : Control
{
	public static PlayerTaxiUI Instance;

	[Export] private TextureRect locationImage;
	[Export] private Label locationName;
	[Export] private CanvasItem locationParent;

	private Action timerCall;
	private float f_timer;
	private bool b_timerRunning;

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

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (b_timerRunning)
		{
			f_timer -= (float)delta;
			if (f_timer <= 0)
			{
				b_timerRunning = false;
				timerCall?.Invoke();
			}
		}
	}

	void ShowTargetLocation(FareLocationData locData)
	{
		locationImage.Texture = locData.locationTexture;
		locationName.Text = locData.locationName;

		SetTargetLocationVisibility(true);

		//Task.Delay(5000).ContinueWith(t => SetTargetLocationVisibility(true));
		
		BeginTimerEvent(5.0f, HideTargetLocation);
	}

	void BeginTimerEvent(float totalTime, Action callAction)
	{
		b_timerRunning = true;
		f_timer = totalTime;
		timerCall = callAction;
	}

	void HideTargetLocation()
	{
		SetTargetLocationVisibility(false);
	}

	void SetTargetLocationVisibility(bool isVisible)
	{
		locationParent.Visible = isVisible;
	}
	
	void DelayCall(int msec, Action fn)
	{
		System.Threading.Timer timer = null; 
		timer = new System.Threading.Timer((obj) =>
			{
				fn.Invoke();
				timer.Dispose();
			}, 
			null, msec, System.Threading.Timeout.Infinite);
	}
}


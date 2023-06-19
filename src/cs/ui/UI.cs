using Godot;
using System;

// General controller for the UI
public partial class UI : CanvasLayer {

	// Button that triggers the passage to a next turn
	private Button NextTurnButton;

	// The two energy bars, showing the availability and demand
	private InfoBar WinterEnergy;
	private InfoBar SummerEnergy;
	
	// The two information bars
	private InfoBar EnvironmentBar;
	private InfoBar SupportBar;

	// Date progression
	private HSlider Timeline;

	// Money related nodes
	private Label MoneyL;
	private Button MoneyButton;
	private ColorRect MoneyInfo;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch Nodes
		NextTurnButton = GetNode<Button>("Bottom/NextTurn/NextTurn");

		// Info Bars
		WinterEnergy = GetNode<InfoBar>("Bottom/EnergyBarWinter");
		SummerEnergy = GetNode<InfoBar>("Bottom/EnergyBarSummer");
		EnvironmentBar = GetNode<InfoBar>("Bottom/Env");
		SupportBar = GetNode<InfoBar>("Bottom/Trust");

		// Timeline
		Timeline = GetNode<HSlider>("Top/Timeline");

		// Money Nodes
		MoneyL = GetNode<Label>("Top/Money/money");
		MoneyButton = GetNode<Button>("Top/MoneyUI");
		MoneyInfo = GetNode<ColorRect>("Top/MoneyInfo");

		// Connect Various signals
		MoneyButton.Pressed += _OnMoneyButtonPressed;
		NextTurnButton.Pressed += _OnNextTurnPressed;
		WinterEnergy.MouseEntered += _OnWinterEnergyMouseEntered;
		WinterEnergy.MouseExited += _OnWinterEnergyMouseExited;
		SummerEnergy.MouseEntered += _OnSummerEnergyMouseEntered;
		SummerEnergy.MouseExited += _OnSummerEnergyMouseExited;
		EnvironmentBar.MouseEntered += _OnEnvironmentMouseEntered;
		EnvironmentBar.MouseExited += _OnEnvironmentMouseExited;
		SupportBar.MouseEntered += _OnSupportMouseEntered;
		SupportBar.MouseExited += _OnSupportMouseExited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// Displays additional details about the money usage
	public void _OnMoneyButtonPressed() {
		// Simply toggle the money info
		if(MoneyInfo.Visible) {
			MoneyInfo.Hide();
		} else {
			MoneyInfo.Show();
		}
	}

	// Updates the timelines and propagates the request up to the game loop
	public void _OnNextTurnPressed() {
		//TODO: Requires functioning game loop
	}

	// Displays the information box related to the winter energy
	public void _OnWinterEnergyMouseEntered() {
		// Display the energy info
		WinterEnergy._DisplayInfo();
	}
	// Hides the information box related to the winter energy
	public void _OnWinterEnergyMouseExited() {
		WinterEnergy._HideInfo();
	}

	// Displays the information box related to the summer energy
	public void _OnSummerEnergyMouseEntered() {
		SummerEnergy._DisplayInfo();
	}
	// Hides the information box related to the summer energy
	public void _OnSummerEnergyMouseExited() {
		SummerEnergy._HideInfo();
	}

	// Displays the information box related to the environment bar
	public void _OnEnvironmentMouseEntered() {
		EnvironmentBar._DisplayInfo();
	}
	// Hides the information box related to the environment bar
	public void _OnEnvironmentMouseExited() {
		EnvironmentBar._HideInfo();
	}

	// Displays the information box related to the Support bar
	public void _OnSupportMouseEntered() {
		SupportBar._DisplayInfo();
	}
	// Hides the information box related to the Support bar
	public void _OnSupportMouseExited() {
		SupportBar._HideInfo();
	}
}

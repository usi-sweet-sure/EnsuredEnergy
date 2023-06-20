using Godot;
using System;
using System.Diagnostics;

// Data structure for the information displayed in the info boxes
public struct InfoData {
	// === Field Numbers for each type ===
	public const int N_W_ENERGY_FIELDS = 2;
	public const int N_S_ENERGY_FIELDS = 2;
	public const int N_ENV_FIELDS = 3;
	public const int N_SUPPORT_FIELDS = 2;
	public const int N_MONEY_FIELDS = 3;

	
	// === Energy metrics ===
	public int W_EnergyDemand; // Energy demand for the winter season
	public int W_EnergySupply; // Energy supply for the winter season
	public int S_EnergyDemand; // Energy demand for the summer season
	public int S_EnergySupply; // Energy supply for the summer season

	// === Support metrics ===
	public int EnergyAffordability; // Used in the support bar
	public int EnvAesthetic; // Also used in the support bar

	// === Environment metrics ===
	public int LandUse; // Used in the environment bar
	public int Pollution; // Also for the environment bar
	public int Biodiversity; // For the environment bar

	// === Money Metrics ===
	public int Budget; // The amount of money you are generating this turn
	public int Production; // The amount of money used for production this turn
	public int Building; // The amount of money spent on building this turn

	// Constructor for the Data
	public InfoData() {
		W_EnergyDemand = 0; 
		W_EnergySupply = 0; 
		S_EnergyDemand = 0; 
		S_EnergySupply = 0; 

		EnergyAffordability = 0; 
		EnvAesthetic = 0; 

		LandUse = 0;
		Pollution = 0;
		Biodiversity = 0; 

		Budget = 0;
		Production = 0;
		Building = 0;
	}
}

// General controller for the UI
public partial class UI : CanvasLayer {
	// Describes the type of bar that contains information about certain metrics
	public enum InfoType { W_ENGERGY, S_ENGERGY, SUPPORT, ENVIRONMENT, MONEY };

	// Contains the data displayed in the UI
	private InfoData Data;

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
	private Label BudgetL;
	private Label BuildL;
	private Label ProdL;

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
		BudgetL = GetNode<Label>("Top/MoneyInfo/budget");
		BuildL = GetNode<Label>("Top/MoneyInfo/build");
		ProdL = GetNode<Label>("Top/MoneyInfo/prod");

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

		// Initialize data
		Data = new InfoData();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// ==================== UI Update API ====================

	// Updates the value of the a given bar
	public void _UpdateBarValue(InfoType t, int val) {
		switch (t) {
			case InfoType.W_ENGERGY:
				WinterEnergy._UpdateProgress(val);
				break;
			case InfoType.S_ENGERGY:
				SummerEnergy._UpdateProgress(val);
				break;
			case InfoType.SUPPORT:
				SupportBar._UpdateProgress(val);
				break;
			case InfoType.ENVIRONMENT:
				EnvironmentBar._UpdateProgress(val);
				break;
			default:
				break;
		}
	}

	// Update the internal UI data
	// This is done following the ordering of the fields in the InfoData struct
	// Energy: demand, supply
	// Support: energy_affordability, env_aesthetic
	// Environment: land_use, pollution, biodiversity
	// Money: budget, production, building
	public void _UpdateData(InfoType t, params int[] d) {
		switch (t) {
			case InfoType.W_ENGERGY:
				// Sanity check, make sure that you were given enough fields
				Debug.Assert(d.Length >= InfoData.N_W_ENERGY_FIELDS);

				// Set the fields in order, for energy it's demand, supply
				Data.W_EnergyDemand = d[0];
				Data.W_EnergySupply = d[1];
				break;
			case InfoType.S_ENGERGY:
				// Sanity check, make sure that you were given enough fields
				Debug.Assert(d.Length >= InfoData.N_S_ENERGY_FIELDS);

				// Set the fields in order, for energy it's demand, supply
				Data.S_EnergyDemand = d[0];
				Data.S_EnergySupply = d[1];
				break;
			case InfoType.SUPPORT:
				// Sanity check, make sure that you were given enough fields
				Debug.Assert(d.Length >= InfoData.N_SUPPORT_FIELDS);

				// Set the fields in order, for support it's affordability, aesthetic
				Data.EnergyAffordability = d[0];
				Data.EnvAesthetic = d[1];
				break;
			case InfoType.ENVIRONMENT:
				// Sanity check, make sure that you were given enough fields
				Debug.Assert(d.Length >= InfoData.N_ENV_FIELDS);

				// Set the fields in order, for environment it's landuse, pollution, biodiversity
				Data.LandUse = d[0];
				Data.Pollution = d[1];
				Data.Biodiversity = d[2];
				break;
			case InfoType.MONEY:
				// Sanity check, make sure that there are enough fields
				Debug.Assert(d.Length >= InfoData.N_MONEY_FIELDS);

				// Set the fields in order, for money it's budget, production, building
				Data.Budget = d[0];
				Data.Production = d[1];
				Data.Building = d[2];
				break;
			default:
				break;
		}
	}

	// ==================== Internal Helpers ====================

	// Sets the energy in
	private void SetEnergyInfo(ref InfoBar eng, InfoType t) {
		// Sanity check
		Debug.Assert(t == InfoType.W_ENGERGY || t == InfoType.S_ENGERGY);

		// Extract data based on energy type
		int demand = t == InfoType.W_ENGERGY ? Data.W_EnergyDemand : Data.S_EnergyDemand;
		int supply = t == InfoType.W_ENGERGY ? Data.W_EnergySupply : Data.S_EnergySupply;

		// Set the info
		eng._UpdateInfo(
			"n/max", // N/Max TODO: Figure out what to use here
			"Demand", demand.ToString(), // T0, N0
			"Supply", supply.ToString() // T1, N1
		);
	}

	// Sets the information fields for the support bar
	private void SetSupportInfo() {
		SupportBar._UpdateInfo(
			"n/max", // N/Max TODO: Figure out what to use here
			"Affordability", Data.EnergyAffordability.ToString(), // T0, N0
			"Aesthetic", Data.EnvAesthetic.ToString() // T1, N1
		);
	}

	// Sets the information fields for the environment bar
	private void SetEnvironmentInfo() {
		EnvironmentBar._UpdateInfo(
			"n/max", // N/Max TODO: Figure out what to use here
			"Land Use", Data.LandUse.ToString(), // T0, N0
			"Pollution", Data.Pollution.ToString(), // T1, N1
			"Biodiversity", Data.Biodiversity.ToString() // T2, N2
		);
	}

	// Sets the information related to the money metric
	private void SetMoneyInfo() {
		BudgetL.Text = Data.Budget.ToString();
		BuildL.Text = Data.Building.ToString();
		ProdL.Text = Data.Production.ToString();
	}

	// ==================== Interaction Callbacks ====================

	// Displays additional details about the money usage
	public void _OnMoneyButtonPressed() {
		// Simply toggle the money info
		if(MoneyInfo.Visible) {
			MoneyInfo.Hide();
		} else {
			// Set the info first
			SetMoneyInfo();

			// Finally display it
			MoneyInfo.Show();
		}
	}

	// Updates the timelines and propagates the request up to the game loop
	public void _OnNextTurnPressed() {
		//TODO: Requires functioning game loop
	}

	// Displays the information box related to the winter energy
	public void _OnWinterEnergyMouseEntered() {
		 SetEnergyInfo(ref WinterEnergy, InfoType.W_ENGERGY);

		 // Display the energy info
		 WinterEnergy._DisplayInfo();
	}
	// Hides the information box related to the winter energy
	public void _OnWinterEnergyMouseExited() {
		WinterEnergy._HideInfo();
	}

	// Displays the information box related to the summer energy
	public void _OnSummerEnergyMouseEntered() {
		SetEnergyInfo(ref SummerEnergy, InfoType.S_ENGERGY);

		// Display the energy info
		SummerEnergy._DisplayInfo();
	}
	// Hides the information box related to the summer energy
	public void _OnSummerEnergyMouseExited() {
		SummerEnergy._HideInfo();
	}

	// Displays the information box related to the environment bar
	public void _OnEnvironmentMouseEntered() {
		// Set the information first
		SetEnvironmentInfo();

		// Then show the info
		EnvironmentBar._DisplayInfo();
	}
	// Hides the information box related to the environment bar
	public void _OnEnvironmentMouseExited() {
		EnvironmentBar._HideInfo();
	}

	// Displays the information box related to the Support bar
	public void _OnSupportMouseEntered() {
		// Set the information first
		SetSupportInfo();

		// Show the new info
		SupportBar._DisplayInfo();
	}
	// Hides the information box related to the Support bar
	public void _OnSupportMouseExited() {
		SupportBar._HideInfo();
	}
}

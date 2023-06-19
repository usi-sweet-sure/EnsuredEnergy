using Godot;
using System;
using System.Collections.Generic;

public enum BuildingType { HYDRO, GAS, SOLAR, TREE, NUCLEAR };

public struct BuildLocation {
	// Contains the Position at which the build wild take place
	public Vector2 Position;

	// Contains the types we are allowed to build at this location
	public List<BuildingType> AvailableTypes; 

	// Struct constructor using var-args for the available types
	public BuildLocation(Vector2 _P, params BuildingType[] _BT) {
		Position = _P;
		AvailableTypes = new List<BuildingType>();

		// Fill in the available types
		foreach(var bt in _BT) {
			AvailableTypes.Add(bt);
		}
	}
}

public partial class BuildButton : Button {
	// Signal used to trigger the showing of the build menu
	[Signal]
	public delegate void ShowBuildMenuEventHandler(BuildButton bb);

	// The only special plant for the time being is Hydro
	// This flag tells us whether or not it is permitted to build a hydro plant at this location.
	[Export]
	public bool AllowHydro = false;
	
	public BuildLocation BL;
	private BuildMenu BM;

	// Power Plants
	private PowerPlant GasPlant;
	private PowerPlant SolarPlant;
	private PowerPlant HydroPlant;
	private PowerPlant TreePlant;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch Node  
		BM = GetNode<BuildMenu>("../../BuildMenu");

		// Fetch Power plants
		GasPlant = GetNode<PowerPlant>("Gas");
		SolarPlant = GetNode<PowerPlant>("Solar");
		HydroPlant = GetNode<PowerPlant>("Hydro");
		TreePlant = GetNode<PowerPlant>("Tree");

		// Initially hide all of the plants
		HideAllPlants();

		// Connect the onShowBuildMenu callback to our signal
		ShowBuildMenu += BM._OnShowBuildMenu;

		// Make sure that the location is set correctly
		if(AllowHydro) {
			BL = new BuildLocation(Position, BuildingType.GAS, BuildingType.SOLAR, BuildingType.TREE, BuildingType.HYDRO);
		} else {
			BL = new BuildLocation(Position, BuildingType.GAS, BuildingType.SOLAR, BuildingType.TREE);
		}

		// Connect the button press callback
		this.Pressed += _OnPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// Show the build menu containing the correct buildings to build
	public void _OnPressed() {
		BM.Show();
		EmitSignal(SignalName.ShowBuildMenu, this);
	}

	// Hides the button but not its children
	private void HideOnlyButton() {
		Text = "";
		Disabled = true;
		Flat = true;
	}

	// Resets the button to it's initial state
	private void Reset() {
		Text = "🔨";
		Disabled = false;
		Flat = false;
	}

	// Hides all of the plants related to this button
	private void HideAllPlants() {
		// Hide all plants
		GasPlant.Hide();
		HydroPlant.Hide();
		SolarPlant.Hide();
		TreePlant.Hide();
	}

	// Creates a new power plant on the current build spot
	public void _SpawnPowerPlant(BuildingType Bt, int buildC, int prodC, int eng) {
		// Create a new power plant that will be places at our current button's location
		PowerPlant newPP = new PowerPlant();

		// Connect it to our parent
		Owner.AddChild(newPP);

		// Set it up to display at our button's location
		newPP.Position = Vector2.Zero;
		newPP.Scale = new Vector2(1, 1);
		newPP.IsPreview = false;
		newPP.BuildCost = buildC;
		newPP.ProductionCost = prodC;
		newPP.EnergyProduction = eng;
		newPP.PlantType = Bt;

		// Force name to be consistent with type
		switch(Bt) {
			case BuildingType.GAS:
				newPP.PlantName = "Gas Plant";
				break;
			
			case BuildingType.HYDRO:
				newPP.PlantName = "Hydro Plant";
				break;

			case BuildingType.SOLAR:
				newPP.PlantName = "Solar Plant";
				break;
			
			case BuildingType.TREE:
				newPP.PlantName = "Trees";
				break;
		}

		// Show the new power plant
		newPP._Ready();
		newPP.Show();
	}

	// Updates a given power plant to match the received power plant
	private void UpdatePowerPlant(ref PowerPlant PP, PowerPlant PPRec) {
		// Set it up to display at our button's location
		PP.Position = Vector2.Zero;
		PP.Scale = new Vector2(1, 1);
		PP.IsPreview = false;
		PP.BuildCost = PPRec.BuildCost;
		PP.ProductionCost = PPRec.ProductionCost;
		PP.EnergyProduction = PPRec.EnergyProduction;
		PP.PlantType = PPRec.PlantType;

		// Force name to be consistent with type
		switch(PP.PlantType) {
			case BuildingType.GAS:
				PP.PlantName = "Gas Plant";
				break;
			
			case BuildingType.HYDRO:
				PP.PlantName = "Hydro Plant";
				break;

			case BuildingType.SOLAR:
				PP.PlantName = "Solar Plant";
				break;
			
			case BuildingType.TREE:
				PP.PlantName = "Trees";
				break;
		}

		// Make sure that the data is propagated to the UI
		PP._UpdatePlantData();
		PP.Show();
	}

	// Receives the power plant selected by the user and now we need to place it
	public void _OnSelectBuilding(PowerPlant PP) {
		// Hide the build menu UI
		BM.Hide();

		// Hide all plants
		HideAllPlants();

		// Select which plant to show and update it's fields to match those that were given
		switch(PP.PlantType) {
			case BuildingType.GAS:
				UpdatePowerPlant(ref GasPlant, PP);
				break;
			
			case BuildingType.HYDRO:
				UpdatePowerPlant(ref HydroPlant, PP);
				break;

			case BuildingType.SOLAR:
				UpdatePowerPlant(ref SolarPlant, PP);
				break;
			
			case BuildingType.TREE:
				UpdatePowerPlant(ref TreePlant, PP);
				break;
		}

		// Hide our button
		HideOnlyButton();
	}
}

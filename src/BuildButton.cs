using Godot;
using System;
using System.Collections.Generic;

public enum BuildingType { HYDRO, GAS, SOLAR, TREE };

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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch Node  
		BM = GetNode<BuildMenu>("../BuildMenu");

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

	// Receives the power plant selected by the user and now we need to place it
	public void _OnSelectBuilding(PowerPlant PP) {
		// Hide the build menu UI
		BM.Hide();

		// Create a new power plant that will be places at our current button's location
		PowerPlant newPP = new PowerPlant();

		// Connect it to our parent
		Owner.AddChild(newPP);

		// Set it up to display at our button's location
		newPP.Position = BL.Position;
		newPP.Scale = new Vector2(1, 1);
		newPP.IsPreview = false;
		newPP.BuildCost = PP.BuildCost;
		newPP.ProductionCost = PP.ProductionCost;
		newPP.EnergyProduction = PP.EnergyProduction;
		newPP.PlantType = PP.PlantType;

		// Force name to be consistent with type
		switch(PP.PlantType) {
			case BuildingType.GAS:
				newPP.PlantName = "Gas Plant";
				break;
			
			case BuildingType.HYDRO:
				newPP.PlantName = "Hydroelectric Plant";
				break;

			case BuildingType.SOLAR:
				newPP.PlantName = "Solar Plant";
				break;
			
			case BuildingType.TREE:
				newPP.PlantName = "Trees";
				break;
		}

		// Show the new power plant
		newPP.Show();

		// Hide our button
		Hide();
	}
}

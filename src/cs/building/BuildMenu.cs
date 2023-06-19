using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

// Represents the menu that pops up when a build is requested
public partial class BuildMenu : CanvasLayer {
	// Returns the result of the building selection to the build button
	[Signal]
	public delegate void SelectBuildingEventHandler(PowerPlant PP);

	[Export] 
	public Vector2 BuildingSpriteBase = new Vector2(150, 100);
	[Export]
	public int BuildingSpriteOffset = 150;
	[Export]
	public string BuildingAssetPathBase = "res://assets";

	// Power Plants
	private PowerPlant GasPlant;
	private PowerPlant SolarPlant;
	private PowerPlant HydroPlant;
	private PowerPlant TreePlant;

	// Buttons to select the power plants
	private Button GasButton;
	private Button SolarButton;
	private Button HydroButton;
	private Button TreeButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Initially hide this menu
		Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}
	
	// Sets the position of the given plant according to its position in the list
	private void SetPlantPosition(ref PowerPlant pp, int idx) {
		pp.Position = new Vector2(
			BuildingSpriteBase.X + (idx * BuildingSpriteOffset), 
			BuildingSpriteBase.Y
		);

		// Show the plant once the position is set
		pp.Show();
	}

	// When the "ShowBuildMenu" signal is triggered, show the given powerplants 
	// in the given order using the predefined base and offset.
	public void _OnShowBuildMenu(BuildButton bb) {
		// Sanity check: List must not contain duplicates
		List<BuildingType> BTs = bb.BL.AvailableTypes.Distinct().ToList();

		// Connect the build button to our return signal
		SelectBuilding += bb._OnSelectBuilding;

		// Display the buildings sent with the signal  
		int idx = 0;
		foreach(var _bt in BTs) {
			switch (_bt) {
				case BuildingType.GAS:
					// Position the plant correctly
					SetPlantPosition(ref GasPlant, idx++);
					break;

				case BuildingType.HYDRO:
					SetPlantPosition(ref HydroPlant, idx++);
					break;

				case BuildingType.SOLAR:
					SetPlantPosition(ref SolarPlant, idx++);
					break;

				case BuildingType.TREE:
					SetPlantPosition(ref TreePlant, idx++);
					break;
			}
		}
	} 

	// Set of stupid button callbacks
	public void _OnGasButtonPressed() {
		EmitSignal(SignalName.SelectBuilding, GasPlant);
	}
	public void _OnHydroButtonPressed() {
		EmitSignal(SignalName.SelectBuilding, HydroPlant);
	}
	public void _OnSolarButtonPressed() {
		EmitSignal(SignalName.SelectBuilding, SolarPlant);
	}
	public void _OnTreeButtonPressed() {
		EmitSignal(SignalName.SelectBuilding, TreePlant);
	}
}

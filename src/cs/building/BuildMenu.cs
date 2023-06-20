/**
	Sustainable Energy Development game modeling the Swiss energy Grid.
	Copyright (C) 2023 Università della Svizzera Italiana

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
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
	public int BuildingSpriteOffset = 250;
	[Export]
	public string BuildingAssetPathBase = "res://assets";

	public bool IsOpen = false;
	
	// Reference to the currently registered button (only one is allowed)
	private BuildButton RegisteredBuildButton;

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

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Initially hide this menu
		Hide();

		// Fetch Power plants
		GasPlant = GetNode<PowerPlant>("ColorRect/Gas");
		SolarPlant = GetNode<PowerPlant>("ColorRect/Solar");
		HydroPlant = GetNode<PowerPlant>("ColorRect/Hydro");
		TreePlant = GetNode<PowerPlant>("ColorRect/Tree");

		// Fetch associated buttons
		GasButton = GetNode<Button>("ColorRect/Gas/GasButton");
		SolarButton = GetNode<Button>("ColorRect/Solar/SolarButton");
		HydroButton = GetNode<Button>("ColorRect/Hydro/HydroButton");
		TreeButton = GetNode<Button>("ColorRect/Tree/TreeButton");

		// Connect the associated button callbacks
		GasButton.Pressed += _OnGasButtonPressed;
		SolarButton.Pressed += _OnSolarButtonPressed;
		HydroButton.Pressed += _OnHydroButtonPressed;
		TreeButton.Pressed += _OnTreeButtonPressed;

		HideAllPlants();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// ==================== Internal Helpers ====================

	// Hides all of the plants related to this button
	private void HideAllPlants() {
		// Hide all plants
		GasPlant.Hide();
		HydroPlant.Hide();
		SolarPlant.Hide();
		TreePlant.Hide();
	}
	
	// Sets the position of the given plant according to its position in the list
	private void SetPlantPosition(ref PowerPlant pp, int idx) {
		pp.Position = new Vector2(
			BuildingSpriteBase.X + (idx * BuildingSpriteOffset), 
			BuildingSpriteBase.Y
		);

		// Make sure that these plants are in preview mode
		pp._UpdateIsPreview(true);

		// Show the plant once the position is set
		pp.Show();
	}

	// ==================== Interaction Callbacks ====================

	// When the "ShowBuildMenu" signal is triggered, show the given powerplants 
	// in the given order using the predefined base and offset.
	public void _OnShowBuildMenu(BuildButton bb) {
		// What happens when we press another button when the menu is open
		if(IsOpen) {
			// Clear the Menu
			HideAllPlants();

			// Deregister the previous Build Button from the selection signal
			// This is done to avoid double building spawns
			SelectBuilding -= RegisteredBuildButton._OnSelectBuilding;
		}

		// Make sure that the previous registered button is deregistered
		if(RegisteredBuildButton != null) {
			SelectBuilding -= RegisteredBuildButton._OnSelectBuilding;
		}

		// Update open flag
		IsOpen = true;

		// Sanity check: List must not contain duplicates
		List<BuildingType> BTs = bb.BL.AvailableTypes.Distinct().ToList();

		// Connect the build button to our return signal and register it
		SelectBuilding += bb._OnSelectBuilding;
		RegisteredBuildButton = bb;

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
		// Send out the selection
		EmitSignal(SignalName.SelectBuilding, GasPlant);

		// Close the menu
		IsOpen = false;
	}
	public void _OnHydroButtonPressed() {
		// Send out the selection
		EmitSignal(SignalName.SelectBuilding, HydroPlant);

		// Close the menu
		IsOpen = false;
	}
	public void _OnSolarButtonPressed() {
		// Send out the selection
		EmitSignal(SignalName.SelectBuilding, SolarPlant);

		// Close the menu
		IsOpen = false;
	}
	public void _OnTreeButtonPressed() {
		// Send out the selection
		EmitSignal(SignalName.SelectBuilding, TreePlant);

		// Close the menu
		IsOpen = false;
	}
}

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
	private PowerPlant WindPlant;

	// Buttons to select the power plants
	private Button GasButton;
	private Button SolarButton;
	private Button HydroButton;
	private Button TreeButton;
	private Button WindButton;

	// Close button
	private Button CloseButton;
	
	// Animation Player
	private AnimationPlayer BuildMenuAP;
	
	// Tab Container
	private TabContainer TabC;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Initially hide this menu
		Hide();

		// Fetch Power plants
		GasPlant = GetNode<PowerPlant>("TabContainer/TabBar2/Gas");
		SolarPlant = GetNode<PowerPlant>("TabContainer/TabBar/Solar");
		HydroPlant = GetNode<PowerPlant>("TabContainer/TabBar/Hydro");
		TreePlant = GetNode<PowerPlant>("TabContainer/TabBar/Tree");
		WindPlant = GetNode<PowerPlant>("TabContainer/TabBar/Wind");

		// Fetch associated buttons
		GasButton = GetNode<Button>("TabContainer/TabBar2/Gas/GasButton");
		SolarButton = GetNode<Button>("TabContainer/TabBar/Solar/SolarButton");
		HydroButton = GetNode<Button>("TabContainer/TabBar/Hydro/HydroButton");
		TreeButton = GetNode<Button>("TabContainer/TabBar/Tree/TreeButton");
		WindButton = GetNode<Button>("TabContainer/TabBar/Wind/WindButton");

		// Fetch Close button
		CloseButton = GetNode<Button>("CloseButton");
		
		// Fetch Animation Player
		BuildMenuAP = GetNode<AnimationPlayer>("AnimationPlayer");
		
		// Fetch TabContainer and sets tab titles 
		//TODO for all tabs in all lang
		TabC = GetNode<TabContainer>("TabContainer");
		TabC.SetTabTitle(0,"Green");
		TabC.SetTabTitle(1,"Fossil");

		// Connect the associated button callbacks
		GasButton.Pressed += _OnGasButtonPressed;
		SolarButton.Pressed += _OnSolarButtonPressed;
		HydroButton.Pressed += _OnHydroButtonPressed;
		TreeButton.Pressed += _OnTreeButtonPressed;
		WindButton.Pressed += _OnWindButtonPressed;
		CloseButton.Pressed += _OnCloseButtonPressed;

		HideAllPlants();
	}

	// ==================== Public API ====================

	// Updates the name of the associate building type (for localization)
	public void _UpdatePlantName(Building Bt, string newName) {
		// Find which building to update
		switch (Bt.type) {
			case Building.Type.GAS:
				// Position the plant correctly
				SetPlantName(ref GasPlant, newName);
				break;

			case Building.Type.HYDRO:
				SetPlantName(ref HydroPlant, newName);
				break;

			case Building.Type.SOLAR:
				SetPlantName(ref SolarPlant, newName);
				break;

			case Building.Type.TREE:
				SetPlantName(ref TreePlant, newName);
				break;
				
			case Building.Type.WIND:
				SetPlantName(ref WindPlant, newName);
				break;
			
			default:
				break;
		}
	}

	// ==================== Internal Helpers ====================

	// Hides all of the plants related to this button
	private void HideAllPlants() {
		// Hide all plants
		GasPlant.Hide();
		HydroPlant.Hide();
		SolarPlant.Hide();
		TreePlant.Hide();
		WindPlant.Hide();
	}

	// Sets the plants name and propagates info to ui
	private void SetPlantName(ref PowerPlant PP, string name) {
		PP.PlantName = name;
		PP._UpdatePlantData();
	}
	
	// Sets the position of the given plant according to its position in the list
	private void SetPlantPosition(ref PowerPlant pp, int idx) {
		//pp.Position = new Vector2(
//			BuildingSpriteBase.X + (idx * BuildingSpriteOffset), 
//			BuildingSpriteBase.Y
//		);

		// Make sure that these plants are in preview mode
		pp._UpdateIsPreview(true);

		// Show the plant once the position is set
		pp.Show();
	}

	// ==================== Interaction Callbacks ====================

	// When the "ShowBuildMenu" signal is triggered, show the given powerplants 
	// in the given order using the predefined base and offset.
	public void _OnShowBuildMenu(BuildButton bb) {
		BuildMenuAP.Play("SlideUp");
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

		// Connect the build button to our return signal and register it
		RegisteredBuildButton = bb;
		SelectBuilding += bb._OnSelectBuilding;

		// Sanity check: List must not contain duplicates
		List<Building> BTs = bb.BL.AvailableTypes.Distinct().ToList();

		// Clear the Menu
		HideAllPlants();

		// Display the buildings sent with the signal  
		int idx = 0;
		foreach(var _bt in BTs) {
			switch (_bt.type) {
				case Building.Type.GAS:
					// Position the plant correctly
					SetPlantPosition(ref GasPlant, idx++);

					// Make sure that its fields are set correctly before displaying anything
					GasPlant._SetPlantFromConfig(Building.Type.GAS);
					GasPlant.PlantType = Building.Type.GAS;
					break;

				case Building.Type.HYDRO:
					SetPlantPosition(ref HydroPlant, idx++);
					HydroPlant._SetPlantFromConfig(Building.Type.HYDRO);
					HydroPlant.PlantType = Building.Type.HYDRO;
					break;

				case Building.Type.SOLAR:
					SetPlantPosition(ref SolarPlant, idx++);
					SolarPlant._SetPlantFromConfig(Building.Type.SOLAR);
					SolarPlant.PlantType = Building.Type.SOLAR;
					break;

				case Building.Type.TREE:
					SetPlantPosition(ref TreePlant, idx++);
					TreePlant._SetPlantFromConfig(Building.Type.TREE);
					TreePlant.PlantType = Building.Type.TREE;
					break;
					
				case Building.Type.WIND:
					SetPlantPosition(ref WindPlant, idx++);
					WindPlant._SetPlantFromConfig(Building.Type.WIND);
					WindPlant.PlantType = Building.Type.WIND;
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
	public void _OnWindButtonPressed() {
		// Send out the selection
		EmitSignal(SignalName.SelectBuilding, WindPlant);

		// Close the menu
		IsOpen = false;
	}
	// Closes the menu
	public void _OnCloseButtonPressed() {
		// Hide and close the menu
		IsOpen = false;
		Hide();
	}
}

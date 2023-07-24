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
using System.Diagnostics;
using System.Collections.Generic;

// Represents the different types of power plants
public enum BuildingType { HYDRO, GAS, SOLAR, TREE, NUCLEAR };

// Models a Build Slot
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

// Models a button that can be used to create power plants
public partial class BuildButton : Button {

	// The possible states the button can be in
	public enum BuildState { IDLE, BUILDING, DONE };

	// Names for the various power plants
	public const string GAS_NAME = "Gas";
	public const string SOLAR_NAME = "Solar";
	public const string HYDRO_NAME = "Hydro";
	public const string TREE_NAME = "Tree";

	// Signal used to trigger the showing of the build menu
	[Signal]
	public delegate void ShowBuildMenuEventHandler(BuildButton bb);

	[Signal]
	public delegate void UpdateBuildSlotEventHandler(BuildButton bb, PowerPlant pp, bool remove);

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

	// Reference to the game loop
	private GameLoop GL;

	// The internal state of the button (used for multy turn builds)
	private BuildState BS;

	// Plant we are currently building (only valid is state is BUILDING)
	private PowerPlant PPInProgress;

	// Number of Turns remaining in current build
	private int TurnsToBuild;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Initialize state
		BS = BuildState.IDLE;

		// Fetch Node  
		BM = GetNode<BuildMenu>("../../BuildMenu");

		// Fetch Power plants
		GasPlant = GetNode<PowerPlant>(GAS_NAME);
		SolarPlant = GetNode<PowerPlant>(SOLAR_NAME);
		HydroPlant = GetNode<PowerPlant>(HYDRO_NAME);
		TreePlant = GetNode<PowerPlant>(TREE_NAME);

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

	// ==================== Public API ====================
	// Records a reference to the game loop (triggered by the game loop itself)
	public void _RecordGameLoopRef(GameLoop _GL) {
		GL = _GL;
	}

	// Signals that a turn has passed
	public void _NextTurn() {
		// Check if we are currently building something
		if(BS == BuildState.BUILDING) {
			// Decrement the number of remaining turns and check for build completion
			if(--TurnsToBuild <= 0) {
				// Finish the current build
				FinishBuild();
			} else {
				// Update the button to show the number of remaining turns
				SetToBuild();
			}
		}
	}	

	// Public accessor which disables the current build button
	public void _Disable() {
		Disabled = true;
	}

	// ==================== Internal Helpers ====================

	// Begins the mutli-turn build of the given power plant
	private void BeginBuild(PowerPlant PP) {
		// Update build state
		BS = BuildState.BUILDING;

		// Update requested build fields
		PPInProgress = PP;
		TurnsToBuild = PP.BuildTime;

		// Update the button
		SetToBuild();
	}

	// Finalizes the current Build
	private void FinishBuild() {
		// Sanity Check
		Debug.Assert(PPInProgress != null);

		// Reset number of turns
		TurnsToBuild = 0;
		
		// Update the current power plant placed at this slot
		UpdateGenericPlant(PPInProgress);

		// Hide the button
		HideOnlyButton();

		// Update the requested build fields
		PPInProgress = null;
		BS = BuildState.DONE;
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

	// Sets the button to the build state
	private void SetToBuild() {
		Text = "🕐 : " + TurnsToBuild.ToString();
		Disabled = true;
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

	// Wrapper for a more specific call depending on the selected plant type
	private void UpdateGenericPlant(PowerPlant PP) {
		// Make sure that the selected plant is setup correctly
		PP._SetPlantFromConfig(PP.PlantType);

		// Pick which plant to show and update
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
			default:
				break;
		}
	}

	// Updates a given power plant to match the received power plant
	private void UpdatePowerPlant(ref PowerPlant PP, PowerPlant PPRec) {
		// Set it up to display at our button's location
		PP.Position = Vector2.Zero;
		PP.Scale = new Vector2(1, 1);
		PP.IsPreview = false;
		PP.BuildCost = PPRec.BuildCost;
		PP.PlantType = PPRec.PlantType;
		PP.BiodiversityImpact = PPRec.BiodiversityImpact;
		PP.LandUse = PPRec.LandUse;

		// Update the plant's internal private fields
		PP._UdpatePowerPlantFields(
			true, // Update the initial values as well
			PPRec._GetPollution(),
			PPRec._GetProductionCost(), 
			PPRec._GetCapacity(),
			PPRec._GetAvailability()
		);

		// Force name to be consistent with type
		PP.PlantName = PPRec.PlantName;

		// Make sure that the data is propagated to the UI
		PP._UpdatePlantData();
		PP.Show();

		// Add the building to the power plant list
		EmitSignal(SignalName.UpdateBuildSlot, this, PP, false);
	}

	// ==================== Interaction Callbacks ====================

	// Show the build menu containing the correct buildings to build
	public void _OnPressed() {
		// Only do stuff if the button is idle
		if(BS == BuildState.IDLE) {
			BM.Show();
			EmitSignal(SignalName.ShowBuildMenu, this);
		}
	}

	// Receives the power plant selected by the user and now we need to place it
	public void _OnSelectBuilding(PowerPlant PP) {
		// Sanity check: Check explicitly for hydro builds and dissallow illegal ones
		if(PP.PlantType == BuildingType.HYDRO && !AllowHydro) {
			return;
		}

		// Hide the build menu UI
		BM.Hide();

		// Check if the requested build was legal
		if(GL._RequestBuild(PP.BuildCost)) {
			// Hide all plants
			HideAllPlants();

			// Check for the requested plant's build time
			if(PP.BuildTime >= 1) {
				BeginBuild(PP);
			} else {
				// Select which plant to show and update it's fields to match those that were given
				UpdateGenericPlant(PP);

				// Hide our button
				HideOnlyButton();

				// Update Build State
				BS = BuildState.DONE;
			}
		}		
	}
}

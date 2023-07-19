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
using System.Collections.Generic;

// Encapsulates all of the resource management used throughout the game
// A resource is any mesurable field that is actively used by the player to make decisions
// There are 3 groups of resources, which represent the 3 pillars of our simulation
// - Support: How aligned the population with the decisions the player is making
// - Energy: The core resource of the game, are the people getting enough energy to meet their demand
// - Environment: How are the energy management decisions impacting the environment
public partial class ResourceManager : Node {

	// Children resource managers
	private SupportManager SM;
	private EnergyManager EngM;
	private EnvironmentManager EnvM;

	// Reference to the UI object
	private UI _UI;

	// Contains all of the PowerPlants and BuildButtons in the scene
	private List<PowerPlant> PowerPlants;
	private List<BuildButton> BBs;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch nodes
		SM = GetNode<SupportManager>("SupportManager");
		EngM = GetNode<EnergyManager>("EnergyManager");
		EnvM = GetNode<EnvironmentManager>("EnvironmentManager");
		_UI = GetNode<UI>("../UI");

		// Initialize the powerplant and Buildbutton lists
		PowerPlants = new List<PowerPlant>();
		BBs = new List<BuildButton>();
	}

	// ==================== Public API ====================

	// Progresses to the next turn
	public void _NextTurn() {

		// Update all build buttons
		foreach(BuildButton bb in BBs) {
			bb._NextTurn();
		}

		// Update all plants
		foreach(PowerPlant pp in PowerPlants) {
			pp._NextTurn();
		}

		// Update the internal managers
		Energy E = EngM._NextTurn();
		Environment Env = EnvM._NextTurn();

		// Update the energy UI
		UpdateEnergyUI(E);
		UpdateEnvironmentUI(Env);
	}

	// Initializes all of the resource managers
	public void _UpdateResourcesUI() {
		// Initialize the internal managers
		Energy E = EngM._GetEnergyValues();
		Environment Env = EnvM._NextTurn();

		// Update the UI
		UpdateEnergyUI(E);
		UpdateEnvironmentUI(Env);
	}

	// Updates the current list of power plants via a deep copy
	public void _UpdatePowerPlants(List<PowerPlant> lPP) {
		// Clear the current list to be safe
		PowerPlants.Clear();

		// Fill in the contents of the list with those of the given one
		foreach(PowerPlant pp in lPP) {
			PowerPlants.Add(pp);
		}

		// Propagate the update to the energy manager
		EngM._UpdatePowerPlants(PowerPlants);
		EnvM._UpdatePowerPlants(PowerPlants);

		// Connect the powerplants signals to propagate changes to the UI
		foreach(PowerPlant pp in PowerPlants) {
			pp.Switch.Toggled += _OnPowerPlantSwitchToggle;
		}
	}

	// Updates the current list of build buttons
	public void _UpdateBuildButtons(List<BuildButton> lBB) {
		// Clear the current list to be safe
		BBs.Clear();

		// Fill in the contents of the list with those of the given one
		foreach(BuildButton bb in lBB) {
			BBs.Add(bb);
			bb.Pressed += _UpdateResourcesUI;
		}
	} 

	// ==================== Helper Methods ====================  
	
	// Updates the UI fields related to the energy resource
	private void UpdateEnergyUI(Energy E) {
		// Update the UI
		_UI._UpdateData(
			UI.InfoType.W_ENGERGY,
			(int)Math.Floor(E.DemandWinter), 
			(int)Math.Floor(E.SupplyWinter)
		);
		_UI._UpdateData(
			UI.InfoType.S_ENGERGY, 
			(int)Math.Floor(E.DemandSummer), 
			(int)Math.Floor(E.SupplySummer)
		);
	}

	// Updates the UI fields related to the environment resource
	private void UpdateEnvironmentUI(Environment Env) {
		// Update the UI
		_UI._UpdateData(
			UI.InfoType.ENVIRONMENT,
			(int)(Env.LandUse * 100), // Convert floating point to integer percentage
			Env.Pollution,
			(int)(Env.Biodiversity * 100),
			(int)(Env.EnvBarValue() * 100)
		);
	}

	// Wrapper for interface compatibility reasons
	private void _UpdateResourcesUIWrapper(bool b) {
		_UpdateResourcesUI();
	}

	// ==================== Callbacks ====================  

	// Simply reacts to a power plant toggle by updating the UI
	// The parameter is only used for signal interface compatibility
	private void _OnPowerPlantSwitchToggle(bool b=false) { 
		_UpdateResourcesUI();
	}
	
}

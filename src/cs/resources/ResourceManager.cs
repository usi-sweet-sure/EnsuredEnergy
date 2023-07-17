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

	// Contains all of the PowerPlants in the scene
	private List<PowerPlant> PowerPlants;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch nodes
		SM = GetNode<SupportManager>("SupportManager");
		EngM = GetNode<EnergyManager>("EnergyManager");
		EnvM = GetNode<EnvironmentManager>("EnvironmentManager");

		// Initialize the powerplant list
		PowerPlants = new List<PowerPlant>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// ==================== Public API ====================

	// Progresses to the next turn
	public void NextTurn() {
		// Update the internal managers
		Energy E = EngM._NextTurn();
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
	}
}

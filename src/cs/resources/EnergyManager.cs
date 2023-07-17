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
using System.Linq;

// Models the resource managed by the EnergyManager
public struct Energy {
	int SupplySummer; // Total Supply for the next turn for the summer months
	int SupplyWinter; // Total Supply for the next turn for the winter months
	int DemandSummer; // Total Demand for the next turn for the summer months
	int DemandWinter; // Total Demand for the next turn for the winter months

	// Basic constructor for the Energy Ressource
	public Energy(int SS=0, int SW=0, int DS=0, int DW=0) {
		SupplySummer = SS;
		SupplyWinter = SW;
		DemandSummer = DS;
		DemandWinter = DW;
	}
}

public partial class EnergyManager : Node {

	// Keep track of all of the placed power plants
	private List<PowerPlant> PowerPlants;

	// Keeps track of the current energy values
	private Energy E;


	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Initialize the fields
		PowerPlants = new List<PowerPlant>();
		E = new Energy();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// ==================== Public API ====================

	// Updates the current internal power plant list
	public void _UpdatePowerPlants(List<PowerPlant> lP) {
		// Clear the current list to be safe
		PowerPlants.Clear();

		// Fill in the contents of the list with those of the given one
		foreach(PowerPlant pp in lP) {
			PowerPlants.Add(pp);
		}
	}

	// Computes the energy 
	public Energy NextTurn() {
		// TODO: Update the Energy by aggregating the capacity from the new power plants
		// and updating the model
		return new Energy();
	}

	// ==================== Helper Methods ====================  

	// Aggregate the current capacities into a single value
	private int AggregateCapacity() =>
		// Sum all capacities for each active power plant
		PowerPlants.Where(pp => pp._GetLiveness()).Select(pp => pp._GetCapacity()).Sum();

}

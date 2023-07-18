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

// Represents a Power Plant object in the game
public partial class PowerPlant : Node2D {

	// Life cycle of a nuclear power plant
	[Export]
	public int NUCLEAR_LIFE_SPAN = 5; 
	public int DEFAULT_LIFE_SPAN = 10;

	// Defines whether or not the building is a preview
	// This is true when the building is being shown in the build menu
	// and is used to know when to hide certain fields
	[Export]
	public bool IsPreview = false; 

	// The number of turns it takes to build this plant
	[Export]
	public int BuildTime = 0;

	// The initial cost of creating the power plant
	// This is what will be displayed in the build menu
	[Export]
	public int BuildCost = 0;

	// The cost that the power plant will require each turn to function
	[Export] 
	public int InitialProductionCost = 0;

	// This is the amount of energy that the plant can produce per turn
	[Export] 
	public int InitialEnergyCapacity = 100;

	// This is the amount of energy that the plant is able to produce given environmental factors
	[Export]
	public float InitialEnergyAvailability = 1.0f; // This is a percentage

	// The name of the power plant that will be displayed in the game
	// This should align with the plant's type
	[Export] 
	public string PlantName = "Power Plant";

	[Export] 
	public BuildingType PlantType = BuildingType.GAS;

	// The number of turns the plant stays usable for
	[Export]
	public int LifeCycle = 10;

	// Internal metrics
	private int ProductionCost = 0;
	private int EnergyCapacity = 100;
	private float EnergyAvailability = 1.0f;

	// Life flag: Whether or not the plant is on
	private bool IsAlive = true;

	// Children Nodes
	private Sprite2D Sprite;
	private Label NameL;
	private Label PollL;
	private Label EnergyL;
	private Label MoneyL;
	public CheckButton Switch;

	// ==================== GODOT Method Overrides ====================
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch all children nodes
		Sprite = GetNode<Sprite2D>("Sprite");
		NameL = GetNode<Label>("NameRect/Name");
		PollL = GetNode<Label>("ResRect/Poll");
		EnergyL = GetNode<Label>("ResRect/Energy");
		MoneyL = GetNode<Label>("ResRect/Money");
		Switch = GetNode<CheckButton>("Switch");

		// Hide unnecessary fields if we are in preview mode
		if(IsPreview) {
			PollL.Hide();
			Switch.Hide();
		} else {
			PollL.Show();
			Switch.Show();
		}

		// Set the labels correctly
		NameL.Text = PlantName;
		EnergyL.Text = EnergyCapacity.ToString();
		MoneyL.Text = BuildCost.ToString();

		// Set plant life cycle
		LifeCycle = (PlantType == BuildingType.NUCLEAR) ? NUCLEAR_LIFE_SPAN : DEFAULT_LIFE_SPAN;

		// Activate the plant
		ActivatePowerPlant();

		// Propagate to UI
		_UpdatePlantData();

		// Connect the switch signal
		Switch.Toggled += _OnSwitchToggled;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// ==================== Power Plant Update API ====================

	// Reacts to a new turn taking place
	public void _NextTurn() {
		if(LifeCycle-- <= 0) {
			// Deactivate the plant
			KillPowerPlant();

			// Disable the switch
			Switch.ButtonPressed = false;
			Switch.Disabled = true;
			
			// Workaround to allow for an immediate update
			IsAlive = true;
		} 
		if(LifeCycle < 0) {
			IsAlive = false;
		}
	}

	// Update API for the private fields of the plant
	public void _UdpatePowerPlantFields(int PC=-1, int EC=-1, float EA=-1.0f) {
		// Only update internal fields that where given a proper value
		ProductionCost = PC == -1 ? ProductionCost : PC;
		EnergyCapacity = EC == -1 ? EnergyCapacity : EC;
		EnergyAvailability = EA <= -1.0f ? EnergyAvailability : Math.Max(Math.Min(EA, 1.0f), 0.0f);
	}

	// Getter for the powerplant's current capacity
	public int _GetCapacity() {
		return EnergyCapacity;
	}

	// Getter for the plant's production cost
	public int _GetProductionCost() {
		return ProductionCost;
	}

	// Getter for the current availability EA in [0.0, 1.0]
	public float _GetAvailability() {
		return Math.Max(Math.Min(EnergyAvailability, 1.0f), 0.0f);
	}

	// Getter for the powerplant's liveness status
	public bool _GetLiveness() {
		return IsAlive;
	}

	// Forces the update of the isPreview state of the plant
	public void _UpdateIsPreview(bool n) {
		IsPreview = n;
		if(IsPreview) {
			PollL.Hide();
			Switch.Hide();
		} else {
			PollL.Show();
			Switch.Show();
		}
	}

	public void _UpdatePlantName(string name) {
		NameL.Text = name;
	}

	// Updates the UI to match the internal state of the plant
	public void _UpdatePlantData() {
		// Update the preview state of the plant
		if(IsPreview) {
			PollL.Hide();
			Switch.Hide();
		} else {
			PollL.Show();
			Switch.Show();
		}

		// Set the labels correctly
		NameL.Text = PlantName;
		EnergyL.Text = EnergyCapacity.ToString();
		MoneyL.Text = BuildCost.ToString();
	}

	// ==================== Helper Methods ====================    

	// Deactivates the current power plant
	private void KillPowerPlant() {
		IsAlive = false;
		EnergyCapacity = 0;
		EnergyAvailability = 0;
		ProductionCost = 0;
		_UpdatePlantData();
	}

	// Activates the power plant
	private void ActivatePowerPlant() {
		IsAlive = true;

		// Reset the internal metrics to their initial values
		EnergyCapacity = InitialEnergyCapacity;
		EnergyAvailability = InitialEnergyAvailability;
		ProductionCost = InitialProductionCost;
		_UpdatePlantData();
	}

	// ==================== Button Callbacks ====================  
	
	// Reacts to the power switch being toggled
	// We chose to ignore the state of the toggle as it should be identical to the IsAlive field
	public void _OnSwitchToggled(bool pressed) {
		// Check the liveness of the current plant
		if(IsAlive) {
			// If the plant is currently alive, then kill it
			KillPowerPlant();
		} else {
			// If the plant is currently dead, then activate it
			ActivatePowerPlant();
		}

		// Update the UI
		_UpdatePlantData();
	}
}

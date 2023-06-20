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
	public int ProductionCost = 0;

	// This is the amount of energy that the plant will produce per turn
	[Export] 
	public int EnergyProduction = 100;

	// The name of the power plant that will be displayed in the game
	// This should align with the plant's type
	[Export] 
	public string PlantName = "Power Plant";

	[Export] 
	public BuildingType PlantType = BuildingType.GAS;

	// The number of turns the plant stays usable for
	[Export]
	public int LifeCycle;

	// Life flag: Whether or not the plant is on
	private bool IsAlive = true;

	// Children Nodes
	private Sprite2D Sprite;
	private Label NameL;
	private Label PollL;
	private Label EnergyL;
	private Label MoneyL;
	private CheckButton Switch;

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
		EnergyL.Text = EnergyProduction.ToString();
		MoneyL.Text = BuildCost.ToString();

		// Set plant life cycle
		LifeCycle = (PlantType == BuildingType.NUCLEAR) ? NUCLEAR_LIFE_SPAN : DEFAULT_LIFE_SPAN;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// ==================== Power Plant Update API ====================

	// Reacts to a new turn taking place
	public void _NewTurn() {
		if(LifeCycle-- <= 0) {
			// Deactivate the plant
			IsAlive = false;
			EnergyProduction = 0;
			ProductionCost = 0;
		}
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
		EnergyL.Text = EnergyProduction.ToString();
		MoneyL.Text = BuildCost.ToString();
	}
}

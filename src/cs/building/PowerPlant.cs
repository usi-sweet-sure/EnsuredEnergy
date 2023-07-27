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


	[ExportGroup("Meta Parameters")]
	[Export] 
	// The name of the power plant that will be displayed in the game
	// This should align with the plant's type
	public string PlantName = "Power Plant";

	[Export] 
	// The type of the power plant, this is for internal use, other fields have to be 
	// updated to match the type of the building
	private Building.Type _PlantType = Building.Type.GAS;
	public Building PlantType;

	[Export]
	// Life cycle of a nuclear power plant
	public int NUCLEAR_LIFE_SPAN = 5; 
	public int DEFAULT_LIFE_SPAN = 10;

	[Export]
	// Defines whether or not the building is a preview
	// This is true when the building is being shown in the build menu
	// and is used to know when to hide certain fields
	public bool IsPreview = false; 

	// The number of turns it takes to build this plant
	public int BuildTime = 0;

	// The initial cost of creating the power plant
	// This is what will be displayed in the build menu
	public int BuildCost = 0;

	// The number of turns the plant stays usable for
	public int LifeCycle = 10;

	// The cost that the power plant will require each turn to function
	public int InitialProductionCost = 0;

	// This is the amount of energy that the plant can produce per turn
	public int InitialEnergyCapacity = 100;

	// This is the amount of energy that the plant is able to produce given environmental factors
	public float InitialEnergyAvailability = 1.0f; // This is a percentage

	// Amount of pollution caused by the power plant (can be negative in the tree case)
	public int InitialPollution = 10;

	// Percentage of the total land used up by this power plant
	public float LandUse = 0.1f;

	// Percentage by which this plant reduces the biodiversity in the country
	// If negative, this will increase the total biodiversity
	public float BiodiversityImpact = 0.1f;

	// Internal metrics
	private int ProductionCost = 0;
	private int EnergyCapacity = 100;
	private float EnergyAvailability = 1.0f;
	private int Pollution = 10;

	// Life flag: Whether or not the plant is on
	private bool IsAlive = true;

	// Children Nodes
	private Sprite2D Sprite;
	private ColorRect NameR;
	private Label NameL;
	private Label PollL;
	private Label EnergyS;
	private Label EnergyW;
	private Label MoneyL;
	public CheckButton Switch;
	private Label Price;

	// Configuration controller
	private ConfigController CC;

	// ==================== GODOT Method Overrides ====================
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch all children nodes
		Sprite = GetNode<Sprite2D>("Sprite");
		NameL = GetNode<Label>("NameRect/Name");
		NameR = GetNode<ColorRect>("NameRect");
		PollL = GetNode<Label>("ResRect/Poll");
		EnergyS = GetNode<Label>("ResRect/EnergyS");
		EnergyW = GetNode<Label>("ResRect/EnergyW");
		MoneyL = GetNode<Label>("ResRect/Money");
		Switch = GetNode<CheckButton>("Switch");
		CC = GetNode<ConfigController>("ConfigController");
		Price = GetNode<Label>("Price");
		
		// Initialize plant type
		PlantType = _PlantType;

		// Hide unnecessary fields if we are in preview mode
		if(IsPreview) {
			PollL.Hide();
			Switch.Hide();
			Price.Show();
		} else {
			//PollL.Show();
			Switch.Show();
			Price.Hide();
		}

		// Set the labels correctly
		NameL.Text = PlantName;
		EnergyS.Text = EnergyCapacity.ToString();
		EnergyW.Text = EnergyCapacity.ToString();
		MoneyL.Text = ProductionCost.ToString();
		Price.Text = BuildCost.ToString();

		// Set plant life cycle
		LifeCycle = (PlantType == Building.Type.NUCLEAR) ? NUCLEAR_LIFE_SPAN : DEFAULT_LIFE_SPAN;

		// Activate the plant
		ActivatePowerPlant();

		// Propagate to UI
		_UpdatePlantData();

		// Connect the switch signal
		Switch.Toggled += _OnSwitchToggled;
	}

	// ==================== Power Plant Update API ====================

	// Getter for the powerplant's current capacity
	public int _GetCapacity() => EnergyCapacity;

	// Getter for the Pollution amount
	public int _GetPollution() => Pollution;

	// Getter for the plant's production cost
	public int _GetProductionCost() => ProductionCost;

	// Getter for the current availability EA in [0.0, 1.0]
	public float _GetAvailability() => 
		Math.Max(Math.Min(EnergyAvailability, 1.0f), 0.0f);

	// Getter for the powerplant's liveness status
	public bool _GetLiveness() => IsAlive;

	// Sets the values of the plant from a given config
	public void _SetPlantFromConfig(Building bt) {
		// Sanity check: only reset the plant if it's alive
		if(IsAlive) {
			PowerPlantConfigData PPCD = new PowerPlantConfigData();

			// Read out the given plant's config
			PPCD = (PowerPlantConfigData)CC._ReadConfig(Config.Type.POWER_PLANT, bt.ToString());

			// Copy over the data to our plant
			CopyFrom(PPCD);

			// Propagate change to the UI
			_UpdatePlantData();
		}
	}

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
			_OnSwitchToggled(false);
		} 
		if(LifeCycle < 0) {
			IsAlive = false;
		}
	}

	// Update API for the private fields of the plant
	public void _UdpatePowerPlantFields(
		bool updateInit=false, // Whether or not to update the initial values as well
		int pol=-1, // pollution amount
		int PC=-1, // Production cost
		int EC=-1, // Energy capacity
		float EA=-1.0f // Energy availability
	) {
		// Only update internal fields that where given a proper value
		Pollution = pol == -1 ? Pollution : pol;
		ProductionCost = PC == -1 ? ProductionCost : PC;
		EnergyCapacity = EC == -1 ? EnergyCapacity : EC;
		// Percentages are clamped to [0, 1]
		EnergyAvailability = EA <= -0.1f ? EnergyAvailability : Math.Max(Math.Min(EA, 1.0f), 0.0f);

		// Check for initial value updates
		if(updateInit) {
			InitialPollution = pol == -1 ? InitialPollution : pol;
			InitialProductionCost = PC == -1 ? InitialProductionCost : PC;
			InitialEnergyCapacity = EC == -1 ? InitialEnergyCapacity : EC;
			InitialEnergyAvailability = EA <= -0.1f ? InitialEnergyAvailability : Math.Max(Math.Min(EA, 1.0f), 0.0f);
		}
	}

	// Forces the update of the isPreview state of the plant
	public void _UpdateIsPreview(bool n) {
		IsPreview = n;
		// If the plant is in preview mode, then it's being shown in the build menu
		// and thus should not have any visible interactive elements.
		if(IsPreview) {
			PollL.Hide();
			Switch.Hide();
			NameR.Show();
			//Price.AddThemeColorOverride("font_color", new Color(1,0,0,1)); red
			//Price.AddThemeColorOverride("font_color", new Color(1,1,1,1)); white
			Price.Show();
		} 
		// When not in preview mode, the interactive elements should be visible
		else {
			//PollL.Show();
			Switch.Show();
			Price.Hide();
			NameR.Hide();
		}
	}

	// Updates the UI label for the plant to the given name
	public void _UpdatePlantName(string name) {
		NameL.Text = name;
	}

	// Updates the UI to match the internal state of the plant
	public void _UpdatePlantData() {
		// Update the preview state of the plant (in case this happens during a build menu selection)
		if(IsPreview) {
			PollL.Hide();
			Switch.Hide();
			NameR.Show();
		} else {
			//PollL.Show();
			Switch.Show();
			NameR.Hide();
		}

		// Set the labels correctly
		NameL.Text = PlantName;
		EnergyS.Text = EnergyCapacity.ToString();
		EnergyW.Text = EnergyCapacity.ToString();
		MoneyL.Text = ProductionCost.ToString();
		Price.Text = BuildCost.ToString();
	}

	// ==================== Helper Methods ====================    

	// Sets the internal fields of a powerplant from a given config data
	private void CopyFrom(PowerPlantConfigData PPCD) {
		// Copy in the public fields
		BuildCost = PPCD.BuildCost;
		BuildTime = PPCD.BuildTime;
		LifeCycle = PPCD.LifeCycle;
		LandUse = PPCD.LandUse;
		BiodiversityImpact = PPCD.Biodiversity;

		// Copy in the private fields
		_UdpatePowerPlantFields(
			true, 
			PPCD.Pollution,
			PPCD.ProductionCost,
			PPCD.Capacity,
			PPCD.Availability
		);
	}

	// Deactivates the current power plant
	private void KillPowerPlant() {
		IsAlive = false;
		EnergyCapacity = 0;
		EnergyAvailability = 0;
		ProductionCost = 0;

		// Plant no longer pollutes when it's powered off
		Pollution = 0;

		// Propagate the new values to the UI
		_UpdatePlantData();
	}

	// Activates the power plant
	private void ActivatePowerPlant() {
		IsAlive = true;

		// Reset the internal metrics to their initial values
		EnergyCapacity = InitialEnergyCapacity;
		EnergyAvailability = InitialEnergyAvailability;
		ProductionCost = InitialProductionCost;
		Pollution = InitialPollution;

		_SetPlantFromConfig(PlantType);

		// Propagate the new values to the UI
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
	
	private void _on_area_2d_mouse_entered() {
		if(!IsPreview) {
			NameR.Show();
		}
		
	}
	private void _on_area_2d_mouse_exited() {
		if(!IsPreview) {
			NameR.Hide();
		}
	}
}

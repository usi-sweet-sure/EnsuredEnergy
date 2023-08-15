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

// Models the overarching game loop, which controls every aspect of the game
// and makes sure that things are synchronized across game objects
public partial class GameLoop : Node2D {

	// Represents the various states that the game can be in
	public enum GameState { NOT_STARTED, PLAYING, ENDED };

	// Context of the game
	private Context C;

	// The total number of turns in the game
	[Export]
	public int N_TURNS = 10;

	// The amount of money the player starts with (in millions of CHF)
	[Export]
	public int START_MONEY = 1000;

	[Export]
	public static int BUDGET_PER_TURN = 1000;

	// Internal game state
	private GameState GS;

	// Contains all of the PowerPlants in the scene
	private List<PowerPlant> PowerPlants;
	
	// Contains all of the BuildButtons in the scene
	private List<BuildButton> BBs;

	// Reference to the UI
	private UI _UI;

	// Reference to the buildmenu
	private BuildMenu BM;

	//TODO: Add resource managers once they are implemented
	private MoneyData Money; // The current money the player has

	private int RemainingTurns; // The number of turns remaining until the end of the game

	private ResourceManager RM;

	// Model controller
	private ModelController MC;

	//TODO: Add Shocks once they are implemented

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch context and model controller
		C = GetNode<Context>("/root/Context");
		MC = GetNode<ModelController>("ModelController");

		// Init Data
		GS = GameState.NOT_STARTED;
		RemainingTurns = N_TURNS;
		Money = new MoneyData(START_MONEY);
		PowerPlants = new List<PowerPlant>();
		BBs = new List<BuildButton>();

		// Fetch initial nodes
		// Start with PowerPlants, in the begining there are only 2 PowerPlants Nuclear and Coal
		PowerPlants.Add(GetNode<PowerPlant>("World/Nuclear"));
		PowerPlants.Add(GetNode<PowerPlant>("World/Coal"));

		// Fill in build buttons
		BBs.Add(GetNode<BuildButton>("World/BuildButton"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton2"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton3"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton4"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton5"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton6"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton7"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton8"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton9"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton10"));

		// Fetch UI and BuildMenu
		_UI = GetNode<UI>("UI");
		BM = GetNode<BuildMenu>("BuildMenu");
		
		// Fetch resource manager
		RM = GetNode<ResourceManager>("ResourceManager");

		// Initially set all plants form their configs
		foreach(PowerPlant pp in PowerPlants) {
			pp._SetPlantFromConfig(pp.PlantType);

			// Add the power plants to the stats
			C._UpdatePPStats(pp.PlantType);
		}

		// Connect Callback to each build button and give them a reference to the loop
		foreach(BuildButton bb in BBs) {
			bb.UpdateBuildSlot += _OnUpdateBuildSlot;

			// Record a reference to the game loop
			bb._RecordGameLoopRef(this);
		}

		// Connect to the UI's signals
		_UI.NextTurn += _OnNextTurn;
		C.UpdateContext += _OnContextUpdate;

		// Start the game
		StartGame();
	}

	// ==================== Resource access API ====================
	
	// Checks if a current build is legal and if so updates the amount of money
	public bool _RequestBuild(int cost) {
		// Check that we have enough money
		if(Money.Money >= cost) {
			// Spend some money
			Money.SpendMoney(cost);

			// Notify the UI of the resource update
			UpdateResources();
			return true;
		}
		// Return false if we couldn't afford the build
		return false;
	}

	// Getter for the internal list of built powerplants
	public List<PowerPlant> _GetPowerPlants() => PowerPlants;

	// Enable public access to a resource update request
	public void _UpdateResourcesUI() {
		// Request a non-new turn update of the UI
		UpdateResources();
	}

	// ==================== Internal Helpers ====================
	
	// Propagates resource updates to the UI
	private void UpdateResources(bool newturn=false) {
		// Update the ressource manager
		if(newturn) {
			RM._NextTurn(ref Money);
		} else {
			RM._UpdateResourcesUI(true);
		}

		// Update Money UI
		_UI._UpdateData(
			UI.InfoType.MONEY,
			Money.Budget, 
			Money.Production,
			Money.Build,
			Money.Money,
			Money.Imports
		);

		// Propagate the update to the UI
		_UI._UpdateUI();
	}

	// Computes which turn we are at
	private int GetTurn() => N_TURNS - RemainingTurns;

	// ==================== Main Game Loop Methods ====================  

	// Initializes all of the data that is propagated across the game
	// This only happens once at the begining of the playthrough
	private void StartGame() {
		// Update the game state
		GS = GameState.PLAYING;

		// Initialize the context stats
		C._InitializePPStats(PowerPlants);

		// Initialize the model
		MC._InitModel();

		// Perform initial Resouce update
		UpdateResources(true);

		// Update the model to include all of the initial plants
		foreach(PowerPlant pp in PowerPlants) {
			C._UpdateModelFromClient(pp);
		}

		// Update model with our current data
		foreach((ModelCol mc, Building b, float val) in C._GetModel(ModelSeason.WINTER).ModifiedCols) {
			// Create a new request for each modified filed in our model
			MC._UpsertModelColumnDataAsync(mc, b);
		}

		// Create a fetch request to get the summer data
		MC._FetchModelDataAsync();

		// Clear the model's modified columns
		C._ClearModified();

		// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		// All of the plants must be in the model for the availability to be set
		// This is why we require two separate loops
		// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		// Now that we have the initial model data, update the availability
		foreach(PowerPlant pp in PowerPlants) {
			pp._SetAvailabilityFromContext();			
		}

		// Set the initial power plants and build buttons
		RM._UpdatePowerPlants(PowerPlants);
		RM._UpdateBuildButtons(BBs);

		// Update the UI
		_UI._UpdateUI();

		// Initialize resources
		_UI._OnUpdatePrediction();
		RM._UpdateResourcesUI();
	}

	// Triggers all of the updates across the whole game at the beginnig of the turn
	// A new turn is triggered when the player presses the next turn button in the UI.
	private void NewTurn() {
		// Decerement the remaining turns and check for game end
		if((GS == GameState.PLAYING) && (RemainingTurns-- > 0)) {

			// Update the Context's turn count
			C._UpdateTurn(GetTurn());

			// Update model with our current data
			foreach((ModelCol mc, Building b, float val) in C._GetModel(ModelSeason.WINTER).ModifiedCols) {
				// Create a new request for each modified filed in our model
				MC._UpsertModelColumnDataAsync(mc, b);
			}

			// Create a fetch request to get the summer data
			MC._FetchModelDataAsync();

			// Clear the model's modified columns
			C._ClearModified();

			// Update Resources 
			UpdateResources(true);
			RM._UpdateResourcesUI();

		} else if(RemainingTurns <= 0) {
			// Update the Context's turn count
			C._UpdateTurn(GetTurn());

			// End the game if all turns have been spent
			EndGame();
		}
	}

	// Triggers all of the actions that happen at the end of the game
	// The end of the game is triggered when the number of remaining turns reaches 0
	private void EndGame() {
		// Update the game state 
		GS = GameState.ENDED;

		// Deactivate all buttons
		foreach(var bb in BBs) {
			bb._Disable();
		}
	}

	// ==================== Interaction Callbacks ====================
	
	// Updates the internal lists on every build slot update
	public void _OnUpdateBuildSlot(BuildButton bb, PowerPlant pp, bool remove) {
		// Check if the update was a power plant addition or removal
		if(remove) {
			//Sanity Check
			Debug.Assert(PowerPlants.Contains(pp));

			// Destroy the power plant
			PowerPlants.Remove(pp);

			// Update the context stats
			C._UpdatePPStats(pp.PlantType, false);

			// Connect the new build button to our signal
			bb.UpdateBuildSlot += _OnUpdateBuildSlot;

			// Make sure that it has access to the game loop
			bb._RecordGameLoopRef(this);

			// Replace it with the new build button
			BBs.Add(bb);
		} else {
			// Sanity Check
			Debug.Assert(BBs.Contains(bb));

			// Destroy the Build Button
			BBs.Remove(bb);

			// Replace it with the new power plant
			PowerPlants.Add(pp);

			// Update the context stats
			C._UpdatePPStats(pp.PlantType);
		}

		// Propagate the updates to the resource manager
		RM._UpdatePowerPlants(PowerPlants);
		RM._UpdateBuildButtons(BBs);
		RM._UpdateResourcesUI(true);
	}

	// Triggers a new turn if the game is currently acitve
	public void _OnNextTurn() {
		if(GS == GameState.PLAYING) {
			NewTurn();
		}
	}

	// Reacts to a context update
	public void _OnContextUpdate() {
		// Propate update to the UI
		UpdateResources();
		RM._UpdateResourcesUI();
	}
}

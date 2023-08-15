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

// Global state of the game
// This will record all persistent data such as:
// - Game instance id
// - Player actions
// - Aggregated model data
// - Player choice statistics
// ...
public partial class Context : Node {

    [Signal]
    // Signals that the context has been updated by an external actor
    public delegate void UpdateContextEventHandler();

    // Current internal storage of the game instance's id
    private int ResId = -1;

    // Current internal storage of the game instance's name
    private string ResName = "";

    // The Curent Turn
    private int Turn = 0;

    // Internal representation of the most recent data retrieved from the model
    private Model MSummer;
    private Model MWinter;

    // Dictionary to link power plant type to the number of plants
    private Dictionary<Building.Type, int> PPStats; 

    // ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
        // Initialize models
        MSummer = new Model(ModelSeason.SUMMER); 
        MWinter = new Model(ModelSeason.WINTER);

        // Initialize the internal stats
        ResetPPStats();
	}    

    // ==================== Public API ====================

    // Initialize the internal stats dictionary given a list of powerplants
    public void _InitializePPStats(List<PowerPlant> PPs) {
        // Reset the stats
        ResetPPStats();

        // Loop over all power plants and aggregate the data
        foreach(PowerPlant pp in PPs) {
            // Increment the number of plants of that type
            PPStats[pp.PlantType]++;
        }
    }

    // Updates the PPStats to modifiy the number of plants of a certain type
    public void _UpdatePPStats(Building b, bool inc=true) {
        // Sanity check
        if(!inc && PPStats[b] == 0) {
            // You can't decrement a type of building that wasn't build
            throw new Exception(b + " can't be decremented as it is at 0 in PPStats!");
        }
        // Increment or decrement the given statistic
        PPStats[b] = inc ? PPStats[b] + 1 : PPStats[b] - 1;
    }

    // Updates the internal model using data retrieved from the server
    // This method should only be called from the ModelController directly
    public void _UdpateModelFromServer(ModelSeason S, Availability A, Capacity C, Demand D) {
        // Check which model needs to be updated using the given season
        if(S == ModelSeason.WINTER) {
            MWinter._UpdateFields(A, C, D, ModelCoherencyState.SHARED);
        } else {
            MSummer._UpdateFields(A, C, D, ModelCoherencyState.SHARED);
        }

        // Signal that the context has been updated 
        EmitSignal(SignalName.UpdateContext);
    }

    // Wrapper for _UdpateModelFromServer that simply unfolds the model struct before calling the update method
    public void _UdpateModelFromServer(Model new_M) {
        _UdpateModelFromServer(new_M._Season, new_M._Availability, new_M._Capacity, new_M._Demand);
    }

    // Sets a new value in the internal model struct and sets a modification to be sent to server
    // This can only be done by adding or removing power plants
    public void _UpdateModelFromClient(PowerPlant pp, bool inc=true) {
        // Fetch the old value from the model
        float cur_cap = MWinter._Capacity._GetField(pp.PlantType);

        // Compute the new capacity by suming the current one with the new addition
        float new_cap = cur_cap + (inc ? 1.0f : -1.0f) * pp._GetCapacity();

        // Only the winter model is updated by the client  
        // Only the capacity can be updated by player action
        MWinter._ModifyField(ModelCol.Type.CAP, pp.PlantType, new_cap);
    }

    // Updates the current ID (should only be done once per game)
    // Returns the internal value of the game id:
    // - newId if the id wasn't set 
    // - ResID if it already had a value (in this case the given id is ignored)
    public int _UpdateGameID(int newId) {
        // Check if the ID has been set yet
        if(ResId == -1) {
            // Set the game id only if it was not previously set
            ResId = newId;
        }
        return ResId;
    }

    // Updates the current name (can be done multiple times per game)
    public void _UpdateGameName(string name) {
        ResName = name;
    }

    // Updates the turn (should only be called by the GameLoop)
    public void _UpdateTurn(int newTurn) {
        Turn = newTurn;
    }

    // Clear the modified columns in each model
    public void _ClearModified() {
        MWinter._ClearModified();
        MSummer._ClearModified();
    }

    // Retrieves the building statistics for a given building type
    public int _GetPPStat(Building b) => PPStats[b];

    // Fetches the game ID and thorws an exception if it's not set
    // Exception: NullReferenceException -> ID has not been set yet
    public int _GetGameID() {
        // Check if the ID has been set yet or not
        if(ResId != -1) {
            return ResId;
        }
        // Otherwise throw an exception
        throw new NullReferenceException("Game ID has not been set yet!");
    }

    // Fetches the game name and returns an empty string if not set
    public string _GetGameName() => ResName;

    // Returns whether or not the model is valid
    public bool _GetModelValidity(ModelSeason S) => 
        S == ModelSeason.WINTER ? MWinter._IsValid() : MSummer._IsValid();

    // Returns the current turn
    public int _GetTurn() => Turn;

    // Returns the requested model
    public Model _GetModel(ModelSeason S) => 
        S == ModelSeason.WINTER ? MWinter : MSummer;

    // Returns both models together (first winter then summer)
    public (Model, Model) _GetModels() => (MWinter, MSummer);

    // ==================== Internal Helpers ====================

    // Resets the internal PPStats to set all types to 0
    private void ResetPPStats() {
        // Make sure that the stats are clear
        if(PPStats != null) {
            PPStats.Clear();
        }

        // Initialize the internal stats
        PPStats = new() {
            {Building.Type.GAS, 0},
            {Building.Type.HYDRO, 0},
            {Building.Type.NUCLEAR, 0},
            {Building.Type.SOLAR, 0},
            {Building.Type.TREE, 0}
        };
    }
}

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

// Global state of the game
// This will record all persistent data such as:
// - Game instance id
// - Player actions
// - Aggregated model data
// - Player choice statistics
// ...
public partial class Context : Node {

    // Current internal storage of the game instance's id
    private int ResId = -1;

    // Current internal storage of the game instance's name
    private string ResName = "";

    // Internal representation of the most recent data retrieved from the model
    private Model M;

    // ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
        // Initialize model
        M = new Model(); 
	}    

    // ==================== Public API ====================

    // Updates the internal model using data retrieved from the server
    // This method should only be called from the ModelController directly
    public void _UdpateModelFromServer(Availability A, Capacity C, Demand D) {
        // Update the model's fields
        M._Availability = A;
        M._Capacity = C;
        M._Demand = D;

        // Update the model's coherency state to shared as the data is from the server
        M._MCS = ModelCoherencyState.SHARED;
    }

    // Wrapper for _UdpateModelFromServer that simply unfolds the model struct before calling the update method
    public void _UdpateModelFromServer(Model new_M) {
        _UdpateModelFromServer(new_M._Availability, new_M._Capacity, new_M._Demand);
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
    public bool _GetModelValidity() => M._IsValid();
}

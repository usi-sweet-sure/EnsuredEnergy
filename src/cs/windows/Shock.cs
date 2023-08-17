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

// Represents the generic window that will contain info about the current shock
// These appear at the end of each turn and are described in shocks.xml
public partial class Shock : CanvasLayer {

    [Signal]
    // Signals that one of the reactions has been selected
    public delegate void SelectReactionEventHandler(int id);

    // Big list of shock ids
    private string[] SHOCKS = { 
        "cold_spell", "heat_wave", "glaciers_melting", 
        "severe_weather", "floods", "earthquake",
        "inc_raw_cost", "protest", "mass_immigration",
        "blackout", "pandemic", "nuc_accident",
        "nuc_reintro", "remote_jobs"
    };

    // The currently displayed shock's ID
    private string CurShock;

    // Various labels used in the shock
    private Label Title;
    private Label Text;
    private Label Result;
    
    // Control node containing the reactions
    private Control Reactions;

    // Buttons for each potential reaction
    private Button R1;
    private Button R2;
    private Button R3;

    // Continue button to pass if no reaction is available
    private Button Continue;

    // Controller used to access the config files
    private ShockController SC;

    // Store the current requirements, rewards and possible reactions
    private List<ShockRequirement> CurRequirements;
    private ShockEffect CurReward;
    private List<ShockEffect> CurReactions;

    // ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
        // Fetch nodes
        Title = GetNode<Label>("ColorRect/Title");
        Text = GetNode<Label>("ColorRect/Text");
        Result = GetNode<Label>("ColorRect/Result");
        Reactions = GetNode<Control>("ColorRect/Reactions");
        R1 = GetNode<Button>("ColorRect/Reactions/Button");
        R2 = GetNode<Button>("ColorRect/Reactions/Button2");
        R3 = GetNode<Button>("ColorRect/Reactions/Button3");
        Continue = GetNode<Button>("ColorRect/Continue");
        SC = GetNode<ShockController>("../ShockController");

        // Set the button callbacks
        R1.Pressed += _OnR1Pressed;
        R2.Pressed += _OnR2Pressed;
        R3.Pressed += _OnR3Pressed;

        // Set the initial shock
        CurShock = SHOCKS[0];
        SetFields();
    }

    // ==================== Public API ====================

    // Sets the internal current shock to a newly selected one
    public void _SelectNewShock() {
        // Initialize pseudo-random number generator
        Random rnd = new ();

        // Pick a random number in the range of shock ids
        int next_idx = rnd.Next(SHOCKS.Length);

        // Pick the associated id
        string next_shock = SHOCKS[next_idx];

        // Sanity check: make sure we didn't pick the same one twice
        while(next_shock == CurShock) {
            // Pick the a new id
            next_shock = SHOCKS[rnd.Next(SHOCKS.Length)];
        }

        // Set the next shock
        CurShock = next_shock;

        // Update the fields to match the new shock
        SetFields();
    }

    // Getter for the shock's requirement
    public List<ShockRequirement> _GetRequirements() => CurRequirements;

    // ==================== Internal Helpers ====================

    // Sets all of the fields for the shock once a new one is selected
    private void SetFields() {
        // Extract the name and the description and set the labels to match them
        Title.Text = SC._GetShockName(CurShock);
        Text.Text = SC._GetShockText(CurShock);

        // Set the current requirement
        CurRequirements = SC._GetRequirements(CurShock);

        // Set the current reward
        CurReward = SC._GetReward(CurShock);

        // Retrieve the current reactions
        CurReactions = SC._GetReactions(CurShock);

        // Set the individual buttons if they have associated reactions
        if(CurReactions.Count > 0) {
            // Set the button's text and enable it
            R1.Text = CurReactions[0].Text;
            R1.Disabled = false;
        }
        if(CurReactions.Count > 1) {
            // Set the button's text and enable it
            R2.Text = CurReactions[1].Text;
            R2.Disabled = false;
        }
        if(CurReactions.Count > 2) {
            // Set the button's text and enable it
            R3.Text = CurReactions[2].Text;
            R3.Disabled = false;
        }
    }

    // ==================== Button Callbacks ====================

    // Reaction to the first button being pressed
    // This will trigger the effects set by the first reaction of the shock
    public void _OnR1Pressed() {
        // Signal that the first reaction was picked
        EmitSignal(SignalName.SelectReaction, 0);
    }

    // Reaction to the second button being pressed
    // This will trigger the effects set by the second reaction of the shock
    public void _OnR2Pressed() {
        // Signal that the second reaction was picked
        EmitSignal(SignalName.SelectReaction, 1);
    }

    // Reaction to the third button being pressed
    // This will trigger the effects set by the third reaction of the shock
    public void _OnR3Pressed() {
        // Signal that the third reaction was picked
        EmitSignal(SignalName.SelectReaction, 2);
    }
}

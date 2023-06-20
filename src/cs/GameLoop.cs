using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;

// Models the overarching game loop, which controls every aspect of the game
// and makes sure that things are synchronized across game objects
public partial class GameLoop : Node2D {

	// The total number of turns in the game
	[Export]
	public int N_TURNS = 10;

	// The amount of money the player starts with (in millions of CHF)
	[Export]
	public int START_MONEY = 1000;

	// Contains all of the buildings in the scene
	private List<PowerPlant> Buildings;
	
	// Contains all of the BuildButtons in the scene
	private List<BuildButton> BBs;

	// Reference to the UI
	private UI _UI;

	// Reference to the buildmenu
	private BuildMenu BM;

	//TODO: Add resource managers once they are implemented
	private int Money; // The current money the player has

	private int RemainingTurns; // The number of turns remaining until the end of the game

	//TODO: Add Shocks once they are implemented

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Init Data
		Money = START_MONEY;
		RemainingTurns = N_TURNS;
		Buildings = new List<PowerPlant>();
		BBs = new List<BuildButton>();

		// Fetch initial nodes
		// Start with buildings, in the begining there are only 2 buildings Nuclear and Coal
		Buildings.Add(GetNode<PowerPlant>("World/Nuclear"));
		Buildings.Add(GetNode<PowerPlant>("World/Coal"));

		// Fill in build buttons
		BBs.Add(GetNode<BuildButton>("World/BuildButton"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton2"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton3"));
		BBs.Add(GetNode<BuildButton>("World/BuildButton4"));

		// Fetch UI and BuildMenu
		_UI = GetNode<UI>("UI");
		BM = GetNode<BuildMenu>("BuildMenu");

		// Connect Callback to each build button
		foreach(BuildButton bb in BBs) {
			bb.UpdateBuildSlot += _OnUpdateBuildSlot;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// ========== Interaction Callbacks ==========
	
	// Updates the internal lists on every build slot update
	public void _OnUpdateBuildSlot(BuildButton bb, PowerPlant pp, bool remove) {
		// Check if the update was a power plant addition or removal
		if(remove) {
			//Sanity Check
			Debug.Assert(Buildings.Contains(pp));

			// Destroy the power plant
			Buildings.Remove(pp);

			// Replace it with the new build button
			BBs.Add(bb);
		} else {
			// Sanity Check
			Debug.Assert(BBs.Contains(bb));

			// Destroy the Build Button
			BBs.Remove(bb);

			// Replace it with the new power plant
			Buildings.Add(pp);
		}
	}
}

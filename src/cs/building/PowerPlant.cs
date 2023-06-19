using Godot;
using System;
using System.Diagnostics;

// Represents a Power Plant object in the game
public partial class PowerPlant : Node2D {

	// Defines whether or not the building is a preview
	// This is true when the building is being shown in the build menu
	// and is used to know when to hide certain fields
	[Export]
	public bool IsPreview = false; 

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

	// Children Nodes
	private Sprite2D Sprite;
	private Label NameL;
	private Label PollL;
	private Label EnergyL;
	private Label MoneyL;
	private CheckButton Switch;
	
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
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

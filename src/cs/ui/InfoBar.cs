using Godot;
using System;
using System.Diagnostics;

// Represents a resource bar, specifically used for the environment status and the support status
public partial class InfoBar : ProgressBar {

	// Line showing the target amount to reach
	private Line2D Target;

	// Label containing the name of the bar
	private Label BarName;

	// Info boc showing all of the relevant subfields of this resource
	private InfoBox Box;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch nodes
		Target = GetNode<Line2D>("Target");
		BarName = GetNode<Label>("Name");
		Box = GetNode<InfoBox>("Name/BarInfo");

		Box.Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// Updates the progress value of this information bar
	public void _UpdateProgress(int v) {
		// Sanity Check
		Debug.Assert(MinValue <= v && v <= MaxValue);

		// Update the progress bar's value
		Value = v;
	}

	// Displays the information related to this progress bar
	public void _DisplayInfo() {
		Box._ShowOnlyFilled();
	}

	// Hides the information realted to this progress bar
	public void _HideInfo() {
		Box.Hide();
	}

	// Updates the information of the associated info box 
	// Follows the same calling semantics as the info box:
	// params: varargs in the form of N/Max, T0, N0, T1, N1, T2, N2
	// If parameters are omitted than the label will not be shown
	public void _UpdateInfo(params string[] ts) {
		Box._SetInfo(ts);
	}
}

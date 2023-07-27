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

// UI Script for the import controlling slider
public partial class ImportSlider : HSlider {
	
	[Signal]
	/* Propagates a value update to the rest of the system */
	public delegate void ImportUpdateEventHandler();

	// Constants for target bar positions
	private const int TARGET_100_X_POS = 45;
	private const int TARGET_0_X_POS = -120;

	// Various labels that need to be dynamic
	private Label Amount; // Current selected import percentage
	private Label Text; // The text label describing the slider
	private Button ApplySelection; // Button that confirms the selected import amount
	private Button Cancel; // Button that cancels the modification of the import slider

	// Target import required to meet demand
	private Line2D Target;

	// The confirmed import amount
	private int ImportAmount;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {

		//  Fetch nodes
		Amount = GetNode<Label>("Amount");
		Text = GetNode<Label>("Text");
		Target = GetNode<Line2D>("Target");
		ApplySelection = GetNode<Button>("Apply");
		Cancel = GetNode<Button>("Cancel");

		// Initialize the import amount
		ImportAmount = 0;

		// Connect the various callbacks
		ValueChanged += OnSliderRangeValueChanged;
		ApplySelection.Pressed += OnApplySelectionPressed;
		Cancel.Pressed += OnCancelPressed;
	}

	// ==================== Public API ====================

	// Sets the value of the target based on a given demand percentage
	// The given demand should represent the percentage of the total
	// demand that would need to be imported to cover the lack of supply
	public void _UpdateTargetImport(float demand) {
		// Clamp demand to be in range [0, 1]
		float _d = Math.Max(0.0f, Math.Min(demand, 1.0f));

		// Set the bar position based on the given percentage
		int x_pos = TARGET_0_X_POS + (int)(_d * (TARGET_100_X_POS - TARGET_0_X_POS));
		Target.Position = new Vector2(x_pos, Target.Position.Y);
	}

	// Udpates the text to match the given string
	// This should only be used in the UI object for localization pourposes
	public void _UpdateLabel(string new_label) {
		Text.Text = new_label;
	}

	// Getter for the current value selected with the slider
	public int _GetImportValue() => Math.Max(0, Math.Min((int) ImportAmount, 100));

	// ==================== Signal Callbacks ====================

	// Updates the amount label to match the selected value of the import slider
	// The value given is the new value of the slider
	private void OnSliderRangeValueChanged(double value) {
		// Update the amount label to reflect the selected amount
		Amount.Text = value.ToString() + " %";

		// Show the two selection related buttons
		ApplySelection.Show();
		Cancel.Show();
	}

	// Confirms the selection of a specific import amount
	private void OnApplySelectionPressed() {
		// Save the import amount
		ImportAmount = Math.Max(0, Math.Min((int) Value, 100));

		// Hide the apply selection button
		ApplySelection.Hide();
		Cancel.Hide();
		this.Hide();

		// Propagate the value update to the rest of the system
		EmitSignal(SignalName.ImportUpdate);
	}

	// Cancels any modifications made to the import slider
	private void OnCancelPressed() {
		// Reset the value to the previously saved import amount
		Value = (double)ImportAmount;

		// Hide all buttons
		ApplySelection.Hide();
		Cancel.Hide();
		this.Hide();
	}
	
	private void _on_imports_b_pressed() {
	this.Visible = !this.Visible;
	}
}


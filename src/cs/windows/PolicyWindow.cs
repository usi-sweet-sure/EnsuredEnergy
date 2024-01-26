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
using System.Reflection.Metadata;

// Represents the policy window containing all policy choices
// TODO: This requires implementing policies, which is a tricky task that will require a ton of work.
public partial class PolicyWindow : CanvasLayer {

	// ==================== Children Nodes ====================

	private ColorRect P;
	private AnimationPlayer AP;
	private Button Vote;
	private Button WindButton;
	private ButtonGroup PolicyGroup;
	private BaseButton PressedPolicy;
	private Label VoteResult;

	private Context C;

	private List<Button> PolicyButtons;
	
	private String SelectedPolicy;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		C = GetNode<Context>("/root/Context");
		P = GetNode<ColorRect>("ColorRect");
		AP = GetNode<AnimationPlayer>("AnimationPlayer");
		VoteResult = GetNode<Label>("ColorRect/NinePatchRect/ColorRect2/VoteResult");
		Vote = GetNode<Button>("ColorRect/NinePatchRect/ColorRect2/Vote");
		WindButton = GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Wind_buildtime");
		
		PolicyGroup = WindButton.ButtonGroup;
		PressedPolicy = PolicyGroup.GetPressedButton();
		
		PolicyButtons = new();

		// Fetch policy buttons
		PolicyButtons.Add(GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Wind_buildtime"));
		PolicyButtons.Add(GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Upgrade_wind"));
		PolicyButtons.Add(GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect2/home_regulation"));
		PolicyButtons.Add(GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect2/industry_subsidy"));
		PolicyButtons.Add(GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Upgrade_PV"));

		PolicyButtons[0].Pressed += _OnWindBuildtimePressed;
		PolicyButtons[1].Pressed += _OnUpgradeWindPressed;
		PolicyButtons[2].Pressed += _OnHomeRegulationPressed;
		PolicyButtons[3].Pressed += _OnIndustrySubsidy;
		PolicyButtons[4].Pressed += _OnUpgradePV;
		
		P.GuiInput += _OnPanelGuiInput;
		Vote.Pressed += _OnVotePressed;
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {}

	// ==================== Public API ====================

	// Activates the given animation either forwards or in reverse.
	public void _PlayAnim(string Anim, bool forward = true) {
		if (forward) {
			AP.Play(Anim);
		} else {
			AP.PlayBackwards(Anim);
		}
	}
			
	// ==================== Interaction Callbacks ====================

	// Hides the window if the panel is pressed
	public void _OnPanelGuiInput(InputEvent input) {
		if(input.GetType() == new InputEventMouseButton().GetType())
			Hide();
			Vote.Hide();
			PressedPolicy = PolicyGroup.GetPressedButton();
			if (PressedPolicy != null) {
				PressedPolicy.ButtonPressed = false;
			}
			
	} 

	// A bunch of specific buttons
	public void _OnWindBuildtimePressed() {
		Vote.Show();
	}
	public void _OnUpgradeWindPressed() {
		Vote.Show();
	}
	public void _OnHomeRegulationPressed() {
		Vote.Show();
	}
	public void _OnIndustrySubsidy() {
		Vote.Show();
	}
	public void _OnUpgradePV() {
		Vote.Show();
	}

	public void _OnVotePressed() {
		// Check the vote result based on the selected policy
		PressedPolicy = PolicyGroup.GetPressedButton();
		if (PressedPolicy != null) {
			C._GetGL()._GetPM()._RequestPolicy(PressedPolicy.Name);
			Vote.Disabled = true;
			VoteResult.Show();
			//VoteResult.Text = TODO
		}
	}
}

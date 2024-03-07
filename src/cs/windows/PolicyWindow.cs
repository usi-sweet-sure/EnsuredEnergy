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
using System.Linq;
using System.Reflection.Metadata;

// Represents the policy window containing all policy choices
// TODO: This requires implementing policies, which is a tricky task that will require a ton of work.
public partial class PolicyWindow : CanvasLayer {

	// ==================== Constants ====================

	private const string POLICY_GROUP = "policies";
	private const string POLICY_SUCCESS = "policy_success";
	private const string POLICY_FAILURE = "policy_failure";

	// ==================== Children Nodes ====================

	private ColorRect P;
	private AnimationPlayer AP;
	private Button Vote;
	private Button WindButton;
	private ButtonGroup PolicyGroup;
	private BaseButton PressedPolicy;
	private Label VoteResult;
	private List<Button> PolicyButtons;
	private string SelectedPolicy;

	// ==================== UI fields ====================

	private Label PN; // Policy Name
	private Label PT; // Policy Text
	private Label ET; // Effects Text
	private ProgressBar Pop; // Vote probability

	// ==================== Singletons ====================	

	private Context C;
	private PolicyController PC;
	private TextController TC;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		C = GetNode<Context>("/root/Context");
		PC = GetNode<PolicyController>("/root/PolicyController");
		TC = GetNode<TextController>("/root/TextController");
		P = GetNode<ColorRect>("ColorRect");
		AP = GetNode<AnimationPlayer>("AnimationPlayer");
		VoteResult = GetNode<Label>("ColorRect/NinePatchRect/ColorRect2/VoteResult");
		Vote = GetNode<Button>("ColorRect/NinePatchRect/ColorRect2/Vote");
		WindButton = GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Wind_buildtime");

		// Fetch UI Elements
		PN = GetNode<Label>("ColorRect/NinePatchRect/ColorRect2/PolicyName");
		PT = GetNode<Label>("ColorRect/NinePatchRect/ColorRect2/Text");
		ET = GetNode<Label>("ColorRect/NinePatchRect/ColorRect2/EffectTitle/Text");
		Pop = GetNode<ProgressBar>("ColorRect/NinePatchRect/ColorRect2/Vote/Popularity");
		
		PolicyGroup = WindButton.ButtonGroup;
		PressedPolicy = PolicyGroup.GetPressedButton();
		
		PolicyButtons = new()
		{
			// Fetch policy buttons
			GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Wind_buildtime"),
			GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Upgrade_wind"),
			GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect2/home_regulation"),
			GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect2/industry_subsidy"),
			GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Upgrade_PV")
		};

		// Connect the policy button callbacks
		PolicyButtons.ForEach(pb => pb.Pressed += _OnPolicyButtonPressed);
		
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
	
	// Reset vote button at the end of each turn (player can vote once/turn)
	public void _ResetVote() {
		Vote.Disabled = false;
	}
			
	// ==================== Interaction Callbacks ====================

	// Hides the window if the panel is pressed
	public void _OnPanelGuiInput(InputEvent input) {
		if(input.GetType() == new InputEventMouseButton().GetType()) {
			Hide();
			PressedPolicy = PolicyGroup.GetPressedButton();
			if (PressedPolicy != null) {
				Vote.Hide();
				PressedPolicy.ButtonPressed = false;
			}
		}
	} 

	// When a policy button is pressed, we simply show the vote button 
	// We also need to update the window to display all of the specific data
	public void _OnPolicyButtonPressed() {
		// Allow for the user to trigger a vote
		Vote.Show();

		// Retrieve the policy information to use it to update the UI
		PressedPolicy = PolicyGroup.GetPressedButton();
		if(PressedPolicy != null) {
			// Retrieve the UI infor such as name, text and effects
			// and update the UI with them
			PN.Text = PC._GetPolicyName(PressedPolicy.Name);
			PT.Text = PC._GetPolicyText(PressedPolicy.Name);
			ET.Text = PC._GetEffects("policy", PressedPolicy.Name)
				.Aggregate("", (acc, e) =>
					 e.Text == "" ? acc : acc + "- " + e.Text + "\n"
				);

			// Update the probability preview
			Pop.Value = C._GetGL()._GetPM()._GetRealProb(PressedPolicy.Name) * 100.0f;
		}
	}

	// Attempts a vote and shows the result
	public void _OnVotePressed() {
		// Check the vote result based on the selected policy
		PressedPolicy = PolicyGroup.GetPressedButton();
		if (PressedPolicy != null) {
			// Attempt the vote
			bool success = C._GetGL()._GetPM()._RequestPolicy(PressedPolicy.Name);

			// Disable the vote
			Vote.Disabled = true;
			VoteResult.Show();

			// Show the result
			VoteResult.Text = TC._GetText(
				"labels.xml", POLICY_GROUP, 
				success ? POLICY_SUCCESS : POLICY_FAILURE
			);
		}
	}
}

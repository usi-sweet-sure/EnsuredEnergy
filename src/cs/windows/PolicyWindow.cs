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

	private Context C;

	private List<Button> PolicyButtons;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		C = GetNode<Context>("/root/Context");
		P = GetNode<ColorRect>("ColorRect");
		P.GuiInput += _OnPanelGuiInput;
		AP = GetNode<AnimationPlayer>("AnimationPlayer");
		PolicyButtons = new();

		// Fetch policy buttons
		PolicyButtons.Add(GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Wind_buildtime"));
		PolicyButtons.Add(GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/Upgrade_wind"));
		PolicyButtons.Add(GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/home_regulation"));
		PolicyButtons.Add(GetNode<Button>("ColorRect/NinePatchRect/ColorRect/ColorRect/industry_subsidy"));

		PolicyButtons[0].Pressed += _OnWindBuildtimePressed;
		PolicyButtons[1].Pressed += _OnUpgradeWindPressed;
		PolicyButtons[2].Pressed += _OnHomeRegulationPressed;
		PolicyButtons[3].Pressed += _OnIndustrySubsidy;
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
	} 

	// A bunch of specific buttons
	public void _OnWindBuildtimePressed() {
		C._GetGL()._GetPM()._RequestPolicy("Wind_buildtime");
	}
	public void _OnUpgradeWindPressed() {
		C._GetGL()._GetPM()._RequestPolicy("Upgrade_wind");
	}
	public void _OnHomeRegulationPressed() {
		C._GetGL()._GetPM()._RequestPolicy("home_regulation");
	}
	public void _OnIndustrySubsidy() {
		C._GetGL()._GetPM()._RequestPolicy("industry_subsidy");
	}

}

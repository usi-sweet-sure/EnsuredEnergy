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

// Models the behavior of the main menu screen shown above the game at the start
public partial class MainMenu : CanvasLayer {
	
	// Buttons on the main menu
	private TextureButton Play;
	private TextureButton Lang;

	// Labels used to disaply the text
	private Label Title;
	private Label PlayL;
	private Label LangL;

	// Text Controller for dynamic localization
	private TextController TC;

	// ==================== GODOT Method Overrides ====================
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch Nodes
		TC = GetNode<TextController>("../TextController");
		Play = GetNode<TextureButton>("Play");
		Lang = GetNode<TextureButton>("Lang");
		Title = GetNode<Label>("Title");
		PlayL = GetNode<Label>("Play/PlayL");
		LangL = GetNode<Label>("Lang/LangL");
		
		// Connect button callbacks
		Play.Pressed += _OnPlayPressed;
		Lang.Pressed += _OnLangPressed;
	}
	
	// ==================== Interaction Callbacks ====================

	private void _OnPlayPressed() {
		Hide();
	}
	
	private void _OnLangPressed() {
		// TODO
	}

}

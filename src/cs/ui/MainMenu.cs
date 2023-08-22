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

	// IDs for accessing XML text fields
	private const string MENU_FILE = "labels.xml";
	private const string MENU_GROUP = "menu";
	private const string TITLE_ID = "title";
	private const string PLAY_ID = "label_play";
	private const string LANG_ID = "label_lang";
	
	// Buttons on the main menu
	private TextureButton Play;
	private TextureButton Lang;

	// Labels used to disaply the text
	private Label Title;
	private Label PlayL;
	private Label LangL;

	// Text Controller for dynamic localization
	private TextController TC;

	// Context, used to update the language
	private Context C;

	// ==================== GODOT Method Overrides ====================
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch Nodes
		C = GetNode<Context>("/root/Context");
		TC = GetNode<TextController>("../TextController");
		Play = GetNode<TextureButton>("Play");
		Lang = GetNode<TextureButton>("Lang");
		Title = GetNode<Label>("Title");
		PlayL = GetNode<Label>("Play/PlayL");
		LangL = GetNode<Label>("Lang/LangL");
		
		// Connect button callbacks
		Play.Pressed += _OnPlayPressed;
		Lang.Pressed += _OnLangPressed;

		// Initialize the various labels
		SetLabels();
	}
	
	// ==================== Interaction Callbacks ====================

	// Starts the game 
	private void _OnPlayPressed() {
		// Our main menu is simply an overlay
		Hide();
	}
	
	// Updates the language
	private void _OnLangPressed() {
		// Update the language 
		C._NextLanguage();

		// Update the labels
		SetLabels();
	}

	// ==================== Internal Helpers ====================

	// Sets all of the text fields based on the current state of the text controller
	private void SetLabels() {
		Title.Text = TC._GetText(MENU_FILE, MENU_GROUP, TITLE_ID);
		PlayL.Text = TC._GetText(MENU_FILE, MENU_GROUP, PLAY_ID);
		LangL.Text = C._GetLanguageName();
	}
}

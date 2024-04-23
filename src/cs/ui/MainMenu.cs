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
	private const string MODE_ID = "label_mode";
	private const string OFFLINE_ID = "mode_offline";
	private const string ONLINE_ID = "mode_online";
	
	// Buttons on the main menu
	private TextureButton Play;
	private TextureButton Lang;
	private TextureButton Mode;

	// Labels used to disaply the text
	private Label Title;
	private Label PlayL;
	private Label LangL;
	private Label OfflineL;
	private Label Drag;
	private Label Scroll;

	// Text Controller for dynamic localization
	private TextController TC;

	// Context, used to update the language
	private Context C;

	// Game loop to start the game
	private GameLoop GL;

	// ==================== GODOT Method Overrides ====================
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch Nodes
		C = GetNode<Context>("/root/Context");
		GL = GetOwner<GameLoop>();
		TC = GetNode<TextController>("/root/TextController");
		Play = GetNode<TextureButton>("Play");
		Lang = GetNode<TextureButton>("Lang");
		Mode = GetNode<TextureButton>("Offline");
		Title = GetNode<Label>("Title");
		PlayL = GetNode<Label>("Play/PlayL");
		LangL = GetNode<Label>("Lang/LangL");
		OfflineL = GetNode<Label>("Offline/OfflineL");
		Drag = GetNode<Label>("BlueprintNormal/Drag");
		Scroll = GetNode<Label>("BlueprintNormal/Scroll");
		
		// Connect button callbacks
		Play.Pressed += _OnPlayPressed;
		Play.Pressed += GL._OnPlayPressed;
		Lang.Pressed += _OnLangPressed;
		Mode.Pressed += _OnModePressed;

		// Initialize the various labels
		SetLabels();

		//////////////////////////////////////////
		//////////For Demo, remove later//////////
		Mode.Disabled = true;
		bool off = C._ToggleOffline();
		if(!off) C._ToggleOffline();
		SetLabels();
		//////////////////////////////////////////
		//////////////////////////////////////////
	}

	// ==================== Public API ====================
	// Resets the main menu to its initial state
	public void _Reset() {
		// The only thing that is needed to reset the main menu is to
		// reset the labels so that they have the correct language
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

	// Toggles the offline/online modes
	private void _OnModePressed() {
		// Update the offline mode
		C._ToggleOffline();

		// Update the labels
		SetLabels();
	}

	// ==================== Internal Helpers ====================

	// Sets all of the text fields based on the current state of the text controller
	private void SetLabels() {
		Title.Text = TC._GetText(MENU_FILE, MENU_GROUP, TITLE_ID);
		PlayL.Text = TC._GetText(MENU_FILE, MENU_GROUP, PLAY_ID);
		LangL.Text = C._GetLanguageName();
		Drag.Text = TC._GetText(MENU_FILE, MENU_GROUP, "hold_drag");
		Scroll.Text = TC._GetText(MENU_FILE, MENU_GROUP, "scroll_zoom");
		OfflineL.Text = TC._GetText(MENU_FILE, MENU_GROUP, MODE_ID) + ": " +
			(C._GetOffline() ? TC._GetText(MENU_FILE, MENU_GROUP, OFFLINE_ID) :
							   TC._GetText(MENU_FILE, MENU_GROUP, ONLINE_ID)
			);
	}
}

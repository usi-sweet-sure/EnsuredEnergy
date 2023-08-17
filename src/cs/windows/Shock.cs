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

// Represents the generic window that will contain info about the current shock
// These appear at the end of each turn and are described in shocks.xml
public partial class Shock : CanvasLayer {
    // Various labels used in the shock
    private Label Title;
    private Label Text;
    private Label Result;
    
    // Control node containing the reactions
    private Control Reactions;

    // Buttons for each potential reaction
    private Button R1;
    private Button R2;
    private Button R3;

    // Continue button to pass if no reaction is available
    private Button Continue;

    // ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
        // Fetch nodes
        Title = GetNode<Label>("ColorRect/Title");
        Text = GetNode<Label>("ColorRect/Text");
        Result = GetNode<Label>("ColorRect/Result");
        Reactions = GetNode<Control>("ColorRect/Reactions");
        R1 = GetNode<Button>("ColorRect/Reactions/Button");
        R2 = GetNode<Button>("ColorRect/Reactions/Button2");
        R3 = GetNode<Button>("ColorRect/Reactions/Button3");
        Continue = GetNode<Button>("ColorRect/Continue");
    }

}

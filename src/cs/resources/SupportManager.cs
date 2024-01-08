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
using System.Xml;

// Updates and handles the political support that the player has
public partial class SupportManager : Node {
	private Support S;
	private const int SUPPORT_DEFAULT_VALUE = 60;
	

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		S = new(SUPPORT_DEFAULT_VALUE);
	}

	// ==================== Public API ====================

	// Getters for the support
	public Support _GetSupport() => S;
	public int _GetSupportValue() => S.Value;

	// Increases the value of the support by the given diff amount (can be negative) 
	public void _UpdateSupport(int diff) {
		S.Value = Math.Max(S.Value + diff, 0);
	}

	// Setter for the support
	public void _SetSupport(int newval) {
		S.Value = Math.Max(newval, 0);
	}

	// Resets the support manager back to its default values
	public void _Reset() {
		S = new(SUPPORT_DEFAULT_VALUE);
	}
}

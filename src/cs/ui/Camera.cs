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

// Models the interactive camera system which allows the player 
// to drag the view around and zoom in and out.
public partial class Camera : Camera2D {

	// Camera control parameters
	private Vector2 ZOOM_MIN = new (0.5f,0.5f);
	private Vector2 ZOOM_MAX = new (1f,1f);
	private Vector2 ZOOM_SPEED = new (0.2f,0.2f);
	private Vector2 ZoomVal = new (1.0f,1.0f);

	// ==================== GODOT Method Overrides ====================

	// Adds control to inputs that otherwise would not have triggered any events  
	// In our case, this includes 2 events:  
	//     1) When the player clicks the screen, in which case we want to drag the camera around
	//     2) When the player uses the mouse wheel, in which case we want to zoom in or out
	public override void _UnhandledInput(InputEvent E) {
		// Camera can be moved by holding left click and dragging the mouse
		if(E is InputEventMouseMotion MouseMotion) {
			if(MouseMotion.ButtonMask == MouseButtonMask.Left)
				Position -= MouseMotion.Relative / Zoom;
		}

		// Can zoom the camera using the mouse wheel, smoothed with a tween animation
		if(E is InputEventMouseButton MouseBtn) {
			// Check what type of scroll was done
			switch(MouseBtn.ButtonIndex) {
				// If we are scrolling down, then we want the view to zoom out
				case MouseButton.WheelDown:
					// Make sure that we clamp the zoom to avoid seeing out of the scene
					if(Zoom > ZOOM_MIN) {
						// Udpate the zoom using a fancy animation to smoothen the transition
						ZoomVal = Zoom - ZOOM_SPEED;
						Tween TweenZoomIn = CreateTween();
						TweenZoomIn.TweenProperty(this, "zoom", ZoomVal, 0.3f);
					}
				break;
				// If we are scrolling up, the we want the view to zoom in
				case MouseButton.WheelUp: 
					// Make sure we can't over zoom
					if(Zoom < ZOOM_MAX) {
						// Update the zoom using a fancy animation
						ZoomVal = Zoom + ZOOM_SPEED;
						Tween TweenZoomOut = CreateTween();
						TweenZoomOut.TweenProperty(this, "zoom", ZoomVal, 0.3f);
					}
				break;
			}
		}
	}
}

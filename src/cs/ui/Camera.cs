using Godot;
using System;

public partial class Camera : Camera2D
{
	private Vector2 ZOOM_MIN = new Vector2(0.5f,0.5f);
	private Vector2 ZOOM_MAX = new Vector2(1f,1f);
	private Vector2 ZOOM_SPEED = new Vector2(0.2f,0.2f);
	private Vector2 ZoomVal = new Vector2(1.0f,1.0f);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public override void _UnhandledInput(InputEvent @event) {
	// Camera can be moved by holding left click and dragging the mouse
		if (@event is InputEventMouseMotion MouseMotion) {
			if (MouseMotion.ButtonMask == MouseButtonMask.Left)
				Position -= MouseMotion.Relative / Zoom;
		}
	// Can zoom the camera using the mouse wheel, smoothed with a tween animation
		if (@event is InputEventMouseButton MouseBtn) {
			if (MouseBtn.ButtonIndex == MouseButton.WheelDown)
				if (Zoom > ZOOM_MIN) {
					ZoomVal = Zoom - ZOOM_SPEED;
					Tween TweenZoomIn = CreateTween();
					TweenZoomIn.TweenProperty(this, "zoom", ZoomVal, 0.3f);
				}
			if (MouseBtn.ButtonIndex == MouseButton.WheelUp)
				if (Zoom < ZOOM_MAX) {
					ZoomVal = Zoom + ZOOM_SPEED;
					Tween TweenZoomOut = CreateTween();
					TweenZoomOut.TweenProperty(this, "zoom", ZoomVal, 0.3f);
				}
		}
	}
}

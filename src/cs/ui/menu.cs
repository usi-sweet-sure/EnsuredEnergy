using Godot;
using System;

public partial class menu : CanvasLayer
{
	
	private TextureButton Play;
	private TextureButton Lang;
	
	
	public override void _Ready() {
		Play = GetNode<TextureButton>("Play");
		Lang = GetNode<TextureButton>("Lang");
		
		Play.Pressed += _OnPlayPressed;
		Lang.Pressed += _OnLangPressed;
	}
	
	private void _OnPlayPressed() {
		Hide();
	}
	
	private void _OnLangPressed() {
		// TODO
	}

}

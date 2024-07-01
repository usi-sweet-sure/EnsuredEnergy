using Godot;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public partial class WebTest : Node2D
{
	
	private Label label;
	private Button button;
	private TextEdit TE;
	
	private Context C;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		C = GetNode<Context>("/root/Context");
		
		label = GetNode<Label>("Label");
		button = GetNode<Button>("Button");
		TE = GetNode<TextEdit>("TextEdit");
		
		button.Pressed += _OnButtonPressed;
	}
	
	private void _OnButtonPressed() {
		label.Text = TE.Text;
		C.test = TE.Text;
		GD.Print(C.test);
	}

}

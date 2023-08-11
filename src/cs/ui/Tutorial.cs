using Godot;
using System;

public partial class Tutorial : CanvasLayer
{
	private string[] TutoText = new string[]{
		"Congratulations !\nYou have been designated to [b]manage the power grid[/b] and ensure a sustainable and resilient energy sector.", 
		"You need to [b]supply[/b] enough [b]energy[/b] to your population. \nThey need it to charge their phones and play their favourite video games.",
		"You need to [b]supply[/b] enough [b]energy[/b] to your population. \nThey need it to charge their phones and play their favourite video games.", 
		"There are different ways you can supply more energy:",
		"There are different ways you can supply more energy:",
	};
	//"You can reach the demand by either: \n - Implementing different policies"
	
	private Button B;
	private RichTextLabel L;
	private int i = 0;
	private AnimationPlayer AP;
	private NinePatchRect R2;
	private NinePatchRect R3;
	private NinePatchRect R4;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		B = GetNode<Button>("Rect/ColorRect/Button");
		AP = GetNode<AnimationPlayer>("AnimationPlayer");
		L = GetNode<RichTextLabel>("Rect/ColorRect/Text");
		R2 = GetNode<NinePatchRect>("Rect2");
		R3 = GetNode<NinePatchRect>("Rect3");
		R4 = GetNode<NinePatchRect>("Rect4");
		
		L.Text = TutoText[i];
		
		B.Pressed += _OnButtonPressed;
		
	}
	
	public void _OnButtonPressed() {
		if (i < TutoText.Length - 1) {
			i++;
			L.Text = TutoText[i];
			if (i == 2) {
				R2.Show();
			}
			if (i == 3) {
				R2.Hide();
				R3.Show();
			}
			if (i == 4) {
				R3.Hide();
				R4.Show();
			}
		} else {
			Hide();
		}
	}
}

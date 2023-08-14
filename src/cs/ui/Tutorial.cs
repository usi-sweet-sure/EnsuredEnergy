using Godot;
using System;

public partial class Tutorial : CanvasLayer
{
	private string[] TutoText = new string[]{
		"Congratulations ! \nYou have been designated to [b]manage the power grid[/b] and ensure a sustainable and resilient energy sector.", 
		"You need to [b]supply[/b] enough [b]energy[/b] for your population. \nThey need it to charge their phones and play their favourite video games.",
		"You need to [b]supply[/b] enough [b]energy[/b] for your population. \nThey need it to charge their phones and play their favourite video games.", 
		"There are different ways to supply more energy :\n- You can import energy, but it pollutes and can be expensive.",
		"There are different ways to supply more energy :\n- You can build new power plants, but it costs time and money.",
	};
	//There are different ways to supply more energy:\n- You can implement different policies, but your population needs to approve."
	
	private Button B;
	private RichTextLabel L;
	private int i = 0;
	private AnimationPlayer AP;
	private NinePatchRect IB;
	private NinePatchRect IB2;
	private NinePatchRect IB3;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		B = GetNode<Button>("TutoPopUp/ColorRect/Button");
		AP = GetNode<AnimationPlayer>("AnimationPlayer");
		L = GetNode<RichTextLabel>("TutoPopUp/ColorRect/Text");
		IB = GetNode<NinePatchRect>("InfoBubble");
		IB2 = GetNode<NinePatchRect>("InfoBubble2");
		IB3 = GetNode<NinePatchRect>("InfoBubble3");
		
		L.Text = TutoText[i];
		
		B.Pressed += _OnButtonPressed;
		
	}
	
	public void _OnButtonPressed() {
		if (i < TutoText.Length - 1) {
			i++;
			L.Text = TutoText[i];
			if (i == 2) {
				IB.Show();
			}
			if (i == 3) {
				IB.Hide();
				IB2.Show();
			}
			if (i == 4) {
				IB2.Hide();
				IB3.Show();
			}
		} else {
			Hide();
		}
	}
}

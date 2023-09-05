using Godot;
using System;

public partial class End : CanvasLayer
{
	// End screen stat labels
	private Label Shocks;
	private Label EnergyW;
	private Label EnergyS;
	private Label Support;
	private Label EnvScore;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		
		Shocks = GetNode<Label>("Stats/Shocks");
		EnergyW = GetNode<Label>("Stats/EnergyW");
		EnergyS = GetNode<Label>("Stats/EnergyS");
		Support = GetNode<Label>("Stats/Support");
		EnvScore = GetNode<Label>("Stats/Env");
		
	}
	
	public void _SetEndStats(float EnW, float EnS, double Supp, double Env) {
		EnergyW.Text = EnW.ToString();
		EnergyS.Text = EnS.ToString();
		Support.Text = (Supp * 100).ToString() + "%";
		// I don't know where to get the env value...
		//EnvScore.Text = Env.ToString();
	}


}

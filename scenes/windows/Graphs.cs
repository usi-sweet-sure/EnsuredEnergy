using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

public partial class Graphs : CanvasLayer
{
	
	private Button Close;
	
	private Vector2 ScreenSize;
	private ColorRect Screen;
	private List<int> YearX;
	private List<int> PointsY;
	private int NUM_YEARS = 10;
	
	private int StackedEnergy;
	
	
	private Line2D DemandW;
	private Line2D DemandS;
	
	private Control PowerPlantNode;
	
	private Context C;
	private GameLoop GL;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		C = GetNode<Context>("/root/Context");
		GL = GetNode<GameLoop>("/root/Main");
		
		Screen = GetNode<ColorRect>("Screen");
		DemandW = GetNode<Line2D>("Screen/Demand/DemandW");
		DemandS = GetNode<Line2D>("Screen/Demand/DemandS");
		Close = GetNode<Button>("Close");
		PowerPlantNode = GetNode<Control>("Screen/PowerPlants");
		
		YearX = new List<int>();
		PointsY = new List<int>();
		
		
		
		Close.Pressed += _OnClosePressed;
		
		_SetYearXPosition();
		_SetLineXPoints(DemandW);
		_SetLineXPoints(DemandS);
		C._InitDemand();
		_SetLineYPoints(DemandW, (int)C._GetDemand().Item1, (int)C._GetDemandInc().Item1);
		_SetLineYPoints(DemandS, (int)C._GetDemand().Item2, (int)C._GetDemandInc().Item2);
		_ChangePoint(DemandW, 7, 5, true);
	}
	
	// Sets the graph line X positions
	public void _SetLineXPoints(Line2D line) {
		for (int i = 0; i < NUM_YEARS; i++) {
			line.SetPointPosition(i, new Vector2(YearX[i],line.Points[i].Y));
		}
	}
	
	// Sets the Year line on x depending on the screensize
	public void _SetYearXPosition() {
		for (int i = 0; i < NUM_YEARS; i++) {
			int dist = (int)Screen.Size.X/NUM_YEARS;
			YearX.Add(i * dist);
		}
	}
	
	// Sets the line's y values depending on the screensize
	public void _SetLineYPoints(Line2D line, int Init, int Inc_Rate) {
		for (int i = 0; i < NUM_YEARS; i++) {
			int point = (int)Mathf.Remap(Init + (Inc_Rate * i), 0, 200, Screen.Size.Y, 0);
			line.SetPointPosition(i, new Vector2(line.Points[i].X, point));
		}
	}
	
	// Shocks or events can change the Y values of a line, either as a one off event or long term
	public void _ChangePoint(Line2D line, int turn, int new_val, bool long_term=false) {
		if(long_term) {
			for (int i = turn; i < NUM_YEARS; i++) {
				int point = (int)Mathf.Remap(new_val, 0, 200, Screen.Size.Y, 0);
				line.SetPointPosition(i, new Vector2(line.Points[i].X, line.Points[i].Y + point - Screen.Size.Y));
			}
		} else {
			int point = (int)Mathf.Remap(new_val, 0, 200, Screen.Size.Y, 0);
			line.SetPointPosition(turn, new Vector2(line.Points[turn].X, point));
		}
	}
	
	public void _CreatePPLine(float energy, string name = "") {
		StackedEnergy += (int)energy;
		Line2D new_line = new Line2D();
		new_line.Width = 2;
		new_line.DefaultColor = new Color(0,1,0,1);
		new_line.Points = DemandW.Points;
		_SetLineYPoints(new_line, StackedEnergy, 0);
		PowerPlantNode.AddChild(new_line);
		
		Label new_label = new Label();
		new_label.Text = name;
		//new_label.HorizontalAlignment = HorizontalAlignment.Right;
		new_label.Position = new Vector2(-80, new_line.Points[0].Y);
		new_line.AddChild(new_label);
	}
	
	
	private void _OnClosePressed() {
		foreach(PowerPlant pp in GL._GetPowerPlants()) {
			_CreatePPLine(pp._GetCapacity() * pp._GetAvailability().Item1, pp.PlantName);
		}
		
		Hide();
	}
	
}

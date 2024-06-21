using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

public partial class Graphs : CanvasLayer
{
	// Graph scene nodes
	private Button Close;
	private Vector2 ScreenSize;
	private ColorRect Screen;
	private Line2D DemandW;
	private Line2D DemandS;
	private Control PowerPlantNode;
	private CheckButton SeasonSwitch;
	
	// Graph variables
	private List<int> YearX;
	private List<int> PointsY;
	private List<PowerPlant> PowerPlantList;
	private int NUM_YEARS = 10;
	private int NUM_PP = 12;
	private int PlantNum;
	private int StackedEnergy;
	
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
		PowerPlantList = new List<PowerPlant>();
		
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
	private void _SetLineXPoints(Line2D line) {
		for (int i = 0; i < NUM_YEARS; i++) {
			line.SetPointPosition(i, new Vector2(YearX[i],line.Points[i].Y));
		}
	}
	
	// Sets the Year line on x depending on the screensize
	private void _SetYearXPosition() {
		for (int i = 0; i < NUM_YEARS; i++) {
			int dist = (int)Screen.Size.X/NUM_YEARS;
			YearX.Add(i * dist);
		}
	}
	
	// Sets the line's y values depending on the screensize
	private void _SetLineYPoints(Line2D line, int Init, int Inc_Rate) {
		for (int i = 0; i < NUM_YEARS; i++) {
			int point = (int)Mathf.Remap(Init + (Inc_Rate * i), 0, 200, Screen.Size.Y, 0);
			line.SetPointPosition(i, new Vector2(line.Points[i].X, point));
		}
	}
	
	// Add new point to a line
	private void _AddPoint(Line2D line, int init, int turn) {
		int point = (int)Mathf.Remap(init, 0, 200, Screen.Size.Y, 0);
		line.AddPoint(new Vector2(YearX[turn], point), turn);
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
	
	private void _CreateLine(string name, int x_pos) {
		Line2D new_line = new Line2D();
		// Line2D properties
		new_line.Width = 2;
		new_line.DefaultColor = new Color(0,1,0,1);
		//new_line.Points = DemandW.Points;
		new_line.Name = name;
		_AddPoint(new_line, x_pos, C._GetTurn());
		_AddPoint(new_line, x_pos, C._GetTurn()+1);
		PowerPlantNode.AddChild(new_line);
		// create a label for the line
		Label new_label = new Label();
		new_label.Text = name;
		//new_label.HorizontalAlignment = HorizontalAlignment.Right;
		new_label.Position = new Vector2(-80, new_line.Points[0].Y);
		new_line.AddChild(new_label);
	}
	
	// Create a powerplant line
	public void _CreatePPLine(int x_pos, string name, bool first) {
		// At the start of the game 
		if(first && C._GetTurn() == 0) {
			_CreateLine(name, x_pos);
		} 
		if(first && C._GetTurn() > 0) {
			// at the start of each turn add a new point
			foreach(Node line in PowerPlantNode.GetChildren()){
				if (line.Name == name) {
					_AddPoint(line as Line2D, x_pos, C._GetTurn()+1);
					}
					}
		} else {
			// else changes existing point
			foreach(Node line in PowerPlantNode.GetChildren()){
				if (line.Name == name) {
					_ChangePoint(line as Line2D, C._GetTurn()+1, x_pos, false);
					}
					}
		}
	
		
		
	}
	
	


	public void _InstancePPLines(bool first) {
			StackedEnergy = 0;
			List<PowerPlant> pplist = GL._GetPowerPlants().OrderBy(pp => pp.PlantName).ToList();
			PlantNum = pplist.Count();
			for (int i = 0; i < pplist.Count(); i++) {
				var pp = pplist[i];
				var energy = (int)(pp._GetCapacity() * pp._GetAvailability().Item1);
				StackedEnergy += energy;

			if (i == pplist.Count()-1) {
				if(pplist[i-1].PlantName != pp.PlantName) {
					_CreatePPLine(StackedEnergy, pp.PlantName, first);
				}
			} else {
				if(pp.PlantName != pplist[i+1].PlantName) {
					_CreatePPLine(StackedEnergy, pp.PlantName, first);
				}
			}
		}
		}
	

	
	
	private void _OnClosePressed() {
		Hide();
	}
	
}

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
	private Control PowerPlantW;
	private Control PowerPlantS;
	private CheckButton SeasonSwitch;
	private Control Economy;
	private Control Pollution;
	
	private Theme LabelTheme;
	
	// Graph variables
	private List<int> YearX;
	private List<int> PointsY;
	private int NUM_YEARS = 11;
	private int StackedEnergyW;
	private int StackedEnergyS;
	
	private Context C;
	private GameLoop GL;
	private ResourceManager RM;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		C = GetNode<Context>("/root/Context");
		GL = GetNode<GameLoop>("/root/Main");
		RM = GL._GetRM();
		
		Screen = GetNode<ColorRect>("Screen");
		DemandW = GetNode<Line2D>("Screen/Demand/DemandW");
		DemandS = GetNode<Line2D>("Screen/Demand/DemandS");
		Close = GetNode<Button>("Close");
		PowerPlantW = GetNode<Control>("Screen/PowerPlantsW");
		PowerPlantS = GetNode<Control>("Screen/PowerPlantsS");
		SeasonSwitch = GetNode<CheckButton>("SeasonSwitch");
		Economy = GetNode<Control>("Screen/Economy");
		Pollution = GetNode<Control>("Screen/Pollution");
		
		LabelTheme = GD.Load("res://scenes/windows/label_themes.tres") as Theme;
		
		YearX = new List<int>();
		PointsY = new List<int>();
		
		Close.Pressed += _OnClosePressed;
		SeasonSwitch.Pressed += _OnSwitchPressed;
		
		_SetYearXPosition();
		_SetLineXPoints(DemandW);
		_SetLineXPoints(DemandS);
		C._InitDemand();
		_SetLineYPoints(DemandW, (int)C._GetDemand().Item1, (int)C._GetDemandInc().Item1);
		_SetLineYPoints(DemandS, (int)C._GetDemand().Item2, (int)C._GetDemandInc().Item2);
		_DrawYearLines();
		
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
	
	// Draws year lines on X
	private void _DrawYearLines() {
		int i = 0;
		foreach(int year in YearX) {
				Line2D new_line = new Line2D();
				new_line.Width = 2;
				new_line.DefaultColor = new Color(1,1,1,0.4f);
				new_line.AddPoint(new Vector2(year, 0), 0);
				new_line.AddPoint(new Vector2(year, Screen.Size.Y), 1);
				Screen.AddChild(new_line);
				// create a label for the line
				Label new_label = new Label();
				new_label.Text = (2022+(i * 3)).ToString();
				i++;
				new_label.Position = new Vector2(new_line.Points[1].X - 20, Screen.Size.Y + 20);
				new_line.AddChild(new_label);
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
	private void _AddPoint(Line2D line, int init, int turn, int scale) {
		int point = (int)Mathf.Remap(init, 0, scale, Screen.Size.Y, 0);
		line.AddPoint(new Vector2(YearX[turn], point), turn);
		
		// create info button
		Button new_button = new Button();
		//new_button.FocusMode = FocusMode.FocusNone;
		//new_button.Flat = true;
		new_button.CustomMinimumSize = new Vector2(10, 10);
		new_button.Position = new Vector2(YearX[turn], point);
		new_button.TooltipText = init.ToString();
		line.AddChild(new_button);
	}
	
	// Shocks or events can change the Y values of a line, either as a one off event or long term
	public void _ChangePoint(Line2D line, int turn, int new_val, int scale, bool long_term=false) {
		if(long_term) {
			for (int i = turn; i < NUM_YEARS; i++) {
				int point = (int)Mathf.Remap(new_val, 0, scale, Screen.Size.Y, 0);
				line.SetPointPosition(i, new Vector2(line.Points[i].X, line.Points[i].Y + point - Screen.Size.Y));
			}
		} else {
			int point = (int)Mathf.Remap(new_val, 0, scale, Screen.Size.Y, 0);
			line.SetPointPosition(turn, new Vector2(line.Points[turn].X, point));
		}
	}
	
	private void _CreateLine(string name, int x_pos, Control node, int scale) {
		Line2D new_line = new Line2D();
		// Line2D properties
		new_line.Width = 2;
		new_line.DefaultColor = new Color(0,1,0,1);
		new_line.Name = name;
		_AddPoint(new_line, x_pos, C._GetTurn(), scale);
		_AddPoint(new_line, x_pos, C._GetTurn()+1, scale);
		node.AddChild(new_line);
		
		// create a label for the line
		Label new_label = new Label();
		new_label.Text = name;
		new_label.Theme = LabelTheme;
		new_label.ThemeTypeVariation = "Screen";
		new_label.CustomMinimumSize = new Vector2(150, 0);
		new_label.HorizontalAlignment = HorizontalAlignment.Right;
		new_label.Position = new Vector2(-170, new_line.Points[0].Y - 20);
		new_line.AddChild(new_label);
	}
	
	// Create a powerplant line
	public void _CreatePPLine(int x_pos, string name, bool first, Control node, int scale) {
		// At the start of the game. create new line
		if(first && C._GetTurn() == 0) {
			_CreateLine(name, x_pos, node, scale);
		} 
		if(first && C._GetTurn() > 0) {
			// at the start of each turn add a new point
			foreach(Node line in node.GetChildren()){
				if (line.Name == name) {
					_AddPoint(line as Line2D, x_pos, C._GetTurn()+1, scale);
					}
				}
		} else {
			// else changes existing point
			foreach(Node line in node.GetChildren()){
				if (line.Name == name) {
					_ChangePoint(line as Line2D, C._GetTurn()+1, x_pos, scale, false);
					}
				}
		}
	}
	

	public void _InstancePPLines(bool first) {
		// Retrieve the current resources
		(Energy Eng, Environment Env, Support Sup) = GL._GetResources();
		
		_CreatePPLine(GL.Money.Money, "Money", first, Economy, 1000);
		GD.Print(Env.EnvBarValue());
		int EnvValue = (int)(Env.EnvBarValue() * 100);
		_CreatePPLine(EnvValue, "Environment", first, Pollution, 100);
		_CreatePPLine((int)Env.PollutionBarValue(), "Pollution", first, Pollution, 100);
		
		StackedEnergyW = 0;
		StackedEnergyS = 0;
		List<PowerPlant> pplist = GL._GetPowerPlants().OrderByDescending(pp => pp._GetCapacity()).ThenBy(pp => pp.PlantName).ToList();
		int PlantNum = pplist.Count();
		for (int i = 0; i < pplist.Count(); i++) {
			var pp = pplist[i];
			var energyW = (int)(pp._GetCapacity() * pp._GetAvailability().Item1);
			var energyS = (int)(pp._GetCapacity() * pp._GetAvailability().Item2);
			StackedEnergyW += energyW;
			StackedEnergyS += energyS;

		if (i == pplist.Count()-1) {
			if(pplist[i-1].PlantName != pp.PlantName) {
				_CreatePPLine(StackedEnergyW, pp.PlantName, first, PowerPlantW, 150);
				_CreatePPLine(StackedEnergyS, pp.PlantName, first, PowerPlantS, 150);
			}
		} else {
			if(pp.PlantName != pplist[i+1].PlantName) {
				_CreatePPLine(StackedEnergyW, pp.PlantName, first, PowerPlantW, 150);
				_CreatePPLine(StackedEnergyS, pp.PlantName, first, PowerPlantS, 150);
			}
		}
	}
		}
	

	private void _OnSwitchPressed() {
		PowerPlantW.Visible = !PowerPlantW.Visible;
		PowerPlantS.Visible = !PowerPlantS.Visible;
		DemandW.Visible = !DemandW.Visible;
		DemandS.Visible = !DemandS.Visible;
	}
	
	private void _OnClosePressed() {
		Hide();
	}
	
}

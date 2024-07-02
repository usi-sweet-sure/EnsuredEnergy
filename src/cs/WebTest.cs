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
	
	private Label label1;
	private Label label2;
	private Button button1;
	private TextEdit text1;
	
	private ServerTest S;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		S = GetNode<ServerTest>("/root/ServerTest"); 
		
		label1 	= GetNode<Label>("Label1");
		label2 	= GetNode<Label>("Label2");
		button1 = GetNode<Button>("Button1");
		text1 	= GetNode<TextEdit>("TextEdit1");

		label1.Text = "hello";
		label2.Text = "there";
		
		button1.Pressed += _OnButtonPressed;
	}
	
	private void _OnButtonPressed() {

		string yr = text1.Text;

		string url = $"https://sure.euler.usi.ch/res.php?mth=ctx&res_id=1&yr={yr}";
		label1.Text = url;
		
		XmlDocument xmldoc = new XmlDocument();
		xmldoc.Load(url); 

		
		

		XmlNode	row = xmldoc.DocumentElement.FirstChild.FirstChild;

		
		label1.Text = row.Attributes["yr"].Value;
		label2.Text = row.Attributes["cnv_riv_hyd"].Value;
	}

}

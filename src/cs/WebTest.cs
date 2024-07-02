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
	private Label label3;
	private Button button1;
	private TextEdit text1;
	private TextEdit text2;
	
	private ServerTest S;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		S = GetNode<ServerTest>("/root/ServerTest"); 
		
		label1 	= GetNode<Label>("Label1");
		label2 	= GetNode<Label>("Label2");
		label3 	= GetNode<Label>("Label3");
		button1 = GetNode<Button>("Button1");
		text1 	= GetNode<TextEdit>("TextEdit1");
		text2 	= GetNode<TextEdit>("TextEdit2");

		text1.Text = "2025";
		text2.Text = "prm_imp_ele";
		
		button1.Pressed += _OnButtonPressed;
	}
	
	private void _OnButtonPressed() {

		string yr = text1.Text;
		string fld = text2.Text;

		string url = $"https://sure.euler.usi.ch/res.php?mth=ctx&res_id=1&yr={yr}";
		label1.Text = url;
		
		XmlDocument xmldoc = new XmlDocument();
		xmldoc.Load(url); 

		XmlNode	row = xmldoc.DocumentElement.FirstChild.FirstChild;

		label1.Text = row.Attributes["yr"].Value;
		label2.Text = row.Attributes[fld].Value;
		label3.Text = row.OuterXml;
	}

}

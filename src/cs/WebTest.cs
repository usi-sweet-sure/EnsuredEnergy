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
	private TextEdit text1;
	private TextEdit text2;
	private TextEdit text3;
	private TextEdit text4;
	private TextEdit text5;
	
	private Button button1;
	private Button button2;
	private Button button3;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		
		text1 	= GetNode<TextEdit>("TextEdit1");
		text2 	= GetNode<TextEdit>("TextEdit2");
		text3 	= GetNode<TextEdit>("TextEdit3");
		text4 	= GetNode<TextEdit>("TextEdit4");
		text5 	= GetNode<TextEdit>("TextEdit5");
		
		button1 = GetNode<Button>("Button1");
		button2 = GetNode<Button>("Button2");
		button3 = GetNode<Button>("Button3");
		
		button1.Pressed += NewGame;
		button2.Pressed += GetContext;
		button3.Pressed += UpdateParam;
	}
	
	private void NewGame() {

		string url = "https://sure.euler.usi.ch/res.php?mth=ins&xsl=0";
		
		XmlDocument xmldoc = new XmlDocument();
		xmldoc.Load(url); 

		XmlNode row = xmldoc.DocumentElement.FirstChild.FirstChild;

		text1.Text = row.Attributes["res_id"].Value;
		text2.Text = row.Attributes["yr"].Value;
		
		text5.Text = row.OuterXml;
	}
	

	private void GetContext() {

		string res_id 	= text1.Text;
		string yr 		= text2.Text;
	
		string url = $"https://sure.euler.usi.ch/res.php?mth=ctx&res_id={res_id}&yr={yr}";
		
		XmlDocument xmldoc = new XmlDocument();
		xmldoc.Load(url); 

		XmlNode row = xmldoc.DocumentElement.FirstChild.FirstChild;

		text5.Text = row.OuterXml;
	}
	
	
	private void UpdateParam() {

		string res_id 	= text1.Text;
		string yr 		= text2.Text;
		string prm_id 	= text3.Text;
		string tj 		= text4.Text;
	
		string url = $"https://sure.euler.usi.ch/prm.php?mth=ups&res_id={res_id}&yr={yr}&prm_id={prm_id}&tj={tj}&xsl=0";

		XmlDocument xmldoc = new XmlDocument();
		xmldoc.Load(url); 

		XmlNode row = xmldoc.DocumentElement.FirstChild.FirstChild;

		text5.Text = row.OuterXml;
	}

}

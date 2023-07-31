/**
	Sustainable Energy Development game modeling the Swiss energy Grid.
	Copyright (C) 2023 Università della Svizzera Italiana

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using Godot;
using System;
using System.Net.Http;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

// HTTP Client for the server-based energy grid model
// This model can be found at https://toby.euler.usi.ch
public partial class ModelController : Node {

	// Model URL Constants
	private const string MODEL_BASE_URL = "https://toby.euler.usi.ch";
	private const string RES_CREATE_METHOD = "res.php?mth=insert";
	private const string RES_UPDATE_METHOD = "res.php?mth=update";
	private const string RES_ID = "res_id";
	private const string RES_NAME = "res_name";
	private const string RES_V1 = "res_v1";
	private const string RES_V2 = "res_v2";

	// Reference to the game context
	private Context C;
	
	// HTTP Client: We are using godot's http client for easy interoperability with other nodes
	// For use of Godot http client
	private Godot.HttpRequest HTTPC;

	// For use of c# standard library httpclient
	private static readonly System.Net.Http.HttpClient _HTTPC = new System.Net.Http.HttpClient();

	// For random name generation
	private long NC = 0;

	// ModelController state
	// IDLE: Model is free to handle new requests
	// PENDING: Model is currently handling a request
	private enum ModelState { IDLE, PENDING };
	private ModelState State;


	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch the nodes
		C = GetNode<Context>("/root/Context");
		HTTPC = GetNode<HttpRequest>("HTTPRequest");

		// Initialize the model's state
		State = ModelState.IDLE;
	}    

	// ==================== Server Interaction Methods ====================

	// Initializes a connection with the model by creating a new game instance
	// Returns whether or not the request was filed
	public async void _InitModel() {
		// Check that the model is free
		if(State != ModelState.IDLE) {
			// TODO: Allow for backlogging of requests, this requires abstract modeling of requests and storing them in a list
			throw new Exception("Model is currently handling another request!");
		}
		
		// Update the Model's state
		State = ModelState.PENDING;

		// Create the POST request
		var Res = await _HTTPC.PostAsync(MODEL_BASE_URL + "/" + RES_CREATE_METHOD, null);

		// Make sure that the connection succeeded
		try {
			Res.EnsureSuccessStatusCode();
		} catch (HttpRequestException e) {
			// Log the error data from the request
			throw new Exception(
				"Unable to connect to model, status code = " + Res.StatusCode.ToString() + 
				" Error: " + e.Message.ToString()
			);
		}

		// Retrieve the response from the model
		var SRes = await Res.Content.ReadAsStringAsync();

		// Parse the resceived data to an XML tree
		XDocument XmlResp = XDocument.Parse(SRes);

		// Retrive the id from the response and store it in the context
		int Id = XmlResp.Root.Descendants("row").Select(r => r.Attribute(RES_ID).Value.ToInt()).ElementAt(0);
		C._UpdateGameID(Id);

		// DEBUG: Check that the id was set correctly
		Debug.Print("Game ID Updated to: " + C._GetGameID());

		// Update the Model's state
		State = ModelState.IDLE;

		// Update the name to something random
		_UpdateModelName(GenerateName());
	}

	// Updates the name of the current game instance in the model
    // Given a new name to use for the instance (usually randomly generated)
	public async void _UpdateModelName(string new_name) {
		// Check that the model is free
		if(State != ModelState.IDLE) {
			// TODO: Allow for backlogging of requests, this requires abstract modeling of requests and storing them in a list
			throw new Exception("Model is currently handling another request!");
		}
		
		// Update the Model's state
		State = ModelState.PENDING;

		// Create the data package
		var Data = new Dictionary<string, string> {
			{ RES_ID, C._GetGameID().ToString() },
			{ RES_NAME, new_name }
		};

		// Encode it in a content header
		var Payload = new FormUrlEncodedContent(Data);

		// Create the POST request
		var Res = await _HTTPC.PostAsync(MODEL_BASE_URL + "/" + RES_UPDATE_METHOD, Payload);

		// Make sure that the connection succeeded
		try {
			Res.EnsureSuccessStatusCode();
		} catch (HttpRequestException e) {
			// Log the error data from the request
			throw new Exception(
				"Unable to connect to model, status code = " + Res.StatusCode.ToString() + 
				" Error: " + e.Message.ToString()
			);
		}

		// Retrieve the response from the model
		var SRes = await Res.Content.ReadAsStringAsync();

		// Parse the resceived data to an XML tree
		XDocument XmlResp = XDocument.Parse(SRes);

		// Retrive the id from the response and store it in the context
		string name = XmlResp.Root.Descendants("row").Select(r => r.Attribute(RES_NAME).Value.ToString()).ElementAt(0);

		// DEBUG: Check that the id was set correctly
		Debug.Print("Game name updated to: " + name);

		// Update the Model's state
		State = ModelState.IDLE;
	}

	// ==================== Internal Helper Methods ====================

	// Generate a random name for the model
	private string GenerateName() {
		// Random Dictionnary of words
		string[] words = {"pocket", "club", "thing", "seat", "roll", "button", "size", "move", "year", "sticks", "trousers", "rule", "transport", "kitty", "north", "pump", "can", "bucket", "clam", "day", "dock", "wind", "pies", "room", "grass", "girls", "songs", "curve", "giraffe", "plane", "channel", "play", "art", "back", "amount", "instrument", "week", "change", "person", "lock", "class", "look", "sleet", "pear", "toe", "haircut", "underwear", "tongue", "experience", "dogs", "uncle", "birds", "spoon", "airport", "desk", "glass", "cherry", "trouble", "cakes", "rabbits", "soup", "team", "eggnog", "stocking", "icicle", "ring", "bath", "daughter", "company", "baseball", "brass", "car", "hot", "blow", "afternoon", "judge", "use", "selection", "cobweb", "temper", "jam", "mass", "snake", "agreement", "rhythm", "mind", "pie", "town", "spiders", "show", "partner", "fork", "queen", "bubble", "end", "language", "book", "marble", "writing", "match"};

		// Pick a word at random and concatenate an arbitrary number to it
		Random rnd = new Random();
		return words[rnd.Next(100) % 100] + "_" + (NC++).ToString();
	}

    // ==================== Server Interaction Methods (GODOT Client) ====================

    // Initializes a connection with the model by creating a new game instance
	// Returns whether or not the request was filed
	// This uses the godot httpclient which doesn't work well with the model's url
	public bool _InitModelGodot() {
		// Check that the model is free
		if(State != ModelState.IDLE) {
			// TODO: Allow for backlogging of requests, this requires abstract modeling of requests and storing them in a list
			return false;
		}

		// Connect the callback signal
		HTTPC.RequestCompleted += OnInitModelRequestCompleted;

		// Send the request to the model
		HTTPC.Request(
			MODEL_BASE_URL + "/" + RES_CREATE_METHOD,
			null,
			Godot.HttpClient.Method.Post
		);

		// Update the Model's state
		State = ModelState.PENDING;
		
		return true;
	}

	// ==================== HTTP Request callbacks (GODOT Client) ====================

	// Handles the response from an init model request
	private void OnInitModelRequestCompleted(long result, long responseCode, string[] headers, byte[] body) {
		// Check response code
		if(result == (long)HttpRequest.Result.Success) {
			// Extract xml from body response
			var XmlResp = XDocument.Parse(body.GetStringFromUtf8());

			// Retrive the id from the response and store it in the context
			int Id = XmlResp.Root.Descendants("row").Select(r => r.Attribute("res_id").Value.ToInt()).ElementAt(0);
			C._UpdateGameID(Id);		
		} else {
			throw new Exception("Unable to connect to model, response = " + responseCode + ", result = " + result);
		}
	}
}  

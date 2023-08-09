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
	private const string RES_FILE = "res.php";
    private const string BAL_FILE = "bal.php";
    private const string INSERT_METHOD = "mth=insert";
	private const string UPDATE_METHOD = "mth=update";
    private const string DISP_METHOD = "mth=disp";
	private const string RES_ID = "res_id";
	private const string RES_NAME = "res_name";
	private const string RES_V1 = "res_v1";
	private const string RES_V2 = "res_v2";
    private const string N = "n"; // The current week we are requesting
    private const string SEASON = "s"; // The season related to the data (s < 0.5 -> WINTER, s >= 0.5 -> SUMMER)
    private const string A_GAS = "avl_gas";
    private const string A_NUCLEAR = "avl_nuc";
    private const string A_RIVER = "avl_riv";
    private const string A_SOLAR = "avl_sol";
    private const string A_WIND = "avl_win";
    private const string C_GAS = "cap_ele_gas";
    private const string C_NUCLEAR = "cap_ele_nuc";
    private const string C_RIVER = "cap_ele_riv";
    private const string C_SOLAR = "cap_ele_sol";
    private const string C_WIND = "cap_ele_win";
    private const string D_BASE = "dem_base";

    // Game value constants
    private const int YEARS_PER_TURN = 3;
    private const int WEEKS_PER_YEAR = 52;

	// Reference to the game context
	private Context C;
	
	// HTTP Client: We are using godot's http client for easy interoperability with other nodes
	// For use of Godot http client
	private Godot.HttpRequest HTTPC;

	// For use of c# standard library httpclient
	private static readonly System.Net.Http.HttpClient _HTTPC = new System.Net.Http.HttpClient();

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

    // ==================== Public API (For General Checks) ====================

    // Checks whether or not the model is currently free
    // Returns true if the model can handle a new request, false otherwise
    public bool _CheckState() => State == ModelState.IDLE;

	// ==================== Server Interaction Methods ====================

	// Initializes a connection with the model by creating a new game instance
	// Returns whether or not the request was filed
    // This method will generate the following POST request :
    // - URL: https://toby.euler.usi.ch/res.php?mth=insert&res_id=Content.ResId 
    // - HEADER: Null
    // - DATA: Null
	public async void _InitModel() {
		// Check that the model is free
		if(State != ModelState.IDLE) {
			// TODO: Allow for backlogging of requests, this requires abstract modeling of requests and storing them in a list
			throw new Exception("Model is currently handling another request!");
		}
		
		// Update the Model's state
		State = ModelState.PENDING;

		// Create the POST request
		var Res = await _HTTPC.PostAsync(ModelURL(RES_FILE, INSERT_METHOD), null);

		// Make sure that the connection succeeded
		try {
            // Check the status code for success
			Res.EnsureSuccessStatusCode();
		} catch (HttpRequestException e) {
            // Reset the model state in case of a crash
            State = ModelState.IDLE;

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
		int Id = (
            from r in XmlResp.Root.Descendants("row")
            select r.Attribute(RES_ID).Value.ToInt()
        ).ElementAt(0);

        // Update the context
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
    // This method will generate the following POST request :
    // - URL: https://toby.euler.usi.ch/res.php?mth=update  
    // - HEADER: Content-Type: application/x-www-form-urlencoded
    // - DATA: res_id = Context.ResId & res_name = @param{new_name} &
    //         res_v1 = 42 & res_v2 = 9001 (arbitrary meme values)
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
			{ RES_NAME, new_name },
			{ RES_V1, 42.ToString() }, // Arbitrary values for now
			{ RES_V2, 9001.ToString() } // Arbitrary values for now
		};

		// Encode it in a content header
		var Payload = new FormUrlEncodedContent(Data);

		// Create the POST request
		var Res = await _HTTPC.PostAsync(ModelURL(RES_FILE, UPDATE_METHOD), Payload);

		// Make sure that the connection succeeded
		try {
			Res.EnsureSuccessStatusCode();
		} catch (HttpRequestException e) {

            // Reset the model state in case of a crash
            State = ModelState.IDLE;

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

		// Retrive the name from the response and store it in the context
		string name = (
            from r in XmlResp.Root.Descendants("row")
            where r.Attribute(RES_ID).Value.ToInt() == C._GetGameID()
            select r.Attribute(RES_NAME).Value.ToString()
        ).ElementAt(0);

        // Update the context
        C._UpdateGameName(name);

		// DEBUG: Check that the id was set correctly
		Debug.Assert(name == new_name);
		Debug.Print("Game name updated to: " + name);

		// Update the Model's state
		State = ModelState.IDLE;
	}

    // Retrieves all of the data from the model and stores it in the context
    // Warning: This will override all of the data in the context with the data 
    // in the model. A coherency protocol should be used to avoid any mistakes.
    // The given current turn is used to compute the timestep in the model
    // The given turn is in batches of 3 year so it will be converted to weeks for the model.
    // This method will generate the following GET request:
    // URL = https://toby.euler.usi.ch/bal.php
    // GET_PARAMS: ?mth=disp&res_id=Context.ResId&n=${@param{current_turn}*3*52}
    public async void _FetchModelData(int current_turn) {

        // Create the parameters
        string resid = GetParam(RES_ID, C._GetGameID());
        string n = GetParam(N, TurnToWeek(current_turn));

        // Make the request and make sure it works
        try {
            // Send GET request
            var Res = await _HTTPC.GetStringAsync(ModelURL(BAL_FILE, DISP_METHOD, resid, n));

            // Parse the received data to an XML tree
		    XDocument XmlResp = XDocument.Parse(Res);

            // Convert the given xml into a model struct
            Model new_M = ModelFromXML(XmlResp);

            // Update the internal state of the context
            C._UdpateModelFromServer(new_M);

        } catch(HttpRequestException e) {

            // Log the error data from the request
			throw new Exception(
				"Unable to connect to model, status code = " + e.StatusCode.ToString() + 
				" Error: " + e.Message.ToString()
			);
        } 
    }

	// ==================== Internal Helper Methods ====================

	// Generate a random name for the model
	private string GenerateName() {
		// Random Dictionnary of words
		string[] words = {"pocket", "club", "thing", "seat", "roll", "button", "size", "move", "year", "sticks", "trousers", "rule", "transport", "kitty", "north", "pump", "can", "bucket", "clam", "day", "dock", "wind", "pies", "room", "grass", "girls", "songs", "curve", "giraffe", "plane", "channel", "play", "art", "back", "amount", "instrument", "week", "change", "person", "lock", "class", "look", "sleet", "pear", "toe", "haircut", "underwear", "tongue", "experience", "dogs", "uncle", "birds", "spoon", "airport", "desk", "glass", "cherry", "trouble", "cakes", "rabbits", "soup", "team", "eggnog", "stocking", "icicle", "ring", "bath", "daughter", "company", "baseball", "brass", "car", "hot", "blow", "afternoon", "judge", "use", "selection", "cobweb", "temper", "jam", "mass", "snake", "agreement", "rhythm", "mind", "pie", "town", "spiders", "show", "partner", "fork", "queen", "bubble", "end", "language", "book", "marble", "writing", "match"};

		// Pick a word at random and concatenate an arbitrary number to it
		Random rnd = new Random();
		return words[rnd.Next(100) % 100] + "_" + (rnd.Next()).ToString();
	}

    // Groups the URL bits into a usable URL string
    private string ModelURL(string file, string method, params string[] getparams) {
        // Group the file and method into url
        string url = MODEL_BASE_URL + "/" + file + "?" + method;
        
        // Concatenate all get parameters
        foreach(string p in getparams) {
            url = url + "&" + p;
        }

        return url;
    }

    // Creates a valid string for individual get parametes
    private string GetParam(string name, string value) => name + "=" + value;
    private string GetParam<T>(string name, T value) => name + "=" + value.ToString();

    // Converts a turn number into a week number
    private int TurnToWeek(int turn) => turn * YEARS_PER_TURN * WEEKS_PER_YEAR;

    // Retrieves a float attribute from a specific xml row
    private float GetFloatAttr(IEnumerable<XElement> row, string attr) => 
        (from r in row select r.Attribute(attr).Value.ToFloat()).ElementAt(0);

    // Retrieves an int attribute from a specific xml row
    private int GetIntAttr(IEnumerable<XElement> row, string attr) => 
        (from r in row select r.Attribute(attr).Value.ToInt()).ElementAt(0);

    // Converts a model XML into a Model Struct
    // The given xml is expected to be the response from the bal.disp() server method
    private Model ModelFromXML(XDocument xml) {
        // Start by extracting the row
        IEnumerable<XElement> row = from r in xml.Root.Descendants("row")
                  where r.Attribute(RES_ID).Value.ToInt() == C._GetGameID()
                  select r;

        // Retrieve the season
        int s_val = (from s in row select s.Attribute(SEASON).Value.ToInt()).ElementAt(0);
        ModelSeason MS = s_val < 0.5f ? ModelSeason.WINTER : ModelSeason.SUMMER;

        // Retrieve the subgroups of the model and return it
        return new Model(
            AvailabilityFromRow(row),
            CapacityFromRow(row),
            DemandFromRow(row),
            ModelCoherencyState.SHARED,
            MS
        );
    }

    // Extracts the availability columns from a given row query
    private Availability AvailabilityFromRow(IEnumerable<XElement> row) => new Availability(
        GetFloatAttr(row, A_GAS), 
        GetFloatAttr(row, A_NUCLEAR), 
        GetFloatAttr(row, A_RIVER), 
        GetFloatAttr(row, A_SOLAR), 
        GetFloatAttr(row, A_WIND)
    );

    // Extracts the availability columns from a given row query
    private Capacity CapacityFromRow(IEnumerable<XElement> row) => new Capacity(
        GetIntAttr(row, C_GAS), 
        GetIntAttr(row, C_NUCLEAR), 
        GetIntAttr(row, C_RIVER), 
        GetIntAttr(row, C_SOLAR), 
        GetIntAttr(row, C_WIND)
    );

    // Extracts the demand columns from a given row query
    private Demand DemandFromRow(IEnumerable<XElement> row) => 
        new Demand(GetIntAttr(row, D_BASE));

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
            ModelURL(RES_FILE, INSERT_METHOD),
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

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
using System.Threading.Tasks;


// HTTP Client for the server-based energy grid model
// This model can be found at https://toby.euler.usi.ch
public partial class ModelController : Node {

	// Model URL Constants
	private const string MODEL_BASE_URL = "https://toby.euler.usi.ch";
	private const string RES_FILE = "res.php";
	private const string BAL_FILE = "bal.php";
	private const string EVT_FILE = "evt.php";
	private const string INSERT_METHOD = "mth=insert";
	private const string UPDATE_METHOD = "mth=update";
	private const string DISP_METHOD = "mth=disp";
	private const string UPSERT_METHOD = "mth=upsert";
	private const string RES_ID = "res_id";
	private const string COL_ID = "col_id";
	private const string RES_NAME = "res_name";
	private const string RES_V1 = "res_v1";
	private const string RES_V2 = "res_v2";
	private const string EVT_V1 = "v1"; // The value that is set for the col event
	private const string N = "n"; // The current week we are requesting
	private const string SEASON = "s"; // The season related to the data (s < 0.5 -> WINTER, s >= 0.5 -> SUMMER)
	private const string A_GAS = "avl_gas";
	private const string A_NUCLEAR = "avl_nuc";
	private const string A_RIVER = "avl_riv";
	private const string A_SOLAR = "avl_sol";
	private const string A_WIND = "avl_wnd";
	private const string C_GAS = "cap_ele_gas";
	private const string C_NUCLEAR = "cap_ele_nuc";
	private const string C_RIVER = "cap_ele_riv";
	private const string C_SOLAR = "cap_ele_sol";
	private const string C_WIND = "cap_ele_wnd";
	private const string D_BASE = "dem_base";
	private const string COL_CAP_GAS = "30";
	private const string COL_CAP_NUC = "31";
	private const string COL_CAP_RIV = "32";
	private const string COL_CAP_SOL = "35";
	private const string COL_CAP_WND = "36";

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

	// ModelController Queue
	// This is used to store pending requests from the model
	private enum RequestType { INIT, FETCH, FETCH_ASYNC, NAME, EVENT };

	// The request queue contains the following information:
	// 1) The type of request being queued  
	// 2) Any potential parameters needed for said request
	private Queue<(RequestType, List<string>)> RequestQ;


	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch the nodes
		C = GetNode<Context>("/root/Context");
		HTTPC = GetNode<HttpRequest>("HTTPRequest");

		// Initialize the model's state
		State = ModelState.IDLE;

		// Initialize the request queue
		RequestQ = new Queue<(RequestType, List<string>)>();
	}

	// Called on every frame
	// In our case, used to handle outstanding model requests
	public override void _Process(double delta) {
		// Call the base process function
		base._Process(delta);  

		// Check the model state
		if(State == ModelState.IDLE) {
			// Check for outstanding requests
			if(RequestQ.Count > 0) {
				// Handle the oldest request first
				(RequestType RT, List<string> par) = RequestQ.Dequeue();

				// Find which function is associated to the given request
				switch(RT) {
					// Call the initModel method
					case RequestType.INIT:
						_InitModel();
						break;
					
					// Call the FetchModelData Method
					case RequestType.FETCH_ASYNC:
						_FetchModelDataAsync();
						break;

					// Synchronous fetch case
					case RequestType.FETCH:
						_FetchModelData();
						break;

					// Call the UpdateModelName Method
					case RequestType.NAME:
						// Check that a new name was given
						string name = par.Count == 0 ? GenerateName() : par.ElementAt(0);

						// Rerun the update request
						_UpdateModelName(name);
						break;
						
					// Call the UpsertModelColumnData Method  
					case RequestType.EVENT:
						// Sanity check
						if(par.Count == 0) {
							throw new ArgumentException("No parameters were stored for an event request!");
						}

						// Decode the parameters using a forced implicit conversion
						ModelCol mc = par[0];
						Building b = par[1];

						// Rerun the upsert request
						_UpsertModelColumnDataAsync(mc, b);
						break;
					default: 
						return;
				}
			}
		}
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
	public void _InitModel() {
		// Check that the model is free
		if(State != ModelState.IDLE) {
			// Enqueue our request and come back later
			RequestQ.Enqueue((RequestType.INIT, new ()));
			return;
		}
		
		// Update the Model's state
		State = ModelState.PENDING;

		// Create the POST request
		Task<HttpResponseMessage> Request = _HTTPC.PostAsync(ModelURL(RES_FILE, INSERT_METHOD), null);

		// Make sure that the connection succeeded
		try {
			// Wait on the request's success (initialization must be done synchronously)
			Request.Wait();
			HttpResponseMessage Res = Request.Result;

			// Request the response from the model
			Task<string> SReq = Res.Content.ReadAsStringAsync();

			// Wait for the request to complete
			SReq.Wait();
			
			// Get the Serialized result
			string SRes = SReq.Result; 

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
		} catch (HttpRequestException e) {
			// Reset the model state in case of a crash
			State = ModelState.IDLE;

			// Log the error data from the request
			throw new Exception(
				"Unable to connect to model, status code = " + Request.Status + 
				" Error: " + e.Message.ToString()
			);
		}
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
			// Enqueue our request and try again back later
			RequestQ.Enqueue((
				RequestType.NAME, 
				new () { new_name }
			));
			return;
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
		FormUrlEncodedContent Payload = new (Data);

		// Create the POST request
		HttpResponseMessage Res = await _HTTPC.PostAsync(ModelURL(RES_FILE, UPDATE_METHOD), Payload);

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
		string SRes = await Res.Content.ReadAsStringAsync();

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

	// Retrieves all of the data from the model and stores it in the context in a synchronous manner
	// Warning: This will override all of the data in the context with the data 
	// in the model. A coherency protocol should be used to avoid any mistakes.
	// The given current turn is used to compute the timestep in the model
	// The given turn is in batches of 3 year so it will be converted to weeks for the model.
	// This method will generate the following two GET requests:
	// URL = https://toby.euler.usi.ch/bal.php
	// GET_PARAMS_1: ?mth=disp&res_id=Context.ResId&n=${@param{current_turn}*3*52}
	// GET_PARAMS_2: ?mth=disp&res_id=Context.ResId&n=${@param{current_turn}*3*52+26}
	public void _FetchModelData() {

		// Check model availability
		if(State != ModelState.IDLE) {
			// Enqueue the request in case that the model is busy
			RequestQ.Enqueue((RequestType.FETCH, new ()));
			return;
		}

		// Claim the model
		State = ModelState.PENDING;

		// Create the parameters
		string resid = GetParam(RES_ID, C._GetGameID());
		string nWinter = GetParam(N, TurnToPeakWinter(C._GetTurn()));
		string nSummer = GetParam(N, TurnToPeakSummer(C._GetTurn()));

		try {
			// Send GET requests for both peak winter and peak summer
			Task<string> wres = _HTTPC.GetStringAsync(ModelURL(BAL_FILE, DISP_METHOD, resid, nWinter));
			Task<string> sres = _HTTPC.GetStringAsync(ModelURL(BAL_FILE, DISP_METHOD, resid, nSummer));

			// Wait for the response
			Task.WaitAll(wres, sres);

			// Sanity check
			if(!(wres.IsCompletedSuccessfully && sres.IsCompletedSuccessfully)) {
				throw new Exception(
					"Model data requests failed with error: " + wres.Status.ToString() + ", and " + sres.Status.ToString()
				);
			}

			// Check that the results aren't empty
			Debug.Print(wres.Result + "\n" + sres.Result);

			// Parse the received data to an XML tree
			XDocument XmlRespW = XDocument.Parse(wres.Result);
			XDocument XmlRespS = XDocument.Parse(sres.Result);

			// Convert the given xml into a model struct
			Model new_MW = ModelFromXML(XmlRespW);
			Model new_MS = ModelFromXML(XmlRespS);

			// Update the internal state of the context
			C._UdpateModelFromServer(new_MW);
			C._UdpateModelFromServer(new_MS);

			// Reset the model state in case of a crash
			State = ModelState.IDLE;

		} catch(HttpRequestException e) {
			// Reset the model state in case of a crash
			State = ModelState.IDLE;

			// Log the error data from the request
			throw new Exception(
				"Unable to connect to model, status code = " + e.StatusCode.ToString() + 
				" Error: " + e.Message.ToString()
			);
		}
	}

	// Retrieves all of the data from the model and stores it in the context in an asynchronous manner
	// Warning: This will override all of the data in the context with the data 
	// in the model. A coherency protocol should be used to avoid any mistakes.
	// The given current turn is used to compute the timestep in the model
	// The given turn is in batches of 3 year so it will be converted to weeks for the model.
	// This method will generate the following two GET requests:
	// URL = https://toby.euler.usi.ch/bal.php
	// GET_PARAMS_1: ?mth=disp&res_id=Context.ResId&n=${@param{current_turn}*3*52}
	// GET_PARAMS_2: ?mth=disp&res_id=Context.ResId&n=${@param{current_turn}*3*52+26}
	public async void _FetchModelDataAsync() {

		// Check model availability
		if(State != ModelState.IDLE) {
			// Enqueue the request in case that the model is busy
			RequestQ.Enqueue((RequestType.FETCH_ASYNC, new ()));
			return;
		}

		// Claim the model
		State = ModelState.PENDING;

		// Create the parameters
		string resid = GetParam(RES_ID, C._GetGameID());
		string nWinter = GetParam(N, TurnToPeakWinter(C._GetTurn()));
		string nSummer = GetParam(N, TurnToPeakSummer(C._GetTurn()));

		try {
			// Signal that the request was sent
			Debug.Print("Fetch request sent");  

			// Send GET requests for both peak winter and peak summer
			string wres = await _HTTPC.GetStringAsync(ModelURL(BAL_FILE, DISP_METHOD, resid, nWinter));
			string sres = await _HTTPC.GetStringAsync(ModelURL(BAL_FILE, DISP_METHOD, resid, nSummer));

			// Check that the results aren't empty
			Debug.Print(wres + "\n" + sres);

			// Parse the received data to an XML tree
			XDocument XmlRespW = XDocument.Parse(wres);
			XDocument XmlRespS = XDocument.Parse(sres);

			// Convert the given xml into a model struct
			Model new_MW = ModelFromXML(XmlRespW);
			Model new_MS = ModelFromXML(XmlRespS);

			// Update the internal state of the context
			C._UdpateModelFromServer(new_MW);
			C._UdpateModelFromServer(new_MS);

			// Reset the model state in case of a crash
			State = ModelState.IDLE;

		} catch(HttpRequestException e) {
			// Reset the model state in case of a crash
			State = ModelState.IDLE;

			// Log the error data from the request
			throw new Exception(
				"Unable to connect to model, status code = " + e.StatusCode.ToString() + 
				" Error: " + e.Message.ToString()
			);
		}
	}

	// Updates the model columns associated to the given column type and powerplant type
	// This method checks the state of the model's coherency before making an update
	// If the model is already shared, then no request is generated.
	// A request is only generated if the model is in the Modified state.
	// The generated GET requests are as follows:  
	// URL = URL = https://toby.euler.usi.ch/evt.php
	// GET_PARAMS_1 = ?mth=upsert&
	//					res_id=Context.ResId&
	//					col_id=${@param{col_type}~@param{PPType}}&
	//					n=${{C.Turn}*3*52}&
	//					v1=${C.MWinter.@param{col_type}.@param{PPType}}
	public async void _UpsertModelColumnDataAsync(ModelCol mc, Building b) {
		// Sanity check: Trees aren't recorded in the model
		if(b.type == Building.Type.TREE) {
			// Ignore trees
			return;
		}

		// Check model availability
		if(State != ModelState.IDLE) {
			// Enqueue request and try again later
			RequestQ.Enqueue((
				RequestType.EVENT, 
				new () { mc.ToString(), b.ToString() } // Encode request parameters for easier storage
			));
			return;
		}

		// Claim the model
		State = ModelState.PENDING;

		// Create the get parameters
		string res_id = GetParam(RES_ID, C._GetGameID());
		string col_id = GetParam(COL_ID, ColIdFromTypes(mc, b));
		string n = GetParam(N, TurnToPeakWinter(C._GetTurn()));
		string v1 = GetParam(EVT_V1, GetModelValue(mc, b).ToString());

		try {
			// Signal that the request was sent
			Debug.Print("Event upsert request sent");  

			// Send the GET request
			string res = await _HTTPC.GetStringAsync(ModelURL(
				EVT_FILE, 
				UPSERT_METHOD, 
				res_id, 
				col_id,
				n,
				v1
			));

			// Check that the results aren't empty
			Debug.Print("EVENT RES: " + res);

			// Parse the received data to an XML tree
			XDocument XmlResp = XDocument.Parse(res);

			// Convert the given xml into a model struct
			Model new_M = ModelFromXML(XmlResp);

			// Update the internal state of the context
			C._UdpateModelFromServer(new_M);

			// Reset the model state in case of a crash
			State = ModelState.IDLE;

		} catch(HttpRequestException e) {
			// Reset the model state in case of a crash
			State = ModelState.IDLE;

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
		return words[rnd.Next(100) % 100] + "_" + rnd.Next().ToString();
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

	// Converts a turn number into a week number: either peak winter or peak summer
	private int TurnToPeakWinter(int turn) => turn * YEARS_PER_TURN * WEEKS_PER_YEAR;
	private int TurnToPeakSummer(int turn) => TurnToPeakWinter(turn) + (int)(WEEKS_PER_YEAR * 0.5);

	// Retrieves a float attribute from a specific xml row
	private float GetFloatAttr(IEnumerable<XElement> row, string attr) {
		// Attempt the query
		try {
	   		return (from r in row select r.Attribute(attr).Value.ToFloat()).ElementAt(0);

		} catch (NullReferenceException e) {
			// In the case where the recieved data didn't match the query, simply print it to find out what's happening
			Debug.Print("REIVED ROW: " + row.ToArray().ToString() + " for attribute: " + attr);
			throw e;
		}
	} 

	// Retrieves an int attribute from a specific xml row
	private int GetIntAttr(IEnumerable<XElement> row, string attr) => 
		(from r in row select r.Attribute(attr).Value.ToInt()).ElementAt(0);

	// Retrives the value associated to the given model column and building type
	private float GetModelValue(ModelCol mc, Building b) => 
		C._GetModel(ModelSeason.WINTER)._GetColumn(mc)._GetField(b);

	// Converts a model XML into a Model Struct
	// The given xml is expected to be the response from the bal.disp() server method
	private Model ModelFromXML(XDocument xml) {
		// Start by extracting the row
		IEnumerable<XElement> row = from r in xml.Root.Descendants("row")
				  where r.Attribute(RES_ID).Value.ToInt() == C._GetGameID()
				  select r;

		// Retrieve the season
		float s_val = (from s in row select s.Attribute(SEASON).Value.ToFloat()).ElementAt(0);
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

	// Converts a given pair of model type and power plant type into its associated column id
	private string ColIdFromTypes(ModelCol mc, Building b) => (mc + b).ToString();

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
		GetFloatAttr(row, C_GAS), 
		GetFloatAttr(row, C_NUCLEAR), 
		GetFloatAttr(row, C_RIVER), 
		GetFloatAttr(row, C_SOLAR), 
		GetFloatAttr(row, C_WIND)
	);

	// Extracts the demand columns from a given row query
	private Demand DemandFromRow(IEnumerable<XElement> row) => 
		new Demand(GetFloatAttr(row, D_BASE));

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

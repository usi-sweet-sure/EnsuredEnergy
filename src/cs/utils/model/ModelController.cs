using Godot;
using System;
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

    // Reference to the game context
    private Context C;
    
    // HTTP Client: We are using godot's http client for easy interoperability with other nodes
    private HttpRequest HTTPC;

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
    public bool _InitModel() {
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
            HttpClient.Method.Post
        );

        // Update the Model's state
        State = ModelState.PENDING;
        
        return true;
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

    // ==================== HTTP Request callbacks ====================

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

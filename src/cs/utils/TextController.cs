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
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

// Models a language
public readonly struct Language {
	// Represents the different types of languages
	public enum Type { EN, FR, DE, IT };

	// Internal storage of a language
	public readonly Type lang;

	// Basic constructor for the language
	public Language(Type l)  {
		lang = l;
	}

	// Override equality and inequality operators
	public static bool operator ==(Language l, Language other) => l.lang == other.lang;
	public static bool operator !=(Language l, Language other) => l.lang != other.lang;

	// Override the incrementation and decrementation operators
	public static Language operator ++(Language l) => new Language((Type)((int)(l.lang + 1) % (int)(Type.IT + 1)));
	public static Language operator --(Language l) => new Language((Type)((int)(l.lang - 1) % (int)(Type.IT + 1)));

	// Implicit conversion from the enum to the struct
	public static implicit operator Language(Type lt) => new Language(lt);

	// Implicit conversion from the struct to the enum
	public static implicit operator Type(Language l) => l.lang;

	// Implicit conversion from a string to a language
	public static implicit operator Language(string s) {
		// Make it as easy to parse as possible
		string s_ = s.ToLower().StripEdges();
		if(s == "en" || s == "english") {
			return new Language(Type.EN);
		} 
		if (s == "fr" || s == "french" || s == "français") {
			return new Language(Type.FR);
		} 
		if(s == "de" || s == "german" || s == "deutsch") {
			return new Language(Type.DE);
		} 
		return new Language(Type.IT);
	}
	
	// Implicit conversion to a string
	public override string ToString() => lang == Type.EN ? "en" : 
										 lang == Type.FR ? "fr" :
										 lang == Type.DE ? "de" :
										 "it";

	public string ToName() => lang == Type.EN ? "Language: English" : 
							  lang == Type.FR ? "Langue: Français" :
							  lang == Type.DE ? "Sprache: Deutsch" :
							  "Lingua: Italiano";

	// Performs the same check as the == operator, but with a run-time check on the type
	public override bool Equals(object obj) {
		// Check for null and compare run-time types.
		if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
			return false;
		}
		// Perform actual equality check
		return lang == ((Language)obj).lang;
	}

	// Override of the get hashcode method (needed to overload == and !=)
	public override int GetHashCode() => HashCode.Combine(lang);
}

// Utility class used to access XML db files.
// This is usually done to display text in a way that is linguistically dynamic .
public partial class TextController : Node {

	// The path to the base of the db 
	private string DB_PATH = Path.Combine("db/");

	// The currently loaded xml document
	private XDocument LoadedXML;
	private string LoadedFileName;
	private Language LoadedLanguage;

	// The current language
	private Language Lang = Language.Type.EN;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	// ==================== Internal Helpers ====================

	// Parses a given xml file and stores in in a target XDocument object
	private void ParseXML(ref XDocument targetXML, string filename) {
		if(filename == null) {
			throw new Exception("No xml file was input for the scene!");
		}
		
		//Load XML file into a XDocument for querying
		string loadedXML;
		XDocument xml;
		string path = DB_PATH + Lang.ToString() + "/" + filename;
		try { 
			loadedXML = File.ReadAllText(path);
			xml = XDocument.Parse(loadedXML);
		} catch(Exception) {
			// Control what error is displayed for better debugging
			throw new Exception("File not found: " + path);
		}
		
		//Sanity check
		if(xml != null) {
			targetXML = xml;
		} else {
			throw new Exception("Unable to load xml file: " + Lang.ToString() + "/" + filename);
		}
	}

	// ==================== Public API ====================
	
	// Updates the language the textcontroller is set to  
	public void _UpdateLanguage(Language l) {
		// Check that the given language is new
		if(l != Lang) {
			Lang = l;
			
			// Update the loaded xml
			ParseXML(ref LoadedXML, LoadedFileName);
		}
		// Don't do anything if the languages are the same
	}

	// Increments the language
	public void _NextLanguage() {
		Lang = ++Lang;
	}

	// Retrieve the language name
	public string _GetLanguageName() => Lang.ToName();

	// Queries the given xml file to retrieve the wanted text
	public string _GetText(string filename, string groupid, string id) {
		// Start by checking if the file is loaded in or not
		if(LoadedFileName != filename || LoadedLanguage != Lang) {
			ParseXML(ref LoadedXML, filename);
			LoadedFileName = filename;
			LoadedLanguage = Lang;
		}

		// Query the file
		var query = from g in LoadedXML.Root.Descendants("group")
					where g.Attribute("id").Value == groupid // Find the correct group
					select (
						from t in g.Descendants("text")
						where t.Attribute("id").Value == id // Find the correct text in the group
						select t.Value
					);

		// Extract query result
		foreach(var g in query) {
			foreach(var t in g) {
				return t;
			}
		}

		// If we reach this point in the method, then we failed somewhere
		throw new Exception("No valid string matches the given query!!");
	}
}

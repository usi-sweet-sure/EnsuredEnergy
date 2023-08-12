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

// ==================== Model Datatypes ===================
// This file contains all of the datastructures pertaining 
// to the internal representation of the data retrieved 
// from the remote energy grid model
// ========================================================

// Coherency state of the Model
// Follows a simple MSI model
// MODIFIED = client data is more recent than server data
// SHARED = server data is up to date with client
// INVALID = client data is useless
// This state is only valid for fields that the player can modify, i.e. capacity
public enum ModelCoherencyState { MODIFIED, SHARED, INVALID };

// Models which "season" the model data belongs to
// This allows us to group the data into WINTER and SUMMER
public enum ModelSeason { WINTER, SUMMER };

// Represents the type of data that can be updated in the remote model
public readonly struct ModelCol {
    private const int MODEL_COL_CAP_ELE_BASE_ID = 29;
    private const int MODEL_COL_AVL_BASE_ID = 1;
    private const int MODEL_COL_DEM_BASE_ID = 231;
    // Internal enum for the struct
    public enum Type { CAP, AVL, DEM, NONE };

    // Actual value of the type
    public readonly Type type;

    // Base constructor
    public ModelCol(Type t) {
        type = t;
    }

    // Override equality and inequality operators
	public static bool operator ==(ModelCol mc, ModelCol other) => mc.type == other.type;
	public static bool operator !=(ModelCol mc, ModelCol other) => mc.type != other.type;

	// Override the incrementation and decrementation operators
	public static ModelCol operator ++(ModelCol mc) => new ((Type)((int)(mc.type + 1) % (int)Type.NONE));
	public static ModelCol operator --(ModelCol mc) => new ((Type)((int)(mc.type - 1) % (int)Type.NONE));

	// Implicit conversion from the enum to the struct
	public static implicit operator ModelCol(Type mct) => new (mct);

	// Implicit conversion from the struct to the enum
	public static implicit operator Type(ModelCol mc) => mc.type;

    // Implicit converion to an int 
    public static implicit operator int(ModelCol mc) => mc.ToInt();

    // Addition with a building
    public static int operator +(ModelCol mc, Building b) => mc.ToInt() + b.ToInt();

    // Implicit conversion from an int
    public static implicit operator ModelCol(int i) =>
        i == MODEL_COL_CAP_ELE_BASE_ID ? new (Type.CAP) :
        i == MODEL_COL_AVL_BASE_ID ? new (Type.AVL) :
        i == MODEL_COL_DEM_BASE_ID ? new (Type.DEM) :
        new (Type.NONE);

	// Implicit conversion from a string to a typeuage
	public static implicit operator ModelCol(string s) {
		// Make it as easy to parse as possible
		string s_ = s.ToLower().StripEdges();
		if(s == "cap_ele" || s == "cap" || s == "capacity") {
			return new (Type.CAP);
		} 
		if (s == "avl" || s == "availability") {
			return new (Type.AVL);
		} 
		if(s == "demand" || s == "dem" || s == "dem_base") {
			return new (Type.DEM);
		} 
		return new (Type.NONE);
	}

    // Implicit string conversion
    public override string ToString() => 
        type == Type.CAP ? "cap_ele" :
        type == Type.AVL ? "avl" : 
        type == Type.DEM ? "dem_base" :
        "none";

    // Explicit converion to an int 
    public int ToInt() => 
        type == Type.CAP ? MODEL_COL_CAP_ELE_BASE_ID :
        type == Type.AVL ? MODEL_COL_AVL_BASE_ID :
        type == Type.DEM ? MODEL_COL_DEM_BASE_ID :
        0;

    // Performs the same check as the == operator, but with a run-time check on the type
    public override bool Equals(object obj) {
		// Check for null and compare run-time types.
		if ((obj == null) || ! GetType().Equals(obj.GetType())) {
			return false;
		}
		// Perform actual equality check
		return type == ((ModelCol)obj).type;
	}

	// Override of the get hashcode method (needed to overload == and !=)
	public override int GetHashCode() => HashCode.Combine(type);
}

// Internal datastructure containing the fields that are obtained through Toby's Model
public struct Model {
    // Different categories of data retrieved from the model
    public Availability _Availability;
    public Capacity _Capacity;
    public Demand _Demand;

    // Current coherency state of the model
    public ModelCoherencyState _MCS;

    // The season the data is correlated to
    public ModelSeason _Season;

    // Constructor using only the season
    public Model(ModelSeason Season) {
        // Initialize the internal fields
        _Availability = new Availability();
        _Capacity = new Capacity();
        _Demand = new Demand();

        // The original state will always be invalid
        _MCS = ModelCoherencyState.INVALID;

        // Set the season to the given season
        _Season = Season;
    }

    // Base Constructor for the model
    public Model(
        Availability A = new Availability(), 
        Capacity C = new Capacity(), 
        Demand D = new Demand(),
        ModelCoherencyState MCS = ModelCoherencyState.INVALID,
        ModelSeason Season = ModelSeason.WINTER 
    ) {
        // Set the fields to new fields
        _Availability = A;
        _Capacity = C;
        _Demand = D;

        // Set the model state
        _MCS = MCS;

        // Set the seasonality of the data
        _Season = Season;
    }

    // Retrieve the internal field groups given a columnt type
    public IColumn _GetColumn(ModelCol mc) {
        // Find which column was requested and return it
        switch(mc.type) {
            case ModelCol.Type.CAP:
                return _Capacity;

            case ModelCol.Type.AVL:
                return _Availability;

            case ModelCol.Type.DEM:
                return _Demand;

            // If NONE is reached then throw an error
            default:
                throw new ArgumentException("No valid column type was given!!");
        }
    }

    // Checks the validity of the model data
    public bool _IsValid() => _MCS != ModelCoherencyState.INVALID;

    // Updates the internal fields of a given model
    public void _UpdateFields(Availability A, Capacity C, Demand D, ModelCoherencyState MCS) {
        // Update the model's fields
        _Availability = A;
        _Capacity = C;
        _Demand = D;

        // Update the model's coherency state to shared as the data is from the server
        _MCS = MCS;
    }
}

// Used for a generic getter in the model struct
public interface IColumn {

    // Retrieves the field associated to the given building type
    public abstract string _GetField(Building b);
}

// Represents the data retrieved from the availability columns of the model
public struct Availability: IColumn {
    // Current availability of various type-based energies
    public float Gas; // Refers to avl_gas
    public float Nuclear; // Refers to avl_nuc
    public float River; // Refers to avl_riv (Hydro-electic)
    public float Solar; // Refers to avl_sol
    public float Wind; // Refers to avl_win

    //TODO: Add entries for avl_pet, avl_res, avl_pmp, avl_bio, avl_wst, and avl_geo

    // Basic constructor for the Availability struct
    public Availability(float g=0.0f, float n=0.0f, float r=0.0f, float s=0.0f, float w=0.0f) {
        Gas = g;
        Nuclear = n;
        River = r;
        Solar = s;
        Wind = w;
    }

    // Retrieves the field associated to the given building type
    public string _GetField(Building b) {
        // Find the associated field and return it as a string
        switch(b.type) {
            case Building.Type.HYDRO:
                return River.ToString();

            case Building.Type.GAS:
                return Gas.ToString();

            case Building.Type.SOLAR:
                return Solar.ToString();

            case Building.Type.NUCLEAR:
                return Nuclear.ToString();

            default:
                throw new ArgumentException("No field is associated to the given type!");
        }
    }

    // Checks whether the internal values are all 0 or not
    public bool _IsEmpty() => 
        Gas == Nuclear && Nuclear == River && River == Solar && Solar == Wind && Wind == 0.0f;
}

// Represents the data retrieved from the Capacity columns of the model
public struct Capacity: IColumn {
    // Current availability of various type-based energies
    public int Gas; // Refers to cap_ele_gas
    public int Nuclear; // Refers to cap_ele_nuc
    public int River; // Refers to cap_ele_riv (Hydro-electic)
    public int Solar; // Refers to cap_ele_sol
    public int Wind; // Refers to cap_ele_win

    //TODO: Add entries for cap_pet, cap_res, cap_pmp, cap_bio, cap_wst, and cap_geo

    // Basic constructor for the Capacity struct
    public Capacity(int g=0, int n=0, int r=0, int s=0, int w=0) {
        Gas = g;
        Nuclear = n;
        River = r;
        Solar = s;
        Wind = w;
    }

    // Retrieves the field associated to the given building type
    public string _GetField(Building b) {
        // Find the associated field and return it as a string
        switch(b.type) {
            case Building.Type.HYDRO:
                return River.ToString();

            case Building.Type.GAS:
                return Gas.ToString();

            case Building.Type.SOLAR:
                return Solar.ToString();

            case Building.Type.NUCLEAR:
                return Nuclear.ToString();

            default:
                throw new ArgumentException("No field is associated to the given type!");
        }
    }

    // Checks whether the internal values are all 0 or not
    public bool _IsEmpty() => 
        Gas == Nuclear && Nuclear == River && River == Solar && Solar == Wind && Wind == 0.0f;
}

// Represents the data retrived from the Demand columns of the model
public struct Demand : IColumn {
    // Current demand for certain types of resources
    public int Base; // Base energy demand

    //TODO: Add entries for dem_cool, dem_hind, dem_hres, dem_road, dem_rail

    // Basic constructor for the Demand struct
    public Demand(int b=0) {
        Base = 0;
    }

    // Retrieves the field associated to the given building type
    // In this case it will always return the base
    public string _GetField(Building b) => Base.ToString();

    // Checks whether the internal values are all 0 or not
    public bool _IsEmpty() => Base == 0;
} 

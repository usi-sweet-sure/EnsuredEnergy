using Godot;
using System;

// ==================== Config Enum ====================

// Models a config type
public readonly struct Config {
	// Represents the different types of configs
	public enum Type { POWER_PLANT, NONE };

	// Internal storage of a config
	public readonly Type type;

	// Basic constructor for the Config type
	public Config(Type ct)  {
		type = ct;
	}

	// Override equality and inequality operators
	public static bool operator ==(Config l, Config other) => l.type == other.type;
	public static bool operator !=(Config l, Config other) => l.type != other.type;

	// Override the incrementation and decrementation operators
	public static Config operator ++(Config l) => new Config((Type)((int)(l.type + 1) % (int)(Type.NONE + 1)));
	public static Config operator --(Config l) => new Config((Type)((int)(l.type - 1) % (int)(Type.NONE + 1)));

	// Implicit conversion from the enum to the struct
	public static implicit operator Config(Type lt) => new Config(lt);

	// Implicit conversion from the struct to the enum
	public static implicit operator Type(Config l) => l.type;

	// Implicit conversion from a string to a config type
	public static implicit operator Config(string s) {
		// Make it as easy to parse as possible
		string s_ = s.ToLower().StripEdges();
		if(s == "powerplants") {
			return new Config(Type.POWER_PLANT);
		} 
		return new Config(Type.NONE);
	}
	
	// Implicit conversion to a string
	public override string ToString() => type == Type.POWER_PLANT ? "powerplants.xml" : "";

	// Performs the same check as the == operator, but with a run-time check on the type
	public override bool Equals(object obj) {
		// Check for null and compare run-time types.
		if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
			return false;
		}
		// Perform actual equality check
		return type == ((Config)obj).type;
	}

	// Override of the get hashcode method (needed to overload == and !=)
	public override int GetHashCode() => HashCode.Combine(type);
}

// ==================== Language Enum ====================

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

// ==================== CONFIG DATATYPES ====================

// Abstract notion of config data
public interface ConfigData {
    // Copies the data into a powerplant
    public abstract void _CopyTo(ref PowerPlant nd);
}

// Represents the data retrieved from a powerplant config file
public readonly struct PowerPlantConfigData : ConfigData {
    
    // Metadata fields
    public readonly int BuildCost;
    public readonly int BuildTime;
    public readonly int LifeCycle;

    // Energy fields
    public readonly int ProductionCost;
    public readonly int Capacity;
    public readonly float Availability;

    // Environment fields
    public readonly int Pollution;
    public readonly float LandUse;
    public readonly float Biodiversity;

    // Basic constructor for the datatype
    public PowerPlantConfigData(
        int bc=0, int bt=0, int lc=0,
        int pc=0, int cap=0, float av=0,
        int pol=0, float lu=0, float bd=0
    ) {
        // Simply fill in the fields
        BuildCost = bc;
        BuildTime = bt;
        LifeCycle = lc;
        ProductionCost = pc;
        Capacity = cap;
        Availability = Math.Max(0.0f, Math.Min(av, 1.0f));
        Pollution = pol;
        LandUse = lu;
        Biodiversity = bd;
    }

    // Copies the datatype fields into a PowerPlant Object
    public void _CopyTo(ref PowerPlant PP) {
        // Sanity check 
        if(PP == null) {
            throw new ArgumentException("Invalid PowerPlant was given!");
        }

        // Copy in the public fields
        PP.BuildCost = BuildCost;
        PP.BuildTime = BuildTime;
        PP.LifeCycle = LifeCycle;
        PP.LandUse = LandUse;
        PP.BiodiversityImpact = Biodiversity;

        // Copy in the private fields
        PP._UdpatePowerPlantFields(
            true, 
            Pollution,
            ProductionCost,
            Capacity,
            Availability
        );
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using Godot;

namespace Localization;

public class AbilityStrings {
    public string NAME {get; set;}
    public Dictionary<string, string> STRINGS {get; set;}
}

public class EffectStrings {
    public string NAME {get; set;}
    public string DESC {get; set;}
}

public class KeywordStrings {
    public string NAME {get; set;}
    public string DESC {get; set;}
}

public class PassiveStrings {
    public string NAME {get; set;}
    public string DESC {get; set;}
}

/// <summary>
/// Singleton class used for all UI-related string content.
/// </summary>
public class LocalizationLibrary {
    public static readonly LocalizationLibrary Instance = new LocalizationLibrary();

    private readonly Dictionary<string, AbilityStrings> abilityStrings;
    private readonly Dictionary<string, PassiveStrings> passiveStrings;
    // Note that all statuses are keywords, but not the other way around (e.g. Final, Push, Pull, Fixed Cooldown are *not* status effects.)
	// All statuses in effects.json should *also* be in keywords.json, but keywords differ in that they refer to a more generic definition
	// while a status refers to direct effects on a character.
    private readonly Dictionary<string, EffectStrings> effectStrings;
    private readonly Dictionary<string, KeywordStrings> keywordStrings;

    private LocalizationLibrary(){
        string jsonString = File.ReadAllText("Strings/English/abilities.json");
        abilityStrings = JsonSerializer.Deserialize<Dictionary<string, AbilityStrings>>(jsonString);

        jsonString = File.ReadAllText("Strings/English/effects.json");
        effectStrings = JsonSerializer.Deserialize<Dictionary<string, EffectStrings>>(jsonString);

        jsonString = File.ReadAllText("Strings/English/keywords.json");
        keywordStrings = JsonSerializer.Deserialize<Dictionary<string, KeywordStrings>>(jsonString);

        jsonString = File.ReadAllText("Strings/English/passives.json");
        passiveStrings = JsonSerializer.Deserialize<Dictionary<string, PassiveStrings>>(jsonString);
    }
    
    public AbilityStrings GetAbilityStrings(string abilityId){
        try {
            return abilityStrings[abilityId];
        } catch (Exception){
            GD.Print($"No key found for {abilityId}.");
            return null;
        }
    }

    public EffectStrings GetEffectStrings(string effectId){
        try {
            return effectStrings[effectId];
        } catch (Exception){
            GD.PushWarning($"No key found for {effectId}.");
            EffectStrings missing = new EffectStrings();
            missing.NAME = "MISSING_ID_FOR_" + effectId;
            return missing;
        }
    }

    public KeywordStrings GetKeywordStrings(string keywordID){
        try {
            return keywordStrings[keywordID];
        } catch (Exception){
            GD.Print($"No key found for {keywordID}.");
            KeywordStrings missing = new KeywordStrings();
            missing.NAME = "MISSING_ID_FOR_" + keywordID;
            return missing;
        }
    }

    public PassiveStrings GetPassiveStrings(string passiveId){
        try {
            return passiveStrings[passiveId];
        } catch (Exception){
            GD.Print($"No key found for {passiveId}.");
            return null;
        }
    }
}
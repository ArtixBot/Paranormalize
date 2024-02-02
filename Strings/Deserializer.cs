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

/// <summary>
/// Singleton class used for all UI-related string content.
/// </summary>
public class LocalizationLibrary {
    public static readonly LocalizationLibrary Instance = new LocalizationLibrary();

    private readonly Dictionary<string, AbilityStrings> abilityStrings;
    private readonly Dictionary<string, EffectStrings> effectStrings;

    private LocalizationLibrary(){
        string jsonString = File.ReadAllText("Strings/English/abilities.json");
        abilityStrings = JsonSerializer.Deserialize<Dictionary<string, AbilityStrings>>(jsonString);

        jsonString = File.ReadAllText("Strings/English/effects.json");
        effectStrings = JsonSerializer.Deserialize<Dictionary<string, EffectStrings>>(jsonString);
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
            GD.Print($"No key found for {effectId}.");
            return null;
        }
    }
}
using Godot;
using System;
using System.Collections.Generic;

public enum CharacterFaction {PLAYER, ALLY, NEUTRAL, ENEMY};

// [GlobalClass] annotation allows this to appear in the Godot editor.
[GlobalClass]
public partial class CharacterInfo : Resource {
    [Export]
    public CharacterFaction CHAR_FACTION;
    [Export]
    public string CHAR_NAME;

    // public List<AbstractAbility> abilities = new List<AbstractAbility>();       // At the start of combat, deep-copy everything from PERMA_ABILITIES.

    private int _CurHP, _MaxHP;
    private int _CurPoise, _MaxPoise;

    [Export]
    public int ActionsPerTurn;
    [Export]
    public int CurHP {
        get {return _CurHP;}
        set { _CurHP = Math.Min(value, MaxHP);}        // Whenever current HP is set, if it's greater than max HP, instead cap it at max HP.
    }
    [Export]
    public int MaxHP {
        get {return _MaxHP;}
        set { _MaxHP = value; CurHP = CurHP;}   // Call the CurHP setter to automatically update its value if curHP exceeded maxHP post-update.
    }
    [Export]
    public int CurPoise {
        get {return _CurPoise;}
        set { _CurPoise = Math.Min(value, _MaxPoise);}
    }
    [Export]
    public int MaxPoise {
        get {return _MaxPoise;}
        set { _MaxPoise = value; CurPoise = CurPoise;}
    }   
    [Export]
    public int MinSpd, MaxSpd;      // Speed modifiers like Haste or Slow are done in event handling.
    [Export]
    public int Position;

    // Make sure you provide a parameterless constructor.
    // In C#, a parameterless constructor is different from a
    // constructor with all default values.
    // Without a parameterless constructor, Godot will have problems
    // creating and editing your resource via the inspector.
    public CharacterInfo() : this(10, 10, 1, 5) {}

    public CharacterInfo(int maxHP, int maxPoise, int minSpd, int maxSpd){
        this.MaxHP = maxHP;
        this.CurHP = maxHP;
        this.MaxPoise = maxPoise;
        this.CurPoise = maxPoise;
        this.MinSpd = minSpd;
        this.MaxSpd = maxSpd;
        this.ActionsPerTurn = 2;
    }
}

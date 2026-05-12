using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public enum DieTags {
    MELEE,      // Used for animation purposes.
    RANGED,     // Used for animation purposes. Possibly ignores reflection/thorns effects?
    RESTRAINED  // This die's value cannot be changed.
};
public enum DieType {SLASH, PIERCE, BLUNT, ELDRITCH, BLOCK, EVADE, UNIQUE};

/*
    The Die struct forms the basis for the entire combat system.
    Abilities are composed of various amounts of dice. When activated, each die is rolled sequentially.
    Die contains:
        - (optional) dieId
        - DieType
        - ints for the minimum and maximum roll value
*/
public class Die {
    // Used for die-specific *descriptions*, e.g. "This die has +2 to rolls against X".
    // This should *not* be used for logic (since multiple instaces of an ability would share the same dice ID.)
    private readonly string _dieId;
    private readonly DieType _dieType;
    private readonly int _minValue;
    private readonly int _maxValue;
    public readonly List<DieTags> DieTags;
    public readonly bool IsAttackDie;
    public readonly bool IsDefenseDie;

    public Die(DieType dieType, int minValue, int maxValue, string dieId = "", List<DieTags> dieTags = null){
        _dieId = dieId;
        _dieType = dieType;
        _minValue = minValue;
        _maxValue = maxValue;
        DieTags = dieTags ?? new List<DieTags>{};
        IsAttackDie = this._dieType == DieType.SLASH || this._dieType == DieType.PIERCE || this._dieType == DieType.BLUNT || this._dieType == DieType.ELDRITCH;
        IsDefenseDie = this._dieType == DieType.BLOCK || this._dieType == DieType.EVADE;
    }    
    public int Roll(){
        return Rng.RandiRange(MinValue, MaxValue);
    }

    public string DieId{
        get {return _dieId;}
    }

    public DieType DieType{
        get {return _dieType;}
    }

    public int MinValue{
        get {return _minValue;}
    }

    public int MaxValue{
        get {return _maxValue;}
    }

    public bool HasTag(DieTags tag){
        return this.DieTags.Contains(tag);
    }
}
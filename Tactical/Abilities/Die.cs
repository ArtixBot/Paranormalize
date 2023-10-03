using System.Collections;
using System.Collections.Generic;

public enum DieType {MELEE, RANGED, BLOCK, EVADE, UNIQUE};

/*
    The Die struct forms the basis for the entire combat system.
    Abilities are composed of various amounts of dice. When activated, each die is rolled sequentially.
    Die contains:
        - (optional) dieId
        - DieType
        - ints for the minimum and maximum roll value
*/
public readonly struct Die {
    private readonly string _dieId;
    private readonly DieType _dieType;
    private readonly int _minValue;
    private readonly int _maxValue;

    public Die(DieType dieType, int minValue, int maxValue, string dieId = ""){
        _dieId = dieId;
        _dieType = dieType;
        _minValue = minValue;
        _maxValue = maxValue;
    }
    
    public readonly int Roll(){
        return Rng.RandiRange(MinValue, MaxValue);
    }

    public readonly string DieId{
        get {return _dieId;}
    }

    public readonly DieType DieType{
        get {return _dieType;}
    }

    public readonly int MinValue{
        get {return _minValue;}
    }

    public readonly int MaxValue{
        get {return _maxValue;}
    }
}
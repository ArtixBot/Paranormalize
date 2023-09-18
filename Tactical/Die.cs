using System.Collections;
using System.Collections.Generic;

public enum DieType {MELEE, RANGED, BLOCK, EVADE, UNIQUE};

/*
    The Die class forms the basis for the entire combat system.
    Abilities are composed of various amounts of dice. When activated, each die is rolled sequentially.
    Die contains:
        - (optional) dieId
        - DieType
        - ints for the minimum and maximum roll value
*/
public class Die {
    public string dieId;
    public DieType dieType;
    public int minValue;
    public int maxValue;

    public Die(DieType dieType, int minValue, int maxValue, string dieId = ""){
        this.dieId = dieId;
        this.dieType = dieType;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
    
    public int Roll(){
        return Rng.RandiRange(minValue, maxValue);
    }

    public Die GetCopy(){
        return new Die(this.dieType, this.minValue, this.maxValue, this.dieId);
    }

    public DieType GetDieType(){
        return this.dieType;
    }

    public int GetMinValue(){
        return this.minValue;
    }

    public int GetMaxValue(){
        return this.maxValue;
    }
}
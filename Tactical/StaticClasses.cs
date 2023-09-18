using Godot;
using System;

public static class Rng {
    private static readonly RandomNumberGenerator rng = new();

    /// <summary>
    /// Invokes Godot's rng.RandiRange() function, returning an inclusive value between [minValue, maxValue].
    /// </summary>
    public static int RandiRange(int minValue, int maxValue){
        return rng.RandiRange(minValue, maxValue);
    }
}
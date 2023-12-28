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

public static class Logging {

    public enum LogLevel {
        ESSENTIAL = 1,
        INFO = 2,
        DEBUG = 3,
    }

    public static LogLevel LOGGING_LEVEL = LogLevel.DEBUG;
    
    /// <summary>
    /// Invokes Godot's GD.Print() function if the logLevel is less than or equal to GameVariables.LOGGING_LEVEL.
    /// Lower values should be used for more critical logs.
    /// </summary>
    /// <param name="logString">The string to be logged by GD.Print.</param>
    /// <param name="logLevel">The level to log at. Lower levels appear more often.</param>
    public static void Log(string logString, LogLevel logLevel){
        if (logLevel <= Logging.LOGGING_LEVEL){
            GD.Print(logString);
        }
    }
}

public static class GameVariables {
    public const int MIN_LANES = 1;
    public const int MAX_LANES = 6;
}
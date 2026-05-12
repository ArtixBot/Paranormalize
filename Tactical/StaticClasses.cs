using System;
using System.Threading.Tasks;
using Godot;

public static class GameVariables {
    public const int MIN_LANES = 1;
    public const int MAX_LANES = 6;
}

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

    public static LogLevel LOGGING_LEVEL = LogLevel.INFO;
    
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

public static class Lerpables {
    // Credit to https://www.febucci.com/2018/08/easing-functions/
	private static float Flip(float x){
    	return 1 - x;
	}

	public static float EaseOut(float t, int power = 3){
		return Flip((float)Math.Pow(Flip(t), power));
	}

    public static async void FadeIn(Control node, double duration){
        if (!GodotObject.IsInstanceValid(node)) return;
		float currentTime = 0f;
        Vector2 startPos = node.Position;
        Vector2 endPos = node.Position + new Vector2(20, 0);

        // Use vectors so we can use Godot's in-built Lerp function.
        Vector2 startBlurRadius = new Vector2(0.0f, 0.0f);
        Vector2 endBlurRadius = new Vector2(1.0f, 0.0f);

		while (currentTime <= duration && GodotObject.IsInstanceValid(node)){
            float normalized = Math.Min((float)(currentTime / duration), 1.0f);
			node.Position = startPos.Lerp(endPos, EaseOut(normalized, 5));
			node.Modulate = new Color(1, 1, 1, startBlurRadius.Lerp(endBlurRadius, EaseOut(normalized, 5)).X);

			await node.ToSignal(node.GetTree(), SceneTree.SignalName.ProcessFrame);
            currentTime += (float)node.GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
        }
	}

}
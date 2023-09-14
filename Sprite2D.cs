using Godot;

public partial class Sprite2D : Godot.Sprite2D
{
	[Signal]
	public delegate void HealthDepletedEventHandler();

	private float _speed = 400;
	private float _angularSpeed = Mathf.Pi;

	public override void _Ready() {
		Timer timer = GetNode<Timer>("Timer");
		timer.Timeout += OnTimerTimeout;
	}

	public override void _Process(double delta) {
		Rotation += _angularSpeed * (float)delta;
		var velocity = Vector2.Up.Rotated(Rotation) * _speed;
		Position += velocity * (float)delta;
	}

	private void _on_button_pressed() {
		SetProcess(!IsProcessing());
	}

	private void OnTimerTimeout(){
		Visible = !Visible;
	}
}

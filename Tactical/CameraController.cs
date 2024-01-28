using Godot;
using System;
using System.Collections;
using System.Threading.Tasks;

public partial class CameraController : Camera2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		this.Position = new Vector2(960, 540);
	}

	// TODO: Remove this, this is for testing purposes only.
	public override async void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed){
			if (keyEvent.Keycode == Key.T){
				await CinematicZoom(0.1f, 0.5f);
			}
		}
	}

	private bool isCinematic = false;

    public async Task<bool> CinematicZoom(float zoomAmount, float duration){
		if (isCinematic) return false;
		Vector2 oldZoom = this.Zoom;
		Vector2 newZoom = this.Zoom + new Vector2(zoomAmount, zoomAmount);

		float currentTime = 0f;
		while (currentTime <= duration){
            float normalized = Math.Min((float)(currentTime / duration), 1.0f);
			this.Zoom = oldZoom.Lerp(newZoom, EaseOut(normalized));

			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
        }
		isCinematic = true;
		return true;
	}

	public async Task<bool> ResetZoom(float duration){
		Vector2 oldZoom = this.Zoom;
		Vector2 newZoom = new Vector2(1.0f, 1.0f);

		float currentTime = 0f;
		while (currentTime <= duration){
            float normalized = Math.Min((float)(currentTime / duration), 1.0f);
			this.Zoom = oldZoom.Lerp(newZoom, EaseOut(normalized));

			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
        }
		isCinematic = false;
		return true;
	}

	// Credit to https://www.febucci.com/2018/08/easing-functions/
	private static float Flip(float x){
    	return 1 - x;
	}

	public static float EaseOut(float t){
		return Flip((float)Math.Pow(Flip(t), 3));
	}
}

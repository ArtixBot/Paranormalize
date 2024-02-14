using Godot;
using System;
using System.Collections;
using System.Threading.Tasks;

public partial class CameraController : Camera2D
{

	public ColorRect cameraEffect;
	public ShaderMaterial vignetteEffect;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		cameraEffect = GetNode<ColorRect>("ColorRect");
		vignetteEffect = GD.Load<ShaderMaterial>("res://Tactical/UI/Shaders/BlurVignette.tres");
		this.Position = new Vector2(960, 540);
	}

	private bool isCinematic = false;

    public async Task<bool> CinematicZoom(float zoomAmount, float duration){
		if (isCinematic) return false;
		Vector2 oldZoom = this.Zoom;
		Vector2 newZoom = this.Zoom + new Vector2(zoomAmount, zoomAmount);

		// Use vectors so we can use Godot's in-built Lerp function.
		Vector2 startBlurRadius = new Vector2(0.01f, 0.0f);
		Vector2 endBlurRadius = new Vector2(0.2f, 0.0f);
		cameraEffect.Material = vignetteEffect;
		vignetteEffect.SetShaderParameter("blur_radius", startBlurRadius);

		float currentTime = 0f;
		while (currentTime <= duration){
            float normalized = Math.Min((float)(currentTime / duration), 1.0f);
			this.Zoom = oldZoom.Lerp(newZoom, EaseOut(normalized));

			vignetteEffect.SetShaderParameter("blur_radius", startBlurRadius.Lerp(endBlurRadius, EaseOut(normalized)).X);

			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
        }

		isCinematic = true;
		return true;
	}

	public async Task<bool> ResetZoom(float duration){
		Vector2 oldZoom = this.Zoom;
		Vector2 newZoom = new Vector2(1.0f, 1.0f);

		// Use vectors so we can use Godot's in-built Lerp function.
		Vector2 startBlurRadius = new Vector2(0.2f, 0.0f);
		Vector2 endBlurRadius = new Vector2(0.01f, 0.0f);
		vignetteEffect.SetShaderParameter("blur_radius", startBlurRadius);

		float currentTime = 0f;
		while (currentTime <= duration){
            float normalized = Math.Min((float)(currentTime / duration), 1.0f);
			this.Zoom = oldZoom.Lerp(newZoom, EaseOut(normalized));

			vignetteEffect.SetShaderParameter("blur_radius", startBlurRadius.Lerp(endBlurRadius, EaseOut(normalized)).X);

			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
        }
		cameraEffect.Material = null;
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

using Godot;
using System;
using System.Threading.Tasks;

public partial class CameraController : Camera2D
{

	public ColorRect focusEffect;
	public ShaderMaterial vignetteEffect;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		focusEffect = GetNode<ColorRect>("Focus Effect");
		vignetteEffect = GD.Load<ShaderMaterial>("res://Tactical/UI/Shaders/FocusVignette.tres");
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
		focusEffect.Material = vignetteEffect;
		vignetteEffect.SetShaderParameter("blur_radius", startBlurRadius);

		float currentTime = 0f;
		while (currentTime <= duration){
            float normalized = Math.Min((float)(currentTime / duration), 1.0f);
			this.Zoom = oldZoom.Lerp(newZoom, Lerpables.EaseOut(normalized));

			vignetteEffect.SetShaderParameter("blur_radius", startBlurRadius.Lerp(endBlurRadius, Lerpables.EaseOut(normalized)).X);

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
			this.Zoom = oldZoom.Lerp(newZoom, Lerpables.EaseOut(normalized));

			vignetteEffect.SetShaderParameter("blur_radius", startBlurRadius.Lerp(endBlurRadius, Lerpables.EaseOut(normalized)).X);

			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
        }
		focusEffect.Material = null;
		isCinematic = false;
		return true;
	}
}

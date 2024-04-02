using Godot;
using System;
using UI;

public partial class TurnNode : Control
{
	private TextureRect charIcon;
	private Label speedLabel;

	public CharacterUI Character {
		set {
			AtlasTexture atlasTex = new();
			if (value.Poses.ContainsKey("icon")){
				atlasTex.Atlas = value.Poses["icon"];
				atlasTex.Region = new Rect2(0, 175, 600, 200);
			} else if (value.Poses.ContainsKey("idle")){
				atlasTex.Atlas = value.Poses["idle"];
				atlasTex.Region = new Rect2(0, 50, 133, 100);
			} else {
				atlasTex.Atlas = GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png");
				atlasTex.Region = new Rect2(0, 175, 600, 200);
			}
			charIcon.Texture = atlasTex;
		}
	}
	public int Speed {
		set {
			speedLabel.Text = value.ToString();
		}
	}

	public override void _Ready(){
		charIcon = GetNode<TextureRect>("BG/TextureRect");
		speedLabel = GetNode<Label>("BG/SpeedLabel");
	}
}

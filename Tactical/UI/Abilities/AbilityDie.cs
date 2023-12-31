using Godot;
using System;

public partial class AbilityDie : Control
{
	private Die _die;
	public Die Die {
		get {return _die;}
		set {_die = value; UpdateImage(); UpdateText(); UpdateDescription();}
	}

	private TextureRect DieImage;
	private Label DieRange;
	private Label DieDesc;

	private void UpdateImage(){
		switch (Die.DieType){
			case DieType.SLASH:
				// TODO: Preload this instead: https://docs.godotengine.org/en/stable/classes/class_%40gdscript.html#class-gdscript-method-preload
				DieImage.Texture = ResourceLoader.Load<Texture2D>("res://Sprites/die - slash.png");
				break;
			case DieType.PIERCE:
				DieImage.Texture = ResourceLoader.Load<Texture2D>("res://Sprites/die - pierce.png");
				break;
			case DieType.BLUNT:
				DieImage.Texture = ResourceLoader.Load<Texture2D>("res://Sprites/die - blunt.png");
				break;
			case DieType.MAGIC:
			case DieType.BLOCK:
				DieImage.Texture = ResourceLoader.Load<Texture2D>("res://Sprites/die - block.png");
				break;
			case DieType.EVADE:
				DieImage.Texture = ResourceLoader.Load<Texture2D>("res://Sprites/die - evade.png");
				break;
			case DieType.UNIQUE:
				break;
		}
	}

	public override void _Ready() {
		// Note: Children _Ready() callbacks are always triggered first, see https://docs.godotengine.org/en/3.2/classes/class_node.html#class-node-method-ready
		DieImage = (TextureRect) GetNode("TextureRect");
		DieRange = (Label) GetNode("Roll Range");
		DieDesc = (Label) GetNode("Description");
		Die = new Die(DieType.BLOCK, 7, 9);
	}

	private void UpdateText(){
		DieRange.Text = $"{Die.MinValue} - {Die.MaxValue}";
	}

	private void UpdateDescription(){
		DieDesc.Text = "DESC";
	}

	// // Called when the node enters the scene tree for the first time.
	// public override void _Ready() {
	// }

	// // Called every frame. 'delta' is the elapsed time since the previous frame.
	// public override void _Process(double delta)
	// {
	// }
}

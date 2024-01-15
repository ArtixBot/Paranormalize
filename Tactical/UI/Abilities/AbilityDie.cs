using Godot;

namespace UI;

public partial class AbilityDie : Control
{
	private Die _die;
	public Die Die {
		get {return _die;}
		set {_die = value; UpdateImage(); UpdateRollRange();}
	}

	private RichTextLabel _dieDesc;
	public string DieDesc {
		get {return _dieDesc?.Text;}
		set {_dieDesc.Text = value;}
	}

	private TextureRect DieImage;
	private Label DieRange;

	public override void _Ready() {
		// Note: Children _Ready() callbacks are always triggered first, see https://docs.godotengine.org/en/3.2/classes/class_node.html#class-node-method-ready
		DieImage = (TextureRect) GetNode("TextureRect");
		DieRange = (Label) GetNode("Roll Range");
		_dieDesc = (RichTextLabel) GetNode("Description");
	}

	private void UpdateImage(){
		switch (_die.DieType){
			case DieType.SLASH:
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

	private void UpdateRollRange(){
		DieRange.Text = $"{Die.MinValue} - {Die.MaxValue}";
	}
}

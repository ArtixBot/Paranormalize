using Godot;
using System;
using System.Linq;

public partial class CharacterUI : Control, IEventSubscriber
{
	private AbstractCharacter _character;
	public AbstractCharacter Character {
		get {return _character;}
		set {_character = value; UpdateStatsText(); UpdateSprite();}
	}

	public Sprite2D Sprite;
	private Label Stats;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		InitSubscriptions();
	}

	public void _on_label_ready(){
		Stats = GetNode<Label>("Sprite2D/Label");
		UpdateStatsText();
	}

	public void _on_sprite_2d_ready(){
		Sprite = GetNode<Sprite2D>("Sprite2D");
		UpdateSprite();
	}

	private void UpdateStatsText(){
		if (Character == null || Stats == null) {return;}
		string poise = $"Poise: {Character.CurPoise} / {Character.MaxPoise}";
		if (Character.statusEffects.Where(status => status.ID == "STAGGERED").Count() > 0){
			poise = "Staggered!";
		}
		Stats.Text = $"HP: {Character.CurHP} / {Character.MaxHP}\n{poise}";
	}

	private void UpdateSprite(){
		if (Character == null || Sprite == null) {return;}
		if (Character.CHAR_FACTION == CharacterFaction.PLAYER){
			Sprite.Texture = ResourceLoader.Load<Texture2D>("res://Sprites/character.png");
		}
		if (Character.CHAR_FACTION == CharacterFaction.ENEMY){
			Sprite.Texture = ResourceLoader.Load<Texture2D>("res://Sprites/target dummy.png");
		}
	}

	public virtual void InitSubscriptions(){
		// TODO: Change this to something like ON_TAKE_DAMAGE or ON_HP_CHANGED instead.
		CombatManager.eventManager?.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.UI);
    }

    public void HandleEvent(CombatEventData eventData){
        UpdateStatsText();
    }
}

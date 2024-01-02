using Godot;
using System.Linq;
using System.Reflection.Metadata;

namespace UI;

public partial class CharacterUI : Control, IEventSubscriber, IEventHandler<CombatEventTurnEnd>
{
	private AbstractCharacter _character;
	public AbstractCharacter Character {
		get {return _character;}
		set {_character = value; UpdateStatsText(); UpdateSprite();}
	}

	public Area2D ClickableArea;
	public Sprite2D Sprite;
	private Label Stats;

	private bool _IsClickable;
	public bool IsClickable {
		get {return _IsClickable;}
		set {_IsClickable = value; ClickableArea.InputPickable = IsClickable;}		// LINK - Tactical\UI\GUIOrchestrator.cs:59
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Sprite = GetNode<Sprite2D>("Area2D/Sprite2D");
		Stats = GetNode<Label>("Area2D/Sprite2D/Label");
		ClickableArea = GetNode<Area2D>("Area2D");
		IsClickable = false;

		UpdateStatsText();
		UpdateSprite();
		InitSubscriptions();
	}

    public void _on_area_2d_input_event(Viewport viewport, InputEvent @event, int shape_idx){
        if (@event is InputEventMouseButton && @event.IsPressed() == false){
			GD.Print($"{Character?.CHAR_NAME} was clicked.");
		}
    }

	public void _on_label_tree_exited(){
		CombatManager.eventManager?.UnsubscribeAll(this);
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
		CombatManager.eventManager?.Subscribe(CombatEventType.ON_TURN_END, this, CombatEventPriority.UI);
    }

    public void HandleEvent(CombatEventTurnEnd eventData){
        UpdateStatsText();
    }
}

using Godot;
using System.Linq;
using System.Reflection.Metadata;

namespace UI;

public partial class CharacterUI : Control, IEventSubscriber, IEventHandler<CombatEventTurnEnd>, IEventHandler<CombatEventRoundStart>
{
	[Signal]
	public delegate void CharacterSelectedEventHandler(CharacterUI character);

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
		set {
			_IsClickable = value; 
			ClickableArea.InputPickable = IsClickable;				// LINK - Tactical\UI\GUIOrchestrator.cs:59
			// TODO - Don't invoke GD.Load on every reassignment to IsClickable, should preload resource instead.
			Sprite.Material = IsClickable ? GD.Load<Material>("res://Tactical/UI/Shaders/CharacterTargetable.tres") : null;
		}
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
        if (IsClickable && @event is InputEventMouseButton && @event.IsPressed() == false){
			EmitSignal(nameof(CharacterSelected), this);
		}
    }

	public void _on_area_2d_mouse_entered(){
		ActiveCharInterfaceLayer activeCharNode = GetNode<ActiveCharInterfaceLayer>("../../Active Character");
		// TODO: Handle case when the unit is staggered (see second condition) more artfully.
		if (IsClickable && CombatManager.combatInstance.turnlist.ContainsItem(_character) && IsInstanceValid(activeCharNode) && _character.Behavior?.reactions.Count > 0){
			activeCharNode.CreateAbilityDetailPanel(_character.Behavior.reactions.First(), true);
		}
	}

	public void _on_area_2d_mouse_exited(){
		ActiveCharInterfaceLayer activeCharNode = GetNode<ActiveCharInterfaceLayer>("../../Active Character");
		if (IsInstanceValid(activeCharNode)){
			activeCharNode.DeleteAbilityDetailPanel(true);
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
		CombatManager.eventManager?.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.UI);
    }

    public void HandleEvent(CombatEventTurnEnd eventData){
        UpdateStatsText();
    }

	public void HandleEvent(CombatEventRoundStart eventData){
        UpdateStatsText();
    }
}

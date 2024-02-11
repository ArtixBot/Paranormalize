using Godot;
using System.Linq;
using System.Reflection.Metadata;

namespace UI;

public partial class CharacterUI : Area2D, IEventSubscriber, IEventHandler<CombatEventTurnEnd>, IEventHandler<CombatEventCombatStateChanged>
{
	[Signal]
	public delegate void CharacterSelectedEventHandler(CharacterUI character);

	private AbstractCharacter _character;
	public AbstractCharacter Character {
		get {return _character;}
		set {_character = value; UpdateSprite();}
	}

	public Sprite2D Sprite;
	private Label HPStat;
	private RichTextLabel PoiseStat;
	
	private TextureRect activeBuffs;
	private TextureRect activeConditions;
	private TextureRect activeDebuffs;

	private readonly PackedScene statusTooltip = GD.Load<PackedScene>("res://Tactical/UI/Tooltip.tscn");
	private StatusTooltip statusTooltipInstance;

	private bool _IsClickable;
	public bool IsClickable {
		get {return _IsClickable;}
		set {
			_IsClickable = value; 
			this.InputPickable = IsClickable;				// LINK - Tactical\UI\GUIOrchestrator.cs:59
			// TODO - Don't invoke GD.Load on every reassignment to IsClickable, should preload resource instead.
			Sprite.Material = IsClickable ? GD.Load<Material>("res://Tactical/UI/Shaders/CharacterTargetable.tres") : null;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Sprite = GetNode<Sprite2D>("Sprite2D");
		HPStat = GetNode<Label>("Sprite2D/HP/Label");
		PoiseStat = GetNode<RichTextLabel>("Sprite2D/Poise/Label");

		activeBuffs = GetNode<TextureRect>("Sprite2D/Active Buffs");
		activeConditions = GetNode<TextureRect>("Sprite2D/Active Conditions");
		activeDebuffs = GetNode<TextureRect>("Sprite2D/Active Debuffs");

		IsClickable = false;

		UpdateSprite();
		InitSubscriptions();
	}

    public void _on_input_event(Viewport viewport, InputEvent @event, int shape_idx){
        if (IsClickable && @event is InputEventMouseButton && @event.IsPressed() == false){
			EmitSignal(nameof(CharacterSelected), this);
		}
    }

	public void _on_mouse_entered(){
		ActiveCharInterfaceLayer activeCharNode = GetNode<ActiveCharInterfaceLayer>("../HUD/GUI/Active Character");
		// TODO: Handle case when the unit is staggered (see second condition) more artfully.
		if (IsClickable && CombatManager.combatInstance.turnlist.ContainsItem(_character) && IsInstanceValid(activeCharNode) && _character.Behavior?.reactions.Count > 0){
			activeCharNode.CreateAbilityDetailPanel(_character.Behavior.reactions.First(), true);
		}
	}

	public void _on_mouse_exited(){
		ActiveCharInterfaceLayer activeCharNode = GetNode<ActiveCharInterfaceLayer>("../HUD/GUI/Active Character");
		if (IsInstanceValid(activeCharNode)){
			activeCharNode.DeleteAbilityDetailPanel(true);
		}
	}

	public void _on_tree_exited(){
		CombatManager.eventManager?.UnsubscribeAll(this);
	}

	public void _on_active_buffs_mouse_entered(){
		StatusTooltip tooltipNode = (StatusTooltip) statusTooltip.Instantiate();
		AddChild(tooltipNode);
		tooltipNode.Effects = this.Character.statusEffects.Where(effect => effect.TYPE == StatusEffectType.BUFF).ToList();
		tooltipNode.SetPosition(new Vector2(100, 300));

		statusTooltipInstance = tooltipNode;
	}

	public void _on_active_conditions_mouse_entered(){
		StatusTooltip tooltipNode = (StatusTooltip) statusTooltip.Instantiate();
		AddChild(tooltipNode);
		tooltipNode.Effects = this.Character.statusEffects.Where(effect => effect.TYPE == StatusEffectType.CONDITION).ToList();
		tooltipNode.SetPosition(new Vector2(100, 300));

		statusTooltipInstance = tooltipNode;
	}

	public void _on_active_debuffs_mouse_entered(){
		StatusTooltip tooltipNode = (StatusTooltip) statusTooltip.Instantiate();
		AddChild(tooltipNode);
		tooltipNode.Effects = this.Character.statusEffects.Where(effect => effect.TYPE == StatusEffectType.DEBUFF).ToList();
		tooltipNode.SetPosition(new Vector2(100, 300));

		statusTooltipInstance = tooltipNode;
	}

	public void _on_active_buffs_mouse_exited(){
		if (!IsInstanceValid(statusTooltipInstance)){ return; }
		statusTooltipInstance.QueueFree();
	}

	public void _on_active_conditions_mouse_exited(){
		if (!IsInstanceValid(statusTooltipInstance)){ return; }
		statusTooltipInstance.QueueFree();
	}

	public void _on_active_debuffs_mouse_exited(){
		if (!IsInstanceValid(statusTooltipInstance)){ return; }
		statusTooltipInstance.QueueFree();
	}

	private void UpdateStatsText(){
		if (Character == null || HPStat == null || PoiseStat == null) {return;}
		HPStat.Text = $"{Character.CurHP}";
		PoiseStat.Text = $"[font n='res://Assets/Jost-Medium.ttf' s=24]{Character.CurPoise}[/font]";
		if (Character.CurPoise == 0) {
			PoiseStat.Text = "[shake]" + PoiseStat.Text + "[/shake]";
		}
	}

	private void UpdateSprite(){
		if (Character == null || Sprite == null) {return;}
		if (Character.CHAR_FACTION == CharacterFaction.PLAYER){
			Sprite.Texture = ResourceLoader.Load<Texture2D>("res://Sprites/Characters/Duelist/idle.png");
		}
		if (Character.CHAR_FACTION == CharacterFaction.ENEMY){
			Sprite.Texture = ResourceLoader.Load<Texture2D>("res://Sprites/Characters/Test Dummy/idle.png");
		}
	}

	private void UpdateBuffs(){
		if (!Character.HasBuff) {
			activeBuffs.Visible = false;
			return;
		}
		activeBuffs.Visible = true;
		// TODO: Add on-hover functionality.
	}

	private void UpdateDebuffs(){
		if (!Character.HasDebuff) {
			activeDebuffs.Visible = false;
			return;
		}
		activeDebuffs.Visible = true;
		// TODO: Add on-hover functionality.
	}

	private void UpdateConditions(){
		if (!Character.HasCondition) {
			activeConditions.Visible = false;
			return;
		}
		activeConditions.Visible = true;
		// TODO: Add on-hover functionality.
	}

	public virtual void InitSubscriptions(){
		// TODO: Change this to something like ON_TAKE_DAMAGE or ON_HP_CHANGED instead.
		CombatManager.eventManager?.Subscribe(CombatEventType.ON_TURN_END, this, CombatEventPriority.UI);
		CombatManager.eventManager?.Subscribe(CombatEventType.ON_COMBAT_STATE_CHANGE, this, CombatEventPriority.UI);
    }

    public void HandleEvent(CombatEventTurnEnd eventData){
        UpdateStatsText();
    }

	public void HandleEvent(CombatEventCombatStateChanged eventData){
        UpdateStatsText();
		UpdateBuffs();
		UpdateDebuffs();
		UpdateConditions();
    }
}

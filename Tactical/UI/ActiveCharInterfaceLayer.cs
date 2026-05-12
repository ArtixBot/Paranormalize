using Godot;
using System;
using System.Collections.Generic;
using UI;

public partial class ActiveCharInterfaceLayer : Control, IEventSubscriber, IEventHandler<CombatEventTurnStart>, IEventHandler<CombatEventClashEligible> {

	private AbstractCharacter _activeChar;
	public AbstractCharacter ActiveChar {
		get {return _activeChar;}
		set {_activeChar = value; UpdateAvailableAbilities(); UpdateCharacterStats();}
	}

	private Label charName;
	private Label charHP;
	private Label charPoise;

	private Control abilityListNode;
	private readonly PackedScene abilityButton = GD.Load<PackedScene>("res://Tactical/UI/Abilities/AbilityButton.tscn");
	private readonly PackedScene abilityInfoPanel = GD.Load<PackedScene>("res://Tactical/UI/Abilities/AbilityInfoPanel.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		abilityListNode = GetNode<Control>("Ability List");
		charName = GetNode<Label>("Portrait/Name");
		charHP = charName.GetNode<Label>("HP Val");
		charPoise = charName.GetNode<Label>("Poise Val");

		InitSubscriptions();
	}
	
	public bool stickyAbilityInfoPanel = false;		// If true, the created ability detail panel is "sticky" and will not be deleted if the mouse exits the ability button.
	private AbilityInfoPanel abilityInfoPanelInstance = new();
	private AbilityInfoPanel opposingAbilityInfoPanelInstance = new();
	private List<AbilityButton> abilityButtonInstances = new();
	private List<AbstractAbility> reactionAbilities = new();
	private void UpdateAvailableAbilities(){
		// Remove previous instances from code.
		foreach(AbilityButton instance in abilityButtonInstances){
			abilityListNode.RemoveChild(instance);
		}
		abilityButtonInstances.Clear();

		CombatInstance combatInstance = CombatManager.combatInstance;

		if (combatInstance == null || ActiveChar == null) return;
		bool doClashProcessing = CombatManager.combatInstance?.combatState == CombatState.AWAITING_CLASH_INPUT;

		List<AbstractAbility> abilitiesToDisplay = doClashProcessing ? reactionAbilities : ActiveChar.abilities;
		for(int i = 0; i < abilitiesToDisplay.Count; i++){
			AbilityButton instance = (AbilityButton) abilityButton.Instantiate();
			abilityButtonInstances.Add(instance);
			instance.SetPosition(new Vector2(0, i * instance.Size.Y));

			AbstractAbility ability = abilitiesToDisplay[i];
			abilityListNode.AddChild(instance);
			instance.Ability = ability;
			
			GUIOrchestrator parent = (GUIOrchestrator) GetParent();
			if (doClashProcessing){
				// LINK - Tactical\UI\Abilities\AbilityButton.cs#emit-ability-selected
				instance.AbilitySelected += (instance) => parent._on_child_clash_selection(instance.Ability);
			} else {
				instance.AbilitySelected += (instance) => parent._on_child_ability_selection(instance.Ability);
			}

			instance.MouseEntered += () => CreateAbilityInfoPanel(ability, false, instance.Position);
			instance.Pressed += () => stickyAbilityInfoPanel = true;
			instance.MouseExited += () => DeleteAbilityInfoPanel(false);
		}
	}

	public void CreateAbilityInfoPanel(AbstractAbility ability, bool isOpposingAbility, Vector2 instancePosition = default){
		if (isOpposingAbility && IsInstanceValid(opposingAbilityInfoPanelInstance)){
			opposingAbilityInfoPanelInstance.QueueFree();
		} else if (!isOpposingAbility && IsInstanceValid(abilityInfoPanelInstance)) {
			abilityInfoPanelInstance.QueueFree();
		}

		AbilityInfoPanel node = (AbilityInfoPanel) abilityInfoPanel.Instantiate();
		AddChild(node);
		node.Ability = ability;
		node.SetSize(new Vector2(600, 400));		// This should be unnecessary but not including it makes the container stretch vertically?
		node.SetPosition((!isOpposingAbility) ? new Vector2(300, instancePosition.Y) : new Vector2(1000, 750));

		Lerpables.FadeIn(node, 0.15);

		if (!isOpposingAbility){
			abilityInfoPanelInstance = node;
		} else {
			opposingAbilityInfoPanelInstance = node;
		}
	}

	public void DeleteAbilityInfoPanel(bool isOpposingAbility){
		if (!stickyAbilityInfoPanel && !isOpposingAbility && IsInstanceValid(abilityInfoPanelInstance)){
			abilityInfoPanelInstance.QueueFree();
		} else if (isOpposingAbility && IsInstanceValid(opposingAbilityInfoPanelInstance)){
			// sticky panel never has to apply for enemy's panel since it doesn't get deleted by ability button exit.
			opposingAbilityInfoPanelInstance.QueueFree();
		}
	}

	private void UpdateCharacterStats(){
		CombatInstance combatInstance = CombatManager.combatInstance;
		if (combatInstance == null) return;
		charName.Text = ActiveChar.CHAR_NAME;
		charHP.Text = ActiveChar.CurHP + "/" + ActiveChar.MaxHP;
		charPoise.Text = ActiveChar.CurPoise + "/" + ActiveChar.MaxPoise;
	}
	
	public virtual void InitSubscriptions(){
		CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_CLASH_ELIGIBLE, this, CombatEventPriority.UI);
    }

    public void HandleEvent(CombatEventTurnStart data){
		stickyAbilityInfoPanel = false;
		DeleteAbilityInfoPanel(false);			// TODO: This deletes the ability panel if a clash occurs (since MouseExited doesn't apply). Find a better way to do this.
		DeleteAbilityInfoPanel(true);			// TODO: This deletes the ability panel if a clash occurs (since MouseExited doesn't apply). Find a better way to do this.
		this.ActiveChar = data.character;
	}

	public void HandleEvent(CombatEventClashEligible data){
		this.reactionAbilities = data.reactableAbilities;		// This should be set first since setting ActiveChar will force an update of abilities.
		this.ActiveChar = data.defender;

		CreateAbilityInfoPanel(data.attackerAbility, true);
	}
}

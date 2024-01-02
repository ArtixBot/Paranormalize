using Godot;
using System.Collections.Generic;
using UI;

public partial class ActiveCharInterfaceLayer : Control, IEventSubscriber, IEventHandler<CombatEventTurnStart> {

	private AbstractCharacter _activeChar;
	public AbstractCharacter ActiveChar {
		get {return _activeChar;}
		set {_activeChar = value; UpdateAvailableAbilities(); UpdateCharacterName();}
	}

	private Label charName;

	private Control abilityListNode;
	private readonly PackedScene abilityButton = GD.Load<PackedScene>("res://Tactical/UI/Abilities/AbilityButton.tscn");
	private readonly PackedScene abilityDetailPanel = GD.Load<PackedScene>("res://Tactical/UI/Abilities/AbilityDetailPanel.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {		
		abilityListNode = GetNode<Control>("Ability List");
		charName = GetNode<Label>("Portrait/Name");

		InitSubscriptions();
	}
	
	private AbilityDetailPanel abilityDetailPanelInstance = new();
	private List<AbilityButton> abilityButtonInstances = new();
	private List<(int lane, HashSet<AbstractCharacter> targetsInLane)> abilityTargeting;
	private void UpdateAvailableAbilities(){
		// Remove previous instances from code.
		foreach(AbilityButton instance in abilityButtonInstances){
			abilityListNode.RemoveChild(instance);
		}
		abilityButtonInstances.Clear();

		CombatInstance combatInstance = CombatManager.combatInstance;

		if (combatInstance == null || ActiveChar == null) return;
		for(int i = 0; i < ActiveChar.abilities.Count; i++){
			AbilityButton instance = (AbilityButton) abilityButton.Instantiate();
			abilityButtonInstances.Add(instance);
			instance.SetPosition(new Vector2(0, -i * instance.Size.Y));

			AbstractAbility ability = ActiveChar.abilities[i];
			abilityListNode.AddChild(instance);
			instance.Ability = ability;
			
			GUIOrchestrator parent = (GUIOrchestrator) GetParent();
			instance.AbilitySelected += (instance) => parent._on_child_ability_selection(instance.Ability);

			instance.MouseEntered += () => CreateAbilityDetailPanel(ability);
			instance.MouseExited += () => DeleteAbilityDetailPanel();
		}
	}

	private void CreateAbilityDetailPanel(AbstractAbility ability){
		AbilityDetailPanel node = (AbilityDetailPanel) abilityDetailPanel.Instantiate();
		AddChild(node);
		node.Ability = ability;
		node.SetPosition(new Vector2(300, 750));

		abilityDetailPanelInstance = node;
	}

	private void DeleteAbilityDetailPanel(){
		RemoveChild(abilityDetailPanelInstance);
	}

	private void UpdateCharacterName(){
		CombatInstance combatInstance = CombatManager.combatInstance;
		if (combatInstance == null) return;
		charName.Text = combatInstance.activeChar.CHAR_NAME;
	}
	
	public virtual void InitSubscriptions(){
		CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_STATE_CHANGE, this, CombatEventPriority.UI);
    }

    public void HandleEvent(CombatEventTurnStart data){
		this.ActiveChar = data.character;
	}
}

using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public partial class ActiveCharInterfaceLayer : Control, IEventSubscriber {

	private Label charName;

	private Control abilityListNode;
	private readonly PackedScene abilityButton = GD.Load<PackedScene>("res://Tactical/UI/AbilityButton.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		CombatManager.combatInstance = new CombatInstance(new TestScenario());
        CombatManager.ChangeCombatState(CombatState.COMBAT_START);        // TODO: Remove, this is for debugging
		_on_ability_list_ready();
		_on_name_ready();

		CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_STATE_CHANGE, this, CombatEventPriority.UI);
	}
	
	public void _on_ability_list_ready(){
		abilityListNode = GetNode<Control>("Ability List");
		UpdateAvailableAbilities();
	}

	public void _on_name_ready(){
		charName = GetNode<Label>("Name");
		UpdateCharacterName();
	}

	private List<Node> abilityButtonInstances = new List<Node>();
	private void UpdateAvailableAbilities(){
		// Remove previous instances from code.
		foreach(Node instance in abilityButtonInstances){
			abilityListNode.RemoveChild(instance);
		}
		abilityButtonInstances.Clear();

		CombatInstance combatInstance = CombatManager.combatInstance;

		if (combatInstance == null) return;
		AbstractCharacter activeChar = combatInstance.activeChar;
		for(int i = 0; i < activeChar.abilities.Count; i++){
			Button instance = (Button) abilityButton.Instantiate();
			abilityButtonInstances.Add(instance);
			instance.SetPosition(new Vector2(0, -i * instance.Size.Y));

			AbstractAbility ability = activeChar.abilities[i];
			instance.Disabled = !ability.IsAvailable || (ability.TYPE == AbilityType.REACTION && CombatManager.combatInstance.combatState != CombatState.AWAITING_CLASH_INPUT);
			instance.Text = ability.NAME;
			instance.Pressed += () => ability.GetValidTargets();
			// instance.Pressed += () => CombatManager.InputAbility(ability, new List<AbstractCharacter>{activeChar});

			abilityListNode.AddChild(instance);
		}
	}

	private void UpdateCharacterName(){
		CombatInstance combatInstance = CombatManager.combatInstance;
		if (combatInstance == null) return;
		charName.Text = combatInstance.activeChar.CHAR_NAME;
	}
	
    public void HandleEvent(CombatEventData data){
		switch (data.eventType){
			case CombatEventType.ON_TURN_START:
				UpdateAvailableAbilities();
				UpdateCharacterName();
				break;
			case CombatEventType.ON_COMBAT_STATE_CHANGE:
				CombatEventCombatStateChanged eventData = (CombatEventCombatStateChanged) data;
				if (eventData.prevState == CombatState.AWAITING_ABILITY_INPUT && eventData.newState == CombatState.AWAITING_CLASH_INPUT){
					UpdateAvailableAbilities();
				}
				break;
		}
	}
}

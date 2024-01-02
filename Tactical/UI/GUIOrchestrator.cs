using System;
using System.Collections.Generic;
using Godot;
using UI;

public partial class GUIOrchestrator : Control, IEventSubscriber, IEventHandler<CombatEventTurnStart>
{	
	public readonly CombatInstance combatData;

	private Label promptTextNode;
	private ActiveCharInterfaceLayer activeCharNode;
	private CombatInterface combatInterfaceNode;

	public GUIOrchestrator(){
		// TODO: Remove, this is for debugging
		CombatManager.combatInstance = new CombatInstance(new TestScenario());
		combatData = CombatManager.combatInstance;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		promptTextNode = GetNode<Label>("Prompt Text");
		activeCharNode = GetNode<ActiveCharInterfaceLayer>("Active Character");
		combatInterfaceNode = GetNode<CombatInterface>("Combat Interface");
		CombatManager.ChangeCombatState(CombatState.COMBAT_START);
	}

	// Connected to in ActiveCharInterfaceLayer.
	// LINK - Tactical\UI\ActiveCharInterfaceLayer.cs:51
	List<CharacterUI> clickableElements = new();
	public void _on_child_ability_selection(AbstractAbility ability){
		promptTextNode.Text = ability.useLaneTargeting ? $"Select a lane for {ability.NAME}" : $"Select a unit for {ability.NAME}";

		// Clear out old clickable elements for cases where a character switches abilities.
		foreach(CharacterUI clickableElement in clickableElements){
			clickableElement.IsClickable = false;
		}
		clickableElements.Clear();

		// Make lanes or character UI elements selectable.
		List<AbstractCharacter> characters = new();
		List<int> lanes = new();
		if (ability.useLaneTargeting){
			// TODO: Make lanes selectable.
			foreach ((int lane, HashSet<AbstractCharacter> _) in ability.GetValidTargets()){
				lanes.Add(lane);
			}
			if (lanes.Count == 0) {
				Logging.Log("No lanes were in range!", Logging.LogLevel.ESSENTIAL);
				return;
			}
		} else {
			foreach ((int _, HashSet<AbstractCharacter> targetsInLane) in ability.GetValidTargets()){
				foreach (AbstractCharacter character in targetsInLane){
					// TODO - Make it obvious which characters can be clicked. Add a shader effect?
					CharacterUI charUI = combatInterfaceNode.characterToNodeMap.GetValueOrDefault(character);
					if (charUI == null) continue;
					clickableElements.Add(charUI);

					charUI.IsClickable = true;		// NOTE - This will automatically set the Area2D collision space as clickable as well.
				}
			}
		}
	}

    public void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
    }

    public void HandleEvent(CombatEventTurnStart eventData){
        promptTextNode.Text = "";
    }
}

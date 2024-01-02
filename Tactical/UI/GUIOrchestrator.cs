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
	private AbstractAbility clickedAbility;
	private List<Lane> clickableLanes = new();
	private List<CharacterUI> clickableChars = new();
	public void _on_child_ability_selection(AbstractAbility ability){
		promptTextNode.Text = ability.useLaneTargeting ? $"Select a lane for {ability.NAME}" : $"Select a unit for {ability.NAME}";

		// Clear out old clickable elements for cases where a character switches abilities.
		foreach(Lane lane in clickableLanes){
			lane.IsClickable = false;
		}
		foreach(CharacterUI clickableElement in clickableChars){
			clickableElement.IsClickable = false;
		}
		clickedAbility = ability;
		clickableLanes.Clear();
		clickableChars.Clear();

		// Make lanes or character UI elements selectable.
		if (ability.useLaneTargeting){
			foreach ((int lane, HashSet<AbstractCharacter> _) in ability.GetValidTargets()){
				Lane laneUI = combatInterfaceNode.laneToNodeMap.GetValueOrDefault(lane);
				if (laneUI == null) continue;

				clickableLanes.Add(laneUI);
				// TODO - Make it more obvious when something is clickable.
				laneUI.IsClickable = true;			// NOTE - This will automatically set the Area2D collision space as clickable as well.
			}
		} else {
			foreach ((int _, HashSet<AbstractCharacter> targetsInLane) in ability.GetValidTargets()){
				foreach (AbstractCharacter character in targetsInLane){
					CharacterUI charUI = combatInterfaceNode.characterToNodeMap.GetValueOrDefault(character);
					if (charUI == null) continue;
					clickableChars.Add(charUI);
	
					// TODO - Make it more obvious when something is clickable.
					charUI.IsClickable = true;		// NOTE - This will automatically set the Area2D collision space as clickable as well.
				}
			}
		}
	}

	public void _on_child_character_selection(AbstractCharacter character){
		if (clickedAbility == null || character == null) return;
		// TODO - This doesn't work with AoE attacks, figure out how to get *that* to work. Maybe we still just do this and have CombatEventManager.AbilityActivated modify the list of fighters post-hoc?
		CombatManager.InputAbility(clickedAbility, new List<AbstractCharacter>{character});
		clickedAbility = null;
	}

	public void _on_child_lane_selection(int lane){
		if (clickedAbility == null) return;
		// TODO - This doesn't work with AoE attacks, figure out how to get *that* to work. Maybe we still just do this and have CombatEventManager.AbilityActivated modify the list of fighters post-hoc?
		CombatManager.InputAbility(clickedAbility, new List<int>{lane});
		clickedAbility = null;
	}

    public void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
    }

    public void HandleEvent(CombatEventTurnStart eventData){
        promptTextNode.Text = "";
    }
}

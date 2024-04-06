using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using UI;

public partial class GUIOrchestrator : Control, IEventSubscriber, IEventHandler<CombatEventRoundStart>, IEventHandler<CombatEventTurnStart>, IEventHandler<CombatEventClashEligible>
{	
	private readonly PackedScene turnNode = GD.Load<PackedScene>("res://Tactical/UI/TurnNode.tscn");
	private TacticalScene tacticalSceneNode;
	private ActiveCharInterfaceLayer activeCharNode;
	private CameraController cameraNode;
	private Label promptTextNode;
	private Label roundCounter;
	private Control turnList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		tacticalSceneNode = GetNode<TacticalScene>("../../");
		activeCharNode = GetNode<ActiveCharInterfaceLayer>("Active Character");

		cameraNode = GetNode<CameraController>("../../Camera2D");
		promptTextNode = GetNode<Label>("Prompt Text");
		roundCounter = GetNode<Label>("Round");
		turnList = GetNode<Control>("Turn List");

		UpdateTurnlist();

		InitSubscriptions();
	}

	// Connected to in ActiveCharInterfaceLayer.
	// LINK - Tactical\UI\ActiveCharInterfaceLayer.cs:51
	private AbstractAbility clickedAbility;
	public async void _on_child_ability_selection(AbstractAbility ability){
		promptTextNode.Text = ability.useLaneTargeting ? $"Select a lane for {ability.NAME}" : $"Select a unit for {ability.NAME}";

		// Clear out old clickable elements for cases where a character switches abilities.
		foreach(Lane lane in tacticalSceneNode.laneToNodeMap.Values){
			lane.IsClickable = false;
		}
		foreach(CharacterUI clickableElement in tacticalSceneNode.characterToNodeMap.Values){
			clickableElement.IsClickable = false;
		}
		clickedAbility = ability;

		// Make lanes or character UI elements selectable.
		if (ability.useLaneTargeting){
			foreach ((int lane, HashSet<AbstractCharacter> _) in ability.GetValidTargets()){
				Lane laneUI = tacticalSceneNode.laneToNodeMap.GetValueOrDefault(lane);
				if (laneUI == null) continue;
				laneUI.IsClickable = true;			// NOTE - This will automatically set the Area2D collision space as clickable as well.
			}
		} else {
			foreach ((int _, HashSet<AbstractCharacter> targetsInLane) in ability.GetValidTargets()){
				foreach (AbstractCharacter character in targetsInLane){
					CharacterUI charUI = tacticalSceneNode.characterToNodeMap.GetValueOrDefault(character);
					if (charUI == null) continue;
					charUI.IsClickable = true;		// NOTE - This will automatically set the Area2D collision space as clickable as well.
				}
			}
		}
		await cameraNode.CinematicZoom(0.05f, 0.25f);
	}

	public void _on_child_clash_selection(AbstractAbility ability){
		clickedAbility = ability;
		CombatManager.InputAbility(clickedAbility, new List<AbstractCharacter>{CombatManager.combatInstance?.activeChar});
	}

	public async void _on_child_character_selection(AbstractCharacter character){
		if (clickedAbility == null || character == null) return;
		// TODO - This doesn't work with AoE attacks, figure out how to get *that* to work. Maybe we still just do this and have CombatEventManager.AbilityActivated modify the list of fighters post-hoc?
		CombatManager.InputAbility(clickedAbility, new List<AbstractCharacter>{character});
		clickedAbility = null;

		foreach (Lane laneUI in tacticalSceneNode.laneToNodeMap.Values){
			laneUI.IsClickable = false;
		}
		foreach (CharacterUI characterUI in tacticalSceneNode.characterToNodeMap.Values){
			characterUI.IsClickable = false;
		}
		await cameraNode.ResetZoom(0.25f);
	}

	public async void _on_child_lane_selection(int lane){
		if (clickedAbility == null) return;
		// TODO - This doesn't work with AoE attacks, figure out how to get *that* to work. Maybe we still just do this and have CombatEventManager.AbilityActivated modify the list of fighters post-hoc?
		if (clickedAbility.requiresUnit){
			List<(int lane, HashSet<AbstractCharacter> targetsInLane)> targeting = clickedAbility.GetValidTargets();
			List<AbstractCharacter> characters = targeting.Where(element => element.lane == lane).First().targetsInLane.ToList();
			CombatManager.InputAbility(clickedAbility, characters);
		} else {
			CombatManager.InputAbility(clickedAbility, new List<int>{lane});
		}
		clickedAbility = null;

		foreach (Lane laneUI in tacticalSceneNode.laneToNodeMap.Values){
			laneUI.IsClickable = false;
		}
		foreach (CharacterUI characterUI in tacticalSceneNode.characterToNodeMap.Values){
			characterUI.IsClickable = false;
		}
		await cameraNode.ResetZoom(0.25f);
	}

	
	private void UpdateTurnlist(){
		CombatInstance combatInstance = CombatManager.combatInstance;
		if (combatInstance == null) return;
		foreach (Node child in turnList.GetChildren().Where(child => child.GetType() == typeof(TurnNode))){
			child.QueueFree();
		}
		for (int i = 0; i < combatInstance.turnlist.GetQueue().Count; i++){
			(AbstractCharacter info, int spd) = combatInstance.turnlist[i];
			TurnNode turnNodeInstance = (TurnNode) turnNode.Instantiate();
			turnList.AddChild(turnNodeInstance);
			turnNodeInstance.Character = tacticalSceneNode.characterToNodeMap[info];
			turnNodeInstance.Speed = spd;
			turnNodeInstance.Position = new Vector2(i * 110, 0);
		}
	}

    public void InitSubscriptions(){
		CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.UI);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_CLASH_ELIGIBLE, this, CombatEventPriority.UI);
    }

	public void HandleEvent(CombatEventRoundStart eventData){
		roundCounter.Text = $"Round\n{eventData.roundStartNum}";
	}

    public void HandleEvent(CombatEventTurnStart eventData){
        promptTextNode.Text = "";
		UpdateTurnlist();
    }

	public void HandleEvent(CombatEventClashEligible eventData){
		promptTextNode.Text = $"{eventData.attacker.CHAR_NAME} attacks {eventData.defender.CHAR_NAME} with {eventData.attackerAbility.NAME}.\nYou may choose an ability to begin a clash.";
	}
}

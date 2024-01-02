using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
namespace UI;

public partial class CombatInterface : Control, IEventSubscriber, IEventHandler<CombatEventCombatStart>, IEventHandler<CombatEventRoundStart>, IEventHandler<CombatEventTurnStart>, IEventHandler<CombatEventCombatStateChanged>, IEventHandler<CombatEventCharacterDeath> {
	private readonly PackedScene characterNode = GD.Load<PackedScene>("res://Tactical/UI/Characters/Character.tscn");
	private readonly PackedScene laneNode = GD.Load<PackedScene>("res://Tactical/UI/Lane.tscn");
	private Label roundCounter;
	private Label turnList;

	public Dictionary<AbstractCharacter, CharacterUI> characterToNodeMap = new();
	public Dictionary<int, Lane> laneToNodeMap = new();

	private GUIOrchestrator parent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		roundCounter = GetNode<Label>("Round");
		turnList = GetNode<Label>("Turn List");
		parent = (GUIOrchestrator) GetParent();

		UpdateRoundText();
		UpdateTurnlistText();
		UpdateCharPositions();

		InitSubscriptions();
	}

	private void UpdateRoundText(){
		CombatInstance combatInstance = CombatManager.combatInstance;
		if (CombatManager.combatInstance != null) {
			roundCounter.Text = $"Round {combatInstance.round}";
		}
	}

	private void UpdateTurnlistText(){
		CombatInstance combatInstance = CombatManager.combatInstance;
		if (combatInstance == null) return;
		string turnlistText = $"Remaining turns: ";
		foreach ((AbstractCharacter info, int spd) in combatInstance.turnlist.GetQueue()){
			turnlistText += $"{info.CHAR_NAME} ({spd}), ";
		}
		turnList.Text = turnlistText;
	}

	private void UpdateCharPositions(){
		foreach (CharacterUI charUI in characterToNodeMap.Values){
			charUI.SetPosition(new Vector2((charUI.Character.Position - 1) * 300, 500));
		}
	}

	public virtual void InitSubscriptions(){
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_CHARACTER_DEATH, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_STATE_CHANGE, this, CombatEventPriority.UI);
    }

	public void HandleEvent(CombatEventCombatStart data){
		for (int i = GameVariables.MIN_LANES; i <= GameVariables.MAX_LANES; i++){
			Lane laneUI = (Lane) laneNode.Instantiate();
			laneUI.position = i;
			laneToNodeMap[i] = laneUI;

			laneUI.LaneSelected += (laneUI) => parent._on_child_lane_selection(laneUI.position);
			laneUI.SetPosition(new Vector2((i - 1) * 275, 500));
			this.AddChild(laneUI);
		}
		
		HashSet<AbstractCharacter> fighters = CombatManager.combatInstance.fighters;
		foreach (AbstractCharacter fighter in fighters){
			CharacterUI charUI = (CharacterUI) characterNode.Instantiate();
			charUI.Character = fighter;
			characterToNodeMap[fighter] = charUI;

			charUI.CharacterSelected += (charUI) => parent._on_child_character_selection(charUI.Character);
			charUI.SetPosition(new Vector2((fighter.Position - 1) * 400, 500));
			this.AddChild(charUI);
		}
	}

    public void HandleEvent(CombatEventRoundStart data){
		UpdateRoundText();
	}

	public void HandleEvent(CombatEventTurnStart data){
		UpdateTurnlistText();
	}

	public void HandleEvent(CombatEventCombatStateChanged data){
		UpdateCharPositions();
	}

	public void HandleEvent(CombatEventCharacterDeath data){
		CharacterUI charUI = characterToNodeMap.GetValueOrDefault(data.deadChar);
		if (charUI == null) return;

		this.RemoveChild(charUI);
		characterToNodeMap.Remove(data.deadChar);
	}
}

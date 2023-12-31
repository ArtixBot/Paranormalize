using Godot;
using System;
using System.Collections.Generic;

public partial class CombatInterface : Control, IEventSubscriber, IEventHandler<CombatEventRoundStart>, IEventHandler<CombatEventTurnStart>, IEventHandler<CombatEventCombatStateChanged> {
	private readonly PackedScene character = GD.Load<PackedScene>("res://Tactical/UI/Characters/Character.tscn");
	private Label roundCounter;
	private Label turnList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		roundCounter = GetNode<Label>("Round");
		turnList = GetNode<Label>("Turn List");

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


	private List<Node> charInstances = new List<Node>();
	private void UpdateCharPositions(){
		foreach(Node instance in charInstances){
			this.RemoveChild(instance);
		}
		charInstances.Clear();
		
		CombatInstance combatInstance = CombatManager.combatInstance;
		if (combatInstance == null) return;

		foreach (AbstractCharacter fighter in combatInstance.fighters){
			CharacterUI x = (CharacterUI) character.Instantiate();
			x.Character = fighter;

			charInstances.Add(x);
			x.SetPosition(new Vector2((fighter.Position - 1) * 300, 500));
			this.AddChild(x);
		}
	}

	public virtual void InitSubscriptions(){
		CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_STATE_CHANGE, this, CombatEventPriority.UI);
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
}

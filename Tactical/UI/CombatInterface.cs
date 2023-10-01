using Godot;
using System;

public partial class CombatInterface : Control, IEventSubscriber {
	private Label roundCounter;
	private Label turnList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_on_round_ready();
		_on_turn_list_ready();

		CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
	}

	public void _on_round_ready(){
		roundCounter = GetNode<Label>("Round");
		UpdateRoundText();
	}

	public void _on_turn_list_ready(){
		turnList = GetNode<Label>("Turn List");
		UpdateTurnlistText();
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

    public void HandleEvent(CombatEventData data){
		switch (data.eventType){
			case CombatEventType.ON_ROUND_START:
				UpdateRoundText();
				break;
			case CombatEventType.ON_TURN_START:
				UpdateTurnlistText();
				break;
		}
	}
}

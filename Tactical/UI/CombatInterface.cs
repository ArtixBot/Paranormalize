using Godot;
using System;
using System.Collections.Generic;

public partial class CombatInterface : Control, IEventSubscriber {
	private readonly PackedScene character = GD.Load<PackedScene>("res://Tactical/UI/Characters/Character.tscn");
	private Label roundCounter;
	private Label turnList;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_on_round_ready();
		_on_turn_list_ready();
		UpdateCharPositions();

		InitSubscriptions();
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

    public void HandleEvent(ref CombatEventData data){
		switch (data.eventType){
			case CombatEventType.ON_ROUND_START:
				UpdateRoundText();
				break;
			case CombatEventType.ON_TURN_START:
				UpdateTurnlistText();
				break;
			case CombatEventType.ON_COMBAT_STATE_CHANGE:
				UpdateCharPositions();
				break;
		}
	}
}

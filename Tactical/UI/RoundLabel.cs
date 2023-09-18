using Godot;
using System;

public partial class RoundLabel : Label, IEventSubscriber {

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.LOWEST_PRIORITY);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public void HandleEvent(CombatEventData data){
		this.Text = $"Round {CombatManager.combatInstance.round}";
	}
}

using System;
using Godot;

public partial class GUIOrchestrator : Control
{	
	public static readonly GUIOrchestrator Instance = new GUIOrchestrator();
	public readonly CombatInstance combatData;

	public GUIOrchestrator(){
		// TODO: Remove, this is for debugging
		CombatManager.combatInstance = new CombatInstance(new TestScenario());
		combatData = CombatManager.combatInstance;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		CombatManager.ChangeCombatState(CombatState.COMBAT_START);
	}
}

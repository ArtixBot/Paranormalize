using Godot;
using System;

public enum CombatState {
	NULL,       // Default state.
	COMBAT_START, COMBAT_END,
	ROUND_START, ROUND_END,
	TURN_START, TURN_END,
	AWAITING_ABILITY_INPUT, AWAITING_CLASH_INPUT,
	RESOLVE_ABILITIES
}

public class CombatInstance {
	public CombatState combatState;
	public int round;

	public CombatInstance(){
		this.combatState = CombatState.NULL;
		this.round = 1;
	}
}

public partial class CombatManager : Node {
	
	public static CombatInstance combatInstance;
	
	public override void _Ready(){
		combatInstance = new CombatInstance();
		GD.Print(combatInstance.round);
	}
}

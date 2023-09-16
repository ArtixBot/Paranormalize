using Godot;
using System;
using System.ComponentModel;

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
	// The event system is more complicated than what signals can provide (especially as signals are intended not to care who is listening to them.)
	// However, in our case, order absolutely matters (e.g. a "revive on death" should absolutely take higher priority over "remove character on death"), so we have a custom system.
	public override void _Ready(){
		combatInstance ??= new CombatInstance();
		ChangeCombatState(CombatState.COMBAT_START);
	}

	public void ChangeCombatState(CombatState newState){
        if (combatInstance.combatState != newState){
            GD.Print($"Combat state changing: {combatInstance.combatState} -> {newState}");
            combatInstance.combatState = newState;
            ResolveCombatState();
        }
    }

	public void ResolveCombatState(){
		switch (combatInstance.combatState){
            case CombatState.COMBAT_START:
                OnCombatStart();
                break;
            case CombatState.COMBAT_END:
                OnCombatEnd();
                break;
            case CombatState.ROUND_START:
                OnRoundStart(combatInstance.round);
                break;
            case CombatState.ROUND_END:
                // RoundEnd();
                break;
            case CombatState.TURN_START:
                // TurnStart();
                break;
            case CombatState.TURN_END:
                // TurnEnd();
                break;
            case CombatState.AWAITING_ABILITY_INPUT:    // This state doesn't do anything by itself, but allows use of InputAbility while at this stage.
                break;
            case CombatState.AWAITING_CLASH_INPUT:      // This state doesn't do anything by itself, but allows use of InputClashReaction while at this stage.
                break;
            case CombatState.RESOLVE_ABILITIES:         // Triggers after AWAITING_ABILITY_INPUT, or (optionally) AWAITING_CLASH_INPUT.
                // ResolveAbilities();
                break;
            default:
                break;
        }
	}

	private void OnCombatStart(){
		GD.Print("OnCombatStart invoked.");
		ChangeCombatState(CombatState.ROUND_START);
	}

	private void OnCombatEnd(){
		GD.Print("OnCombatEnd invoked.");
		combatInstance = null;
	}

	private void OnRoundStart(int round){
		GD.Print($"OnRoundStart invoked, round {round}");
	}

	// private static void CombatStart(){
    //     // EmitSignal(SignalName.CombatStart, combatInstance.round);
    //     ChangeCombatState(CombatState.ROUND_START);
    // }

    // private static void CombatEnd(){
    //     // eventManager.BroadcastEvent(new CombatEventCombatEnd());
    //     // combatData = null;        // Clean up by discarding both the combat data instance and combat event info instance.
    //     // eventManager = null;
    // }

}

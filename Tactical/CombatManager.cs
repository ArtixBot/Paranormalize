using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

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
    public Dictionary<CharacterFaction, List<CharacterInfo>> fighters = new Dictionary<CharacterFaction, List<CharacterInfo>>{
        {CharacterFaction.PLAYER, new List<CharacterInfo>()},
        {CharacterFaction.ALLY, new List<CharacterInfo>()},
        {CharacterFaction.NEUTRAL, new List<CharacterInfo>()},
        {CharacterFaction.ENEMY, new List<CharacterInfo>()}
    };

    public ModdablePriorityQueue<CharacterInfo> turnlist = new ModdablePriorityQueue<CharacterInfo>();
    
    public CharacterInfo activeChar;
    public int activeCharSpd;

	public CombatInstance(ScenarioInfo info){
		this.combatState = CombatState.NULL;
		this.round = 1;

        foreach((CharacterInfo character, int startingPosition) in info.fighters){
            this.fighters[character.CHAR_FACTION].Add(character);
            character.Position = startingPosition;
        }
	}
}

// This is found in TacticalScene.tscn and performs all combat orchestration.
public partial class CombatManager : Node {
	
    public static ScenarioInfo scenarioInfo;
	public static CombatInstance combatInstance;
	// The event system is more complicated than what signals can provide (especially as signals are intended not to care who is listening to them.)
	// However, in our case, order absolutely matters (e.g. a "revive on death" should absolutely take higher priority over "remove character on death"), so we have a custom system.
    public static CombatEventManager eventManager;
    
	public override void _Ready(){

        List<(CharacterInfo, int)> testScenarioFighters = new List<(CharacterInfo, int)>{
            (new CharacterInfo("Player"), 1),
            (new CharacterInfo("Enemy"), 3)
        };

		combatInstance = new CombatInstance(new ScenarioInfo(testScenarioFighters));
        eventManager = new CombatEventManager();

        ChangeCombatState(CombatState.COMBAT_START);        // TODO: Remove, this is for debugging
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
                CombatStart();
                break;
            case CombatState.COMBAT_END:
                CombatEnd();
                break;
            case CombatState.ROUND_START:
                RoundStart();
                break;
            case CombatState.ROUND_END:
                RoundEnd();
                break;
            case CombatState.TURN_START:
                TurnStart();
                break;
            case CombatState.TURN_END:
                TurnEnd();
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

	private void CombatStart(){
        eventManager.BroadcastEvent(new CombatEventCombatStart());
		ChangeCombatState(CombatState.ROUND_START);
	}

	private void CombatEnd(){
        eventManager.BroadcastEvent(new CombatEventCombatEnd());
		combatInstance = null;
        eventManager = null;
	}

	private void RoundStart(){
        foreach (CharacterFaction faction in combatInstance.fighters.Keys){
            foreach (CharacterInfo character in combatInstance.fighters[faction]){
                for (int i = 0; i < character.ActionsPerTurn; i++){
                    combatInstance.turnlist.AddToQueue(character, Rng.RandiRange(character.MinSpd, character.MaxSpd));
                }
            }
        }
        eventManager.BroadcastEvent(new CombatEventRoundStart(combatInstance.round));
        ChangeCombatState(CombatState.TURN_START);
	}

    private void RoundEnd(){
        eventManager.BroadcastEvent(new CombatEventRoundEnd(combatInstance.round));
        combatInstance.round += 1;
        ChangeCombatState(CombatState.ROUND_START);
    }

    private void TurnStart(){
        (combatInstance.activeChar, combatInstance.activeCharSpd) = combatInstance.turnlist.PopNextItem();
        GD.Print($"{combatInstance.activeChar?.CHAR_NAME} is taking their turn.");
        eventManager.BroadcastEvent(new CombatEventTurnStart(combatInstance.activeChar, combatInstance.activeCharSpd));
        ChangeCombatState(CombatState.AWAITING_ABILITY_INPUT);
    }

    private void TurnEnd(){
        eventManager.BroadcastEvent(new CombatEventTurnEnd(combatInstance.activeChar, combatInstance.activeCharSpd));
        if (combatInstance.turnlist.GetNextItem() == (null, 0)){
            (combatInstance.activeChar, combatInstance.activeCharSpd) = combatInstance.turnlist.PopNextItem();
            ChangeCombatState(CombatState.ROUND_END);
        } else {
            ChangeCombatState(CombatState.TURN_START);
        }
    }

}

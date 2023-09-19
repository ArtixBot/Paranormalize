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

    public AbstractAbility activeAbility;
    public List<Die> activeAbilityDice;
    public List<CharacterInfo> activeAbilityTargets;
    public List<int> activeAbilityLanes;

    public AbstractAbility reactAbility;
    public List<Die> reactAbilityDice;

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
        CharacterInfo playerChar = new CharacterInfo("Player");
        playerChar.EquipAbility(new TestAttack());
        playerChar.EquipAbility(new TestReact());
        List<(CharacterInfo, int)> testScenarioFighters = new List<(CharacterInfo, int)>{
            (playerChar, 1),
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
                ResolveAbilities();
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
        GD.Print($"Starting round {combatInstance.round} with {combatInstance.turnlist.Count} actions in the queue.");
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
        GD.Print($"{combatInstance.activeChar?.CHAR_NAME} ({combatInstance.activeCharSpd}) is taking their turn.");
        eventManager.BroadcastEvent(new CombatEventTurnStart(combatInstance.activeChar, combatInstance.activeCharSpd));
        ChangeCombatState(CombatState.AWAITING_ABILITY_INPUT);
    }

    private void TurnEnd(){
        eventManager.BroadcastEvent(new CombatEventTurnEnd(combatInstance.activeChar, combatInstance.activeCharSpd));
        if (combatInstance.turnlist.GetNextItem() == (null, 0)){
            (combatInstance.activeChar, combatInstance.activeCharSpd) = combatInstance.turnlist.PopNextItem();
            GD.Print("No remaining actions on the turnlist.");
            ChangeCombatState(CombatState.ROUND_END);
        } else {
            ChangeCombatState(CombatState.TURN_START);
        }
    }

    // Input unit-targeted abilities.
    // Note that this is a public function so that the UI can call this.
    public void InputAbility(AbstractAbility ability, List<CharacterInfo> targets){
        // Don't do anything if not in AWAITING_ABILITY_INPUT stage, or if no targets were selected.
        if (combatInstance.combatState != CombatState.AWAITING_ABILITY_INPUT || targets.Count == 0){
            return;
        }
        combatInstance.activeAbility = ability;
        combatInstance.activeAbilityDice = ability.BASE_DICE;
        combatInstance.activeAbilityTargets = targets;

        if (ability.TYPE == AbilityType.ATTACK &&
            !ability.HasTag(AbilityTag.AOE) && !ability.HasTag(AbilityTag.DEVIOUS) &&
            combatInstance.turnlist.ContainsItem(targets[0]) &&
            GetEligibleReactions().Count > 0) {
            // If the ability in question is a single-target non-DEVIOUS attack, the defender has a remaining action, and the defender has eligible reactions, change to AWAITING_CLASH_INPUT.
            ChangeCombatState(CombatState.AWAITING_CLASH_INPUT);
        } else {
            ChangeCombatState(CombatState.RESOLVE_ABILITIES);
        }
    }

    /// <summary>
    /// Input lane-targeted abilties. Note that <b>lane-targeted abilities can have no characters on them</b>; this is specifically for ground-based effects.
    /// </summary>
    /// <param name="ability"></param>
    /// <param name="lanes"></param>
    public void InputAbility(AbstractAbility ability, List<int> lanes){
        // Don't do anything if not in AWAITING_ABILITY_INPUT stage.
        if (combatInstance.combatState != CombatState.AWAITING_ABILITY_INPUT){
            return;
        }
        combatInstance.activeAbility = ability;
        combatInstance.activeAbilityDice = ability.BASE_DICE;
        combatInstance.activeAbilityLanes = lanes;

        // Lane-targeted abilities by default are either AoE attack abilities or utility abilities, so no need for clash check.
        ChangeCombatState(CombatState.RESOLVE_ABILITIES);
    }

    public void InputReaction(AbstractAbility ability){
        if (ability != null){
            combatInstance.reactAbility = ability;
            combatInstance.reactAbilityDice = ability.BASE_DICE;
        }
        ChangeCombatState(CombatState.RESOLVE_ABILITIES);
    }

    private void ResolveAbilities(){
        if (combatInstance.reactAbility == null){
            // No clash occurs.
            // ResolveUnopposedAbility();
        }
        if (combatInstance.reactAbility != null){
            // Clash occurs!
            // ResolveClash();
        }
        
        // If the active ability had Cantrip, don't end turn and instead return to AWAITING_ABILITY_INPUT.
        CombatState nextState = combatInstance.activeAbility.HasTag(AbilityTag.CANTRIP) ? CombatState.AWAITING_ABILITY_INPUT : CombatState.TURN_END;
        
        // Cleanup abilities after activation.
        combatInstance.activeAbility = null;
        combatInstance.reactAbility = null;
        ChangeCombatState(nextState);
    }

    private List<AbstractAbility> GetEligibleReactions(){
        int atkLane = combatInstance.activeChar.Position;
        CharacterInfo defender = combatInstance.activeAbilityTargets[0];
        int defLane = defender.Position;
        List<AbstractAbility> availableReactionAbilties = new List<AbstractAbility>();
        foreach (AbstractAbility ability in defender.abilities){ 
            if (!ability.isAvailable || ability.HasTag(AbilityTag.CANNOT_REACT)){
                continue;
            }
            // Available reactions are always eligible for reactions.
            if (ability.TYPE == AbilityType.REACTION){
                availableReactionAbilties.Add(ability);
            }
            // Available single-target attacks are eligible *if* the attacker is in range of the defender's attack.
            if (ability.TYPE == AbilityType.ATTACK && !ability.HasTag(AbilityTag.AOE)){
                int range = Math.Abs(atkLane - defLane);
                if (range >= ability.MIN_RANGE && range <= ability.MAX_RANGE) availableReactionAbilties.Add(ability);
            }
        }
        return availableReactionAbilties;
    }

}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum CombatState {
	NULL,       // Default state.
	COMBAT_START, COMBAT_END,
	ROUND_START, ROUND_END,
	TURN_START, TURN_END,
	AWAITING_ABILITY_INPUT, AWAITING_CLASH_INPUT,
	RESOLVE_ABILITIES, POST_RESOLVE_ABILITIES,
}

public class CombatInstance {
	public CombatState combatState;
	public int round;
    public HashSet<AbstractCharacter> fighters = new HashSet<AbstractCharacter>();
    public ModdablePriorityQueue<AbstractCharacter> turnlist = new ModdablePriorityQueue<AbstractCharacter>();
    
    public AbstractCharacter activeChar {
        get {return turnlist[0].element; }
    }
    public int activeCharSpd {
        get {return turnlist[0].priority; }
    }

    public AbstractAbility activeAbility;
    public List<Die> activeAbilityDice;
    public List<AbstractCharacter> activeAbilityTargets;
    public List<int> activeAbilityLanes;

    public AbstractAbility reactAbility;
    public List<Die> reactAbilityDice;

	public CombatInstance(ScenarioInfo info){
        CombatManager.eventManager = new CombatEventManager();
        CombatEventManager.instance = CombatManager.eventManager;
        
		this.combatState = CombatState.NULL;
		this.round = 1;

        foreach((AbstractCharacter character, int startingPosition) in info.fighters){
            this.fighters.Add(character);
            character.Position = startingPosition;
            foreach(AbstractAbility ability in character.abilities){
                CombatManager.eventManager.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, ability, CombatEventPriority.HIGHEST_PRIORITY);
            }
        }
	}
}

public static class CombatManager {
	
    public static ScenarioInfo scenarioInfo;
	public static CombatInstance combatInstance;
	// The event system is more complicated than what signals can provide (especially as signals are intended not to care who is listening to them.)
	// However, in our case, order absolutely matters (e.g. a "revive on death" should absolutely take higher priority over "remove character on death"), so we have a custom system.
    public static CombatEventManager eventManager;

	public static void ChangeCombatState(CombatState newState){
        if (combatInstance.combatState != newState){
            GD.Print($"Combat state changing: {combatInstance.combatState} -> {newState}");
            combatInstance.combatState = newState;
            eventManager?.BroadcastEvent(new CombatEventCombatStateChanged(combatInstance.combatState, newState));
            ResolveCombatState();
        }
    }

	public static void ResolveCombatState(){
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
                if (combatInstance.activeChar.CHAR_FACTION != CharacterFaction.PLAYER){
                    // TODO: do AI control here, else just wait for player input
                    InputAbility(combatInstance.activeChar.abilities.Where(ability => ability.ID == "PASS").First(), new List<int>());
                }
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

	private static void CombatStart(){
        eventManager.BroadcastEvent(new CombatEventCombatStart());
		ChangeCombatState(CombatState.ROUND_START);
	}

	private static void CombatEnd(){
        eventManager.BroadcastEvent(new CombatEventCombatEnd());
		combatInstance = null;
        eventManager = null;
	}

	private static void RoundStart(){
        foreach (AbstractCharacter character in combatInstance.fighters){
            for (int i = 0; i < character.ActionsPerTurn; i++){
                combatInstance.turnlist.AddToQueue(character, Rng.RandiRange(character.MinSpd, character.MaxSpd));
            }
        }
        GD.Print($"Starting round {combatInstance.round} with {combatInstance.turnlist.Count} actions in the queue.");
        eventManager.BroadcastEvent(new CombatEventRoundStart(combatInstance.round));
        ChangeCombatState(CombatState.TURN_START);
	}

    private static void RoundEnd(){
        eventManager.BroadcastEvent(new CombatEventRoundEnd(combatInstance.round));
        combatInstance.round += 1;
        ChangeCombatState(CombatState.ROUND_START);
    }

    private static void TurnStart(){
        GD.Print($"{combatInstance.activeChar?.CHAR_NAME} ({combatInstance.activeCharSpd}) is taking their turn.");
        eventManager.BroadcastEvent(new CombatEventTurnStart(combatInstance.activeChar, combatInstance.activeCharSpd));
        ChangeCombatState(CombatState.AWAITING_ABILITY_INPUT);
    }

    private static void TurnEnd(){
        eventManager.BroadcastEvent(new CombatEventTurnEnd(combatInstance.activeChar, combatInstance.activeCharSpd));
        combatInstance.turnlist.PopNextItem();
        if (combatInstance.turnlist.GetNextItem() == (null, 0)){
            GD.Print("No remaining actions on the turnlist. Ending round.");
            ChangeCombatState(CombatState.ROUND_END);
        } else {
            ChangeCombatState(CombatState.TURN_START);
        }
    }

    // Input unit-targeted abilities.
    // Note that this is a public function so that the UI can call this.
    public static void InputAbility(AbstractAbility ability, List<AbstractCharacter> targets){
        // Don't do anything if not in AWAITING_ABILITY_INPUT/AWAITING_CLASH_INPUT stage, or if no targets were selected.
        if ((combatInstance.combatState != CombatState.AWAITING_ABILITY_INPUT && combatInstance.combatState != CombatState.AWAITING_CLASH_INPUT) || targets.Count == 0){
            return;
        }
        if (combatInstance.combatState == CombatState.AWAITING_ABILITY_INPUT){
            combatInstance.activeAbility = ability;
            combatInstance.activeAbilityDice = ability.BASE_DICE;
            combatInstance.activeAbilityTargets = targets;

            // If the ability in question is a single-target non-DEVIOUS attack, the defender has a remaining action, and the defender has eligible reactions, change to AWAITING_CLASH_INPUT instead of going to resolve abiltiies.
            if (ability.TYPE == AbilityType.ATTACK &&
                !ability.HasTag(AbilityTag.AOE) && !ability.HasTag(AbilityTag.DEVIOUS) &&
                combatInstance.turnlist.ContainsItem(targets[0]) &&
                GetEligibleReactions().Count > 0) {
                ChangeCombatState(CombatState.AWAITING_CLASH_INPUT);
                return;
            }
        } else if (combatInstance.combatState == CombatState.AWAITING_CLASH_INPUT){
            combatInstance.reactAbility = ability;
            combatInstance.reactAbilityDice = ability?.BASE_DICE;
        }
        ChangeCombatState(CombatState.RESOLVE_ABILITIES);

        
    }

    /// <summary>
    /// Input lane-targeted abilties. Note that <b>lane-targeted abilities can have no characters on them</b>; this is specifically for ground-based effects.
    /// </summary>
    public static void InputAbility(AbstractAbility ability, List<int> lanes){
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

    private static void ResolveAbilities(){
        if (combatInstance.reactAbility == null){
            // No clash occurs.
            ResolveUnopposedAbility();
        }
        if (combatInstance.reactAbility != null){
            // Clash occurs!
            ResolveClash();
        }
        // If the active ability had Cantrip, don't end turn and instead return to AWAITING_ABILITY_INPUT.
        CombatState nextState = combatInstance.activeAbility.HasTag(AbilityTag.CANTRIP) ? CombatState.AWAITING_ABILITY_INPUT : CombatState.TURN_END;
        
        // Cleanup abilities after activation.
        combatInstance.activeAbility = null;
        combatInstance.activeAbilityDice = null;
        combatInstance.activeAbilityLanes = null;
        combatInstance.activeAbilityTargets = null;
        combatInstance.reactAbility = null;
        combatInstance.reactAbilityDice = null;
        ChangeCombatState(nextState);
    }

    private static void ResolveUnopposedAbility(){
        // Check whether to emit a unit-targeted ABILITY_ACTIVATED event or a lane-targeted ABILITY_ACTIVATED event.
        // Only necessary in ResolveUnopposedAbility since lane-target abilities are unclashable.
        // TODO: redo targeting
        List<AbstractCharacter> charTargeting = combatInstance.activeAbilityTargets;
        List<int> laneTargeting = combatInstance.activeAbilityLanes;
        if (charTargeting != null){
            eventManager.BroadcastEvent(new CombatEventAbilityActivated(combatInstance.activeChar, combatInstance.activeAbility, ref combatInstance.activeAbilityDice, charTargeting));
        } else if (laneTargeting != null){
            eventManager.BroadcastEvent(new CombatEventAbilityActivated(combatInstance.activeChar, combatInstance.activeAbility, ref combatInstance.activeAbilityDice, laneTargeting));
        }

        // Use a while loop since additional dice can be tossed into the queue during combat processing (e.g. die has "Cycle" effect).
        int i = 0;      // There should never be more than 100 iterations but if somehow there were an infinite Cycle loop, this should break that.
        while (combatInstance.activeAbilityDice.Count > 0 && i < 100){
            Die die = combatInstance.activeAbilityDice[0];
            int dieRoll = die.Roll();
            eventManager.BroadcastEvent(new CombatEventDieRolled(die, ref dieRoll));

            combatInstance.activeAbilityDice.RemoveAt(0);
            i += 1;
        }
    }

    private static void ResolveClash(){
        eventManager.BroadcastEvent(new CombatEventAbilityActivated(combatInstance.activeChar, combatInstance.activeAbility, ref combatInstance.activeAbilityDice, combatInstance.activeAbilityTargets));
        eventManager.BroadcastEvent(new CombatEventAbilityActivated(combatInstance.activeAbilityTargets[0], combatInstance.reactAbility, ref combatInstance.reactAbilityDice, combatInstance.activeChar));
        eventManager.BroadcastEvent(new CombatEventClashOccurs(combatInstance.activeChar, 
                                                               combatInstance.activeAbility, 
                                                               ref combatInstance.activeAbilityDice, 
                                                               combatInstance.activeAbilityTargets[0], 
                                                               combatInstance.reactAbility, 
                                                               ref combatInstance.reactAbilityDice));
    }

    private static List<AbstractAbility> GetEligibleReactions(){
        int atkLane = combatInstance.activeChar.Position;
        AbstractCharacter defender = combatInstance.activeAbilityTargets[0];
        int defLane = defender.Position;
        List<AbstractAbility> availableReactionAbilties = new List<AbstractAbility>();
        foreach (AbstractAbility ability in defender.abilities){ 
            if (!ability.IsAvailable || ability.HasTag(AbilityTag.CANNOT_REACT)){
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

    private static void ResolveCombatantDeath(){

    }
}

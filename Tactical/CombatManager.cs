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
	RESOLVE_ABILITIES
}

public class CombatInstance {
	public CombatState combatState;
	public int round;
    public HashSet<AbstractCharacter> fighters = new HashSet<AbstractCharacter>();
    public ModdablePriorityQueue<AbstractCharacter> turnlist = new ModdablePriorityQueue<AbstractCharacter>();
    
    public AbstractCharacter activeChar;
    public int activeCharSpd;

    public AbstractAbility activeAbility;
    public List<Die> activeAbilityDice;
    public List<AbstractCharacter> activeAbilityTargets;
    public List<int> activeAbilityLanes;

    public AbstractAbility reactAbility;
    public List<Die> reactAbilityDice;

    // TODO: Consider making this an combat event property instead of directly accessing this value.
    public int abilityItrCount;     // Used only for UI purposes to determine when to play events like push/pull/forward/back, which could occur on later dice.

	public CombatInstance(ScenarioInfo info){
        CombatManager.eventManager = new CombatEventManager();
        CombatEventManager.instance = CombatManager.eventManager;
        
		this.combatState = CombatState.NULL;
		this.round = 1;

        foreach((AbstractCharacter character, int startingPosition) in info.fighters){
            this.fighters.Add(character);
            character.Position = startingPosition;
            character.InitSubscriptions();
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
            Logging.Log($"Combat state changing: {combatInstance.combatState} -> {newState}", Logging.LogLevel.INFO);
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
            // This state doesn't do anything by itself, but allows use of InputAbility while at this stage.
            case CombatState.AWAITING_ABILITY_INPUT:
                if (combatInstance.activeChar.CHAR_FACTION != CharacterFaction.PLAYER){
                    if (combatInstance.activeChar.Behavior == null){        // This should never happen.
                        InputAbility(combatInstance.activeChar.abilities.FirstOrDefault(ability => ability.ID == "PASS"), new List<AbstractCharacter>{combatInstance.activeChar});
                        break;
                    }
                    combatInstance.activeChar.Behavior.DecideAbilityToUse();
                }
                break;
            // This state doesn't do anything by itself, but allows use of InputAbility while at this stage.
            case CombatState.AWAITING_CLASH_INPUT:
                if (combatInstance.activeChar.CHAR_FACTION == CharacterFaction.PLAYER){
                    // AI chooses an ability (since activeChar was the player).
                    AbstractCharacter ai = combatInstance.activeAbilityTargets.First();
                    List<AbstractAbility> reactableAbilities = CombatManager.GetEligibleReactions(ai);
                    
                    // Get unit's first intended reaction intent. If there's nothing, don't clash.
                    AbstractAbility reactionIntent = ai.Behavior?.reactions.FirstOrDefault();
                    if (reactableAbilities.Contains(reactionIntent)){
                        InputAbility(reactionIntent, new List<AbstractCharacter>{combatInstance.activeChar});
                    } else {
                        InputAbility(null, new List<AbstractCharacter>{combatInstance.activeChar});
                    }
                    // Ai then moves to next reaction intent.
                    try {
                        ai.Behavior.reactions.RemoveAt(0);
                    } catch (Exception) {
                        Logging.Log("AI unit had no remaining reactions to remove.", Logging.LogLevel.ESSENTIAL);
                    }
                } else {
                    // Player chooses an ability (since activeChar was AI).
                    AbstractCharacter defender = combatInstance.activeAbilityTargets.First();
                    eventManager.BroadcastEvent(new CombatEventClashEligible(
                        attacker: combatInstance.activeChar, 
                        attackerAbility: combatInstance.activeAbility,
                        defender: defender,
                        reactableAbilities: CombatManager.GetEligibleReactions(defender)
                    ));
                }
                break;
            case CombatState.RESOLVE_ABILITIES:         // Triggers after AWAITING_ABILITY_INPUT or AWAITING_CLASH_INPUT.
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
        bool enemiesRemaining = combatInstance.fighters.Where(fighter => fighter.CHAR_FACTION == CharacterFaction.ENEMY).ToHashSet().Count > 0;

        bool playerVictory;
        // If all players and enemies are dead at once, you still win!
        if (!enemiesRemaining) {
            GD.Print("Players win!");
            playerVictory = true;
        } else {
            GD.Print("Enemies win!");
            playerVictory = false;
        }

        eventManager.BroadcastEvent(new CombatEventCombatEnd(playerVictory));
		combatInstance = null;
        eventManager = null;
	}

	private static void RoundStart(){
        foreach (AbstractCharacter character in combatInstance.fighters){
            for (int i = 0; i < character.ActionsPerTurn; i++){
                combatInstance.turnlist.AddToQueue(character, Rng.RandiRange(character.MinSpd, character.MaxSpd));
            }
            if (character.CHAR_FACTION != CharacterFaction.PLAYER){
                character.Behavior?.DecideReactions();
            }
        }
        Logging.Log($"Starting round {combatInstance.round} with {combatInstance.turnlist.Count} actions in the queue.", Logging.LogLevel.INFO);
        eventManager.BroadcastEvent(new CombatEventRoundStart(combatInstance.round));
        ChangeCombatState(CombatState.TURN_START);
	}

    private static void RoundEnd(){
        eventManager.BroadcastEvent(new CombatEventRoundEnd(combatInstance.round));
        combatInstance.round += 1;
        ChangeCombatState(CombatState.ROUND_START);
    }

    private static void TurnStart(){
        combatInstance.activeChar = combatInstance.turnlist[0].element;
        combatInstance.activeCharSpd = combatInstance.turnlist[0].priority;
        Logging.Log($"{combatInstance.activeChar?.CHAR_NAME} ({combatInstance.activeCharSpd}) is taking their turn. They have {combatInstance.activeChar.CountAvailableAbilities()} available abilities.", Logging.LogLevel.INFO);
        eventManager.BroadcastEvent(new CombatEventTurnStart(combatInstance.activeChar, combatInstance.activeCharSpd));
        ChangeCombatState(CombatState.AWAITING_ABILITY_INPUT);
    }

    private static void TurnEnd(){
        eventManager.BroadcastEvent(new CombatEventTurnEnd(combatInstance.activeChar, combatInstance.activeCharSpd));
        // If the active character is staggered, their actions are automatically removed from the turn queue, so only pop if the active character is not staggered.
        if (combatInstance.turnlist.GetNextItem().element == combatInstance.activeChar){
            combatInstance.turnlist.PopNextItem();
        }
        if (combatInstance.turnlist.GetNextItem() == (null, 0)){
            Logging.Log("No remaining actions on the turnlist. Ending round.", Logging.LogLevel.INFO);
            ChangeCombatState(CombatState.ROUND_END);
        } else {
            ChangeCombatState(CombatState.TURN_START);
        }
    }

    // Input unit-targeted abilities.
    // Note that this is a public function so that the UI can call this.
    public static void InputAbility(AbstractAbility ability, List<AbstractCharacter> targets){
        if (combatInstance.combatState != CombatState.AWAITING_ABILITY_INPUT && combatInstance.combatState != CombatState.AWAITING_CLASH_INPUT) return;
        if (targets == null || targets.Count == 0) return;
        // A null ability *can* be passed, but *only* should happen during AWAITING_CLASH_INPUT.
        if (combatInstance.combatState == CombatState.AWAITING_ABILITY_INPUT && ability == null) return;

        if (combatInstance.combatState == CombatState.AWAITING_ABILITY_INPUT){
            combatInstance.activeAbility = ability;
            combatInstance.activeAbilityDice = ability.BASE_DICE;
            combatInstance.activeAbilityTargets = targets;

            // Check for clash.
            if (ability.TYPE == AbilityType.ATTACK &&
                !ability.HasTag(AbilityTag.AOE) &&
                !ability.HasTag(AbilityTag.DEVIOUS) &&
                targets.Count == 1 &&           // Note: this should be redundant with !ability.HasTag(AbilityTag.AOE).
                combatInstance.turnlist.ContainsItem(targets[0]) &&
                GetEligibleReactions(targets[0]).Count > 0) {
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
    /// Input lane-targeted abilties. Lane-targeted abilties are intrinsically area-of-effect and therefore cannot clash or be clashed against.
    /// </summary>
    public static void InputAbility(AbstractAbility ability, List<int> lanes){
        // Don't do anything if not in AWAITING_ABILITY_INPUT stage.
        if (ability == null || combatInstance.combatState != CombatState.AWAITING_ABILITY_INPUT){
            return;
        }
        combatInstance.activeAbility = ability;
        combatInstance.activeAbilityDice = ability.BASE_DICE;
        combatInstance.activeAbilityLanes = lanes;

        // Lane-targeted abilities by default are either AoE attack abilities or utility abilities, so no need for clash check.
        ChangeCombatState(CombatState.RESOLVE_ABILITIES);
    }

    private static void ResolveAbilities(){
        // No clash occurs.
        if (combatInstance.reactAbility == null){
            // TODO: redo targeting?
            List<AbstractCharacter> charTargeting = combatInstance.activeAbilityTargets;
            List<int> laneTargeting = combatInstance.activeAbilityLanes;
            // Check whether to emit a unit-targeted ABILITY_ACTIVATED event or a lane-targeted ABILITY_ACTIVATED event.
            if (charTargeting != null){
                eventManager.BroadcastEvent(new CombatEventAbilityActivated(combatInstance.activeChar, combatInstance.activeAbility, combatInstance.activeAbilityDice, charTargeting));
            } else if (laneTargeting != null){
                eventManager.BroadcastEvent(new CombatEventAbilityActivated(combatInstance.activeChar, combatInstance.activeAbility, combatInstance.activeAbilityDice, laneTargeting));
            }

            ResolveUnopposedAbility();
        }
        // Clash occurs!
        if (combatInstance.reactAbility != null){
            AbstractCharacter attacker = combatInstance.activeChar;
            AbstractCharacter defender = combatInstance.activeAbilityTargets.First();
            eventManager.BroadcastEvent(new CombatEventAbilityActivated(attacker, combatInstance.activeAbility, combatInstance.activeAbilityDice, defender));
            eventManager.BroadcastEvent(new CombatEventAbilityActivated(defender, combatInstance.reactAbility, combatInstance.reactAbilityDice, attacker));
            eventManager.BroadcastEvent(new CombatEventClashOccurs(attacker, 
                                                                    combatInstance.activeAbility, 
                                                                    combatInstance.activeAbilityDice, 
                                                                    defender, 
                                                                    combatInstance.reactAbility, 
                                                                    combatInstance.reactAbilityDice));
            ResolveClash();

            // TODO - Should this be done here? Normally the active character's turn is removed in TurnEnd, but the reacter hasn't reached it, hence why this code is here for now.
            if (!combatInstance.reactAbility.HasTag(AbilityTag.CANTRIP)){
                combatInstance.turnlist.RemoveNextInstanceOfItem(combatInstance.reactAbility.OWNER);
            }
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
        combatInstance.abilityItrCount = 0;
        ChangeCombatState(nextState);
    }

    private static void ResolveUnopposedAbility(){
        // Use a while loop since additional dice can be tossed into the queue during combat processing (e.g. die has "Cycle" effect).
        int i = 0;      // There should never be more than 100 iterations but if somehow there were an infinite Cycle loop, this should break that.
        while (combatInstance.activeAbilityDice?.Count > 0 && i < 100){
            combatInstance.abilityItrCount += 1;
            Die die = combatInstance.activeAbilityDice[0];
            int dieRoll = die.Roll();
            Logging.Log($"{combatInstance.activeAbility.OWNER.CHAR_NAME} rolls a(n) {die.DieType} die (range: {die.MinValue} - {die.MaxValue}, natural roll: {dieRoll}).", Logging.LogLevel.ESSENTIAL);
            
            foreach (AbstractCharacter target in combatInstance.activeAbilityTargets.ToList()){
                int modifiedRoll = eventManager.BroadcastEvent(new CombatEventDieRolled(combatInstance.activeAbility, die, dieRoll, target)).rolledValue;
                ResolveDieRoll(combatInstance.activeAbility.OWNER, target, die, dieRoll, modifiedRoll, rolledDuringClash: false, losingDieWasAttack: false);
            }

            try {
                // An exception can occur if all targets in combatInstance.activeAbilityTargets are removed from combat, as this will preemptively remove all dice from activeAbilityDice.
                combatInstance.activeAbilityDice.RemoveAt(0);
            } catch (ArgumentOutOfRangeException){
                GD.Print("Attempted to remove dice at position zero but no dice existed, as all targets were staggered/died and thus dice were preemptively removed.");
            }
            i += 1;
        }

        i = 0;
        while (combatInstance.reactAbilityDice?.Count > 0 && i < 100){
            combatInstance.abilityItrCount += 1;
            Die die = combatInstance.reactAbilityDice[0];
            int dieRoll = die.Roll();
            Logging.Log($"{combatInstance.reactAbility.OWNER.CHAR_NAME} rolls a(n) {die.DieType} die (range: {die.MinValue} - {die.MaxValue}, natural roll: {dieRoll}).", Logging.LogLevel.ESSENTIAL);
            int modifiedRoll = eventManager.BroadcastEvent(new CombatEventDieRolled(combatInstance.reactAbility, die, dieRoll, combatInstance.activeChar)).rolledValue;

            GD.Print($"{combatInstance.reactAbility.OWNER.CHAR_NAME} targets {combatInstance.activeChar.CHAR_NAME}");
            ResolveDieRoll(combatInstance.reactAbility.OWNER, combatInstance.activeChar, die, dieRoll, modifiedRoll, rolledDuringClash: false, losingDieWasAttack: false);

            try {
                // An exception can occur if the attacker is killed during unopposed ability resolution, as this will preemptively remove all dice from reactAbilityDice.
                combatInstance.reactAbilityDice.RemoveAt(0);
            } catch (ArgumentOutOfRangeException){
                GD.Print("Attempted to remove dice at position zero but no dice existed, as the attacker was staggered/died and thus dice were preemptively removed.");
            }
            i += 1;
        }
    }

    private static void ResolveDieRoll(AbstractCharacter roller, AbstractCharacter target, Die die, int naturalRoll, int actualRoll, bool rolledDuringClash, bool losingDieWasAttack){
        switch (die.DieType){
            case DieType.SLASH:
                CombatManager.ExecuteAction(new DamageAction(roller, target, DamageType.SLASH, actualRoll, isPoiseDamage: false));
                CombatManager.ExecuteAction(new DamageAction(roller, target, DamageType.SLASH, actualRoll, isPoiseDamage: true));
                eventManager.BroadcastEvent(new CombatEventDieHit(roller, target, die, naturalRoll, actualRoll));
                break;
            case DieType.PIERCE:
                CombatManager.ExecuteAction(new DamageAction(roller, target, DamageType.PIERCE, actualRoll, isPoiseDamage: false));
                CombatManager.ExecuteAction(new DamageAction(roller, target, DamageType.PIERCE, actualRoll, isPoiseDamage: true));
                eventManager.BroadcastEvent(new CombatEventDieHit(roller, target, die, naturalRoll, actualRoll));
                break;
            case DieType.BLUNT:
                CombatManager.ExecuteAction(new DamageAction(roller, target, DamageType.BLUNT, actualRoll, isPoiseDamage: false));
                CombatManager.ExecuteAction(new DamageAction(roller, target, DamageType.BLUNT, actualRoll, isPoiseDamage: true));
                eventManager.BroadcastEvent(new CombatEventDieHit(roller, target, die, naturalRoll, actualRoll));
                break;
            case DieType.ELDRITCH:
                CombatManager.ExecuteAction(new DamageAction(roller, target, DamageType.ELDRITCH, actualRoll, isPoiseDamage: false));
                CombatManager.ExecuteAction(new DamageAction(roller, target, DamageType.ELDRITCH, actualRoll, isPoiseDamage: true));
                eventManager.BroadcastEvent(new CombatEventDieHit(roller, target, die, naturalRoll, actualRoll));
                break;
            case DieType.BLOCK:
                if (!rolledDuringClash){
                    eventManager.BroadcastEvent(new CombatEventDieEvaded(roller, target, die, naturalRoll, actualRoll, rolledDuringClash: false));
                    break;
                };
                CombatManager.ExecuteAction(new DamageAction(roller, target, DamageType.PURE, actualRoll, isPoiseDamage: true));
                eventManager.BroadcastEvent(new CombatEventDieBlocked(roller, target, die, naturalRoll, actualRoll, rolledDuringClash: true));
                break;
            case DieType.EVADE:
                if (!rolledDuringClash){
                    eventManager.BroadcastEvent(new CombatEventDieEvaded(roller, target, die, naturalRoll, actualRoll, rolledDuringClash: false));
                    break;
                };
                CombatManager.ExecuteAction(new RestoreAction(roller, actualRoll, RestoreAction.RestoreType.POISE));
                eventManager.BroadcastEvent(new CombatEventDieEvaded(roller, target, die, naturalRoll, actualRoll, rolledDuringClash: true));
                if (losingDieWasAttack) CombatManager.CycleDie(roller, die);
                break;
            case DieType.UNIQUE:
            default:
                break;
        }
    }

    private static void ResolveClash(){
        int i = 0;      // There should never be more than 100 iterations but if somehow there were an infinite Cycle loop, this should break that.
        Logging.Log($"Clash between {combatInstance.activeAbility.NAME} and {combatInstance.reactAbility.NAME}!", Logging.LogLevel.ESSENTIAL);
        while (combatInstance.activeAbilityDice.Count > 0 && combatInstance.reactAbilityDice.Count > 0 && i < 100){
            combatInstance.abilityItrCount += 1;

            Die atkDie = combatInstance.activeAbilityDice[0], reactDie = combatInstance.reactAbilityDice[0];
            int natAtkRoll = atkDie.Roll(), natReactRoll = reactDie.Roll();

            Logging.Log($"Clash {i+1}:" +
            $"\n\t{combatInstance.activeAbility.OWNER.CHAR_NAME}: {atkDie.DieType} die (range {atkDie.MinValue} - {atkDie.MaxValue}, roll: {natAtkRoll})" +
            $"\n\t{combatInstance.reactAbility.OWNER.CHAR_NAME}: {reactDie.DieType} die (range {reactDie.MinValue} - {reactDie.MaxValue}, roll: {natReactRoll}))", Logging.LogLevel.ESSENTIAL);

            // Reassign to atkRoll/reactRoll to handle any changes in dice power.
            int modAtkRoll = eventManager.BroadcastEvent(new CombatEventDieRolled(combatInstance.activeAbility, atkDie, natAtkRoll, combatInstance.reactAbility.OWNER)).rolledValue;
            int modReactRoll = eventManager.BroadcastEvent(new CombatEventDieRolled(combatInstance.reactAbility, reactDie, natReactRoll, combatInstance.activeChar)).rolledValue;

            CombatEventDieClash dieClashData = eventManager.BroadcastEvent(new CombatEventDieClash(combatInstance.activeAbility, atkDie, modAtkRoll, combatInstance.reactAbility, reactDie, modReactRoll));
            modAtkRoll = dieClashData.attackerRoll;
            modReactRoll = dieClashData.reactRoll;

            // On tie, remove both dice.
            if (modAtkRoll == modReactRoll){
                combatInstance.abilityItrCount += 1;
                eventManager.BroadcastEvent(new CombatEventClashTie(combatInstance.activeAbility.OWNER, combatInstance.reactAbility.OWNER, atkDie, reactDie, modAtkRoll));
                try {
                    combatInstance.activeAbilityDice.RemoveAt(0);
                } catch (ArgumentOutOfRangeException){
                    GD.Print("Attempted to remove attacker dice at position zero, but dice were preemptively removed since either the attacker was staggered or the defender was killed.");
                }
                try {
                    combatInstance.reactAbilityDice.RemoveAt(0);
                } catch (ArgumentOutOfRangeException){
                    GD.Print("Attempted to remove defender dice at position zero, but dice were preemptively removed since either the defender was staggered or the attacker was killed.");
                }
                i += 1;
                continue;
            }

            Die winningDie = (modAtkRoll > modReactRoll) ? atkDie : reactDie;
            Die losingDie = (modAtkRoll > modReactRoll) ? reactDie : atkDie;

            int winningNatRoll = (modAtkRoll > modReactRoll) ? natAtkRoll : natReactRoll;
            int winningRoll = (modAtkRoll > modReactRoll) ? modAtkRoll : modReactRoll;

            int losingRoll = (modAtkRoll > modReactRoll) ? modReactRoll : modAtkRoll;

            AbstractCharacter winningChar = (modAtkRoll > modReactRoll) ? combatInstance.activeChar : combatInstance.activeAbilityTargets.First();
            AbstractCharacter losingChar = (modAtkRoll > modReactRoll) ? combatInstance.activeAbilityTargets.First() : combatInstance.activeChar;

            // If the losing die was a block die, and the winning die was an attack die, reduce the winning roll by the losing roll.
            if (winningDie.IsAttackDie && losingDie.DieType == DieType.BLOCK) {
                winningRoll -= losingRoll;
            }
            CombatEventClashComplete data = eventManager.BroadcastEvent(new CombatEventClashComplete(winningChar, losingChar, winningDie, winningRoll, losingDie, losingRoll));
            data.winningRoll = Math.Max(data.winningRoll, 0);        // Cannot have dice values < 0.
            data.losingRoll = Math.Max(data.losingRoll, 0);

            ResolveDieRoll(data.winningClasher, data.losingClasher, data.winningDie, winningNatRoll, data.winningRoll, rolledDuringClash: true, losingDieWasAttack: losingDie.IsAttackDie);
            try {
                combatInstance.activeAbilityDice.RemoveAt(0);
            } catch (ArgumentOutOfRangeException){
                GD.Print("Attempted to remove attacker dice at position zero, but dice were preemptively removed since either the attacker was staggered or the defender was killed.");
            }
            try {
                combatInstance.reactAbilityDice.RemoveAt(0);
            } catch (ArgumentOutOfRangeException){
                GD.Print("Attempted to remove defender dice at position zero, but dice were preemptively removed since either the defender was staggered or the attacker was killed.");
            }

            i += 1;
        }
        ResolveUnopposedAbility();      // After one of the two clashing die queues is empty, just invoke ResolveUnopposedAbility.
    }

    private static List<AbstractAbility> GetEligibleReactions(AbstractCharacter defender){
        int atkLane = combatInstance.activeChar.Position;
        int defLane = defender.Position;
        List<AbstractAbility> availableReactionAbilties = new List<AbstractAbility>();
        foreach (AbstractAbility ability in defender.abilities){ 
            if (!ability.IsActivatable || ability.HasTag(AbilityTag.AOE) || ability.HasTag(AbilityTag.CANNOT_REACT) || (ability.TYPE != AbilityType.ATTACK && ability.TYPE != AbilityType.REACTION)){
                continue;
            }
            // Available reactions are always eligible for reactions.
            if (ability.TYPE == AbilityType.REACTION){
                availableReactionAbilties.Add(ability);
            }
            // Available single-target attacks are eligible *if* the attacker is in range of the defender's attack.
            if (ability.TYPE == AbilityType.ATTACK){
                int range = Math.Abs(atkLane - defLane);
                if (range >= ability.MIN_RANGE && range <= ability.MAX_RANGE) availableReactionAbilties.Add(ability);
            }
        }
        return availableReactionAbilties;
    }

    public static void ExecuteAction(AbstractAction action){
        action.Execute();
    }

    /// <summary>
    /// Helper method to cycle a die for a character.
    /// </summary>
    public static void CycleDie(AbstractCharacter character, Die dieToCycle){
        if (character == combatInstance.activeChar && combatInstance.activeAbilityDice != null){
            combatInstance.activeAbilityDice.Insert(0, dieToCycle);
        } else if (combatInstance.reactAbilityDice != null && combatInstance.activeAbilityTargets.Contains(character)){
            combatInstance.reactAbilityDice.Insert(0, dieToCycle);
        }
    }

    public static List<Die> GetDieQueueFromCharacter(AbstractCharacter character){
        if (character == combatInstance.activeChar){
            return combatInstance.activeAbilityDice;
        } else if (combatInstance.reactAbilityDice != null && combatInstance.activeAbilityTargets.Contains(character)){
            return combatInstance.reactAbilityDice;
        } else {
            return new();
        }
    }
}

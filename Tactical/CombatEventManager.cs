using Godot;
using System;
using System.Collections.Generic;

public interface IEventSubscriber {
    public abstract void HandleEvent(ref CombatEventData eventData);

    /// <summary>
    /// An IEventSubscriber should override InitSubscriptions to begin subscribing to all relevant events.
    /// Note that unsubscribing can be done with CombatEventManager.instance.UnsubscribeAll(this).
    /// </summary>
    public abstract void InitSubscriptions();
}

public enum CombatEventType {
    ON_COMBAT_STATE_CHANGE,
    ON_COMBAT_START, ON_COMBAT_END,
    ON_ROUND_START, ON_ROUND_END,
    ON_TURN_START, ON_TURN_END,
    ON_ABILITY_ACTIVATED,
    // Attacks will always trigger ON_DEAL_DAMAGE, then ON_TAKE_DAMAGE. Status effects like burn/bleed will only trigger ON_TAKE_DAMAGE.
    ON_DEAL_DAMAGE, ON_TAKE_DAMAGE,
    ON_CHARACTER_DEATH,
    ON_DIE_ROLLED, ON_PRE_HIT,
    ON_CLASH_ELIGIBLE, ON_CLASH, ON_CLASH_TIE, ON_CLASH_WIN, ON_CLASH_LOSE,
    ON_UNIT_MOVED,
    ON_STATUS_APPLIED, ON_STATUS_EXPIRED,
}

// Higher numbers execute first.
public enum CombatEventPriority {
    HIGHEST_PRIORITY = 99999,       // e.g. "revive on death", "the first time you take fatal damage, go to 1 HP instead", invulnerability effects.
    DICE_MODIFICATION = 700,        // Adding/removing dice from a dice queue occur first.
    DICE_CONVERSION = 600,          // After all dice queue modifications finish, perform dice conversions (e.g. converting Evade -> Attack die, or Attack -> Block).
    BASE_ADDITIVE = 500,            // Additive modifiers to a value. These happen first and therefore have an outsized effect on multipliers.
    BASE_MULTIPLICATIVE = 400,      // Multiplicative multipliers to a value.
    STANDARD = 200,
    UI = 1,                         // UI updates should only update after all other calculations are complete.
}

public partial class CombatEventManager{
    // When loading a new combat scenario, a new CombatEventManager instance is created during CombatInstance's constructor, and the instance is assigned to this.
    public static CombatEventManager instance;
    public Dictionary<CombatEventType, ModdablePriorityQueue<IEventSubscriber>> events;

    public CombatEventManager() {
        this.events = new Dictionary<CombatEventType, ModdablePriorityQueue<IEventSubscriber>>();
        foreach (CombatEventType type in Enum.GetValues(typeof(CombatEventType))){
            events.Add(type, new ModdablePriorityQueue<IEventSubscriber>());
        }
    }

    public bool Subscribe(CombatEventType eventType, IEventSubscriber subscriber, CombatEventPriority priority){
        if (!events.ContainsKey(eventType)){
            events.Add(eventType, new ModdablePriorityQueue<IEventSubscriber>());
        }
        if (!events[eventType].ContainsItem(subscriber)){
            events[eventType].AddToQueue(subscriber, (int)priority);
            return true;
        }
        return false;
    }

    // This invocation allows for custom priority values.
    public bool Subscribe(CombatEventType eventType, IEventSubscriber subscriber, int priority){
        if (!events.ContainsKey(eventType)){
            events.Add(eventType, new ModdablePriorityQueue<IEventSubscriber>());
        }
        if (!events[eventType].ContainsItem(subscriber)){
            events[eventType].AddToQueue(subscriber, priority);
            return true;
        }
        return false;
    }

    // Notify all subscribers of a specified event. Subscribers will execute in order.
    public void BroadcastEvent(CombatEventData eventData){
        if (!events.ContainsKey(eventData.eventType)) return;
        Logging.Log($"CombatEventManager broadcasting event {eventData.eventType} to {events[eventData.eventType].GetQueue().Count} subscribers.", Logging.LogLevel.DEBUG);
        int i = 0;
        List<(IEventSubscriber subscriber, int priority)> eventSubscribers = events[eventData.eventType].GetQueue();
        while (i < eventSubscribers.Count){
            eventSubscribers[i].subscriber.HandleEvent(ref eventData);
            i += 1;
        }
    }

    // Return true if the subscriber is in the events dictionary for that event type, false otherwise.
    public bool CheckIfSubscribed(CombatEventType eventType, IEventSubscriber subscriber){
        if (!events.ContainsKey(eventType)) {return false;}
        return events[eventType].ContainsItem(subscriber);
    }

    // Remove a subscriber from a specified event type from the event dictionary.
    public void Unsubscribe(CombatEventType eventType, IEventSubscriber subscriber){
        if (!events.ContainsKey(eventType)){ return; }
        events[eventType].RemoveAllInstancesOfItem(subscriber);
    }

    // Remove the provided subscriber from all events. Used in instances such as when a character is defeated.
    public void UnsubscribeAll(IEventSubscriber subscriber){
        foreach (CombatEventType eventType in events.Keys){
            events[eventType].RemoveAllInstancesOfItem(subscriber);
        }
    }
}

public abstract class CombatEventData {
    public CombatEventType eventType;
}

public class CombatEventCombatStart : CombatEventData {
    public CombatEventCombatStart(){
        this.eventType = CombatEventType.ON_COMBAT_START;
    }
}

public class CombatEventCombatEnd : CombatEventData {
    public bool playerWon;

    public CombatEventCombatEnd(bool playerWon){
        this.eventType = CombatEventType.ON_COMBAT_END;
        this.playerWon = playerWon;
    }
}

public class CombatEventRoundStart : CombatEventData {
    public int roundStartNum;

    public CombatEventRoundStart(int roundStartNum){
        this.eventType = CombatEventType.ON_ROUND_START;
        this.roundStartNum = roundStartNum;
    }
}

public class CombatEventRoundEnd : CombatEventData {
    public int roundEndNum;

    public CombatEventRoundEnd(int roundEndNum){
        this.eventType = CombatEventType.ON_ROUND_END;
        this.roundEndNum = roundEndNum;
    }
}

public class CombatEventTurnStart : CombatEventData {
    public AbstractCharacter character;
    public int spd;

    public CombatEventTurnStart(AbstractCharacter character, int spd){
        this.eventType = CombatEventType.ON_TURN_START;
        this.character = character;
        this.spd = spd;
    }
}

public class CombatEventCombatStateChanged : CombatEventData {
    public CombatState prevState;
    public CombatState newState;

    /// <summary>
    /// Mostly used for interfaces, specifically when changing from AWAITING_ABILITY_INPUT to AWAITING_CLASH_INPUT (so that we can enable Reaction button clicks)
    /// </summary>
    /// <param name="prevState"></param>
    /// <param name="newState"></param>
    public CombatEventCombatStateChanged(CombatState prevState, CombatState newState){
        this.eventType = CombatEventType.ON_COMBAT_STATE_CHANGE;
        this.prevState = prevState;
        this.newState = newState;
    }
}

public class CombatEventTurnEnd : CombatEventData {
    public AbstractCharacter character;
    public int spd;

    public CombatEventTurnEnd(AbstractCharacter character, int spd){
        this.eventType = CombatEventType.ON_TURN_END;
        this.character = character;
        this.spd = spd;
    }
}

public class CombatEventCharacterDeath : CombatEventData {
    public AbstractCharacter deadChar;
    public CombatEventCharacterDeath(AbstractCharacter deadChar){
        this.eventType = CombatEventType.ON_CHARACTER_DEATH;
        this.deadChar = deadChar;
    }
}

public class CombatEventAbilityActivated : CombatEventData {
    public AbstractAbility abilityActivated;
    public List<Die> abilityDice;
    public AbstractCharacter caster;
    public AbstractCharacter target;
    public List<AbstractCharacter> targets = new List<AbstractCharacter>();
    public List<int> lanes = new List<int>();

    public CombatEventAbilityActivated(AbstractCharacter caster, AbstractAbility abilityActivated, ref List<Die> abilityDice, ref AbstractCharacter target){
        this.eventType = CombatEventType.ON_ABILITY_ACTIVATED;
        this.abilityActivated = abilityActivated;
        this.abilityDice = abilityDice;
        this.caster = caster;
        this.target = target;
    }
    
    public CombatEventAbilityActivated(AbstractCharacter caster, AbstractAbility abilityActivated, ref List<Die> abilityDice, ref List<AbstractCharacter> targets){
        this.eventType = CombatEventType.ON_ABILITY_ACTIVATED;
        this.abilityActivated = abilityActivated;
        this.abilityDice = abilityDice;
        this.caster = caster;
        this.targets = targets;
    }

    public CombatEventAbilityActivated(AbstractCharacter caster, AbstractAbility abilityActivated, ref List<Die> abilityDice, List<int> lanes){
        this.eventType = CombatEventType.ON_ABILITY_ACTIVATED;
        this.abilityActivated = abilityActivated;
        this.abilityDice = abilityDice;
        this.caster = caster;
        this.lanes = lanes;
    }
}

public class CombatEventClashOccurs : CombatEventData {
    public AbstractCharacter attacker;
    public AbstractAbility attackerAbility;
    public List<Die> attackerDice;

    public AbstractCharacter reacter;
    public AbstractAbility reacterAbility;
    public List<Die> reacterDice;

    ///<summary>Notify subscribers that a clash is occurring.</summary>
    public CombatEventClashOccurs(AbstractCharacter attacker, AbstractAbility attackerAbility, ref List<Die> attackerDice, AbstractCharacter reacter, AbstractAbility reacterAbility, ref List<Die> reacterDice){
        this.eventType = CombatEventType.ON_CLASH;
        this.attacker = attacker;
        this.attackerAbility = attackerAbility;
        this.attackerDice = attackerDice;

        this.reacter = reacter;
        this.reacterAbility = reacterAbility;
        this.reacterDice = reacterDice;
    }
}

public class CombatEventDieRolled : CombatEventData {
    public Die die;
    public int rolledValue;

    public CombatEventDieRolled(Die die, ref int rolledValue){
        this.eventType = CombatEventType.ON_DIE_ROLLED;
        this.die = die;
        this.rolledValue = rolledValue;
    }
}

public class CombatEventStatusApplied : CombatEventData {
    public AbstractStatusEffect statusEffect;
    public AbstractCharacter effectAppliedToChar;
    public int effectAppliedToLane;

    public CombatEventStatusApplied(AbstractStatusEffect effect, AbstractCharacter character){
        this.eventType = CombatEventType.ON_STATUS_APPLIED;
        this.statusEffect = effect;
        this.effectAppliedToChar = character;
    }

    public CombatEventStatusApplied(AbstractStatusEffect effect, int lane){
        this.eventType = CombatEventType.ON_STATUS_APPLIED;
        this.statusEffect = effect;
        this.effectAppliedToLane = lane;
    }
}

public class CombatEventDamageDealt : CombatEventData {
    public AbstractCharacter dealer;
    public int damageDealt;
    public bool isPoiseDamage;

    public CombatEventDamageDealt(AbstractCharacter dealer, ref int damageDealt, bool isPoiseDamage){
        this.eventType = CombatEventType.ON_DEAL_DAMAGE;
        this.dealer = dealer;
        this.damageDealt = damageDealt;
        this.isPoiseDamage = isPoiseDamage;
    }
}

public class CombatEventDamageTaken : CombatEventData {
    public AbstractCharacter target;
    public int damageTaken;
    public bool isPoiseDamage;

    public CombatEventDamageTaken(AbstractCharacter target, ref int damageTaken, bool isPoiseDamage){
        this.eventType = CombatEventType.ON_TAKE_DAMAGE;
        this.target = target;
        this.damageTaken = damageTaken;
        this.isPoiseDamage = isPoiseDamage;
    }
}

public class CombatEventClashTie : CombatEventData {

    public Die atkDie;
    public Die reactDie;

    public CombatEventClashTie(Die atkDie, Die reactDie){
        this.eventType = CombatEventType.ON_CLASH_TIE;
        this.atkDie = atkDie;
        this.reactDie = reactDie;
    }
}

public class CombatEventClashWin : CombatEventData {
    public Die winningDie;
    public int winningRoll;

    public CombatEventClashWin(Die winningDie, ref int winningRoll){
        this.eventType = CombatEventType.ON_CLASH_WIN;
        this.winningDie = winningDie;
        this.winningRoll = winningRoll;
    }
}

public class CombatEventClashLose : CombatEventData {
    public Die losingDie;
    public int losingRoll;

    public CombatEventClashLose(Die losingDie, ref int losingRoll){
        this.eventType = CombatEventType.ON_CLASH_LOSE;
        this.losingDie = losingDie;
        this.losingRoll = losingRoll;
    }
}


public class CombatEventUnitMoved : CombatEventData {
    public AbstractCharacter movedUnit;
    public int originalLane;
    public int moveMagnitude;
    public bool isMoveLeft;       // If false, assume move right.
    public bool isForcedMovement;   // Voluntary lane shifts and shifts as part of an ability effect are not considered "forced movement", but Pushes/Pulls are.

    public CombatEventUnitMoved(AbstractCharacter movedUnit, int originalLane, ref int moveMagnitude, bool isMoveLeft, bool isForcedMovement){
        this.eventType = CombatEventType.ON_UNIT_MOVED;
        this.movedUnit = movedUnit;
        this.originalLane = originalLane;
        this.moveMagnitude = moveMagnitude;
        this.isMoveLeft = isMoveLeft;
        this.isForcedMovement = isForcedMovement;
    }
}
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Godot;

public enum AbilityType {ATTACK, REACTION, UTILITY, SPECIAL};        // Actions like "Shift" or "Pass" are SPECIAL abilities.
public enum AbilityTargeting {
    /// <summary>
    /// Only include the ability's owner. Adding this modifier disregards all other modifiers.
    /// </summary>
    SELF,
    /// <summary>
    /// Only include all allies within range of the ability, including self.
    /// </summary>
    ALLIES_ONLY,
    /// <summary>
    /// This ability cannot target its owner.
    /// </summary>
    NOT_SELF,
    /// <summary>
    /// Only include all enemies within range of the ability.
    /// </summary>
    ENEMIES_ONLY,
    /// <summary>
    /// This ability targets a lane in range instead of any particular character (e.g. Move).
    /// </summary>
    LANE
}

public enum AbilityTag {AOE, CANNOT_REACT, CANTRIP, DEVIOUS};

public abstract class AbstractAbility : IEventSubscriber {

    public string ID;
    public string NAME;
    public string DESC;
    public AbstractCharacter OWNER;
    public AbilityType TYPE;
    public List<AbilityTargeting> targetingModifiers;     // If no filters are provided, by default this will return all allies and enemies within range of the ability.
    public int BASE_CD;
    public bool IS_GENERIC;         // Characters can't equip more than 4 IS_GENERIC abilities.
    public int MIN_RANGE;           
    public int MAX_RANGE;
    
    // An attack/reaction consists of a list of dice and any events (e.g. on hit, on clash, on clash win, on clash lose, etc.) associated with that die.
    // On attack/reaction activation, copy the list of BASE_DICE to CombatManager's attacker/defender dice queue.
    private List<Die> _BASE_DICE = new List<Die>();
    public List<Die> BASE_DICE {
        get {
            List<Die> deepCopy = new List<Die>();
            foreach (Die die in _BASE_DICE){
                deepCopy.Add(die.GetCopy());
            }
            return deepCopy;
        } 
        set {_BASE_DICE = value;}}
    public List<AbilityTag> TAGS = new List<AbilityTag>();

    public int curCooldown = 0;
    public bool IsAvailable {
        get { return curCooldown == 0; }
    }
    
    public AbstractAbility(string ID, string NAME, string DESC, AbilityType TYPE, int BASE_CD, int MIN_RANGE, int MAX_RANGE, List<AbilityTargeting> targetingModifiers = null){
        this.ID = ID;
        this.NAME = NAME;
        this.DESC = DESC;
        this.TYPE = TYPE;
        this.BASE_CD = BASE_CD;
        this.MIN_RANGE = MIN_RANGE;
        this.MAX_RANGE = MAX_RANGE;
        this.targetingModifiers = targetingModifiers ?? new List<AbilityTargeting>();
    }

    public bool HasTag(AbilityTag tag){
        return this.TAGS.Contains(tag);
    }

    public virtual List<AbstractCharacter> GetEligibleTargets(){
        List<AbstractCharacter> eligibleTargets = new List<AbstractCharacter>();
        int casterPosition = this.OWNER.Position;

        // Return early if there's a SELF-modifier.
        if (this.targetingModifiers.Contains(AbilityTargeting.SELF)) {
            eligibleTargets.Add(this.OWNER);
            GD.Print($"Attempting to cast {this.NAME}. There are {eligibleTargets.Count} eligible target(s).");
            foreach (AbstractCharacter target in eligibleTargets){
                GD.Print($"Eligible target: {target.CHAR_NAME}");
            }
            return eligibleTargets;
        }

        // Get the list of all fighters in range of this ability's activator.
        foreach ((CharacterFaction _, List<AbstractCharacter> fighters) in CombatManager.combatInstance.fighters){
            foreach (AbstractCharacter fighter in fighters){
                if (Math.Abs(fighter.Position - casterPosition) >= this.MIN_RANGE && Math.Abs(fighter.Position - casterPosition) <= this.MAX_RANGE){
                    eligibleTargets.Add(fighter);
                }
            }
        }
        foreach (AbilityTargeting modifier in this.targetingModifiers){
            switch (modifier){
                case AbilityTargeting.ALLIES_ONLY:
                    eligibleTargets.RemoveAll(character => character.CHAR_FACTION == CharacterFaction.NEUTRAL || character.CHAR_FACTION == CharacterFaction.ENEMY);
                    break;
                case AbilityTargeting.NOT_SELF:
                    eligibleTargets.Remove(this.OWNER);
                    break;
                case AbilityTargeting.ENEMIES_ONLY:
                    eligibleTargets.RemoveAll(character => character.CHAR_FACTION == CharacterFaction.ALLY || character.CHAR_FACTION == CharacterFaction.PLAYER);
                    break;
            }
        }

        GD.Print($"Attempting to cast {this.NAME}. There are {eligibleTargets.Count} eligible target(s).");
        foreach (AbstractCharacter target in eligibleTargets){
            GD.Print($"Eligible target: {target.CHAR_NAME}");
        }
        return eligibleTargets;
    }

    public virtual void HandleEvent(CombatEventData eventData){
        CombatEventType eventType = eventData.eventType;
        if (eventType == CombatEventType.ON_ABILITY_ACTIVATED) {
            CombatEventAbilityActivated data = (CombatEventAbilityActivated) eventData;
            if (data.abilityActivated == this){
                this.Activate(data);
            }
        }
    }

    // Should be overridden by Utility abilties.
    public virtual void Activate(CombatEventAbilityActivated data){
        this.curCooldown = this.BASE_CD;
    }
}
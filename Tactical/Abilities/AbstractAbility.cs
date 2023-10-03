using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Godot;

public enum AbilityType {ATTACK, REACTION, UTILITY, SPECIAL};        // Actions like "Shift" or "Pass" are SPECIAL abilities.
public enum TargetingModifiers {
    /// <summary>
    /// This ability can only target its owner.
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
    ENEMIES_ONLY
}

public enum AbilityTag {AOE, CANNOT_REACT, CANTRIP, DEVIOUS};

public abstract class AbstractAbility : IEventSubscriber {

    public string ID;
    public string NAME;
    public string DESC;
    public AbstractCharacter OWNER;
    public AbilityType TYPE;
    public int BASE_CD;
    public int curCooldown = 0;
    public bool IS_GENERIC;         // Characters can't equip more than 4 IS_GENERIC abilities.
    public int MIN_RANGE;           
    public int MAX_RANGE;

    public bool requiresUnit;       // If true, there must be at least one eligible unit in the targeted lane(s).
    public HashSet<TargetingModifiers> targetingModifiers;     // If no filters are provided, by default this will return all allies and enemies within range of the ability.
    
    // An attack/reaction consists of a list of dice and any events (e.g. on hit, on clash, on clash win, on clash lose, etc.) associated with that die.
    // On attack/reaction activation, copy the list of BASE_DICE to CombatManager's attacker/defender dice queue.
    private List<Die> _BASE_DICE = new List<Die>();
    public List<Die> BASE_DICE {
        get {
            List<Die> deepCopy = new List<Die>();
            foreach (Die die in _BASE_DICE){
                deepCopy.Add(die);
            }
            return deepCopy;
        } 
        set {_BASE_DICE = value;}}
    public List<AbilityTag> TAGS = new List<AbilityTag>();

    public bool IsAvailable {
        get { return curCooldown == 0; }
    }
    
    public AbstractAbility(string ID, string NAME, string DESC, AbilityType TYPE, int BASE_CD, int MIN_RANGE, int MAX_RANGE, bool requiresUnit, HashSet<TargetingModifiers> targetingModifiers = null){
        this.ID = ID;
        this.NAME = NAME;
        this.DESC = DESC;
        this.TYPE = TYPE;
        this.BASE_CD = BASE_CD;
        this.MIN_RANGE = MIN_RANGE;
        this.MAX_RANGE = MAX_RANGE;
        this.requiresUnit = requiresUnit;
        this.targetingModifiers = targetingModifiers ?? new HashSet<TargetingModifiers>();
    }

    public bool HasTag(AbilityTag tag){
        return this.TAGS.Contains(tag);
    }

    // When an ability is activated, check the list of (lane, hash set of units in that lane).
    // This is sorted, since there's only ever 6 lanes (so constant time).
    public List<(int lane, HashSet<AbstractCharacter> targetsInLane)> GetValidTargets(){
        CombatInstance combatInstance = CombatManager.combatInstance;
        if (combatInstance == null){
            GD.PrintErr("CombatManager does not have a valid combat instance!");
            throw new Exception("CombatManager does not have a valid combat instance!");
        }
        
        List<(int lane, HashSet<AbstractCharacter> targetsInLane)> results = new();
        foreach (int lane in this.GetTargetableLanes()){
            HashSet<AbstractCharacter> validFightersInLane = combatInstance.fighters.Where(fighter => fighter.Position == lane).ToHashSet();
            foreach (TargetingModifiers modifier in this.targetingModifiers){
                switch (modifier){
                    case TargetingModifiers.ALLIES_ONLY:
                        validFightersInLane.RemoveWhere(fighter => fighter.CHAR_FACTION != this.OWNER.CHAR_FACTION);
                        break;
                    case TargetingModifiers.NOT_SELF:
                        validFightersInLane.RemoveWhere(fighter => fighter == this.OWNER);
                        break;
                    case TargetingModifiers.ENEMIES_ONLY:
                        validFightersInLane.RemoveWhere(fighter => fighter.CHAR_FACTION == this.OWNER.CHAR_FACTION);
                        break;
                    case TargetingModifiers.SELF:
                        validFightersInLane.RemoveWhere(fighter => fighter != this.OWNER);
                        break;
                }
            }
            results.Add((lane, validFightersInLane));
        }
        // Sort from lanes 1->6.
        results = results.OrderBy(tuple => tuple.lane).ToList();

        // Debug
        GD.Print();
        GD.Print($"Available lanes and targets for {this.NAME}");
        GD.Print("============================================");
        foreach ((int lane, HashSet<AbstractCharacter> targetsInLane) in results){
            string str = $"Lane {lane}: ";
            foreach (AbstractCharacter target in targetsInLane){
                str += $"{target.CHAR_NAME} | ";
            }
            GD.Print(str);
        }
        GD.Print();

        return results;
    }

    private HashSet<int> GetTargetableLanes(){
        int casterPosition = this.OWNER.Position;
        HashSet<int> targetableLanes = new HashSet<int>();

        for (int i = this.MIN_RANGE; i <= this.MAX_RANGE; i++){
            targetableLanes.Add(casterPosition - i);
            targetableLanes.Add(casterPosition + i);
        }
        targetableLanes.RemoveWhere(i => i < 1 || i > 6);
        return targetableLanes;
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
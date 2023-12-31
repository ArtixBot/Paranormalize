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

public enum AbilityTag {
    /// <summary>
    /// This ability targets multiple units/lanes (or both!). It cannot be clashed against and cannot be used to react to a clash. Practically speaking, a combination of CANNOT_REACT and DEVIOUS.
    /// </summary>
    AOE,
    /// <summary>
    /// This ability cannot be used to clash against an incoming attack.
    /// </summary>
    CANNOT_REACT,
    /// <summary>
    /// Activating this ability does not consume an action.
    /// </summary>
    CANTRIP,
    /// <summary>
    /// If used to initiate an attack, this ability cannot be clashed against.
    /// </summary>
    DEVIOUS,
    /// <summary>
    /// This ability can only be activated once per combat. Exhausted abilities are considered Unavailable.
    /// </summary>
    EXHAUST
};

public abstract class AbstractAbility : IEventSubscriber, IEventHandler<CombatEventAbilityActivated>, IEventHandler<CombatEventRoundEnd> {

    public string ID;
    public string NAME;
    public Dictionary<string, string> STRINGS = new();      // STRINGS contains all localized information for the ability and its associated dice.
    public AbstractCharacter OWNER;
    public AbilityType TYPE;
    public int BASE_CD;
    public int curCooldown = 0;
    public bool IS_GENERIC;         // Characters can't equip more than 4 IS_GENERIC abilities.
    public int MIN_RANGE;           
    public int MAX_RANGE;

    public bool useLaneTargeting;   // If true, the UI should supply lanes to target rather than a list of units.
    public bool requiresUnit;       // This ability requires an actual unit. If useLaneTargeting is true and this is true, only lanes with applicable units will be included.
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
    
    public AbstractAbility(string ID, Localization.AbilityStrings ABILITY_STRINGS, AbilityType TYPE, int BASE_CD, int MIN_RANGE, int MAX_RANGE, bool useLaneTargeting, bool requiresUnit, HashSet<TargetingModifiers> targetingModifiers = null){
        this.ID = ID;
        this.NAME = ABILITY_STRINGS.NAME;
        this.STRINGS = (ABILITY_STRINGS.STRINGS != null) ? ABILITY_STRINGS.STRINGS : new();
        this.TYPE = TYPE;
        this.BASE_CD = BASE_CD;
        this.MIN_RANGE = MIN_RANGE;
        this.MAX_RANGE = MAX_RANGE;
        this.useLaneTargeting = useLaneTargeting;
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
            if (requiresUnit && validFightersInLane.Count == 0) continue;
            results.Add((lane, validFightersInLane));
        }
        // Sort from lanes 1->6.
        results = results.OrderBy(tuple => tuple.lane).ToList();

        // Debug
        // GD.Print();
        // GD.Print($"Available lanes and targets for {this.NAME}");
        // GD.Print("============================================");
        // foreach ((int lane, HashSet<AbstractCharacter> targetsInLane) in results){
        //     string str = $"Lane {lane}: ";
        //     foreach (AbstractCharacter target in targetsInLane){
        //         str += $"{target.CHAR_NAME} | ";
        //     }
        //     GD.Print(str);
        // }
        // GD.Print();

        return results;
    }

    private HashSet<int> GetTargetableLanes(){
        int casterPosition = this.OWNER.Position;
        HashSet<int> targetableLanes = new HashSet<int>();

        for (int i = this.MIN_RANGE; i <= this.MAX_RANGE; i++){
            targetableLanes.Add(casterPosition - i);
            targetableLanes.Add(casterPosition + i);
        }
        targetableLanes.RemoveWhere(i => i < GameVariables.MIN_LANES || i > GameVariables.MAX_LANES);
        return targetableLanes;
    }

    public virtual void InitSubscriptions(){
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.IMMEDIATELY);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.IMMEDIATELY);
    }

    public virtual void HandleEvent(CombatEventAbilityActivated data){
        if (data.abilityActivated.Equals(this)){
            this.curCooldown = this.BASE_CD;
            this.Activate(data);
        }
    }

    public virtual void HandleEvent(CombatEventRoundEnd data){
        this.curCooldown = Math.Max(this.curCooldown - 1, 0);
    }

    // Should be overridden by Utility abilties.
    public virtual void Activate(CombatEventAbilityActivated data){}
}
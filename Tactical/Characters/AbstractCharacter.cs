using Godot;
using System;
using System.Collections.Generic;

public enum CharacterFaction {PLAYER, NEUTRAL, ENEMY};

public partial class AbstractCharacter : IEventSubscriber {
    public CharacterFaction CHAR_FACTION;
    public string CHAR_NAME;

    public List<AbstractAbility> abilities = new();       // At the start of combat, deep-copy everything from PERMA_ABILITIES.
    public List<AbstractStatusEffect> statusEffects = new();

    private int _CurHP, _MaxHP, _CurPoise, _MaxPoise;
    public int CurHP {
        get {return _CurHP;}
        set { _CurHP = Math.Min(value, MaxHP);}                         // Current HP cannot exceed the range of [-inf, MaxHP].
    }
    public int MaxHP {
        get {return _MaxHP;}
        set { _MaxHP = value; CurHP = CurHP;}   // Call the CurHP setter to automatically update its value if curHP exceeded maxHP post-update.
    }
    public int CurPoise {
        get {return _CurPoise;}
        set { _CurPoise = Math.Max(0, Math.Min(value, _MaxPoise));}     // Current poise cannot exceed the range of [0, MaxPoise].
    }
    public int MaxPoise {
        get {return _MaxPoise;}
        set { _MaxPoise = value; CurPoise = CurPoise;}
    }   

    public int ActionsPerTurn;
    public int MinSpd, MaxSpd;      // Speed modifiers like Haste or Slow are done in event handling.
    public int Position;

    public AbstractCharacter() : this(10, 10, 1, 5, CharacterFaction.NEUTRAL, "Unnamed Fighter") {}
    public AbstractCharacter(string name) : this(10, 10, 1, 5, CharacterFaction.NEUTRAL, name) {}
    public AbstractCharacter(string name, CharacterFaction faction) : this(10, 10, 1, 5, faction, name) {}

    public AbstractCharacter(int maxHP, int maxPoise, int minSpd, int maxSpd, CharacterFaction faction, string name){
        this.MaxHP = maxHP;
        this.CurHP = maxHP;
        this.MaxPoise = maxPoise;
        this.CurPoise = maxPoise;
        this.MinSpd = minSpd;
        this.MaxSpd = maxSpd;
        this.ActionsPerTurn = 2;
        this.CHAR_FACTION = faction;
        this.CHAR_NAME = name;

        EquipAbility(new AbilityPass());
        EquipAbility(new AbilityMove());
    }

    // Add an ability to the character's list of equipped abilities.
    // Returns true if successful, otherwise false.
    // TODO: Move default character ability equip limits to UI logic instead? We might have in-combat granted abilities and this wouldn't work well with that.
    public bool EquipAbility(AbstractAbility ability){
        if (this.abilities.Count >= 8) return false;
        // Cannot equip more than 4 generic abilities at any given time.
        if (this.abilities.FindAll(ability => ability.IS_GENERIC).Count >= 4) return false;

        ability.OWNER = this;
        this.abilities.Add(ability);
        return true;
    }

    // Remove an ability to the character's list of equipped abilities.
    // Returns true if successful, otherwise false.
    public bool UnequipAbility(AbstractAbility ability){
        return this.abilities.Remove(ability);
    }

    public int CountAvailableAbilities(){
        int cnt = 0;
        foreach (AbstractAbility ability in abilities){
            if (ability.IsAvailable && ability.TYPE != AbilityType.SPECIAL) cnt++;
        }
        return cnt;
    }

    public int CountUnavailableAbilities(){
        int cnt = 0;
        foreach (AbstractAbility ability in abilities){
            if (!ability.IsAvailable && ability.TYPE != AbilityType.SPECIAL) cnt++;
        }
        return cnt;
    }

    public virtual void InitSubscriptions(){
        CombatEventManager.instance.Subscribe(CombatEventType.ON_CHARACTER_DEATH, this, CombatEventPriority.STANDARD);
        foreach (AbstractAbility ability in this.abilities){
            ability.InitSubscriptions();
        }
    }

    public virtual void HandleEvent(CombatEventData data){
        switch (data.eventType) {
            case CombatEventType.ON_CHARACTER_DEATH:
                CombatEventCharacterDeath deathData = (CombatEventCharacterDeath) data;
                if (deathData.deadChar != this) return;
                // Handle death stuff.
                foreach (AbstractAbility ability in this.abilities){
                    CombatManager.eventManager?.UnsubscribeAll(ability);
                }
                return;
        }
    }
}
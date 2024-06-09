using System;
using System.Diagnostics;
using Godot;

public enum DamageType {
    BLUNT,
    PIERCE,
    SLASH,
    ELDRITCH,
    PURE        // Reserved for effects like DoTs (Bleed, Burn, etc.) and Poise stagger damage dealt from Block dice.
}

public class DamageAction : AbstractAction {

    private AbstractCharacter attacker;
    private AbstractCharacter defender;
    private DamageType damageType;
    private int damage;
    private bool isPoiseDamage;
    private int clashIteration;     // Used for UI data (possibly other passives in the future?).

    public DamageAction(AbstractCharacter attacker, AbstractCharacter defender, DamageType damageType, int damage, bool isPoiseDamage, int clashIteration = 0){
        this.attacker = attacker;
        this.defender = defender;
        this.damageType = damageType;
        this.damage = damage;
        this.isPoiseDamage = isPoiseDamage;
        this.clashIteration = clashIteration;
    }

    public override void Execute(){
        if (this.defender == null) return;
        // Attacker can be null from status effect damage (burn, bleed, etc.)
        if (this.attacker != null) {
            CombatEventDamageDealt dealtData = CombatManager.eventManager.BroadcastEvent(new CombatEventDamageDealt(this.attacker, this.damage, this.isPoiseDamage, this.defender));
            this.damage = (int) dealtData.damageDealt;
        }

        // Note that CombatEventDamageTaken does calculations of this.damage as a float.
        // This accounts for cases like a +50% and +100% damage multiplier, which is 3 * 1.5 => 4.5 * 2 => 9.
        CombatEventDamageTaken damageData = new(this.defender, this.damageType, this.damage, this.isPoiseDamage, clashIteration);
        CombatManager.eventManager.BroadcastEvent(damageData);

        // Damage cannot be negative.
        damageData.damageTaken = Math.Max(damageData.damageTaken, 0);

        // The final value always takes the floor (no rounding up).
        if (damageData.isPoiseDamage) {
            this.defender.CurPoise -= (int) damageData.damageTaken;
        } else {
            this.defender.CurHP -= (int) damageData.damageTaken;
        }

        Logging.Log($"{defender.CHAR_NAME} takes {(int)damageData.damageTaken} {(isPoiseDamage ? "Poise " : "")}damage.", Logging.LogLevel.ESSENTIAL);

        if (this.defender.CurPoise <= 0){
            // Note that passives which prevent stagger for the first time in combat should listen to CombatEventDamageTaken.
            CombatManager.ExecuteAction(new ApplyStatusAction(damageData.target, new ConditionStaggered(), stacksToApply: 2));
        }
        if (!this.isPoiseDamage && this.defender.CurHP <= 0){
            // Ditto with above.
            CombatManager.ExecuteAction(new RemoveCombatantAction(damageData.target));
        }
    }
}
using System.Diagnostics;
using Godot;

public class DamageAction : AbstractAction {

    private AbstractCharacter attacker;
    private AbstractCharacter defender;
    private int damage;
    private bool isPoiseDamage;

    public DamageAction(AbstractCharacter attacker, AbstractCharacter defender, int damage, bool isPoiseDamage){
        this.attacker = attacker;
        this.defender = defender;
        this.damage = damage;
        this.isPoiseDamage = isPoiseDamage;
    }

    public override void Execute(){
        if (this.defender == null) return;
        // Attacker can be null from status effect damage (burn, bleed, etc.)
        if (this.attacker != null) {
            CombatEventDamageDealt dealtData = CombatManager.eventManager.BroadcastEvent(new CombatEventDamageDealt(this.attacker, this.damage, this.isPoiseDamage));
            this.damage = (int) dealtData.damageDealt;
        }

        // Note that CombatEventDamageTaken does calculations of this.damage as a float.
        // This accounts for cases like a +50% and +100% damage multiplier, which is 3 * 1.5 => 4.5 * 2 => 9.
        CombatEventDamageTaken damageData = new(this.defender, this.damage, this.isPoiseDamage);
        CombatManager.eventManager.BroadcastEvent(damageData);

        // The final value always takes the floor (no rounding up).
        if (damageData.isPoiseDamage) {
            this.defender.CurPoise -= (int) damageData.damageTaken;
        } else {
            this.defender.CurHP -= (int) damageData.damageTaken;
        }

        Logging.Log($"{defender.CHAR_NAME} takes {(int)damageData.damageTaken} {(isPoiseDamage ? "Poise " : "")}damage.", Logging.LogLevel.ESSENTIAL);

        if (this.defender.CurPoise <= 0){
            // Note that passives which prevent stagger for the first time in combat should listen to CombatEventDamageDealt.
            CombatManager.ExecuteAction(new ApplyStatusAction(damageData.target, new ConditionStaggered(), stacksToApply: 2));
        }
        if (this.defender.CurHP <= 0){
            // Ditto with above.
            CombatManager.ExecuteAction(new RemoveCombatantAction(damageData.target));
        }
    }
}
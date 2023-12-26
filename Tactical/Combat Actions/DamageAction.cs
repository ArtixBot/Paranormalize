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
            CombatManager.eventManager.BroadcastEvent(new CombatEventDamageDealt(this.attacker, ref this.damage, this.isPoiseDamage));
        }

        CombatManager.eventManager.BroadcastEvent(new CombatEventDamageTaken(this.defender, ref this.damage, this.isPoiseDamage));
        if (this.isPoiseDamage) {
            this.defender.CurPoise -= this.damage;
        } else {
            this.defender.CurHP -= this.damage;
        }

        if (this.defender.CurPoise <= 0){
            // Note that passives which prevent stagger for the first time in combat should listen to CombatEventDamageDealt instead.
            CombatManager.ExecuteAction(new ApplyStatusAction(this.defender, new ConditionStaggered()));
        }
        if (this.defender.CurHP <= 0){
            // Ditto with above.
            // TODO: Swap with CombatManager.ExecuteAction(new RemoveCombatantAction(combatant));
            CombatManager.ExecuteAction(new RemoveCombatantAction(this.defender));
        }
    }
}
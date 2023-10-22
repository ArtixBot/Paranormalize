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

        if (this.isPoiseDamage) {
            this.defender.CurPoise -= this.damage;
            // CombatManager.eventManager.BroadcastEvent(new CombatEventPoiseDamageDealt());
        } else {
            this.defender.CurHP -= this.damage;
            // CombatManager.eventManager.BroadcastEvent(new CombatEventHpDamageDealt());
        }

        if (this.defender.CurPoise <= 0){
            CombatManager.ExecuteAction(new ApplyStatusAction(this.defender, new ConditionStaggered()));
            // CombatManager.eventManager.BroadcastEvent(new CombatEventStaggered());
        }
        if (this.defender.CurHP <= 0){
            // TODO: Swap with CombatManager.ExecuteAction(new RemoveCombatantAction(combatant));
            CombatManager.ResolveCombatantDeath(this.defender);
        }
    }
}
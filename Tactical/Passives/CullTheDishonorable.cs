using Localization;
namespace CharacterPassives;

public class CullTheDishonorable : AbstractPassive, IEventHandler<CombatEventDieRolled>, IEventHandler<CombatEventDamageDealt> {

    public static readonly string id = "CULL_THE_DISHONORABLE";
    public static readonly int cost = 6;
    private static readonly PassiveStrings strings = LocalizationLibrary.Instance.GetPassiveStrings(id);
    
    public CullTheDishonorable() : base(id, strings, cost){}

    public override void InitSubscriptions() {
        CombatManager.eventManager.Subscribe(CombatEventType.ON_DIE_ROLLED, this, CombatEventPriority.BASE_ADDITIVE);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_DEAL_DAMAGE, this, CombatEventPriority.BASE_MULTIPLICATIVE);
    }

    public void HandleEvent(CombatEventDieRolled data){
        if (data.roller == this.OWNER && data.rollTarget.HasCondition("DISHONORABLE")){
            Logging.Log($"{this.OWNER.CHAR_NAME} triggered Cull the Dishonorable, increasing the roll from {data.rolledValue} to {data.rolledValue + 3}.", Logging.LogLevel.INFO);
            data.rolledValue += 3;
        }
    }

    // Check the damage dealer event to see if this unit is dealing the damage. If it is, then the damage taken event will trigger.
    public void HandleEvent(CombatEventDamageDealt data){
        if (data.dealer == this.OWNER && !data.isPoiseDamage && data.target.HasCondition("DISHONORABLE")){
            Logging.Log($"{this.OWNER.CHAR_NAME} triggered Cull the Dishonorable, doubling incoming damage from {data.damageDealt} to {data.damageDealt * 2}.", Logging.LogLevel.INFO);
            data.damageDealt *= 2.0f;
        }
    }
}
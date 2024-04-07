using Localization;
namespace CharacterPassives;

public class DeathBeforeDishonor : AbstractPassive, IEventHandler<CombatEventStatusApplied> {

    public static readonly string id = "DEATH_BEFORE_DISHONOR";
    public static readonly int cost = 0;
    private static readonly PassiveStrings strings = LocalizationLibrary.Instance.GetPassiveStrings(id);
    
    public DeathBeforeDishonor() : base(id, strings, cost){}

    public override void InitSubscriptions() {
        CombatManager.eventManager.Subscribe(CombatEventType.ON_STATUS_APPLIED, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventStatusApplied data){
        if (data.statusEffect.ID == "DISHONORABLE" && data.effectAppliedToChar == this.OWNER){
            // Dishonorable triggers!
            CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, data.statusEffect));
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new ConditionStaggered(), 2));
        }
    }
}
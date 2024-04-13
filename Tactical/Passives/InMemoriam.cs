using System.Linq;
using Localization;
namespace CharacterPassives;

public class InMemoriam : AbstractPassive, IEventHandler<CombatEventCharacterDeath> {

    public static readonly string id = "IN_MEMORIAM";
    public static readonly int cost = 6;
    private static readonly PassiveStrings strings = LocalizationLibrary.Instance.GetPassiveStrings(id);
    
    public InMemoriam() : base(id, strings, cost){}

    public override void InitSubscriptions() {
        CombatManager.eventManager.Subscribe(CombatEventType.ON_CHARACTER_DEATH, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventCharacterDeath data){
        if (data.deadChar.CHAR_FACTION == this.OWNER.CHAR_FACTION){
            this.OWNER.ActionsPerTurn += 1;
            // Has to check for 2 characters; the actual removal of characters occurs *after* this event fires.
            if (CombatManager.combatInstance.fighters.Where(character => character.CHAR_FACTION == this.OWNER.CHAR_FACTION).Count() == 2){
                CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, "STAGGERED"));
                CombatManager.ExecuteAction(new RestoreAction(this.OWNER, 1.0f, RestoreAction.RestoreType.HEALTH, RestoreAction.RestorePercentType.PERCENTAGE_MAX));
                CombatManager.ExecuteAction(new RestoreAction(this.OWNER, 1.0f, RestoreAction.RestoreType.POISE, RestoreAction.RestorePercentType.PERCENTAGE_MAX));
            }
        }
    }
}
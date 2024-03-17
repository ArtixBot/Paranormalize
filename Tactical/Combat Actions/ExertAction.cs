using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ExertAction : AbstractAction {

    private AbstractAbility abilityToExert;
    private int exertDuration;

    public ExertAction(AbstractAbility abilityToExert, int exertDuration){
        this.abilityToExert = abilityToExert;
        this.exertDuration = exertDuration;
    }

    public override void Execute(){
        if (this.abilityToExert == null) return;
        // TODO: CombatManager.eventManager.BroadcastEvent(new CombatEventAbilityExerted(this.abilityToExert, this.exertDuration));
        this.abilityToExert.curCooldown += this.exertDuration;
        return;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RemoveStatusAction : AbstractAction {

    private AbstractCharacter target;
    private AbstractStatusEffect effect;

    public RemoveStatusAction(AbstractCharacter target, AbstractStatusEffect effect){
        this.target = target;
        this.effect = effect;
    }

    public override void Execute(){
        if (this.target == null) return;
        target.statusEffects.Remove(effect);
        CombatManager.eventManager.UnsubscribeAll(effect);
        // TODO: CombatManager.eventManager.BroadcastEvent(new CombatEventEffectRemoved).
        return;
    }
}
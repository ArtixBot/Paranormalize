using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RemoveStatusAction : AbstractAction {

    private AbstractCharacter target;
    private AbstractStatusEffect effect;
    private string effectID;

    public RemoveStatusAction(AbstractCharacter target, string effectID){
        this.target = target;
        this.effectID = effectID;
    }

    public RemoveStatusAction(AbstractCharacter target, AbstractStatusEffect effect){
        this.target = target;
        this.effect = effect;
    }

    public override void Execute(){
        if (this.target == null) return;
        if (this.effect != null){
            target.statusEffects.Remove(effect);
            CombatManager.eventManager.UnsubscribeAll(effect);
        } else if (this.effectID != null){
            AbstractStatusEffect findEffect = target.statusEffects.Where(effect => effect.ID == this.effectID).FirstOrDefault();
            if (findEffect != default){
                target.statusEffects.Remove(findEffect);
                CombatManager.eventManager.UnsubscribeAll(findEffect);
            }
        }
        // TODO: CombatManager.eventManager.BroadcastEvent(new CombatEventEffectRemoved).
        return;
    }
}
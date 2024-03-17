using System;

public class InvigorateAction : AbstractAction {

    private AbstractAbility abilityToInvigorate;
    private int invigorateDuration;

    public InvigorateAction(AbstractAbility abilityToInvigorate, int invigorateDuration){
        this.abilityToInvigorate = abilityToInvigorate;
        this.invigorateDuration = invigorateDuration;
    }

    public override void Execute(){
        if (this.abilityToInvigorate == null) return;
        // TODO: CombatManager.eventManager.BroadcastEvent(new CombatEventAbilityInvigorateed(this.abilityToInvigorate, this.invigorateDuration));
        this.abilityToInvigorate.curCooldown -= this.invigorateDuration;
        this.abilityToInvigorate.curCooldown = Math.Max(this.abilityToInvigorate.curCooldown, 0);
        return;
    }
}
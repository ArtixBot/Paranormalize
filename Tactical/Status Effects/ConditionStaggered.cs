using Godot;
using System;
using System.Collections.Generic;

public class ConditionStaggered : AbstractStatusEffect{
    public ConditionStaggered(){
        this.ID = "STAGGERED";
        this.TYPE = StatusEffectType.CONDITION;
        this.STACKS = 2;
    }

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
    }

    public override void HandleEvent(CombatEventData data){
        this.STACKS -= 1;
        if (this.STACKS == 0){
            // CombatManager.ExecuteAction(new RemoveStatusAction(character, this));
        }
    }
}
using Godot;
using System;
using System.Collections.Generic;

public class ConditionStaggered : AbstractStatusEffect{
    private int oldActionsPerTurn;

    public ConditionStaggered(){
        this.ID = "STAGGERED";
        this.TYPE = StatusEffectType.CONDITION;
        this.STACKS = 2;
    }

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
        this.oldActionsPerTurn = this.OWNER.ActionsPerTurn;

        this.OWNER.DamageTakenMod += 1.0f;
        this.OWNER.ActionsPerTurn = 0;
    }

    public override void HandleEvent(CombatEventData data){
        // At end of round, reduce stacks (duration) by 1.
        this.STACKS -= 1;

        // At 0 stacks, Staggered is removed.
        if (this.STACKS == 0){
            this.OWNER.DamageTakenMod -= 1.0f;
            this.OWNER.ActionsPerTurn = this.oldActionsPerTurn;
            this.OWNER.CurPoise = this.OWNER.MaxPoise;
            CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
        }
    }
}
using Godot;
using System;
using System.Collections.Generic;

public class ConditionStaggered : AbstractStatusEffect{
    private int oldActionsPerTurn;

    public ConditionStaggered(){
        this.ID = "STAGGERED";
        this.TYPE = StatusEffectType.CONDITION;
        this.CAN_GAIN_STACKS = false;
    }

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_TAKE_DAMAGE, this, CombatEventPriority.BASE_MULTIPLICATIVE);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
        this.oldActionsPerTurn = this.OWNER.ActionsPerTurn;

        this.OWNER.ActionsPerTurn = 0;

        // If the staggered character is in a combat/clash phase, remove all dice from their queue.
        // This will either immediately resolve combat or force a one-sided attack.
        if (CombatManager.combatInstance.activeAbility?.OWNER == this.OWNER){
            CombatManager.combatInstance.activeAbilityDice?.Clear();
        }
        if (CombatManager.combatInstance.reactAbility?.OWNER == this.OWNER){
            CombatManager.combatInstance.reactAbilityDice?.Clear();
        }
        // On stagger, remove all remaining actions from the user this turn as well.
        CombatManager.combatInstance.turnlist.RemoveAllInstancesOfItem(this.OWNER);
    }

    public override void HandleEvent(CombatEventData data){
        switch (data.eventType){
            case CombatEventType.ON_TAKE_DAMAGE:
                data = data as CombatEventDamageTaken;
                // TODO: Despite passing by reference the payload itself (CombatEventData data) is not having its value changed. Need to look into this.
                // See https://stackoverflow.com/a/1429763?
                // if (!data.isPoiseDamage){
                //     data.damageTaken *= 2;      // Damage taken while staggered is doubled.
                // }
                break;
            case CombatEventType.ON_ROUND_END:
                // At end of round, reduce stacks (duration) by 1.
                this.STACKS -= 1;

                // At 0 stacks, Staggered is removed.
                if (this.STACKS == 0){
                    this.OWNER.ActionsPerTurn = this.oldActionsPerTurn;
                    CombatManager.ExecuteAction(new RecoverPoiseAction(this.OWNER, this.OWNER.MaxPoise));
                    CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
                }
                break;
        }
    }
}
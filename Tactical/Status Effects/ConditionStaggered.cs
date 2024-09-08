using Godot;
using System;
using System.Collections.Generic;

public class ConditionStaggered : AbstractStatusEffect, IEventHandler<CombatEventDamageTaken>, IEventHandler<CombatEventRoundStart>, IEventHandler<CombatEventRoundEnd>{

    public static string id = "STAGGERED";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public int UNSTAGGER_ROUND;        // Parsed in effects.json and Tooltip.cs.

    public ConditionStaggered() : base(
        id,
        strings,
        StatusEffectType.CONDITION,
        CAN_GAIN_STACKS: false
    ){}

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_TAKE_DAMAGE, this, CombatEventPriority.BASE_MULTIPLICATIVE);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.FINAL);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
        this.UNSTAGGER_ROUND = CombatManager.combatInstance.round + 1;      // Current round + next round.

        // If the staggered character is in a combat/clash phase, remove all dice from their queue.
        // This will either immediately resolve combat or force a one-sided attack.
        if (CombatManager.combatInstance.activeAbility?.OWNER == this.OWNER){
            CombatManager.combatInstance.activeAbilityDice?.Clear();
        }
        if (CombatManager.combatInstance.reactAbility?.OWNER == this.OWNER){
            CombatManager.combatInstance.reactAbilityDice?.Clear();
        }
        // On stagger, remove all remaining actions from the user this turn as well.
        this.OWNER.CurPoise = 0;        // Also set to zero from cases where Stagger is applied (e.g. All In ability)
        CombatManager.combatInstance.turnlist.RemoveAllInstancesOfItem(this.OWNER);
    }

    public void HandleEvent(CombatEventDamageTaken data){
        if (data.target == this.OWNER && !data.isPoiseDamage){
            Logging.Log($"{this.OWNER.CHAR_NAME} was Staggered, doubling incoming damage from {data.damageTaken} to {data.damageTaken * 2}.", Logging.LogLevel.INFO);
            data.damageTaken *= 2.0f;      // Damage taken while staggered is doubled.
        }
    }

    public void HandleEvent(CombatEventRoundStart data){
        CombatManager.combatInstance.turnlist.RemoveAllInstancesOfItem(this.OWNER);
    }

    public void HandleEvent(CombatEventRoundEnd data){
        this.STACKS -= 1;

        // At 0 stacks, Staggered is removed.
        if (this.STACKS == 0){
            CombatManager.ExecuteAction(new RestoreAction(this.OWNER, 1.0f, RestoreAction.RestoreType.POISE, RestoreAction.RestorePercentType.PERCENTAGE_MAX));
            CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
        }
    }
}
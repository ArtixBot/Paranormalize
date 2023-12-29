using Godot;
using System;
using System.Collections.Generic;

public class DebuffVulnerable : AbstractStatusEffect, IEventHandler<CombatEventDamageTaken>, IEventHandler<CombatEventRoundEnd>{

    /// <summary>
    /// A debuff that increases damage taken by +50%. Stacks have no additional effect.
    /// </summary>
    public DebuffVulnerable(){
        this.ID = "VULNERABLE";
        this.TYPE = StatusEffectType.DEBUFF;
        this.CAN_GAIN_STACKS = false;
    }

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_TAKE_DAMAGE, this, CombatEventPriority.BASE_MULTIPLICATIVE);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventDamageTaken data){
        if (data.target == this.OWNER && !data.isPoiseDamage){
            Logging.Log($"{this.OWNER.CHAR_NAME} was Vulnerable, increasing incoming damage from {data.damageTaken} to {data.damageTaken * 1.5}.", Logging.LogLevel.INFO);
            data.damageTaken *= 1.5f;
        }
    }

    public void HandleEvent(CombatEventRoundEnd data){
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }
}
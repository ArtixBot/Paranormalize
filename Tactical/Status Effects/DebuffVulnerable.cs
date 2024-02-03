using Godot;
using System;
using System.Collections.Generic;

public class DebuffVulnerable : AbstractStatusEffect, IEventHandler<CombatEventDamageTaken>, IEventHandler<CombatEventRoundEnd>{

    public static string id = "VULNERABLE";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public DebuffVulnerable() : base(
        id,
        strings,
        StatusEffectType.DEBUFF
    ){}

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
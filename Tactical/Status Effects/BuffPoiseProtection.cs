using Godot;
using System;
using System.Collections.Generic;

public class BuffPoiseProtection : AbstractStatusEffect, IEventHandler<CombatEventDamageTaken>, IEventHandler<CombatEventRoundEnd>{

    public static string id = "POISE_PROTECTION";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public BuffPoiseProtection() : base(
        id,
        strings,
        StatusEffectType.BUFF
    ){}

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_TAKE_DAMAGE, this, CombatEventPriority.BASE_ADDITIVE);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventDamageTaken data){
        if (data.target == this.OWNER && data.isPoiseDamage){
            Logging.Log($"{this.OWNER.CHAR_NAME} has Poise Protection, reducing incoming damage from {data.damageTaken} to {data.damageTaken - this.STACKS}.", Logging.LogLevel.INFO);
            data.damageTaken -= this.STACKS;
        }
    }

    public void HandleEvent(CombatEventRoundEnd data){
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }
}
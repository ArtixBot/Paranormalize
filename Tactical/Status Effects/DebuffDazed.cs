using Godot;
using System;
using System.Collections.Generic;

public class DebuffDazed : AbstractStatusEffect, IEventHandler<CombatEventDamageTaken>, IEventHandler<CombatEventRoundEnd>{

    public static string id = "DAZED";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public DebuffDazed() : base(
        id,
        strings,
        StatusEffectType.DEBUFF
    ){}

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_TAKE_DAMAGE, this, CombatEventPriority.BASE_ADDITIVE);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventDamageTaken data){
        if (data.target == this.OWNER && data.isPoiseDamage){
            Logging.Log($"{this.OWNER.CHAR_NAME} is Dazed, increasing incoming Poise damage from {data.damageTaken - this.STACKS} to {data.damageTaken}.", Logging.LogLevel.INFO);
            data.damageTaken += this.STACKS;
        }
    }

    public void HandleEvent(CombatEventRoundEnd data){
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }
}
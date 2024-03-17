using Godot;
using System;
using System.Collections.Generic;

public class BuffStrength : AbstractStatusEffect, IEventHandler<CombatEventDieRolled>, IEventHandler<CombatEventRoundEnd>{

    public static string id = "STRENGTH";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public BuffStrength() : base(
        id,
        strings,
        StatusEffectType.BUFF
    ){}

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_DIE_ROLLED, this, CombatEventPriority.BASE_ADDITIVE);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventDieRolled data){
        if (data.ability.OWNER == this.OWNER && data.die.IsAttackDie){
            data.rolledValue += this.STACKS;
            Logging.Log($"Strength increases die roll by +{this.STACKS} (from {data.rolledValue - this.STACKS} => {data.rolledValue}).", Logging.LogLevel.ESSENTIAL);
        }
    }

    public void HandleEvent(CombatEventRoundEnd data){
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }
}
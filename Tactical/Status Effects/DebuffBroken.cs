using Godot;
using System;
using System.Collections.Generic;

public class DebuffBroken : AbstractStatusEffect, IEventHandler<CombatEventDieRolled>, IEventHandler<CombatEventRoundEnd>{
    
    public static string id = "BROKEN";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public DebuffBroken() : base(
        id,
        strings,
        StatusEffectType.DEBUFF
    ){}

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_DIE_ROLLED, this, CombatEventPriority.BASE_ADDITIVE);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventDieRolled data){
        if (data.ability.OWNER == this.OWNER && data.die.DieType == DieType.BLOCK){
            data.rolledValue -= this.STACKS;
            Logging.Log($"Broken reduces die roll by -{this.STACKS} (from {data.rolledValue + this.STACKS} => {data.rolledValue}).", Logging.LogLevel.ESSENTIAL);
        }
    }

    public void HandleEvent(CombatEventRoundEnd data){
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }

}
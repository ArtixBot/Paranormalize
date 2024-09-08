using Godot;
using System;
using System.Collections.Generic;

public class BuffHaste : AbstractStatusEffect, IEventHandler<CombatEventRoundStart>, IEventHandler<CombatEventRoundEnd>{

    public static string id = "HASTE";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public BuffHaste() : base(
        id,
        strings,
        StatusEffectType.BUFF
    ){}

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.STANDARD);
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_END, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventRoundStart data){
        CombatManager.combatInstance.turnlist.ModifyItemPriority(this.OWNER, this.STACKS);
    }

    public void HandleEvent(CombatEventRoundEnd data){
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }
}
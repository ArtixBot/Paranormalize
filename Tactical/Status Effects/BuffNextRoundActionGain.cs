using Godot;
using System;
using System.Collections.Generic;

public class BuffNextRoundActionGain : AbstractStatusEffect, IEventHandler<CombatEventRoundStart>{

    public static string id = "NEXT_ROUND_ACTION_GAIN";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public BuffNextRoundActionGain() : base(
        id,
        strings,
        StatusEffectType.BUFF,
        CAN_GAIN_STACKS: true
    ){}

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.STANDARD + 1);       // When gaining actions, gain all actions first, THEN Haste/Slow should modify the speed of all actions.
    }

    public void HandleEvent(CombatEventRoundStart data){
        for (int i = 0; i < this.STACKS; i++){
            CombatManager.combatInstance?.turnlist.AddToQueue(this.OWNER, Rng.RandiRange(this.OWNER.MinSpd, this.OWNER.MaxSpd));
        }
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }
}
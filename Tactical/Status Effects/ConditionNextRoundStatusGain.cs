using Godot;
using System;
using System.Collections.Generic;

public class ConditionNextRoundStatusGain : AbstractStatusEffect, IEventHandler<CombatEventRoundStart>{

    public static string id = "NEXT_ROUND_STATUS_GAIN";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    private AbstractStatusEffect effectToGain;
    public string STATUS_NAME = "";        // Parsed in effects.json and Tooltip.cs.

    public ConditionNextRoundStatusGain(AbstractStatusEffect effectToGainNextRound) : base(
        id,
        strings,
        StatusEffectType.CONDITION,     // Considered a condition for the base constructor, but immediately overridden.
        CAN_GAIN_STACKS: true
    ){
        this.ID = this.ID + "_" + effectToGainNextRound.ID;
        this.effectToGain = effectToGainNextRound;
        this.STATUS_NAME = effectToGainNextRound.NAME;
        this.TYPE = effectToGainNextRound.TYPE;     // Immediately override base constructor with the effect to gain's type.
    }

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.IMMEDIATELY);
    }

    public void HandleEvent(CombatEventRoundStart data){
        CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, this.effectToGain, this.STACKS));
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }
}
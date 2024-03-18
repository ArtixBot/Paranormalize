using Godot;
using System;
using System.Collections.Generic;

public class ConditionDuelToTheDeath : AbstractStatusEffect, IEventHandler<CombatEventAbilityActivated>{

    public static string id = "DUEL_TO_THE_DEATH";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public string TARGET_NAME = "";        // Parsed in effects.json and StatusTooltip.cs.

    private AbstractCharacter applier;
    private AbstractCharacter target;

    public ConditionDuelToTheDeath(AbstractCharacter applier, AbstractCharacter target) : base(
        id,
        strings,
        StatusEffectType.CONDITION,
        CAN_GAIN_STACKS: false
    ){
        this.TARGET_NAME = target.CHAR_NAME;
        this.target = target;
        this.applier = applier;
    }

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.IMMEDIATELY);
    }

    public void HandleEvent(CombatEventAbilityActivated data){
        if (data.abilityActivated.OWNER == applier || data.abilityActivated.OWNER == target){
            
        }
    }
}
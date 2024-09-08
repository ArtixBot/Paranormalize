using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ConditionDuelToTheDeath : AbstractStatusEffect, IEventHandler<CombatEventAbilityActivated>{

    public static string id = "DUEL_TO_THE_DEATH";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public string APPLIER_NAME = "";        // Parsed in effects.json and Tooltip.cs.

    private readonly AbstractCharacter applier;
    private readonly AbstractCharacter target;

    public ConditionDuelToTheDeath(AbstractCharacter applier, AbstractCharacter target) : base(
        id,
        strings,
        StatusEffectType.CONDITION,
        CAN_GAIN_STACKS: false
    ){
        this.APPLIER_NAME = applier.CHAR_NAME;
        this.target = target;
        this.applier = applier;
    }

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.IMMEDIATELY);
    }

    private void ActivateDuelToTheDeathEffect(CombatEventAbilityActivated data){
        foreach (AbstractCharacter fighter in CombatManager.combatInstance?.fighters){
            AbstractStatusEffect ddEffect = fighter.statusEffects.Find(effect => effect.ID == "DUEL_TO_THE_DEATH");
            if (ddEffect != default){
                CombatManager.ExecuteAction(new RemoveStatusAction(fighter, ddEffect));
            }
        }
        CombatManager.ExecuteAction(new ApplyStatusAction(data.caster, new ConditionDishonorable(), 1));
    }

    public void HandleEvent(CombatEventAbilityActivated data){
        if (data.abilityActivated.TYPE != AbilityType.ATTACK){
            return;
        }
        // If the attacker is either the applier or victim, check if the ability's targets are anyone other than the applier/victim.
        // If the attacker is neither the applier not victim, check if the ability's targets are the applier/victim.
        Func<AbstractCharacter, bool> condition = (data.caster == this.applier || data.caster == this.target) ?
            (AbstractCharacter character) => character != this.applier && character != this.target : 
            (AbstractCharacter character) => character == this.applier || character == this.target;
        foreach (AbstractCharacter character in data.targets){
            if (condition(character)){
                ActivateDuelToTheDeathEffect(data);
                return;
            }
        }
    }
}
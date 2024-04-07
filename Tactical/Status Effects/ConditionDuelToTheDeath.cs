using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ConditionDuelToTheDeath : AbstractStatusEffect, IEventHandler<CombatEventAbilityActivated>{

    public static string id = "DUEL_TO_THE_DEATH";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public string APPLIER_NAME = "";        // Parsed in effects.json and StatusTooltip.cs.

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

    public void HandleEvent(CombatEventAbilityActivated data){
        if (data.abilityActivated.TYPE != AbilityType.ATTACK || 
            (data.abilityActivated.OWNER != this.applier && data.abilityActivated.OWNER != this.target)){
            return;
        }
        foreach (AbstractCharacter character in data.targets){
            GD.Print(character.CHAR_NAME, this.applier.CHAR_NAME, this.target.CHAR_NAME);
            if (character != this.applier && character != this.target){
                // Duel to the Death is activated!
                GD.Print("EFFECT OF DUEL TO THE DEATH SHOULD BE HAPPENING!");
                foreach (AbstractCharacter fighter in CombatManager.combatInstance?.fighters){
                    AbstractStatusEffect ddEffect = fighter.statusEffects.Find(effect => effect.ID == "DUEL_TO_THE_DEATH");
                    if (ddEffect != default){
                        CombatManager.ExecuteAction(new RemoveStatusAction(fighter, ddEffect));
                    }
                }
                CombatManager.ExecuteAction(new ApplyStatusAction(data.abilityActivated.OWNER, new ConditionDishonorable(), 1));
                return;
            }
        }
    }
}
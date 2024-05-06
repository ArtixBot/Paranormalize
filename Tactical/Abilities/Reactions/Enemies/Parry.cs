using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class Parry : AbstractAbility, IEventSubscriber, IEventHandler<CombatEventBeforeDieRolled>, IEventHandler<CombatEventClashTie>, IEventHandler<CombatEventClashComplete> {
    public static string id = "PARRY";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 1;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die blockDie = new Die(DieType.BLOCK, 5, 10, "ON_CLASH_DEAL_DAMAGE");
    private Die blockDieB = new Die(DieType.BLOCK, 4, 6, "MAY_BE_CONVERTED");
    private Die slashDie = new Die(DieType.SLASH, 9, 13);
    public Parry(): base(
        id,
        strings,
        AbilityType.REACTION,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit
    ){
        this.BASE_DICE = new List<Die>{blockDie, blockDieB};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.BEFORE_DIE_ROLLED, this, CombatEventPriority.IMMEDIATELY);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_CLASH_TIE, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_CLASH_COMPLETE, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventBeforeDieRolled data){
        if (data.die == this.blockDieB && data.rollTarget.HasCondition("STAGGERED") && !data.dieHasBeenConverted){
            data.die = this.slashDie;
            data.dieHasBeenConverted = true;
        }
    }

    public virtual void HandleEvent(CombatEventClashTie data){
        if (data.atkDie != this.blockDie && data.reactDie != this.blockDie) return;
        AbstractCharacter targetToDamage = data.atkDie == this.blockDie ? data.reactingClasher : data.attackingClasher;
        Logging.Log($"Parry is super successful! {data.tiedRoll * 2} damage and Poise damage is dealt.", Logging.LogLevel.ESSENTIAL);
        CombatManager.ExecuteAction(new DamageAction(this.OWNER, targetToDamage, DamageType.PURE, data.tiedRoll * 2, false));
        CombatManager.ExecuteAction(new DamageAction(this.OWNER, targetToDamage, DamageType.PURE, data.tiedRoll * 2, true));
    }

    public virtual void HandleEvent(CombatEventClashComplete data){
        if (data.winningDie == this.blockDie){
            Logging.Log($"Parry is successful! {data.winningRoll} damage is dealt.", Logging.LogLevel.ESSENTIAL);
            CombatManager.ExecuteAction(new DamageAction(this.OWNER, data.losingClasher, DamageType.PURE, data.winningRoll, false));
        }
    }
}
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Retribution : AbstractAbility, IEventSubscriber, IEventHandler<CombatEventDieHit>, IEventHandler<CombatEventClashComplete>, IEventHandler<CombatEventDieRolled> {
    public static string id = "RETRIBUTION";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 2;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die blockDie = new Die(DieType.BLOCK, 4, 7, "BOOST_NEXT_DIE_ON_LOSS");
    private Die bluntDie = new Die(DieType.BLUNT, 6, 10, "NEXT_ROUND_DAZE");

    private bool boostNextDie = false;
    private int boostNextDieValue = 0;

    public Retribution(): base(
        id,
        strings,
        AbilityType.REACTION,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit
    ){
        this.BASE_DICE = new List<Die>{blockDie, bluntDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_CLASH_COMPLETE, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_ROLLED, this, CombatEventPriority.BASE_ADDITIVE);
    }

    public void HandleEvent(CombatEventDieHit data){
        if (data.die == bluntDie){
            CombatManager.ExecuteAction(new ApplyStatusAction(data.hitUnit, new ConditionNextRoundStatusGain(new DebuffDazed()), 1));
        }
    }

    public virtual void HandleEvent(CombatEventClashComplete data){
        if (data.losingDie == this.blockDie){
            this.boostNextDie = true;
            this.boostNextDieValue = data.losingRoll;
        }
    }

    public virtual void HandleEvent(CombatEventDieRolled data){
        if (data.ability == this && this.boostNextDie){
            data.rolledValue += this.boostNextDieValue;
            Logging.Log($"Retribution boosted this die's roll by +{this.boostNextDieValue} (from {data.rolledValue - this.boostNextDieValue} to {data.rolledValue}).", Logging.LogLevel.INFO);
            this.boostNextDie = false;
            this.boostNextDieValue = 0;
        }
    }
}
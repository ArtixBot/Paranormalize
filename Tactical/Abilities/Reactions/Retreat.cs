using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Retreat : AbstractAbility, IEventSubscriber, IEventHandler<CombatEventAbilityActivated>, IEventHandler<CombatEventClashWin> {
    public static string id = "RETREAT";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 1;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private int MOVE_DISTANCE = 1;

    private Die evadeDie = new Die(DieType.EVADE, 5, 10, "EVADE_HASTE");
    public Retreat(): base(
        id,
        strings,
        AbilityType.REACTION,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit
    ){
        this.BASE_DICE = new List<Die>{evadeDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_CLASH_WIN, this, CombatEventPriority.STANDARD);
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated == this){
            CombatManager.ExecuteAction(new BackAction(this.OWNER, data.target, MOVE_DISTANCE));
        }
    }

    public virtual void HandleEvent(CombatEventClashWin data){
        if (data.winningDie == evadeDie){
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new BuffHaste(), 1));
        }
    }
}
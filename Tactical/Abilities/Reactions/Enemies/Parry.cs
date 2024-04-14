using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Parry : AbstractAbility, IEventSubscriber, IEventHandler<CombatEventClashComplete> {
    public static string id = "PARRY";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 0;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die blockDie = new Die(DieType.BLOCK, 5, 10, "ON_CLASH_DEAL_DAMAGE");
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
        this.BASE_DICE = new List<Die>{blockDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_CLASH_COMPLETE, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventClashComplete data){
        if (data.winningDie == this.blockDie){
            Logging.Log($"Parry is successful! {data.winningRoll} damage is dealt.", Logging.LogLevel.ESSENTIAL);
            CombatManager.ExecuteAction(new DamageAction(this.OWNER, data.losingClasher, DamageType.PURE, data.winningRoll, false));
        }
    }
}
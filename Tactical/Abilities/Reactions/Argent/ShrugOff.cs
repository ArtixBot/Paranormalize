using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ShrugOff : AbstractAbility, IEventSubscriber, IEventHandler<CombatEventClashComplete> {
    public static string id = "SHRUG_OFF";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 0;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die blockDie = new Die(DieType.BLOCK, 1, 3, "SHRUG_OFF");
    public ShrugOff(): base(
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
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_CLASH_COMPLETE, this, CombatEventPriority.FINAL);
    }

    public virtual void HandleEvent(CombatEventClashComplete data){
        if (data.winningDie.IsAttackDie && data.winningRoll <= 4){
            Logging.Log($"Opposing attack roll had a value <= 4; {this.OWNER.CHAR_NAME} shrugs off the attack!", Logging.LogLevel.ESSENTIAL);
            data.winningRoll = 0;
        }
    }
}
using System.Collections.Generic;

public class BalestraFente : AbstractAbility, IEventSubscriber, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "BALESTRA_FENTE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int MOVE_DISTANCE = 3;
    private static int cd = 4;
    private static int min_range = 3;
    private static int max_range = 3;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDie = new Die(DieType.PIERCE, 22, 22, "CRIT_DIE");

    public BalestraFente(): base(
        id,
        strings,
        AbilityType.ATTACK,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.ENEMIES_ONLY}
    ){
        this.BASE_DICE = new List<Die>{atkDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.STANDARD);
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated == this){
            CombatManager.ExecuteAction(new ForwardAction(this.OWNER, data.target, MOVE_DISTANCE));
        }
    }
}
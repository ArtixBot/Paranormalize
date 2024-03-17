using System.Collections.Generic;

public class Repartee : AbstractAbility, IEventHandler<CombatEventClashComplete> {
    public static string id = "REPARTEE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 1;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDieA = new Die(DieType.SLASH, 1, 4, "CLASH_LOSS_DIE");
    private Die atkDieB = new Die(DieType.PIERCE, 3, 7);
    private Die atkDieC = new Die(DieType.PIERCE, 4, 7);

    public Repartee(): base(
        id,
        strings,
        AbilityType.REACTION,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.ENEMIES_ONLY}
    ){
        this.BASE_DICE = new List<Die>{atkDieA, atkDieB, atkDieC};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_CLASH_COMPLETE, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventClashComplete data){
        if (data.losingDie == atkDieA){
            CombatManager.GetDieQueueFromCharacter(this.OWNER).Clear();
        }
    }
}
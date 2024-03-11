using System.Collections.Generic;

public class IronSwan : AbstractAbility, IEventHandler<CombatEventDieHit> {
    public static string id = "IRON_SWAN";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 2;
    private static int min_range = 2;
    private static int max_range = 4;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDie = new Die(DieType.PIERCE, 12, 18, "PULL_DIE");

    public IronSwan(): base(
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
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventDieHit data){
        if (data.die == atkDie){
            CombatManager.ExecuteAction(new PullAction(this.OWNER, data.hitUnit, 2));
        }
    }
}